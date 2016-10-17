/***********************
* Helping hand constants
************************/

#define DEBUG false

// Delays
#define DELAY_NO_DATA 2
#define DELAY_DATA_LOOP 10
#define DELAY_CLIENT_CHECK_LOOP 500

// Command encodings
#define COMMAND_COUNT 3
#define CMD_GIMBAL_YAW 0
#define CMD_GIMBAL_PITCH 1
#define CMD_LMOTOR_PWM 2
#define CMD_RMOTOR_PWM 3
#define CMD_LMOTOR_DIRECTION 4
#define CMD_RMOTOR_DIRECTION 5

// Motor directions
#define BRAKEVCC 0
#define CW   1
#define CCW  2
#define BRAKEGND 3

// Default values
#define BRIDGE_PORT 5566
#define GIMBAL_YAW_ANGLE 90
#define GIMBAL_PITCH_ANGLE 90

// Intermediate values
#define MAX_CHAR_INPUT_SIZE 100

// Pins
#define PIN_GIMBAL_YAW A0
#define PIN_GIMBAL_PITCH A1

extern int motor_inA_Pins[2]; // INA: Clockwise input
extern int motor_inB_Pins[2]; // INB: Counter-clockwise input
extern int motor_PWM_Pins[2]; // PWM input