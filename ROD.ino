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



  void loop() {
    drive();//continuously drive motor
    Serial.println(i);
  }
  void drive(int x, int y)//sends control signal to motor shield
  {
    int speed0;//speed left motor
    int speed1;//speed right motor
    int direction0 = CW; //direction left motor
    int direction1 = CW; //direction right motor
    calcultateSpeedDirection (x, y);//calculate parameters
    speed0 = map(velocityLeft, 0, controllerMax, 0, 255);
    speed1 = map(velocityRight, 0, controllerMax, 0, 255);
    adjustMotor(0, direction0, speed0);//run left motor
    adjustMotor (1, direction1, speed1);//run right motor
  }
  void adjustMotor(int motor, int direct, int pwm)//sends signal to run motor
  {
    if (direct = CW)
      digitalWrite(inApin[motor], HIGH);
    else
      digitalWrite(inApin[motor], LOW);
    if (direct = CCW)
      digitalWrite(inBpin[motor], HIGH);
    else
      digitalWrite(inBpin[motor], LOW);
    analogWrite(pwmpin[motor], pwm);
  }
  void calcultateSpeedDirection (x, y)//converts input in controller-format to suitable combination of track movements
  {
    int velocityLeft = (x / 2) + y;//compute velociy left track
    if (velocityLeft < 0)// if needed change left direction
    {
      directionLeft = CCW;
    }
    int velocityRight = ((-x) / 2) + y;//see Left
    if (velocityRight < 0)
    {
      directionRight = CCW;//see Left
    }
  }
