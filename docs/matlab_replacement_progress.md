# MATLAB mlapp 原生平替进度

日期：2026-04-26

## 当前结论

`ClinicRehabSuite.exe` 已经可以脱离 MATLAB / MATLAB Runtime 构建和运行。核心算法、协议帧、训练状态机、标定计算、SVM 训练/预测、OpenSignal/牵伸反馈解析等已迁移到 `ClinicCore.dll` 原生 C/C++ 层，C# 负责 WinForms UI、设备调度、P/Invoke、记录导出和流程编排。

按源码对照，mlapp 中可以通过软件实现的主要功能已具备原生替代路径；不能在本机完全确认的是依赖真实设备的闭环行为，包括真实电机、USB-CAN、OpenSignal、Arduino 推杆/气囊以及真实患者标定数据训练出的生产模型。这些不是 MATLAB 代码迁移缺口，而是硬件联调和临床验证缺口。

## 已完成

- Windows 原生可执行文件：`ClinicRehabSuite/bin/ClinicRehabSuite.exe`
- 原生核心库：`ClinicCore/bin/ClinicCore.dll`
- C# 通过 P/Invoke 调用原生核心，核心 DLL 缺失时直接报错，不再保留 MATLAB 算法 fallback
- WinForms 主界面：总览、患者、设备、标定、训练处方、实时监测、牵伸/气囊、记录
- CAN/UART 16 字节帧封装
- 电机属性写入、角度预置、执行、急停、阻抗控制等命令帧原生生成
- 电机状态帧解码
- 四杆机构电机角/关节角换算
- MATLAB `Seg_trace_result` 分段轨迹计算迁移到原生接口
- 被动、对抗、自适应被动、自适应对抗、静态牵引、自适应充气训练模式入口
- 训练前/训练后最大角和三分疼痛角标定计算入口
- MATLAB `mamfis/evalfis` 规则表迁移为原生模糊角度调整函数
- 静态牵引载荷判断、自适应充气保持判断、压力灯状态判断、ADC 压力换算
- OpenSignal 样本解析和 config 解析
- 推杆/气囊反馈解析
- EMG 特征提取迁移到原生 DLL
- libsvm 源码集成到 `ClinicCore.dll`
- libsvm 两阶段流程：训练保存、重新加载、运行时预测
- 命令行自测覆盖模型训练、保存、加载、预测、训练状态机、临床流程决策和解析器

## 仍需现场确认

- 真实电机、USB-CAN、OpenSignal、Arduino 推杆/气囊硬件闭环联调
- OpenSignal `devices/config/start/stop` 在真实设备上的完整响应格式校准
- Arduino 固件是否完整支持 MATLAB 调用过的 `1/5/6/8/9/o/c/q` 等命令
- 使用真实标定数据生成并冻结生产路径 `Models/pain_svm.model`
- 医疗软件商业化所需的权限、审计日志、异常报告、安装包、需求追踪、风险管理和验证确认

## 最近自测

命令：

```powershell
cd D:\zgx
.\ClinicRehabSuite\build.ps1
.\ClinicRehabSuite\bin\ClinicRehabSuite.exe --self-test
type .\ClinicRehabSuite\bin\selftest.log
```

结果：

```text
PASS
native-core-ok
record-ok
can-frame-ok
native-motor-frames-ok
device-simulation-ok
training-state-machine-ok
native-clinical-workflow-ok
opensignal-svm-ok
stretch-feedback-ok
motor-state-decode-ok
kinematics-ok
libsvm-train-predict-ok
native-detector-ok
```
