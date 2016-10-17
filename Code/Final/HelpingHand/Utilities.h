#include <Servo.h>
#include <Console.h>
#include "Constants.h"
/***********************
* Helping hand Utilities
************************/

extern void driveMotor(int motor, int direct, int pwm);

struct ServoData {
	int currentPos;
	Servo servo;
	void setup(int pin, int angle) {
		servo.attach(pin);
		setAngle(angle);
	}
	void setAngle(int angle)
	{
		if (currentPos == angle)
			return;
		servo.write(angle);
		currentPos = angle;
	}
};

struct MotorData {
	int motorId;
	int speed;
	int isCcw;

	void setup(int id) {
		motorId = id;
		speed = 0;
		isCcw = 0;
	}

	void setSpeed(int pwm) {
		if (speed == pwm)
			return;
		driveMotor(motorId, isCcw == 1 ? 2 : 1, pwm);
		speed = pwm;
	}

	void setDirection(int ccw) {
		if (isCcw == ccw)
			return;
		driveMotor(motorId, ccw == 1 ? 2 : 1, speed);
		isCcw = ccw;
	}

	void brake(int vcc) {
		driveMotor(motorId, vcc == 1 ? 0 : 3, speed);
	}
};

struct Servos {
	struct ServoData GimbalYaw;
	struct ServoData GimbalPitch;
};

struct Motors {
	struct MotorData LeftMotor;
	struct MotorData RightMotor;
};