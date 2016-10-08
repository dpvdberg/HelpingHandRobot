#define FORWARD 0
#define BACK 1
#define LEFTTIGHT 2
#define RIGHTIGHT 3
#define LEFT 4
#define RIGHT 5
void drive(int direction,int speed)
{
  
}
// motor one
int enA = 10;
int in1 = 9;
int in2 = 8;
// motor two
int enB = 5;
int in3 = 7;
int in4 = 6;
void setup() {
    // set all the motor control pins to outputs
  pinMode(enA, OUTPUT);
  pinMode(enB, OUTPUT);
  pinMode(in1, OUTPUT);
  pinMode(in2, OUTPUT);
  pinMode(in3, OUTPUT);
  pinMode(in4, OUTPUT);
}

void loop() {
  // put your main code here, to run repeatedly:

}
