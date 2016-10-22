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
#define CMD_ARM_ROTATOR 6
#define CMD_ARM_SHOULDER 7
#define CMD_ARM_ELBOW 8
#define CMD_ARM_WRIST 9
#define CMD_ARM_GRABBER 10

// Motor directions
#define BRAKEVCC 0
#define CW   1
#define CCW  2
#define BRAKEGND 3

// Default values
#define BRIDGE_PORT 5566
#define GIMBAL_YAW_ANGLE 90
#define GIMBAL_PITCH_ANGLE 90
#define ARM_ROTATOR_ANGLE 90
#define ARM_SHOULDER_ANGLE 90
#define ARM_ELBOW_ANGLE 90
#define ARM_WRIST_ANGLE 90
#define ARM_GRABBER_ANGLE 90

// Intermediate values
#define MAX_CHAR_INPUT_SIZE 100

// Pins
#define PIN_GIMBAL_YAW 10
#define PIN_GIMBAL_PITCH 11
#define PIN_ARM_ROTATOR A0
#define PIN_ARM_SHOULDER A1
#define PIN_ARM_ELBOW A2
#define PIN_ARM_WRIST A3
#define PIN_ARM_GRABBER A4
#define PIN_LMOTOR_INA 7
#define PIN_LMOTOR_INB 8
#define PIN_LMOTOR_PWM 5
#define PIN_RMOTOR_INA 4
#define PIN_RMOTOR_INB 9
#define PIN_RMOTOR_PWM 6
