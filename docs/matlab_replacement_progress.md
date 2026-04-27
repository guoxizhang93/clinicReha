# MATLAB mlapp 原生平替进度

日期：2026-04-27

## 当前结论

`ClinicRehabSuite.exe` 已经可以脱离 MATLAB / MATLAB Runtime 构建和运行。核心算法、协议帧、训练状态机、标定计算、SVM 训练/预测、OpenSignal/牵伸反馈解析等已迁移到 `ClinicCore.dll` 原生 C/C++ 层，C# 负责 WinForms UI、设备调度、P/Invoke、记录导出和流程编排。

按 `clinic_test0401.txt` 与 `DrEmpower_can.txt` 对照，mlapp 中可以通过软件实现的主要功能已具备原生替代路径；不能在本机完全确认的是依赖真实设备的闭环行为，包括真实电机、USB-CAN、OpenSignal、Arduino 推杆/气囊以及真实患者标定数据训练出的生产模型。这些不是 MATLAB 代码迁移缺口，而是硬件联调和临床验证缺口。

## 已完成

- Windows 原生可执行文件：`ClinicRehabSuite/bin/ClinicRehabSuite.exe`
- 原生核心库：`ClinicCore/bin/ClinicCore.dll`
- C# 通过 P/Invoke 调用原生核心，核心 DLL 缺失时直接报错，不再保留 MATLAB 算法 fallback
- C# 已移除模拟模式：设备连接失败、端口未打开或命令无法下发时会明确报错，不再静默降级
- WinForms 主界面已改为接近 MATLAB mlapp 的三栏同步布局：左侧为设备控制/OpenSignal/推杆控制 Tab 和底部提示/快捷操作，中间为全高电机数据，右侧为全高肌电数据/推杆数据/记录 Tab
- 默认窗口已改为最大化打开，初始尺寸为 `1680 x 940`，最小窗口为 `1514 x 820`
- CAN/UART 16 字节帧封装
- 电机属性写入、属性读取、角度预置、绝对角度、相对步进、速度、扭矩、运动助力、自适应力位、PID、零位/配置、角度范围、执行、急停、阻抗控制等命令帧原生生成
- 电机状态帧解码
- 四杆机构电机角/关节角换算
- MATLAB `Seg_trace_result` 分段轨迹计算迁移到原生接口
- 被动、对抗、自适应被动、自适应对抗、静态牵引、自适应充气训练模式入口
- 训练速度三档、对抗力三档、牵引时间等 MATLAB 处方参数已在 C# 界面和处方读取逻辑中恢复
- 训练前/训练后最大角和三分疼痛角标定计算入口
- MATLAB `mamfis/evalfis` 规则表迁移为原生模糊角度调整函数
- 自适应被动/自适应对抗训练已接入模糊逻辑：根据保持阶段 SVM 决策值和电机力矩更新后续周期终止角
- 静态牵引载荷判断、自适应充气保持判断、压力灯状态判断、ADC 压力换算
- OpenSignal 样本解析和 config 解析
- OpenSignal `devices/config/start/stop` 操作入口已在 C# 界面补齐
- 推杆/气囊反馈解析
- 推杆归零、一次拉伸、伸出、回缩、开启/停止反馈、大腿充气/保持/放气、小腿充气/保持/放气等 MATLAB 手动控制入口已补齐
- EMG 特征提取迁移到原生 DLL
- libsvm 源码集成到 `ClinicCore.dll`
- libsvm 两阶段流程：真实采样窗口训练保存、重新加载、运行时预测；C# 不再用固定玩具样本生成生产模型
- 命令行自测覆盖模型训练、保存、加载、预测、训练状态机、临床流程决策和解析器
- 命令行自测已扩展覆盖新增 DrEmpower 通用电机命令帧

## 仍需现场确认

- 真实电机、USB-CAN、OpenSignal、Arduino 推杆/气囊硬件闭环联调
- OpenSignal `devices/config/start/stop` 在真实设备上的完整响应格式校准
- Arduino 固件是否完整支持 MATLAB 调用过的 `1/2/3/4/5/6/7/8/9/o/c/q` 等命令
- 使用真实标定数据生成并冻结生产路径 `Models/pain_svm.model`
- 真实 SVM 模型需要同时采集疼痛/非疼痛标签窗口，否则 C# 会拒绝生成模型
- 医疗软件商业化所需的权限、审计日志、异常报告、安装包、需求追踪、风险管理和验证确认

## 最近自测

命令：

```powershell
cd D:\zgx
.\ClinicRehabSuite\build.ps1
.\ClinicRehabSuite\bin\ClinicRehabSuite.exe --self-test
type "$env:TEMP\ClinicRehabSuite-selftest.log"
```

结果：

```text
PASS
native-core-ok
record-ok
can-frame-ok
native-motor-frames-ok
hardware-failure-explicit-ok
native-motor-advanced-frames-ok
training-state-machine-ok
native-clinical-workflow-ok
opensignal-svm-ok
stretch-feedback-ok
motor-state-decode-ok
kinematics-ok
libsvm-train-predict-ok
native-detector-ok
```
