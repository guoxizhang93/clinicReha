classdef clinic_test0401 < matlab.apps.AppBase

    % Properties that correspond to app components
    properties (Access = public)
        UIFigure                    matlab.ui.Figure
        TabGroup2                   matlab.ui.container.TabGroup
        Tab                         matlab.ui.container.Tab
        Gauge_SVM                   matlab.ui.control.LinearGauge
        svmGaugeLabel               matlab.ui.control.Label
        UIAxes_EMG_5                matlab.ui.control.UIAxes
        UIAxes_EMG_trigger          matlab.ui.control.UIAxes
        UIAxes_EMG_4                matlab.ui.control.UIAxes
        UIAxes_EMG_3                matlab.ui.control.UIAxes
        UIAxes_EMG_2                matlab.ui.control.UIAxes
        UIAxes_EMG_1                matlab.ui.control.UIAxes
        Tab_2                       matlab.ui.container.Tab
        Lamp_1                      matlab.ui.control.Lamp
        Lamp_2                      matlab.ui.control.Lamp
        Lamp_3                      matlab.ui.control.Lamp
        Lamp_4                      matlab.ui.control.Lamp
        Lamp_5                      matlab.ui.control.Lamp
        Lamp_6                      matlab.ui.control.Lamp
        Lamp_7                      matlab.ui.control.Lamp
        Lamp_8                      matlab.ui.control.Lamp
        Stretch_State               matlab.ui.control.Label
        Heat_Press                  matlab.ui.control.UIAxes
        Stretch_pos                 matlab.ui.control.UIAxes
        Stretch_force               matlab.ui.control.UIAxes
        Label_State                 matlab.ui.control.Label
        Label_UpdateData            matlab.ui.control.Label
        Lamp                        matlab.ui.control.Lamp
        Label_12                    matlab.ui.control.Label
        Button_MsgPatient           matlab.ui.control.Button
        Label_Tips                  matlab.ui.control.Label
        Panel_MotorUIAxes           matlab.ui.container.Panel
        Label_MotorDegree           matlab.ui.control.Label
        UIAxes_Speed                matlab.ui.control.UIAxes
        UIAxes_Degree               matlab.ui.control.UIAxes
        UIAxes_Current              matlab.ui.control.UIAxes
        TabGroup                    matlab.ui.container.TabGroup
        Tab_TwoMotor                matlab.ui.container.Tab
        Panel_DeviceOpra            matlab.ui.container.Panel
        Button_Finish               matlab.ui.control.Button
        Button_InitAll              matlab.ui.control.Button
        Button_CloseDevice          matlab.ui.control.Button
        Panel_RangeDetect           matlab.ui.container.Panel
        Label_MaxDegree             matlab.ui.control.Label
        Button_MaxAngle_After       matlab.ui.control.Button
        Button_MaxAngle_Before      matlab.ui.control.Button
        Panel_Calibration           matlab.ui.container.Panel
        Label_DegreeCalib           matlab.ui.control.Label
        Label_DegreePlus            matlab.ui.control.Label
        Button_DegreePlus           matlab.ui.control.Button
        EditField_StartDegreeCalib  matlab.ui.control.NumericEditField
        Label_14                    matlab.ui.control.Label
        Button_StartCalib           matlab.ui.control.Button
        Panel_TrainSet              matlab.ui.container.Panel
        DropDown_RestTime           matlab.ui.control.DropDown
        Label_22                    matlab.ui.control.Label
        DropDown_TractionTime       matlab.ui.control.DropDown
        Label_21                    matlab.ui.control.Label
        DropDown_TractionForce      matlab.ui.control.DropDown
        Label_20                    matlab.ui.control.Label
        DropDown_KeepTime           matlab.ui.control.DropDown
        Label_19                    matlab.ui.control.Label
        DropDown_AgainstPower       matlab.ui.control.DropDown
        Label_17                    matlab.ui.control.Label
        DropDown_TrainMode          matlab.ui.control.DropDown
        Label_16                    matlab.ui.control.Label
        DropDown_Speed              matlab.ui.control.DropDown
        Label_11                    matlab.ui.control.Label
        DropDown_Times              matlab.ui.control.DropDown
        Button_TrainStart           matlab.ui.control.Button
        EditField_EndDegree         matlab.ui.control.NumericEditField
        Label_3                     matlab.ui.control.Label
        EditField_StartDegree       matlab.ui.control.NumericEditField
        Label_2                     matlab.ui.control.Label
        Label                       matlab.ui.control.Label
        Tab_Opensignal              matlab.ui.container.Tab
        Label_OP_ID                 matlab.ui.control.Label
        Label_StateOP               matlab.ui.control.Label
        Button_InitOP               matlab.ui.control.Button
        Panel_OP_Message            matlab.ui.container.Panel
        TextArea_receive            matlab.ui.control.TextArea
        TextArea_send               matlab.ui.control.TextArea
        TextArea_config             matlab.ui.control.TextArea
        Label_receive               matlab.ui.control.Label
        Label_send                  matlab.ui.control.Label
        Label_config                matlab.ui.control.Label
        ButtonGroup_OP_OPRA         matlab.ui.container.ButtonGroup
        Button_update_config        matlab.ui.control.Button
        Button_send                 matlab.ui.control.Button
        SelectButton_devices        matlab.ui.control.RadioButton
        SelectButton_config         matlab.ui.control.RadioButton
        SelectButton_enable         matlab.ui.control.RadioButton
        SelectButton_disable        matlab.ui.control.RadioButton
        Tab_3                       matlab.ui.container.Tab
        Button_stretch_4            matlab.ui.control.Button
        Button_stretch_3            matlab.ui.control.Button
        Button_stretch_2            matlab.ui.control.Button
        Button_stretch              matlab.ui.control.Button
        Button_StretchMinus_2       matlab.ui.control.Button
        Button_StretchPlus_2        matlab.ui.control.Button
        ButtonGroup_2               matlab.ui.container.ButtonGroup
        Button_6                    matlab.ui.control.ToggleButton
        Button_5                    matlab.ui.control.ToggleButton
        Button_4                    matlab.ui.control.ToggleButton
        ButtonGroup                 matlab.ui.container.ButtonGroup
        Button_3                    matlab.ui.control.ToggleButton
        Button_2                    matlab.ui.control.ToggleButton
        Button                      matlab.ui.control.ToggleButton
    end

    
    properties (Access = private)
        % 鏍囧畾鏈€澶ц搴﹁寖鍥?        max_degree
        % 鎷変几鍙傛暟
        Com_stretch
        stopflag = ""

        % 鎮ｈ€呬俊鎭?        DialogApp
        Patient = []

        % 閫氱敤鍙傛暟
        IsPain = 0 % 鐤肩棝鏍囩 鏄惁涓虹柤鐥涚姸鎬?        IsStop = 0 % 鍋滄鏍囪 鏄惁鍋滄
        IsPlus = 0 % 澧炲姞鐢垫満瑙掑害鏍囪 鏄惁澧炲姞瑙掑害
        IsOP = 0 % 鑲岀數浠爣璁?鏄惁杩炴帴鑲岀數浠?        IsInit = 0 %鍒濆鍖栨爣璁?鏄惁鍒濆鍝?        name_save_mat 
        name_save_all
        path_SaveFolder
        % 澶х劧鐢垫満
        dr % 瀵硅薄鍚嶇О
        id = [1,2] % 鐢垫満id
        f  = 10 % 鐢垫満鍙戦€佽建杩硅窡闅忛鐜?

        % 鐢垫満鐘舵€佷俊鎭?        message_motor = struct("temperature",[],"current",[],"speed",[] ,"degree",[]);
        % 鐢垫満褰撳墠鐘舵€?        state_now_motor = struct("temperature",[0,0],"torque",[0,0],"speed",[0,0],"degree",[0,0])
        machine_maxdegree
        state_degree_start % 鑷€傚簲璧峰瑙掑害
        state_degree_end % 鑷€傚簲缁堟瑙掑害
        Train_times = 0 % 璁粌娆℃暟
        Calib_times = 0 % 鏍囧畾娆℃暟
        Adapt_times = 0 % 鑷€傚簲璁粌娆℃暟
        Train_data % 璁粌鏁版嵁
        Calib_data % 鏍囧畾鏁版嵁
        Adapt_data % 鑷€傚簲璁粌鏁版嵁
        Train_config
        Calib_config

        keep_total_save
        fis = mamfis('Name', 'AngleControl');

        Window_data % 瀹炴椂绐楁暟鎹?        % 搴峰鍙傛暟
        Data_motor_1 = []
        Data_motor_2 = []


        % OP鍙傛暟
        op_clent % tcp/ip瀹㈡埛绔?        commands_op % 瀹㈡埛绔悜op鍙戦€佺殑鎸囦护
        receives_op % 瀹㈡埛绔粠op鎺ユ敹鍒扮殑鏁版嵁
        Data_op = [];
        active
        config_op = struct( "ID",'',...
            "activeChannels",[], ...
            "samplingFreq",0, ...
            "labelChannels",[], ...
            "sensorChannels",[] ...
            )

        C = []
        S = []
        Model %svm鐤肩棝妫€娴嬫ā鍨?
        % 缁樺浘鍙傛暟
        Buff_motor = zeros(500,6);
        Buff_op = zeros(5000,6);
        Count_op

        data_pos=[]; % 璁板綍4缁勬帹鏉嗙殑浣嶇疆
        data_press=[];% 鍘嬪姏浼犳劅鍣ㄦ暟鎹?        % Com_stretch
        % stopflag=''
        stop_flag=0;
        inflation_flag=0;
        Press_well=[6,6,6,6,6,6,6,6];
        Press_limit=[15,15,15,15,15,15,15,15];
        
        Buff_motor2=zeros(100,8); %鎺ㄦ潌鏁版嵁
        Buff_motor3=zeros(100,8); %浼犳劅鍣ㄦ暟鎹?        name_save
        pos_direct

        image_exoskeleton
        actuators_max_movment=0;
    end

    properties (Access = public)
    end

    methods (Access = private)
        % 鐢垫満瑙掑害杞崲涓哄洓鏉嗘満鏋勮搴?        function angle_output=motor2linkage(app,angle_input)
            l1=43.6;
            l2=60.6;
            l3=48.5;
            l4=24.6;
            phi=pi/3;

            angle2rad=pi/180;
            rad2angle=180/pi;

            theta0=(angle_input+3)*angle2rad;
            l_BD=sqrt(l1^2+l4^2-2*l1*l4*cos(phi+theta0));
            theta1=asin((l1*sin(phi+theta0))/l_BD);
            theta2=acos((l_BD^2+l3^2-l2^2)/(2*l_BD*l3));
            phi3=theta1+theta2-(pi-phi);
            phi2=asin((l1*sin(theta0)+l4*sin(phi)+l3*sin(phi3))/l2);
            angle_output=90-(phi2*rad2angle);
            app.Label_MotorDegree.Text = strcat("褰撳墠瑙掑害锛?,string(angle_output));
        end

        %  鍥涙潌鏈烘瀯瑙掑害杞崲涓虹數鏈鸿搴?        function angle_output=linkage2motor(app,angle_input)
            motor_angle=1:180;
            linkage_angle=[0.975843406238283	1.84353832467785	2.69832422248591	3.54066256064978	4.37099891655988	5.18976353935132	5.99737189708415	6.79422521394369	7.58071099603211	8.35720354460096	9.12406445585637	9.88164310667889	10.6302771258035	11.3702928501584	12.1020057662025	12.8257209362168	13.5417334096008	14.2503286193016	14.9517827635742	15.6463631733213	16.3343286653056	17.0159298815597	17.6914096153455	18.3610031240350	19.0249384292994	19.6834366049979	20.3367120531687	20.9849727685229	21.6284205918412	22.2672514526704	22.9016556017136	23.5318178332961	24.1579176982867	24.7801297078401	25.3986235283216	26.0135641677583	26.6251121541574	27.2334237060154	27.8386508953345	28.4409418034501	29.0404406699613	29.6372880350473	30.2316208754397	30.8235727343114	31.4132738453324	32.0008512511310	32.5864289163927	33.1701278358137	33.7520661371242	34.3323591793815	34.9111196467274	35.4884576377961	36.0644807509504	36.6392941655172	37.2130007191844	37.7857009817181	38.3574933251481	38.9284739905673	39.4987371516823	40.0683749752482	40.6374776785158	41.2061335838115	41.7744291703700	42.3424491235320	42.9102763814162	43.4779921791702	44.0456760909029	44.6134060693949	45.1812584836831	45.7493081546095	46.3176283884241	46.8862910085258	47.4553663854276	48.0249234650238	48.5950297952398	49.1657515511397	49.7371535585672	50.3092993163913	50.8822510174272	51.4560695681038	52.0308146069410	52.6065445219069	53.1833164667163	53.7611863761350	54.3402089803503	54.9204378184699	55.5019252512059	56.0847224728036	56.6688795222710	57.2544452939643	57.8414675475840	58.4299929176348	59.0200669224007	59.6117339724868	60.2050373789768	60.8000193612550	61.3967210545401	61.9951825171760	62.5954427377256	63.1975396419095	63.8015100994338	64.4073899307451	65.0152139137545	65.6250157905678	66.2368282742576	66.8506830557139	67.4666108106056	68.0846412064859	68.7048029100711	69.3271235947227	69.9516299481595	70.5783476804256	71.2073015321382	71.8385152830377	72.4720117608606	73.1078128505545	73.7459395038519	74.3864117492194	75.0292487021949	75.6744685761267	76.3220886933236	76.9721254966260	77.6245945614052	78.2795106079966	78.9368875145709	79.5967383304473	80.2590752898481	80.9239098260972	81.5912525862590	82.2611134462162	82.9335015261838	83.6084252066514	84.2858921447517	84.9659092910449	85.6484829067139	86.3336185811602	87.0213212499907	87.7115952133881	88.4044441548507	89.0998711602943	89.7978787375016	90.4984688359103	91.2016428667260	91.9074017233496	92.6157458021065	93.3266750232656	94.0401888523378	94.7562863216407	95.4749660521214	96.1962262754236	96.9200648561922	97.6464793146043	98.3754668491177	99.1070243594318	99.8411484696502	100.577835551643	101.317081748600	102.058882998776	102.803235059420	103.550133530890	104.299573880958	105.051551469295	105.806061572150	106.563099407225	107.322660158747	108.084739002754	108.849331132592	109.616431784649	110.386036264330	111.158139972291	111.932738430948	112.709827311287	113.489402459988	114.271459926895	115.055995992856	115.843007197961	116.632490370212	117.424442654661	118.218861543055	119.015744904022];
            [~,num]=min(abs(angle_input-linkage_angle));
            angle_output=motor_angle(num);
        end

        % 鐢垫満缁樺浘
        function PlotData_motor(app)
            len = min(size(app.Data_motor_1,1),size(app.Data_motor_2,1));
            if len >= 500
                app.Buff_motor = [flip(app.Data_motor_1(end-499:end,1:3)),-1 * flip(app.Data_motor_2(end-499:end,1:3))];
            else                
                Z = zeros(500-len,6);
                app.Buff_motor = [[flip(app.Data_motor_1(1:len,1:3)),-1 * flip(app.Data_motor_2(1:len,1:3))];Z];
            end
            plot(app.UIAxes_Current,app.Buff_motor(:,[3,6]));
            plot(app.UIAxes_Speed,app.Buff_motor(:,[2,5]));
            plot(app.UIAxes_Degree,app.Buff_motor(:,[1,4]));
        end

        % 淇濆瓨鏁版嵁璁剧疆锛堢‘瀹氫繚瀛樻枃浠跺悕"閲囨牱鏃堕棿鐐?锛?        function Save_dataset(app)
            fullpath = mfilename('fullpath');%鑾峰彇褰撳墠m鏂囦欢鐨勭粷瀵硅矾寰勶紝涓嶅寘鍚枃浠跺悕鍚庣紑
            [path_,name,ext] = fileparts(fullpath);%灏嗙粷瀵硅矾寰勬媶鍒嗭紝path涓哄綋鍓嶅伐浣滆矾寰勶紱name涓哄綋鍓嶈繍琛屾枃浠跺悕锛沞xt涓烘枃浠跺悕鍚庣紑
            pathname = strcat(app.path_SaveFolder,'\');
            %pathname = strcat(path_,'\',app.path_SaveFolder,'\');
            time_now = string(datetime("now","InputFormat","uuuuMMdd'T'HHmmss"),"uuuu骞碝M鏈坉d鏃H鏃秏m鍒唖s绉?);
            app.name_save_mat = strcat(pathname,time_now,'.mat');
            app.name_save_all = strcat(pathname,time_now,'.mat');
        end
        % 淇濆瓨鏁版嵁
        function Save_data_part(app)

            Data_save{1} = app.Data_motor_1;
            Data_save{2} = app.Data_motor_2;
            Data_save{3} = app.Data_op;
            Data_save{4} = app.Train_config;
            name = strcat(app.name_save_mat,'-train_times_',num2str(app.Train_times));
            save(name,"Data_save");
        end

        function Save_data_all(app)

            train_data = app.Train_data;
            train_config = app.Train_config;

            calib_data = app.Calib_data;
            calib_config = app.Calib_config;

            patient = app.Patient;

            save(strcat(app.name_save_all,app.Patient.name),"train_data","train_config", ...
                                                            "calib_data","calib_config", ...
                                                            "patient");
        end

        % op鐢熺悊淇″彿閲囬泦鐩稿叧鍑芥暟

        % 鍒濆鍖栵紝杩炴帴op锛岀‘璁p鐨処D
        function init_clent(app)
            app.op_clent = tcpclient('127.0.0.1',5555);
            pause(1);
            write(app.op_clent,'devices');
            pause(0.5);
            text = read(app.op_clent);
            if isempty(text)
                app.Label_State.Text = "鏃犳硶鎺ユ敹鍒版暟鎹?妫€鏌p锛屽皾璇曢噸鏂拌繛鎺?;
                pause(1);
                error('error');
            else
                text = char(text);
                app.TextArea_receive.Value = text;
                app.config_op.ID = app.TextArea_receive.Value{1}(end-19:end-3);
                app.Label_OP_ID.Text = app.config_op.ID;
            end
        end
        % 鍚憃p鍙戦€佹寚浠?        function send_command(app)
            if app.IsOP == 1
                write(app.op_clent,app.commands_op);
                app.Label_State.Text = "鍙戦€佸畬鎴?;
            end
        end
        % 浠巓p鎺ユ敹鏁版嵁
        function receive_command(app)
            if app.IsOP == 1
                app.receives_op = char(read(app.op_clent));
                app.Label_State.Text = "鎺ユ敹瀹屾垚";
            end
        end
        % 鏇存柊op閲囨牱閰嶇疆鑷砿atlab
        function config_update(app)
            if app.IsOP == 1
                command = "config";
                text = app.TextArea_receive.Value{1};
    
                if command == "config" && length(text) > 100
                    app.Label_State.Text = "鍙洿鏂?;
                    pos = strfind(text,'"activeChannels": ');
                    app.config_op.activeChannels = str2num(text(pos+18:pos+41)); %18涓?"activeChannels": '鐨勯暱搴︼紝41涓?8鍔犳暟鎹暱搴?    
                    pos = strfind(text,'"samplingFreq": ');
                    app.config_op.samplingFreq = str2num(text(pos+16:pos+19)); %16涓?"samplingFreq": '鐨勯暱搴︼紝19涓?6鍔犳暟鎹暱搴?    
                    pos = strfind(text,'"labelChannels": ');
                    pos_end = strfind(text,']');
                    pos_end1 = find(pos_end > pos);
                    app.config_op.labelChannels = split(erase(text(pos+18:pos_end(pos_end1(1))-1),' ')); %16涓?"samplingFreq": '鐨勯暱搴︼紝19涓?6鍔犳暟鎹暱搴?    
                    pos = strfind(text,'"sensorChannels": ');
                    pos_end = strfind(text,']');
                    pos_end1 = find(pos_end > pos);
                    app.config_op.sensorChannels = split(erase(text(pos+19:pos_end(pos_end1(1))-1),' ')); %16涓?"samplingFreq": '鐨勯暱搴︼紝19涓?6鍔犳暟鎹暱搴?    
                    app.TextArea_config.Value = ["activeChannels:",num2str(app.config_op.activeChannels),"samplingFreq:",num2str(app.config_op.samplingFreq),"sensorChannels:",app.config_op.sensorChannels];
    
                    app.config_op.labelChannels = split(string(app.config_op.labelChannels),',');
                    app.config_op.sensorChannels = split(string(app.config_op.sensorChannels),',');
                else
                    app.Label_State.Text = "鏃犳硶鏇存柊";
                    return;
                end
    
                app.active =  find(app.config_op.activeChannels == 1);
                title_UI = strcat(app.config_op.labelChannels,app.config_op.sensorChannels);
                for i = 1 : length(app.active)
                    app.("UIAxes_EMG_" + num2str(i)).Title.String = char(title_UI(app.active(i)));
                end
            end
        end

        % 鎺ユ敹op閲囨牱鏁版嵁寮€鍚樁娈碉紙闇€瑕佺瓑寰呭嚑绉掓墠鑳藉紑濮嬮噰闆嗭級
        function receive_data_op_pre(app)
            if app.IsOP == 1
                app.Data_op = [];
                app.Count_op = 0;
                app.commands_op = 'start';
                send_command(app);
                pause(0.5);
                app.receives_op = char(read(app.op_clent));
                while app.op_clent.NumBytesAvailable == 0
                end
            end
        end

        function receive_data_op_clearBuff(app)
            if app.IsOP == 1
                app.Data_op = [];
                app.Count_op = 0;
                app.receives_op = char(read(app.op_clent));
                app.receives_op = [];           
            end

        end


        function [predict_label,accy,svmrst] = receive_data_op(app)
            if app.IsOP == 1
                predict_label = nan; accy = nan; svmrst = nan;           
                if app.op_clent.NumBytesAvailable ~= 0
                    data = process_data_op(app); % 灏嗛噰闆嗗埌鐨勬枃鏈牸寮忕殑鏁版嵁杞寲涓篸ouble鏍煎紡
                    if ~isempty(app.Model)
                        [predict_label,accy,svmrst] = Predict_WindowData(app,data);
                    end
                    app.Data_op = [app.Data_op;data];
                    PlotData_op(app,data);
                end
            else
                predict_label = nan; accy = nan; svmrst = nan;
            end
            
        end

        function data = process_data_op(app)

            app.receives_op = char(read(app.op_clent));
            app.receives_op = erase(app.receives_op,'{"returnCode": 0, "returnData": {"00:07:80:4B:29:56":');
            app.receives_op = erase(app.receives_op,'}}');

            data_ = str2num(app.receives_op);
            data__ = reshape(data_,length(app.active)+2,[]);
            data = data__';

            trigger = data(:,2);
            if max(trigger) == 1
                app.IsStop = 1;
            else
                app.IsStop = 0;
            end

            % 娣诲姞鐤肩棝鏍囩
            label = zeros(size(data,1),1);
            label(:) = app.IsPain;
            data = [data,label];


        end

        function [predict_label,accy,svmrst] = Predict_WindowData(app,Data)
            data = Data(:,[3:6,end]);
            % data_1 = normalize(abs(data(:,1:4)),"center",app.C,"scale",app.S);
            data_1 = [data(:,1:4),data(:,end)];


            feature(1,1:4) = mean(abs(data_1(:,1:4))); % 鍧囧€?            feature(1,5:8) = std(data_1(:,1:4)); % 鏍囧噯宸?            feature(1,9:12) = rms(data_1(:,1:4)); % 鍧囨柟鏍?            feature(1,13:16) = sum(abs(diff(data_1(:,1:4))))/size(data,1); % 娉㈤暱
            feature(1,17:20) = bandpower(data_1(:,1:4),2000,[20,500]); % band power
            % feature(1,21:24) = meanfreq(data_1(:,1:4),2000); % 骞冲潎棰戠巼
            label(1) = max(data_1(:,end));
            if label == -1
                predict_label = 0;
                accy = 0;
                svmrst = 0;
            end

            [predict_label,accy,svmrst] = svmpredict(label,feature,app.Model);

        end

        function PlotData_op(app,data)
            if app.IsOP == 1
                [L,CH] = size(data);
                app.Count_op = app.Count_op + L;
    
                for i = 1 : CH-2
                    app.Buff_op(:,i) = [flip(data(:,i+2));app.Buff_op(1:end-L,i)];
                end
                app.Buff_op(:,6) = [flip(data(:,2));app.Buff_op(1:end-L,6)];
    
                for i = 1 : CH-2
                    plot(app.("UIAxes_EMG_" + num2str(i)),app.Buff_op(:,i));
                    %app.("UIAxes_EMG_" + num2str(i+3)).XTickLabel = linspace(roundn(app.Count_op/2000,-1),roundn((app.Count_op-1000)/2000,-1),6);
                end
                plot(app.UIAxes_EMG_trigger,app.Buff_op(:,6));
                %app.UIAxes_EMG_trigger.XTickLabel = linspace(roundn(app.Count_op/2000,-1),roundn((app.Count_op-1000)/2000,-1),6);                
            end

        end


        % 鍔熻兘鎵ц杩囩▼涓娇鑳藉叧闂?        function EnableOff(app)
            app.Button_InitAll.Enable = 'off';
            app.Panel_RangeDetect.Enable = 'off';
            app.Panel_Calibration.Enable = 'off';
            app.Panel_TrainSet.Enable = 'off';           
            app.Button_MsgPatient.Enable = 'off';
        end

        % 鍔熻兘鎵ц杩囩▼涓娇鑳芥墦寮€
        function EnableOn(app)
            app.Button_InitAll.Enable = 'on';
            app.Panel_RangeDetect.Enable = 'on';
            app.Panel_Calibration.Enable = 'on';
            app.Panel_TrainSet.Enable = 'on';
            app.Button_MsgPatient.Enable = 'on';
        end



        function model = TrainingModel(app,Data)
            if app.IsOP == 1
                data = Data(:,[3:6,end]);
                % [data_1,app.C,app.S] = normalize(abs(data(:,1:4)),"range");
                data_1 = [data(:,1:4),data(:,end)];
    
                %璁剧疆鍒嗘瀽绐?                Slwindow_L = 500;
                Slwindow_step = 250;
                Slwindow_start = 1;
                Slwindow_end = Slwindow_L;
                Slwindow_data = zeros(Slwindow_L,1);
    
                L_data = size(data_1,1);
                step_count = ceil((L_data-Slwindow_L)/Slwindow_step);
                feature = zeros(step_count,20);
    
                for step = 0:step_count -1
                    Slwindow_data = data_1(Slwindow_start:Slwindow_end,1:4);
                    Slwindow_label = data_1(Slwindow_start:Slwindow_end,end);
                    feature(step+1,1:4) = mean(abs(Slwindow_data)); % 鍧囧€?                    feature(step+1,5:8) = std(Slwindow_data); % 鏍囧噯宸?                    feature(step+1,9:12) = rms(Slwindow_data); % 鍧囨柟鏍?                    feature(step+1,13:16) = sum(abs(diff(Slwindow_data)))/Slwindow_L; % 娉㈤暱
                    feature(step+1,17:20) = bandpower(Slwindow_data,2000,[20,500]); % band power
                    % feature(step+1,21:24) = medfreq(Slwindow_data,2000); % 骞冲潎棰戠巼

                    label(step+1,1) = max(Slwindow_label);
                    Slwindow_start = Slwindow_start + Slwindow_step;   %绐楁杩?                    Slwindow_end = Slwindow_end + Slwindow_step;       %绐楁杩?                end
    
                feature_T = feature((label == 0 | label == 1),:);
                label_T = label(label == 0 | label == 1);
                paramlibsvm=['-t ',num2str(0),' -q '];
                model = svmtrain(label_T, feature_T, paramlibsvm);
            else
                model = [];
            end
        end


        function callbackFcn(app,~,~)
            result = app.dr.angle_speed_torque_states(app.id);

            label1 = zeros(size(result{1},1),1);
            label1(:) = app.IsPain;
            result{1} = [result{1},label1];

            label2 = zeros(size(result{2},1),1);
            label2(:) = app.IsPain;
            result{2} = [result{2},label2];

            app.Data_motor_1 = [app.Data_motor_1;result{1}];
            app.Data_motor_2 = [app.Data_motor_2;result{2}];

            app.state_now_motor.degree = [app.Data_motor_1(end,1),app.Data_motor_2(end,1)];
            app.state_now_motor.speed = [app.Data_motor_1(end,2),app.Data_motor_2(end,2)];
            app.state_now_motor.torque = [app.Data_motor_1(end,3),app.Data_motor_2(end,3)];
            degree = mean(abs(app.state_now_motor.degree));
            motor2linkage(app,degree);
            PlotData_motor(app);
        end


        % 琚姩璁粌
        function train_mode_1(app)
            train_mode = app.DropDown_TrainMode.Value;
            times = str2double(app.DropDown_Times.Value);
            speed_level = app.DropDown_Speed.Value;            
            degree_start_ = app.EditField_StartDegree.Value;           
            degree_end_ = app.EditField_EndDegree.Value; 
            degree_start = linkage2motor(app,degree_start_);
            degree_end = linkage2motor(app,degree_end_);
            T_keep = str2double(app.DropDown_KeepTime.Value);
            T_rest = str2double(app.DropDown_RestTime.Value);
            app.Train_config{app.Train_times} = struct("train_mode",train_mode, ...
                                                       "times",times, ...
                                                       "speed",speed_level, ...
                                                       "degree_start",[degree_start_,degree_start], ...
                                                       "degree_end",[degree_end_,degree_end], ...
                                                       "keep_time",T_keep, ...
                                                       "rest_time",T_rest); 
            
            % 閫熷害涓夋。璋冭妭            
            switch speed_level
                case "缂撴參"
                    T = 20;
                case "閫備腑"
                    T = 15;
                case "蹇€?
                    T = 10;
            end
            
            % 杞ㄨ抗璁剧疆
            x = 0:0.1:T+1;

            % degree_go = (degree_end-degree_start)/2 * cos(pi/T * x + pi) + (degree_end+degree_start)/2;
            trace = app.Seg_trace_result(T, degree_start, degree_end);
            degree_go = [trace,ones(1,10)*trace(end)];

            % 鍑嗗寮€濮?            app.dr.enable_angle_speed_torque_state(1); % 寮€鍚疄鏃跺弽棣堜娇鑳?            app.dr.enable_angle_speed_torque_state(2); % 寮€鍚疄鏃跺弽棣堜娇鑳?            receive_data_op_clearBuff(app);
            % 璁粌寰幆
            for i = 1 : times
                app.Label_Tips.Text = ['绗?,num2str(i),'娆¤缁?];
            % 灞堟洸杩愬姩                
                tic;
                app.Label_State.Text = "寮€濮?;
                while 1
                    pause(0.1);
                    t = ceil(toc*10);
                    target_angle = degree_go(t);
                    app.dr.set_angles([1,2], [target_angle,-target_angle], 10, app.f/2, 0);
                    [~,~,svmrst] = receive_data_op(app);
                    if ~isnan(svmrst)
                        app.Gauge_SVM.Value = -svmrst;
                        disp(-svmrst);
                    end
                    if (app.IsStop == 1) || (toc >= T)
                        app.dr.estop(0);
                        app.IsPain = 1;
                        app.IsStop = 0;
                        break;
                    end
                end                
            % 鍒拌揪璁惧畾浣嶇疆锛屼繚鎸乀_keep绉?                app.Label_State.Text = "淇濇寔";
                tic
                while (toc < T_keep) 
                    [~,~,svmrst] = receive_data_op(app);
                    if ~isnan(svmrst)
                        app.Gauge_SVM.Value = -svmrst;
                        disp(-svmrst);
                    end
                    pause(0.1);
                end
                
                degree_stop = round(mean(abs(app.state_now_motor.degree)));
                % degree_back = (degree_stop-degree_start)/2 * cos(pi/T * x ) + (degree_stop+degree_start)/2;
                degree_back = [trace(end:-1:1), ones(1,10)*trace(1)];
                % 浼稿睍杩愬姩
                app.IsPain = -1;
                tic;
                app.Label_State.Text = "缁撴潫";
                while toc < T
                    pause(0.1);
                    t = ceil(toc*10);
                    target_angle = degree_back(t);
                    app.dr.set_angles([1,2], [target_angle,-target_angle], 10, app.f/2, 0);
                    [~,~,svmrst] = receive_data_op(app);
                    if ~isnan(svmrst)
                        app.Gauge_SVM.Value = -svmrst;
                        disp(-svmrst);
                    end
                end            
            % 鍒拌揪鍒濆浣嶇疆锛屼紤鎭疶_rest绉?                app.IsPain = 0;
                app.Label_State.Text = "浼戞伅";
                tic
                while toc < T_rest
                    [~,~,svmrst] = receive_data_op(app);
                    if ~isnan(svmrst)
                        app.Gauge_SVM.Value = -svmrst;
                        disp(-svmrst);
                    end
                    pause(0.1);
                end

            end
            pause(1);
            app.dr.disable_angle_speed_torque_state(1);
            app.dr.disable_angle_speed_torque_state(2);
            flush(app.dr.dev);
        end

        % 瀵规姉璁粌
        function train_mode_2(app)
            train_mode = app.DropDown_TrainMode.Value;
            times = str2double(app.DropDown_Times.Value);
            speed_level = app.DropDown_Speed.Value;            
            degree_start_ = app.EditField_StartDegree.Value;           
            degree_end_ = app.EditField_EndDegree.Value; 
            degree_start = linkage2motor(app,degree_start_);
            degree_end = linkage2motor(app,degree_end_);
            against_power = app.DropDown_AgainstPower.Value;
            T_keep = str2double(app.DropDown_KeepTime.Value);
            T_rest = str2double(app.DropDown_RestTime.Value);
            app.Train_config{app.Train_times} = struct("train_mode",train_mode, ...
                                                       "times",times, ...
                                                       "speed",speed_level, ...
                                                       "degree_start",[degree_start_,degree_start], ...
                                                       "degree_end",[degree_end_,degree_end], ...
                                                       "keep_time",T_keep, ...
                                                       "rest_time",T_rest, ...
                                                       "against_power",against_power); 
            
            % 閫熷害涓夋。璋冭妭            
            switch speed_level
                case "缂撴參"
                    T = 20;
                case "閫備腑"
                    T = 15;
                case "蹇€?
                    T = 10;
            end
            switch against_power
                case "杈冨皬"
                    ag_pw = 1;
                case "閫備腑"
                    ag_pw = 1.5;
                case "杈冨ぇ"
                    ag_pw = 2;
            end
            
            % 杞ㄨ抗璁剧疆
            x = 0:0.1:T+1;
            % degree_go = (degree_end-degree_start)/2 * cos(pi/T * x + pi) + (degree_end+degree_start)/2;
            trace = app.Seg_trace_result(T, degree_start, degree_end);
            degree_go = [trace,ones(1,10)*trace(end)];
            % 鍑嗗寮€濮?            app.dr.enable_angle_speed_torque_state(1); % 寮€鍚疄鏃跺弽棣堜娇鑳?            app.dr.enable_angle_speed_torque_state(2); % 寮€鍚疄鏃跺弽棣堜娇鑳?            receive_data_op_clearBuff(app);
            % 璁粌寰幆
            for i = 1 : times
                app.Label_Tips.Text = ['绗?,num2str(i),'娆¤缁?];
            % 灞堟洸杩愬姩                
                tic;
                app.Label_State.Text = "寮€濮?;
                while 1
                    pause(0.1);
                    t = ceil(toc*10);
                    target_angle = degree_go(t);
                    app.dr.set_angles([1,2], [target_angle,-target_angle], 10, app.f/2, 0);
                    [~,~,~] = receive_data_op(app);
                    disp(t);
                    if (app.IsStop == 1) || (toc >= T)
                        app.dr.estop(0);
                        app.IsPain = 1;
                        app.IsStop = 0;
                        break;
                    end
                end                
            % 鍒拌揪璁惧畾浣嶇疆锛屼繚鎸?0s
                app.Label_State.Text = "瀵规姉";
                app.dr.impedance_control_multi([1,2], app.state_now_motor.degree, [1,1], app.state_now_motor.torque, [ag_pw,ag_pw], [0.1,0.1]);
                
                tic
                while (toc < T_keep) 
                    [~,~,~] = receive_data_op(app);
                    pause(0.1);
                end
                app.dr.estop(0);
                
                degree_stop = round(mean(abs(app.state_now_motor.degree)));
                % degree_back = (degree_stop-degree_start)/2 * cos(pi/T * x ) + (degree_stop+degree_start)/2;    
                degree_back = [trace(end:-1:1), ones(1,10)*trace(1)];
            % 浼稿睍杩愬姩
                app.IsPain = -1;
                tic;
                app.Label_State.Text = "缁撴潫";
                while toc < T
                    pause(0.1);
                    t = ceil(toc*10);
                    target_angle = degree_back(t);
                    app.dr.set_angles([1,2], [target_angle,-target_angle], 10, app.f/2, 0);
                    [~,~,~] = receive_data_op(app);
                    disp(t);
                end            
            % 鍒拌揪鍒濆浣嶇疆锛屼紤鎭?s
                app.IsPain = 0;
                app.Label_State.Text = "浼戞伅";
                tic
                while toc < T_rest
                    [~,~,~] = receive_data_op(app);
                    pause(0.1);
                end

            end
            pause(1);
            app.dr.disable_angle_speed_torque_state(1);
            app.dr.disable_angle_speed_torque_state(2);
            flush(app.dr.dev);
        end
        
        function fuzzy_control(app)
            

            % 娣诲姞杈撳叆鍙橀噺 鐤肩棝绋嬪害
            app.fis = addInput(app.fis, [-1 1], 'Name', 'PainLevel'); % 杈撳叆鑼冨洿涓?鍒?00鎽勬皬搴?            % 娣诲姞闅跺睘搴﹀嚱鏁?- 涓嶇柤--寰堢柤
            app.fis = addMF(app.fis, 'PainLevel', 'zmf', [-1 -0.5], 'Name', 'NP');
            app.fis = addMF(app.fis, 'PainLevel', 'trimf', [-1 -0.5 0], 'Name', 'SP');
            app.fis = addMF(app.fis, 'PainLevel', 'trimf', [-0.5 0 0.5], 'Name', 'P');
            app.fis = addMF(app.fis, 'PainLevel', 'trimf', [0 0.5 1], 'Name', 'MP');
            app.fis = addMF(app.fis, 'PainLevel', 'smf', [0.5 1], 'Name', 'VP');

            % 娣诲姞杈撳叆鍙橀噺 鍏宠妭闃诲姏
            app.fis = addInput(app.fis, [0 1], 'Name', 'JointResistance'); % 杈撳叆鑼冨洿涓?鍒?00鎽勬皬搴?            % 娣诲姞闅跺睘搴﹀嚱鏁?- 鏃犻樆鍔?-澶ч樆鍔?            app.fis = addMF(app.fis, 'JointResistance', 'zmf', [0 0.25], 'Name', 'NR');
            app.fis = addMF(app.fis, 'JointResistance', 'trimf', [0 0.25 0.5], 'Name', 'SR');
            app.fis = addMF(app.fis, 'JointResistance', 'trimf', [0.25 0.5 0.75], 'Name', 'R');
            app.fis = addMF(app.fis, 'JointResistance', 'trimf', [0.5 0.75 1], 'Name', 'MR');
            app.fis = addMF(app.fis, 'JointResistance', 'smf', [0.75 1], 'Name', 'BR');

            % 娣诲姞杈撳嚭鍙橀噺 - 鍔犵儹鍣ㄥ己搴?            app.fis = addOutput(app.fis, [-6 6], 'Name', 'angle'); % 杈撳嚭鑼冨洿涓?鍒?
            % 娣诲姞闅跺睘搴﹀嚱鏁?- 浣庛€佷腑銆侀珮
            app.fis = addMF(app.fis, 'angle', 'zmf', [-6 -3], 'Name', 'BM');
            app.fis = addMF(app.fis, 'angle', 'trimf', [-6 -3 0], 'Name', 'SM');
            app.fis = addMF(app.fis, 'angle', 'trimf', [-3 0 3], 'Name', 'K');
            app.fis = addMF(app.fis, 'angle', 'trimf', [0 3 6], 'Name', 'SA');
            app.fis = addMF(app.fis, 'angle', 'smf', [3 6], 'Name', 'BA');


            % 瀹氫箟妯＄硦瑙勫垯锛堜娇鐢ㄦ暟鍊肩煩闃碉級
            rules = [
                1 1 5 1 1; % JointResistance=NR, PainLevel=NP => angle=BA
                1 2 5 1 1; % JointResistance=NR, PainLevel=SP => angle=BA
                1 3 4 1 1; % JointResistance=NR, PainLevel=P => angle=SA
                1 4 3 1 1; % JointResistance=NR, PainLevel=MP => angle=SM
                1 5 3 1 1; % JointResistance=NR, PainLevel=VP => angle=SM

                2 1 5 1 1; % JointResistance=SR, PainLevel=NP => angle=BA
                2 2 4 1 1; % JointResistance=SR, PainLevel=SP => angle=SA
                2 3 3 1 1; % JointResistance=SR, PainLevel=P => angle=K
                2 4 3 1 1; % JointResistance=SR, PainLevel=MP => angle=K
                2 5 3 1 1; % JointResistance=SR, PainLevel=VP => angle=SM

                3 1 5 1 1; % JointResistance=R, PainLevel=NP => angle=BA
                3 2 4 1 1; % JointResistance=R, PainLevel=SP => angle=SA
                3 3 3 1 1; % JointResistance=R, PainLevel=P => angle=K
                3 4 3 1 1; % JointResistance=R, PainLevel=MP => angle=SM
                3 5 2 1 1; % JointResistance=R, PainLevel=VP => angle=BM

                4 1 4 1 1; % JointResistance=MR, PainLevel=NP => angle=SA
                4 2 3 1 1; % JointResistance=MR, PainLevel=SP => angle=K
                4 3 3 1 1; % JointResistance=MR, PainLevel=P => angle=K
                4 4 3 1 1; % JointResistance=MR, PainLevel=MP => angle=SM
                4 5 2 1 1; % JointResistance=MR, PainLevel=VP => angle=BM

                5 1 3 1 1; % JointResistance=BR, PainLevel=NP => angle=SM
                5 2 3 1 1; % JointResistance=BR, PainLevel=SP => angle=SM
                5 3 3 1 1; % JointResistance=BR, PainLevel=P => angle=SM
                5 4 2 1 1; % JointResistance=BR, PainLevel=MP => angle=BM
                5 5 2 1 1  % JointResistance=BR, PainLevel=VP => angle=BM
                ];
            app.fis = addRule(app.fis, rules);
        end

        % 鑷€傚簲琚姩璁粌
        function train_mode_3(app)
            add_degree = [5.1487827 0.58418626 1.1090648 -2.4133179 -1.4525875 0.35588762 0.74423593 -3.0000007 3.0027387 3.9916439];
            train_mode = app.DropDown_TrainMode.Value;
            times = str2double(app.DropDown_Times.Value);
            speed_level = app.DropDown_Speed.Value;            
            degree_start_ = app.EditField_StartDegree.Value;            
            degree_end_ = app.state_degree_end;   
            degree_start = linkage2motor(app,degree_start_);
            degree_end = linkage2motor(app,degree_end_);
            T_keep = str2double(app.DropDown_KeepTime.Value);
            T_rest = str2double(app.DropDown_RestTime.Value);
            app.Train_config{app.Train_times} = struct("train_mode",train_mode, ...
                                                       "times",times, ...
                                                       "speed",speed_level, ...
                                                       "degree_start",[degree_start_,degree_start], ...
                                                       "degree_end",[degree_end_,degree_end], ...
                                                       "keep_time",T_keep, ...
                                                       "rest_time",T_rest, ...
                                                       "model",app.Model); 
            
            % 閫熷害涓夋。璋冭妭            
            switch speed_level
                case "缂撴參"
                    T = 20;
                case "閫備腑"
                    T = 15;
                case "蹇€?
                    T = 10;
            end
            
            count_painlabel = 0;
            update_time = 5;
            keep_total = [0,0,0];
            app.keep_total_save = [];

            x = 0:0.1:T+1;
            
            app.dr.enable_angle_speed_torque_state(1); % 寮€鍚疄鏃跺弽棣堜娇鑳?            app.dr.enable_angle_speed_torque_state(2); % 寮€鍚疄鏃跺弽棣堜娇鑳?            receive_data_op_clearBuff(app);
         
            for i = 1 : times
                fz_in_force1 = [];
                fz_in_force2 = [];
                fz_in_pain = [];
                fz_input = [];

                trace = app.Seg_trace_result(T, degree_start, degree_end);
                degree_go = [trace,ones(1,10)*trace(end)];
                % degree_go = (degree_end-degree_start)/2 * cos(pi/T * x + pi) + (degree_end+degree_start)/2;
                app.Label_Tips.Text = strcat('绗?,num2str(i),'娆¤缁?鏈€澶ц搴?',num2str(motor2linkage(app,degree_end)));
                app.IsPain = 0;               
                app.IsStop = 0;
                % 灞堟洸杩愬姩                
                tic;
                app.Label_State.Text = "寮€濮?;
                while 1
                    pause(0.1);
                    t = ceil(toc*10);
                    target_angle = degree_go(t);
                    app.dr.set_angles([1,2], [target_angle,-target_angle], 10, app.f/2, 0);
                    [~,~,svmrst] = receive_data_op(app);
                    if svmrst < 0 % svm杈撳嚭闃堝€?浣庝簬0鍒ゆ柇涓虹柤鐥?                        count_painlabel = count_painlabel + 1;
                    elseif svmrst >= 0
                        count_painlabel = 0;
                    end
                    if ~isnan(svmrst)
                        app.Gauge_SVM.Value = -svmrst;
                        disp(-svmrst);
                    end

                    keep = [app.IsStop == 1,toc >= T,count_painlabel == 5];
                    if max(keep)
                        app.dr.estop(0);
                        keep_total = keep_total + keep;
                        app.IsStop = 0;
                        app.IsPain = 1;
                        count_painlabel = 0;
                        app.Label_UpdateData.Text = num2str(keep_total);
                        break;
                    end
                end                 

% 鍒拌揪璁惧畾浣嶇疆锛屼繚鎸?0s
                app.Label_State.Text = "淇濇寔";
                tic
                while (toc < T_keep) 
                    [~,~,svmrst] = receive_data_op(app);
                    if ~isnan(svmrst)
                        app.Gauge_SVM.Value = -svmrst;
                        fz_in_pain = [fz_in_pain, -svmrst];
                    end
                    
                    pause(0.1);
                end
                fz_in_force1 = abs(app.Data_motor_1(end-20*T_keep:end, 3));
                fz_in_force2 = abs(app.Data_motor_2(end-20*T_keep:end, 3));
                fz_input = [mean(fz_in_pain), (mean(fz_in_force1) + mean(fz_in_force2))/10];

                degree_stop = round(mean(abs(app.state_now_motor.degree)));
                trace = app.Seg_trace_result(T, degree_stop, degree_start);
                degree_back = [trace,ones(1,10)*trace(end)];
                % degree_back = (degree_stop-degree_start)/2 * cos(pi/T * x ) + (degree_stop+degree_start)/2;  


% 杩斿洖鍒濆浣嶇疆
                app.Label_State.Text = "缁撴潫";
                app.IsPain = -1;
                tic;
                while 1
                    pause(0.1);
                    t = ceil(toc*10);
                    target_angle = degree_back(t);
                    app.dr.set_angles([1,2], [target_angle,-target_angle], 10, app.f/2, 0);
                    [~,~,svmrst] = receive_data_op(app);
                    if ~isnan(svmrst)
                        app.Gauge_SVM.Value = -svmrst;
                        disp(-svmrst);
                    end
                    
                    if (toc >= T)
                        app.dr.estop(0);
                        break;
                    end
                end 

% 鍒拌揪鍒濆浣嶇疆锛屼紤鎭?s
                app.IsPain = 0;
                app.Label_State.Text = "浼戞伅";
                tic
                while toc < T_rest
                    pause(0.1);
                    [~,~,svmrst] = receive_data_op(app);
                    if ~isnan(svmrst)
                        app.Gauge_SVM.Value = -svmrst;
                    end                    
                end 

                
                % outputAngle = evalfis(app.fis, fz_input);
                app.keep_total_save = [app.keep_total_save;[fz_input,outputAngle]];
                
                angle = motor2linkage(app,degree_end);
                angle = angle + add_degree(i); % outputAngle;
                degree_end = linkage2motor(app,angle);
                % if mod(i,update_time) == 0 % 姣忓線杩斾笁娆℃洿鏂颁竴娆″弬鏁?                %     if keep_total(1) >= update_time - 1
                %         angle = motor2linkage(app,degree_end);
                %         angle = angle - 2;
                %         degree_end = linkage2motor(app,angle);
                %     end
                %     if keep_total(2) >= update_time - 1
                %         angle = motor2linkage(app,degree_end);
                %         angle = angle + 1;
                %         if angle > (app.machine_maxdegree - 0.5)
                %             angle = app.machine_maxdegree - 0.5;
                %         end
                %         degree_end = linkage2motor(app,angle);
                %     end                   
                %     keep_total = [0,0,0];
                % end
            end
            pause(1);
            app.dr.disable_angle_speed_torque_state(1);
            app.dr.disable_angle_speed_torque_state(2);
            flush(app.dr.dev);
            app.state_degree_end = motor2linkage(app,degree_end);
        end
        
        % 鑷€傚簲瀵规姉璁粌
        function train_mode_4(app)
            train_mode = app.DropDown_TrainMode.Value;
            times = str2double(app.DropDown_Times.Value);
            speed_level = app.DropDown_Speed.Value;            
            degree_start_ = app.EditField_StartDegree.Value;            
            degree_end_ = app.state_degree_end; 
            degree_start = linkage2motor(app,degree_start_);
            degree_end = linkage2motor(app,degree_end_);
            T_keep = str2double(app.DropDown_KeepTime.Value);
            T_rest = str2double(app.DropDown_RestTime.Value);
            against_power = app.DropDown_AgainstPower.Value;
            app.Train_config{app.Train_times} = struct("train_mode",train_mode, ...
                                                       "times",times, ...
                                                       "speed",speed_level, ...
                                                       "degree_start",[degree_start_,degree_start], ...
                                                       "degree_end",[degree_end_,degree_end], ...
                                                       "keep_time",T_keep, ...
                                                       "rest_time",T_rest, ...
                                                       "against_power",against_power, ...
                                                       "model",app.Model); 
            
            % 閫熷害涓夋。璋冭妭            
            switch speed_level
                case "缂撴參"
                    T = 20;
                case "閫備腑"
                    T = 15;
                case "蹇€?
                    T = 10;
            end

            switch against_power
                case "杈冨皬"
                    ag_pw = 1;
                case "閫備腑"
                    ag_pw = 1.5;
                case "杈冨ぇ"
                    ag_pw = 2;
            end
            
            count_painlabel = 0;
            update_time = 5;
            keep_total = [0,0,0];
            app.keep_total_save = [];

            x = 0:0.1:T+1;
            
            app.dr.enable_angle_speed_torque_state(1); % 寮€鍚疄鏃跺弽棣堜娇鑳?            app.dr.enable_angle_speed_torque_state(2); % 寮€鍚疄鏃跺弽棣堜娇鑳?            receive_data_op_clearBuff(app);
         
            for i = 1 : times
                degree_go = (degree_end-degree_start)/2 * cos(pi/T * x + pi) + (degree_end+degree_start)/2;
                app.Label_Tips.Text = strcat('绗?,num2str(i),'娆¤缁?鏈€澶ц搴?',num2str(motor2linkage(app,degree_end)));
                app.IsPain = 0;               
                app.IsStop = 0;

                % 灞堟洸杩愬姩                
                tic;
                app.Label_State.Text = "寮€濮?;
                while 1
                    pause(0.1);
                    t = ceil(toc*10);
                    target_angle = degree_go(t);
                    app.dr.set_angles([1,2], [target_angle,-target_angle], 10, app.f/2, 0);
                    [~,~,svmrst] = receive_data_op(app);
                    if svmrst < 0 % svm杈撳嚭闃堝€?浣庝簬0.2鍒ゆ柇涓虹柤鐥?                        count_painlabel = count_painlabel + 1;
                    elseif svmrst >= 0
                        count_painlabel = 0;
                    end
                    if ~isnan(svmrst)
                        app.Gauge_SVM.Value = -svmrst;
                    end
                    disp(t);
                    keep = [app.IsStop == 1,toc >= T,count_painlabel == 5];
                    if max(keep)
                        app.dr.estop(0);
                        keep_total = keep_total + keep;
                        app.IsStop = 0;
                        app.IsPain = 1;
                        count_painlabel = 0;
                        app.Label_UpdateData.Text = num2str(keep_total);
                        break;
                    end
                end                 

% 鍒拌揪璁惧畾浣嶇疆锛屼繚鎸?0s
                app.Label_State.Text = "瀵规姉";
                app.dr.impedance_control_multi([1,2], app.state_now_motor.degree, [1,1], app.state_now_motor.torque, [ag_pw,ag_pw], [0.1,0.1]);
                tic
                while (toc < T_keep) 
                    [~,~,svmrst] = receive_data_op(app);
                    if ~isnan(svmrst)
                        app.Gauge_SVM.Value = -svmrst;
                    end
                    pause(0.1);
                end

                degree_stop = round(mean(abs(app.state_now_motor.degree)));
                degree_back = (degree_stop-degree_start)/2 * cos(pi/T * x ) + (degree_stop+degree_start)/2;  
                
% 杩斿洖鍒濆浣嶇疆
                app.Label_State.Text = "缁撴潫";
                app.IsPain = -1;
                tic;
                while 1
                    pause(0.1);
                    t = ceil(toc*10);
                    target_angle = degree_back(t);
                    app.dr.set_angles([1,2], [target_angle,-target_angle], 10, app.f/2, 0);
                    [~,~,svmrst] = receive_data_op(app);
                    if ~isnan(svmrst)
                        app.Gauge_SVM.Value = -svmrst;
                    end
                    disp(t);
                    if (toc >= T)
                        app.dr.estop(0);
                        break;
                    end
                end 

% 鍒拌揪鍒濆浣嶇疆锛屼紤鎭?s
                app.IsPain = 0;
                app.Label_State.Text = "浼戞伅";
                tic
                while toc < T_rest
                    pause(0.1);
                    [~,~,svmrst] = receive_data_op(app);
                    if ~isnan(svmrst)
                        app.Gauge_SVM.Value = -svmrst;
                    end                    
                end 

                app.keep_total_save = [app.keep_total_save;keep_total];
                
                if mod(i,update_time) == 0 % 姣忓線杩斾笁娆℃洿鏂颁竴娆″弬鏁?                    if keep_total(1) >= update_time - 1
                        angle = motor2linkage(app,degree_end);
                        angle = angle - 2;
                        degree_end = linkage2motor(app,angle);
                    end
                    if keep_total(2) >= update_time - 1
                        angle = motor2linkage(app,degree_end);
                        angle = angle + 1;
                        if angle > (app.machine_maxdegree - 0.5)
                            angle = app.machine_maxdegree - 0.5;
                        end
                        degree_end = linkage2motor(app,angle);
                    end                   
                    keep_total = [0,0,0];
                end
            end
            pause(1);
            app.dr.disable_angle_speed_torque_state(1);
            app.dr.disable_angle_speed_torque_state(2);
            flush(app.dr.dev);
            app.state_degree_end = motor2linkage(app,degree_end);
        end
        % 闈欐€佺壍寮曡缁?        function train_mode_5(app)
             app.IsStop = 0;
            % 璁剧疆鎷変几鍙傛暟
            train_mode = app.DropDown_TrainMode.Value;
            traction_force = app.DropDown_TractionForce.Value;
            traction_time = app.DropDown_TractionTime.Value;
            app.Train_config{app.Train_times} = struct("train_mode",train_mode, ...
                                                       "traction_force",traction_force, ...
                                                       "traction_time",traction_time); 
            %% 鍒拌揪鎸囧畾浣嶇疆鎷変几
            degree_end = app.EditField_EndDegree.Value;
            degree_end = linkage2motor(app,degree_end);
            % 鍥炲綊鍒濆浣嶇疆
            app.dr.set_angles(app.id, [degree_end,-degree_end], 2, 1, 1);
            app.dr.positions_done(app.id);
            % 娓呯┖鏁版嵁缂撳瓨
            flush(app.dr.dev);

            set_load = str2double(app.DropDown_TractionForce.Value);
            traction_keeptime = str2double(app.DropDown_TractionTime.Value);
            real_load=0;  %褰撳墠鐗靛紩鍔?            times_flag=0; %澧炲姞鐗靛紩琛岀▼鐨勬鏁?
            windowSize = 5;  %璁剧疆婊ゆ尝鍙傛暟
            b = (1/windowSize)*ones(1,windowSize);
            a=1;
            % 寮€鍚弽棣堥潤姝?0s
            write(app.Com_stretch,'o',"char"); % 寮€鍚弽棣?
            tic
            while (toc<10 && app.IsStop ==0)
                pause(0.01);
            end

            % 璇诲彇鎺ㄦ潌鍒濆鐘舵€?            initial_data=app.data_pos(end-49:end,:);
            initial_data_fit=filter(b,a,initial_data); %鍒嗗埆瀵?鍒楀垵濮嬫暟鎹潎鍊兼护娉?            ini_force_vec=mean(initial_data_fit(:,5:8));%涓€琛屽洓涓悜閲?
            % 姣?s妫€娴嬩竴娆″姏鐘舵€侊紝鏈揪闃堝€煎垯澧炲姞琛岀▼
            while (real_load<set_load && times_flag < 3 &&  app.IsStop == 0)
                write(app.Com_stretch,'3',"char"); % 鍔?                write(app.Com_stretch,'3',"char"); % 鍔?                write(app.Com_stretch,'3',"char"); % 鍔?                times_flag=times_flag+1;                
                tic
                while (toc<6 && app.IsStop == 0)
                    pause(0.01);
                end

                % 姣忔寰幆涓绠楁槸鍚﹁揪鍒拌姹傜姸鎬?                current_data=app.data_pos(end-49:end,:);
                current_data_fit=filter(b,a,current_data);
                cur_force_vec=mean(current_data_fit(:,5:8));% 缁撴灉涓轰竴琛屽洓涓悜閲?
                real_load=sum(cur_force_vec-ini_force_vec);

                % linshi=[times_flag,ini_force_vec, cur_force_vec,real_load];
                % disp(linshi);
            end
            
            % 杈惧埌璁惧畾鍔涙垨鏈€澶ц绋嬩繚鎸?0s
            tic
            while (toc<traction_keeptime &&  app.IsStop == 0)
                pause(0.01);
            end
            current_data=app.data_pos(end-49:end,:);
            current_data_fit=filter(b,a,current_data);
            app.actuators_max_movment=mean(current_data_fit(:,1:4));

            % 鎺ㄦ潌褰掗浂
            write(app.Com_stretch,'1',"char"); % 褰掗浂
            tic
            while (toc<5 &&  app.IsStop == 0)
                pause(0.01);
            end

            % 缁撴潫Arduino璇诲彇骞堕噸缃爣蹇椾綅
            write(app.Com_stretch,'c',"char"); % 缁撴潫鍙嶉
            pause(0.5);
            app.stopflag=''; %閲嶇疆鏍囧織浣?            if (app.Com_stretch.NumBytesAvailable>0) %娓呯┖涓插彛
                flush(app.Com_stretch);
            end
        end

        % 鑷€傚簲鍏呮皵鍔熻兘
        function train_mode_6(app)
            app.IsStop = 0;
            % 璁剧疆鎷変几鍙傛暟
            write(app.Com_stretch,'o',"char"); % 寮€鍚弽棣?
            app.Stretch_State.Text='鍙嶉寮€鍚?;
            
            % 鍑嗗璋冩暣
            app.Stretch_State.Text = sprintf('鍑嗗涓?);
            tic
            while (toc<5 &&  app.IsStop == 0)
                % read_Arduino(app);
                % Plot_Stretch(app);
                % Plot_Sensor(app);
                pause(0.01);
            end

           % 澶ц吙鑷€傚簲鍏呮皵
           app.Stretch_State.Text = sprintf('澶ц吙鍏呮皵');
           Thigh_limit=[9 9 5]; %澶ц吙鍏呮皵涓婇檺
           write(app.Com_stretch,'5',"char"); % 澶ц吙鍏呮皵
           tic
            while( app.IsStop == 0 && toc<8)
                inflation_time=toc;
                app.Stretch_State.Text = sprintf('鍏呮皵鏃堕棿锛?f ',inflation_time);

                % 澶ц吙鍏呮皵鍒ゆ柇
                cur_data_press=app.data_press(end-30:end,:); %浼犳劅鍣ㄥ綋鍓?0鐐规暟鎹?                cur_press_vec=mean(cur_data_press); %骞冲潎鍚庤浆鎹负鍚戦噺
                Thigh_press=cur_press_vec(1:3); %寰楀埌澶ц吙褰撳墠鍘嬪姏

                if sum(Thigh_press>=Thigh_limit)==3||sum(Thigh_press>app.Press_limit(1:3))>=1
                    Mark_press=Thigh_press;
                    break;
                end
            end
            write(app.Com_stretch,'6',"char"); % 澶ц吙淇濇寔
            app.Stretch_State.Text = sprintf('澶ц吙鍏呮皵缁撴潫');
            tic
            while toc<1
                pause(0.01);
            end

            % 灏忚吙鑷€傚簲鍏呮皵
            app.Stretch_State.Text = sprintf('灏忚吙鍏呮皵');
            calf_limit=[5 5 5 5]; %灏忚吙鍏呮皵涓婇檺
            write(app.Com_stretch,'8',"char"); % 灏忚吙鍏呮皵
            tic
            while( app.IsStop == 0 && toc<8)
                inflation_time=toc;
                app.Stretch_State.Text = sprintf('鍏呮皵鏃堕棿锛?f ',inflation_time);

                % 灏忚吙鍏呮皵鍒ゆ柇
                cur_data_press=app.data_press(end-30:end,:); %浼犳劅鍣ㄥ綋鍓?0鐐规暟鎹?                cur_press_vec=mean(cur_data_press); %骞冲潎鍚庤浆鎹负鍚戦噺
                calf_press=cur_press_vec(5:8); %寰楀埌灏忚吙鍓嶄晶褰撳墠鍘嬪姏

                if sum(calf_press>=calf_limit)>=3||sum(calf_press>app.Press_limit(5:8))>=1
                    Mark_press=[Mark_press,calf_press];
                    break;
                end
            end
            

            write(app.Com_stretch,'9',"char"); % 灏忚吙淇濇寔
            app.Stretch_State.Text = sprintf('灏忚吙鍏呮皵缁撴潫');
            
            % 鍏呮皵瀹屾瘯缁撴潫鍙嶉
            write(app.Com_stretch,'c',"char"); % 缁撴潫鍙嶉
            app.Stretch_State.Text=sprintf('鍏呮皵瀹屾瘯锛氱敤鏃?%f ',inflation_time);
            pause(0.5);
            app.stopflag=''; %閲嶇疆鏍囧織浣?            if (app.Com_stretch.NumBytesAvailable>0) %娓呯┖涓插彛
                flush(app.Com_stretch);
            end
        end
        function callbackFcn2(app,~,~)       
            % 璇诲彇鎺ㄦ潌鏁版嵁
            for i=1:2
                readbuff=readline(app.Com_stretch);
                actuators_back=double(split(readbuff," "));
                app.data_pos=[app.data_pos;actuators_back'];
            % 璇诲彇鍘嬪姏浼犳劅鍣?                readbuff=readline(app.Com_stretch);
                sensor_back=double(split(readbuff," "));
                sensor_back=(sensor_back*9.8)/1000;
                app.data_press=[app.data_press;sensor_back'];
            end

            Plot_Stretch(app);
            Plot_Sensor(app);
            pause(0.01);

        end
        % 鎺ㄦ潌缁樺浘        
        function Plot_Stretch(app)

                app.Buff_motor2 =[flip(app.data_pos);app.Buff_motor2];                         
                plot(app.Stretch_pos,(flip(mean(app.Buff_motor2(1:100,1:4),2))),'LineWidth',3.5,'Color',[0.8500 0.3250 0.0980]);%[0 0.4470 0.7410]  [0.9290 0.6940 0.1250]
                hold(app.Stretch_pos,"on")
                plot(app.Stretch_pos,flip(app.Buff_motor2(1:100,1)));
                hold(app.Stretch_pos,"on")
                plot(app.Stretch_pos,flip(app.Buff_motor2(1:100,2)));
                hold(app.Stretch_pos,"on")
                plot(app.Stretch_pos,flip(app.Buff_motor2(1:100,3)));
                hold(app.Stretch_pos,"on")
                plot(app.Stretch_pos,flip(app.Buff_motor2(1:100,4)));
                hold(app.Stretch_pos,"off")

                % app.Buff_motor2 =flip(app.data_pos);                  
                plot(app.Stretch_force,(flip(sum(app.Buff_motor2(1:100,5:8),2))),'LineWidth',3.5,'Color',[0.8500 0.3250 0.0980]);%,'Marker','*'
                hold(app.Stretch_force,"on")
                plot(app.Stretch_force,flip(app.Buff_motor2(1:100,5)));
                hold(app.Stretch_force,"on")
                plot(app.Stretch_force,flip(app.Buff_motor2(1:100,6)));
                hold(app.Stretch_force,"on")
                plot(app.Stretch_force,flip(app.Buff_motor2(1:100,7)));
                hold(app.Stretch_force,"on")
                plot(app.Stretch_force,flip(app.Buff_motor2(1:100,8)));
                hold(app.Stretch_force,"off")

            % end
        end
                %% 浼犳劅鍣ㄧ粯鍥?       
        function Plot_Sensor(app)
            % i = length(app.data_press(:,1));
            % if i > 10

                % app.Buff_motor = [[flip(app.data_pos(:,1)),flip(app.data_pos(:,2)),flip(app.data_pos(:,3)),flip(app.data_pos(:,4))];app.Buff_motor(1:end-i,:)];
                app.Buff_motor3 =[flip(app.data_press);app.Buff_motor3];
                % plot(app.Press_Thigh_left,flip(app.Buff_motor3(1:100,1)));
                if(app.Buff_motor3(1,1)>=app.Press_limit(1))
                    app.Lamp_1.Color="1,0,0";
                elseif(app.Buff_motor3(1,1)>=app.Press_well(1))
                    app.Lamp_1.Color="0.00,0.4470,0.7410";
                else
                    app.Lamp_1.Color="0.40,0.90,0.00";
                end

                % plot(app.Press_Thigh_right,flip(app.Buff_motor3(1:100,2)));
                if(app.Buff_motor3(1,2)>=app.Press_limit(2))
                    app.Lamp_2.Color="1,0,0";
                elseif(app.Buff_motor3(1,2)>=app.Press_well(2))
                    app.Lamp_2.Color="0.00,0.4470,0.7410";
                else
                    app.Lamp_2.Color="0.40,0.90,0.00";
                end

                % plot(app.Press_Thigh_front,flip(app.Buff_motor3(1:100,3)));
                if(app.Buff_motor3(1,3)>=app.Press_limit(3))
                    app.Lamp_3.Color="1,0,0";
                elseif(app.Buff_motor3(1,3)>=app.Press_well(3))
                    app.Lamp_3.Color="0.00,0.4470,0.7410";
                else
                    app.Lamp_3.Color="0.40,0.90,0.00";
                end
                % plot(app.Press_Calf_back,flip(app.Buff_motor3(1:100,4)));
                if(app.Buff_motor3(1,4)>=app.Press_limit(4))
                    app.Lamp_4.Color="1,0,0";
                elseif(app.Buff_motor3(1,4)>=app.Press_well(4))
                    app.Lamp_4.Color="0.00,0.4470,0.7410";
                else
                    app.Lamp_4.Color="0.40,0.90,0.00";
                end
                % plot(app.Press_Calf_L_F,flip(app.Buff_motor3(1:100,5)));
                if(app.Buff_motor3(1,5)>=app.Press_limit(5))
                    app.Lamp_5.Color="1,0,0";
                elseif(app.Buff_motor3(1,5)>=app.Press_well(5))
                    app.Lamp_5.Color="0.00,0.4470,0.7410";
                else
                    app.Lamp_5.Color="0.40,0.90,0.00";
                end
                % plot(app.Press_Calf_L_B,flip(app.Buff_motor3(1:100,6)));
                if(app.Buff_motor3(1,6)>=app.Press_limit(6))
                    app.Lamp_6.Color="1,0,0";
                elseif(app.Buff_motor3(1,6)>=app.Press_well(6))
                    app.Lamp_6.Color="0.00,0.4470,0.7410";
                else
                    app.Lamp_6.Color="0.40,0.90,0.00";
                end
                % plot(app.Press_Calf_R_F,flip(app.Buff_motor3(1:100,7)));
                if(app.Buff_motor3(1,7)>=app.Press_limit(7))
                    app.Lamp_7.Color="1,0,0";
                elseif(app.Buff_motor3(1,7)>=app.Press_well(7))
                    app.Lamp_7.Color="0.00,0.4470,0.7410";
                else
                    app.Lamp_7.Color="0.40,0.90,0.00";
                end
                % plot(app.Press_Calf_R_B,flip(app.Buff_motor3(1:100,8)));
                if(app.Buff_motor3(1,8)>=app.Press_limit(8))
                    app.Lamp_8.Color="1,0,0";
                elseif(app.Buff_motor3(1,8)>=app.Press_well(8))
                    app.Lamp_8.Color="0.00,0.4470,0.7410";
                else
                    app.Lamp_8.Color="0.40,0.90,0.00";
                end

        end
                %% 浼犳劅鍣ㄧ粯鍥?        function press_N=Trans_Press(app,read_data)
            voltage=(5000*read_data)/1024; %寮曡剼鎹㈢畻鐢靛帇
            press=(10000*voltage)/3300; %鐢靛帇鎹㈢畻鍔涳紙g锛?            press_N=press*9.8/1000;  %鍔涘崟浣峠鎹㈢畻涓篘
        end

        function read_Arduino(app)
            while(app.Com_stretch.NumBytesAvailable==0)
            end
            %% 璇诲彇鎺ㄦ潌鏁版嵁
            readbuff=readline(app.Com_stretch);
            app.stopflag=readbuff;
            if  (app.stopflag~="s t o p")
                actuators_back=double(split(readbuff," "));
                app.data_pos=[app.data_pos;actuators_back'];
            
                %% 璇诲彇鍘嬪姏浼犳劅鍣?                readbuff=readline(app.Com_stretch);
                sensor_back=double(split(readbuff," "));
                sensor_back=(sensor_back*9.8)/1000;
                app.data_press=[app.data_press;sensor_back'];
            end
        end
        
        function Trace = Seg_trace_result(app,T,Q0,Qf)
            % 鏃堕棿鍙傛暟
     
            T1 = 0.25*T; % 绗竴娈典簲娆″椤瑰紡鐨勬椂闂撮暱搴?            T2 = 0.5*T; % 绾挎€ф鐨勬椂闂撮暱搴?            T3 = 0.25*T; % 绗簩娈典簲娆″椤瑰紡鐨勬椂闂撮暱搴?
            % 鏋勫缓鏃堕棿鐭╅樀 T
            T11 = [T1^5, T1^4, T1^3, T1^2, T1;
                5*T1^4, 4*T1^3, 3*T1^2, 2*T1, 1;
                20*T1^3, 12*T1^2, 6*T1, 2, 0;
                60*T1^2, 24*T1, 6, 0, 0];

            T12 = [1, 0, -1, 0;
                0, -1, 0, 0;
                0, 0, 0, 0;
                0, 0, 0, 0];

            T22 = [0, T2, 1, 0;
                0, 1, 0, 0;
                0, 0, 0, 0;
                0, 0, 0, 0;
                0, 0, 0, T3^5];

            T23 = [0, 0, 0, 0, -1;
                0, 0, 0, -1, 0;
                0, 0, -2, 0, 0;
                0, -6, 0, 0, 0;
                T3^4, T3^3, T3^2, T3, 1];

            T31 = [0, 0, 0, 0, 0;
                0, 0, 0, 0, 0;
                0, 0, 0, 0, 0;
                0, 0, 0, 0, 1;
                0, 0, 0, 2, 0];

            T32 = [0, 0, 0, 5*T3^4;
                0, 0, 0, 20*T3^3;
                1, 0, 0, 0;
                0, 0, 0, 0;
                0, 0, 0, 0];

            T33 = [4*T3^3, 3*T3^2, 2*T3, 1, 0;
                12*T3^2, 6*T3, 2, 0, 0;
                0, 0, 0, 0, 0;
                0, 0, 0, 0, 0;
                0, 0, 0, 0, 0];

            % 鏋勫缓鎬荤殑鏃堕棿鐭╅樀 T
            T = [T11, T12, zeros(4,5);
                zeros(5,5), T22, T23;
                T31, T32, T33];

            % 鏋勫缓浣嶇疆鍚戦噺 theta
            theta = [0, 0, 0, 0, 0, 0, 0, 0, Qf, 0, 0, Q0, 0, 0]';
            % 姹傝В绯绘暟鐭╅樀 B
            B = T \ theta;

            % 鎻愬彇鍚勬鐨勭郴鏁?            coeffs1 = B(1:6)';
            coeffs2 = B(7:8)';
            coeffs3 = B(9:14)';

            % 鐢熸垚杞ㄨ抗
            t1 = linspace(0.1, T1, round(10*T1));
            t2 = linspace(T1+0.1, T1 + T2, round(10*T2));
            t3 = linspace(T1 + T2+0.1, T1 + T2 + T3, round(10*T3));

            trace1 = polyval(coeffs1, t1);
            trace2 = polyval(coeffs2, t2 - T1); % 璋冩暣鏃堕棿鍋忕Щ
            trace3 = polyval(coeffs3, t3 - (T1 + T2));

            Trace = [trace1,trace2,trace3];
        end
    end

    methods (Access = public)

        function msg_patient_save(app,patient)
            app.Patient = patient;
            app.path_SaveFolder = strcat('Save_Data\',app.Patient.date,app.Patient.name);
            mkdir(app.path_SaveFolder);
        end
    end

    % Callbacks that handle component events
    methods (Access = private)

        function ok = initMotorDevice(app)
            ok = false;
            uart_baudrate = 115200;
            try
                if ~isempty(app.dr) && ismethod(app.dr,"is_connected") && app.dr.is_connected()
                    ok = true;
                    return;
                end
            catch
                app.dr = [];
            end

            try
                app.dr = DrEmpower_can("COM4", uart_baudrate, 0.5);
                flush(app.dr.dev);
                app.dr.set_mode(0, 1);
                pause(0.5);
                app.dr.set_mode(0, 2);
                app.dr.disable_angle_speed_torque_state(1);
                app.dr.disable_angle_speed_torque_state(2);
                app.dr.set_state_feedback_rate_ms(1, 50);
                app.dr.set_state_feedback_rate_ms(2, 50);
                app.dr.set_torque_limit(0,54);
                app.dr.set_speed_limit(0,5);
                configureCallback(app.dr.dev,"byte",256,@app.callbackFcn);
                ok = true;
            catch ME
                app.dr = [];
                app.Label_State.Text = "电机CAN连接失败";
                app.Label_Tips.Text = "无法连接COM4电机控制器：" + string(ME.message);
            end
        end

        function cleanupDevices(app)
            try
                if ~isempty(app.dr)
                    app.dr.set_mode(0, 1);
                    if isprop(app.dr,"dev") && ~isempty(app.dr.dev)
                        configureCallback(app.dr.dev,"off");
                    end
                end
            catch
            end
            try
                if app.IsOP == 1 && ~isempty(app.op_clent)
                    write(app.op_clent,'stop');
                end
            catch
            end
        end

        % Code that executes after component creation
        function startupFcn(app)
%             img = imread('Exoskeleton.jpg'); % 鏇挎崲涓轰綘鐨勫浘鐗囨枃浠惰矾寰?%             % app.image_exoskeleton = image(img);
%             imshow(img,'Parent',app.Heat_Press);
%             axis(app.Heat_Press, 'image'); % 璁剧疆杞翠负鍥惧儚澶у皬
%             xlim(app.Heat_Press, [0 size(img, 2)]); % 璋冩暣 X 杞寸殑鑼冨洿
%             ylim(app.Heat_Press, [0 size(img, 1)]); % 璋冩暣 Y 杞寸殑鑼冨洿

            app.Label_Tips.Text = "训练开始前请先登记患者信息，然后点击启动设备连接硬件";
            app.Label_State.Text = "未连接设备";
            app.dr = [];


        end

        % Button pushed function: Button_MsgPatient
        function Button_MsgPatientPushed(app, event)
            app.Button_MsgPatient.Enable = "off";
            app.DialogApp = msg_patient(app);            
        end

        % Button pushed function: Button_InitAll
        function Button_InitAllPushed(app, event)
            app.Label_Tips.Text = "鍚姩涓?;
            EnableOff(app);

            if ~initMotorDevice(app)
                EnableOn(app);
                return;
            end

            
%             % 鎵撳紑涓插彛
%             try
%                 app.Com_stretch=serialport("COM10",9600,"Timeout",5);
%                 % app.Drive_State.Text = [sprintf('\n涓插彛鎵撳紑鎴愬姛\n')];
%                 app.Stretch_State.Text = '涓插彛鎵撳紑鎴愬姛';
%             catch
%                 % app.Drive_State.Text = [sprintf('\n涓插彛鎵撳紑澶辫触\n')];
%                 app.Stretch_State.Text = '涓插彛鎵撳紑澶辫触';
%                 delete(app.Com_stretch);
%             end
%             configureTerminator(app.Com_stretch,"CR/LF");
%             if (app.Com_stretch.NumBytesAvailable>0)
%                 flush(app.Com_stretch);
%             end

%             %pause(8);
%             %configureCallback(app.Com_stretch,"byte",62,@app.callbackFcn2);
%             configureCallback(app.Com_stretch,"byte",125,@app.callbackFcn2);
% 

            % 鍒濆鍖杘p
            
            try
                init_clent(app);
                app.IsOP = 1;
            catch
                app.Label_Tips.Text = "鏈繛鎺ヨ倢鐢典华锛岄儴鍒嗗姛鑳芥棤娉曚娇鐢?;
                app.Label_StateOP.Text = "鏈繛鎺ヨ倢鐢典华锛岄儴鍒嗗姛鑳芥棤娉曚娇鐢?;
                app.IsOP = 0;
                pause(2);
            end
            app.Label_StateOP.Text = "鍒濆鍖栧畬鎴?;

            % 鏇存柊op閰嶇疆
            pause(0.5);
            if app.IsOP == 1
                app.commands_op = strcat('config',',',app.config_op.ID);
                app.TextArea_send.Value{1} = '';
                app.TextArea_send.Value = app.commands_op;
                send_command(app);
                pause(0.1);
                receive_command(app);
                app.TextArea_receive.Value{1} = '';
                app.TextArea_receive.Value = app.receives_op;
                config_update(app);
            end

            % 鍒濆鍖?鍚姩)CAN鎺у埗鐩?
            app.Label_Tips.Text = "璁?澶?鍚?鍔?涓?.";
            pause(1)
            app.Label_Tips.Text = "璁?澶?鍚?鍔?涓?..";
            pause(1)


            % 璁惧鏈€澶х墿鐞嗚搴︽娴?            app.Label_State.Text = "2.璁惧杩愬姩妫€娴?;
            app.Label_Tips.Text = "2绉掑悗灏嗚繘琛岃澶囪繍鍔ㄦ娴嬶紝璇峰仛濂藉噯澶?;
            pause(2);
            app.Label_Tips.Text = (['寮€濮嬫娴嬭澶囨槸鍚﹀瓨鍦ㄥ崱椤?,newline,'璇峰皢璁惧灞堟洸鑷虫渶澶ц寖鍥达紝瀹屾垚鍚庣偣鍑汇€愮粨鏉熴€?]);
            
            app.dr.set_mode(0, 1);
            pause(0.5);
            app.dr.enable_angle_speed_torque_state(1);
            app.dr.enable_angle_speed_torque_state(2);
            maxdegree = 0;
            while 1
                if mean(abs(app.state_now_motor.degree)) > maxdegree
                    maxdegree = mean(abs(app.state_now_motor.degree));
                end
                if app.IsStop == 1
                    maxdegree = floor(motor2linkage(app,maxdegree)); % 璁剧疆璁惧鏈€澶у眻鏇茶搴?                    maxdegree = double(maxdegree);
                    app.EditField_EndDegree.Limits = [5,maxdegree];
                    app.EditField_StartDegree.Limits = [5,maxdegree];
                    app.machine_maxdegree = maxdegree;
                    break;
                end
                pause(0.01);
            end
            app.IsStop = 0;
            app.dr.disable_angle_speed_torque_state(1);
            app.dr.disable_angle_speed_torque_state(2);
            flush(app.dr.dev);
            

            app.Label_Tips.Text = "璁惧杩愬姩妫€娴嬬粨鏉?;
            pause(1);
            app.Label_Tips.Text = "璁惧宸插惎鍔紝璇锋偅鑰呯┛鎴村ソ璁惧";
            receive_data_op_pre(app);
            pause(1);
            app.Label_State.Text = "璁惧宸插氨缁?;
            pause(1);
            app.Label_Tips.Text = "璇风偣鍑烩€樿缁冨墠灞堟洸瑙掑害妫€娴嬧€?;
            
            
            EnableOn(app);
  
        end

        % Button pushed function: Button_CloseDevice
        function Button_CloseDevicePushed(app, event)
            cleanupDevices(app);
            if isempty(app.Data_motor_1)
                delete(app);
                return;
            end
            if isempty(app.Patient)
                delete(app)
                return;
            end
            Save_dataset(app);
            Save_data_all(app);
            delete(app);            

   
        end

        % Button pushed function: Button_MaxAngle_Before
        function Button_MaxAngle_BeforePushed(app, event)
            EnableOff(app)
            % -----------鏍囧畾鏈€澶ф棆杞搴?-----------
            app.Label_State.Text = "搴峰鍓嶆渶澶ц搴︽爣璁?;
            app.Label_Tips.Text = (['璇锋偅鑰呬富鍔ㄥ集鏇茶啙鍏宠妭,杈惧埌鑷繁鐨勬渶澶ц搴﹀悗鏀炬澗',newline,'鐐瑰嚮銆愮粨鏉熴€戝畬鎴愭渶澶ц搴︽祴閲?']);
            app.dr.set_mode(0, 1);


            receive_data_op_clearBuff(app);

            app.Data_motor_1 = [];
            app.Data_motor_2 = [];

            
            app.dr.enable_angle_speed_torque_state(1);
            app.dr.enable_angle_speed_torque_state(2);
            app.max_degree=0;
            app.state_now_motor.degree = [0,0];
            while 1
                if mean(abs(app.state_now_motor.degree)) > app.max_degree
                    app.max_degree = mean(abs(app.state_now_motor.degree));
                end

                if app.IsStop == 1
                    app.dr.set_mode(0, 2);
                    break;
                end
                receive_data_op(app);
                pause(0.01);
            end
            app.IsStop = 0;

            app.dr.disable_angle_speed_torque_state(1);
            app.dr.disable_angle_speed_torque_state(2);
            flush(app.dr.dev);

            

            app.Label_Tips.Text = "鏈€澶ц搴︽祴閲忓畬鎴?;
            pause(1);
            app.Label_Tips.Text = "璇风偣鍑烩€滃紑濮嬫爣瀹氣€濊繘琛岃鍔ㄨ缁冩椿鍔ㄨ寖鍥存爣瀹?;

            linkage_angle = motor2linkage(app,app.max_degree);

            app.Label_MaxDegree.Text = ["璁粌鍓嶆渶澶ц搴?",round(linkage_angle)];

            app.Patient.pre_angle_auto = linkage_angle;
            app.EditField_StartDegreeCalib.Value = double(round(linkage_angle)) - 30;
            app.Label_State.Text = " ";

            EnableOn(app);
        end

        % Button pushed function: Button_MaxAngle_After
        function Button_MaxAngle_AfterPushed(app, event)
            EnableOff(app);
            % -----------鏍囧畾鏈€澶ф棆杞搴?-----------
            app.Label_Tips.Text='璇峰尰甯堝崗鍔╂墭浣忔偅鑲㈠苟缂撴參鏀句笅';
            pause(3);
            app.dr.set_mode(0, 1);

            app.Label_State.Text = "搴峰鍚庢渶澶ц搴︽爣璁?;
            app.Label_Tips.Text = (['璇锋偅鑰呬富鍔ㄥ集鏇茶啙鍏宠妭,杈惧埌鑷繁鐨勬渶澶ц搴﹀悗鏀炬澗',newline,'鐐瑰嚮銆愮粨鏉熴€戝畬鎴愭渶澶ц搴︽祴閲?]);
            app.max_degree=0;
            
            app.Data_motor_1 = [];
            app.Data_motor_2 = [];

            app.dr.enable_angle_speed_torque_state(1);
            app.dr.enable_angle_speed_torque_state(2);
            while 1
                if mean(abs(app.state_now_motor.degree)) > app.max_degree
                    app.max_degree = mean(abs(app.state_now_motor.degree));
                end

                if app.IsStop == 1
                    app.dr.set_mode(0, 2);
                    break;
                end
                pause(0.01);
            end
            app.IsStop = 0;

            app.dr.disable_angle_speed_torque_state(1);
            app.dr.disable_angle_speed_torque_state(2);
            flush(app.dr.dev);



            app.Label_Tips.Text = "鏈€澶ц搴︽祴閲忓畬鎴?;
            pause(1);
            app.Label_Tips.Text = ['璇锋偅鑰呰劚涓嬪悍澶嶈澶?,newline,'鍖诲笀浣跨敤閲忚鍣ㄨ繘琛岃啙鍏宠妭灞堟洸瑙掑害娴嬮噺锛屽苟灏嗙粨鏋滆緭鍏ュ璇濇'];

            after_degree = inputdlg('鍖诲笀浣跨敤閲忚鍣ㄦ祴閲忚缁冨悗鎮ｈ€呰啙鍏宠妭鏈€澶у眻鏇茶搴?,'杈撳叆',[1 30],{'0'});
            app.Patient.after_angle = str2double(after_degree);
            
            linkage_angle=motor2linkage(app,app.max_degree);

            app.Patient.after_angle_auto = linkage_angle;

            app.Label_MaxDegree.Text =["璁粌鍚庢渶澶ц搴?",round(linkage_angle)];
            app.Label_Tips.Text='璇峰叧闂澶?;
            EnableOn(app);
        end

        % Button pushed function: Button_StartCalib
        function Button_StartCalibPushed(app, event)
            EnableOff(app);
            app.dr.set_mode(0, 2);
            app.Panel_Calibration.Enable = 'on';
            app.EditField_StartDegreeCalib.Enable = 'off';
            app.Button_StartCalib.Enable = 'off';
            app.Button_DegreePlus.Enable = 'off';

            % -----------鍖荤敓杈呭姪琚姩杩愬姩------------
            app.Calib_times = app.Calib_times+1; %鏍囧畾娆℃暟+1
            app.Calib_config{app.Calib_times} =  struct("degree_end",0, ...
                                                        "degree_start",app.EditField_StartDegreeCalib.Value, ...
                                                        "speed",0, ...
                                                        "times",3);

            % 璁剧疆寮€濮嬩綅缃?            degree_start = app.EditField_StartDegreeCalib.Value;
            degree_start = linkage2motor(app,degree_start);
            app.Label_Tips.Text = "澶嶄綅涓紝璇风瓑寰?;

            app.dr.enable_angle_speed_torque_state(1); % 寮€鍚疄鏃跺弽棣堜娇鑳?            app.dr.enable_angle_speed_torque_state(2); % 寮€鍚疄鏃跺弽棣堜娇鑳?
            % 鐢垫満鍥炲綊寮€濮嬩綅缃?            app.dr.set_angles(app.id, [degree_start,-degree_start], 1, 0.5, 1);
            while abs(mean(abs(app.state_now_motor.degree)) - degree_start) > 0.5
                pause(0.1);
            end
            pause(1);
            % 鎻愮ず璇?            app.Label_Tips.Text = "3绉掑悗璁惧灏嗛€愭笎澧炲姞瑙掑害杩涜琚姩杩愬姩";
            pause(3);
            app.Button_DegreePlus.Enable = 'on';
            app.Label_State.Text = "鍏宠妭娲诲姩鑼冨洿鏍囧畾";
            app.Label_Tips.Text = (['璇峰尰鐢熼€愭鐐瑰嚮鍔犲彿,鐩村埌杈惧埌鎮ｈ€呰啙鍏宠妭涓夊垎鐤肩棝',newline,'鐐瑰嚮銆愮粨鏉熴€戣繘鍏ユ爣瀹氳繃绋?]);
            app.Data_motor_1 = [];
            app.Data_motor_2 = [];
            app.Data_op = [];
            receive_data_op_clearBuff(app);

            % 閫愭鍔犺搴?            while 1
                if app.IsPlus == 1
                    app.IsPlus = 0;
                    app.dr.step_angles(app.id, [5,-5], 2, 0.2, 1);
                end
                if app.IsStop == 1
                    app.IsPain = 1;
                    app.IsStop = 0;
                    Degree_max = mean(abs(app.state_now_motor.degree));
                    app.dr.set_mode(0,2);
                    app.Button_DegreePlus.Enable = 'off';
                    app.Label_Tips.Text = "杩涘叆鏍囧畾杩囩▼";
                    break;
                end
                [~,~,~] = receive_data_op(app);
                pause(0.1);
            end

            % 淇濇寔瑙掑害
            tic;
            while toc < 5
                [~,~,~] = receive_data_op(app);
                pause(0.1);
            end
            app.IsPain = -1;
            % 鐢垫満鍥炲綊寮€濮嬩綅缃?            app.dr.set_angles(app.id, [degree_start,-degree_start], 2, 0.2, 1);
            while abs(mean(abs(app.state_now_motor.degree)) - degree_start) > 0.5
                [~,~,~] = receive_data_op(app);
                pause(0.1);                
            end
            pause(1);
            app.dr.set_mode(0,2);

% 姝ｅ紡鏍囧畾鏁版嵁
            if app.IsOP == 1
                T = 15;       
                x = 0:0.1:T+1;
                angle_go = (Degree_max-degree_start)/2 * cos(pi/T * x + pi) + (Degree_max+degree_start)/2;
                angle_back = (Degree_max-degree_start)/2 * cos(pi/T * x ) + (Degree_max+degree_start)/2;
                pause(2);
                receive_data_op_clearBuff(app);
                app.Data_motor_1 = [];
                app.Data_motor_2 = [];
                app.Data_op = [];
    
                for i = 1 : 1
                    % 灞堟洸
                    app.IsPain = 0;
                    tic;
                    while toc < T
                        pause(0.1);
                        t = ceil(toc*10);
                        target_degree = angle_go(t);
                        app.dr.set_angles([1,2], [target_degree,-target_degree], 1, app.f/2, 0);
                        [~,~,~] = receive_data_op(app);
                        disp(t);
                    end
                    toc;
                    % 淇濇寔
                    app.IsPain = 1;
                    tic;
                    while toc < 5
                        pause(0.1);
                        [~,~,~] = receive_data_op(app);
                    end
                    toc;
                    % 鐢垫満鍥炲綊寮€濮嬩綅缃?                    app.IsPain = -1;
                    tic;
                    while toc < T
                        pause(0.1);
                        t = ceil(toc*10);
                        target_degree = angle_back(t);
                        app.dr.set_angles([1,2], [target_degree,-target_degree], 1, app.f/2, 0);
                        [~,~,~] = receive_data_op(app);
                        disp(t);
                    end
                    toc;
                    % 鐢垫満淇濇寔鍦ㄥ紑濮嬩綅缃?                    app.IsPain = 0;
                    tic;
                    while toc < 5
                        pause(0.1);
                        [~,~,~] = receive_data_op(app);
                    end
                    toc;
                end
            end

            % 缁撴潫鏍囧畾 淇濆瓨鏁版嵁
            pause(1);
            app.dr.disable_angle_speed_torque_state(1);
            app.dr.disable_angle_speed_torque_state(2);
            flush(app.dr.dev);

            Save_dataset(app);
            app.Calib_data{app.Calib_times} = {app.Data_motor_1,app.Data_motor_2,app.Data_op};           
            calib_data = app.Calib_data{app.Calib_times};
            save(strcat(app.name_save_mat,'Calibration',app.Patient.name),"calib_data");
            app.Label_Tips.Text = "鏍囧畾杩愬姩璁板綍瀹屾垚";
            app.Label_DegreeCalib.Text = ["涓夊垎鐤肩棝瑙掑害:",round(motor2linkage(app,mean(Degree_max)))];
            pause(1);
            app.Label_Tips.Text = ['璇疯缃悍澶嶈缁冪殑妯″紡鍜屽弬鏁?,newline,'鐐瑰嚮銆愬紑濮嬭缁冦€戣繘琛屽悍澶嶈缁?];

            % 鏁版嵁鏁寸悊
            app.Model = TrainingModel(app,app.Data_op);

            app.state_degree_end = motor2linkage(app,mean(Degree_max));
            app.Calib_config{app.Calib_times}.degree_end = mean(Degree_max);
            app.Calib_config{app.Calib_times}.speed = max(app.Data_motor_1(:,2));
            app.Adapt_times = 0;
            app.Adapt_data = [];
            app.EditField_StartDegreeCalib.Enable = 'on';
            app.Button_StartCalib.Enable = 'on';
            app.Button_DegreePlus.Enable = 'on';
            EnableOn(app);
        end

        % Button pushed function: Button_DegreePlus
        function Button_DegreePlusPushed(app, event)
            app.IsPlus = 1;
        end

        % Value changed function: DropDown_TrainMode
        function DropDown_TrainModeValueChanged(app, event)
            value = app.DropDown_TrainMode.Value;
           
            switch value
                case '璇烽€夋嫨'
                    app.Button_TrainStart.Enable = 'off';
                case '琚姩璁粌'
                    app.DropDown_TractionForce.Enable = 'off';
                    app.DropDown_TractionTime.Enable = 'off';
                    app.DropDown_AgainstPower.Enable = 'off';
                    app.DropDown_RestTime.Enable = 'on';
                    app.DropDown_Speed.Enable = 'on';
                    app.DropDown_KeepTime.Enable = 'on';
                    app.DropDown_Times.Enable = 'on';
                    app.EditField_StartDegree.Enable = 'on';
                    app.EditField_EndDegree.Enable = 'on';
                    app.Button_TrainStart.Enable = 'on';
                    app.Label_Tips.Text = "纭濂借缁冨弬鏁板悗鐐瑰嚮鈥樺紑濮嬭缁冣€欒繘琛岃缁?;
                case '瀵规姉璁粌'
                    app.DropDown_TractionForce.Enable = 'off';
                    app.DropDown_TractionTime.Enable = 'off';
                    app.DropDown_RestTime.Enable = 'on';
                    app.DropDown_Speed.Enable = 'on';
                    app.DropDown_KeepTime.Enable = 'on';
                    app.DropDown_Times.Enable = 'on';
                    app.EditField_StartDegree.Enable = 'on';
                    app.DropDown_AgainstPower.Enable = 'on';
                    app.EditField_EndDegree.Enable = 'on';
                    app.Button_TrainStart.Enable = 'on';
                    app.Label_Tips.Text = "纭濂借缁冨弬鏁板悗鐐瑰嚮鈥樺紑濮嬭缁冣€欒繘琛岃缁?;
                case '鑷€傚簲琚姩璁粌'
                    app.DropDown_TractionForce.Enable = 'off';
                    app.DropDown_TractionTime.Enable = 'off';
                    app.DropDown_AgainstPower.Enable = 'off';
                    app.EditField_EndDegree.Enable = 'off';
                    app.DropDown_RestTime.Enable = 'on';
                    app.DropDown_Speed.Enable = 'on';
                    app.DropDown_KeepTime.Enable = 'on';
                    app.DropDown_Times.Enable = 'on';
                    app.EditField_StartDegree.Enable = 'on';
                    if app.IsOP == 0
                        app.Button_TrainStart.Enable = 'off';
                        app.Label_Tips.Text = "鏈娇鐢ㄨ倢鐢典华锛屾棤娉曡繘琛岃璁粌";
                    else
                        app.Button_TrainStart.Enable = 'on';
                        app.Label_Tips.Text = "纭濂借缁冨弬鏁板悗鐐瑰嚮鈥樺紑濮嬭缁冣€欒繘琛岃缁?;
                    end
                case '鑷€傚簲瀵规姉璁粌'
                    app.DropDown_TractionForce.Enable = 'off';
                    app.DropDown_TractionTime.Enable = 'off';
                    app.DropDown_RestTime.Enable = 'on';
                    app.DropDown_AgainstPower.Enable = 'on';
                    app.DropDown_Speed.Enable = 'on';
                    app.DropDown_KeepTime.Enable = 'on';
                    app.DropDown_Times.Enable = 'on';
                    app.EditField_StartDegree.Enable = 'on';
                    app.EditField_EndDegree.Enable = 'off';
                    if app.IsOP == 0
                        app.Button_TrainStart.Enable = 'off';
                        app.Label_Tips.Text = "鏈娇鐢ㄨ倢鐢典华锛屾棤娉曡繘琛岃璁粌";
                    else
                        app.Button_TrainStart.Enable = 'on';
                        app.Label_Tips.Text = "纭濂借缁冨弬鏁板悗鐐瑰嚮鈥樺紑濮嬭缁冣€欒繘琛岃缁?;
                    end
                case '闈欐€佺壍寮?
                    app.Button_TrainStart.Enable='on';
                    app.DropDown_TractionForce.Enable = 'on';
                    app.DropDown_TractionTime.Enable = 'on';
                    app.DropDown_RestTime.Enable = 'off';
                    app.DropDown_AgainstPower.Enable = 'off';
                    app.DropDown_Speed.Enable = 'off';
                    app.DropDown_KeepTime.Enable = 'off';
                    app.DropDown_Times.Enable = 'off';
                    app.EditField_StartDegree.Enable = 'off';
                    app.EditField_EndDegree.Enable = 'on';
                    app.Label_Tips.Text = "纭濂借缁冨弬鏁板悗鐐瑰嚮鈥樺紑濮嬭缁冣€欒繘琛岃缁?;
                case '鑷€傚簲鍏呮皵'
                    app.Button_TrainStart.Enable='on';
                    app.DropDown_RestTime.Enable = 'off';
                    app.DropDown_AgainstPower.Enable = 'off';
                    app.DropDown_Speed.Enable = 'off';
                    app.DropDown_KeepTime.Enable = 'off';
                    app.DropDown_Times.Enable = 'off';
                    app.EditField_StartDegree.Enable = 'off';
                    app.EditField_EndDegree.Enable = 'off';
                    app.DropDown_TractionForce.Enable = 'off';
                    app.DropDown_TractionTime.Enable = 'off';
                    app.Label_Tips.Text = "纭濂借缁冨弬鏁板悗鐐瑰嚮鈥樺紑濮嬭缁冣€欒繘琛岃缁?;
            end
        end

        % Button pushed function: Button_TrainStart
        function Button_TrainStartPushed(app, event)
            EnableOff(app);
            app.dr.set_mode(0, 2);
            app.Train_times = app.Train_times+1; %璁粌娆℃暟+1

            
            degree_start = app.EditField_StartDegree.Value;
            degree_start = linkage2motor(app,degree_start);
            % 鍥炲綊鍒濆浣嶇疆
            app.dr.set_angles(app.id, [degree_start,-degree_start], 2, 1, 1);
            app.dr.positions_done(app.id);
            % 娓呯┖鏁版嵁缂撳瓨
            flush(app.dr.dev);
            app.IsStop = 0;
            app.IsPain = 0;
            app.Data_motor_1 = [];
            app.Data_motor_2 = [];
            app.Data_op = [];
            app.keep_total_save = [];
            app.Label_State.Text = " "; % 娓呯┖鎻愮ず
            app.Lamp.Color = [0 1 0];
            app.Label_Tips.Text = "寮€濮嬭缁?;
            switch app.DropDown_TrainMode.Value
                case "琚姩璁粌"
                    train_mode_1(app);
                    app.Train_data{app.Train_times} = {app.Data_motor_1,app.Data_motor_2,app.Data_op};
                case "瀵规姉璁粌"
                    train_mode_2(app);
                    app.Train_data{app.Train_times} = {app.Data_motor_1,app.Data_motor_2,app.Data_op};
                case "鑷€傚簲琚姩璁粌"
                    fuzzy_control(app);
                    train_mode_3(app);
                    app.Train_data{app.Train_times} = {app.Data_motor_1,app.Data_motor_2,app.Data_op,app.keep_total_save};
                case "鑷€傚簲瀵规姉璁粌"
                    fuzzy_control(app);
                    train_mode_4(app);
                    app.Train_data{app.Train_times} = {app.Data_motor_1,app.Data_motor_2,app.Data_op,app.keep_total_save};
                case "闈欐€佺壍寮?
                    train_mode_5(app);
                    % app.Train_data{app.Train_times} = {app.Data_motor_1,app.Data_motor_2,app.Data_op,app.keep_total_save};
                case "鑷€傚簲鍏呮皵"
                    train_mode_6(app);
                    % app.Train_data{app.Train_times} = {app.Data_motor_1,app.Data_motor_2,app.Data_op,app.keep_total_save};
                case "璇烽€夋嫨"
                    app.Label_Tips.Text = "鏈€夋嫨璁粌妯″紡锛岃閲嶈瘯";
                    return;
            end
            app.Lamp.Color = [0.8 0.8 0.8];
% 淇濆瓨鍗曠粍璁粌鏁版嵁
            Save_dataset(app)
            Save_data_part(app);
            app.Label_Tips.Text = ['鍗曟璁粌缁撴潫锛岀‘璁ゅ弬鏁板悗鐐瑰嚮銆愬紑濮嬭缁冦€戝彲鍐嶆璁粌',newline,'璁粌瀹屾瘯璇疯繘琛屾媺浼歌缁冿紝鎴栫偣鍑汇€愯缁冨悗灞堟洸瑙掑害妫€娴嬨€戦€€鍑?];

            EnableOn(app);
        end

        % Callback function
        function Button_StretchTozeroPushed(app, event)
            EnableOff(app);

            write(app.Com_stretch,'7',"char"); % 鎺ㄦ潌褰掗浂
            back = read(app.Com_stretch,1,"char");

            pause(3);
            EnableOn(app);
        end

        % Callback function
        function Button_StretchMinusPushed(app, event)
            write(app.Com_stretch,'4',"char"); % 鏀剁缉鎸囦护涓哄彂閫?
            back = read(app.Com_stretch,1,"char");
        end

        % Callback function
        function Button_StretchOncePushed(app, event)
            EnableOff(app);

            % 璋冩暣鎷変几浣嶇疆
            app.Label_Tips.Text=['璇峰尰甯堝崗鍔╂墭浣忔偅鑲㈠苟缂撴參鏀句笅锛岃皟鏁磋嚦鑷劧鏀炬澗鐨勪綅缃?,newline,'鎸夈€愮粨鏉熴€戦敭寮€濮嬫媺浼?];
            pause(3);
            app.order_send = app.funcMotor.order_close();
            transceive(app,app.order_send); 
            while 1
                app.order_send = app.funcMotor.order_state();
                transceive(app,app.order_send);
                if app.IsStop == 1
                    break;
                end
                PlotData_motor(app);
                pause(0.001);
            end
            app.IsStop = 0;  
            pause(1)

            % 寮€濮嬫媺浼告帶鍒?            % 娓呯┖缂撳瓨鍖?            % app.Label_Tips.Text = [sprintf('\n %d',app.Com_stretch.NumBytesAvailable)]; %涓插彛涓墿浣欐湭鍙戦€佷俊鎭暟閲?            % pause(2);

            if (app.Com_stretch.NumBytesAvailable>0)
                flush(app.Com_stretch);
            end
           % 鍙戦€佹帶鍒舵寚浠?骞剁‘璁?              app.Label_Tips.Text="鍗冲皢寮€濮嬫媺浼?;
              pause(1);
              app.Label_Tips.Text="鎷変几杩涜涓?;
              write(app.Com_stretch,'2',"char"); %%%ardiuno
              back=read(app.Com_stretch,1,"char");
            
            pause(10);%%%%%%%%%%%
            app.Label_Tips.Text=['鐐瑰嚮銆愭帹鏉嗗綊闆躲€戝洖褰掗浂浣嶏紝鎴栦娇鐢ㄣ€?銆戙€?銆戣嚜鐢辫皟鏁?,newline,'缁撴潫璁粌璇峰皢鎺ㄦ潌褰掗浂鍚庣偣鍑汇€愯缁冨悗灞堟洸瑙掑害妫€娴嬨€?];
            
            EnableOn(app);
        end

        % Callback function
        function Button_StretchPlusPushed(app, event)
            write(app.Com_stretch,'3',"char"); % 鎷変几鎸囦护涓哄彂閫?
            back = read(app.Com_stretch,1,"char");
        end

        % Button pushed function: Button_Finish
        function Button_FinishPushed(app, event)
            app.IsStop = 1;
        end

        % Close request function: UIFigure
        function UIFigureCloseRequest(app, event)
            cleanupDevices(app);
            
            if isempty(app.Data_motor_1)
                delete(app);
                return;
            end
            if isempty(app.Patient)
                delete(app);
                return;
            end
            Save_dataset(app);
            Save_data_all(app);
            delete(app);
        end

        % Button pushed function: Button_stretch_2
        function Button_stretch_2Pushed(app, event)
            write(app.Com_stretch,'o',"char"); % 寮€鍚弽棣?
            app.Stretch_State.Text='鍙嶉寮€鍚?;
            
            %% 鍑嗗璋冩暣
            app.Stretch_State.Text = sprintf('鍑嗗涓?);
            tic
            while (toc<5 && app.stopflag~="s t o p")
                % read_Arduino(app);
                % Plot_Stretch(app);
                % Plot_Sensor(app);
                pause(0.01);
            end

           %% 澶ц吙鑷€傚簲鍏呮皵
           app.Stretch_State.Text = sprintf('澶ц吙鍏呮皵');
           Thigh_limit=[9 9 5]; %澶ц吙鍏呮皵涓婇檺
           write(app.Com_stretch,'5',"char"); % 澶ц吙鍏呮皵
           tic
            while(app.stopflag~="s t o p" && toc<10 && app.inflation_flag==0)
                inflation_time=toc;
                app.Stretch_State.Text = sprintf('鍏呮皵鏃堕棿锛?f ',inflation_time);

                %% 澶ц吙鍏呮皵鍒ゆ柇
                cur_data_press=app.data_press(end-30:end,:); %浼犳劅鍣ㄥ綋鍓?0鐐规暟鎹?                cur_press_vec=mean(cur_data_press); %骞冲潎鍚庤浆鎹负鍚戦噺
                Thigh_press=cur_press_vec(1:3); %寰楀埌澶ц吙褰撳墠鍘嬪姏

                if sum(Thigh_press>=Thigh_limit)==3||sum(Thigh_press>app.Press_limit(1:3))>=1
                    Mark_press=Thigh_press;
                    break;
                end
            end
            write(app.Com_stretch,'6',"char"); % 澶ц吙淇濇寔
            app.Stretch_State.Text = sprintf('澶ц吙鍏呮皵缁撴潫');
            tic
            while toc<1
                pause(0.01);
            end

            %% 灏忚吙鑷€傚簲鍏呮皵
            app.Stretch_State.Text = sprintf('灏忚吙鍏呮皵');
            calf_limit=[5 5 5 5]; %灏忚吙鍏呮皵涓婇檺
            write(app.Com_stretch,'8',"char"); % 灏忚吙鍏呮皵
            tic
            while(app.stopflag~="s t o p" && toc<10 && app.inflation_flag==0)
                inflation_time=toc;
                app.Stretch_State.Text = sprintf('鍏呮皵鏃堕棿锛?f ',inflation_time);

                %% 灏忚吙鍏呮皵鍒ゆ柇
                cur_data_press=app.data_press(end-30:end,:); %浼犳劅鍣ㄥ綋鍓?0鐐规暟鎹?                cur_press_vec=mean(cur_data_press); %骞冲潎鍚庤浆鎹负鍚戦噺
                calf_press=cur_press_vec(5:8); %寰楀埌灏忚吙鍓嶄晶褰撳墠鍘嬪姏

                if sum(calf_press>=calf_limit)>=3||sum(calf_press>app.Press_limit(5:8))>=1
                    Mark_press=[Mark_press,calf_press];
                    break;
                end
            end
            

            write(app.Com_stretch,'9',"char"); % 灏忚吙淇濇寔
            app.Stretch_State.Text = sprintf('灏忚吙鍏呮皵缁撴潫');
            
            %% 鍏呮皵瀹屾瘯缁撴潫鍙嶉
            write(app.Com_stretch,'c',"char"); % 缁撴潫鍙嶉
            app.Stretch_State.Text=sprintf('鍏呮皵瀹屾瘯锛氱敤鏃?%f ',inflation_time);
            pause(0.5);
            app.stopflag=''; %閲嶇疆鏍囧織浣?            if (app.Com_stretch.NumBytesAvailable>0) %娓呯┖涓插彛
                flush(app.Com_stretch);
            end

            % Save_dataset(app);  %鎸夊綋鍓嶆椂闂翠繚瀛樻暟鎹?            % save_buff=app.data_pos;
            % save_buff2=app.data_press;
            % save(app.name_save,"save_buff","save_buff2");
        end

        % Button pushed function: Button_stretch_3
        function Button_stretch_3Pushed(app, event)
             app.IsStop = 0;
            % 璁剧疆鎷変几鍙傛暟
           % train_mode = app.DropDown_TrainMode.Value;
            traction_force = app.DropDown_TractionForce.Value;
            traction_time = app.DropDown_TractionTime.Value;
            %app.Train_config{app.Train_times} = struct("train_mode",train_mode, ...
                                                     %  "traction_force",traction_force, ...
                                                   %    "traction_time",traction_time); 
            set_load = str2double(app.DropDown_TractionForce.Value);
            traction_keeptime = str2double(app.DropDown_TractionTime.Value);
            real_load=0;  %褰撳墠鐗靛紩鍔?            times_flag=0; %澧炲姞鐗靛紩琛岀▼鐨勬鏁?
            windowSize = 5;  %璁剧疆婊ゆ尝鍙傛暟
            b = (1/windowSize)*ones(1,windowSize);
            a=1;
            % 寮€鍚弽棣堥潤姝?0s
            write(app.Com_stretch,'o',"char"); % 寮€鍚弽棣?
            tic
            while (toc<10 && app.IsStop ==0)
                pause(0.01);
            end

            % 璇诲彇鎺ㄦ潌鍒濆鐘舵€?            initial_data=app.data_pos(end-49:end,:);
            initial_data_fit=filter(b,a,initial_data); %鍒嗗埆瀵?鍒楀垵濮嬫暟鎹潎鍊兼护娉?            ini_force_vec=mean(initial_data_fit(:,5:8));%涓€琛屽洓涓悜閲?
            % 姣?s妫€娴嬩竴娆″姏鐘舵€侊紝鏈揪闃堝€煎垯澧炲姞琛岀▼
            while (real_load<set_load && times_flag < 3 &&  app.IsStop == 0)
                write(app.Com_stretch,'3',"char"); % 鍔?                write(app.Com_stretch,'3',"char"); % 鍔?                write(app.Com_stretch,'3',"char"); % 鍔?                times_flag=times_flag+1;                
                tic
                while (toc<6 && app.IsStop == 0)
                    pause(0.01);
                end

                % 姣忔寰幆涓绠楁槸鍚﹁揪鍒拌姹傜姸鎬?                current_data=app.data_pos(end-49:end,:);
                current_data_fit=filter(b,a,current_data);
                cur_force_vec=mean(current_data_fit(:,5:8));% 缁撴灉涓轰竴琛屽洓涓悜閲?
                real_load=sum(cur_force_vec-ini_force_vec);

                % linshi=[times_flag,ini_force_vec, cur_force_vec,real_load];
                % disp(linshi);
            end
            
            % 杈惧埌璁惧畾鍔涙垨鏈€澶ц绋嬩繚鎸?0s
            tic
            while (toc<traction_keeptime &&  app.IsStop == 0)
                pause(0.01);
            end
            current_data=app.data_pos(end-49:end,:);
            current_data_fit=filter(b,a,current_data);
            app.actuators_max_movment=mean(current_data_fit(:,1:4));

            % 鎺ㄦ潌褰掗浂
            write(app.Com_stretch,'1',"char"); % 褰掗浂
            tic
            while (toc<5 &&  app.IsStop == 0)
                pause(0.01);
            end

            % 缁撴潫Arduino璇诲彇骞堕噸缃爣蹇椾綅
            write(app.Com_stretch,'c',"char"); % 缁撴潫鍙嶉
            pause(0.5);
            app.stopflag=''; %閲嶇疆鏍囧織浣?            if (app.Com_stretch.NumBytesAvailable>0) %娓呯┖涓插彛
                flush(app.Com_stretch);
            end
        end

        % Button pushed function: Button_stretch
        function Button_stretchPushed(app, event)
            write(app.Com_stretch,'1',"char"); % 鎺ㄦ潌褰掗浂
            % back=read(app.Com_stretch,1,"char");
        end

        % Button pushed function: Button_StretchPlus_2
        function Button_StretchPlus_2Pushed(app, event)
            write(app.Com_stretch,'3',"char"); % 鍔?            % back=read(app.Com_stretch,1,"char");
        end

        % Button pushed function: Button_StretchMinus_2
        function Button_StretchMinus_2Pushed(app, event)
            write(app.Com_stretch,'4',"char"); % 鍑?            % back=read(app.Com_stretch,1,"char");
        end

        % Selection changed function: ButtonGroup
        function ButtonGroupSelectionChanged(app, event)
            selectedButton = app.ButtonGroup.SelectedObject;
            switch selectedButton.Text
                case "鍏呮皵"
                    write(app.Com_stretch,'5',"char"); % 澶ц吙鍏呮皵
                case "淇濇寔"
                    write(app.Com_stretch,'6',"char"); % 澶ц吙淇濇寔
                case "鏀炬皵"
                    write(app.Com_stretch,'7',"char"); % 澶ц吙鏀炬皵
                otherwise
                    %     msgbox("鍔熻兘鏃犳晥锛岃閲嶆柊閫夋嫨","Notice","error");
            end
        end

        % Selection changed function: ButtonGroup_2
        function ButtonGroup_2SelectionChanged(app, event)
            selectedButton = app.ButtonGroup_2.SelectedObject;
            switch selectedButton.Text
                case "鍏呮皵"
                    write(app.Com_stretch,'8',"char"); % 灏忚吙鍏呮皵
                case "淇濇寔"
                    write(app.Com_stretch,'9',"char"); % 灏忚吙淇濇寔
                case "鏀炬皵"
                    write(app.Com_stretch,'q',"char"); % 灏忚吙鏀炬皵
                otherwise
                    %     msgbox("鍔熻兘鏃犳晥锛岃閲嶆柊閫夋嫨","Notice","error");
            end
        end

        % Button pushed function: Button_stretch_4
        function Button_stretch_4Pushed(app, event)
            app.IsStop=0;
            if (app.Com_stretch.NumBytesAvailable>0) %娓呯┖涓插彛
                flush(app.Com_stretch);
            end
            %% 寮€鍚弽棣?            write(app.Com_stretch,'o',"char"); % 寮€鍚弽棣?
            %tic
            %while (toc<10 && app.IsStop==0)
            while(app.IsStop==0)
                pause(0.01);
                disp(app.Com_stretch.NumBytesAvailable);
            end          
            
            %% 缁撴潫Arduino璇诲彇骞堕噸缃爣蹇椾綅
            write(app.Com_stretch,'c',"char"); % 缁撴潫鍙嶉
            pause(0.5);
            if (app.Com_stretch.NumBytesAvailable>0) %娓呯┖涓插彛
                flush(app.Com_stretch);
            end

            app.IsStop=0;

        end
    end

    % Component initialization
    methods (Access = private)

        % Create UIFigure and components
        function createComponents(app)

            % Create UIFigure and hide until all components are created
            app.UIFigure = uifigure('Visible', 'off');
            app.UIFigure.Position = [15 55 1514 765];
            app.UIFigure.Name = 'MATLAB App';
            app.UIFigure.CloseRequestFcn = createCallbackFcn(app, @UIFigureCloseRequest, true);

            % Create TabGroup
            app.TabGroup = uitabgroup(app.UIFigure);
            app.TabGroup.Position = [3 236 681 525];

            % Create Tab_TwoMotor
            app.Tab_TwoMotor = uitab(app.TabGroup);
            app.Tab_TwoMotor.Title = '璁惧鎺у埗';

            % Create Panel_TrainSet
            app.Panel_TrainSet = uipanel(app.Tab_TwoMotor);
            app.Panel_TrainSet.Title = '搴峰璁粌';
            app.Panel_TrainSet.Position = [4 13 673 187];

            % Create Label
            app.Label = uilabel(app.Panel_TrainSet);
            app.Label.HorizontalAlignment = 'right';
            app.Label.FontSize = 16;
            app.Label.Position = [216 127 39 22];
            app.Label.Text = '娆℃暟';

            % Create Label_2
            app.Label_2 = uilabel(app.Panel_TrainSet);
            app.Label_2.HorizontalAlignment = 'right';
            app.Label_2.FontSize = 16;
            app.Label_2.Position = [357 127 69 22];
            app.Label_2.Text = '璧峰瑙掑害';

            % Create EditField_StartDegree
            app.EditField_StartDegree = uieditfield(app.Panel_TrainSet, 'numeric');
            app.EditField_StartDegree.Limits = [5 85];
            app.EditField_StartDegree.FontSize = 16;
            app.EditField_StartDegree.Position = [433 127 28 22];
            app.EditField_StartDegree.Value = 30;

            % Create Label_3
            app.Label_3 = uilabel(app.Panel_TrainSet);
            app.Label_3.HorizontalAlignment = 'right';
            app.Label_3.FontSize = 16;
            app.Label_3.Position = [355 79 69 22];
            app.Label_3.Text = '缁堟瑙掑害';

            % Create EditField_EndDegree
            app.EditField_EndDegree = uieditfield(app.Panel_TrainSet, 'numeric');
            app.EditField_EndDegree.Limits = [5 85];
            app.EditField_EndDegree.FontSize = 16;
            app.EditField_EndDegree.Position = [433 79 27 22];
            app.EditField_EndDegree.Value = 60;

            % Create Button_TrainStart
            app.Button_TrainStart = uibutton(app.Panel_TrainSet, 'push');
            app.Button_TrainStart.ButtonPushedFcn = createCallbackFcn(app, @Button_TrainStartPushed, true);
            app.Button_TrainStart.FontSize = 16;
            app.Button_TrainStart.FontWeight = 'bold';
            app.Button_TrainStart.Enable = 'off';
            app.Button_TrainStart.Position = [18 26 184 56];
            app.Button_TrainStart.Text = '寮€濮嬭缁?;

            % Create DropDown_Times
            app.DropDown_Times = uidropdown(app.Panel_TrainSet);
            app.DropDown_Times.Items = {'1', '2', '3', '5', '10', '15', '20', '25', '30'};
            app.DropDown_Times.FontSize = 16;
            app.DropDown_Times.Position = [271 127 64 22];
            app.DropDown_Times.Value = '1';

            % Create Label_11
            app.Label_11 = uilabel(app.Panel_TrainSet);
            app.Label_11.HorizontalAlignment = 'right';
            app.Label_11.FontSize = 16;
            app.Label_11.Position = [218 79 37 22];
            app.Label_11.Text = '閫熷害';

            % Create DropDown_Speed
            app.DropDown_Speed = uidropdown(app.Panel_TrainSet);
            app.DropDown_Speed.Items = {'缂撴參', '閫備腑', '蹇€?};
            app.DropDown_Speed.FontSize = 16;
            app.DropDown_Speed.Position = [271 79 64 22];
            app.DropDown_Speed.Value = '閫備腑';

            % Create Label_16
            app.Label_16 = uilabel(app.Panel_TrainSet);
            app.Label_16.HorizontalAlignment = 'right';
            app.Label_16.FontSize = 16;
            app.Label_16.Position = [18 127 64 22];
            app.Label_16.Text = '璁粌妯″紡';

            % Create DropDown_TrainMode
            app.DropDown_TrainMode = uidropdown(app.Panel_TrainSet);
            app.DropDown_TrainMode.Items = {'璇烽€夋嫨', '琚姩璁粌', '瀵规姉璁粌', '鑷€傚簲琚姩璁粌', '鑷€傚簲瀵规姉璁粌', '闈欐€佺壍寮?, '鑷€傚簲鍏呮皵'};
            app.DropDown_TrainMode.ValueChangedFcn = createCallbackFcn(app, @DropDown_TrainModeValueChanged, true);
            app.DropDown_TrainMode.FontSize = 16;
            app.DropDown_TrainMode.Position = [98 127 104 22];
            app.DropDown_TrainMode.Value = '璇烽€夋嫨';

            % Create Label_17
            app.Label_17 = uilabel(app.Panel_TrainSet);
            app.Label_17.HorizontalAlignment = 'right';
            app.Label_17.FontSize = 16;
            app.Label_17.Position = [218 16 37 42];
            app.Label_17.Text = {'瀵规姉'; '鍔涢噺'};

            % Create DropDown_AgainstPower
            app.DropDown_AgainstPower = uidropdown(app.Panel_TrainSet);
            app.DropDown_AgainstPower.Items = {'杈冨皬', '閫備腑', '杈冨ぇ'};
            app.DropDown_AgainstPower.FontSize = 16;
            app.DropDown_AgainstPower.Position = [271 26 64 22];
            app.DropDown_AgainstPower.Value = '閫備腑';

            % Create Label_19
            app.Label_19 = uilabel(app.Panel_TrainSet);
            app.Label_19.HorizontalAlignment = 'right';
            app.Label_19.FontSize = 16;
            app.Label_19.Position = [503 70 74 42];
            app.Label_19.Text = {'淇濇寔/瀵规姉'; '鏃堕棿(绉?'};

            % Create DropDown_KeepTime
            app.DropDown_KeepTime = uidropdown(app.Panel_TrainSet);
            app.DropDown_KeepTime.Items = {'5', '10', '15'};
            app.DropDown_KeepTime.FontSize = 16;
            app.DropDown_KeepTime.Position = [593 80 64 22];
            app.DropDown_KeepTime.Value = '10';

            % Create Label_20
            app.Label_20 = uilabel(app.Panel_TrainSet);
            app.Label_20.HorizontalAlignment = 'right';
            app.Label_20.FontSize = 16;
            app.Label_20.Position = [365 26 53 22];
            app.Label_20.Text = '鐗靛紩鍔?;

            % Create DropDown_TractionForce
            app.DropDown_TractionForce = uidropdown(app.Panel_TrainSet);
            app.DropDown_TractionForce.Items = {'10', '15', '20', '25', '30', '35'};
            app.DropDown_TractionForce.FontSize = 16;
            app.DropDown_TractionForce.Position = [425 26 57 22];
            app.DropDown_TractionForce.Value = '10';

            % Create Label_21
            app.Label_21 = uilabel(app.Panel_TrainSet);
            app.Label_21.HorizontalAlignment = 'right';
            app.Label_21.FontSize = 16;
            app.Label_21.Position = [497 26 96 22];
            app.Label_21.Text = '鎷変几鏃堕棿(绉?';

            % Create DropDown_TractionTime
            app.DropDown_TractionTime = uidropdown(app.Panel_TrainSet);
            app.DropDown_TractionTime.Items = {'1', '5', '10', '15'};
            app.DropDown_TractionTime.FontSize = 16;
            app.DropDown_TractionTime.Position = [600 26 58 22];
            app.DropDown_TractionTime.Value = '1';

            % Create Label_22
            app.Label_22 = uilabel(app.Panel_TrainSet);
            app.Label_22.HorizontalAlignment = 'right';
            app.Label_22.FontSize = 16;
            app.Label_22.Position = [514 117 53 42];
            app.Label_22.Text = {'浼戞伅鏃?; '闂?绉?'};

            % Create DropDown_RestTime
            app.DropDown_RestTime = uidropdown(app.Panel_TrainSet);
            app.DropDown_RestTime.Items = {'5', '10', '15'};
            app.DropDown_RestTime.FontSize = 16;
            app.DropDown_RestTime.Position = [593 126 64 22];
            app.DropDown_RestTime.Value = '10';

            % Create Panel_Calibration
            app.Panel_Calibration = uipanel(app.Tab_TwoMotor);
            app.Panel_Calibration.Title = '鏍囧畾';
            app.Panel_Calibration.Position = [4 207 674 90];

            % Create Button_StartCalib
            app.Button_StartCalib = uibutton(app.Panel_Calibration, 'push');
            app.Button_StartCalib.ButtonPushedFcn = createCallbackFcn(app, @Button_StartCalibPushed, true);
            app.Button_StartCalib.FontSize = 16;
            app.Button_StartCalib.FontWeight = 'bold';
            app.Button_StartCalib.Position = [7 19 139 38];
            app.Button_StartCalib.Text = '寮€濮嬫爣瀹?;

            % Create Label_14
            app.Label_14 = uilabel(app.Panel_Calibration);
            app.Label_14.HorizontalAlignment = 'right';
            app.Label_14.FontSize = 16;
            app.Label_14.Position = [347 29 69 22];
            app.Label_14.Text = '璧峰瑙掑害';

            % Create EditField_StartDegreeCalib
            app.EditField_StartDegreeCalib = uieditfield(app.Panel_Calibration, 'numeric');
            app.EditField_StartDegreeCalib.Limits = [5 85];
            app.EditField_StartDegreeCalib.FontSize = 16;
            app.EditField_StartDegreeCalib.Position = [420 29 58 22];
            app.EditField_StartDegreeCalib.Value = 30;

            % Create Button_DegreePlus
            app.Button_DegreePlus = uibutton(app.Panel_Calibration, 'push');
            app.Button_DegreePlus.ButtonPushedFcn = createCallbackFcn(app, @Button_DegreePlusPushed, true);
            app.Button_DegreePlus.Enable = 'off';
            app.Button_DegreePlus.Position = [283 28 27 22];
            app.Button_DegreePlus.Text = '+';

            % Create Label_DegreePlus
            app.Label_DegreePlus = uilabel(app.Panel_Calibration);
            app.Label_DegreePlus.FontSize = 16;
            app.Label_DegreePlus.Enable = 'off';
            app.Label_DegreePlus.Position = [179 28 101 22];
            app.Label_DegreePlus.Text = '澧炲姞瑙掑害鎸夐挳';

            % Create Label_DegreeCalib
            app.Label_DegreeCalib = uilabel(app.Panel_Calibration);
            app.Label_DegreeCalib.BackgroundColor = [0.902 0.902 0.902];
            app.Label_DegreeCalib.HorizontalAlignment = 'center';
            app.Label_DegreeCalib.FontSize = 16;
            app.Label_DegreeCalib.FontWeight = 'bold';
            app.Label_DegreeCalib.FontColor = [0 0 1];
            app.Label_DegreeCalib.Position = [489 21 168 36];
            app.Label_DegreeCalib.Text = '鏍?瀹?瑙?搴?;

            % Create Panel_RangeDetect
            app.Panel_RangeDetect = uipanel(app.Tab_TwoMotor);
            app.Panel_RangeDetect.Title = '杩愬姩鑼冨洿妫€娴?;
            app.Panel_RangeDetect.Position = [4 306 674 95];

            % Create Button_MaxAngle_Before
            app.Button_MaxAngle_Before = uibutton(app.Panel_RangeDetect, 'push');
            app.Button_MaxAngle_Before.ButtonPushedFcn = createCallbackFcn(app, @Button_MaxAngle_BeforePushed, true);
            app.Button_MaxAngle_Before.FontSize = 16;
            app.Button_MaxAngle_Before.FontWeight = 'bold';
            app.Button_MaxAngle_Before.Position = [36 20 192 40];
            app.Button_MaxAngle_Before.Text = '璁粌鍓嶅眻鏇茶搴︽娴?;

            % Create Button_MaxAngle_After
            app.Button_MaxAngle_After = uibutton(app.Panel_RangeDetect, 'push');
            app.Button_MaxAngle_After.ButtonPushedFcn = createCallbackFcn(app, @Button_MaxAngle_AfterPushed, true);
            app.Button_MaxAngle_After.FontSize = 16;
            app.Button_MaxAngle_After.FontWeight = 'bold';
            app.Button_MaxAngle_After.Position = [265 20 185 40];
            app.Button_MaxAngle_After.Text = '璁粌鍚庡眻鏇茶搴︽娴?;

            % Create Label_MaxDegree
            app.Label_MaxDegree = uilabel(app.Panel_RangeDetect);
            app.Label_MaxDegree.BackgroundColor = [0.902 0.902 0.902];
            app.Label_MaxDegree.HorizontalAlignment = 'center';
            app.Label_MaxDegree.FontSize = 16;
            app.Label_MaxDegree.FontWeight = 'bold';
            app.Label_MaxDegree.FontColor = [0 0 1];
            app.Label_MaxDegree.Position = [486 21 172 40];
            app.Label_MaxDegree.Text = '鏈€ 澶?瑙?搴?;

            % Create Panel_DeviceOpra
            app.Panel_DeviceOpra = uipanel(app.Tab_TwoMotor);
            app.Panel_DeviceOpra.Title = '璁惧鎿嶄綔';
            app.Panel_DeviceOpra.Position = [4 406 674 87];

            % Create Button_CloseDevice
            app.Button_CloseDevice = uibutton(app.Panel_DeviceOpra, 'push');
            app.Button_CloseDevice.ButtonPushedFcn = createCallbackFcn(app, @Button_CloseDevicePushed, true);
            app.Button_CloseDevice.FontSize = 16;
            app.Button_CloseDevice.FontWeight = 'bold';
            app.Button_CloseDevice.Position = [486 14 119 40];
            app.Button_CloseDevice.Text = '鍏抽棴璁惧';

            % Create Button_InitAll
            app.Button_InitAll = uibutton(app.Panel_DeviceOpra, 'push');
            app.Button_InitAll.ButtonPushedFcn = createCallbackFcn(app, @Button_InitAllPushed, true);
            app.Button_InitAll.FontSize = 16;
            app.Button_InitAll.FontWeight = 'bold';
            app.Button_InitAll.Position = [37 14 128 40];
            app.Button_InitAll.Text = '鍚姩璁惧';

            % Create Button_Finish
            app.Button_Finish = uibutton(app.Panel_DeviceOpra, 'push');
            app.Button_Finish.ButtonPushedFcn = createCallbackFcn(app, @Button_FinishPushed, true);
            app.Button_Finish.FontSize = 16;
            app.Button_Finish.FontWeight = 'bold';
            app.Button_Finish.Position = [262 14 128 40];
            app.Button_Finish.Text = '缁撴潫';

            % Create Tab_Opensignal
            app.Tab_Opensignal = uitab(app.TabGroup);
            app.Tab_Opensignal.Title = 'Opensignal';

            % Create ButtonGroup_OP_OPRA
            app.ButtonGroup_OP_OPRA = uibuttongroup(app.Tab_Opensignal);
            app.ButtonGroup_OP_OPRA.Title = '鎿嶄綔';
            app.ButtonGroup_OP_OPRA.Position = [383 236 210 211];

            % Create SelectButton_disable
            app.SelectButton_disable = uiradiobutton(app.ButtonGroup_OP_OPRA);
            app.SelectButton_disable.Text = 'disable';
            app.SelectButton_disable.Position = [113 90 64 22];

            % Create SelectButton_enable
            app.SelectButton_enable = uiradiobutton(app.ButtonGroup_OP_OPRA);
            app.SelectButton_enable.Text = 'enable';
            app.SelectButton_enable.Position = [24 90 107 22];

            % Create SelectButton_config
            app.SelectButton_config = uiradiobutton(app.ButtonGroup_OP_OPRA);
            app.SelectButton_config.Text = 'config';
            app.SelectButton_config.Position = [113 134 64 22];

            % Create SelectButton_devices
            app.SelectButton_devices = uiradiobutton(app.ButtonGroup_OP_OPRA);
            app.SelectButton_devices.Text = 'devices';
            app.SelectButton_devices.Position = [24 134 107 22];
            app.SelectButton_devices.Value = true;

            % Create Button_send
            app.Button_send = uibutton(app.ButtonGroup_OP_OPRA, 'push');
            app.Button_send.Position = [11 35 83 23];
            app.Button_send.Text = '纭畾';

            % Create Button_update_config
            app.Button_update_config = uibutton(app.ButtonGroup_OP_OPRA, 'push');
            app.Button_update_config.Position = [107 35 83 23];
            app.Button_update_config.Text = '鏇存柊鍙傛暟';

            % Create Panel_OP_Message
            app.Panel_OP_Message = uipanel(app.Tab_Opensignal);
            app.Panel_OP_Message.Title = '娑堟伅';
            app.Panel_OP_Message.Position = [24 217 335 250];

            % Create Label_config
            app.Label_config = uilabel(app.Panel_OP_Message);
            app.Label_config.Position = [7 48 29 22];
            app.Label_config.Text = '鐘舵€?;

            % Create Label_send
            app.Label_send = uilabel(app.Panel_OP_Message);
            app.Label_send.Position = [7 119 29 22];
            app.Label_send.Text = '鍙戦€?;

            % Create Label_receive
            app.Label_receive = uilabel(app.Panel_OP_Message);
            app.Label_receive.Position = [7 196 29 22];
            app.Label_receive.Text = '鎺ユ敹';

            % Create TextArea_config
            app.TextArea_config = uitextarea(app.Panel_OP_Message);
            app.TextArea_config.Editable = 'off';
            app.TextArea_config.Position = [35 8 288 63];

            % Create TextArea_send
            app.TextArea_send = uitextarea(app.Panel_OP_Message);
            app.TextArea_send.Editable = 'off';
            app.TextArea_send.Position = [36 90 288 51];

            % Create TextArea_receive
            app.TextArea_receive = uitextarea(app.Panel_OP_Message);
            app.TextArea_receive.Editable = 'off';
            app.TextArea_receive.Position = [35 152 288 67];

            % Create Button_InitOP
            app.Button_InitOP = uibutton(app.Tab_Opensignal, 'push');
            app.Button_InitOP.Position = [50 158 100 25];
            app.Button_InitOP.Text = '鍒濆鍖?;

            % Create Label_StateOP
            app.Label_StateOP = uilabel(app.Tab_Opensignal);
            app.Label_StateOP.Position = [292 159 238 22];

            % Create Label_OP_ID
            app.Label_OP_ID = uilabel(app.Tab_Opensignal);
            app.Label_OP_ID.Position = [290 92 244 30];
            app.Label_OP_ID.Text = 'Label2';

            % Create Tab_3
            app.Tab_3 = uitab(app.TabGroup);
            app.Tab_3.Title = '鎺ㄦ潌鎺у埗';

            % Create ButtonGroup
            app.ButtonGroup = uibuttongroup(app.Tab_3);
            app.ButtonGroup.SelectionChangedFcn = createCallbackFcn(app, @ButtonGroupSelectionChanged, true);
            app.ButtonGroup.TitlePosition = 'centertop';
            app.ButtonGroup.Title = '澶ц吙姘斿泭鎺у埗';
            app.ButtonGroup.Position = [359 351 122 96];

            % Create Button
            app.Button = uitogglebutton(app.ButtonGroup);
            app.Button.Text = '鍏呮皵';
            app.Button.Position = [10 47 100 23];

            % Create Button_2
            app.Button_2 = uitogglebutton(app.ButtonGroup);
            app.Button_2.Text = '淇濇寔';
            app.Button_2.Position = [10 26 100 23];
            app.Button_2.Value = true;

            % Create Button_3
            app.Button_3 = uitogglebutton(app.ButtonGroup);
            app.Button_3.Text = '鏀炬皵';
            app.Button_3.Position = [10 5 100 23];

            % Create ButtonGroup_2
            app.ButtonGroup_2 = uibuttongroup(app.Tab_3);
            app.ButtonGroup_2.SelectionChangedFcn = createCallbackFcn(app, @ButtonGroup_2SelectionChanged, true);
            app.ButtonGroup_2.TitlePosition = 'centertop';
            app.ButtonGroup_2.Title = '灏忚吙姘斿泭鎺у埗';
            app.ButtonGroup_2.Position = [501 351 120 96];

            % Create Button_4
            app.Button_4 = uitogglebutton(app.ButtonGroup_2);
            app.Button_4.Text = '鍏呮皵';
            app.Button_4.Position = [11 47 100 23];

            % Create Button_5
            app.Button_5 = uitogglebutton(app.ButtonGroup_2);
            app.Button_5.Text = '淇濇寔';
            app.Button_5.Position = [11 26 100 23];
            app.Button_5.Value = true;

            % Create Button_6
            app.Button_6 = uitogglebutton(app.ButtonGroup_2);
            app.Button_6.Text = '鏀炬皵';
            app.Button_6.Position = [11 5 100 23];

            % Create Button_StretchPlus_2
            app.Button_StretchPlus_2 = uibutton(app.Tab_3, 'push');
            app.Button_StretchPlus_2.ButtonPushedFcn = createCallbackFcn(app, @Button_StretchPlus_2Pushed, true);
            app.Button_StretchPlus_2.FontSize = 18;
            app.Button_StretchPlus_2.FontWeight = 'bold';
            app.Button_StretchPlus_2.Position = [192 363 37 35];
            app.Button_StretchPlus_2.Text = '+';

            % Create Button_StretchMinus_2
            app.Button_StretchMinus_2 = uibutton(app.Tab_3, 'push');
            app.Button_StretchMinus_2.ButtonPushedFcn = createCallbackFcn(app, @Button_StretchMinus_2Pushed, true);
            app.Button_StretchMinus_2.FontSize = 18;
            app.Button_StretchMinus_2.FontWeight = 'bold';
            app.Button_StretchMinus_2.Position = [286 363 37 35];
            app.Button_StretchMinus_2.Text = '-';

            % Create Button_stretch
            app.Button_stretch = uibutton(app.Tab_3, 'push');
            app.Button_stretch.ButtonPushedFcn = createCallbackFcn(app, @Button_stretchPushed, true);
            app.Button_stretch.FontSize = 16;
            app.Button_stretch.FontWeight = 'bold';
            app.Button_stretch.Position = [190 414 133 35];
            app.Button_stretch.Text = '鎺ㄦ潌褰掗浂';

            % Create Button_stretch_2
            app.Button_stretch_2 = uibutton(app.Tab_3, 'push');
            app.Button_stretch_2.ButtonPushedFcn = createCallbackFcn(app, @Button_stretch_2Pushed, true);
            app.Button_stretch_2.FontSize = 16;
            app.Button_stretch_2.FontWeight = 'bold';
            app.Button_stretch_2.Position = [41 419 128 35];
            app.Button_stretch_2.Text = '鑷€傚簲鍏呮皵';

            % Create Button_stretch_3
            app.Button_stretch_3 = uibutton(app.Tab_3, 'push');
            app.Button_stretch_3.ButtonPushedFcn = createCallbackFcn(app, @Button_stretch_3Pushed, true);
            app.Button_stretch_3.FontSize = 16;
            app.Button_stretch_3.FontWeight = 'bold';
            app.Button_stretch_3.Position = [41 354 128 35];
            app.Button_stretch_3.Text = '闈欐€佺壍寮?;

            % Create Button_stretch_4
            app.Button_stretch_4 = uibutton(app.Tab_3, 'push');
            app.Button_stretch_4.ButtonPushedFcn = createCallbackFcn(app, @Button_stretch_4Pushed, true);
            app.Button_stretch_4.FontSize = 16;
            app.Button_stretch_4.FontWeight = 'bold';
            app.Button_stretch_4.Position = [40 292 128 35];
            app.Button_stretch_4.Text = '寮€鍚弽棣?;

            % Create Panel_MotorUIAxes
            app.Panel_MotorUIAxes = uipanel(app.UIFigure);
            app.Panel_MotorUIAxes.Title = '鐢垫満鏁版嵁';
            app.Panel_MotorUIAxes.Position = [688 3 304 758];

            % Create UIAxes_Current
            app.UIAxes_Current = uiaxes(app.Panel_MotorUIAxes);
            title(app.UIAxes_Current, '鍔涚煩')
            xlabel(app.UIAxes_Current, 'X')
            ylabel(app.UIAxes_Current, 'N路m')
            zlabel(app.UIAxes_Current, 'Z')
            app.UIAxes_Current.XLim = [0 500];
            app.UIAxes_Current.YLim = [-20 20];
            app.UIAxes_Current.XDir = 'reverse';
            app.UIAxes_Current.XTick = [0 100 200 300 400 500];
            app.UIAxes_Current.XTickLabel = {'0'; '2s'; '4s'; '6s'; '8s'; '10s'};
            app.UIAxes_Current.YTick = [-15 -10 -5 0 5 10 15];
            app.UIAxes_Current.YTickLabel = {'-15'; '-10'; '-5'; '0'; '5'; '10'; '15'};
            app.UIAxes_Current.XGrid = 'on';
            app.UIAxes_Current.YGrid = 'on';
            app.UIAxes_Current.Position = [14 450 264 218];

            % Create UIAxes_Degree
            app.UIAxes_Degree = uiaxes(app.Panel_MotorUIAxes);
            title(app.UIAxes_Degree, '鐢垫満瑙掑害')
            xlabel(app.UIAxes_Degree, 'X')
            ylabel(app.UIAxes_Degree, '搴β?)
            zlabel(app.UIAxes_Degree, 'Z')
            app.UIAxes_Degree.XLim = [0 500];
            app.UIAxes_Degree.YLim = [-10 160];
            app.UIAxes_Degree.XDir = 'reverse';
            app.UIAxes_Degree.XTick = [0 100 200 300 400 500];
            app.UIAxes_Degree.XTickLabel = {'0'; '2s'; '4s'; '6s'; '8s'; '10s'};
            app.UIAxes_Degree.YTick = [0 20 40 60 80 100 120 140];
            app.UIAxes_Degree.YTickLabel = {'0'; '20'; '40'; '60'; '80'; '100'; '120'; '140'};
            app.UIAxes_Degree.XGrid = 'on';
            app.UIAxes_Degree.YGrid = 'on';
            app.UIAxes_Degree.Position = [14 10 264 218];

            % Create UIAxes_Speed
            app.UIAxes_Speed = uiaxes(app.Panel_MotorUIAxes);
            title(app.UIAxes_Speed, '閫熷害')
            xlabel(app.UIAxes_Speed, 'X')
            ylabel(app.UIAxes_Speed, 'r/min')
            zlabel(app.UIAxes_Speed, 'Z')
            app.UIAxes_Speed.XLim = [0 500];
            app.UIAxes_Speed.YLim = [-5 5];
            app.UIAxes_Speed.XDir = 'reverse';
            app.UIAxes_Speed.XTick = [0 100 200 300 400 500];
            app.UIAxes_Speed.XTickLabel = {'0'; '2s'; '4s'; '6s'; '8s'; '10s'};
            app.UIAxes_Speed.YTick = [-5 -2.5 0 2.5 5];
            app.UIAxes_Speed.YTickLabel = {'-5'; '-2.5'; '0'; '2.5'; '5'};
            app.UIAxes_Speed.XGrid = 'on';
            app.UIAxes_Speed.YGrid = 'on';
            app.UIAxes_Speed.Position = [14 233 264 218];

            % Create Label_MotorDegree
            app.Label_MotorDegree = uilabel(app.Panel_MotorUIAxes);
            app.Label_MotorDegree.BackgroundColor = [0.902 0.902 0.902];
            app.Label_MotorDegree.HorizontalAlignment = 'center';
            app.Label_MotorDegree.FontSize = 18;
            app.Label_MotorDegree.FontWeight = 'bold';
            app.Label_MotorDegree.Position = [42 671 233 52];
            app.Label_MotorDegree.Text = '鍏?鑺?瑙?搴?;

            % Create Label_Tips
            app.Label_Tips = uilabel(app.UIFigure);
            app.Label_Tips.BackgroundColor = [0.902 0.902 0.902];
            app.Label_Tips.HorizontalAlignment = 'center';
            app.Label_Tips.FontSize = 22;
            app.Label_Tips.FontWeight = 'bold';
            app.Label_Tips.FontColor = [1 0 0];
            app.Label_Tips.Position = [6 98 675 133];
            app.Label_Tips.Text = '';

            % Create Button_MsgPatient
            app.Button_MsgPatient = uibutton(app.UIFigure, 'push');
            app.Button_MsgPatient.ButtonPushedFcn = createCallbackFcn(app, @Button_MsgPatientPushed, true);
            app.Button_MsgPatient.FontSize = 16;
            app.Button_MsgPatient.FontWeight = 'bold';
            app.Button_MsgPatient.Position = [6 37 131 39];
            app.Button_MsgPatient.Text = '鎮ｈ€呬俊鎭櫥璁?;

            % Create Label_12
            app.Label_12 = uilabel(app.UIFigure);
            app.Label_12.HorizontalAlignment = 'right';
            app.Label_12.Position = [315 45 29 22];
            app.Label_12.Text = '鍚屾';

            % Create Lamp
            app.Lamp = uilamp(app.UIFigure);
            app.Lamp.Position = [359 45 20 20];
            app.Lamp.Color = [0.8 0.8 0.8];

            % Create Label_UpdateData
            app.Label_UpdateData = uilabel(app.UIFigure);
            app.Label_UpdateData.Position = [174 45 119 22];
            app.Label_UpdateData.Text = '';

            % Create Label_State
            app.Label_State = uilabel(app.UIFigure);
            app.Label_State.BackgroundColor = [0.902 0.902 0.902];
            app.Label_State.HorizontalAlignment = 'center';
            app.Label_State.FontSize = 22;
            app.Label_State.FontWeight = 'bold';
            app.Label_State.FontColor = [0.4667 0.6745 0.1882];
            app.Label_State.Position = [397 23 280 67];
            app.Label_State.Text = ' ';

            % Create TabGroup2
            app.TabGroup2 = uitabgroup(app.UIFigure);
            app.TabGroup2.Position = [997 4 508 757];

            % Create Tab
            app.Tab = uitab(app.TabGroup2);
            app.Tab.Title = '鑲岀數鏁版嵁';

            % Create UIAxes_EMG_1
            app.UIAxes_EMG_1 = uiaxes(app.Tab);
            title(app.UIAxes_EMG_1, 'CUSTOM')
            xlabel(app.UIAxes_EMG_1, 'X')
            ylabel(app.UIAxes_EMG_1, 'mV')
            zlabel(app.UIAxes_EMG_1, 'Z')
            app.UIAxes_EMG_1.XLim = [0 5000];
            app.UIAxes_EMG_1.YLim = [-3 3];
            app.UIAxes_EMG_1.XDir = 'reverse';
            app.UIAxes_EMG_1.XTick = [0 1000 2000 3000 4000 5000];
            app.UIAxes_EMG_1.XTickLabel = {'0s'; '0.5s'; '1s'; '1.5s'; '2s'; '2.5s'};
            app.UIAxes_EMG_1.YTick = [-4 -3 -2 -1 0 1 2 3 4];
            app.UIAxes_EMG_1.YTickLabel = {'-4'; '-3'; '-2'; '-1'; '0'; '1'; '2'; '3'; '4'};
            app.UIAxes_EMG_1.YGrid = 'on';
            app.UIAxes_EMG_1.Position = [18 513 224 180];

            % Create UIAxes_EMG_2
            app.UIAxes_EMG_2 = uiaxes(app.Tab);
            title(app.UIAxes_EMG_2, 'CUSTOM')
            xlabel(app.UIAxes_EMG_2, 'X')
            ylabel(app.UIAxes_EMG_2, 'mV')
            zlabel(app.UIAxes_EMG_2, 'Z')
            app.UIAxes_EMG_2.XLim = [0 5000];
            app.UIAxes_EMG_2.YLim = [-3 3];
            app.UIAxes_EMG_2.XDir = 'reverse';
            app.UIAxes_EMG_2.XTick = [0 1000 2000 3000 4000 5000];
            app.UIAxes_EMG_2.XTickLabel = {'0s'; '0.5s'; '1s'; '1.5s'; '2s'; '2.5s'};
            app.UIAxes_EMG_2.YTick = [-4 -3 -2 -1 0 1 2 3 4];
            app.UIAxes_EMG_2.YTickLabel = {'-4'; '-3'; '-2'; '-1'; '0'; '1'; '2'; '3'; '4'};
            app.UIAxes_EMG_2.YGrid = 'on';
            app.UIAxes_EMG_2.Position = [266 512 224 180];

            % Create UIAxes_EMG_3
            app.UIAxes_EMG_3 = uiaxes(app.Tab);
            title(app.UIAxes_EMG_3, 'CUSTOM')
            xlabel(app.UIAxes_EMG_3, 'X')
            ylabel(app.UIAxes_EMG_3, 'mV')
            zlabel(app.UIAxes_EMG_3, 'Z')
            app.UIAxes_EMG_3.XLim = [0 5000];
            app.UIAxes_EMG_3.YLim = [-3 3];
            app.UIAxes_EMG_3.XDir = 'reverse';
            app.UIAxes_EMG_3.XTick = [0 1000 2000 3000 4000 5000];
            app.UIAxes_EMG_3.XTickLabel = {'0s'; '0.5s'; '1s'; '1.5s'; '2s'; '2.5s'};
            app.UIAxes_EMG_3.YTick = [-4 -3 -2 -1 0 1 2 3 4];
            app.UIAxes_EMG_3.YTickLabel = {'-4'; '-3'; '-2'; '-1'; '0'; '1'; '2'; '3'; '4'};
            app.UIAxes_EMG_3.YGrid = 'on';
            app.UIAxes_EMG_3.Position = [19 302 224 180];

            % Create UIAxes_EMG_4
            app.UIAxes_EMG_4 = uiaxes(app.Tab);
            title(app.UIAxes_EMG_4, 'CUSTOM')
            xlabel(app.UIAxes_EMG_4, 'X')
            ylabel(app.UIAxes_EMG_4, 'mV')
            zlabel(app.UIAxes_EMG_4, 'Z')
            app.UIAxes_EMG_4.XLim = [0 5000];
            app.UIAxes_EMG_4.YLim = [-3 3];
            app.UIAxes_EMG_4.XDir = 'reverse';
            app.UIAxes_EMG_4.XTick = [0 1000 2000 3000 4000 5000];
            app.UIAxes_EMG_4.XTickLabel = {'0s'; '0.5s'; '1s'; '1.5s'; '2s'; '2.5s'};
            app.UIAxes_EMG_4.YTick = [-4 -3 -2 -1 0 1 2 3 4];
            app.UIAxes_EMG_4.YTickLabel = {'-4'; '-3'; '-2'; '-1'; '0'; '1'; '2'; '3'; '4'};
            app.UIAxes_EMG_4.YGrid = 'on';
            app.UIAxes_EMG_4.Position = [266 302 224 180];

            % Create UIAxes_EMG_trigger
            app.UIAxes_EMG_trigger = uiaxes(app.Tab);
            title(app.UIAxes_EMG_trigger, 'Trigger')
            xlabel(app.UIAxes_EMG_trigger, 'X')
            ylabel(app.UIAxes_EMG_trigger, 'mV')
            zlabel(app.UIAxes_EMG_trigger, 'Z')
            app.UIAxes_EMG_trigger.XLim = [0 5000];
            app.UIAxes_EMG_trigger.YLim = [-0.1 1.1];
            app.UIAxes_EMG_trigger.XDir = 'reverse';
            app.UIAxes_EMG_trigger.XTick = [0 1000 2000 3000 4000 5000];
            app.UIAxes_EMG_trigger.XTickLabel = {'0s'; '0.5s'; '1s'; '1.5s'; '2s'; '2.5s'};
            app.UIAxes_EMG_trigger.YTick = [-4 -3 -2 -1 0 1 2 3 4];
            app.UIAxes_EMG_trigger.YTickLabel = {'-4'; '-3'; '-2'; '-1'; '0'; '1'; '2'; '3'; '4'};
            app.UIAxes_EMG_trigger.Position = [266 98 224 180];

            % Create UIAxes_EMG_5
            app.UIAxes_EMG_5 = uiaxes(app.Tab);
            title(app.UIAxes_EMG_5, 'STATE')
            xlabel(app.UIAxes_EMG_5, 'X')
            zlabel(app.UIAxes_EMG_5, 'Z')
            app.UIAxes_EMG_5.XLim = [0 5000];
            app.UIAxes_EMG_5.YLim = [-1.2 1.2];
            app.UIAxes_EMG_5.XDir = 'reverse';
            app.UIAxes_EMG_5.XTick = [0 1000 2000 3000 4000 5000];
            app.UIAxes_EMG_5.XTickLabel = {'0s'; '0.5s'; '1s'; '1.5s'; '2s'; '2.5s'};
            app.UIAxes_EMG_5.YTick = [-1 0 1];
            app.UIAxes_EMG_5.YTickLabel = {'-1'; '0'; '1'};
            app.UIAxes_EMG_5.YGrid = 'on';
            app.UIAxes_EMG_5.Position = [19 98 224 180];

            % Create svmGaugeLabel
            app.svmGaugeLabel = uilabel(app.Tab);
            app.svmGaugeLabel.HorizontalAlignment = 'center';
            app.svmGaugeLabel.Position = [228 24 51 22];
            app.svmGaugeLabel.Text = 'svm杈撳嚭';

            % Create Gauge_SVM
            app.Gauge_SVM = uigauge(app.Tab, 'linear');
            app.Gauge_SVM.Limits = [-10 10];
            app.Gauge_SVM.Position = [18 44 472 41];

            % Create Tab_2
            app.Tab_2 = uitab(app.TabGroup2);
            app.Tab_2.Title = '鎺ㄦ潌鏁版嵁';

            % Create Stretch_force
            app.Stretch_force = uiaxes(app.Tab_2);
            title(app.Stretch_force, '鎺ㄦ潌鍔?)
            ylabel(app.Stretch_force, 'Force')
            zlabel(app.Stretch_force, 'Z')
            app.Stretch_force.XLim = [0 100];
            app.Stretch_force.YLim = [-50 50];
            app.Stretch_force.YTick = [-50 -25 0 25 50];
            app.Stretch_force.Box = 'on';
            app.Stretch_force.YGrid = 'on';
            app.Stretch_force.Position = [103 239 276 195];

            % Create Stretch_pos
            app.Stretch_pos = uiaxes(app.Tab_2);
            title(app.Stretch_pos, '鎺ㄦ潌浣嶇疆')
            ylabel(app.Stretch_pos, 'Position')
            zlabel(app.Stretch_pos, 'Z')
            app.Stretch_pos.XLim = [0 100];
            app.Stretch_pos.YLim = [0 50];
            app.Stretch_pos.Box = 'on';
            app.Stretch_pos.YGrid = 'on';
            app.Stretch_pos.Position = [103 455 276 197];

            % Create Heat_Press
            app.Heat_Press = uiaxes(app.Tab_2);
            title(app.Heat_Press, '鐑姏鍥?)
            zlabel(app.Heat_Press, 'Z')
            app.Heat_Press.XLim = [0 100];
            app.Heat_Press.YLim = [0 50];
            app.Heat_Press.XTick = [];
            app.Heat_Press.YTick = [];
            app.Heat_Press.Box = 'on';
            colormap(app.Heat_Press, 'jet')
            app.Heat_Press.Position = [109 3 270 225];

            % Create Stretch_State
            app.Stretch_State = uilabel(app.Tab_2);
            app.Stretch_State.BackgroundColor = [0.902 0.902 0.902];
            app.Stretch_State.HorizontalAlignment = 'center';
            app.Stretch_State.FontSize = 18;
            app.Stretch_State.FontWeight = 'bold';
            app.Stretch_State.Position = [72 659 336 54];
            app.Stretch_State.Text = '鎺?鏉?鐘?鎬?;

            % Create Lamp_8
            app.Lamp_8 = uilamp(app.Tab_2);
            app.Lamp_8.Position = [256 37 28 28];
            app.Lamp_8.Color = [0.4 0.902 0];

            % Create Lamp_7
            app.Lamp_7 = uilamp(app.Tab_2);
            app.Lamp_7.Position = [209 37 30 30];
            app.Lamp_7.Color = [0.4 0.902 0];

            % Create Lamp_6
            app.Lamp_6 = uilamp(app.Tab_2);
            app.Lamp_6.Position = [256 79 28 28];
            app.Lamp_6.Color = [0.4 0.902 0];

            % Create Lamp_5
            app.Lamp_5 = uilamp(app.Tab_2);
            app.Lamp_5.Position = [209 77 30 30];
            app.Lamp_5.Color = [0.4 0.902 0];

            % Create Lamp_4
            app.Lamp_4 = uilamp(app.Tab_2);
            app.Lamp_4.Position = [233 58 28 28];
            app.Lamp_4.Color = [0.4 0.902 0];

            % Create Lamp_3
            app.Lamp_3 = uilamp(app.Tab_2);
            app.Lamp_3.Position = [233 164 28 28];
            app.Lamp_3.Color = [0.4 0.902 0];

            % Create Lamp_2
            app.Lamp_2 = uilamp(app.Tab_2);
            app.Lamp_2.Position = [266 148 28 28];
            app.Lamp_2.Color = [0.4 0.902 0];

            % Create Lamp_1
            app.Lamp_1 = uilamp(app.Tab_2);
            app.Lamp_1.Position = [200 148 28 28];
            app.Lamp_1.Color = [0.4 0.902 0];

            % Show the figure after all components are created
            app.UIFigure.Visible = 'on';
        end
    end

    % App creation and deletion
    methods (Access = public)

        % Construct app
        function app = clinic_test0401

            % Create UIFigure and components
            createComponents(app)

            % Register the app with App Designer
            registerApp(app, app.UIFigure)

            % Execute the startup function
            runStartupFcn(app, @startupFcn)

            if nargout == 0
                clear app
            end
        end

        % Code that executes before app deletion
        function delete(app)

            % Delete UIFigure when app is deleted
            delete(app.UIFigure)
        end
    end
end
