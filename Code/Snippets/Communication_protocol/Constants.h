#pragma once
/****************************************************************************************
* Helping hand constants
****************************************************************************************/

#define DEBUG false

// Delays
#define DELAY_NO_DATA 2
#define DELAY_DATA_LOOP 5
#define DELAY_CLIENT_CHECK_LOOP 500

// Command encodings
#define COMMAND_COUNT 3
#define RIGHT_AXIS_X 0
#define RIGHT_AXIS_Y 1
#define RIGHT_TRIGGER 2

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
#define MAX_CHAR_INPUT_SIZE 60

// Pins
#define PIN_GIMBAL_YAW A0
#define PIN_GIMBAL_PITCH A1

int motor_inA_Pins[2] = { 7, 4 }; // INA: Clockwise input
int motor_inB_Pins[2] = { 8, 9 }; // INB: Counter-clockwise input
int motor_PWM_Pins[2] = { 5, 6 }; // PWM input
