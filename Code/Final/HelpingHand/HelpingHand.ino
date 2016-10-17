#include <Console.h>
#include <Bridge.h>
#include <BridgeServer.h>
#include <BridgeClient.h>
#include <Servo.h>
#include "Utilities.h"
#include "Constants.h"

// Servo data
struct Servos servos;

// Motor data
struct Motors motors;

// Open the bridge socket 5566 to communicate.
BridgeServer server(BRIDGE_PORT);

// Saves values for commands
int savedCommandValues[COMMAND_COUNT];

int motor_inA_Pins[2] = { 7, 4 }; // INA: Clockwise input
int motor_inB_Pins[2] = { 8, 9 }; // INB: Counter-clockwise input
int motor_PWM_Pins[2] = { 5, 6 }; // PWM input

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

	// Attach servos and set default values
	servos.GimbalPitch.setup(PIN_GIMBAL_PITCH, GIMBAL_PITCH_ANGLE);
	servos.GimbalYaw.setup(PIN_GIMBAL_YAW, GIMBAL_YAW_ANGLE);

	// Initialize motors
	motors.LeftMotor.setup(0);
	motors.RightMotor.setup(1);

	// Initialize pins for motors
	for (int i = 0; i < 2; i++)
	{
		pinMode(motor_inA_Pins[i], OUTPUT);
		pinMode(motor_inB_Pins[i], OUTPUT);
		pinMode(motor_PWM_Pins[i], OUTPUT);
	}
	// Initialize motors braked
	for (int i = 0; i < 2; i++)
	{
		digitalWrite(motor_inA_Pins[i], LOW);
		digitalWrite(motor_inB_Pins[i], LOW);
	}
	// Initialize saved command values
	for (int i = 0; i < COMMAND_COUNT; i++) {
		savedCommandValues[i] = 0;
	}

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
				if (clientAlivePingCounter > 1000 / DELAY_NO_DATA) {
					// Check if client is still alive
					Console.println("No data available, checking if client is alive..");
					if (!client.connected()) {
						clientStop = true;
						break;
					}
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
		client.stop();

		digitalWrite(13, HIGH);
		Console.println("Exiting client..");
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

	// If command is not changed, skip updating
	// This prevents 'flickering' of servos
	if (savedCommandValues[id] == argument)
		return;
	savedCommandValues[id] = argument;

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