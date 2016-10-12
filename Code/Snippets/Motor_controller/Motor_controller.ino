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
    //drive(x,y);//continuously drive motor
  }
  void drive(int x, int y)//sends control signal to motor shield
  {
    int speed0;//speed left motor
    int speed1;//speed right motor
    int direction0 = CW; //direction left motor
    int direction1 = CW; //direction right motor
    //calcultateSpeedDirection (x, y, &speed0, &speed1, &direction0, &direction1); //calculate parameters
    adjustMotor(0, 0, 200);//run left motor
    adjustMotor(1, 0, 200);//run right motor
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
  void calcultateSpeedDirection (int x, int y, int *vL, int *vR, int *dL, int *dR) //converts input in controller-format to suitable combination of track movements
  {
    *vL = (x / 2) + y;//compute velociy left track
    if (*vL < 0)// if needed change left direction
    {
      *dL = CCW;
    }
    *vR = ((-x) / 2) + y;//see Left
    if (*vR < 0)
    {
      *dR = CCW;//see Left
    }
    *vL = map(*vL, 0, 8000, 0, 255);
    *vR = map(*vR, 0, 8000, 0, 255);
  }
