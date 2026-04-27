# 功能核查报告

日期：2026-04-27

## 核查结论

当前仓库已整理为 `C/C++ ClinicCore.dll + C# ClinicRehabSuite.exe` 的原生 Windows 版本。MATLAB `.mlapp/.m` 原包、解包分析目录、历史自测 CSV、编译中间产物和未使用的 libsvm 子项目已清理；保留 `clinic_test0401.txt` 与 `DrEmpower_can.txt` 作为 MATLAB 源码对照文本。

## 功能状态

- 设备连接：CAN 电机、Arduino 推杆/气囊、OpenSignal TCP 均由 C# 统一连接；任一真实通道失败会明确报错，不再进入模拟模式。
- 电机控制：已覆盖属性读写、模式切换、角度预置、同步执行、相对步进、速度/扭矩、运动助力、自适应力位、阻抗控制、PID、角度范围、配置/初始化、急停和状态帧解码。
- 训练流程：已实现被动、对抗、自适应被动、自适应对抗、静态牵引、自适应充气入口；训练状态机、分段轨迹、速度档位、对抗力档位、保持/休息/牵引参数已接入。
- 标定流程：训练前最大角、训练后最大角、三分疼痛角入口已接入实时电机/遥测数据。
- SVM：libsvm 已集成进 `ClinicCore.dll`；支持 EMG 特征提取、真实采样窗口训练保存、模型加载和运行时预测；无真实疼痛/非疼痛标签窗口时拒绝生成生产模型。
- 模糊自适应控制：MATLAB `mamfis/evalfis` 规则已迁移到原生函数；自适应训练会根据保持阶段 SVM 决策值和电机力矩更新后续终止角。
- OpenSignal：支持 `devices/config/start/stop` 命令入口，样本和 config 文本解析已自测。
- 推杆/气囊：支持归零、单次拉伸、伸出、回缩、开启/停止反馈、大腿充气/保持/放气、小腿充气/保持/放气；反馈解析、静态牵引载荷判断、自适应充气保持判断和压力灯状态已自测。
- 数据记录：治疗记录仍输出到运行目录 `Records`；命令行自测的临时 CSV、SVM 模型和日志改写到 `%TEMP%`，不再污染仓库。
- UI：默认最大化打开，采用医疗工作站风格的克制色彩、清晰边框、三栏布局和高对比操作按钮。

## 已执行验证

```powershell
cd D:\zgx
.\ClinicRehabSuite\build.ps1
.\ClinicRehabSuite\bin\ClinicRehabSuite.exe --self-test
type "$env:TEMP\ClinicRehabSuite-selftest.log"
```

自测覆盖原生核心加载、记录写入、CAN 帧、电机命令帧、硬件失败显式报错、训练状态机、临床决策、OpenSignal/SVM、推杆反馈、电机状态解码、运动学、libsvm 训练/加载/预测和原生检测器调用。

## 仍需现场验证

- 真实电机、USB-CAN、OpenSignal、Arduino 推杆/气囊闭环通信。
- Arduino 固件对 `1/2/3/4/5/6/7/8/9/o/c/q` 命令的完整一致性。
- 真实标定采样生成的 `Models/pain_svm.model` 的特征维度、标签定义和阈值。
- 医疗软件商业化所需风险管理、审计日志、权限控制、异常追踪、安装包和验证确认。
