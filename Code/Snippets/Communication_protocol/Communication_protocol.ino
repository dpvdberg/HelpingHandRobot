#include "Constants.h"
#include <Console.h>
#include <Bridge.h>
#include <BridgeServer.h>
#include <BridgeClient.h>
#include <Servo.h>
#include "Constants.h"

#define MAX_INPUT_SIZE 30

Servo cameraGimbalYaw;
Servo cameraGimbalPitch;

int cameraGimbalYawPin = A0;
int cameraGimbalPitchPin = A1;

int cameraGimbalYawAngle = 90;
int cameraGimbalPitchAngle = 90;

// Open the bridge socket 5566 to communicate.
BridgeServer server(5566);

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

	cameraGimbalYaw.attach(cameraGimbalYawPin);
	cameraGimbalPitch.attach(cameraGimbalPitchPin);

	cameraGimbalYaw.write(cameraGimbalYawAngle);
	cameraGimbalPitch.write(cameraGimbalPitchAngle);

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
			client.flush();
			while (client.available() == 0)
			{
				delay(DELAY_NO_DATA);
				clientAlivePingCounter++;
				if (clientAlivePingCounter > 1000/DELAY_NO_DATA) {
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

			while (client.available() > 0)
			{
				// Get next command from Serial (add 1 for final 0)
				char input[MAX_INPUT_SIZE + 1];
				byte size = client.readBytes(input, MAX_INPUT_SIZE);
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
	delay(500);
}

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

void handleCommand(int id, int argument) {
	switch (id) {
		case RIGHT_AXIS_X:
			cameraGimbalYaw.write(argument);
			break;
		case RIGHT_AXIS_Y:
			cameraGimbalPitch.write(argument);
			break;
	}
}