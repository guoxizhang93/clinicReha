classdef DrEmpower_can< handle
    properties %% 成员属性块--开始
        id
        baudrate
        READ_FLAG
        uart_data
        id_list
        cur_angle_list
        angle_speed_torque_state_flag
        set_angles_mode_1_flag
        dev
        read_timeout_seconds
        state_timeout_seconds
        LastError
    end %% 成员属性块--结束
    
    methods %% 成员函数块--开始
        %%  构造函数（初始化CAN接口）
        function obj = DrEmpower_can(COM, baudrate, read_timeout_seconds)  %% %% Point2D类的构造函数
            if nargin < 2 || isempty(baudrate)
                baudrate = 115200;
            end
            if nargin < 3 || isempty(read_timeout_seconds)
                read_timeout_seconds = 0.5;
            end
            obj.id = 1;
            obj.baudrate=baudrate;
            obj.uart_data = [];
            obj.READ_FLAG = 1;
            obj.cur_angle_list = [];
            obj.read_timeout_seconds = read_timeout_seconds;
            obj.state_timeout_seconds = 1.0;
            obj.LastError = "";
            try
                obj.dev = serialport(COM,obj.baudrate,"Timeout",obj.read_timeout_seconds);
            catch ME
                obj.READ_FLAG = -1;
                obj.LastError = string(ME.message);
                error("DrEmpower_can:OpenSerialFailed","无法打开CAN串口 %s：%s",string(COM),string(ME.message));
            end
            obj.angle_speed_torque_state_flag = 0;
            obj.set_angles_mode_1_flag = 0;
            flush(obj.dev);
        end

        function delete(obj)
            try
                if obj.is_connected()
                    configureCallback(obj.dev,"off");
                    flush(obj.dev);
                end
            catch
            end
        end

        function connected = is_connected(obj)
            connected = false;
            try
                connected = ~isempty(obj.dev) && isvalid(obj.dev);
            catch
                connected = ~isempty(obj.dev);
            end
        end

        function n = available_bytes(obj)
            n = 0;
            try
                if obj.is_connected()
                    n = obj.dev.NumBytesAvailable;
                end
            catch ME
                obj.READ_FLAG = -1;
                obj.LastError = string(ME.message);
            end
        end

        function ok = wait_for_bytes(obj, num_bytes, timeout_seconds)
            if nargin < 3 || isempty(timeout_seconds)
                timeout_seconds = obj.read_timeout_seconds;
            end
            ok = false;
            t0 = tic;
            while toc(t0) < timeout_seconds
                if obj.available_bytes() >= num_bytes
                    ok = true;
                    return;
                end
                pause(0.001);
            end
            obj.READ_FLAG = -1;
            obj.LastError = "等待串口数据超时";
        end
        
        %%  串口发送函数
        function write_data(obj,data)
%             if obj.dev.NumBytesAvailable>0
%                 flush(obj.dev);
%             end
            if isempty(data)
                obj.READ_FLAG = -1;
                obj.LastError = "发送数据为空";
                return;
            end
            if ~obj.is_connected()
                obj.READ_FLAG = -1;
                obj.LastError = "串口未连接";
                return;
            end
            try
                write(obj.dev,uint8(data),"uint8");
            catch ME
                obj.READ_FLAG = -1;
                obj.LastError = string(ME.message);
                warning("DrEmpower_can:WriteFailed","CAN串口发送失败：%s",string(ME.message));
            end
        end
        
        %%  串口接收函数 - 阻塞读取
        function rdata = read_data(obj,num)
            rdata = [];
            if ~obj.wait_for_bytes(num,obj.read_timeout_seconds)
                obj.READ_FLAG = -1;
                return;
            end
            try
                rdata = read(obj.dev,num,"uint8");
                if length(rdata)==num
                    obj.READ_FLAG = 1;
                else
                    obj.READ_FLAG = -1;
                    obj.LastError = "串口返回数据长度不足";
                end
            catch ME
                obj.READ_FLAG = -1;
                obj.LastError = string(ME.message);
            end
        end
        %% 串口接收--get_ids专用
        function id_list = read_data_id(obj)
            byte_list = [];
            id_lists = [];
            if ~obj.wait_for_bytes(16,obj.state_timeout_seconds)
                id_list = [];
                return;
            end
            while obj.available_bytes() >= 16
%                 disp(obj.dev.NumBytesAvailable());
                byte_list = [byte_list, read(obj.dev,16,"uint8")];
            end
%             disp(byte_list);
            NN = numel(byte_list);
%             disp(NN);
            if (rem(numel(byte_list),16) == 0) && (NN > 16)
                for i = 1 : ((NN/16))
%                     disp(NN);
                    jdata = byte_list(((i-1) * 16 + 1): (i) * 16);
                    cdata = obj.uart_to_can_ID(jdata);
                    id_lists = [id_lists, bitshift((cdata(2) * 256 + cdata(3) - 1), -5)];
%                     disp(id_lists);
                end
            end
            if (rem(numel(byte_list),16) == 0) && (numel(byte_list) == 16)
%                 disp(NN);
                jdata = byte_list(1: 16);
                cdata = obj.uart_to_can_ID(jdata);
                id_lists = [id_lists, bitshift((cdata(2) * 256 + cdata(3) - 1), -5)];
%                 disp(cdata);
            end
%             disp(id_lists);
            id_list = id_lists;
        end
        %% read_data 状态反馈转用
        function byte_list_result = read_data_state2(obj,n)
            obj.READ_FLAG = -1;
            byte_list = zeros(0,16,'uint8');
            t0 = tic;
            while size(byte_list,1) < n && toc(t0) < obj.state_timeout_seconds
                if obj.available_bytes() < 1
                    pause(0.001);
                    continue;
                end
                frame_head = read(obj.dev,1,"uint8");
                if isempty(frame_head) || frame_head(1) ~= 170
                    continue;
                end
                remain_timeout = max(0.01,obj.state_timeout_seconds - toc(t0));
                if ~obj.wait_for_bytes(15,remain_timeout)
                    break;
                end
                frame_rest = read(obj.dev,15,"uint8");
                frame = uint8([frame_head,frame_rest]);
                if numel(frame) == 16 && frame(4) == 0x08
                    byte_list(end+1,:) = frame;
                end
            end
            if size(byte_list,1) >= n
                obj.READ_FLAG = 1;
                byte_list_result = byte_list;
            else
                obj.LastError = "状态反馈读取超时或帧不完整";
                obj.READ_FLAG = -1;
                byte_list_result = [];
            end
        end
        %% USB转CAN模块包模式：CAN报文->串行帧
        function udata = can_to_uart(obj, data, rtr)
            udata = [0xAA, 0, 0, 0x08, 0, 0, 0, 0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
            if length(data) == 11 && data(1) == 0x08
                if rtr == 1
                    udata(3) = 0x01;
                end
                for i= 1:10
                    udata(6 + i) = data(i + 1);
                end
            else
                udata = [];
            end
        end
        
        
        %%  USB转CAN模块包模式：串行帧->CAN报文
        function cdata = uart_to_can(obj,data)
            cdata = [0x08, 0, 0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
            if length(data) == 16 && data(4) == 0x08
                for i = 1:10
                    cdata(1 + i) = data(i + 6);
                end
            else
                obj.READ_FLAG = -1;
                cdata = [];
            end
        end
        
        %%  USB转CAN模块包模式：串行帧->CAN报文
        function cdata = uart_to_can_ID(obj,data)
            cdata = [0x08, 0, 0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
            if length(data) == 16 && data(4) == 0x08
                for i = 1:10
                    cdata(1 + i) = data(i + 6);
                end
            else
                obj.READ_FLAG = -1;
                cdata = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
            end
        end
        
        %%  CAN发送函数
        function [] = send_command(obj,id_num, cmd, data, rtr)
            if numel(data) > 8
                obj.READ_FLAG = -1;
                obj.LastError = "CAN数据区超过8字节";
                warning("DrEmpower_can:InvalidFrame","CAN数据区超过8字节，已取消发送");
                return;
            end
            cdata = [0x08, 0, 0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
            can_id = bitshift(id_num ,5) + cmd;
            cdata(2) = bitshift(can_id ,-8);
            cdata(3) = bitand(can_id,0xFF);
            for i = 1:length(data)
                cdata(3 + i) = data(i);
            end
            write_data(obj,can_to_uart(obj,cdata, rtr));
            obj.set_angles_mode_1_flag = 0;
        end
        
        %%  CAN接收函数
        function rdata = receive_data(obj)
            obj.READ_FLAG = -1;
            rdata = [];
            if ~obj.wait_for_bytes(16,obj.read_timeout_seconds)   %%  目前测试大概在2ms以内
                return;
            end
            rmsg = read_data(obj,16);
            if obj.READ_FLAG==-1
                rdata = [];
            else
                data = uart_to_can(obj,rmsg);
                if numel(data) >= 11
                    rdata = data(4:end);
                else
                    rdata = [];
                    obj.READ_FLAG = -1;
                    obj.LastError = "CAN接收帧解析失败";
                end
            end
        end
        
        %%  预设角度 -- 过程辅助函数
        function preset_angle(obj,id_num, angle, t, param, mode)
            factor = 0.01;
            if mode == 0
                f_angle = angle;
                s16_time = single(abs(t) / factor);
                if param > 300
                    param = 300;
                end
                s16_width = single(abs(param / factor));
                data = format_data([f_angle, s16_time, s16_width], 'f s16 s16', 'encode');
            elseif mode == 1
                f_angle = angle;
                s16_time = single(abs(t) / factor);
                s16_accel = single((abs(param)) / factor);
                data = format_data([f_angle, s16_time, s16_accel], 'f s16 s16', 'encode');
%                 disp(data);
            elseif mode == 2
                f_angle = angle;
                s16_speed_ff = single((t) / factor);
                s16_torque_ff = single((param) / factor);
                data = format_data([f_angle, s16_speed_ff, s16_torque_ff], 'f s16 s16', 'encode');
            end
            obj.send_command(id_num, 0x0C, data, 0);
            
        end
        %%  预设速度 -- 过程辅助函数
        function preset_speed(obj,id_num, speed, param, mode)
            factor = 0.01;
            f_speed = speed;
            if mode == 0
                s16_torque = single((param) / factor);
                if f_speed == 0
                    s16_torque = 0;
                end
                s16_input_mode = single(1 / factor);
                data = format_data([f_speed, s16_torque, s16_input_mode], 'f s16 s16', 'encode');
            else
                s16_ramp_rate = single((param) / factor);
                s16_input_mode = single(2 / factor);
                data = format_data([f_speed, s16_ramp_rate, s16_input_mode], 'f s16 s16', 'encode');
            end
            obj.send_command(id_num, 0x0C, data, 0);
            
        end
        %%  预设扭矩  -- 过程辅助函数
        function preset_torque(obj,id_num, torque, param, mode)
            factor = 0.01;
            f_torque = torque;
            if mode == 0
                s16_input_mode = single(1 / factor);
                s16_ramp_rate = 0;
            else
                s16_input_mode = single(6 / factor);
                s16_ramp_rate = single((param) / factor);
            end
            data = format_data([f_torque, s16_ramp_rate, s16_input_mode], 'f s16 s16', 'encode');
            obj.send_command(id_num, 0x0C, data, 0);
            
        end
        %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% 用户函数%% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% 
        %% 单个关节绝对角度控制
        function true_or_false = set_angle(obj,id_num, angle, speed, param, mode)
            factor = 0.01;
            if mode == 0
                f_angle = angle;
                s16_speed = single((abs(speed)) / factor);
                if param > 300
                    param = 300;
                end
                s16_width = single(abs(param / factor));
                data = format_data([f_angle, s16_speed, s16_width], 'f s16 s16', 'encode');
                obj.send_command(id_num, 0x19, data, 0);
            elseif mode == 1
                if speed > 0 && param > 0
                    f_angle = single(angle);
                    s16_speed = single((abs(speed)) / factor);
                    s16_accel = single((abs(param)) / factor);
                    data = format_data([f_angle, s16_speed, s16_accel], 'f s16 s16', 'encode');
%                     disp(data);
                    obj.send_command(id_num, 0x1A, data, 0);
                end
            elseif mode == 2
                f_angle = angle;
                s16_speed_ff = single((speed) / factor);
                s16_torque_ff = single((param) / factor);
                data = format_data([f_angle, s16_speed_ff, s16_torque_ff], 'f s16 s16', 'encode');
                obj.send_command(id_num, 0x1B, data, 0);
            end
            true_or_false = true;
        end
        %% 多个关节绝对角度控制
        function true_or_false = set_angles(obj,id_list, angle_list, speed, param, mode)
            if length(id_list) == length(angle_list)
                switch mode
                    case 0
                        for i = 1:length(id_list)
                            obj.preset_angle(id_list(i), angle_list(i), speed, param, mode);
                        end
                        order_num = 0x10;
                        data = format_data([order_num, 0], 'u32 u16', 'encode');
                        obj.send_command(0 , 0x08, data, 0);
                    case 1
                        if numel(obj.cur_angle_list) ~= numel(angle_list) || obj.set_angles_mode_1_flag == 0
                            obj.cur_angle_list = [];
                            if obj.angle_speed_torque_state_flag == 0
                                for i = 1 : numel(id_list)
                                    state = obj.get_angle(id_list(i));
                                    if obj.READ_FLAG == 1
                                        obj.cur_angle_list = [obj.cur_angle_list, state];
                                    else
                                        obj.cur_angle_list = [obj.cur_angle_list, 0];
                                    end
                                end
                            else
                                angle_speed_torques = obj.angle_speed_torque_states(id_list);
                                if isempty(angle_speed_torques)
                                    obj.READ_FLAG = -1;
                                    obj.LastError = "读取关节状态失败";
                                    true_or_false = false;
                                    return;
                                end
                                for i = 1 : length(id_list)
                                    if i <= numel(angle_speed_torques) && ~isempty(angle_speed_torques{i})
                                        obj.cur_angle_list = [obj.cur_angle_list, angle_speed_torques{i}(end,1)];
                                    else
                                        obj.cur_angle_list = [obj.cur_angle_list, 0];
                                    end
                                end
                            end
                        end
                        if speed > 0 && param > 0
                            DELTA_angle_list = [];
                            for i = 1:length(angle_list)
                                DELTA_angle_list = [DELTA_angle_list,abs(angle_list(i)-obj.cur_angle_list(i))];
                            end
                            delta_angle = max(DELTA_angle_list);
                            if delta_angle <= (6 * speed * speed / abs(param))
                                t = 2 * sqrt(delta_angle / (6 * abs(param)));
                            else
                                t = speed / abs(param) + delta_angle / (6 * speed);
                            end
                            for i = 1:length(id_list)
                                obj.preset_angle(id_list(i), angle_list(i), t, param, mode);
                            end
                            order_num = 0x11;
                            data = format_data([order_num, 0], 'u32 u16', 'encode');
                            obj.send_command(0 , 0x08, data, 0);
                            obj.set_angles_mode_1_flag = 1;
                        else
                            disp("speed or accel <= 0");
                        end
                    case 2
                        for i = 1:length(id_list)
                            obj.preset_angle(id_list(i), angle_list(i), speed, param, mode);
                        end
                        order_num = 0x12;
                        data = format_data([order_num, 0], 'u32 u16', 'encode');
                        obj.send_command(0 , 0x08, data, 0) ;
                end
                obj.cur_angle_list = angle_list;
            else
                true_or_false = false;
                return;
            end
            true_or_false = true;
        end
        %% 单个关节相对角度控制
        function true_or_false = step_angle(obj,id_num, angle, speed, param, mode)
            factor = 0.01;
            if mode == 0
                f_angle = angle;
                s16_speed = single((abs(speed)) / factor);
                if param > 300
                    param = 300;
                end
                s16_width = single(abs(param / factor));
                data = format_data([f_angle, s16_speed, s16_width], 'f s16 s16', 'encode');
                obj.send_command(id_num, 0x0C, data, 0);
                order_num = 0x10;
                data = format_data([order_num, 1], 'u32 u16', 'encode');
                obj.send_command(id_num , 0x08, data, 0) ;
            elseif mode == 1
                if speed > 0 && param > 0
                    f_angle = single(angle);
                    s16_speed = single((abs(speed)) / factor);
                    s16_accel = single((abs(param)) / factor);
                    data = format_data([f_angle, s16_speed, s16_accel], 'f s16 s16', 'encode');
                    obj.send_command(id_num, 0x0C, data, 0);
                    order_num = 0x11;
                    data = format_data([order_num, 1], 'u32 u16', 'encode');
                    obj.send_command(id_num , 0x08, data, 0) ;
                end
            elseif mode == 2
                f_angle = angle;
                s16_speed_ff = single((speed) / factor);
                s16_torque_ff = single((param) / factor);
                data = format_data([f_angle, s16_speed_ff, s16_torque_ff], 'f s16 s16', 'encode');
                obj.send_command(id_num, 0x0C, data, 0);
                order_num = 0x12;
                data = format_data([order_num, 1], 'u32 u16', 'encode');
                obj.send_command(id_num , 0x08, data, 0) ;
            end
            true_or_false = true;
        end
        %% 多个关节相对角度同步控制
        function true_or_false = step_angles(obj,id_list, angle_list, speed, param, mode)
            if length(id_list) == length(angle_list)
                switch mode
                    case 0
                        for i = 1:length(id_list)
                            obj.preset_angle(id_list(i), angle_list(i), speed, param, mode);
                        end
                        order_num = 0x10;
                        data = format_data([order_num, 1], 'u32 u16', 'encode');
                        obj.send_command(0 , 0x08, data, 0);
                    case 1
                        if speed > 0 && param > 0
                            DETA_angle_list = abs(angle_list);
                            delta_angle = max(DETA_angle_list);
                            if delta_angle <= (6 * speed * speed / abs(param))
                                t = 2 * sqrt(delta_angle / (6 * abs(param)));
                            else
                                t = speed / abs(param) + delta_angle / (6 * speed);
                            end
                            for i = 1:length(id_list)
                                obj.preset_angle(id_list(i), angle_list(i), t, param, mode)
                            end
                            order_num = 0x11;
                            data = format_data([order_num, 2], 'u32 u16', 'encode');
                            obj.send_command(0 , 0x08, data, 0)  ;
                        end
                    case 2
                        for i = 1:length(id_list)
                            obj.preset_angle(id_list(i), angle_list(i), speed, param, mode);
                        end
                        order_num = 0x12;
                        data = format_data([order_num, 1], 'u32 u16', 'encode');
                        obj.send_command(0 , 0x08, data, 0);
                end
            else
                true_or_false = false;
                return;
            end
            true_or_false = true;
        end
        
        %%  单关节力位混合控制函数
        function true_or_false = set_angle_adaptive(obj,id_num, angle, speed, torque)
            factor = 0.01;
            f_angle = angle;
            s16_speed = single((abs(speed)) / factor);
            s16_torque = single(abs(torque / factor));
            data = format_data([f_angle, s16_speed, s16_torque], 'f s16 s16', 'encode');
            obj.send_command(id_num, 0x0B, data, 0);
            true_or_false = true;
        end
        %%  多关节力位混合控制函数
        function true_or_false = set_angles_adaptive(obj,id_list, angle_list, speed_list, torque_list)
            length = numel(id_list);
            if numel(angle_list) ~= length || numel(speed_list) ~= length || numel(torque_list) ~= length
                disp("关节数量与参数数量不一致，请检查");
                true_or_false = false;
                return;
            end
            for i=1:length
                obj.preset_angle(id_list(i), angle_list(i), abs(speed_list(i)), abs(torque_list(i)),1);
            end
            order_num = 0x11;
            data = format_data([order_num, 3], 'u32 u16', 'encode');
            obj.send_command(0 , 0x08, data, 0);
            true_or_false = true;
        end
        
        %% 单个关节阻抗控制
        function true_or_false = impedance_control(obj,id_num, angle, speed, tff, kp, kd)
            factor = 0.001;
            kp = abs(kp);
            kd = abs(kd);
            if kp > 20
                kp = 20;
            end
            if kd > 20
                kd = 20;
            end
            if kp ~= 0
                angle_set = (- kd * speed - tff) / kp + angle;
            else
                angle_set = angle;
            end
            obj.preset_angle(id_num, angle_set, speed, tff, 2);
            order_num = 21;
            data = format_data([order_num, int16(kp / factor), int16(kd / factor)], 'u32 s16 s16', 'encode');
            obj.send_command(0 , 0x08, data, 0);
            true_or_false = true;
        end
        %% 多关节阻抗控制
        function true_or_false = impedance_control_multi(obj, id_list, angle_list, speed_list, tff_list, kp_list, kd_list)
            factor = 0.001;
            length = numel(id_list);
            if numel(angle_list) ~= length || numel(speed_list) ~= length || numel(tff_list) ~= length || numel(kp_list) ~= length || numel(kd_list) ~= length
                disp("关节数量与参数数量不一致，请检查");
                true_or_false = false;
                return;
            end
            for i=1:length
                kp_list(i) = abs(kp_list(i));
                kd_list(i) = abs(kd_list(i));
                if kp_list(i) > 10
                    kp_list(i) = 10; % 限制系数，否则带载时容易震动
                end
                if kd_list(i) > 10
                    kd_list(i) = 10; % 限制系数，否则带载时容易震动
                end
%                 if kp_list(i) ~= 0
%                     angle_list(i) = (- kd_list(i) * speed_list(i) - tff_list(i)) / kp_list(i) + angle_list(i);
%                 else
%                     disp("机器人中关节不允许不间断连续旋转");
%                     true_or_false = false;
%                     return;
%                 end
                obj.preset_angle(id_list(i), angle_list(i), speed_list(i), tff_list(i), 2);
                order_num = 22;
                data = format_data([order_num, int16(kp_list(i) / factor), int16(kd_list(i) / factor)], 'u32 s16 s16','encode');
                obj.send_command(id_list(i), 0x08, data, 0);
                pause(0.01);
            end
            order_num = 0x17;
            data = format_data([order_num], 'u32', 'encode');
            obj.send_command(0, 0x08, data, 0);
            true_or_false = true;
        end
        
        %% 单关节运动助力
        function true_or_false = motion_aid(obj, id_num, angle, speed, angle_err, speed_err, torque)
            factor = 0.01;
            if (angle < -300) || (angle > 300)
                disp("助力角度超出范围，允许范围为 -300°~300°，请检查");
                true_or_false = false;
            else
                data = format_data([int16(angle / factor), int16(angle_err / factor), int16(speed_err / factor), int16(torque / factor)],'s16 u16 u16 s16', 'encode');
                obj.send_command(id_num, 0x0D, data, 0)
                obj.set_speed_adaptive(id_num, speed);
                true_or_false = true;
            end
        end
        %% 多关节运动助力
        function true_or_false = motion_aid_multi(obj, id_list, angle_list, speed_list, angle_err_list, speed_err_list, torque_list)
            factor = 0.01;
            length = numel(id_list);
            for i=1:length
                if (angle_list(i) < -300) || (angle_list(i) > 300)
                    disp("助力角度超出范围，允许范围为 -300°~300°，请检查");
                    true_or_false = false;
                    return;
                end
            end
            if numel(angle_list) ~= length || numel(speed_list) ~= length || numel(angle_err_list) ~= length || numel(speed_err_list) ~= length || numel(torque_list) ~= length
                disp("关节数量与参数数量不一致，请检查");
                true_or_false = false;
                return;
            end
            for i =1:length
                data = format_data([int16(angle_list(i) / factor), uint16(angle_err_list(i) / factor), uint16(speed_err_list(i) / factor), int16(torque_list(i) / factor)],'s16 u16 u16 s16', 'encode');
                obj.send_command(id_list(i), 0x06, data, 0);
            end
            order_num = 0x11;
            data = format_data([order_num, 4], 'u32 u16', 'encode');
            obj.send_command(0, 0x08, data, 0);
            pause(0.1);
            for i=1:length
                obj.set_speed_adaptive(id_list(i), speed_list(i));
            end
            true_or_false = true;
        end
        %%  单关节转速自适应
        function true_or_false = set_speed_adaptive(obj, id_num, speed_adaptive)
            if speed_adaptive < 0
                disp("speed_adaptive 必须大于 0，请重新输入");
                true_or_false = false;
                return;
            else
                obj.preset_angle(id_num, abs(speed_adaptive), 0, 0, 1);
                order_num = 0x20;
                data = format_data([order_num], 'u32', 'encode');
                obj.send_command(id_num, 0x08, data, 0); 
                true_or_false = true;
            end
        end
        %% 设置转速限制
        function true_or_false = set_speed_limit(obj, id_num, speed_limit)
            if speed_limit < 0
                disp("speed_limit 必须大于 0，请重新输入");
                true_or_false = false;
            else
                obj.preset_angle(id_num, abs(speed_limit), 0, 0, 1);
                order_num = 0x18;
                data = format_data([order_num], 'u32', 'encode');
                obj.send_command(id_num, 0x08, data, 0); 
                true_or_false = true;
            end
        end
        %%  单关节力矩自适应
        function true_or_false = set_torque_adaptive(obj, id_num, torque_adaptive)
            if torque_adaptive < 0
                disp("torque_adaptive 必须大于 0，请重新输入");
                true_or_false = false;
            else
                obj.preset_angle(id_num, abs(torque_adaptive), 0, 0, 1);
                order_num = 0x21;
                data = format_data([order_num], 'u32', 'encode');
                obj.send_command(id_num, 0x08, data, 0); 
                true_or_false = true;
            end
        end
        %%  单关节力矩限制
        function true_or_false = set_torque_limit(obj, id_num, torque_limit)
            if torque_limit < 0
                disp("torque_limit 必须大于 0，请重新输入");
                true_or_false = false;
            else
                obj.preset_angle(id_num, abs(torque_limit), 0, 0, 1);
                order_num = 0x19;
                data = format_data([order_num], 'u32', 'encode');
                obj.send_command(id_num, 0x08, data, 0);
                true_or_false = true;
            end
        end
        
        %%  等待关节运动到位函数
        function true_or_false = position_done(obj, id_num, timeout_seconds)
            if nargin < 3 || isempty(timeout_seconds)
                timeout_seconds = 10;
            end
            kk = 0;
            t0 = tic;
            while kk ~= 1 && toc(t0) < timeout_seconds
                kk = obj.read_property(id_num,32002,3);
                if obj.READ_FLAG ~= 1
                    pause(0.02);
                end
            end 
            if kk == 1
                true_or_false = true;
            else
                obj.LastError = "等待关节到位超时";
                true_or_false = false;
            end 
        end 
        
        function true_or_false = positions_done(obj, id_list, timeout_seconds)
            if nargin < 3 || isempty(timeout_seconds)
                timeout_seconds = 10;
            end
            k = 1;
            t0 = tic;
            while k == 1 && toc(t0) < timeout_seconds
                kk = 1;
                for i=1:numel(id_list)
                    kk = kk & obj.position_done(id_list(i), max(0.1, timeout_seconds - toc(t0)));
                end %%  for end
                if kk == 1
                    k = 0;
                end %%  if end
            end %%  while end
            true_or_false = (k == 0);
            if ~true_or_false
                obj.LastError = "等待多个关节到位超时";
            end
        end %%  function positions_done
        
        %% 单个关节速度控制
        function set_speed(obj,id_num, speed, param, mode)
            factor = 0.01;
            f_speed = speed;
            if mode == 0
                s16_torque = int32(param / factor);
                if f_speed == 0
                    s16_torque = 0;
                end
                u16_input_mode = 1;
                data = format_data([f_speed, s16_torque, u16_input_mode], 'f s16 u16', 'encode');
            else
                s16_ramp_rate = int32((param) / factor);
                u16_input_mode = 2;
                data = format_data([f_speed, s16_ramp_rate, u16_input_mode], 'f s16 u16', 'encode');
            end
            obj.send_command(id_num , 0x1C, data, 0);
            
        end
        %% 多个关节速度控制
        function set_speeds(obj,id_list, speed_list, param, mode)
            if length(id_list) == length(speed_list)
                for i = 1:length(id_list)
                    obj.preset_speed(id_list(i), speed_list(i), param, mode);
                end
                order_num = 0x13;
                data = format_data([order_num], 'u32', 'encode');
                obj.send_command(0 , 0x08, data, 0);
            end
        end
        %% 单个关节扭矩控制
        function set_torque(obj, id_num, torque, param, mode)
            factor = 0.01;
            f_torque = torque;
            if mode == 0
                u16_input_mode = 1;
                s16_ramp_rate = 0;
            else
                u16_input_mode = 6;
                s16_ramp_rate = int32((param) / factor);
            end
            data = format_data([f_torque, s16_ramp_rate, u16_input_mode], 'f s16 u16', 'encode');
            obj.send_command(id_num , 0x1D, data, 0);
        end
        %% 多个关节扭矩控制
        function set_torques(obj, id_list, torque_list, param, mode)
            if length(id_list) == length(torque_list)
                for i = 1:length(id_list)
                    obj.preset_torque(id_list(i), torque_list(i), param, mode);
                end
                order_num = 0x14;
                data = format_data([order_num], 'u32', 'encode');
                obj.send_command(0 , 0x08, data, 0);
            end
        end
        %% 急停
        function estop(obj,id_num)
            order_num = 0x06;
            data = format_data([order_num], 'u32', 'encode');
            obj.send_command(id_num , 0x08, data, 0);
        end
        %% 修改关节ID号
        function true_or_false = set_id(obj, id_num, new_id)
            obj.write_property(id_num,31001,3,new_id);
            if obj.get_id(new_id) == new_id
                obj.save_config(new_id);
                disp(num2str(id_num)+"号修改为 "+ num2str(new_id));
                true_or_false = true;
            else
                true_or_false = false;
            end
        end
        %% 设置CAN波特率
         % 注意该函数执行后需要使用USB转CAN模块配置软件将CAN波特率同步修改,然后再次调用该函数才能设置成功（永久保存），否则下一次重新上电关节CAN波特率将恢复成原来的值
        function true_or_false = set_can_baud_rate(obj,id_num, baud_rate)
            obj.write_property(id_num, 21001, 3,baud_rate);
            true_or_false = true;
%             obj.save_config(id_num);
        end
        %% 设置关节模式
        function true_or_false = set_mode(obj,id_num, mode)
            if mode == 1
                obj.write_property(id_num,30003,3,1);
            elseif mode == 2
                obj.write_property(id_num,30003,3,2);
            end
            true_or_false = true;
        end
        %% 设置关节零点
        function true_or_false = set_zero_position(obj, id_num)
            order_num = 0x05;
            data = format_data([order_num], 'u32', 'encode');
            obj.send_command(id_num , 0x08, data, 0);
            obj.save_config(id_num);
            disp("设置零点成功");
            true_or_false = true;
        end
        
        %% 设置关节临时零点
        function true_or_false = set_zero_position_temp(obj, id_num)
            order_num = 0x23;
            data = format_data([order_num], 'u32', 'encode');
            obj.send_command(id_num , 0x08, data, 0);
            disp("设置 临时 零点成功");
            true_or_false = true;
        end

        %% 设置临时角度限制范围
        function true_or_false = set_angle_range(obj, id_num, angle_min, angle_max)
            angle = obj.read_property(id_num,38001,0);
            if obj.READ_FLAG == 1
                if angle >= angle_min && angle <= angle_max
                    obj.write_property(id_num, 38004, 0, angle_min);
                    obj.write_property(id_num, 38005, 0, angle_max);
                    obj.write_property(id_num, 38006, 3, 1);
                    true_or_false = true;
                else
                    disp("当前角度不在角度限制范围内，请检查");
                    true_or_false = false;
                end
            end
        end
        %% 取消临时角度限制
        function true_or_false = disable_angle_range(obj,id_num)
            obj.write_property(id_num, 38006, 3, 0);
            if obj.read_property(id_num,38006,3) == 0
                disp("取消临时角度限制成功");
                true_or_false = true;
            else
                true_or_false = false;
            end
        end
        %% 设置角度限制范围属性
        function true_or_false = set_angle_range_config(obj, id_num, angle_min, angle_max)
            angle = obj.read_property(id_num,38001,0);
            if obj.READ_FLAG == 1
                if angle >= angle_min && angle <= angle_max
                    obj.write_property(id_num, 31202, 0, angle_min);
                    obj.write_property(id_num, 31203, 0, angle_max);
                    obj.write_property(id_num, 31201, 3, 1);
%                     obj.save_config(id_num);
                    true_or_false = true;
                else
                    disp("当前角度不在角度限制范围内，请检查");
                    true_or_false = false;
                end
            end
        end
        %% 取消角度限制属性
        function true_or_false = disable_angle_range_config(obj,id_num)
            obj.write_property(id_num, 31201, 3, 0);
            if obj.read_property(id_num,31201,3) == 0
                disp("取消角度限制属性成功");
                true_or_false = true;
            else
                true_or_false = false;
            end
        end
        
        %%  设置关节pid
        function true_or_false = set_pid(obj, id_num, P, I, D)
            if P<=0 || I<=0 || D<0
                disp('请输出大于 0 的 PID 数值');
                true_or_false = false;
            else
                obj.write_property(id_num, 32102, 0, P);
                obj.write_property(id_num, 32103, 0, D);
                obj.write_property(id_num, 32104, 0, I);
                true_or_false = true;
            end
        end
        %%  读取关节pid
        function pid = get_pid(obj, id_num)
            P = obj.read_property(id_num,32102,0);
            D = obj.read_property(id_num, 32103, 0);
            I = obj.read_property(id_num, 32104, 0);
            pid = [P, I, D];
        end
        %% 参数属性修改
        function true_or_false = write_property(obj,id_num,address,data_type,value)
            data_types = ["f", "u16", "s16", "u32", "s32"];
            data = format_data([single(address), single(data_type), single(value)], "u16 u16 " + data_types(data_type+1), 'encode');
            obj.send_command(id_num , 0x1F, data, 0);
            true_or_false = true;
        end
        %% 单关节读取关节ID号
        function id = get_id(obj, id_num)
            id = obj.read_property(id_num,31001,3);
        end
        %% 多关节读取ID号
        function id_list = get_ids(obj)
            data = format_data([31001, 3], 'u16 u16', 'encode');
            obj.send_command(0, 0x1E, data, 0);
            id_list = sort(obj.read_data_id());
        end
        %% 读取角度
        function angle = get_angle(obj,id_num)
            angle = obj.read_property(id_num,38001,0);
        end
        %% 读取转速
        function speed = get_speed(obj,id_num)
            speed = obj.read_property(id_num,38002,0);
        end
        %% 读取关节角度和速度
        function state = get_state(obj, id_num)
            angle = obj.read_property(id_num,38001,0);
            if obj.READ_FLAG==1
                speed = obj.read_property(id_num,38002,0);
                if obj.READ_FLAG==1
                    state = [angle,speed];
                else
                    state = [];
                end
            else
                state = [];
            end
        end
        %% 读取力矩
        function torque = get_torque(obj,id_num)
            torque = obj.read_property(id_num,38003,0);
        end
        %% 读取关节电压和电流
        function volcur = get_volcur(obj, id_num)
            vol  = obj.read_property(id_num, 1, 0);
            if obj.READ_FLAG == 1
                cur  = obj.read_property(id_num, 33201,0);
                if obj.READ_FLAG == 1
                    volcur = [vol,cur];
                else
                    volcur = [];
                end
            else
                volcur = [];
            end
        end
        %% 开启状态反馈功能
        function true_or_false = enable_angle_speed_torque_state(obj,id_num)
             obj.write_property(id_num, 22001, 3, 1);
             obj.angle_speed_torque_state_flag = 1;
             true_or_false = true;
        end
        %% 单关节状态反馈
        function angle_speed_torque_result = angle_speed_torque_state(obj, id_num,n)
            angle_speed_torque_result = [];
            udata = obj.read_data_state2(n);
            if obj.READ_FLAG == 1
                for i = 1 : n
                    jdata = udata(((i-1) * 16 + 1): (i) * 16);
                    cdata = obj.uart_to_can_ID(jdata);
                    if id_num ==  bitshift((cdata(2) * 256 + cdata(3) - 1), -5)
                        angle_speed_torque = format_data(cdata(4:numel(cdata)), "f s16 s16", 'decode');
                        angle_speed_torque_result = [round(angle_speed_torque(1), 3), round(angle_speed_torque(2) * 0.01, 3),round(angle_speed_torque(3) * 0.01, 3)];
                    end
                    if id_num ~=  bitshift((cdata(2) * 256 + cdata(3) - 1), -5)
                        disp("angle_speed_torque_state 函数中 ID 号有误");
                        angle_speed_torque_result = [];
                    end
                end
            end
        end
        %% 多关节状态反馈
        function angle_speed_torques_result = angle_speed_torque_states(obj, id_list)
            n = numel(id_list);
            udata = obj.read_data_state2(n);
            angle_speed_torques_result = [];
            if obj.READ_FLAG == 1
                jdata = udata;
                cdata = [];
                id_num = [];
                angle_speed_torque = [];
                for j = 1 : size(jdata,1)
                    cdata(j,:) = obj.uart_to_can_ID(jdata(j,:));
                    if numel(cdata(j,:)) < 11
                        continue;
                    end
                    id_num(j) =  bitshift((cdata(j,2) * 256 + cdata(j,3) - 1), -5);
                    angle_speed_torque(j,:) = format_data(cdata(j,4:end), "f s16 s16", 'decode');
                end
                if isempty(angle_speed_torque)
                    disp("return None");
                    angle_speed_torques_result = [];
                    return;
                else
                    angle_speed_torque(:,1) = round(angle_speed_torque(:,1), 3);
                    angle_speed_torque(:,2) = round(angle_speed_torque(:,2)* 0.01, 3);
                    angle_speed_torque(:,3) = round(angle_speed_torque(:,3)* 0.01, 3);

                    angle_speed_torques_result{1} = angle_speed_torque(id_num == 1,:);
                    angle_speed_torques_result{2} = angle_speed_torque(id_num == 2,:);
                end
            end
        end
        %% 设置状态反馈间隔时间
        function true_or_false = set_state_feedback_rate_ms(obj,id_num, n)
             obj.write_property(id_num, 31002, 3, n);
             true_or_false = true;
        end
        
        %% 关闭状态反馈
        function true_or_false = disable_angle_speed_torque_state(obj,id_num)
            for i=1:5
                obj.write_property(id_num, 22001, 3, 0);
            end
             obj.angle_speed_torque_state_flag = 0;
             true_or_false = true;
        end
        %% 参数属性读取
        function value = read_property(obj,id_num,address,data_type)
            data = format_data([address, data_type], 'u16 u16', 'encode');
            obj.send_command(id_num, 0x1E, data, 0);
            cdata = obj.receive_data();
            if obj.READ_FLAG == 1
                data_types = ["f", "u16", "s16", "u32", "s32"];
                property = format_data(cdata, "u16 u16 " + data_types(data_type+1), 'decode');
                if length(property) > 0
                    switch data_type
                        case 1
                            value = uint16(property(end));
                        case 2
                            value = int16(property(end));
                        case 3
                            value = uint32(property(end));
                        case 4
                            value = int32(property(end));
                        otherwise
                            value = single(property(end));
                    end
                else
                    value = [];
                end
            else
                value = [];
            end
        end
        
        
        
        
        %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% 系统辅助函数%% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% %% 
        
        %% 保存配置
        function true_or_false = save_config(obj,id_num)
            order_num = 0x01;
            data = format_data([order_num], 'u32', 'encode');
            obj.send_command(id_num , 0x08, data, 0);
            true_or_false = true;
        end
        %% 软件重启
        function true_or_false = reboot(obj,id_num)
            order_num = 0x03;
            data = format_data([order_num], 'u32', 'encode');
            obj.send_command(id_num , 0x08, data, 0);
            true_or_false = true;
        end
        %% 恢复出厂设置
        function true_or_false = init_config(obj,id_num)
            data = format_data([id_num], 'u32', 'encode');
            obj.send_command(id_num , 0x0E, data, 0);
            true_or_false = true;
        end
    end %% 成员函数块--结束
end %% 类定义结束

function rdata = format_data(data, format, type)
format_list = string(format).split();
rdata = [];
if type == "decode" && length(data) == 8
    p = 0;
    for i =1:length(format_list)
        f = format_list(i);
        len = 0;
        s_f = '';
        if f == 'f'
            len = 4;
            s_f = 'single';
        elseif f == 'u16'
            len = 2;
            s_f = 'uint16';
        elseif f == 's16'
            len = 2;
            s_f = 'int16';
        elseif f == 'u32'
            len = 4;
            s_f = 'uint32';
        elseif f == 's32'
            len = 4;
            s_f = 'int32';
        end
        ba = [];
        if len > 0
            for j = 1:len
                ba=[ba,data(p+1)];
                p = p+1;
            end
            rdata = [rdata,single(typecast(uint8(ba),s_f))];
        else
            rdata = [];
        end
    end
elseif type=="encode"&&length(format_list)==length(data)
    for i =1:length(format_list)
        f = format_list(i);
        ba = 0;
        len = 0;
        if f == 'f'
            ba = single(data(i));
            len = 4;
        elseif f == 'u16'
            ba = uint16(data(i));
            len = 2;
        elseif f == 's16'
            ba = int16(data(i));
            len = 2;
        elseif f == 'u32'
            ba = uint32(data(i));
            len = 4;
        elseif f == 's32'
            ba = int32(data(i));
            len = 4;
        end
        bytes = [];
        if len > 0
            bytes = typecast(ba,'uint8');
%             disp(bytes);
            rdata = [rdata,bytes];
        else
            print32('unkown format in format_data(): ' + f);
            rdata = [];
        end
    end
end
end

