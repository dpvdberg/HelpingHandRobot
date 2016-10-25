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
void driveMotor(MotorData &motor, int setting)
{
	/*
	Console.println("Writing motor data.. ");
	Console.print(motor.pin_inA);
	Console.print(" and ");
	Console.print(motor.pin_inB);
	Console.print(" at ");
	Console.print(motor.pin_pwm);
	Console.print(" speed ");
	Console.print(motor.speed);
	Console.print("\n");
	*/
	if (setting <= 4)
	{
		// Set inA[motor]
		if (setting <= 1)
			digitalWrite(motor.pin_inA, HIGH);
		else
			digitalWrite(motor.pin_inA, LOW);

		// Set inB[motor]
		if ((setting == 0) || (setting == 2))
			digitalWrite(motor.pin_inB, HIGH);
		else
			digitalWrite(motor.pin_inB, LOW);

		analogWrite(motor.pin_pwm, motor.speed);
	}
}


void MotorData::setDirection(int ccw) {
	if (isCcw == ccw)
		return;
	driveMotor(*this, ccw == 1 ? 2 : 1);
	isCcw = ccw;
}

void MotorData::brake(bool vcc) {
	driveMotor(*this, vcc ? 0 : 3);
}

void MotorData::setSpeed(int pwm) {
	if (speed == pwm)
		return;
	driveMotor(*this, isCcw == 1 ? 2 : 1);
	speed = pwm;
}