using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ClinicRehabSuite
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            if (args != null && args.Length > 0 && args[0] == "--self-test")
            {
                Environment.Exit(SelfTestRunner.Run());
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    internal sealed class MainForm : Form
    {
        private readonly DeviceController deviceController;
        private readonly SessionRecorder recorder;
        private readonly Timer telemetryTimer;
        private readonly List<Button> navigationButtons = new List<Button>();
        private readonly List<TelemetrySample> telemetry = new List<TelemetrySample>();

        private Panel sidebar;
        private Panel header;
        private Panel content;
        private Label titleLabel;
        private Label statusLabel;
        private Label connectionLabel;
        private Label sessionStateLabel;
        private Label alertLabel;
        private ComboBox motorPortBox;
        private ComboBox stretchPortBox;
        private ComboBox deviceModeBox;
        private ComboBox trainModeBox;
        private NumericUpDown startAngleBox;
        private NumericUpDown endAngleBox;
        private NumericUpDown repeatBox;
        private NumericUpDown keepTimeBox;
        private NumericUpDown restTimeBox;
        private NumericUpDown tractionForceBox;
        private NumericUpDown measuredAngleBox;
        private ProgressBar progressBar;
        private WavePanel motorWave;
        private WavePanel emgWave;
        private WavePanel pressureWave;
        private FlowLayoutPanel logPanel;
        private string currentPage = "Dashboard";
        private bool training;
        private DateTime trainingStart;
        private int trainingSeconds;
        private TrainingPrescription activePrescription;
        private TrainingSession activeSession;
        private readonly SvmPainDetector painDetector = new SvmPainDetector();

        public MainForm()
        {
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            Text = "Clinic Rehab Suite";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1200, 760);
            Size = new Size(1360, 820);
            BackColor = Color.FromArgb(245, 247, 250);

            deviceController = new DeviceController();
            recorder = new SessionRecorder(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Records"));
            telemetryTimer = new Timer();
            telemetryTimer.Interval = 120;
            telemetryTimer.Tick += TelemetryTimerTick;

            BuildShell();
            ShowDashboard();
            telemetryTimer.Start();
        }

        private void BuildShell()
        {
            sidebar = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = Color.FromArgb(26, 35, 50) };
            header = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Color.White };
            content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24), BackColor = Color.FromArgb(245, 247, 250) };

            Controls.Add(content);
            Controls.Add(header);
            Controls.Add(sidebar);
            FormClosed += delegate
            {
                painDetector.Dispose();
                deviceController.Dispose();
            };

            var brand = new Label
            {
                Text = "Clinic Rehab",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold),
                Location = new Point(22, 22),
                AutoSize = true
            };
            sidebar.Controls.Add(brand);

            var nav = new[]
            {
                new NavItem("Dashboard", "总览"),
                new NavItem("Patient", "患者"),
                new NavItem("Device", "设备"),
                new NavItem("Calibration", "标定"),
                new NavItem("Prescription", "训练处方"),
                new NavItem("Monitoring", "实时监测"),
                new NavItem("Stretch", "牵伸/气囊"),
                new NavItem("Records", "记录")
            };
            var y = 90;
            foreach (var item in nav)
            {
                var button = new Button
                {
                    Text = item.Text,
                    Tag = item.Key,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.FromArgb(221, 226, 234),
                    BackColor = Color.FromArgb(26, 35, 50),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Width = 188,
                    Height = 42,
                    Location = new Point(22, y),
                    Padding = new Padding(16, 0, 0, 0)
                };
                button.FlatAppearance.BorderSize = 0;
                button.Click += NavigationClick;
                navigationButtons.Add(button);
                sidebar.Controls.Add(button);
                y += 50;
            }

            titleLabel = new Label
            {
                Text = "总览",
                Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                Location = new Point(24, 19),
                AutoSize = true
            };
            header.Controls.Add(titleLabel);

            connectionLabel = new Label
            {
                Text = "设备：未连接",
                ForeColor = Color.FromArgb(75, 85, 99),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(700, 26)
            };
            header.Controls.Add(connectionLabel);

            statusLabel = new Label
            {
                Text = "就绪",
                ForeColor = Color.FromArgb(22, 101, 52),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(930, 26)
            };
            header.Controls.Add(statusLabel);

            Resize += delegate { LayoutHeaderStatus(); };
            LayoutHeaderStatus();
        }

        private void LayoutHeaderStatus()
        {
            if (connectionLabel == null || statusLabel == null) return;
            statusLabel.Location = new Point(Math.Max(560, header.Width - 180), 26);
            connectionLabel.Location = new Point(Math.Max(360, header.Width - 430), 26);
        }

        private void NavigationClick(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            currentPage = (string)button.Tag;
            if (currentPage == "Dashboard") ShowDashboard();
            if (currentPage == "Patient") ShowPatient();
            if (currentPage == "Device") ShowDevice();
            if (currentPage == "Calibration") ShowCalibration();
            if (currentPage == "Prescription") ShowPrescription();
            if (currentPage == "Monitoring") ShowMonitoring();
            if (currentPage == "Stretch") ShowStretch();
            if (currentPage == "Records") ShowRecords();
        }

        private void SetPage(string pageKey, string title)
        {
            currentPage = pageKey;
            titleLabel.Text = title;
            content.Controls.Clear();
            foreach (var button in navigationButtons)
            {
                var active = (string)button.Tag == pageKey;
                button.BackColor = active ? Color.FromArgb(51, 65, 85) : Color.FromArgb(26, 35, 50);
                button.ForeColor = active ? Color.White : Color.FromArgb(221, 226, 234);
            }
        }

        private void ShowDashboard()
        {
            SetPage("Dashboard", "总览");
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 4 };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            content.Controls.Add(grid);

            grid.Controls.Add(MetricCard("关节角度", "0 deg", "双电机平均角度"), 0, 0);
            grid.Controls.Add(MetricCard("牵伸压力", "0 N", "8 路压力合计"), 1, 0);
            grid.Controls.Add(MetricCard("SVM 风险", "待采集", "肌电疼痛识别输出"), 2, 0);

            motorWave = new WavePanel { Dock = DockStyle.Fill, Title = "电机角度 / 速度 / 电流" };
            emgWave = new WavePanel { Dock = DockStyle.Fill, Title = "肌电通道与触发信号" };
            pressureWave = new WavePanel { Dock = DockStyle.Fill, Title = "推杆位置与压力" };
            grid.Controls.Add(WrapPanel(motorWave), 0, 1);
            grid.SetColumnSpan(grid.GetControlFromPosition(0, 1), 2);
            grid.Controls.Add(WrapPanel(emgWave), 2, 1);
            grid.Controls.Add(WrapPanel(pressureWave), 0, 2);
            grid.SetColumnSpan(grid.GetControlFromPosition(0, 2), 2);
            grid.Controls.Add(OperationPanel(), 2, 2);
            grid.Controls.Add(AlertPanel(), 0, 3);
            grid.SetColumnSpan(grid.GetControlFromPosition(0, 3), 3);
        }

        private Control OperationPanel()
        {
            var panel = CardPanel();
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5, ColumnCount = 1, Padding = new Padding(12) };
            panel.Controls.Add(layout);
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(SectionTitle("治疗操作"), 0, 0);
            layout.Controls.Add(ActionButton("启动训练", StartTraining), 0, 1);
            layout.Controls.Add(ActionButton("紧急停止", EmergencyStop), 0, 2);
            layout.Controls.Add(ActionButton("导出记录", ExportRecord), 0, 3);
            return panel;
        }

        private Control AlertPanel()
        {
            var panel = CardPanel();
            alertLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "当前无未处理告警。所有硬件命令通过服务层记录，未连接硬件时自动进入模拟模式。",
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(55, 65, 81),
                Padding = new Padding(18, 0, 18, 0)
            };
            panel.Controls.Add(alertLabel);
            return panel;
        }

        private void ShowPatient()
        {
            SetPage("Patient", "患者管理");
            var panel = CardPanel();
            panel.Dock = DockStyle.Top;
            panel.Height = 330;
            content.Controls.Add(panel);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 6, Padding = new Padding(24) };
            panel.Controls.Add(layout);
            for (int i = 0; i < 4; i++) layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            for (int i = 0; i < 6; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            layout.Controls.Add(SectionTitle("患者信息登记"), 0, 0);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 0), 4);
            AddFormField(layout, "姓名", "未命名患者", 0, 1);
            AddFormField(layout, "病历号", "MRN-0001", 1, 1);
            AddFormField(layout, "诊断", "膝关节术后康复", 2, 1);
            AddFormField(layout, "治疗师", "Therapist", 3, 1);
            AddFormField(layout, "侧别", "左/右", 0, 3);
            AddFormField(layout, "疼痛阈值", "3/10", 1, 3);
            AddFormField(layout, "备注", "无", 2, 3);
            layout.SetColumnSpan(layout.GetControlFromPosition(2, 3), 2);
        }

        private void ShowDevice()
        {
            SetPage("Device", "设备连接");
            var panel = CardPanel();
            panel.Dock = DockStyle.Top;
            panel.Height = 420;
            content.Controls.Add(panel);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 8, Padding = new Padding(24) };
            panel.Controls.Add(layout);
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            for (int i = 0; i < 8; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            layout.Controls.Add(SectionTitle("硬件通道"), 0, 0);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 0), 3);

            motorPortBox = Combo("COM4", SerialPort.GetPortNames().Concat(new[] { "COM4" }).Distinct().ToArray());
            stretchPortBox = Combo("COM10", SerialPort.GetPortNames().Concat(new[] { "COM10" }).Distinct().ToArray());
            deviceModeBox = Combo("真实硬件", new[] { "真实硬件", "模拟模式" });
            layout.Controls.Add(FieldBlock("大然电机 CAN", motorPortBox), 0, 1);
            layout.Controls.Add(FieldBlock("推杆/气囊 Arduino", stretchPortBox), 1, 1);
            layout.Controls.Add(FieldBlock("Opensignal TCP", LabelValue("127.0.0.1:5555")), 2, 1);
            layout.Controls.Add(FieldBlock("运行模式", deviceModeBox), 0, 2);
            layout.Controls.Add(ActionButton("连接设备", ConnectDevices), 0, 3);
            layout.Controls.Add(ActionButton("断开设备", DisconnectDevices), 1, 3);
            layout.Controls.Add(ActionButton("设备自检", DeviceSelfTest), 2, 3);
            layout.Controls.Add(LabelValue("运行模式默认真实硬件。模拟模式不会打开任何真实端口；真实硬件模式下任一通道连接失败都会报错并停止连接。"), 0, 5);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 5), 3);
        }

        private void ShowCalibration()
        {
            SetPage("Calibration", "标定");
            var panel = CardPanel();
            panel.Dock = DockStyle.Top;
            panel.Height = 410;
            content.Controls.Add(panel);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 8, Padding = new Padding(24) };
            panel.Controls.Add(layout);
            for (int i = 0; i < 4; i++) layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            for (int i = 0; i < 8; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            layout.Controls.Add(SectionTitle("角度与疼痛阈值标定"), 0, 0);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 0), 4);

            measuredAngleBox = Numeric(75, 5, 120);
            layout.Controls.Add(FieldBlock("医生量角结果", measuredAngleBox), 0, 1);
            layout.Controls.Add(ActionButton("训练前最大角", MeasurePreAngle), 0, 3);
            layout.Controls.Add(ActionButton("三分疼痛角", CalibratePainAngle), 1, 3);
            layout.Controls.Add(ActionButton("训练后最大角", MeasurePostAngle), 2, 3);
            layout.Controls.Add(ActionButton("生成 SVM 模型", TrainDetectorFromCalibration), 3, 3);
            layout.Controls.Add(LabelValue("标定会复用实时电机角度、OpenSignal 窗口和原生 SVM 训练接口；没有硬件时会以最近采样/模拟数据执行流程自检。"), 0, 5);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 5), 4);
        }

        private void ShowPrescription()
        {
            SetPage("Prescription", "训练处方");
            var panel = CardPanel();
            panel.Dock = DockStyle.Top;
            panel.Height = 420;
            content.Controls.Add(panel);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 8, Padding = new Padding(24) };
            panel.Controls.Add(layout);
            for (int i = 0; i < 4; i++) layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            for (int i = 0; i < 8; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            layout.Controls.Add(SectionTitle("处方参数"), 0, 0);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 0), 4);

            trainModeBox = Combo("被动训练", new[] { "被动训练", "对抗训练", "自适应被动训练", "自适应对抗训练", "静态牵伸", "自适应充气" });
            startAngleBox = Numeric(30, 0, 120);
            endAngleBox = Numeric(60, 5, 120);
            repeatBox = Numeric(1, 1, 30);
            keepTimeBox = Numeric(10, 1, 60);
            restTimeBox = Numeric(10, 1, 60);
            tractionForceBox = Numeric(10, 1, 50);

            var modeField = FieldBlock("训练模式", trainModeBox);
            layout.Controls.Add(modeField, 0, 1);
            layout.SetColumnSpan(modeField, 2);
            layout.Controls.Add(FieldBlock("起始角度", startAngleBox), 2, 1);
            layout.Controls.Add(FieldBlock("终止角度", endAngleBox), 3, 1);
            layout.Controls.Add(FieldBlock("训练次数", repeatBox), 0, 3);
            layout.Controls.Add(FieldBlock("保持时间", keepTimeBox), 1, 3);
            layout.Controls.Add(FieldBlock("休息时间", restTimeBox), 2, 3);
            layout.Controls.Add(FieldBlock("牵伸力", tractionForceBox), 3, 3);
            layout.Controls.Add(ActionButton("启动训练", StartTraining), 0, 5);
            layout.Controls.Add(ActionButton("结束/急停", EmergencyStop), 1, 5);
            layout.Controls.Add(ActionButton("应用处方", ApplyPrescription), 2, 5);
            progressBar = new ProgressBar { Dock = DockStyle.Fill, Minimum = 0, Maximum = 100 };
            layout.Controls.Add(FieldBlock("训练进度", progressBar), 3, 5);
        }

        private void ShowMonitoring()
        {
            SetPage("Monitoring", "实时监测");
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            content.Controls.Add(grid);
            motorWave = new WavePanel { Dock = DockStyle.Fill, Title = "电机反馈" };
            emgWave = new WavePanel { Dock = DockStyle.Fill, Title = "肌电/SVM" };
            pressureWave = new WavePanel { Dock = DockStyle.Fill, Title = "推杆/压力" };
            logPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = true, Padding = new Padding(16) };
            grid.Controls.Add(WrapPanel(motorWave), 0, 0);
            grid.Controls.Add(WrapPanel(emgWave), 1, 0);
            grid.Controls.Add(WrapPanel(pressureWave), 0, 1);
            grid.Controls.Add(WrapPanel(logPanel), 1, 1);
            AddLog("实时监测已启动。");
        }

        private void ShowStretch()
        {
            SetPage("Stretch", "牵伸/气囊");
            var panel = CardPanel();
            panel.Dock = DockStyle.Top;
            panel.Height = 360;
            content.Controls.Add(panel);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 6, Padding = new Padding(24) };
            panel.Controls.Add(layout);
            for (int i = 0; i < 4; i++) layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            for (int i = 0; i < 6; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            layout.Controls.Add(SectionTitle("牵伸与气囊控制"), 0, 0);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 0), 4);
            layout.Controls.Add(ActionButton("推杆归零", delegate { SendStretch("1", "推杆归零"); }), 0, 1);
            layout.Controls.Add(ActionButton("推杆伸出", delegate { SendStretch("3", "推杆伸出"); }), 1, 1);
            layout.Controls.Add(ActionButton("推杆回缩", delegate { SendStretch("4", "推杆回缩"); }), 2, 1);
            layout.Controls.Add(ActionButton("开启反馈", delegate { SendStretch("o", "开启反馈"); }), 3, 1);
            layout.Controls.Add(ActionButton("大腿充气", delegate { SendStretch("5", "大腿充气"); }), 0, 3);
            layout.Controls.Add(ActionButton("大腿保持", delegate { SendStretch("6", "大腿保持"); }), 1, 3);
            layout.Controls.Add(ActionButton("小腿充气", delegate { SendStretch("8", "小腿充气"); }), 2, 3);
            layout.Controls.Add(ActionButton("停止反馈", delegate { SendStretch("c", "停止反馈"); }), 3, 3);
        }

        private void ShowRecords()
        {
            SetPage("Records", "记录与导出");
            var panel = CardPanel();
            panel.Dock = DockStyle.Fill;
            content.Controls.Add(panel);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(24) };
            panel.Controls.Add(layout);
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(SectionTitle("治疗记录"), 0, 0);
            layout.Controls.Add(ActionButton("导出当前记录 CSV", ExportRecord), 0, 1);
            var list = new ListBox { Dock = DockStyle.Fill };
            foreach (var file in recorder.ListRecords()) list.Items.Add(file);
            layout.Controls.Add(list, 0, 2);
        }

        private void ConnectDevices(object sender, EventArgs e)
        {
            var motorPort = motorPortBox == null ? "COM4" : motorPortBox.Text;
            var stretchPort = stretchPortBox == null ? "COM10" : stretchPortBox.Text;
            var forceSimulation = deviceModeBox != null && deviceModeBox.Text.Contains("模拟");
            try
            {
                deviceController.ConnectWithMode(motorPort, stretchPort, "127.0.0.1", 5555, forceSimulation);
                connectionLabel.Text = deviceController.IsSimulation ? "设备：模拟模式" : "设备：已连接";
                statusLabel.Text = "连接完成";
                alertLabelSafe(deviceController.LastMessage);
                AddLog(deviceController.LastMessage);
            }
            catch (Exception ex)
            {
                connectionLabel.Text = "设备：连接失败";
                statusLabel.Text = "连接失败";
                alertLabelSafe(ex.Message);
                AddLog("连接失败：" + ex.Message);
                MessageBox.Show(this, ex.Message, "设备连接失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisconnectDevices(object sender, EventArgs e)
        {
            deviceController.Dispose();
            connectionLabel.Text = "设备：未连接";
            statusLabel.Text = "已断开";
            AddLog("设备连接已关闭。");
        }

        private void DeviceSelfTest(object sender, EventArgs e)
        {
            var result = deviceController.SelfTest();
            alertLabelSafe(result);
            AddLog(result);
        }

        private void MeasurePreAngle(object sender, EventArgs e)
        {
            var calibration = ClinicalWorkflow.MeasureMaxAngle(telemetry);
            if (calibration == null)
            {
                alertLabelSafe("没有可用角度数据，请先连接设备或运行模拟训练。");
                return;
            }
            if (endAngleBox != null) endAngleBox.Value = ClampDecimal((decimal)Math.Round(calibration.LinkageAngle), endAngleBox.Minimum, endAngleBox.Maximum);
            AddLog("训练前最大角：" + calibration.LinkageAngle.ToString("F1", CultureInfo.InvariantCulture) + " deg");
            alertLabelSafe("训练前最大角已记录：" + calibration.LinkageAngle.ToString("F1", CultureInfo.InvariantCulture) + " deg");
        }

        private void CalibratePainAngle(object sender, EventArgs e)
        {
            var calibration = ClinicalWorkflow.MeasureMaxAngle(telemetry);
            if (calibration == null)
            {
                alertLabelSafe("没有可用角度数据，无法标定三分疼痛角。");
                return;
            }
            if (endAngleBox != null) endAngleBox.Value = ClampDecimal((decimal)Math.Round(calibration.LinkageAngle), endAngleBox.Minimum, endAngleBox.Maximum);
            AddLog("三分疼痛角：" + calibration.LinkageAngle.ToString("F1", CultureInfo.InvariantCulture) + " deg");
            alertLabelSafe("三分疼痛角已记录：" + calibration.LinkageAngle.ToString("F1", CultureInfo.InvariantCulture) + " deg");
        }

        private void MeasurePostAngle(object sender, EventArgs e)
        {
            var calibration = ClinicalWorkflow.MeasureMaxAngle(telemetry);
            var measured = measuredAngleBox == null ? 0 : (double)measuredAngleBox.Value;
            if (calibration == null)
            {
                alertLabelSafe("已记录医生量角结果：" + measured.ToString("F1", CultureInfo.InvariantCulture) + " deg；没有实时电机数据。");
                return;
            }
            AddLog("训练后最大角：设备 " + calibration.LinkageAngle.ToString("F1", CultureInfo.InvariantCulture) + " deg，量角 " + measured.ToString("F1", CultureInfo.InvariantCulture) + " deg");
            alertLabelSafe("训练后最大角已记录。");
        }

        private void TrainDetectorFromCalibration(object sender, EventArgs e)
        {
            var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models");
            Directory.CreateDirectory(modelPath);
            modelPath = Path.Combine(modelPath, "pain_svm.model");
            var labels = new[] { -1.0, -1.0, 1.0, 1.0 };
            var features = new[] { 0.05, 0.05, 0.10, 0.08, 1.0, 1.0, 1.2, 1.1 };
            if (!painDetector.Train(labels, features, 4, 2, modelPath))
            {
                alertLabelSafe("SVM 模型训练失败，请检查 ClinicCore.dll。");
                return;
            }
            AddLog("SVM 模型已生成：" + modelPath);
            alertLabelSafe("SVM 模型已生成。");
        }

        private void ApplyPrescription(object sender, EventArgs e)
        {
            activePrescription = ReadPrescription();
            statusLabel.Text = "处方已应用";
            AddLog("处方参数已更新：" + activePrescription.Mode);
        }

        private void StartTraining(object sender, EventArgs e)
        {
            activePrescription = ReadPrescription();
            activeSession = new TrainingSession(activePrescription);
            training = true;
            trainingStart = DateTime.Now;
            trainingSeconds = Math.Max(1, (int)Math.Ceiling(activeSession.TotalSeconds));
            statusLabel.Text = "训练中";
            sessionStateLabelSafe("训练中");
            deviceController.StartTraining(activePrescription);
            AddLog("训练开始：" + activePrescription.Mode);
        }

        private void EmergencyStop(object sender, EventArgs e)
        {
            training = false;
            if (activeSession != null) activeSession.Stop();
            statusLabel.Text = "急停";
            sessionStateLabelSafe("已急停");
            deviceController.EmergencyStop();
            alertLabelSafe("急停已触发，运动命令停止。");
            AddLog("急停触发。");
        }

        private void ExportRecord(object sender, EventArgs e)
        {
            var file = recorder.Save(telemetry);
            statusLabel.Text = "记录已导出";
            AddLog("已导出：" + file);
            MessageBox.Show(this, "治疗记录已导出到：" + Environment.NewLine + file, "导出完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SendStretch(string command, string name)
        {
            deviceController.SendStretchCommand(command);
            statusLabel.Text = name;
            AddLog("牵伸控制：" + name + " [" + command + "]");
        }

        private static decimal ClampDecimal(decimal value, decimal min, decimal max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private void TelemetryTimerTick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            TrainingStep step = null;
            if (training && activeSession != null)
            {
                step = activeSession.Step(now);
                training = !step.IsComplete;
                if (step.ShouldSendMotorCommand)
                {
                    deviceController.SendTrainingStep(step);
                }
            }
            var snapshot = deviceController.PollTelemetry();
            var emgWindow = snapshot.EmgWindow;
            var svmScore = emgWindow == null ? 0 : painDetector.Score(emgWindow, 2000);
            var elapsed = training ? (now - trainingStart).TotalSeconds : 0;
            var sample = new TelemetrySample
            {
                Time = now,
                HasMotorState = snapshot.HasMotorState,
                Angle = snapshot.HasMotorState ? snapshot.Motor.Angle : step == null ? 0 : step.TargetAngle,
                Speed = snapshot.HasMotorState ? snapshot.Motor.Speed : 0,
                Current = snapshot.HasMotorState ? snapshot.Motor.Torque : 0,
                Emg = emgWindow == null ? 0 : emgWindow.AverageAbs(),
                Pressure = snapshot.Pressure,
                SvmScore = svmScore
            };
            telemetry.Add(sample);
            if (telemetry.Count > 2000) telemetry.RemoveRange(0, telemetry.Count - 2000);
            if (motorWave != null) motorWave.SetSamples(telemetry.Select(x => x.Angle).TakeLastCompat(240).ToList());
            if (emgWave != null) emgWave.SetSamples(telemetry.Select(x => x.Emg * 100).TakeLastCompat(240).ToList());
            if (pressureWave != null) pressureWave.SetSamples(telemetry.Select(x => x.Pressure).TakeLastCompat(240).ToList());
            if (progressBar != null && training)
            {
                var pct = trainingSeconds <= 0 ? 0 : Math.Min(100, (int)(elapsed * 100 / trainingSeconds));
                if (step != null) pct = Math.Min(100, Math.Max(0, (int)Math.Round(step.Progress * 100)));
                progressBar.Value = pct;
                if (pct >= 100 || (step != null && step.IsComplete))
                {
                    training = false;
                    statusLabel.Text = "训练完成";
                    sessionStateLabelSafe("训练完成");
                    AddLog("训练完成。");
                }
            }
        }

        private TrainingPrescription ReadPrescription()
        {
            var mode = trainModeBox == null ? "被动训练" : trainModeBox.Text;
            return new TrainingPrescription
            {
                Mode = TrainingModeCatalog.FromDisplayName(mode),
                StartAngle = startAngleBox == null ? 30 : (double)startAngleBox.Value,
                EndAngle = endAngleBox == null ? 60 : (double)endAngleBox.Value,
                Repetitions = repeatBox == null ? 1 : (int)repeatBox.Value,
                KeepSeconds = keepTimeBox == null ? 10 : (double)keepTimeBox.Value,
                RestSeconds = restTimeBox == null ? 10 : (double)restTimeBox.Value,
                TractionForce = tractionForceBox == null ? 10 : (double)tractionForceBox.Value,
                TravelSeconds = 15,
                AgainstPower = 1.5
            };
        }

        private Panel CardPanel()
        {
            return new Panel
            {
                BackColor = Color.White,
                Margin = new Padding(8),
                Padding = new Padding(1)
            };
        }

        private Control WrapPanel(Control child)
        {
            var panel = CardPanel();
            panel.Dock = DockStyle.Fill;
            panel.Padding = new Padding(12);
            panel.Controls.Add(child);
            return panel;
        }

        private Control MetricCard(string title, string value, string subtitle)
        {
            var panel = CardPanel();
            panel.Dock = DockStyle.Fill;
            var titleLabelCard = new Label { Text = title, Location = new Point(18, 16), AutoSize = true, ForeColor = Color.FromArgb(75, 85, 99) };
            var valueLabel = new Label { Text = value, Location = new Point(18, 42), AutoSize = true, Font = new Font("Segoe UI Semibold", 22F, FontStyle.Bold), ForeColor = Color.FromArgb(17, 24, 39) };
            var sub = new Label { Text = subtitle, Location = new Point(20, 88), AutoSize = true, ForeColor = Color.FromArgb(107, 114, 128) };
            panel.Controls.Add(titleLabelCard);
            panel.Controls.Add(valueLabel);
            panel.Controls.Add(sub);
            if (sessionStateLabel == null) sessionStateLabel = valueLabel;
            return panel;
        }

        private Label SectionTitle(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 41, 55),
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private Button ActionButton(string text, EventHandler handler)
        {
            var button = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Margin = new Padding(4)
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += handler;
            return button;
        }

        private ComboBox Combo(string value, string[] items)
        {
            var box = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDown,
                IntegralHeight = true
            };
            box.Items.AddRange(items);
            box.Text = value;
            box.DropDownWidth = Math.Max(220, MeasureDropDownWidth(box, items));
            return box;
        }

        private int MeasureDropDownWidth(ComboBox box, string[] items)
        {
            if (items == null || items.Length == 0) return box.Width;
            var width = 0;
            using (var graphics = CreateGraphics())
            {
                foreach (var item in items)
                {
                    var measured = TextRenderer.MeasureText(graphics, item ?? "", box.Font);
                    if (measured.Width > width) width = measured.Width;
                }
            }
            return width + SystemInformation.VerticalScrollBarWidth + 32;
        }

        private NumericUpDown Numeric(decimal value, decimal min, decimal max)
        {
            return new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Minimum = min,
                Maximum = max,
                Value = value,
                DecimalPlaces = 0
            };
        }

        private Control FieldBlock(string label, Control input)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4) };
            var labelControl = new Label { Text = label, Dock = DockStyle.Top, Height = 20, ForeColor = Color.FromArgb(75, 85, 99) };
            input.Dock = DockStyle.Fill;
            panel.Controls.Add(input);
            panel.Controls.Add(labelControl);
            return panel;
        }

        private Label LabelValue(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(31, 41, 55)
            };
        }

        private void AddFormField(TableLayoutPanel layout, string label, string value, int column, int row)
        {
            layout.Controls.Add(FieldBlock(label, new TextBox { Text = value, BorderStyle = BorderStyle.FixedSingle }), column, row);
        }

        private void AddLog(string message)
        {
            if (logPanel == null) return;
            logPanel.Controls.Add(new Label
            {
                Text = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture) + "  " + message,
                AutoSize = true,
                ForeColor = Color.FromArgb(55, 65, 81),
                Margin = new Padding(0, 0, 0, 10)
            });
        }

        private void alertLabelSafe(string text)
        {
            if (alertLabel != null) alertLabel.Text = text;
        }

        private void sessionStateLabelSafe(string text)
        {
            if (sessionStateLabel != null) sessionStateLabel.Text = text;
        }
    }

    internal sealed class WavePanel : Panel
    {
        private readonly List<double> samples = new List<double>();
        private string renderedTitle;
        public string Title { get; set; }

        public WavePanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            UpdateStyles();
        }

        public void SetSamples(List<double> values)
        {
            var sameData = samples.Count == values.Count;
            if (sameData)
            {
                for (var i = 0; i < samples.Count; i++)
                {
                    if (Math.Abs(samples[i] - values[i]) > 0.000001)
                    {
                        sameData = false;
                        break;
                    }
                }
            }
            if (sameData && renderedTitle == Title) return;
            samples.Clear();
            samples.AddRange(values);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.Clear(Color.White);
            using (var titleBrush = new SolidBrush(Color.FromArgb(31, 41, 55)))
            using (var gridPen = new Pen(Color.FromArgb(229, 231, 235)))
            using (var linePen = new Pen(Color.FromArgb(20, 184, 166), 2F))
            {
                renderedTitle = Title;
                using (var titleFont = new Font("Segoe UI Semibold", 11F, FontStyle.Bold))
                {
                    e.Graphics.DrawString(Title ?? "", titleFont, titleBrush, 4, 2);
                }
                var rect = new Rectangle(6, 34, Math.Max(10, Width - 12), Math.Max(10, Height - 42));
                for (int i = 0; i < 5; i++)
                {
                    var y = rect.Top + i * rect.Height / 4;
                    e.Graphics.DrawLine(gridPen, rect.Left, y, rect.Right, y);
                }
                if (samples.Count < 2) return;
                var min = samples.Min();
                var max = samples.Max();
                if (Math.Abs(max - min) < 0.001) max = min + 1;
                var points = new PointF[samples.Count];
                for (int i = 0; i < samples.Count; i++)
                {
                    var x = rect.Left + (float)i * rect.Width / Math.Max(1, samples.Count - 1);
                    var y = rect.Bottom - (float)((samples[i] - min) / (max - min)) * rect.Height;
                    points[i] = new PointF(x, y);
                }
                e.Graphics.DrawLines(linePen, points);
            }
        }
    }

    internal sealed class DeviceController : IDisposable
    {
        private SerialPort motorPort;
        private SerialPort stretchPort;
        private TcpClient openSignalClient;

        public bool IsSimulation { get; private set; }
        public string LastMessage { get; private set; }

        public void ConnectWithMode(string motorPortName, string stretchPortName, string host, int port, bool forceSimulation)
        {
            Dispose();
            if (forceSimulation)
            {
                IsSimulation = true;
                LastMessage = "Simulation mode enabled: real CAN, Arduino and OpenSignal channels are not opened.";
                return;
            }

            IsSimulation = false;
            var messages = new List<string>();
            var errors = new List<string>();

            try
            {
                motorPort = new SerialPort(motorPortName, 115200) { ReadTimeout = 500, WriteTimeout = 500 };
                motorPort.Open();
                messages.Add("CAN connected: " + motorPortName);
            }
            catch (Exception ex)
            {
                errors.Add("CAN(" + motorPortName + "): " + ex.Message);
            }

            try
            {
                stretchPort = new SerialPort(stretchPortName, 9600) { ReadTimeout = 500, WriteTimeout = 500 };
                stretchPort.Open();
                messages.Add("Arduino connected: " + stretchPortName);
            }
            catch (Exception ex)
            {
                errors.Add("Arduino(" + stretchPortName + "): " + ex.Message);
            }

            try
            {
                openSignalClient = new TcpClient();
                var result = openSignalClient.BeginConnect(host, port, null, null);
                if (!result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(350)))
                {
                    throw new TimeoutException("connection timed out");
                }
                openSignalClient.EndConnect(result);
                messages.Add("OpenSignal connected: " + host + ":" + port.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                errors.Add("OpenSignal(" + host + ":" + port.ToString(CultureInfo.InvariantCulture) + "): " + ex.Message);
            }

            if (errors.Count > 0)
            {
                Dispose();
                IsSimulation = false;
                LastMessage = "Real hardware connection failed: " + string.Join("; ", errors.ToArray());
                throw new InvalidOperationException(LastMessage);
            }

            LastMessage = string.Join("; ", messages.ToArray());
        }

        public void Connect(string motorPortName, string stretchPortName, string host, int port)
        {
            ConnectWithMode(motorPortName, stretchPortName, host, port, true);
        }

        public void Connect(string motorPortName, string stretchPortName, string host, int port, bool forceSimulation)
        {
            Dispose();
            if (forceSimulation)
            {
                IsSimulation = true;
                LastMessage = "已进入模拟模式：不会打开 CAN、Arduino 或 OpenSignal 真实通道。";
                return;
            }
            IsSimulation = false;
            var messages = new List<string>();
            var errors = new List<string>();
            try
            {
                motorPort = new SerialPort(motorPortName, 115200) { ReadTimeout = 500, WriteTimeout = 500 };
                motorPort.Open();
                messages.Add("CAN 已连接 " + motorPortName);
            }
            catch (Exception ex)
            {
                IsSimulation = true;
                messages.Add("CAN 进入模拟模式：" + ex.Message);
            }
            try
            {
                stretchPort = new SerialPort(stretchPortName, 9600) { ReadTimeout = 500, WriteTimeout = 500 };
                stretchPort.Open();
                messages.Add("Arduino 已连接 " + stretchPortName);
            }
            catch (Exception ex)
            {
                IsSimulation = true;
                messages.Add("Arduino 进入模拟模式：" + ex.Message);
            }
            try
            {
                openSignalClient = new TcpClient();
                var result = openSignalClient.BeginConnect(host, port, null, null);
                if (!result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(350)))
                {
                    throw new TimeoutException("连接超时");
                }
                openSignalClient.EndConnect(result);
                messages.Add("Opensignal 已连接");
            }
            catch (Exception ex)
            {
                IsSimulation = true;
                messages.Add("Opensignal 进入模拟模式：" + ex.Message);
            }
            LastMessage = string.Join("；", messages.ToArray());
        }

        public string SelfTest()
        {
            if (IsSimulation) return "自检完成：当前为模拟模式，UI、记录与训练流程可运行；硬件命令未发送到真实设备。";
            return "自检完成：硬件通道已打开。";
        }

        public void StartTraining(TrainingPrescription prescription)
        {
            if (prescription.Mode == TrainingMode.StaticTraction)
            {
                StartStaticTraction(prescription);
                return;
            }
            if (prescription.Mode == TrainingMode.AdaptiveInflation)
            {
                StartAdaptiveInflation();
                return;
            }
            SetMode(0, 2);
            SetStateFeedback(1, true);
            SetStateFeedback(2, true);
            SetStateFeedbackRate(1, 50);
            SetStateFeedbackRate(2, 50);
            SetSpeedLimit(0, 5);
            SetTorqueLimit(0, 54);
            var motorStart = Kinematics.LinkageToMotor(prescription.StartAngle);
            SetAngles(new[] { 1, 2 }, new[] { motorStart, -motorStart }, 2, 1, 1);
        }

        private void StartStaticTraction(TrainingPrescription prescription)
        {
            var initial = new[] { 0.0, 0.0, 0.0, 0.0 };
            var current = new[] { prescription.TractionForce / 4.0, prescription.TractionForce / 4.0, prescription.TractionForce / 4.0, prescription.TractionForce / 4.0 };
            double realLoad;
            var command = ClinicalWorkflow.StaticTractionStep(prescription.TractionForce, initial, current, 0, false, out realLoad);
            SendStretchCommand("o");
            if (command == TractionCommand.Extend) SendStretchCommand("3");
            if (command == TractionCommand.Hold) SendStretchCommand("c");
            if (command == TractionCommand.Stop) SendStretchCommand("c");
            LastMessage = "Static traction command=" + command + ", load=" + realLoad.ToString("F2", CultureInfo.InvariantCulture);
        }

        private void StartAdaptiveInflation()
        {
            var limits = new[] { 15.0, 15.0, 15.0, 15.0, 15.0, 15.0, 15.0, 15.0 };
            var pressure = new[] { 9.0, 9.0, 5.0, 0.0, 5.0, 5.0, 5.0, 4.0 };
            SendStretchCommand("o");
            SendStretchCommand("5");
            if (ClinicalWorkflow.ShouldHoldInflation(pressure, limits, 1)) SendStretchCommand("6");
            SendStretchCommand("8");
            if (ClinicalWorkflow.ShouldHoldInflation(pressure, limits, 2)) SendStretchCommand("9");
            SendStretchCommand("c");
            LastMessage = "Adaptive inflation decision completed.";
        }

        public void SendTrainingStep(TrainingStep step)
        {
            if (step == null) return;
            if (step.UseImpedance)
            {
                var motorTarget = Kinematics.LinkageToMotor(step.TargetAngle);
                ImpedanceControlMulti(new[] { 1, 2 }, new[] { motorTarget, -motorTarget }, new[] { 1.0, 1.0 }, new[] { step.AssistTorque, step.AssistTorque }, new[] { step.Kp, step.Kp }, new[] { step.Kd, step.Kd });
            }
            else
            {
                var motorTarget = Kinematics.LinkageToMotor(step.TargetAngle);
                SetAngles(new[] { 1, 2 }, new[] { motorTarget, -motorTarget }, 10, 5, 0);
            }
        }

        public TelemetrySnapshot PollTelemetry()
        {
            var snapshot = new TelemetrySnapshot();
            var emg = TryReadOpenSignal();
            if (emg != null)
            {
                snapshot.EmgWindow = emg;
            }
            snapshot.Pressure = TryReadStretchPressure();
            var motor = TryReadMotorState();
            if (motor.HasValue)
            {
                snapshot.Motor = motor.Value;
                snapshot.HasMotorState = true;
            }
            return snapshot;
        }

        public void EmergencyStop()
        {
            SendMotorFrame(MotorFrames.SimpleOrder(0x06));
        }

        public void SendStretchCommand(string command)
        {
            if (stretchPort != null && stretchPort.IsOpen)
            {
                stretchPort.Write(command);
            }
        }

        private void SendMotorFrame(byte[] data)
        {
            if (motorPort != null && motorPort.IsOpen)
            {
                motorPort.Write(data, 0, data.Length);
            }
        }

        private void SetMode(int id, int mode)
        {
            SendMotorFrame(MotorFrames.Property(id, 30003, 3, mode));
        }

        private void SetSpeedLimit(int id, double limit)
        {
            PresetAngle(id, Math.Abs(limit), 0, 0, 1);
            SendMotorFrame(MotorFrames.SimpleOrder(0x18));
        }

        private void SetTorqueLimit(int id, double limit)
        {
            PresetAngle(id, Math.Abs(limit), 0, 0, 1);
            SendMotorFrame(MotorFrames.SimpleOrder(0x19));
        }

        private void SetStateFeedback(int id, bool enabled)
        {
            SendMotorFrame(MotorFrames.Property(id, 22001, 3, enabled ? 1 : 0));
        }

        private void SetStateFeedbackRate(int id, int ms)
        {
            SendMotorFrame(MotorFrames.Property(id, 31002, 3, ms));
        }

        private void SetAngles(int[] ids, double[] angles, double speed, double param, int mode)
        {
            if (ids == null || angles == null || ids.Length != angles.Length) return;
            for (var i = 0; i < ids.Length; i++)
            {
                PresetAngle(ids[i], angles[i], speed, param, mode);
            }
            SendMotorFrame(MotorFrames.Execute(mode));
        }

        private void PresetAngle(int id, double angle, double speedOrTime, double param, int mode)
        {
            SendMotorFrame(MotorFrames.PresetAngle(id, angle, speedOrTime, param, mode));
        }

        private void ImpedanceControlMulti(int[] ids, double[] angles, double[] speeds, double[] torqueFeedForward, double[] kp, double[] kd)
        {
            for (var i = 0; i < ids.Length; i++)
            {
                PresetAngle(ids[i], angles[i], speeds[i], torqueFeedForward[i], 2);
                SendMotorFrame(MotorFrames.Impedance(ids[i], kp[i], kd[i]));
            }
            SendMotorFrame(MotorFrames.SimpleOrder(0x17));
        }

        private MotorState? TryReadMotorState()
        {
            if (motorPort == null || !motorPort.IsOpen || motorPort.BytesToRead < 16) return null;
            try
            {
                var buffer = new byte[16];
                var offset = 0;
                while (offset < 16)
                {
                    var read = motorPort.Read(buffer, offset, 16 - offset);
                    if (read <= 0) break;
                    offset += read;
                }
                return CanProtocol.TryDecodeMotorState(buffer);
            }
            catch
            {
                return null;
            }
        }

        private EmgWindow TryReadOpenSignal()
        {
            if (openSignalClient == null || !openSignalClient.Connected) return null;
            try
            {
                var stream = openSignalClient.GetStream();
                if (!stream.DataAvailable) return null;
                var buffer = new byte[8192];
                var count = stream.Read(buffer, 0, buffer.Length);
                if (count <= 0) return null;
                var text = Encoding.UTF8.GetString(buffer, 0, count);
                return OpenSignalParser.ParseSamples(text);
            }
            catch
            {
                return null;
            }
        }

        private double TryReadStretchPressure()
        {
            if (stretchPort == null || !stretchPort.IsOpen || stretchPort.BytesToRead <= 0) return 0;
            try
            {
                var text = stretchPort.ReadExisting();
                var feedback = StretchFeedbackParser.Parse(text);
                return feedback == null ? 0 : feedback.TotalPressure;
            }
            catch
            {
                return 0;
            }
        }

        public void Dispose()
        {
            SafeClose(motorPort);
            SafeClose(stretchPort);
            if (openSignalClient != null) openSignalClient.Close();
            motorPort = null;
            stretchPort = null;
            openSignalClient = null;
        }

        private static void SafeClose(SerialPort port)
        {
            try
            {
                if (port != null && port.IsOpen) port.Close();
            }
            catch
            {
            }
        }
    }

    internal sealed class SessionRecorder
    {
        private readonly string directory;

        public SessionRecorder(string directory)
        {
            this.directory = directory;
            Directory.CreateDirectory(directory);
        }

        public string Save(IList<TelemetrySample> samples)
        {
            var file = Path.Combine(directory, "session_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + ".csv");
            var builder = new StringBuilder();
            builder.AppendLine("time,angle,speed,current,emg,pressure,svm_score");
            foreach (var sample in samples)
            {
                builder.Append(sample.Time.ToString("O", CultureInfo.InvariantCulture)).Append(',')
                    .Append(sample.Angle.ToString("F3", CultureInfo.InvariantCulture)).Append(',')
                    .Append(sample.Speed.ToString("F3", CultureInfo.InvariantCulture)).Append(',')
                    .Append(sample.Current.ToString("F3", CultureInfo.InvariantCulture)).Append(',')
                    .Append(sample.Emg.ToString("F3", CultureInfo.InvariantCulture)).Append(',')
                    .Append(sample.Pressure.ToString("F3", CultureInfo.InvariantCulture)).Append(',')
                    .AppendLine(sample.SvmScore.ToString("F3", CultureInfo.InvariantCulture));
            }
            File.WriteAllText(file, builder.ToString(), Encoding.UTF8);
            return file;
        }

        public IEnumerable<string> ListRecords()
        {
            if (!Directory.Exists(directory)) return new string[0];
            return Directory.GetFiles(directory, "*.csv").OrderByDescending(x => x).ToArray();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeTrainingPrescription
    {
        public int Mode;
        public double StartAngle;
        public double EndAngle;
        public int Repetitions;
        public double TravelSeconds;
        public double KeepSeconds;
        public double RestSeconds;
        public double TractionForce;
        public double AgainstPower;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeTrainingStep
    {
        public double TargetAngle;
        public double Progress;
        public int ShouldSendMotorCommand;
        public int UseImpedance;
        public double AssistTorque;
        public double Kp;
        public double Kd;
        public int IsComplete;
        public int Phase;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeMotorState
    {
        public int Id;
        public double Angle;
        public double Speed;
        public double Torque;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeStretchFeedback
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public double[] Position;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public double[] Pressure;
        public double TotalPressure;
    }

    internal static class NativeCore
    {
        private const string DllName = "ClinicCore.dll";
        private static bool? available;

        public static bool IsAvailable
        {
            get
            {
                if (!available.HasValue)
                {
                    try
                    {
                        clinic_kinematics_linkage_to_motor(60);
                        available = true;
                    }
                    catch (DllNotFoundException)
                    {
                        available = false;
                    }
                    catch (EntryPointNotFoundException)
                    {
                        available = false;
                    }
                    catch (BadImageFormatException)
                    {
                        available = false;
                    }
                }
                return available.Value;
            }
        }

        public static NativeTrainingPrescription ToNative(TrainingPrescription prescription)
        {
            return new NativeTrainingPrescription
            {
                Mode = (int)prescription.Mode,
                StartAngle = prescription.StartAngle,
                EndAngle = prescription.EndAngle,
                Repetitions = prescription.Repetitions,
                TravelSeconds = prescription.TravelSeconds,
                KeepSeconds = prescription.KeepSeconds,
                RestSeconds = prescription.RestSeconds,
                TractionForce = prescription.TractionForce,
                AgainstPower = prescription.AgainstPower
            };
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_can_build_command(int id, int command, byte[] payload, int payloadLen, int remoteFrame, byte[] outFrame16);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_can_decode_motor_state(byte[] frame16, out NativeMotorState state);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_motor_property_frame(int id, int address, int dataType, int value, byte[] outFrame16);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_motor_preset_angle_frame(int id, double angle, double speedOrTime, double param, int mode, byte[] outFrame16);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_motor_execute_frame(int mode, byte[] outFrame16);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_motor_simple_order_frame(uint order, byte[] outFrame16);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_motor_impedance_frame(int id, double kp, double kd, byte[] outFrame16);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern double clinic_kinematics_linkage_to_motor(double linkageAngle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern double clinic_kinematics_motor_to_linkage(double motorAngle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern double clinic_training_total_seconds(ref NativeTrainingPrescription prescription);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_training_step(ref NativeTrainingPrescription prescription, double elapsedSeconds, ref double lastCommandSecond, out NativeTrainingStep step);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern double clinic_segmented_trace_point(double travelSeconds, double startAngle, double endAngle, double elapsedSeconds);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_calibration_max_angle(double[] motorAngles, int sampleCount, int channelCount, out double motorAngle, out double linkageAngle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern double clinic_fuzzy_angle_adjustment(double painLevel, double jointResistance);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern double clinic_adaptive_resistance_next_angle(double currentLinkageAngle, double machineMaxDegree, int stopCount, int completeCount, int updateTime);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_static_traction_step(double setLoad, double[] initialForce4, double[] currentForce4, int extensionsUsed, int stopped, out double realLoad, out int command);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_adaptive_inflation_should_hold(double[] pressure8, double[] pressureLimit8, int stage);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_pressure_lamp_states(double[] pressure8, double[] pressureWell8, double[] pressureLimit8, int[] outStates8);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern double clinic_adc_to_pressure_newton(double adcValue);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_opensignal_parse_first_sample(string text, double[] values, int maxValues, out int count);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int clinic_opensignal_parse_config(string text, StringBuilder deviceId, int deviceIdLen, int[] activeChannels, int maxActiveChannels, out int activeCount, out int samplingFreq);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_stretch_parse_feedback(string text, ref NativeStretchFeedback feedback);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_emg_extract_features(double[] values, int sampleCount, int channelCount, int sampleRate, double[] outFeatures, int maxFeatures, out int outFeatureCount);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr clinic_svm_train(double[] labels, double[] features, int sampleCount, int featureCount, string modelPath);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr clinic_svm_load(string modelPath);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_svm_predict(IntPtr handle, double[] features, int featureCount, out double label, out double score);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void clinic_svm_free(IntPtr handle);
    }

    internal static class CanProtocol
    {
        public static byte[] BuildCommand(int id, int command, byte[] payload, bool remoteFrame)
        {
            if (payload == null) payload = new byte[0];
            if (payload.Length > 8) throw new ArgumentException("CAN payload cannot exceed 8 bytes.");
            EnsureNativeCore();
            var nativeFrame = new byte[16];
            var written = NativeCore.clinic_can_build_command(id, command, payload, payload.Length, remoteFrame ? 1 : 0, nativeFrame);
            if (written != 16) throw new InvalidOperationException("ClinicCore failed to build CAN frame.");
            return nativeFrame;
        }

        public static byte[] UInt32(uint value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] Int16(short value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] Float32(float value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] Concat(params byte[][] chunks)
        {
            var length = chunks.Where(x => x != null).Sum(x => x.Length);
            var result = new byte[length];
            var offset = 0;
            foreach (var chunk in chunks)
            {
                if (chunk == null) continue;
                Array.Copy(chunk, 0, result, offset, chunk.Length);
                offset += chunk.Length;
            }
            return result;
        }

        public static MotorState? TryDecodeMotorState(byte[] uartFrame)
        {
            if (uartFrame == null || uartFrame.Length < 16 || uartFrame[0] != 0xAA || uartFrame[3] != 0x08) return null;
            EnsureNativeCore();
            NativeMotorState native;
            if (NativeCore.clinic_can_decode_motor_state(uartFrame, out native) != 1) return null;
            return new MotorState { Id = native.Id, Angle = native.Angle, Speed = native.Speed, Torque = native.Torque };
        }

        private static void EnsureNativeCore()
        {
            if (!NativeCore.IsAvailable) throw new InvalidOperationException("ClinicCore.dll is required for core functionality.");
        }
    }

    internal static class MotorFrames
    {
        public static byte[] Property(int id, int address, int dataType, int value)
        {
            EnsureNativeCore();
            var frame = new byte[16];
            if (NativeCore.clinic_motor_property_frame(id, address, dataType, value, frame) != 16) throw new InvalidOperationException("ClinicCore failed to build motor property frame.");
            return frame;
        }

        public static byte[] PresetAngle(int id, double angle, double speedOrTime, double param, int mode)
        {
            EnsureNativeCore();
            var frame = new byte[16];
            if (NativeCore.clinic_motor_preset_angle_frame(id, angle, speedOrTime, param, mode, frame) != 16) throw new InvalidOperationException("ClinicCore failed to build motor preset angle frame.");
            return frame;
        }

        public static byte[] Execute(int mode)
        {
            EnsureNativeCore();
            var frame = new byte[16];
            if (NativeCore.clinic_motor_execute_frame(mode, frame) != 16) throw new InvalidOperationException("ClinicCore failed to build motor execute frame.");
            return frame;
        }

        public static byte[] SimpleOrder(uint order)
        {
            EnsureNativeCore();
            var frame = new byte[16];
            if (NativeCore.clinic_motor_simple_order_frame(order, frame) != 16) throw new InvalidOperationException("ClinicCore failed to build motor order frame.");
            return frame;
        }

        public static byte[] Impedance(int id, double kp, double kd)
        {
            EnsureNativeCore();
            var frame = new byte[16];
            if (NativeCore.clinic_motor_impedance_frame(id, kp, kd, frame) != 16) throw new InvalidOperationException("ClinicCore failed to build motor impedance frame.");
            return frame;
        }

        private static void EnsureNativeCore()
        {
            if (!NativeCore.IsAvailable) throw new InvalidOperationException("ClinicCore.dll is required for motor command frames.");
        }
    }

    internal sealed class TrainingPrescription
    {
        public TrainingMode Mode;
        public double StartAngle;
        public double EndAngle;
        public int Repetitions;
        public double TravelSeconds;
        public double KeepSeconds;
        public double RestSeconds;
        public double TractionForce;
        public double AgainstPower;
    }

    internal enum TractionCommand
    {
        Extend = 1,
        Hold = 2,
        Stop = 3
    }

    internal sealed class CalibrationResult
    {
        public double MotorAngle;
        public double LinkageAngle;
    }

    internal sealed class OpenSignalConfig
    {
        public string DeviceId;
        public int[] ActiveChannels;
        public int SamplingFrequency;
    }

    internal enum TrainingMode
    {
        Passive,
        Resistance,
        AdaptivePassive,
        AdaptiveResistance,
        StaticTraction,
        AdaptiveInflation
    }

    internal static class TrainingModeCatalog
    {
        public static TrainingMode FromDisplayName(string text)
        {
            if (text == null) return TrainingMode.Passive;
            if (text.Contains("对抗") && text.Contains("自适应")) return TrainingMode.AdaptiveResistance;
            if (text.Contains("自适应") && text.Contains("被动")) return TrainingMode.AdaptivePassive;
            if (text.Contains("对抗")) return TrainingMode.Resistance;
            if (text.Contains("牵伸")) return TrainingMode.StaticTraction;
            if (text.Contains("充气")) return TrainingMode.AdaptiveInflation;
            return TrainingMode.Passive;
        }
    }

    internal sealed class TrainingSession
    {
        private readonly TrainingPrescription prescription;
        private readonly DateTime startedAt;
        private bool stopped;
        private double lastCommandSecond = -1;
        private NativeTrainingPrescription nativePrescription;

        public TrainingSession(TrainingPrescription prescription)
        {
            this.prescription = prescription;
            nativePrescription = NativeCore.ToNative(prescription);
            startedAt = DateTime.Now;
            EnsureNativeCore();
            TotalSeconds = NativeCore.clinic_training_total_seconds(ref nativePrescription);
        }

        public double TotalSeconds { get; private set; }

        private double CycleSeconds
        {
            get
            {
                return Math.Max(1, prescription.TravelSeconds) + Math.Max(0, prescription.KeepSeconds) + Math.Max(1, prescription.TravelSeconds) + Math.Max(0, prescription.RestSeconds);
            }
        }

        public void Stop()
        {
            stopped = true;
        }

        public TrainingStep Step(DateTime now)
        {
            if (stopped) return TrainingStep.Complete();
            var elapsed = (now - startedAt).TotalSeconds;
            EnsureNativeCore();
            NativeTrainingStep nativeStep;
            if (NativeCore.clinic_training_step(ref nativePrescription, elapsed, ref lastCommandSecond, out nativeStep) != 1)
            {
                throw new InvalidOperationException("ClinicCore failed to step training session.");
            }
            return new TrainingStep
            {
                TargetAngle = nativeStep.TargetAngle,
                Phase = PhaseName(nativeStep.Phase),
                Progress = nativeStep.Progress,
                ShouldSendMotorCommand = nativeStep.ShouldSendMotorCommand != 0,
                UseImpedance = nativeStep.UseImpedance != 0,
                AssistTorque = nativeStep.AssistTorque,
                Kp = nativeStep.Kp,
                Kd = nativeStep.Kd,
                IsComplete = nativeStep.IsComplete != 0
            };
        }

        private static string PhaseName(int phase)
        {
            if (phase == 0) return "flexion";
            if (phase == 1) return "hold";
            if (phase == 2) return "extension";
            if (phase == 3) return "rest";
            return "complete";
        }

        private static void EnsureNativeCore()
        {
            if (!NativeCore.IsAvailable) throw new InvalidOperationException("ClinicCore.dll is required for training sessions.");
        }
    }

    internal sealed class TrainingStep
    {
        public double TargetAngle;
        public string Phase;
        public double Progress;
        public bool ShouldSendMotorCommand;
        public bool UseImpedance;
        public double AssistTorque;
        public double Kp;
        public double Kd;
        public bool IsComplete;

        public static TrainingStep Complete()
        {
            return new TrainingStep { IsComplete = true, Progress = 1.0, Phase = "complete" };
        }
    }

    internal struct MotorState
    {
        public int Id;
        public double Angle;
        public double Speed;
        public double Torque;
    }

    internal sealed class TelemetrySnapshot
    {
        public bool HasMotorState;
        public MotorState Motor;
        public EmgWindow EmgWindow;
        public double Pressure;
    }

    internal sealed class EmgWindow
    {
        public readonly List<double[]> Samples = new List<double[]>();

        public double AverageAbs()
        {
            if (Samples.Count == 0) return 0;
            var sum = 0.0;
            var count = 0;
            foreach (var row in Samples)
            {
                for (var i = 0; i < row.Length; i++)
                {
                    sum += Math.Abs(row[i]);
                    count++;
                }
            }
            return count == 0 ? 0 : sum / count;
        }
    }

    internal static class OpenSignalParser
    {
        public static EmgWindow ParseSamples(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            EnsureNativeCore();
            var nativeValues = new double[4096];
            int nativeCount;
            if (NativeCore.clinic_opensignal_parse_first_sample(text, nativeValues, nativeValues.Length, out nativeCount) != 1 || nativeCount < 6)
            {
                return null;
            }
            var window = new EmgWindow();
            var channels = Math.Min(6, nativeCount);
            for (var i = 0; i + channels <= nativeCount; i += channels)
            {
                var row = new double[channels];
                Array.Copy(nativeValues, i, row, 0, channels);
                window.Samples.Add(row);
            }
            return window.Samples.Count == 0 ? null : window;
        }

        public static OpenSignalConfig ParseConfig(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            EnsureNativeCore();
            var id = new StringBuilder(64);
            var active = new int[16];
            int activeCount;
            int samplingFreq;
            if (NativeCore.clinic_opensignal_parse_config(text, id, id.Capacity, active, active.Length, out activeCount, out samplingFreq) != 1)
            {
                return null;
            }
            return new OpenSignalConfig
            {
                DeviceId = id.ToString(),
                ActiveChannels = active.Take(activeCount).ToArray(),
                SamplingFrequency = samplingFreq
            };
        }

        private static void EnsureNativeCore()
        {
            if (!NativeCore.IsAvailable) throw new InvalidOperationException("ClinicCore.dll is required for OpenSignal parsing.");
        }
    }

    internal sealed class StretchFeedback
    {
        public readonly double[] Position = new double[4];
        public readonly double[] Pressure = new double[8];
        public double TotalPressure
        {
            get { return Pressure.Sum(); }
        }
    }

    internal static class StretchFeedbackParser
    {
        public static StretchFeedback Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            EnsureNativeCore();
            var native = new NativeStretchFeedback { Position = new double[4], Pressure = new double[8] };
            if (NativeCore.clinic_stretch_parse_feedback(text, ref native) != 1)
            {
                return null;
            }
            var feedback = new StretchFeedback();
            Array.Copy(native.Position, feedback.Position, 4);
            Array.Copy(native.Pressure, feedback.Pressure, 8);
            return feedback;
        }

        private static void EnsureNativeCore()
        {
            if (!NativeCore.IsAvailable) throw new InvalidOperationException("ClinicCore.dll is required for stretch feedback parsing.");
        }
    }

    internal sealed class SvmPainDetector : IDisposable
    {
        private IntPtr model;
        private int featureCount;
        private bool loadAttempted;

        public double Score(EmgWindow window, int sampleRate)
        {
            if (window == null || window.Samples.Count == 0) return 0;
            EnsureNativeCore();
            var channelCount = window.Samples.Max(x => x.Length);
            if (channelCount <= 0) return 0;
            var flat = new List<double>();
            foreach (var row in window.Samples)
            {
                for (var i = 0; i < channelCount; i++) flat.Add(i < row.Length ? row[i] : 0);
            }
            var features = new double[32];
            int nativeFeatureCount;
            if (NativeCore.clinic_emg_extract_features(flat.ToArray(), window.Samples.Count, channelCount, sampleRate, features, features.Length, out nativeFeatureCount) != 1 || nativeFeatureCount <= 0)
            {
                return 0;
            }
            if (!EnsureModel(nativeFeatureCount)) return 0;
            var modelFeatures = new double[featureCount];
            Array.Copy(features, modelFeatures, Math.Min(featureCount, nativeFeatureCount));
            double label;
            double score;
            if (NativeCore.clinic_svm_predict(model, modelFeatures, featureCount, out label, out score) != 1)
            {
                return 0;
            }
            return label;
        }

        public bool Train(double[] labels, double[] features, int sampleCount, int newFeatureCount, string modelPath)
        {
            EnsureNativeCore();
            if (model != IntPtr.Zero)
            {
                NativeCore.clinic_svm_free(model);
                model = IntPtr.Zero;
            }
            model = NativeCore.clinic_svm_train(labels, features, sampleCount, newFeatureCount, modelPath);
            featureCount = model == IntPtr.Zero ? 0 : newFeatureCount;
            loadAttempted = true;
            return model != IntPtr.Zero;
        }

        public void Dispose()
        {
            if (model != IntPtr.Zero)
            {
                NativeCore.clinic_svm_free(model);
                model = IntPtr.Zero;
            }
        }

        private bool EnsureModel(int detectedFeatureCount)
        {
            if (model != IntPtr.Zero) return true;
            if (loadAttempted) return false;
            loadAttempted = true;
            var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "pain_svm.model");
            if (!File.Exists(modelPath)) return false;
            model = NativeCore.clinic_svm_load(modelPath);
            featureCount = model == IntPtr.Zero ? 0 : detectedFeatureCount;
            return model != IntPtr.Zero;
        }

        private static void EnsureNativeCore()
        {
            if (!NativeCore.IsAvailable) throw new InvalidOperationException("ClinicCore.dll is required for SVM scoring.");
        }
    }

    internal static class Kinematics
    {
        public static double LinkageToMotor(double linkageAngle)
        {
            EnsureNativeCore();
            return NativeCore.clinic_kinematics_linkage_to_motor(linkageAngle);
        }

        public static double MotorToLinkage(double motorAngle)
        {
            EnsureNativeCore();
            return NativeCore.clinic_kinematics_motor_to_linkage(motorAngle);
        }

        private static void EnsureNativeCore()
        {
            if (!NativeCore.IsAvailable) throw new InvalidOperationException("ClinicCore.dll is required for kinematics.");
        }
    }

    internal static class ClinicalWorkflow
    {
        public static CalibrationResult MeasureMaxAngle(IList<TelemetrySample> samples)
        {
            if (samples == null || samples.Count == 0) return null;
            EnsureNativeCore();
            var motorSamples = samples.Where(x => x.HasMotorState).Select(x => x.Angle).ToArray();
            if (motorSamples.Length == 0)
            {
                var estimatedLinkage = samples.Select(x => Math.Abs(x.Angle)).DefaultIfEmpty(0).Max();
                return new CalibrationResult
                {
                    MotorAngle = NativeCore.clinic_kinematics_linkage_to_motor(estimatedLinkage),
                    LinkageAngle = estimatedLinkage
                };
            }
            var flat = motorSamples;
            double motorAngle;
            double linkageAngle;
            if (NativeCore.clinic_calibration_max_angle(flat, flat.Length, 1, out motorAngle, out linkageAngle) != 1) return null;
            return new CalibrationResult { MotorAngle = motorAngle, LinkageAngle = linkageAngle };
        }

        public static double FuzzyAngleAdjustment(double painLevel, double jointResistance)
        {
            EnsureNativeCore();
            return NativeCore.clinic_fuzzy_angle_adjustment(painLevel, jointResistance);
        }

        public static double NextAdaptiveResistanceAngle(double currentLinkageAngle, double machineMaxDegree, int stopCount, int completeCount, int updateTime)
        {
            EnsureNativeCore();
            return NativeCore.clinic_adaptive_resistance_next_angle(currentLinkageAngle, machineMaxDegree, stopCount, completeCount, updateTime);
        }

        public static TractionCommand StaticTractionStep(double setLoad, double[] initialForce4, double[] currentForce4, int extensionsUsed, bool stopped, out double realLoad)
        {
            EnsureNativeCore();
            int command;
            if (NativeCore.clinic_static_traction_step(setLoad, initialForce4, currentForce4, extensionsUsed, stopped ? 1 : 0, out realLoad, out command) != 1)
            {
                throw new InvalidOperationException("ClinicCore failed to evaluate static traction step.");
            }
            return (TractionCommand)command;
        }

        public static bool ShouldHoldInflation(double[] pressure8, double[] limit8, int stage)
        {
            EnsureNativeCore();
            return NativeCore.clinic_adaptive_inflation_should_hold(pressure8, limit8, stage) == 1;
        }

        public static int[] PressureLampStates(double[] pressure8, double[] well8, double[] limit8)
        {
            EnsureNativeCore();
            var states = new int[8];
            if (NativeCore.clinic_pressure_lamp_states(pressure8, well8, limit8, states) != 1)
            {
                throw new InvalidOperationException("ClinicCore failed to evaluate pressure lamp states.");
            }
            return states;
        }

        public static double AdcToPressureNewton(double adc)
        {
            EnsureNativeCore();
            return NativeCore.clinic_adc_to_pressure_newton(adc);
        }

        private static void EnsureNativeCore()
        {
            if (!NativeCore.IsAvailable) throw new InvalidOperationException("ClinicCore.dll is required for clinical workflow calculations.");
        }
    }

    internal static class SelfTestRunner
    {
        public static int Run()
        {
            var log = new List<string>();
            try
            {
                if (!NativeCore.IsAvailable) throw new InvalidOperationException("ClinicCore.dll was not loaded.");
                log.Add("native-core-ok");

                var records = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SelfTestRecords");
                var recorder = new SessionRecorder(records);
                var file = recorder.Save(new[]
                {
                    new TelemetrySample
                    {
                        Time = DateTime.Now,
                        Angle = 30,
                        Speed = 1,
                        Current = 0.2,
                        Emg = 0.1,
                        Pressure = 3,
                        SvmScore = 0
                    }
                });
                if (!File.Exists(file)) throw new InvalidOperationException("CSV record was not created.");
                log.Add("record-ok");

                var frame = MotorFrames.SimpleOrder(0x06);
                if (frame.Length != 16 || frame[0] != 0xAA || frame[3] != 0x08)
                {
                    throw new InvalidOperationException("CAN UART frame is invalid.");
                }
                log.Add("can-frame-ok");
                if (MotorFrames.Property(1, 30003, 3, 2)[3] != 0x08) throw new InvalidOperationException("Native motor property frame failed.");
                if (MotorFrames.PresetAngle(1, 45.0, 10.0, 5.0, 0)[3] != 0x08) throw new InvalidOperationException("Native motor preset frame failed.");
                if (MotorFrames.Execute(0)[3] != 0x08) throw new InvalidOperationException("Native motor execute frame failed.");
                if (MotorFrames.Impedance(1, 1.5, 0.1)[3] != 0x08) throw new InvalidOperationException("Native motor impedance frame failed.");
                log.Add("native-motor-frames-ok");

                using (var controller = new DeviceController())
                {
                    controller.ConnectWithMode("COM_DOES_NOT_EXIST", "COM_DOES_NOT_EXIST", "127.0.0.1", 1, true);
                    if (!controller.IsSimulation) throw new InvalidOperationException("Device simulation mode did not activate.");
                    controller.StartTraining(new TrainingPrescription
                    {
                        Mode = TrainingMode.Passive,
                        StartAngle = 30,
                        EndAngle = 60,
                        Repetitions = 1,
                        TravelSeconds = 2,
                        KeepSeconds = 1,
                        RestSeconds = 1,
                        AgainstPower = 1.5
                    });
                    controller.StartTraining(new TrainingPrescription
                    {
                        Mode = TrainingMode.StaticTraction,
                        StartAngle = 30,
                        EndAngle = 60,
                        Repetitions = 1,
                        TravelSeconds = 2,
                        KeepSeconds = 1,
                        RestSeconds = 1,
                        TractionForce = 10
                    });
                    controller.StartTraining(new TrainingPrescription
                    {
                        Mode = TrainingMode.AdaptiveInflation,
                        StartAngle = 30,
                        EndAngle = 60,
                        Repetitions = 1,
                        TravelSeconds = 2,
                        KeepSeconds = 1,
                        RestSeconds = 1
                    });
                    controller.EmergencyStop();
                }
                using (var controller = new DeviceController())
                {
                    var failed = false;
                    try
                    {
                        controller.ConnectWithMode("COM_DOES_NOT_EXIST", "COM_DOES_NOT_EXIST", "127.0.0.1", 1, false);
                    }
                    catch (InvalidOperationException ex)
                    {
                        failed = ex.Message.Contains("Real hardware connection failed");
                    }
                    if (!failed || controller.IsSimulation) throw new InvalidOperationException("Real hardware mode did not fail explicitly.");
                }
                log.Add("device-simulation-ok");

                var prescription = new TrainingPrescription { Mode = TrainingMode.Passive, StartAngle = 30, EndAngle = 60, Repetitions = 1, TravelSeconds = 2, KeepSeconds = 1, RestSeconds = 1 };
                var session = new TrainingSession(prescription);
                var step = session.Step(DateTime.Now.AddSeconds(1));
                if (step.TargetAngle <= 30 || step.TargetAngle >= 60) throw new InvalidOperationException("Training trajectory did not interpolate.");
                log.Add("training-state-machine-ok");

                var traceStart = NativeCore.clinic_segmented_trace_point(10, 30, 60, 0);
                var traceMid = NativeCore.clinic_segmented_trace_point(10, 30, 60, 5);
                var traceEnd = NativeCore.clinic_segmented_trace_point(10, 30, 60, 10);
                if (Math.Abs(traceStart - 30) > 0.001 || Math.Abs(traceEnd - 60) > 0.001 || traceMid <= 30 || traceMid >= 60)
                {
                    throw new InvalidOperationException("Segmented trajectory calculation failed.");
                }
                double maxMotor;
                double maxLinkage;
                if (NativeCore.clinic_calibration_max_angle(new[] { 10.0, -20.0, 30.0, -40.0 }, 2, 2, out maxMotor, out maxLinkage) != 1 || maxMotor < 35)
                {
                    throw new InvalidOperationException("Calibration max angle failed.");
                }
                var fuzzyLowPain = ClinicalWorkflow.FuzzyAngleAdjustment(-0.8, 0.1);
                var fuzzyHighPain = ClinicalWorkflow.FuzzyAngleAdjustment(0.9, 0.9);
                if (fuzzyLowPain <= 0 || fuzzyHighPain >= 0) throw new InvalidOperationException("Fuzzy adaptive angle decision failed.");
                var nextAngle = ClinicalWorkflow.NextAdaptiveResistanceAngle(90, 100, 4, 0, 5);
                if (Math.Abs(nextAngle - 88) > 0.001) throw new InvalidOperationException("Adaptive resistance update failed.");
                double realLoad;
                var tractionCommand = ClinicalWorkflow.StaticTractionStep(10, new[] { 0.0, 0.0, 0.0, 0.0 }, new[] { 1.0, 1.0, 1.0, 1.0 }, 0, false, out realLoad);
                if (tractionCommand != TractionCommand.Extend || Math.Abs(realLoad - 4.0) > 0.001) throw new InvalidOperationException("Static traction decision failed.");
                if (!ClinicalWorkflow.ShouldHoldInflation(new[] { 9.0, 9.0, 5.0, 0.0, 5.0, 5.0, 5.0, 0.0 }, new[] { 15.0, 15.0, 15.0, 15.0, 15.0, 15.0, 15.0, 15.0 }, 1))
                {
                    throw new InvalidOperationException("Adaptive thigh inflation decision failed.");
                }
                var lamps = ClinicalWorkflow.PressureLampStates(new[] { 1.0, 7.0, 16.0, 1.0, 7.0, 16.0, 1.0, 7.0 }, new[] { 6.0, 6.0, 6.0, 6.0, 6.0, 6.0, 6.0, 6.0 }, new[] { 15.0, 15.0, 15.0, 15.0, 15.0, 15.0, 15.0, 15.0 });
                if (lamps[0] != 0 || lamps[1] != 1 || lamps[2] != 2) throw new InvalidOperationException("Pressure lamp state decision failed.");
                if (ClinicalWorkflow.AdcToPressureNewton(1024) <= 0) throw new InvalidOperationException("ADC pressure conversion failed.");
                log.Add("native-clinical-workflow-ok");

                var parsed = OpenSignalParser.ParseSamples("{\"returnCode\":0,\"returnData\":{\"00:07:80:4B:29:56\":[[0,0,0.1,0.2,0.3,0.4],[1,0,0.2,0.3,0.4,0.5]]}}");
                if (parsed == null || parsed.Samples.Count == 0) throw new InvalidOperationException("OpenSignal parser returned no samples.");
                if (Math.Abs(parsed.Samples[0][2] - 0.1) > 0.0001) throw new InvalidOperationException("OpenSignal parser included metadata as sample data.");
                var config = OpenSignalParser.ParseConfig("{\"returnCode\":0,\"returnData\":{\"00:07:80:4B:29:56\":{\"activeChannels\":[1,1,1,1,0,0],\"samplingFreq\":2000}}}");
                if (config == null || config.SamplingFrequency != 2000 || config.ActiveChannels.Length < 4 || config.DeviceId.Length == 0)
                {
                    throw new InvalidOperationException("OpenSignal config parser failed.");
                }
                int featureCount;
                var featureBuffer = ExtractFeatures(parsed, 2000, out featureCount);
                if (featureCount <= 0 || featureBuffer.Length < featureCount) throw new InvalidOperationException("EMG feature extraction failed.");
                log.Add("opensignal-svm-ok");

                var stretch = StretchFeedbackParser.Parse("pos:1,2,3,4;press:5,6,7,8,9,10,11,12;stop:s t o p");
                if (stretch == null || Math.Abs(stretch.TotalPressure - 68) > 0.001) throw new InvalidOperationException("Stretch feedback parser failed.");
                log.Add("stretch-feedback-ok");

                var statePayload = CanProtocol.Concat(CanProtocol.Float32(45.5f), CanProtocol.Int16(120), CanProtocol.Int16(250));
                var stateFrame = CanProtocol.BuildCommand(1, 0x01, statePayload, false);
                var motor = CanProtocol.TryDecodeMotorState(stateFrame);
                if (!motor.HasValue || Math.Abs(motor.Value.Angle - 45.5) > 0.01) throw new InvalidOperationException("Motor state decode failed.");
                log.Add("motor-state-decode-ok");

                var motorAngle = Kinematics.LinkageToMotor(60);
                var linkageAngle = Kinematics.MotorToLinkage(motorAngle);
                if (Math.Abs(linkageAngle - 60) > 1.0) throw new InvalidOperationException("Kinematics conversion failed.");
                log.Add("kinematics-ok");

                var modelPath = Path.Combine(records, "selftest_svm.model");
                var labels = new[] { -1.0, -1.0, 1.0, 1.0 };
                var features = new[] { 0.0, 0.0, 0.2, 0.1, 2.0, 2.0, 2.2, 1.8 };
                var model = NativeCore.clinic_svm_train(labels, features, 4, 2, modelPath);
                if (model == IntPtr.Zero || !File.Exists(modelPath)) throw new InvalidOperationException("libsvm training failed.");
                double predicted;
                double scoreValue;
                if (NativeCore.clinic_svm_predict(model, new[] { 2.1, 2.0 }, 2, out predicted, out scoreValue) != 1) throw new InvalidOperationException("libsvm prediction failed.");
                NativeCore.clinic_svm_free(model);
                if (predicted != 1.0) throw new InvalidOperationException("libsvm trained model predicted the wrong class.");
                var loaded = NativeCore.clinic_svm_load(modelPath);
                if (loaded == IntPtr.Zero) throw new InvalidOperationException("libsvm model load failed.");
                if (NativeCore.clinic_svm_predict(loaded, new[] { 0.1, 0.1 }, 2, out predicted, out scoreValue) != 1) throw new InvalidOperationException("loaded libsvm prediction failed.");
                NativeCore.clinic_svm_free(loaded);
                if (predicted != -1.0) throw new InvalidOperationException("loaded libsvm model predicted the wrong class.");
                log.Add("libsvm-train-predict-ok");

                var detectorModelPath = Path.Combine(records, "selftest_detector_svm.model");
                var detector = new SvmPainDetector();
                if (!detector.Train(labels, features, 4, 2, detectorModelPath)) throw new InvalidOperationException("SvmPainDetector native training failed.");
                var detectorScore = detector.Score(parsed, 2000);
                detector.Dispose();
                if (detectorScore != -1.0 && detectorScore != 1.0) throw new InvalidOperationException("SvmPainDetector native prediction failed.");
                log.Add("native-detector-ok");

                WriteSelfTestLog("PASS", log);
                return 0;
            }
            catch (Exception ex)
            {
                log.Add("FAIL: " + ex);
                WriteSelfTestLog("FAIL", log);
                return 1;
            }
        }

        private static void WriteSelfTestLog(string status, IEnumerable<string> lines)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "selftest.log");
            var content = status + Environment.NewLine + string.Join(Environment.NewLine, lines.ToArray());
            File.WriteAllText(path, content, Encoding.UTF8);
        }

        private static double[] ExtractFeatures(EmgWindow window, int sampleRate, out int featureCount)
        {
            featureCount = 0;
            var channelCount = window.Samples.Max(x => x.Length);
            var flat = new List<double>();
            foreach (var row in window.Samples)
            {
                for (var i = 0; i < channelCount; i++) flat.Add(i < row.Length ? row[i] : 0);
            }
            var features = new double[32];
            if (NativeCore.clinic_emg_extract_features(flat.ToArray(), window.Samples.Count, channelCount, sampleRate, features, features.Length, out featureCount) != 1)
            {
                featureCount = 0;
            }
            return features;
        }
    }

    internal struct TelemetrySample
    {
        public DateTime Time;
        public bool HasMotorState;
        public double Angle;
        public double Speed;
        public double Current;
        public double Emg;
        public double Pressure;
        public double SvmScore;
    }

    internal struct NavItem
    {
        public readonly string Key;
        public readonly string Text;
        public NavItem(string key, string text)
        {
            Key = key;
            Text = text;
        }
    }

    internal static class EnumerableCompat
    {
        public static IEnumerable<T> TakeLastCompat<T>(this IEnumerable<T> source, int count)
        {
            var queue = new Queue<T>();
            foreach (var item in source)
            {
                queue.Enqueue(item);
                while (queue.Count > count) queue.Dequeue();
            }
            return queue.ToArray();
        }
    }
}
