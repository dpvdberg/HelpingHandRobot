#include "Utilities.h"

///
/// Sets motor speeds and directions
///
/// direct: Should be between 0 and 3, with the following result
/// 0 : Brake to VCC
/// 1 : Clockwise
/// 2 : CounterClockwise
/// 3 : Brake to GND
///
/// pwm: Should be in the interval [0,255]
///
void driveMotor(int motor, int setting, int pwm)
{
	if (motor <= 1 && setting <= 4)
	{
		// Set inA[motor]
		if (setting <= 1)
			digitalWrite(motor_inA_Pins[motor], HIGH);
		else
			digitalWrite(motor_inA_Pins[motor], LOW);

		// Set inB[motor]
		if ((setting == 0) || (setting == 2))
			digitalWrite(motor_inB_Pins[motor], HIGH);
		else
			digitalWrite(motor_inB_Pins[motor], LOW);

		analogWrite(motor_PWM_Pins[motor], pwm);
	}
}