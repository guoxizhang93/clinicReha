# clinic_test0401.mlapp 功能与重构分析

分析日期：2026-04-26  
对象：`clinic_test0401.mlapp`、`DrEmpower_can.m`、`sketch_01/sketch_01.ino`、`libsvm-3.32`

## 1. 软件总体定位

该软件是 MATLAB App Designer 编写的下肢康复训练控制系统，主要面向膝关节/下肢康复设备。现有架构由 4 部分组成：

1. MATLAB App Designer UI：`clinic_test0401.mlapp`
2. 大然电机 CAN/串口控制类：`DrEmpower_can.m`
3. Opensignal 肌电采集 TCP 客户端与 SVM 疼痛识别
4. Arduino 推杆/气囊控制固件：`sketch_01/sketch_01.ino`

`clinic_test0401.mlapp` 的元数据表明：创建于 2023-12-01，最近修改于 2025-07-20，App Designer 最低支持 R2018a，保存环境为 MATLAB R2023b Update 1。

## 2. 详细功能清单

### 2.1 患者与数据管理

- 患者信息登记入口：`Button_MsgPatient`
- 保存患者对象：`msg_patient_save(app, patient)`
- 按患者日期与姓名创建保存目录：`Save_Data\<date><name>`
- 保存单次训练数据：`Save_data_part`
- 保存完整训练、标定、配置、患者信息：`Save_data_all`
- 数据类型覆盖：
  - 电机 1/2 状态数据
  - Opensignal 肌电数据
  - 训练配置
  - 标定配置
  - 自适应/模糊控制中间量

### 2.2 电机与 CAN 控制

- CAN 串口初始化，默认硬编码 `COM4`、`115200`
- 电机模式切换：
  - 卸载/使能相关模式
  - 待机模式
- 双电机 ID 默认 `[1,2]`
- 电机状态读取：
  - 角度
  - 速度
  - 力矩/电流
  - 电压/电流
  - PID
  - 关节 ID
- 电机参数设置：
  - ID 修改
  - CAN 波特率
  - 零点/临时零点
  - 角度范围限制
  - PID
  - 状态反馈周期
  - 速度限制
  - 力矩限制
- 运动控制：
  - 单关节绝对角度控制
  - 多关节绝对角度同步控制
  - 单关节/多关节相对角度控制
  - 速度控制
  - 力矩控制
  - 力位混合控制
  - 阻抗控制
  - 运动助力
  - 急停
- 状态反馈回调：`configureCallback(app.dr.dev,"byte",256,@app.callbackFcn)`

### 2.3 机构运动学

- 电机角度到四杆机构角度转换：`motor2linkage`
- 四杆机构角度到电机角度查表转换：`linkage2motor`
- S 曲线/分段轨迹生成：`Seg_trace_result`
- 实时角度显示：`Label_MotorDegree`

### 2.4 Opensignal 肌电采集

- TCP 连接：`127.0.0.1:5555`
- Opensignal 命令：
  - `devices`
  - `config,<deviceId>`
  - `start`
  - `stop`
- 配置解析：
  - activeChannels
  - samplingFreq
  - labelChannels
  - sensorChannels
- 实时数据接收与清空缓存
- 触发通道识别急停/疼痛标记
- 肌电绘图：
  - 5 路 EMG 图
  - trigger 图

### 2.5 SVM 疼痛识别

- 依赖 `libsvm-3.32/matlab`
- 训练函数：`svmtrain`
- 预测函数：`svmpredict`
- 特征提取：
  - 平均绝对值
  - 标准差
  - RMS
  - 波长
  - 20-500 Hz bandpower
- 输出显示：
  - SVM Gauge
  - `Gauge_SVM.Value = -svmrst`
- 训练/预测结果用于自适应训练策略

### 2.6 训练模式

现有 UI 下拉框包含 6 种训练模式：

1. 被动训练
2. 对抗训练
3. 自适应被动训练
4. 自适应对抗训练
5. 静态牵伸
6. 自适应充气

训练参数包括：

- 次数
- 起始角度
- 终止角度
- 速度档位
- 对抗力量
- 保持/对抗时间
- 牵引力
- 拉伸时间
- 休息时间

### 2.7 标定与范围检测

- 设备最大物理角度检测
- 训练前最大屈曲角度检测
- 训练后最大屈曲角度检测
- 被动活动范围标定
- 三分疼痛角度标定
- 标定过程保存肌电与电机数据

### 2.8 推杆、牵伸与气囊控制

MATLAB 端使用 `Com_stretch` 串口控制 Arduino，固件默认 `9600` 波特率。现有代码中串口打开逻辑被注释，UI 按钮仍调用 `app.Com_stretch`，这是运行崩溃点之一。

Arduino 命令映射：

- `1`：推杆归零
- `2`：单次拉伸
- `3`：推杆增加/伸出
- `4`：推杆减少/回缩
- `5`：大腿气囊充气
- `6`：大腿气囊保持
- `7`：停止/归零
- `8`：小腿气囊充气
- `9`：小腿气囊保持
- `o`：开启反馈
- `c`：结束反馈
- `q`：小腿放气

当前 Arduino 草图只实现了 `2/3/4/7`，MATLAB 端调用的 `1/5/6/8/9/o/c/q` 在固件中未实现或不完整。

## 3. 关键崩溃与缺陷

### 3.1 App 打开即连接 COM4

原 `startupFcn` 打开界面后立即执行：

- `DrEmpower_can("COM4", 115200)`
- `flush`
- `set_mode`
- `configureCallback`

没有设备、端口号不同、端口被占用时 App 会直接崩溃，无法进入 UI。

处理：`clinic_test0401_refactored.m` 中已移动到 `initMotorDevice`，只在点击“启动设备”时连接，并用 `try/catch` 显示错误。

### 3.2 多处无限阻塞

典型位置：

- `while obj.dev.NumBytesAvailable() == 0`
- `while obj.dev.NumBytesAvailable<16`
- `while byte_list_head ~= 170`
- `while kk ~= 1`
- `while isempty(angle_speed_torques)`

这些循环在硬件掉线、包格式异常、Opensignal 未返回数据时会永久卡死。

处理：`DrEmpower_can.m` 已加入超时、连接检查、`LastError`，并重构状态帧读取逻辑。

### 3.3 状态帧读取逻辑错误

原 `read_data_state2` 使用：

```matlab
byte_list_head = read(obj.dev,16,"uint8");
while byte_list_head ~= 170
```

`byte_list_head` 是 16 字节数组，与标量比较会产生逻辑错误或行为不确定。

处理：已改为按 1 字节查找帧头 `0xAA`，再读取剩余 15 字节。

### 3.4 推杆/气囊串口未初始化

`Button_stretch_*` 等函数直接调用 `app.Com_stretch`，但串口初始化代码被注释，点击相关按钮会报错。

建议：迁移到独立 `StretchController`，先检测串口连接状态，再允许按钮操作。

### 3.5 缺失依赖

- `msg_patient` 类/函数未在目录中提供
- `funcMotor`、`transceive` 未定义
- `Exoskeleton.jpg` 被注释但不是完整资产管理
- `svmtrain/svmpredict` 依赖 MEX 与 MATLAB 版本/路径
- `bandpower` 依赖 Signal Processing Toolbox
- `mamfis/evalfis` 依赖 Fuzzy Logic Toolbox

### 3.6 中文编码异常

从 `.mlapp` 提取的源码中大量中文显示为乱码，可能由历史编码转换导致。商业化前必须统一 UTF-8 或资源文件管理。

### 3.7 UI 线程长时间阻塞

训练和标定回调里大量 `while/tic/toc/pause` 运行在 UI 回调线程，容易造成界面假死、按钮无法及时响应。应迁移为后台任务/状态机。

## 4. 已完成重构

### 4.1 `DrEmpower_can.m`

已完成：

- 构造函数支持可配置读超时
- 串口打开失败给出明确异常
- 新增 `is_connected`
- 新增 `available_bytes`
- 新增 `wait_for_bytes`
- `write_data` 防空数据和断线
- `read_data` 防无限等待
- `read_data_id` 加超时
- `read_data_state2` 重写为帧头同步 + 完整 16 字节帧读取
- `receive_data` 加超时与解析失败保护
- `send_command` 限制 CAN 数据区不超过 8 字节
- `position_done/positions_done` 加最大等待时间
- `angle_speed_torque_state(s)` 处理空结果

### 4.2 `clinic_test0401_refactored.m`

已完成：

- 从 `.mlapp` 提取出 MATLAB 源码副本
- 新增 `initMotorDevice`
- 新增 `cleanupDevices`
- `startupFcn` 不再连接硬件
- “启动设备”按钮中连接 CAN，并在失败时恢复 UI
- 关闭窗口/关闭设备时统一清理回调和 Opensignal stop

说明：没有直接改写 `clinic_test0401.mlapp` 原包，因为 `.mlapp` 同时包含 App Designer 二进制界面模型 `appModel.mat`。在没有 MATLAB App Designer 的环境下直接回写有破坏工程的风险。

