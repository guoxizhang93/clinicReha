# ClinicCore 原生核心架构

日期：2026-04-26

## 当前边界

当前版本采用 `C/C++ 原生核心 + C# WinForms UI/调度`：

```text
ClinicRehabSuite.exe
  - WinForms UI、页面切换、设备连接调度、记录导出
  - P/Invoke 调用 ClinicCore.dll

ClinicCore.dll
  - CAN/UART 帧封装与电机状态解码
  - 电机属性写入、角度预置、执行、阻抗控制等高层命令帧构造
  - 四杆机构角度转换
  - 训练状态机与轨迹计算
  - OpenSignal 样本解析
  - 推杆/气囊反馈解析
  - EMG 特征提取
  - libsvm 训练、模型加载、模型预测
```

C# 不再保留 MATLAB 算法 fallback。`ClinicCore.dll` 缺失、位数不匹配或导出函数缺失时，核心功能直接报错；设备无硬件时仅进入“设备仿真模式”，用于 UI 和流程自测，不替代算法。

## 原生导出 API

核心函数：

- `clinic_can_build_command`
- `clinic_can_decode_motor_state`
- `clinic_motor_property_frame`
- `clinic_motor_preset_angle_frame`
- `clinic_motor_execute_frame`
- `clinic_motor_simple_order_frame`
- `clinic_motor_impedance_frame`
- `clinic_kinematics_linkage_to_motor`
- `clinic_kinematics_motor_to_linkage`
- `clinic_training_total_seconds`
- `clinic_training_step`
- `clinic_opensignal_parse_first_sample`
- `clinic_stretch_parse_feedback`
- `clinic_emg_extract_features`

libsvm 两阶段接口：

- `clinic_svm_train(labels, features, sample_count, feature_count, model_path)`
- `clinic_svm_load(model_path)`
- `clinic_svm_predict(handle, features, feature_count, out_label, out_score)`
- `clinic_svm_free(handle)`

训练接口强制要求 `model_path`。训练完成后先保存模型，再释放训练期模型，并重新从模型文件加载返回句柄，避免返回依赖训练数据内存的 libsvm 模型。

## SVM 工作流

1. 标定或训练模块采集 EMG 数据。
2. `ClinicCore.dll` 通过 `clinic_emg_extract_features` 提取特征。
3. `clinic_svm_train` 训练并保存 `.model` 文件。
4. 运行期用 `clinic_svm_load` 加载训练好的模型。
5. 每个 EMG 窗口提取同维度特征后调用 `clinic_svm_predict`。

当前默认运行期模型路径为：

```text
ClinicRehabSuite/bin/Models/pain_svm.model
```

该模型文件不存在时，界面不进行疼痛风险推理；不会启用 C# 评分替代逻辑。

## 构建

构建核心 DLL：

```powershell
cd D:\zgx\ClinicCore
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

构建完整 EXE：

```powershell
cd D:\zgx\ClinicRehabSuite
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

自测：

```powershell
D:\zgx\ClinicRehabSuite\bin\ClinicRehabSuite.exe --self-test
type D:\zgx\ClinicRehabSuite\bin\selftest.log
```

当前自测已覆盖原生核心加载、CAN 帧、高层电机命令帧、电机状态解码、训练状态机、OpenSignal 解析、推杆反馈解析、运动学转换、libsvm 训练保存、模型加载预测和原生检测器调用。
