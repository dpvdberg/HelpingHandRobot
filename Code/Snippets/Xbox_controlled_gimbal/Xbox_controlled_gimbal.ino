#include <Console.h>
#include <Bridge.h>
#include <BridgeServer.h>
#include <BridgeClient.h>
#include <Servo.h>  // standard servo library

#define INPUT_SIZE 30

Servo cameraGimbalYaw;
Servo cameraGimbalPitch;

int cameraGimbalYawPin = A0;
int cameraGimbalPitchPin = A1;

int cameraGimbalYawAngle = 90;
int cameraGimbalPitchAngle = 90;

// We open the socket 5566 to communicate.
BridgeServer server(5566);

void setup() {
  // Initialize Console and wait for port to open:
  // Bridge startup
  pinMode(13, OUTPUT);
  digitalWrite(13, LOW);
  Bridge.begin();
  digitalWrite(13, HIGH);
  Console.begin();

  // Wait for Console port to connect
  while (!Console) {};

  server.noListenOnLocalhost();
  server.begin();

  cameraGimbalYaw.attach(cameraGimbalYawPin);
  cameraGimbalPitch.attach(cameraGimbalPitchPin);
  
  cameraGimbalYaw.write(cameraGimbalYawAngle);
  cameraGimbalPitch.write(cameraGimbalPitchAngle);
  
  Console.println("Setup done");
}

void loop() {  
  BridgeClient client = server.accept();
  
  // There is a new client?
  if (client) {
    client.setTimeout(5);
    Console.println("we have a new client");
    String argument;
    int value;
    //digitalWrite(13, LOW);

    int checker = 0;
    bool clientStop = false;
    while (true) {
      client.flush();
      while (client.available() == 0)
      {
        checker++;
        if (checker > 100) {
          checker = 0;
          if (!client.connected()) {
            clientStop = true;
            break;
          }
        }
        delay(5);
      }

      if (clientStop)
        break;
  
      while(client.available() > 0)
      {
        // Get argument
        argument = client.readStringUntil(':');
        Console.println(argument);
        if (argument == "")
          break;
        // Get value for argument
        value = client.parseInt();
        Console.println(value);
  
        // toInt() + servo write == crash :(
        // parseint from string stream + servo write ==  crash :(
        // parseint from int stream + servo write == success :)
          
        if (argument == "GXA") {
          cameraGimbalYaw.write(value);
        } else if (argument == "GYA") {
          cameraGimbalPitch.write(value);
        }
  
        client.readStringUntil('|');
      }      
      int command = client.parseInt();
      Console.println(command);
      
    }
    client.stop();
    digitalWrite(13, HIGH);
    Console.println("exiting client");
  }
  delay(100);
}


String getValue(String data, char separator, int index)
{
  int found = 0;
  int strIndex[] = { 0, -1  };
  int maxIndex = data.length()-1;
  for(int i=0; i<=maxIndex && found<=index; i++){
    if(data.charAt(i)==separator || i==maxIndex){
      found++;
      strIndex[0] = strIndex[1]+1;
      strIndex[1] = (i == maxIndex) ? i+1 : i;
    }
  }
  return found > index ? data.substring(strIndex[0], strIndex[1]) : "";
}
