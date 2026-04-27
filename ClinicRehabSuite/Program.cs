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
        private readonly List<TelemetrySample> telemetry = new List<TelemetrySample>();

        private Panel header;
        private Panel content;
        private Label titleLabel;
        private Label statusLabel;
        private Label connectionLabel;
        private Label sessionStateLabel;
        private Label alertLabel;
        private ComboBox motorPortBox;
        private ComboBox stretchPortBox;
        private ComboBox trainModeBox;
        private ComboBox speedLevelBox;
        private ComboBox againstPowerBox;
        private NumericUpDown startAngleBox;
        private NumericUpDown endAngleBox;
        private NumericUpDown repeatBox;
        private NumericUpDown keepTimeBox;
        private NumericUpDown restTimeBox;
        private NumericUpDown tractionForceBox;
        private NumericUpDown tractionTimeBox;
        private NumericUpDown measuredAngleBox;
        private ProgressBar progressBar;
        private Label angleMetricLabel;
        private Label pressureMetricLabel;
        private Label svmMetricLabel;
        private Label machineMaxLabel;
        private Label stretchStateLabel;
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
        private static readonly Color AppBackground = Color.FromArgb(242, 246, 249);
        private static readonly Color SurfaceColor = Color.White;
        private static readonly Color BorderColor = Color.FromArgb(214, 221, 230);
        private static readonly Color TextColor = Color.FromArgb(25, 33, 46);
        private static readonly Color MutedTextColor = Color.FromArgb(91, 103, 120);
        private static readonly Color PrimaryColor = Color.FromArgb(12, 111, 133);
        private static readonly Color PrimaryHoverColor = Color.FromArgb(8, 91, 112);
        private static readonly Color SecondaryColor = Color.FromArgb(67, 81, 101);
        private static readonly Color DangerColor = Color.FromArgb(176, 42, 55);
        private static readonly Color DangerHoverColor = Color.FromArgb(143, 31, 43);

        public MainForm()
        {
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            Text = "Clinic Rehab Suite";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1514, 820);
            Size = new Size(1680, 940);
            WindowState = FormWindowState.Maximized;
            BackColor = AppBackground;

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
            header = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = SurfaceColor };
            content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = AppBackground };

            Controls.Add(content);
            Controls.Add(header);
            FormClosed += delegate
            {
                painDetector.Dispose();
                deviceController.Dispose();
            };

            titleLabel = new Label
            {
                Text = "Clinic Rehab Suite",
                Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold),
                ForeColor = TextColor,
                Location = new Point(24, 18),
                AutoSize = true
            };
            header.Controls.Add(titleLabel);

            connectionLabel = new Label
            {
                Text = "设备：未连接",
                ForeColor = MutedTextColor,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(700, 28)
            };
            header.Controls.Add(connectionLabel);

            statusLabel = new Label
            {
                Text = "就绪",
                ForeColor = Color.FromArgb(18, 122, 76),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(930, 28)
            };
            header.Controls.Add(statusLabel);
            header.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = BorderColor });

            Resize += delegate { LayoutHeaderStatus(); };
            LayoutHeaderStatus();
        }

        private void LayoutHeaderStatus()
        {
            if (connectionLabel == null || statusLabel == null) return;
            statusLabel.Location = new Point(Math.Max(560, header.Width - 180), 28);
            connectionLabel.Location = new Point(Math.Max(360, header.Width - 430), 28);
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
        }

        private void ShowDashboard()
        {
            SetPage("Dashboard", "康复训练控制台");
            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            content.Controls.Add(grid);

            grid.Controls.Add(BuildMatlabControlColumn(), 0, 0);
            grid.Controls.Add(BuildMotorDataPanel(), 1, 0);
            grid.Controls.Add(BuildSignalTabs(), 2, 0);
        }

        private Control BuildMatlabControlColumn()
        {
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 82));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 18));
            panel.Controls.Add(BuildControlTabs(), 0, 0);
            panel.Controls.Add(BuildBottomStatusPanel(), 0, 1);
            return panel;
        }

        private Control BuildControlTabs()
        {
            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(TabPageWith("设备控制", BuildDeviceControlPage()));
            tabs.TabPages.Add(TabPageWith("OpenSignal", BuildOpenSignalPage()));
            tabs.TabPages.Add(TabPageWith("推杆控制", BuildStretchControlPage()));
            return tabs;
        }

        private Control BuildDeviceControlPage()
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, ColumnCount = 4, RowCount = 14, Padding = new Padding(10) };
            for (int i = 0; i < 4; i++) layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            for (int i = 0; i < 14; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 0 || i == 4 || i == 8 ? 34 : 48));

            layout.Controls.Add(SectionTitle("设备操作"), 0, 0);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 0), 4);
            motorPortBox = Combo("COM4", SerialPort.GetPortNames().Concat(new[] { "COM4" }).Distinct().ToArray());
            stretchPortBox = Combo("COM10", SerialPort.GetPortNames().Concat(new[] { "COM10" }).Distinct().ToArray());
            layout.Controls.Add(FieldBlock("大然电机 CAN", motorPortBox), 0, 1);
            layout.Controls.Add(FieldBlock("推杆/气囊 Arduino", stretchPortBox), 1, 1);
            layout.Controls.Add(FieldBlock("OpenSignal TCP", LabelValue("127.0.0.1:5555")), 2, 1);
            layout.Controls.Add(ActionButton("启动设备", ConnectDevices), 0, 2);
            layout.Controls.Add(ActionButton("关闭设备", DisconnectDevices), 1, 2);
            layout.Controls.Add(ActionButton("设备自检", DeviceSelfTest), 2, 2);
            layout.Controls.Add(ActionButton("结束/急停", EmergencyStop), 3, 2);

            layout.Controls.Add(SectionTitle("标定"), 0, 4);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 4), 4);
            measuredAngleBox = Numeric(75, 5, 120);
            machineMaxLabel = LabelValue("最大角度：未测量");
            layout.Controls.Add(FieldBlock("起始标定角", Numeric(30, 5, 120)), 0, 5);
            layout.Controls.Add(FieldBlock("医生量角结果", measuredAngleBox), 1, 5);
            layout.Controls.Add(FieldBlock("设备活动范围", machineMaxLabel), 2, 5);
            layout.SetColumnSpan(layout.GetControlFromPosition(2, 5), 2);
            layout.Controls.Add(ActionButton("训练前最大角", MeasurePreAngle), 0, 6);
            layout.Controls.Add(ActionButton("三分疼痛角", CalibratePainAngle), 1, 6);
            layout.Controls.Add(ActionButton("训练后最大角", MeasurePostAngle), 2, 6);
            layout.Controls.Add(ActionButton("生成 SVM 模型", TrainDetectorFromCalibration), 3, 6);

            layout.Controls.Add(SectionTitle("康复训练"), 0, 8);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 8), 4);
            trainModeBox = Combo("被动训练", new[] { "被动训练", "对抗训练", "自适应被动训练", "自适应对抗训练", "静态牵引", "自适应充气" });
            speedLevelBox = Combo("适中", new[] { "缓慢", "适中", "快速" });
            againstPowerBox = Combo("适中", new[] { "较小", "适中", "较大" });
            startAngleBox = Numeric(30, 0, 120);
            endAngleBox = Numeric(60, 5, 120);
            repeatBox = Numeric(1, 1, 30);
            keepTimeBox = Numeric(10, 1, 60);
            restTimeBox = Numeric(10, 1, 60);
            tractionForceBox = Numeric(10, 1, 50);
            tractionTimeBox = Numeric(10, 1, 60);

            layout.Controls.Add(FieldBlock("训练模式", trainModeBox), 0, 9);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 9), 2);
            layout.Controls.Add(FieldBlock("速度", speedLevelBox), 2, 9);
            layout.Controls.Add(FieldBlock("对抗力量", againstPowerBox), 3, 9);
            layout.Controls.Add(FieldBlock("次数", repeatBox), 0, 10);
            layout.Controls.Add(FieldBlock("起始角度", startAngleBox), 1, 10);
            layout.Controls.Add(FieldBlock("终止角度", endAngleBox), 2, 10);
            layout.Controls.Add(FieldBlock("保持/对抗时间", keepTimeBox), 3, 10);
            layout.Controls.Add(FieldBlock("休息时间", restTimeBox), 0, 11);
            layout.Controls.Add(FieldBlock("牵引力", tractionForceBox), 1, 11);
            layout.Controls.Add(FieldBlock("牵引时间", tractionTimeBox), 2, 11);
            progressBar = new ProgressBar { Dock = DockStyle.Fill, Minimum = 0, Maximum = 100 };
            layout.Controls.Add(FieldBlock("训练进度", progressBar), 3, 11);
            layout.Controls.Add(ActionButton("开始训练", StartTraining), 0, 12);
            layout.Controls.Add(ActionButton("应用处方", ApplyPrescription), 1, 12);
            layout.Controls.Add(ActionButton("导出记录", ExportRecord), 2, 12);
            layout.Controls.Add(ActionButton("停止", EmergencyStop), 3, 12);
            return layout;
        }

        private Control BuildOpenSignalPage()
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 8, Padding = new Padding(12) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 32));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 32));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1));
            layout.Controls.Add(SectionTitle("OpenSignal 操作"), 0, 0);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 0), 3);

            var idLabel = LabelValue("设备 ID：未读取");
            layout.Controls.Add(ActionButton("devices", delegate { SendOpenSignalCommand("devices", idLabel, null, null); }), 0, 1);
            layout.Controls.Add(ActionButton("config", delegate { SendOpenSignalCommand("config", idLabel, null, null); }), 1, 1);
            layout.Controls.Add(ActionButton("start", delegate { SendOpenSignalCommand("start", idLabel, null, null); }), 2, 1);
            layout.Controls.Add(ActionButton("stop", delegate { SendOpenSignalCommand("stop", idLabel, null, null); }), 0, 2);
            layout.Controls.Add(idLabel, 1, 2);
            layout.SetColumnSpan(idLabel, 2);

            var sendBox = MultilineBox();
            var receiveBox = MultilineBox();
            var configBox = MultilineBox();
            layout.Controls.Add(FieldBlock("发送", sendBox), 0, 3);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 3), 3);
            layout.Controls.Add(FieldBlock("接收", receiveBox), 0, 4);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 4), 3);
            layout.Controls.Add(FieldBlock("配置", configBox), 0, 5);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 5), 3);

            foreach (Control button in layout.Controls)
            {
                if (button is Button)
                {
                    button.Click += delegate
                    {
                        sendBox.Text = statusLabel.Text;
                        receiveBox.Text = deviceController.LastMessage ?? "";
                        var config = OpenSignalParser.ParseConfig(deviceController.LastOpenSignalResponse);
                        if (config != null)
                        {
                            idLabel.Text = "设备 ID：" + config.DeviceId;
                            configBox.Text = "activeChannels: " + string.Join(",", config.ActiveChannels.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray()) + Environment.NewLine +
                                "samplingFreq: " + config.SamplingFrequency.ToString(CultureInfo.InvariantCulture);
                        }
                    };
                }
            }
            return layout;
        }

        private Control BuildStretchControlPage()
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, ColumnCount = 4, RowCount = 8, Padding = new Padding(12) };
            for (int i = 0; i < 4; i++) layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            for (int i = 0; i < 8; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 0 || i == 3 || i == 5 ? 38 : 50));
            layout.Controls.Add(SectionTitle("推杆"), 0, 0);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 0), 4);
            layout.Controls.Add(ActionButton("推杆归零", delegate { SendStretch("1", "推杆归零"); }), 0, 1);
            layout.Controls.Add(ActionButton("一次拉伸", delegate { SendStretch("2", "一次拉伸"); }), 1, 1);
            layout.Controls.Add(ActionButton("推杆伸出", delegate { SendStretch("3", "推杆伸出"); }), 2, 1);
            layout.Controls.Add(ActionButton("推杆回缩", delegate { SendStretch("4", "推杆回缩"); }), 3, 1);
            layout.Controls.Add(ActionButton("开启反馈", delegate { SendStretch("o", "开启反馈"); }), 0, 2);
            layout.Controls.Add(ActionButton("停止反馈", delegate { SendStretch("c", "停止反馈"); }), 1, 2);
            layout.Controls.Add(ActionButton("静态牵引", delegate { RunStretchTraining(TrainingMode.StaticTraction); }), 2, 2);
            layout.Controls.Add(ActionButton("自适应充气", delegate { RunStretchTraining(TrainingMode.AdaptiveInflation); }), 3, 2);

            layout.Controls.Add(SectionTitle("大腿气囊"), 0, 3);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 3), 4);
            layout.Controls.Add(ActionButton("大腿充气", delegate { SendStretch("5", "大腿充气"); }), 0, 4);
            layout.Controls.Add(ActionButton("大腿保持", delegate { SendStretch("6", "大腿保持"); }), 1, 4);
            layout.Controls.Add(ActionButton("大腿放气", delegate { SendStretch("7", "大腿放气"); }), 2, 4);

            layout.Controls.Add(SectionTitle("小腿气囊"), 0, 5);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 5), 4);
            layout.Controls.Add(ActionButton("小腿充气", delegate { SendStretch("8", "小腿充气"); }), 0, 6);
            layout.Controls.Add(ActionButton("小腿保持", delegate { SendStretch("9", "小腿保持"); }), 1, 6);
            layout.Controls.Add(ActionButton("小腿放气", delegate { SendStretch("q", "小腿放气"); }), 2, 6);
            stretchStateLabel = LabelValue("推杆状态：待反馈");
            layout.Controls.Add(stretchStateLabel, 0, 7);
            layout.SetColumnSpan(stretchStateLabel, 4);
            return layout;
        }

        private Control BuildMotorDataPanel()
        {
            var panel = CardPanel();
            panel.Dock = DockStyle.Fill;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(12) };
            panel.Controls.Add(layout);
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

            angleMetricLabel = MetricValue("0 deg");
            layout.Controls.Add(FieldBlock("关节角度", angleMetricLabel), 0, 0);
            motorWave = new WavePanel { Dock = DockStyle.Fill, Title = "电机角度" };
            layout.Controls.Add(motorWave, 0, 1);
            layout.Controls.Add(new WavePanel { Dock = DockStyle.Fill, Title = "速度" }, 0, 2);
            layout.Controls.Add(new WavePanel { Dock = DockStyle.Fill, Title = "力矩/电流" }, 0, 3);
            sessionStateLabel = MetricValue("就绪");
            layout.Controls.Add(FieldBlock("训练状态", sessionStateLabel), 0, 4);
            return panel;
        }

        private Control BuildSignalTabs()
        {
            var tabs = new TabControl { Dock = DockStyle.Fill };
            var emgPage = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, Padding = new Padding(10) };
            emgPage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            emgPage.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            emgPage.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            emgPage.RowStyles.Add(new RowStyle(SizeType.Percent, 36));
            emgPage.RowStyles.Add(new RowStyle(SizeType.Percent, 36));
            emgPage.RowStyles.Add(new RowStyle(SizeType.Percent, 28));
            svmMetricLabel = MetricValue("待采集");
            emgPage.Controls.Add(FieldBlock("SVM 输出", svmMetricLabel), 0, 0);
            emgPage.SetColumnSpan(emgPage.GetControlFromPosition(0, 0), 2);
            emgWave = new WavePanel { Dock = DockStyle.Fill, Title = "EMG 通道" };
            emgPage.Controls.Add(emgWave, 0, 1);
            emgPage.SetColumnSpan(emgWave, 2);
            emgPage.Controls.Add(new WavePanel { Dock = DockStyle.Fill, Title = "Trigger" }, 0, 2);
            emgPage.Controls.Add(new WavePanel { Dock = DockStyle.Fill, Title = "State" }, 1, 2);
            emgPage.Controls.Add(OperationPanel(), 0, 3);
            emgPage.SetColumnSpan(emgPage.GetControlFromPosition(0, 3), 2);

            var stretchPage = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(10) };
            stretchPage.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            stretchPage.RowStyles.Add(new RowStyle(SizeType.Percent, 36));
            stretchPage.RowStyles.Add(new RowStyle(SizeType.Percent, 36));
            stretchPage.RowStyles.Add(new RowStyle(SizeType.Percent, 28));
            pressureMetricLabel = MetricValue("0 N");
            stretchPage.Controls.Add(FieldBlock("牵伸压力", pressureMetricLabel), 0, 0);
            pressureWave = new WavePanel { Dock = DockStyle.Fill, Title = "推杆位置 / 压力" };
            stretchPage.Controls.Add(pressureWave, 0, 1);
            stretchPage.Controls.Add(new WavePanel { Dock = DockStyle.Fill, Title = "压力热区" }, 0, 2);
            stretchPage.Controls.Add(AlertPanel(), 0, 3);

            tabs.TabPages.Add(TabPageWith("肌电数据", emgPage));
            tabs.TabPages.Add(TabPageWith("推杆数据", stretchPage));
            tabs.TabPages.Add(TabPageWith("记录", BuildRecordsListPanel()));
            return tabs;
        }

        private Control BuildBottomStatusPanel()
        {
            var panel = CardPanel();
            panel.Dock = DockStyle.Fill;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2, Padding = new Padding(12) };
            panel.Controls.Add(layout);
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 52));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 48));
            layout.Controls.Add(LabelValue("训练开始前请完成患者信息登记、设备启动和活动范围标定。"), 0, 0);
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 0), 3);
            layout.Controls.Add(ActionButton("患者信息登记", delegate { ShowPatient(); }), 0, 1);
            layout.Controls.Add(ActionButton("保存当前记录", ExportRecord), 1, 1);
            layout.Controls.Add(ActionButton("设备停止", EmergencyStop), 2, 1);
            return panel;
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
                Text = "当前无未处理告警。硬件命令只会发送到已连接端口；未连接时会明确报错。",
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
            layout.Controls.Add(FieldBlock("大然电机 CAN", motorPortBox), 0, 1);
            layout.Controls.Add(FieldBlock("推杆/气囊 Arduino", stretchPortBox), 1, 1);
            layout.Controls.Add(FieldBlock("Opensignal TCP", LabelValue("127.0.0.1:5555")), 2, 1);
            layout.Controls.Add(ActionButton("连接设备", ConnectDevices), 0, 3);
            layout.Controls.Add(ActionButton("断开设备", DisconnectDevices), 1, 3);
            layout.Controls.Add(ActionButton("设备自检", DeviceSelfTest), 2, 3);
            layout.Controls.Add(LabelValue("所有通道按真实硬件连接。任一通道连接失败都会报错并停止连接，不再自动降级。"), 0, 5);
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
            layout.Controls.Add(LabelValue("标定会复用实时电机角度、OpenSignal 窗口和原生 SVM 训练接口；没有真实采样时不会生成生产模型。"), 0, 5);
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

            trainModeBox = Combo("被动训练", new[] { "被动训练", "对抗训练", "自适应被动训练", "自适应对抗训练", "静态牵引", "自适应充气" });
            speedLevelBox = Combo("适中", new[] { "缓慢", "适中", "快速" });
            againstPowerBox = Combo("适中", new[] { "较小", "适中", "较大" });
            startAngleBox = Numeric(30, 0, 120);
            endAngleBox = Numeric(60, 5, 120);
            repeatBox = Numeric(1, 1, 30);
            keepTimeBox = Numeric(10, 1, 60);
            restTimeBox = Numeric(10, 1, 60);
            tractionForceBox = Numeric(10, 1, 50);
            tractionTimeBox = Numeric(10, 1, 60);

            var modeField = FieldBlock("训练模式", trainModeBox);
            layout.Controls.Add(modeField, 0, 1);
            layout.SetColumnSpan(modeField, 2);
            layout.Controls.Add(FieldBlock("速度", speedLevelBox), 2, 1);
            layout.Controls.Add(FieldBlock("对抗力量", againstPowerBox), 3, 1);
            layout.Controls.Add(FieldBlock("起始角度", startAngleBox), 0, 3);
            layout.Controls.Add(FieldBlock("终止角度", endAngleBox), 1, 3);
            layout.Controls.Add(FieldBlock("训练次数", repeatBox), 2, 3);
            layout.Controls.Add(FieldBlock("保持时间", keepTimeBox), 3, 3);
            layout.Controls.Add(FieldBlock("休息时间", restTimeBox), 0, 4);
            layout.Controls.Add(FieldBlock("牵伸力", tractionForceBox), 1, 4);
            layout.Controls.Add(FieldBlock("牵引时间", tractionTimeBox), 2, 4);
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
            layout.Controls.Add(ActionButton("一次拉伸", delegate { SendStretch("2", "一次拉伸"); }), 1, 1);
            layout.Controls.Add(ActionButton("推杆伸出", delegate { SendStretch("3", "推杆伸出"); }), 2, 1);
            layout.Controls.Add(ActionButton("推杆回缩", delegate { SendStretch("4", "推杆回缩"); }), 3, 1);
            layout.Controls.Add(ActionButton("开启反馈", delegate { SendStretch("o", "开启反馈"); }), 0, 2);
            layout.Controls.Add(ActionButton("停止反馈", delegate { SendStretch("c", "停止反馈"); }), 1, 2);
            layout.Controls.Add(ActionButton("大腿充气", delegate { SendStretch("5", "大腿充气"); }), 0, 3);
            layout.Controls.Add(ActionButton("大腿保持", delegate { SendStretch("6", "大腿保持"); }), 1, 3);
            layout.Controls.Add(ActionButton("大腿放气", delegate { SendStretch("7", "大腿放气"); }), 2, 3);
            layout.Controls.Add(ActionButton("小腿充气", delegate { SendStretch("8", "小腿充气"); }), 0, 4);
            layout.Controls.Add(ActionButton("小腿保持", delegate { SendStretch("9", "小腿保持"); }), 1, 4);
            layout.Controls.Add(ActionButton("小腿放气", delegate { SendStretch("q", "小腿放气"); }), 2, 4);
        }

        private void ShowRecords()
        {
            SetPage("Records", "记录与导出");
            var panel = BuildRecordsListPanel();
            content.Controls.Add(panel);
        }

        private Control BuildRecordsListPanel()
        {
            var panel = CardPanel();
            panel.Dock = DockStyle.Fill;
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
            return panel;
        }

        private void ConnectDevices(object sender, EventArgs e)
        {
            var motorPort = motorPortBox == null ? "COM4" : motorPortBox.Text;
            var stretchPort = stretchPortBox == null ? "COM10" : stretchPortBox.Text;
            try
            {
                deviceController.Connect(motorPort, stretchPort, "127.0.0.1", 5555);
                connectionLabel.Text = "设备：已连接";
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
                alertLabelSafe("没有可用角度数据，请先连接设备并采集电机反馈。");
                return;
            }
            if (endAngleBox != null) endAngleBox.Value = ClampDecimal((decimal)Math.Round(calibration.LinkageAngle), endAngleBox.Minimum, endAngleBox.Maximum);
            AddLog("训练前最大角：" + calibration.LinkageAngle.ToString("F1", CultureInfo.InvariantCulture) + " deg");
            alertLabelSafe("训练前最大角已记录：" + calibration.LinkageAngle.ToString("F1", CultureInfo.InvariantCulture) + " deg");
            if (machineMaxLabel != null) machineMaxLabel.Text = "训练前最大角：" + calibration.LinkageAngle.ToString("F1", CultureInfo.InvariantCulture) + " deg";
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
            if (machineMaxLabel != null) machineMaxLabel.Text = "三分疼痛角：" + calibration.LinkageAngle.ToString("F1", CultureInfo.InvariantCulture) + " deg";
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
            if (machineMaxLabel != null) machineMaxLabel.Text = "训练后最大角：" + calibration.LinkageAngle.ToString("F1", CultureInfo.InvariantCulture) + " deg";
        }

        private void TrainDetectorFromCalibration(object sender, EventArgs e)
        {
            var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models");
            Directory.CreateDirectory(modelPath);
            modelPath = Path.Combine(modelPath, "pain_svm.model");
            var trainingSet = BuildSvmTrainingSet(telemetry);
            if (trainingSet == null)
            {
                alertLabelSafe("SVM 模型未生成：需要真实 OpenSignal 采样，并且记录中要同时包含疼痛/非疼痛标签。");
                return;
            }
            if (!painDetector.Train(trainingSet.Labels, trainingSet.Features, trainingSet.SampleCount, trainingSet.FeatureCount, modelPath))
            {
                alertLabelSafe("SVM 模型训练失败，请检查 ClinicCore.dll。");
                return;
            }
            AddLog("SVM 模型已生成：" + modelPath + "，样本数：" + trainingSet.SampleCount.ToString(CultureInfo.InvariantCulture));
            alertLabelSafe("SVM 模型已由真实采样生成。");
        }

        private void SendOpenSignalCommand(string command, Label idLabel, TextBox sendBox, TextBox receiveBox)
        {
            try
            {
                var response = deviceController.SendOpenSignalCommand(command);
                statusLabel.Text = "OpenSignal: " + command;
                if (sendBox != null) sendBox.Text = command;
                if (receiveBox != null) receiveBox.Text = response;
                if (idLabel != null)
                {
                    var config = OpenSignalParser.ParseConfig(response);
                    if (config != null && !string.IsNullOrEmpty(config.DeviceId))
                    {
                        idLabel.Text = "设备 ID：" + config.DeviceId;
                    }
                }
                AddLog("OpenSignal 指令：" + command);
            }
            catch (Exception ex)
            {
                alertLabelSafe("OpenSignal 指令失败：" + ex.Message);
                AddLog("OpenSignal 指令失败：" + ex.Message);
            }
        }

        private void ApplyPrescription(object sender, EventArgs e)
        {
            activePrescription = ReadPrescription();
            statusLabel.Text = "处方已应用";
            AddLog("处方参数已更新：" + activePrescription.Mode);
        }

        private void StartTraining(object sender, EventArgs e)
        {
            try
            {
                activePrescription = ReadPrescription();
                if (activePrescription.Mode == TrainingMode.StaticTraction || activePrescription.Mode == TrainingMode.AdaptiveInflation)
                {
                    activeSession = null;
                    training = false;
                }
                else
                {
                    activeSession = new TrainingSession(activePrescription);
                    training = true;
                    trainingStart = DateTime.Now;
                    trainingSeconds = Math.Max(1, (int)Math.Ceiling(activeSession.TotalSeconds));
                }
                deviceController.StartTraining(activePrescription);
                statusLabel.Text = "训练中";
                sessionStateLabelSafe("训练中");
                AddLog("训练开始：" + activePrescription.Mode);
            }
            catch (Exception ex)
            {
                training = false;
                statusLabel.Text = "训练启动失败";
                alertLabelSafe(ex.Message);
                AddLog("训练启动失败：" + ex.Message);
            }
        }

        private void EmergencyStop(object sender, EventArgs e)
        {
            training = false;
            if (activeSession != null) activeSession.Stop();
            statusLabel.Text = "急停";
            sessionStateLabelSafe("已急停");
            try
            {
                deviceController.EmergencyStop();
                alertLabelSafe("急停已触发，运动命令停止。");
                AddLog("急停触发。");
            }
            catch (Exception ex)
            {
                alertLabelSafe("急停命令未发送：" + ex.Message);
                AddLog("急停命令未发送：" + ex.Message);
            }
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
            try
            {
                deviceController.SendStretchCommand(command);
                statusLabel.Text = name;
                if (stretchStateLabel != null) stretchStateLabel.Text = "推杆状态：" + name + " [" + command + "]";
                AddLog("牵伸控制：" + name + " [" + command + "]");
            }
            catch (Exception ex)
            {
                alertLabelSafe("牵伸命令未发送：" + ex.Message);
                AddLog("牵伸命令未发送：" + ex.Message);
            }
        }

        private void RunStretchTraining(TrainingMode mode)
        {
            try
            {
                var prescription = ReadPrescription();
                prescription.Mode = mode;
                deviceController.StartTraining(prescription);
                statusLabel.Text = mode == TrainingMode.StaticTraction ? "静态牵引" : "自适应充气";
                if (stretchStateLabel != null) stretchStateLabel.Text = "推杆状态：" + statusLabel.Text;
                AddLog("牵伸训练：" + prescription.Mode);
            }
            catch (Exception ex)
            {
                alertLabelSafe("牵伸训练未启动：" + ex.Message);
                AddLog("牵伸训练未启动：" + ex.Message);
            }
        }

        private SvmTrainingSet BuildSvmTrainingSet(IList<TelemetrySample> samples)
        {
            if (samples == null || samples.Count < 40) return null;
            const int windowLength = 20;
            const int stepLength = 10;
            var labels = new List<double>();
            var featureRows = new List<double[]>();
            for (var start = 0; start + windowLength <= samples.Count; start += stepLength)
            {
                var window = samples.Skip(start).Take(windowLength).ToArray();
                var label = window.Max(x => x.PainLabel);
                if (label != 0 && label != 1) continue;
                if (window.All(x => Math.Abs(x.Emg) < 1.0e-12)) continue;
                var emg = new EmgWindow();
                foreach (var sample in window)
                {
                    emg.Samples.Add(new[] { sample.Emg, sample.Emg, sample.Emg, sample.Emg });
                }
                int featureCount;
                var features = SvmPainDetector.ExtractFeatures(emg, 2000, out featureCount);
                if (features == null || featureCount <= 0) continue;
                labels.Add(label == 1 ? 1.0 : -1.0);
                featureRows.Add(features.Take(featureCount).ToArray());
            }
            if (labels.Distinct().Count() < 2 || featureRows.Count == 0) return null;
            var width = featureRows.Min(x => x.Length);
            var flat = new double[featureRows.Count * width];
            for (var row = 0; row < featureRows.Count; row++)
            {
                Array.Copy(featureRows[row], 0, flat, row * width, width);
            }
            return new SvmTrainingSet
            {
                Labels = labels.ToArray(),
                Features = flat,
                SampleCount = featureRows.Count,
                FeatureCount = width
            };
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
                SvmScore = svmScore,
                PainLabel = step == null ? 0 : step.PainLabel
            };
            telemetry.Add(sample);
            if (telemetry.Count > 2000) telemetry.RemoveRange(0, telemetry.Count - 2000);
            if (activeSession != null)
            {
                activeSession.Observe(sample);
                activeSession.CompleteCycleIfNeeded(step);
            }
            if (angleMetricLabel != null) angleMetricLabel.Text = sample.Angle.ToString("F1", CultureInfo.InvariantCulture) + " deg";
            if (pressureMetricLabel != null) pressureMetricLabel.Text = sample.Pressure.ToString("F1", CultureInfo.InvariantCulture) + " N";
            if (svmMetricLabel != null) svmMetricLabel.Text = emgWindow == null ? "待采集" : svmScore.ToString("F2", CultureInfo.InvariantCulture);
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
            var speed = speedLevelBox == null ? "适中" : speedLevelBox.Text;
            var againstPower = againstPowerBox == null ? "适中" : againstPowerBox.Text;
            return new TrainingPrescription
            {
                Mode = TrainingModeCatalog.FromDisplayName(mode),
                StartAngle = startAngleBox == null ? 30 : (double)startAngleBox.Value,
                EndAngle = endAngleBox == null ? 60 : (double)endAngleBox.Value,
                Repetitions = repeatBox == null ? 1 : (int)repeatBox.Value,
                KeepSeconds = keepTimeBox == null ? 10 : (double)keepTimeBox.Value,
                RestSeconds = restTimeBox == null ? 10 : (double)restTimeBox.Value,
                TractionForce = tractionForceBox == null ? 10 : (double)tractionForceBox.Value,
                TravelSeconds = TravelSecondsFromSpeed(speed),
                AgainstPower = AgainstPowerFromLevel(againstPower),
                TractionSeconds = tractionTimeBox == null ? 10 : (double)tractionTimeBox.Value
            };
        }

        private static double TravelSecondsFromSpeed(string speedLevel)
        {
            if (speedLevel != null && speedLevel.Contains("缓慢")) return 20;
            if (speedLevel != null && speedLevel.Contains("快速")) return 10;
            return 15;
        }

        private static double AgainstPowerFromLevel(string level)
        {
            if (level != null && level.Contains("较小")) return 1.0;
            if (level != null && level.Contains("较大")) return 2.0;
            return 1.5;
        }

        private Panel CardPanel()
        {
            var panel = new Panel
            {
                BackColor = SurfaceColor,
                Margin = new Padding(8),
                Padding = new Padding(1)
            };
            panel.Paint += delegate(object sender, PaintEventArgs e)
            {
                var bounds = ((Control)sender).ClientRectangle;
                bounds.Width -= 1;
                bounds.Height -= 1;
                using (var pen = new Pen(BorderColor))
                {
                    e.Graphics.DrawRectangle(pen, bounds);
                }
            };
            return panel;
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
                ForeColor = TextColor,
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
                BackColor = IsDestructiveAction(text) ? DangerColor : PrimaryColor,
                ForeColor = Color.White,
                Margin = new Padding(5),
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = IsDestructiveAction(text) ? DangerHoverColor : PrimaryHoverColor;
            button.FlatAppearance.MouseDownBackColor = SecondaryColor;
            button.Click += handler;
            return button;
        }

        private static bool IsDestructiveAction(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            return text.Contains("急停") || text.Contains("停止") || text.Contains("关闭") || text.Contains("结束");
        }

        private ComboBox Combo(string value, string[] items)
        {
            var box = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDown,
                IntegralHeight = true,
                FlatStyle = FlatStyle.Flat,
                BackColor = SurfaceColor,
                ForeColor = TextColor
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
                DecimalPlaces = 0,
                BackColor = SurfaceColor,
                ForeColor = TextColor
            };
        }

        private Control FieldBlock(string label, Control input)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4) };
            var labelControl = new Label { Text = label, Dock = DockStyle.Top, Height = 20, ForeColor = MutedTextColor };
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
                ForeColor = TextColor
            };
        }

        private Label MetricValue(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = TextColor
            };
        }

        private TabPage TabPageWith(string title, Control contentControl)
        {
            var page = new TabPage(title) { BackColor = AppBackground, Padding = new Padding(6) };
            contentControl.Dock = DockStyle.Fill;
            page.Controls.Add(contentControl);
            return page;
        }

        private TextBox MultilineBox()
        {
            return new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = SurfaceColor,
                ForeColor = TextColor
            };
        }

        private void AddFormField(TableLayoutPanel layout, string label, string value, int column, int row)
        {
            layout.Controls.Add(FieldBlock(label, new TextBox { Text = value, BorderStyle = BorderStyle.FixedSingle, BackColor = SurfaceColor, ForeColor = TextColor }), column, row);
        }

        private void AddLog(string message)
        {
            if (logPanel == null) return;
            logPanel.Controls.Add(new Label
            {
                Text = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture) + "  " + message,
                AutoSize = true,
                ForeColor = TextColor,
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
        private static readonly Color SurfaceColor = Color.White;
        private static readonly Color BorderColor = Color.FromArgb(214, 221, 230);
        private static readonly Color TextColor = Color.FromArgb(25, 33, 46);
        private static readonly Color PrimaryColor = Color.FromArgb(12, 111, 133);
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
            e.Graphics.Clear(SurfaceColor);
            using (var titleBrush = new SolidBrush(TextColor))
            using (var gridPen = new Pen(BorderColor))
            using (var linePen = new Pen(PrimaryColor, 2F))
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

        public string LastMessage { get; private set; }
        public string LastOpenSignalResponse { get; private set; }

        public void Connect(string motorPortName, string stretchPortName, string host, int port)
        {
            Dispose();
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
                LastMessage = "Real hardware connection failed: " + string.Join("; ", errors.ToArray());
                throw new InvalidOperationException(LastMessage);
            }

            LastMessage = string.Join("; ", messages.ToArray());
        }

        public string SendOpenSignalCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return "";
            if (openSignalClient == null || !openSignalClient.Connected)
            {
                throw new InvalidOperationException("OpenSignal is not connected.");
            }
            var stream = openSignalClient.GetStream();
            var bytes = Encoding.UTF8.GetBytes(command);
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
            var buffer = new byte[8192];
            var started = DateTime.Now;
            while (!stream.DataAvailable && (DateTime.Now - started).TotalMilliseconds < 500)
            {
                Application.DoEvents();
            }
            if (!stream.DataAvailable)
            {
                LastOpenSignalResponse = "";
                LastMessage = "OpenSignal command sent without immediate response: " + command;
                return LastOpenSignalResponse;
            }
            var count = stream.Read(buffer, 0, buffer.Length);
            LastOpenSignalResponse = count <= 0 ? "" : Encoding.UTF8.GetString(buffer, 0, count);
            LastMessage = LastOpenSignalResponse;
            return LastOpenSignalResponse;
        }

        public string SelfTest()
        {
            if (motorPort == null || !motorPort.IsOpen) throw new InvalidOperationException("CAN motor port is not connected.");
            if (stretchPort == null || !stretchPort.IsOpen) throw new InvalidOperationException("Arduino stretch port is not connected.");
            if (openSignalClient == null || !openSignalClient.Connected) throw new InvalidOperationException("OpenSignal TCP is not connected.");
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
            double realLoad = 0;
            var command = TractionCommand.Extend;
            SendStretchCommand("o");
            for (var i = 0; i < 3; i++)
            {
                command = ClinicalWorkflow.StaticTractionStep(prescription.TractionForce, initial, current, i, false, out realLoad);
                if (command != TractionCommand.Extend) break;
                SendStretchCommand("3");
            }
            if (command == TractionCommand.Hold) SendStretchCommand("c");
            if (command == TractionCommand.Stop) SendStretchCommand("c");
            LastMessage = "Static traction command=" + command + ", load=" + realLoad.ToString("F2", CultureInfo.InvariantCulture) + ", hold=" + prescription.TractionSeconds.ToString("F0", CultureInfo.InvariantCulture) + "s";
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
            if (stretchPort == null || !stretchPort.IsOpen) throw new InvalidOperationException("Arduino stretch port is not connected.");
            stretchPort.Write(command);
        }

        private void SendMotorFrame(byte[] data)
        {
            if (motorPort == null || !motorPort.IsOpen) throw new InvalidOperationException("CAN motor port is not connected.");
            motorPort.Write(data, 0, data.Length);
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
        public static extern int clinic_motor_set_angle_frame(int id, double angle, double speedOrTime, double param, int mode, byte[] outFrame16);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_motor_step_execute_frame(int id, int mode, int multiAxis, byte[] outFrame16);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_motor_preset_speed_frame(int id, double speed, double param, int mode, byte[] outFrame16);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_motor_preset_torque_frame(int id, double torque, double param, int mode, byte[] outFrame16);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_motor_motion_aid_frame(int id, double angle, double angleError, double speedError, double torque, byte[] outFrame16);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_motor_read_property_frame(int id, int address, int dataType, byte[] outFrame16);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_motor_adaptive_angle_frame(int id, double angle, double speed, double torque, byte[] outFrame16);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_motor_adaptive_multi_execute_frame(byte[] outFrame16);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_motor_set_speed_frame(int id, double speed, double param, int mode, byte[] outFrame16);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_motor_set_torque_frame(int id, double torque, double param, int mode, byte[] outFrame16);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_motor_pid_property_frames(int id, double p, double i, double d, byte[] outFrame16Array48);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_motor_angle_range_frames(int id, double angleMin, double angleMax, int persistent, int enable, byte[] outFrame16Array48);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_motor_config_order_frame(int id, uint order, byte[] outFrame16);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clinic_motor_init_config_frame(int id, byte[] outFrame16);

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

        public static byte[] SetAngle(int id, double angle, double speedOrTime, double param, int mode)
        {
            EnsureNativeCore();
            var frame = new byte[16];
            if (NativeCore.clinic_motor_set_angle_frame(id, angle, speedOrTime, param, mode, frame) != 16) throw new InvalidOperationException("ClinicCore failed to build motor set-angle frame.");
            return frame;
        }

        public static byte[] StepExecute(int id, int mode, bool multiAxis)
        {
            EnsureNativeCore();
            var frame = new byte[16];
            if (NativeCore.clinic_motor_step_execute_frame(id, mode, multiAxis ? 1 : 0, frame) != 16) throw new InvalidOperationException("ClinicCore failed to build motor step-execute frame.");
            return frame;
        }

        public static byte[] PresetSpeed(int id, double speed, double param, int mode)
        {
            EnsureNativeCore();
            var frame = new byte[16];
            if (NativeCore.clinic_motor_preset_speed_frame(id, speed, param, mode, frame) != 16) throw new InvalidOperationException("ClinicCore failed to build motor speed frame.");
            return frame;
        }

        public static byte[] PresetTorque(int id, double torque, double param, int mode)
        {
            EnsureNativeCore();
            var frame = new byte[16];
            if (NativeCore.clinic_motor_preset_torque_frame(id, torque, param, mode, frame) != 16) throw new InvalidOperationException("ClinicCore failed to build motor torque frame.");
            return frame;
        }

        public static byte[] MotionAid(int id, double angle, double angleError, double speedError, double torque)
        {
            EnsureNativeCore();
            var frame = new byte[16];
            if (NativeCore.clinic_motor_motion_aid_frame(id, angle, angleError, speedError, torque, frame) != 16) throw new InvalidOperationException("ClinicCore failed to build motor motion-aid frame.");
            return frame;
        }

        public static byte[] ReadProperty(int id, int address, int dataType)
        {
            EnsureNativeCore();
            var frame = new byte[16];
            if (NativeCore.clinic_motor_read_property_frame(id, address, dataType, frame) != 16) throw new InvalidOperationException("ClinicCore failed to build motor read-property frame.");
            return frame;
        }

        public static byte[] AdaptiveAngle(int id, double angle, double speed, double torque)
        {
            EnsureNativeCore();
            var frame = new byte[16];
            if (NativeCore.clinic_motor_adaptive_angle_frame(id, angle, speed, torque, frame) != 16) throw new InvalidOperationException("ClinicCore failed to build adaptive angle frame.");
            return frame;
        }

        public static byte[] AdaptiveMultiExecute()
        {
            EnsureNativeCore();
            var frame = new byte[16];
            if (NativeCore.clinic_motor_adaptive_multi_execute_frame(frame) != 16) throw new InvalidOperationException("ClinicCore failed to build adaptive multi-axis frame.");
            return frame;
        }

        public static byte[] SetSpeed(int id, double speed, double param, int mode)
        {
            EnsureNativeCore();
            var frame = new byte[16];
            if (NativeCore.clinic_motor_set_speed_frame(id, speed, param, mode, frame) != 16) throw new InvalidOperationException("ClinicCore failed to build direct speed frame.");
            return frame;
        }

        public static byte[] SetTorque(int id, double torque, double param, int mode)
        {
            EnsureNativeCore();
            var frame = new byte[16];
            if (NativeCore.clinic_motor_set_torque_frame(id, torque, param, mode, frame) != 16) throw new InvalidOperationException("ClinicCore failed to build direct torque frame.");
            return frame;
        }

        public static byte[][] PidProperties(int id, double p, double i, double d)
        {
            EnsureNativeCore();
            var frames = new byte[48];
            if (NativeCore.clinic_motor_pid_property_frames(id, p, i, d, frames) != 48) throw new InvalidOperationException("ClinicCore failed to build PID frames.");
            return SplitFrames(frames, 3);
        }

        public static byte[][] AngleRange(int id, double angleMin, double angleMax, bool persistent, bool enable)
        {
            EnsureNativeCore();
            var frames = new byte[48];
            var written = NativeCore.clinic_motor_angle_range_frames(id, angleMin, angleMax, persistent ? 1 : 0, enable ? 1 : 0, frames);
            if (written != 16 && written != 48) throw new InvalidOperationException("ClinicCore failed to build angle-range frames.");
            return SplitFrames(frames, written / 16);
        }

        public static byte[] ConfigOrder(int id, uint order)
        {
            EnsureNativeCore();
            var frame = new byte[16];
            if (NativeCore.clinic_motor_config_order_frame(id, order, frame) != 16) throw new InvalidOperationException("ClinicCore failed to build config order frame.");
            return frame;
        }

        public static byte[] InitConfig(int id)
        {
            EnsureNativeCore();
            var frame = new byte[16];
            if (NativeCore.clinic_motor_init_config_frame(id, frame) != 16) throw new InvalidOperationException("ClinicCore failed to build init-config frame.");
            return frame;
        }

        private static byte[][] SplitFrames(byte[] frames, int count)
        {
            var result = new byte[count][];
            for (var i = 0; i < count; i++)
            {
                result[i] = new byte[16];
                Array.Copy(frames, i * 16, result[i], 0, 16);
            }
            return result;
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
        public double TractionSeconds;
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

    internal sealed class SvmTrainingSet
    {
        public double[] Labels;
        public double[] Features;
        public int SampleCount;
        public int FeatureCount;
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
        private double adaptiveEndAngle;
        private double holdPainSum;
        private int holdPainCount;
        private double holdTorqueSum;
        private int holdTorqueCount;
        private int stopCount;
        private int completeCount;
        private int cyclesSeen;

        public TrainingSession(TrainingPrescription prescription)
        {
            this.prescription = prescription;
            adaptiveEndAngle = prescription.EndAngle;
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
            var phase = PhaseName(nativeStep.Phase);
            return new TrainingStep
            {
                TargetAngle = nativeStep.TargetAngle,
                Phase = phase,
                Progress = nativeStep.Progress,
                ShouldSendMotorCommand = nativeStep.ShouldSendMotorCommand != 0,
                UseImpedance = nativeStep.UseImpedance != 0,
                AssistTorque = nativeStep.AssistTorque,
                Kp = nativeStep.Kp,
                Kd = nativeStep.Kd,
                IsComplete = nativeStep.IsComplete != 0,
                PainLabel = phase == "hold" ? 1 : phase == "extension" ? -1 : 0
            };
        }

        public void Observe(TelemetrySample sample)
        {
            if (prescription.Mode != TrainingMode.AdaptivePassive && prescription.Mode != TrainingMode.AdaptiveResistance) return;
            if (sample.PainLabel == 1)
            {
                holdPainSum += sample.SvmScore;
                holdPainCount++;
                holdTorqueSum += Math.Abs(sample.Current);
                holdTorqueCount++;
            }
        }

        public void CompleteCycleIfNeeded(TrainingStep step)
        {
            if (step == null || (prescription.Mode != TrainingMode.AdaptivePassive && prescription.Mode != TrainingMode.AdaptiveResistance)) return;
            var cycle = Math.Max(1, (int)Math.Floor((DateTime.Now - startedAt).TotalSeconds / Math.Max(1.0, CycleSeconds)));
            if (cycle <= cyclesSeen) return;
            cyclesSeen = cycle;
            var avgPain = holdPainCount == 0 ? 0 : holdPainSum / holdPainCount;
            var avgTorque = holdTorqueCount == 0 ? 0 : holdTorqueSum / holdTorqueCount;
            if (avgPain < 0) stopCount++;
            else completeCount++;
            var resistance = Math.Min(1.0, avgTorque / 10.0);
            var fuzzyDelta = ClinicalWorkflow.FuzzyAngleAdjustment(Math.Max(-1.0, Math.Min(1.0, -avgPain)), resistance);
            adaptiveEndAngle = Math.Max(0, Math.Min(prescription.EndAngle + 15, adaptiveEndAngle + fuzzyDelta));
            adaptiveEndAngle = ClinicalWorkflow.NextAdaptiveResistanceAngle(adaptiveEndAngle, prescription.EndAngle + 15, stopCount, completeCount, 5);
            nativePrescription.EndAngle = adaptiveEndAngle;
            holdPainSum = 0;
            holdPainCount = 0;
            holdTorqueSum = 0;
            holdTorqueCount = 0;
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
        public int PainLabel;

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
            int nativeFeatureCount;
            var features = ExtractFeatures(window, sampleRate, out nativeFeatureCount);
            if (features == null || nativeFeatureCount <= 0) return 0;
            if (!EnsureModel(nativeFeatureCount)) return 0;
            var modelFeatures = new double[featureCount];
            Array.Copy(features, modelFeatures, Math.Min(featureCount, nativeFeatureCount));
            double label;
            double score;
            if (NativeCore.clinic_svm_predict(model, modelFeatures, featureCount, out label, out score) != 1)
            {
                return 0;
            }
            return score == 0 ? label : score;
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

        public static double[] ExtractFeatures(EmgWindow window, int sampleRate, out int featureCount)
        {
            featureCount = 0;
            if (window == null || window.Samples.Count == 0) return null;
            EnsureNativeCore();
            var channelCount = window.Samples.Max(x => x.Length);
            if (channelCount <= 0) return null;
            var flat = new List<double>();
            foreach (var row in window.Samples)
            {
                for (var i = 0; i < channelCount; i++) flat.Add(i < row.Length ? row[i] : 0);
            }
            var features = new double[32];
            if (NativeCore.clinic_emg_extract_features(flat.ToArray(), window.Samples.Count, channelCount, sampleRate, features, features.Length, out featureCount) != 1 || featureCount <= 0)
            {
                featureCount = 0;
                return null;
            }
            return features;
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

                var records = Path.Combine(Path.GetTempPath(), "ClinicRehabSuiteSelfTest");
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
                if (MotorFrames.SetAngle(1, 30.0, 5.0, 1.0, 1)[3] != 0x08) throw new InvalidOperationException("Native motor set-angle frame failed.");
                if (MotorFrames.StepExecute(1, 1, false)[3] != 0x08) throw new InvalidOperationException("Native motor step frame failed.");
                if (MotorFrames.StepExecute(0, 1, true)[3] != 0x08) throw new InvalidOperationException("Native motor multi-step frame failed.");
                if (MotorFrames.PresetSpeed(1, 2.0, 0.5, 1)[3] != 0x08) throw new InvalidOperationException("Native motor speed frame failed.");
                if (MotorFrames.PresetTorque(1, 1.0, 0.2, 1)[3] != 0x08) throw new InvalidOperationException("Native motor torque frame failed.");
                if (MotorFrames.MotionAid(1, 10.0, 1.0, 1.0, 0.5)[3] != 0x08) throw new InvalidOperationException("Native motor motion-aid frame failed.");
                if (MotorFrames.ReadProperty(1, 32002, 3)[3] != 0x08) throw new InvalidOperationException("Native motor read-property frame failed.");
                log.Add("native-motor-frames-ok");

                using (var controller = new DeviceController())
                {
                    var failed = false;
                    try
                    {
                        controller.Connect("COM_DOES_NOT_EXIST", "COM_DOES_NOT_EXIST", "127.0.0.1", 1);
                    }
                    catch (InvalidOperationException ex)
                    {
                        failed = ex.Message.Contains("Real hardware connection failed");
                    }
                    if (!failed) throw new InvalidOperationException("Hardware connection failure was not explicit.");
                }
                log.Add("hardware-failure-explicit-ok");

                if (MotorFrames.AdaptiveAngle(1, 45.0, 2.0, 1.0)[3] != 0x08) throw new InvalidOperationException("Native motor adaptive angle frame failed.");
                if (MotorFrames.AdaptiveMultiExecute()[3] != 0x08) throw new InvalidOperationException("Native motor adaptive execute frame failed.");
                if (MotorFrames.SetSpeed(1, 2.0, 0.2, 1)[3] != 0x08) throw new InvalidOperationException("Native motor direct speed frame failed.");
                if (MotorFrames.SetTorque(1, 1.0, 0.2, 1)[3] != 0x08) throw new InvalidOperationException("Native motor direct torque frame failed.");
                if (MotorFrames.PidProperties(1, 1.0, 1.0, 0.1).Length != 3) throw new InvalidOperationException("Native motor PID frames failed.");
                if (MotorFrames.AngleRange(1, 5.0, 90.0, false, true).Length != 3) throw new InvalidOperationException("Native motor angle range frames failed.");
                if (MotorFrames.ConfigOrder(1, 0x05)[3] != 0x08) throw new InvalidOperationException("Native motor config order frame failed.");
                if (MotorFrames.InitConfig(1)[3] != 0x08) throw new InvalidOperationException("Native motor init-config frame failed.");
                log.Add("native-motor-advanced-frames-ok");

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
                if (double.IsNaN(detectorScore) || double.IsInfinity(detectorScore) || Math.Abs(detectorScore) < 1.0e-12) throw new InvalidOperationException("SvmPainDetector native prediction failed.");
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
            var path = Path.Combine(Path.GetTempPath(), "ClinicRehabSuite-selftest.log");
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
        public int PainLabel;
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
