#include <Servo.h>
#include <Console.h>
#include "Constants.h"
/***********************
* Helping hand Utilities
************************/

class ServoData {
	int _currentPos;
	int _pin;
	public:
		Servo servo;

		void initialize(int pin, int angle) {
			servo.attach(pin);
			setAngle(angle);

			_pin = pin;
			_currentPos = angle;
		}

		void setAngle(int angle)
		{
			if (_currentPos == angle)
				return;
			servo.write(angle);
			_currentPos = angle;
		}
};

class MotorData {
	bool _isLeft;
	int getMotorId() { return _isLeft ? 0 : 1; };
	public:
		int pin_inA;
		int pin_inB;
		int pin_pwm;
		int speed;
		int isCcw;

		void initialize(bool isLeft, int inApin, int inBpin, int pwmPin) {
			_isLeft = isLeft;
			pin_inA = inApin;
			pin_inB = inBpin;
			pin_pwm = pwmPin;

			// Initialize pins
			digitalWrite(pin_inA, OUTPUT);
			digitalWrite(pin_inB, OUTPUT);
			digitalWrite(pin_pwm, OUTPUT);

			// Set motors braked
			brake(0);
		}

		void setSpeed(int pwm);
		void setDirection(int ccw);
		void brake(int vcc);
};

struct Servos {
	ServoData GimbalYaw;
	ServoData GimbalPitch;
};

struct Motors {
	MotorData LeftMotor;
	MotorData RightMotor;
};