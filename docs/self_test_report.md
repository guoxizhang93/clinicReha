# 自测报告

日期：2026-04-27

## 环境

- Windows PowerShell 5.1
- .NET Framework C# Compiler 4.8
- MSVC 2022 `cl.exe`
- 未检测到 MATLAB / Octave 命令行环境

影响：

- 本机无法执行 MATLAB App Designer 运行级测试。
- 当前 Windows EXE 和原生 DLL 均已用本机编译器构建并通过命令行自测。

## 构建结果

命令：

```powershell
cd D:\zgx
.\ClinicRehabSuite\build.ps1
```

产物：

- `D:\zgx\ClinicCore\bin\ClinicCore.dll`
- `D:\zgx\ClinicRehabSuite\bin\ClinicCore.dll`
- `D:\zgx\ClinicRehabSuite\bin\ClinicRehabSuite.exe`
- 自测临时 CSV 和 SVM 模型写入 `%TEMP%\ClinicRehabSuiteSelfTest`

## 命令行自测

命令：

```powershell
cd D:\zgx
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

覆盖项：

- `ClinicCore.dll` 原生核心加载
- CSV 记录写入
- CAN UART 16 字节帧生成
- 电机属性写入、角度预置、执行、阻抗控制命令帧原生生成
- 无硬件端口时明确报错，不再进入设备仿真模式
- 扩展 DrEmpower 电机命令帧，包括自适应力位、直接速度/扭矩、PID、角度范围和配置命令
- 训练状态机和分段轨迹插值
- 标定最大角计算
- 模糊角度调节
- 自适应对抗角度更新
- 静态牵引载荷决策
- 自适应充气保持决策
- 压力灯状态决策和 ADC 压力换算
- OpenSignal 样本和 config 文本解析
- EMG 原生特征提取
- 推杆/气囊反馈解析
- CAN 电机状态帧解码
- 四杆机构/电机角度换算
- libsvm 原生训练、保存、重新加载和预测
- C# 检测器通过原生模型句柄预测

## 剩余风险

- 尚未接入真实电机、推杆、气囊、OpenSignal 硬件做闭环测试。
- `Models/pain_svm.model` 需要由真实标定数据训练生成，并确认特征维度和标签定义固定。
- 医疗商业化仍需补齐风险管理、可追溯需求、验证确认、权限审计、异常日志和安装包。
