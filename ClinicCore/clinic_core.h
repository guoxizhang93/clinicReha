#ifndef CLINIC_CORE_H
#define CLINIC_CORE_H

#ifdef _WIN32
#define CLINIC_API __declspec(dllexport)
#else
#define CLINIC_API
#endif

#ifdef __cplusplus
extern "C" {
#endif

typedef struct ClinicTrainingPrescription {
    int mode;
    double start_angle;
    double end_angle;
    int repetitions;
    double travel_seconds;
    double keep_seconds;
    double rest_seconds;
    double traction_force;
    double against_power;
} ClinicTrainingPrescription;

typedef struct ClinicTrainingStep {
    double target_angle;
    double progress;
    int should_send_motor_command;
    int use_impedance;
    double assist_torque;
    double kp;
    double kd;
    int is_complete;
    int phase;
} ClinicTrainingStep;

typedef struct ClinicMotorState {
    int id;
    double angle;
    double speed;
    double torque;
} ClinicMotorState;

typedef struct ClinicStretchFeedback {
    double position[4];
    double pressure[8];
    double total_pressure;
} ClinicStretchFeedback;

typedef void* ClinicSvmModelHandle;

CLINIC_API int clinic_can_build_command(int id, int command, const unsigned char* payload, int payload_len, int remote_frame, unsigned char* out_frame16);
CLINIC_API int clinic_can_decode_motor_state(const unsigned char* frame16, ClinicMotorState* out_state);
CLINIC_API int clinic_motor_property_frame(int id, int address, int data_type, int value, unsigned char* out_frame16);
CLINIC_API int clinic_motor_preset_angle_frame(int id, double angle, double speed_or_time, double param, int mode, unsigned char* out_frame16);
CLINIC_API int clinic_motor_execute_frame(int mode, unsigned char* out_frame16);
CLINIC_API int clinic_motor_simple_order_frame(unsigned int order, unsigned char* out_frame16);
CLINIC_API int clinic_motor_impedance_frame(int id, double kp, double kd, unsigned char* out_frame16);

CLINIC_API double clinic_kinematics_linkage_to_motor(double linkage_angle);
CLINIC_API double clinic_kinematics_motor_to_linkage(double motor_angle);

CLINIC_API double clinic_training_total_seconds(const ClinicTrainingPrescription* prescription);
CLINIC_API int clinic_training_step(const ClinicTrainingPrescription* prescription, double elapsed_seconds, double* last_command_second, ClinicTrainingStep* out_step);
CLINIC_API double clinic_segmented_trace_point(double travel_seconds, double start_angle, double end_angle, double elapsed_seconds);
CLINIC_API int clinic_calibration_max_angle(const double* motor_angles, int sample_count, int channel_count, double* out_motor_angle, double* out_linkage_angle);
CLINIC_API double clinic_fuzzy_angle_adjustment(double pain_level, double joint_resistance);
CLINIC_API double clinic_adaptive_resistance_next_angle(double current_linkage_angle, double machine_max_degree, int stop_count, int complete_count, int update_time);
CLINIC_API int clinic_static_traction_step(double set_load, const double* initial_force4, const double* current_force4, int extensions_used, int stopped, double* out_real_load, int* out_command);
CLINIC_API int clinic_adaptive_inflation_should_hold(const double* pressure8, const double* pressure_limit8, int stage);
CLINIC_API int clinic_pressure_lamp_states(const double* pressure8, const double* pressure_well8, const double* pressure_limit8, int* out_states8);
CLINIC_API double clinic_adc_to_pressure_newton(double adc_value);

CLINIC_API int clinic_opensignal_parse_first_sample(const char* text, double* out_values, int max_values, int* out_count);
CLINIC_API int clinic_opensignal_parse_config(const char* text, char* out_device_id, int device_id_len, int* out_active_channels, int max_active_channels, int* out_active_count, int* out_sampling_freq);
CLINIC_API int clinic_stretch_parse_feedback(const char* text, ClinicStretchFeedback* out_feedback);
CLINIC_API int clinic_emg_extract_features(const double* values, int sample_count, int channel_count, int sample_rate, double* out_features, int max_features, int* out_feature_count);

CLINIC_API ClinicSvmModelHandle clinic_svm_train(const double* labels, const double* features, int sample_count, int feature_count, const char* model_path);
CLINIC_API ClinicSvmModelHandle clinic_svm_load(const char* model_path);
CLINIC_API int clinic_svm_predict(ClinicSvmModelHandle handle, const double* features, int feature_count, double* out_label, double* out_score);
CLINIC_API void clinic_svm_free(ClinicSvmModelHandle handle);

#ifdef __cplusplus
}
#endif

#endif
