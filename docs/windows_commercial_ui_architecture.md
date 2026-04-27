# Windows 商业化 UI 与软件架构方案

## 1. 已交付可运行版本

目录：`ClinicRehabSuite/`  
源码：`ClinicRehabSuite/Program.cs`  
构建脚本：`ClinicRehabSuite/build.ps1`  
可执行文件：`ClinicRehabSuite/bin/ClinicRehabSuite.exe`

该版本使用 Windows 自带 .NET Framework WinForms 编译器构建，不依赖 MATLAB Runtime，不需要联网安装依赖。

## 2. 当前原生 Windows UI 功能

已实现页面：

1. 总览
   - 电机角度/速度/电流趋势
   - 肌电/SVM 趋势
   - 推杆/压力趋势
   - 启动训练、急停、导出记录
2. 患者管理
   - 姓名、病历号、诊断、治疗师、侧别、疼痛阈值、备注
3. 设备连接
   - 大然电机 CAN 串口
   - Arduino 推杆/气囊串口
   - Opensignal TCP
   - 连接、断开、自检
   - 连接失败明确报错，不再自动进入模拟模式
4. 训练处方
   - 训练模式
   - 起止角度
   - 次数
   - 保持/休息时间
   - 牵伸力
   - 训练进度
5. 实时监测
   - 电机反馈曲线
   - 肌电/SVM 曲线
   - 推杆/压力曲线
   - 事件日志
6. 牵伸/气囊
   - 推杆归零、伸出、回缩
   - 大腿/小腿充气与保持
   - 开启/停止反馈
7. 记录
   - CSV 导出
   - 记录列表

## 3. 原生服务层设计

当前 C# 程序已经按商业软件迁移方向分层：

- `MainForm`：UI 展示与用户操作
- `DeviceController`：硬件连接、训练命令、急停、推杆命令
- `CanProtocol`：大然电机 CAN -> UART 16 字节包封装
- `SessionRecorder`：CSV 记录导出
- `SelfTestRunner`：命令行自测，覆盖无硬件时显式失败路径
- `WavePanel`：实时曲线控件

当前已迁移的协议能力：

- CAN UART 16 字节封包
- `WriteProperty`
- `SetMode`
- `PresetAngle`
- `SetAngles`
- 急停命令
- Arduino 单字符控制命令
- Opensignal TCP 连接检测
- DrEmpower 自适应力位、直接速度/扭矩、PID、角度范围和配置命令帧

## 4. 商业化建议架构

建议正式产品使用如下架构：

```text
ClinicRehabSuite
├─ UI 层
│  ├─ 患者
│  ├─ 设备
│  ├─ 处方
│  ├─ 训练
│  ├─ 实时监测
│  └─ 记录报表
├─ Application 层
│  ├─ 训练状态机
│  ├─ 标定流程状态机
│  ├─ 急停与告警策略
│  └─ 权限与审计
├─ Device 层
│  ├─ MotorCanService
│  ├─ StretchActuatorService
│  ├─ AirCuffService
│  └─ OpenSignalService
├─ Signal 层
│  ├─ EMG 预处理
│  ├─ 特征提取
│  ├─ SVM/ML 推理
│  └─ 疼痛/异常判定
├─ Data 层
│  ├─ 患者数据
│  ├─ 训练记录
│  ├─ 设备日志
│  └─ 审计日志
└─ Safety 层
   ├─ 软急停
   ├─ 硬件看门狗
   ├─ 限位/限速/限扭矩
   └─ 通信超时降级
```

## 5. 后续替换 MATLAB 的技术路径

短期：

- 用当前 WinForms EXE 作为临床演示/需求确认原型
- 继续补全 `DeviceController` 的 CAN 协议
- 将 MATLAB 训练流程逐步迁移为 C# 状态机
- 用 CSV/JSON 保存记录

中期：

- 改为 WPF 或 WinUI 3，提高商用 UI 质量和可维护性
- 加入 SQLite 本地数据库
- 使用后台线程/任务执行硬件通信，UI 只订阅状态
- 把 SVM 模型迁移为 ONNX、ML.NET 或自研线性 SVM 推理

长期：

- IEC 62304 风格软件生命周期管理
- 完整需求、风险、测试追踪矩阵
- 设备通信 watchdog
- 日志不可篡改与权限控制
- 安装包、自动升级、崩溃日志

## 6. 运行方式

构建：

```powershell
cd D:\zgx\ClinicRehabSuite
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

运行：

```powershell
D:\zgx\ClinicRehabSuite\bin\ClinicRehabSuite.exe
```

命令行自测：

```powershell
D:\zgx\ClinicRehabSuite\bin\ClinicRehabSuite.exe --self-test
type "$env:TEMP\ClinicRehabSuite-selftest.log"
```
