#include "clinic_core.h"
#include <ctype.h>
#include <math.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>

#ifdef __cplusplus
#include "../libsvm-3.32/libsvm-3.32/svm.h"
#endif

static const double k_linkage_angles[] = {
    0.975843406238283,1.84353832467785,2.69832422248591,3.54066256064978,4.37099891655988,5.18976353935132,5.99737189708415,6.79422521394369,7.58071099603211,8.35720354460096,
    9.12406445585637,9.88164310667889,10.6302771258035,11.3702928501584,12.1020057662025,12.8257209362168,13.5417334096008,14.2503286193016,14.9517827635742,15.6463631733213,
    16.3343286653056,17.0159298815597,17.6914096153455,18.3610031240350,19.0249384292994,19.6834366049979,20.3367120531687,20.9849727685229,21.6284205918412,22.2672514526704,
    22.9016556017136,23.5318178332961,24.1579176982867,24.7801297078401,25.3986235283216,26.0135641677583,26.6251121541574,27.2334237060154,27.8386508953345,28.4409418034501,
    29.0404406699613,29.6372880350473,30.2316208754397,30.8235727343114,31.4132738453324,32.0008512511310,32.5864289163927,33.1701278358137,33.7520661371242,34.3323591793815,
    34.9111196467274,35.4884576377961,36.0644807509504,36.6392941655172,37.2130007191844,37.7857009817181,38.3574933251481,38.9284739905673,39.4987371516823,40.0683749752482,
    40.6374776785158,41.2061335838115,41.7744291703700,42.3424491235320,42.9102763814162,43.4779921791702,44.0456760909029,44.6134060693949,45.1812584836831,45.7493081546095,
    46.3176283884241,46.8862910085258,47.4553663854276,48.0249234650238,48.5950297952398,49.1657515511397,49.7371535585672,50.3092993163913,50.8822510174272,51.4560695681038,
    52.0308146069410,52.6065445219069,53.1833164667163,53.7611863761350,54.3402089803503,54.9204378184699,55.5019252512059,56.0847224728036,56.6688795222710,57.2544452939643,
    57.8414675475840,58.4299929176348,59.0200669224007,59.6117339724868,60.2050373789768,60.8000193612550,61.3967210545401,61.9951825171760,62.5954427377256,63.1975396419095,
    63.8015100994338,64.4073899307451,65.0152139137545,65.6250157905678,66.2368282742576,66.8506830557139,67.4666108106056,68.0846412064859,68.7048029100711,69.3271235947227,
    69.9516299481595,70.5783476804256,71.2073015321382,71.8385152830377,72.4720117608606,73.1078128505545,73.7459395038519,74.3864117492194,75.0292487021949,75.6744685761267,
    76.3220886933236,76.9721254966260,77.6245945614052,78.2795106079966,78.9368875145709,79.5967383304473,80.2590752898481,80.9239098260972,81.5912525862590,82.2611134462162,
    82.9335015261838,83.6084252066514,84.2858921447517,84.9659092910449,85.6484829067139,86.3336185811602,87.0213212499907,87.7115952133881,88.4044441548507,89.0998711602943,
    89.7978787375016,90.4984688359103,91.2016428667260,91.9074017233496,92.6157458021065,93.3266750232656,94.0401888523378,94.7562863216407,95.4749660521214,96.1962262754236,
    96.9200648561922,97.6464793146043,98.3754668491177,99.1070243594318,99.8411484696502,100.577835551643,101.317081748600,102.058882998776,102.803235059420,103.550133530890,
    104.299573880958,105.051551469295,105.806061572150,106.563099407225,107.322660158747,108.084739002754,108.849331132592,109.616431784649,110.386036264330,111.158139972291,
    111.932738430948,112.709827311287,113.489402459988,114.271459926895,115.055995992856,115.843007197961,116.632490370212,117.424442654661,118.218861543055,119.015744904022
};

static int linkage_count(void) {
    return (int)(sizeof(k_linkage_angles) / sizeof(k_linkage_angles[0]));
}

static void write_u16(unsigned char* p, uint16_t v) {
    p[0] = (unsigned char)(v & 0xFF);
    p[1] = (unsigned char)((v >> 8) & 0xFF);
}

static void write_i16(unsigned char* p, int16_t v) {
    write_u16(p, (uint16_t)v);
}

static void write_u32(unsigned char* p, uint32_t v) {
    p[0] = (unsigned char)(v & 0xFF);
    p[1] = (unsigned char)((v >> 8) & 0xFF);
    p[2] = (unsigned char)((v >> 16) & 0xFF);
    p[3] = (unsigned char)((v >> 24) & 0xFF);
}

static void write_i32(unsigned char* p, int32_t v) {
    write_u32(p, (uint32_t)v);
}

static void write_f32(unsigned char* p, float v) {
    memcpy(p, &v, sizeof(float));
}

static int16_t read_i16(const unsigned char* p) {
    return (int16_t)((uint16_t)p[0] | ((uint16_t)p[1] << 8));
}

static float read_f32(const unsigned char* p) {
    float value;
    memcpy(&value, p, sizeof(float));
    return value;
}

static double smooth_interpolate(double start, double end, double ratio) {
    double smooth;
    if (ratio < 0.0) ratio = 0.0;
    if (ratio > 1.0) ratio = 1.0;
    smooth = 0.5 - 0.5 * cos(3.14159265358979323846 * ratio);
    return start + (end - start) * smooth;
}

static double clamp_double(double value, double min_value, double max_value) {
    if (value < min_value) return min_value;
    if (value > max_value) return max_value;
    return value;
}

static double trimf(double x, double a, double b, double c) {
    if (x <= a || x >= c) return 0.0;
    if (x == b) return 1.0;
    if (x < b) return (x - a) / (b - a);
    return (c - x) / (c - b);
}

static double smf(double x, double a, double b) {
    double mid;
    if (x <= a) return 0.0;
    if (x >= b) return 1.0;
    mid = (a + b) * 0.5;
    if (x <= mid) {
        double t = (x - a) / (b - a);
        return 2.0 * t * t;
    } else {
        double t = (b - x) / (b - a);
        return 1.0 - 2.0 * t * t;
    }
}

static double zmf(double x, double a, double b) {
    return 1.0 - smf(x, a, b);
}

static double pain_membership(double x, int index) {
    switch (index) {
    case 1: return zmf(x, -1.0, -0.5);
    case 2: return trimf(x, -1.0, -0.5, 0.0);
    case 3: return trimf(x, -0.5, 0.0, 0.5);
    case 4: return trimf(x, 0.0, 0.5, 1.0);
    case 5: return smf(x, 0.5, 1.0);
    default: return 0.0;
    }
}

static double resistance_membership(double x, int index) {
    switch (index) {
    case 1: return zmf(x, 0.0, 0.25);
    case 2: return trimf(x, 0.0, 0.25, 0.5);
    case 3: return trimf(x, 0.25, 0.5, 0.75);
    case 4: return trimf(x, 0.5, 0.75, 1.0);
    case 5: return smf(x, 0.75, 1.0);
    default: return 0.0;
    }
}

static double angle_membership(double x, int index) {
    switch (index) {
    case 1: return zmf(x, -6.0, -3.0);
    case 2: return trimf(x, -6.0, -3.0, 0.0);
    case 3: return trimf(x, -3.0, 0.0, 3.0);
    case 4: return trimf(x, 0.0, 3.0, 6.0);
    case 5: return smf(x, 3.0, 6.0);
    default: return 0.0;
    }
}

static int solve_linear_14(double a[14][15], double out[14]) {
    int col;
    for (col = 0; col < 14; col++) {
        int pivot = col;
        int row;
        double max_value = fabs(a[col][col]);
        for (row = col + 1; row < 14; row++) {
            double value = fabs(a[row][col]);
            if (value > max_value) {
                max_value = value;
                pivot = row;
            }
        }
        if (max_value < 1.0e-12) return 0;
        if (pivot != col) {
            int k;
            for (k = col; k < 15; k++) {
                double tmp = a[col][k];
                a[col][k] = a[pivot][k];
                a[pivot][k] = tmp;
            }
        }
        for (row = col + 1; row < 14; row++) {
            double factor = a[row][col] / a[col][col];
            int k;
            if (factor == 0.0) continue;
            for (k = col; k < 15; k++) a[row][k] -= factor * a[col][k];
        }
    }

    for (col = 13; col >= 0; col--) {
        double value = a[col][14];
        int k;
        for (k = col + 1; k < 14; k++) value -= a[col][k] * out[k];
        out[col] = value / a[col][col];
    }
    return 1;
}

static double polyval_descending(const double* coeffs, int count, double x) {
    double y = 0.0;
    int i;
    for (i = 0; i < count; i++) y = y * x + coeffs[i];
    return y;
}

static int parse_doubles(const char* text, double* out_values, int max_values) {
    int count = 0;
    const char* p = text;
    while (p && *p && count < max_values) {
        char* endp = 0;
        double value;
        while (*p && !(isdigit((unsigned char)*p) || *p == '-' || *p == '+')) p++;
        if (!*p) break;
        value = strtod(p, &endp);
        if (endp == p) {
            p++;
            continue;
        }
        out_values[count++] = value;
        p = endp;
    }
    return count;
}

static int clamp_i16_from_scaled(double value, double scale) {
    double scaled;
    if (scale == 0.0) return 0;
    scaled = value / scale;
    if (scaled > 32767.0) return 32767;
    if (scaled < -32768.0) return -32768;
    return (int)scaled;
}

static int write_typed_value(unsigned char* out, int data_type, int value) {
    if (!out) return 0;
    if (data_type == 1) {
        write_u16(out, (uint16_t)value);
        return 2;
    }
    if (data_type == 2) {
        write_i16(out, (int16_t)value);
        return 2;
    }
    if (data_type == 3) {
        write_u32(out, (uint32_t)value);
        return 4;
    }
    if (data_type == 4) {
        write_i32(out, (int32_t)value);
        return 4;
    }
    write_f32(out, (float)value);
    return 4;
}

CLINIC_API int clinic_can_build_command(int id, int command, const unsigned char* payload, int payload_len, int remote_frame, unsigned char* out_frame16) {
    int can_id;
    int i;
    unsigned char can[11];
    if (!out_frame16 || payload_len < 0 || payload_len > 8) return 0;
    memset(can, 0, sizeof(can));
    memset(out_frame16, 0, 16);
    can[0] = 0x08;
    can_id = (id << 5) + command;
    can[1] = (unsigned char)((can_id >> 8) & 0xFF);
    can[2] = (unsigned char)(can_id & 0xFF);
    if (payload && payload_len > 0) memcpy(can + 3, payload, (size_t)payload_len);
    out_frame16[0] = 0xAA;
    out_frame16[2] = remote_frame ? 0x01 : 0x00;
    out_frame16[3] = 0x08;
    for (i = 0; i < 10; i++) out_frame16[6 + i] = can[1 + i];
    return 16;
}

CLINIC_API int clinic_can_decode_motor_state(const unsigned char* frame16, ClinicMotorState* out_state) {
    unsigned char cdata[11];
    int i;
    if (!frame16 || !out_state || frame16[0] != 0xAA || frame16[3] != 0x08) return 0;
    memset(cdata, 0, sizeof(cdata));
    cdata[0] = 0x08;
    for (i = 0; i < 10; i++) cdata[1 + i] = frame16[6 + i];
    out_state->id = ((cdata[1] * 256 + cdata[2] - 1) >> 5);
    out_state->angle = (double)read_f32(cdata + 3);
    out_state->speed = (double)read_i16(cdata + 7) * 0.01;
    out_state->torque = (double)read_i16(cdata + 9) * 0.01;
    return 1;
}

CLINIC_API int clinic_motor_property_frame(int id, int address, int data_type, int value, unsigned char* out_frame16) {
    unsigned char payload[8];
    int value_len;
    memset(payload, 0, sizeof(payload));
    write_u16(payload, (uint16_t)address);
    write_u16(payload + 2, (uint16_t)data_type);
    value_len = write_typed_value(payload + 4, data_type, value);
    if (value_len <= 0 || value_len + 4 > 8) return 0;
    return clinic_can_build_command(id, 0x1F, payload, value_len + 4, 0, out_frame16);
}

CLINIC_API int clinic_motor_preset_angle_frame(int id, double angle, double speed_or_time, double param, int mode, unsigned char* out_frame16) {
    unsigned char payload[8];
    const double factor = 0.01;
    double p = param;
    memset(payload, 0, sizeof(payload));
    write_f32(payload, (float)angle);
    if (mode == 1) {
        write_i16(payload + 4, (int16_t)clamp_i16_from_scaled(fabs(speed_or_time), factor));
        write_i16(payload + 6, (int16_t)clamp_i16_from_scaled(fabs(param), factor));
    } else if (mode == 2) {
        write_i16(payload + 4, (int16_t)clamp_i16_from_scaled(speed_or_time, factor));
        write_i16(payload + 6, (int16_t)clamp_i16_from_scaled(param, factor));
    } else {
        if (p < 0.0) p = -p;
        if (p > 300.0) p = 300.0;
        write_i16(payload + 4, (int16_t)clamp_i16_from_scaled(fabs(speed_or_time), factor));
        write_i16(payload + 6, (int16_t)clamp_i16_from_scaled(p, factor));
    }
    return clinic_can_build_command(id, 0x0C, payload, 8, 0, out_frame16);
}

CLINIC_API int clinic_motor_execute_frame(int mode, unsigned char* out_frame16) {
    unsigned char payload[6];
    uint32_t order = 0x10U;
    memset(payload, 0, sizeof(payload));
    if (mode == 1) order = 0x11U;
    else if (mode == 2) order = 0x12U;
    write_u32(payload, order);
    write_u16(payload + 4, 0);
    return clinic_can_build_command(0, 0x08, payload, 6, 0, out_frame16);
}

CLINIC_API int clinic_motor_simple_order_frame(unsigned int order, unsigned char* out_frame16) {
    unsigned char payload[4];
    memset(payload, 0, sizeof(payload));
    write_u32(payload, (uint32_t)order);
    return clinic_can_build_command(0, 0x08, payload, 4, 0, out_frame16);
}

CLINIC_API int clinic_motor_impedance_frame(int id, double kp, double kd, unsigned char* out_frame16) {
    unsigned char payload[8];
    const double factor = 0.001;
    double safe_kp = fabs(kp);
    double safe_kd = fabs(kd);
    memset(payload, 0, sizeof(payload));
    if (safe_kp > 10.0) safe_kp = 10.0;
    if (safe_kd > 10.0) safe_kd = 10.0;
    write_u32(payload, 22U);
    write_i16(payload + 4, (int16_t)clamp_i16_from_scaled(safe_kp, factor));
    write_i16(payload + 6, (int16_t)clamp_i16_from_scaled(safe_kd, factor));
    return clinic_can_build_command(id, 0x08, payload, 8, 0, out_frame16);
}

CLINIC_API double clinic_kinematics_linkage_to_motor(double linkage_angle) {
    int best = 0;
    int i;
    double best_distance = 1.0e100;
    for (i = 0; i < linkage_count(); i++) {
        double distance = fabs(linkage_angle - k_linkage_angles[i]);
        if (distance < best_distance) {
            best_distance = distance;
            best = i;
        }
    }
    return (double)(best + 1);
}

CLINIC_API double clinic_kinematics_motor_to_linkage(double motor_angle) {
    int index = (int)floor(motor_angle + 0.5) - 1;
    if (index < 0) index = 0;
    if (index >= linkage_count()) index = linkage_count() - 1;
    return k_linkage_angles[index];
}

CLINIC_API double clinic_training_total_seconds(const ClinicTrainingPrescription* prescription) {
    double cycle;
    int reps;
    if (!prescription) return 0.0;
    reps = prescription->repetitions > 0 ? prescription->repetitions : 1;
    cycle = fmax(1.0, prescription->travel_seconds) + fmax(0.0, prescription->keep_seconds) + fmax(1.0, prescription->travel_seconds) + fmax(0.0, prescription->rest_seconds);
    return reps * cycle;
}

CLINIC_API int clinic_training_step(const ClinicTrainingPrescription* prescription, double elapsed_seconds, double* last_command_second, ClinicTrainingStep* out_step) {
    double cycle;
    double total;
    double cycle_time;
    double travel;
    double keep;
    double send_tick;
    if (!prescription || !out_step) return 0;
    memset(out_step, 0, sizeof(*out_step));
    total = clinic_training_total_seconds(prescription);
    if (elapsed_seconds >= total) {
        out_step->is_complete = 1;
        out_step->progress = 1.0;
        out_step->phase = 4;
        return 1;
    }
    travel = fmax(1.0, prescription->travel_seconds);
    keep = fmax(0.0, prescription->keep_seconds);
    cycle = fmax(1.0, prescription->travel_seconds) + keep + fmax(1.0, prescription->travel_seconds) + fmax(0.0, prescription->rest_seconds);
    cycle_time = fmod(elapsed_seconds, cycle);
    out_step->target_angle = prescription->start_angle;
    out_step->phase = 3;
    if (cycle_time < travel) {
        out_step->target_angle = clinic_segmented_trace_point(travel, prescription->start_angle, prescription->end_angle, cycle_time);
        out_step->phase = 0;
    } else if (cycle_time < travel + keep) {
        out_step->target_angle = prescription->end_angle;
        out_step->phase = 1;
    } else if (cycle_time < travel + keep + travel) {
        out_step->target_angle = clinic_segmented_trace_point(travel, prescription->end_angle, prescription->start_angle, cycle_time - travel - keep);
        out_step->phase = 2;
    }
    out_step->progress = total <= 0.0 ? 1.0 : elapsed_seconds / total;
    send_tick = floor(elapsed_seconds * 10.0) / 10.0;
    if (!last_command_second || fabs(send_tick - *last_command_second) >= 0.099) {
        out_step->should_send_motor_command = 1;
        if (last_command_second) *last_command_second = send_tick;
    }
    out_step->use_impedance = (prescription->mode == 1 || prescription->mode == 3);
    out_step->assist_torque = out_step->use_impedance ? prescription->against_power : 0.0;
    out_step->kp = out_step->use_impedance ? prescription->against_power : 0.0;
    out_step->kd = out_step->use_impedance ? 0.1 : 0.0;
    return 1;
}

CLINIC_API double clinic_segmented_trace_point(double travel_seconds, double start_angle, double end_angle, double elapsed_seconds) {
    double t1;
    double t2;
    double t3;
    double matrix[14][15];
    double b[14];
    double x;
    int r;
    int c;
    if (travel_seconds <= 0.0) return start_angle;
    t1 = 0.25 * travel_seconds;
    t2 = 0.5 * travel_seconds;
    t3 = 0.25 * travel_seconds;
    if (t1 <= 0.0 || t2 <= 0.0 || t3 <= 0.0) return smooth_interpolate(start_angle, end_angle, elapsed_seconds / travel_seconds);
    memset(matrix, 0, sizeof(matrix));
    for (r = 0; r < 14; r++) {
        for (c = 0; c < 15; c++) matrix[r][c] = 0.0;
    }

    matrix[0][0] = pow(t1, 5); matrix[0][1] = pow(t1, 4); matrix[0][2] = pow(t1, 3); matrix[0][3] = pow(t1, 2); matrix[0][4] = t1;
    matrix[1][0] = 5 * pow(t1, 4); matrix[1][1] = 4 * pow(t1, 3); matrix[1][2] = 3 * pow(t1, 2); matrix[1][3] = 2 * t1; matrix[1][4] = 1;
    matrix[2][0] = 20 * pow(t1, 3); matrix[2][1] = 12 * pow(t1, 2); matrix[2][2] = 6 * t1; matrix[2][3] = 2;
    matrix[3][0] = 60 * pow(t1, 2); matrix[3][1] = 24 * t1; matrix[3][2] = 6;

    matrix[0][5] = 1; matrix[0][7] = -1;
    matrix[1][6] = -1;

    matrix[4][6] = t2; matrix[4][7] = 1;
    matrix[5][6] = 1;
    matrix[8][8] = pow(t3, 5);
    matrix[4][13] = -1;
    matrix[5][12] = -1;
    matrix[6][11] = -2;
    matrix[7][10] = -6;
    matrix[8][9] = pow(t3, 4); matrix[8][10] = pow(t3, 3); matrix[8][11] = pow(t3, 2); matrix[8][12] = t3; matrix[8][13] = 1;

    matrix[12][4] = 1;
    matrix[13][3] = 2;
    matrix[9][8] = 5 * pow(t3, 4);
    matrix[10][8] = 20 * pow(t3, 3);
    matrix[11][5] = 1;
    matrix[9][9] = 4 * pow(t3, 3); matrix[9][10] = 3 * pow(t3, 2); matrix[9][11] = 2 * t3; matrix[9][12] = 1;
    matrix[10][9] = 12 * pow(t3, 2); matrix[10][10] = 6 * t3; matrix[10][11] = 2;

    matrix[8][14] = end_angle;
    matrix[11][14] = start_angle;

    if (!solve_linear_14(matrix, b)) return smooth_interpolate(start_angle, end_angle, elapsed_seconds / travel_seconds);
    x = clamp_double(elapsed_seconds, 0.0, travel_seconds);
    if (x <= t1) {
        double coeffs[6] = { b[0], b[1], b[2], b[3], b[4], b[5] };
        return polyval_descending(coeffs, 6, x);
    }
    if (x <= t1 + t2) {
        double coeffs[2] = { b[6], b[7] };
        return polyval_descending(coeffs, 2, x - t1);
    }
    {
        double coeffs[6] = { b[8], b[9], b[10], b[11], b[12], b[13] };
        return polyval_descending(coeffs, 6, x - t1 - t2);
    }
}

CLINIC_API int clinic_calibration_max_angle(const double* motor_angles, int sample_count, int channel_count, double* out_motor_angle, double* out_linkage_angle) {
    int i;
    double max_motor = 0.0;
    if (!motor_angles || sample_count <= 0 || channel_count <= 0 || !out_motor_angle || !out_linkage_angle) return 0;
    for (i = 0; i < sample_count; i++) {
        int ch;
        double sum = 0.0;
        int used = 0;
        for (ch = 0; ch < channel_count; ch++) {
            sum += fabs(motor_angles[i * channel_count + ch]);
            used++;
        }
        if (used > 0) {
            double mean_abs = sum / used;
            if (mean_abs > max_motor) max_motor = mean_abs;
        }
    }
    *out_motor_angle = max_motor;
    *out_linkage_angle = clinic_kinematics_motor_to_linkage(max_motor);
    return 1;
}

CLINIC_API double clinic_fuzzy_angle_adjustment(double pain_level, double joint_resistance) {
    static const int rules[25][3] = {
        {1,1,5}, {1,2,5}, {1,3,4}, {1,4,3}, {1,5,3},
        {2,1,5}, {2,2,4}, {2,3,3}, {2,4,3}, {2,5,3},
        {3,1,5}, {3,2,4}, {3,3,3}, {3,4,3}, {3,5,2},
        {4,1,4}, {4,2,3}, {4,3,3}, {4,4,3}, {4,5,2},
        {5,1,3}, {5,2,3}, {5,3,3}, {5,4,2}, {5,5,2}
    };
    double numerator = 0.0;
    double denominator = 0.0;
    int sample;
    pain_level = clamp_double(pain_level, -1.0, 1.0);
    joint_resistance = clamp_double(joint_resistance, 0.0, 1.0);
    for (sample = 0; sample <= 480; sample++) {
        double x = -6.0 + 12.0 * (double)sample / 480.0;
        double aggregate = 0.0;
        int rule;
        for (rule = 0; rule < 25; rule++) {
            double strength = pain_membership(pain_level, rules[rule][0]);
            double resistance = resistance_membership(joint_resistance, rules[rule][1]);
            double output;
            if (resistance < strength) strength = resistance;
            output = angle_membership(x, rules[rule][2]);
            if (output > strength) output = strength;
            if (output > aggregate) aggregate = output;
        }
        numerator += x * aggregate;
        denominator += aggregate;
    }
    if (denominator <= 1.0e-12) return 0.0;
    return numerator / denominator;
}

CLINIC_API double clinic_adaptive_resistance_next_angle(double current_linkage_angle, double machine_max_degree, int stop_count, int complete_count, int update_time) {
    double next = current_linkage_angle;
    if (update_time <= 0) update_time = 5;
    if (stop_count >= update_time - 1) next -= 2.0;
    if (complete_count >= update_time - 1) {
        next += 1.0;
        if (machine_max_degree > 0.0 && next > machine_max_degree - 0.5) next = machine_max_degree - 0.5;
    }
    if (next < 0.0) next = 0.0;
    return next;
}

CLINIC_API int clinic_static_traction_step(double set_load, const double* initial_force4, const double* current_force4, int extensions_used, int stopped, double* out_real_load, int* out_command) {
    int i;
    double real_load = 0.0;
    if (!initial_force4 || !current_force4 || !out_real_load || !out_command) return 0;
    for (i = 0; i < 4; i++) real_load += current_force4[i] - initial_force4[i];
    *out_real_load = real_load;
    if (stopped) {
        *out_command = 3;
    } else if (real_load < set_load && extensions_used < 3) {
        *out_command = 1;
    } else {
        *out_command = 2;
    }
    return 1;
}

CLINIC_API int clinic_adaptive_inflation_should_hold(const double* pressure8, const double* pressure_limit8, int stage) {
    double default_limit[8] = {15,15,15,15,15,15,15,15};
    const double* limit = pressure_limit8 ? pressure_limit8 : default_limit;
    if (!pressure8) return 0;
    if (stage == 1) {
        double thigh_limit[3] = {9,9,5};
        int ready = 0;
        int over_limit = 0;
        int i;
        for (i = 0; i < 3; i++) {
            if (pressure8[i] >= thigh_limit[i]) ready++;
            if (pressure8[i] > limit[i]) over_limit++;
        }
        return ready == 3 || over_limit >= 1;
    }
    if (stage == 2) {
        int ready = 0;
        int over_limit = 0;
        int i;
        for (i = 4; i < 8; i++) {
            if (pressure8[i] >= 5.0) ready++;
            if (pressure8[i] > limit[i]) over_limit++;
        }
        return ready >= 3 || over_limit >= 1;
    }
    return 0;
}

CLINIC_API int clinic_pressure_lamp_states(const double* pressure8, const double* pressure_well8, const double* pressure_limit8, int* out_states8) {
    int i;
    if (!pressure8 || !pressure_well8 || !pressure_limit8 || !out_states8) return 0;
    for (i = 0; i < 8; i++) {
        if (pressure8[i] >= pressure_limit8[i]) out_states8[i] = 2;
        else if (pressure8[i] >= pressure_well8[i]) out_states8[i] = 1;
        else out_states8[i] = 0;
    }
    return 1;
}

CLINIC_API double clinic_adc_to_pressure_newton(double adc_value) {
    double voltage = (5000.0 * adc_value) / 1024.0;
    double gram_force = (10000.0 * voltage) / 3300.0;
    return gram_force * 9.8 / 1000.0;
}

CLINIC_API int clinic_opensignal_parse_first_sample(const char* text, double* out_values, int max_values, int* out_count) {
    const char* p;
    int count;
    if (!text || !out_values || max_values <= 0) return 0;
    p = strstr(text, "[[");
    if (p) text = p;
    count = parse_doubles(text, out_values, max_values);
    if (out_count) *out_count = count;
    return count > 0;
}

CLINIC_API int clinic_opensignal_parse_config(const char* text, char* out_device_id, int device_id_len, int* out_active_channels, int max_active_channels, int* out_active_count, int* out_sampling_freq) {
    const char* active_pos;
    const char* sampling_pos;
    const char* return_data;
    int active_count = 0;
    if (out_active_count) *out_active_count = 0;
    if (out_sampling_freq) *out_sampling_freq = 0;
    if (out_device_id && device_id_len > 0) out_device_id[0] = '\0';
    if (!text) return 0;

    return_data = strstr(text, "\"returnData\"");
    if (return_data && out_device_id && device_id_len > 1) {
        const char* p = strchr(return_data, '{');
        if (p) {
            const char* q1 = strchr(p + 1, '"');
            const char* q2 = q1 ? strchr(q1 + 1, '"') : 0;
            if (q1 && q2 && q2 > q1 + 1) {
                int len = (int)(q2 - q1 - 1);
                if (len >= device_id_len) len = device_id_len - 1;
                memcpy(out_device_id, q1 + 1, (size_t)len);
                out_device_id[len] = '\0';
            }
        }
    }

    active_pos = strstr(text, "\"activeChannels\"");
    if (active_pos && out_active_channels && max_active_channels > 0) {
        const char* bracket = strchr(active_pos, '[');
        const char* p = bracket ? bracket + 1 : active_pos;
        while (*p && *p != ']' && active_count < max_active_channels) {
            while (*p && !(isdigit((unsigned char)*p) || *p == '-' || *p == '+') && *p != ']') p++;
            if (*p == ']') break;
            if (*p) {
                out_active_channels[active_count++] = (int)strtol(p, (char**)&p, 10);
            }
        }
    }
    if (out_active_count) *out_active_count = active_count;

    sampling_pos = strstr(text, "\"samplingFreq\"");
    if (sampling_pos && out_sampling_freq) {
        const char* p = sampling_pos;
        while (*p && !(isdigit((unsigned char)*p) || *p == '-' || *p == '+')) p++;
        if (*p) *out_sampling_freq = (int)strtol(p, 0, 10);
    }
    return (active_count > 0) || (out_sampling_freq && *out_sampling_freq > 0) || (out_device_id && out_device_id[0] != '\0');
}

CLINIC_API int clinic_stretch_parse_feedback(const char* text, ClinicStretchFeedback* out_feedback) {
    double values[32];
    int count;
    int i;
    int pressure_start;
    if (!text || !out_feedback) return 0;
    memset(out_feedback, 0, sizeof(*out_feedback));
    count = parse_doubles(text, values, 32);
    if (count < 8) return 0;
    for (i = 0; i < 4 && i < count; i++) out_feedback->position[i] = values[i];
    pressure_start = count >= 12 ? 4 : 0;
    for (i = 0; i < 8 && pressure_start + i < count; i++) {
        out_feedback->pressure[i] = values[pressure_start + i];
        out_feedback->total_pressure += out_feedback->pressure[i];
    }
    return 1;
}

CLINIC_API int clinic_emg_extract_features(const double* values, int sample_count, int channel_count, int sample_rate, double* out_features, int max_features, int* out_feature_count) {
    int ch;
    int channels;
    int base_std;
    int base_rms;
    int base_wavelength;
    int base_bandpower;
    if (out_feature_count) *out_feature_count = 0;
    if (!values || sample_count <= 0 || channel_count <= 0 || !out_features || max_features <= 0) return 0;
    channels = channel_count < 4 ? channel_count : 4;
    if (max_features < channels * 5) return 0;
    base_std = channels;
    base_rms = channels * 2;
    base_wavelength = channels * 3;
    base_bandpower = channels * 4;
    for (ch = 0; ch < channels; ch++) {
        int i;
        double mean_abs = 0.0;
        double mean = 0.0;
        double variance = 0.0;
        double rms = 0.0;
        double wavelength = 0.0;
        double bandpower = 0.0;
        int band_bins = 0;
        int first_bin;
        int last_bin;
        for (i = 0; i < sample_count; i++) {
            double v = values[i * channel_count + ch];
            mean_abs += fabs(v);
            mean += v;
            rms += v * v;
            if (i > 0) wavelength += fabs(v - values[(i - 1) * channel_count + ch]);
        }
        mean_abs /= sample_count;
        mean /= sample_count;
        rms = sqrt(rms / sample_count);
        for (i = 0; i < sample_count; i++) {
            double d = values[i * channel_count + ch] - mean;
            variance += d * d;
        }
        variance = sqrt(variance / sample_count);
        out_features[ch] = mean_abs;
        out_features[base_std + ch] = variance;
        out_features[base_rms + ch] = rms;
        out_features[base_wavelength + ch] = wavelength / sample_count;

        first_bin = sample_rate > 0 ? (int)ceil(20.0 * sample_count / sample_rate) : 0;
        last_bin = sample_rate > 0 ? (int)floor(500.0 * sample_count / sample_rate) : 0;
        if (first_bin < 1) first_bin = 1;
        if (last_bin > sample_count / 2) last_bin = sample_count / 2;
        for (i = first_bin; i <= last_bin; i++) {
            int n;
            double real = 0.0;
            double imag = 0.0;
            double omega = -2.0 * 3.14159265358979323846 * (double)i / (double)sample_count;
            for (n = 0; n < sample_count; n++) {
                double v = values[n * channel_count + ch];
                double angle = omega * n;
                real += v * cos(angle);
                imag += v * sin(angle);
            }
            bandpower += (real * real + imag * imag) / ((double)sample_count * (double)sample_count);
            band_bins++;
        }
        out_features[base_bandpower + ch] = band_bins > 0 ? bandpower / band_bins : 0.0;
    }
    if (out_feature_count) *out_feature_count = channels * 5;
    return channels > 0;
}

#ifdef __cplusplus
static svm_node* build_svm_nodes(const double* features, int feature_count) {
    int i;
    svm_node* nodes;
    if (!features || feature_count <= 0) return 0;
    nodes = (svm_node*)calloc((size_t)feature_count + 1, sizeof(svm_node));
    if (!nodes) return 0;
    for (i = 0; i < feature_count; i++) {
        nodes[i].index = i + 1;
        nodes[i].value = features[i];
    }
    nodes[feature_count].index = -1;
    nodes[feature_count].value = 0.0;
    return nodes;
}

static void clinic_svm_silent_print(const char* text) {
    (void)text;
}

CLINIC_API ClinicSvmModelHandle clinic_svm_train(const double* labels, const double* features, int sample_count, int feature_count, const char* model_path) {
    svm_problem problem;
    svm_parameter parameter;
    svm_model* model = 0;
    svm_model* loaded_model = 0;
    svm_node** x = 0;
    const char* error_msg;
    int i;
    if (!labels || !features || sample_count <= 0 || feature_count <= 0 || !model_path || !model_path[0]) return 0;

    memset(&problem, 0, sizeof(problem));
    memset(&parameter, 0, sizeof(parameter));
    problem.l = sample_count;
    problem.y = (double*)calloc((size_t)sample_count, sizeof(double));
    x = (svm_node**)calloc((size_t)sample_count, sizeof(svm_node*));
    if (!problem.y || !x) goto fail;
    for (i = 0; i < sample_count; i++) {
        problem.y[i] = labels[i];
        x[i] = build_svm_nodes(features + i * feature_count, feature_count);
        if (!x[i]) goto fail;
    }
    problem.x = x;

    parameter.svm_type = C_SVC;
    parameter.kernel_type = LINEAR;
    parameter.degree = 3;
    parameter.gamma = feature_count > 0 ? 1.0 / feature_count : 0.0;
    parameter.coef0 = 0.0;
    parameter.cache_size = 100.0;
    parameter.eps = 0.001;
    parameter.C = 1.0;
    parameter.nr_weight = 0;
    parameter.weight_label = 0;
    parameter.weight = 0;
    parameter.nu = 0.5;
    parameter.p = 0.1;
    parameter.shrinking = 1;
    parameter.probability = 0;

    svm_set_print_string_function(&clinic_svm_silent_print);
    error_msg = svm_check_parameter(&problem, &parameter);
    if (error_msg) goto fail;
    model = svm_train(&problem, &parameter);
    if (!model) goto fail;
    if (svm_save_model(model_path, model) != 0) goto fail;
    svm_free_and_destroy_model(&model);

    for (i = 0; i < sample_count; i++) free(x[i]);
    free(x);
    free(problem.y);
    loaded_model = svm_load_model(model_path);
    return (ClinicSvmModelHandle)loaded_model;

fail:
    if (x) {
        for (i = 0; i < sample_count; i++) free(x[i]);
        free(x);
    }
    free(problem.y);
    if (model) svm_free_and_destroy_model(&model);
    return 0;
}

CLINIC_API ClinicSvmModelHandle clinic_svm_load(const char* model_path) {
    if (!model_path || !model_path[0]) return 0;
    svm_set_print_string_function(&clinic_svm_silent_print);
    return (ClinicSvmModelHandle)svm_load_model(model_path);
}

CLINIC_API int clinic_svm_predict(ClinicSvmModelHandle handle, const double* features, int feature_count, double* out_label, double* out_score) {
    svm_model* model = (svm_model*)handle;
    svm_node* nodes;
    double* decision_values = 0;
    int decision_count;
    int class_count;
    double label;
    if (!model || !features || feature_count <= 0 || !out_label) return 0;
    nodes = build_svm_nodes(features, feature_count);
    if (!nodes) return 0;
    class_count = svm_get_nr_class(model);
    decision_count = class_count > 1 ? class_count * (class_count - 1) / 2 : 1;
    decision_values = (double*)calloc((size_t)decision_count, sizeof(double));
    if (!decision_values) {
        free(nodes);
        return 0;
    }
    label = svm_predict_values(model, nodes, decision_values);
    *out_label = label;
    if (out_score) *out_score = decision_values[0];
    free(decision_values);
    free(nodes);
    return 1;
}

CLINIC_API void clinic_svm_free(ClinicSvmModelHandle handle) {
    svm_model* model = (svm_model*)handle;
    if (model) svm_free_and_destroy_model(&model);
}
#else
CLINIC_API ClinicSvmModelHandle clinic_svm_train(const double* labels, const double* features, int sample_count, int feature_count, const char* model_path) {
    (void)labels; (void)features; (void)sample_count; (void)feature_count; (void)model_path; return 0;
}
CLINIC_API ClinicSvmModelHandle clinic_svm_load(const char* model_path) {
    (void)model_path; return 0;
}
CLINIC_API int clinic_svm_predict(ClinicSvmModelHandle handle, const double* features, int feature_count, double* out_label, double* out_score) {
    (void)handle; (void)features; (void)feature_count; (void)out_label; (void)out_score; return 0;
}
CLINIC_API void clinic_svm_free(ClinicSvmModelHandle handle) {
    (void)handle;
}
#endif
