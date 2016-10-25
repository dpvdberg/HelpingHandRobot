#include <Console.h>
#include <Bridge.h>
#include <BridgeServer.h>
#include <BridgeClient.h>
#include <Servo.h>
#include "Utilities.h"
#include "Constants.h"

// Servo data
Servos servos;

// Motor data
Motors motors;

// Open the bridge socket 5566 to communicate.
BridgeServer server(BRIDGE_PORT);

///
/// Arduino setup function
///
void setup() {
	Bridge.begin();
	// Setup Console, this is for debug purposes only
	Console.begin();

	// Wait for Console port to connect
	while (!Console) {};

	// Begin listening
	server.noListenOnLocalhost();
	server.begin();

	// Initialize servos
	servos.GimbalPitch.initialize(PIN_GIMBAL_PITCH, GIMBAL_PITCH_ANGLE);
	servos.GimbalYaw.initialize(PIN_GIMBAL_YAW, GIMBAL_YAW_ANGLE);
	servos.ArmRotator.initialize(PIN_ARM_ROTATOR, ARM_ROTATOR_ANGLE);
	servos.ArmShoulder.initialize(PIN_ARM_SHOULDER, ARM_SHOULDER_ANGLE);
	servos.ArmElbow.initialize(PIN_ARM_ELBOW, ARM_ELBOW_ANGLE);
	servos.ArmWrist.initialize(PIN_ARM_WRIST, ARM_WRIST_ANGLE);
	servos.ArmGrabber.initialize(PIN_ARM_GRABBER, ARM_GRABBER_ANGLE);

	// Initialize motors
	motors.LeftMotor.initialize(true,
		PIN_LMOTOR_INA,
		PIN_LMOTOR_INB,
		PIN_LMOTOR_PWM);
	motors.RightMotor.initialize(false,
		PIN_RMOTOR_INA,
		PIN_RMOTOR_INB,
		PIN_RMOTOR_PWM);

	Console.println("Setup done");
}

///
/// Main loop
///
void loop() {
	BridgeClient client = server.accept();

	Console.println("Waiting for client..");
	// There is a new client?
	if (client) {
		// Set low timeout
		client.setTimeout(5);
		// Notify user of new client
		Console.println("We have a new client!");

		// Counter for whenever the client is not sending data
		int clientAlivePingCounter = 0;
		// Boolean to disconnect client
		bool clientStop = false;

		// Loop while client is connected
		// Note: while(client.connected()) is slow.
		while (true) {
			// Reset ping counter
			clientAlivePingCounter = 0;
			// Flush everything on the buffer
			//client.flush();

			// While nothing is on the buffer
			while (client.available() == 0)
			{
				delay(DELAY_NO_DATA);
				clientAlivePingCounter++;
				if (clientAlivePingCounter > DELAY_CHECK_CLIENT_ALIVE / DELAY_NO_DATA && !client.connected()) {
					// Check if client is still alive
					clientStop = true;
					break;
				}
			}

			// If we need to stop the client, exit while loop
			if (clientStop)
				break;

			// While there is still data on the buffer
			while (client.available() > 0)
			{
				// Get next command from client buffer (add 1 for final 0)
				char input[MAX_CHAR_INPUT_SIZE + 1];
				byte size = client.readBytes(input, MAX_CHAR_INPUT_SIZE);
				// Add the final 0 to end the C string
				input[size] = 0;

				parseCommand(input);
			}
			delay(DELAY_DATA_LOOP);
		}

		Console.println("Exiting client..");
		client.stop();

		digitalWrite(13, HIGH);
	}
	delay(DELAY_CLIENT_CHECK_LOOP);
}

///
/// Handles a given command
///
void handleCommand(int id, int argument) {
	// Print if debugging is turned on
	if (DEBUG) {
		char buffer[50];
		sprintf(buffer, "Command: %d, Argument: %d", id, argument);
		Console.println(buffer);
	}

	// Switch over commands
	switch (id) {
	case CMD_GIMBAL_YAW:
		servos.GimbalYaw.setAngle(argument);
		break;
	case CMD_GIMBAL_PITCH:
		servos.GimbalPitch.setAngle(argument);
		break;
	case CMD_LMOTOR_PWM:
		motors.LeftMotor.setSpeed(argument);
		break;
	case CMD_RMOTOR_PWM:
		motors.RightMotor.setSpeed(argument);
		break;
	case CMD_LMOTOR_DIRECTION:
		motors.LeftMotor.setDirection(argument);
		break;
	case CMD_RMOTOR_DIRECTION:
		motors.RightMotor.setDirection(argument);
		break;
	case CMD_ARM_ROTATOR:
		servos.ArmRotator.setAngle(argument);
		break;
	case CMD_ARM_SHOULDER:
		servos.ArmShoulder.setAngle(argument);
		break;
	case CMD_ARM_ELBOW:
		servos.ArmElbow.setAngle(argument);
		break;
	case CMD_ARM_WRIST:
		servos.ArmWrist.setAngle(argument);
		break;
	case CMD_ARM_GRABBER:
		servos.ArmGrabber.setAngle(argument);
		break;
	}
}

///
/// Parses a command given by the client
/// Command example:
///		1:100&2:80&3:200
///
void parseCommand(char* input) {
	// Read each command pair 
	char* command = strtok(input, "&");
	while (command != 0)
	{
		// Split the command in two values
		char* separator = strchr(command, ':');
		if (separator != 0)
		{
			// Actually split the string in 2: replace ':' with 0
			*separator = 0;
			int commandId = atoi(command);
			++separator;
			int commandArg = atoi(separator);

			handleCommand(commandId, commandArg);
		}
		// Find the next command in input string
		command = strtok(0, "&");
	}
}