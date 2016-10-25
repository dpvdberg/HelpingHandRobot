#define CW 0
#define CCW 1


int pwmpin[2] = {5, 6}; // PWM's input
int inApin[2] = {7, 4}; // INA: Clockwise
int inBpin[2] = {8, 9}; // INB: Counterlockwise




void setup() {
  Serial.begin(9600);
  //set outputs
  for (int i = 0; i < 2; i++)
  {
    pinMode(inApin[i], OUTPUT);
    pinMode(inBpin[i], OUTPUT);
    pinMode(pwmpin[i], OUTPUT);
  }
  //set motor pins low
  for (int i = 0; i < 2; i++)
  {
    digitalWrite(inApin[i], LOW);
    digitalWrite(inBpin[i], LOW);
  }
}



  void loop() {
    int x;
    int y;
    x = 4000;
    y = 4000;

    adjustMotor(0, 0, 200);//run left motor
    adjustMotor(1, 0, 200);//run right motor
    
    delay(500);
  }
  
  void adjustMotor(int motor, int direct, int pwm)//sends signal to run motor
  {
    //if (direct = CW)
    digitalWrite(inApin[motor], HIGH);
    /*else
      digitalWrite(inApin[motor], LOW);
      
    if (direct = CCW)
      digitalWrite(inBpin[motor], HIGH);
    else*/
    digitalWrite(inBpin[motor], LOW);
    analogWrite(pwmpin[motor], pwm);
  }
