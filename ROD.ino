
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
  }
  void drive(int x, int y, int velocity)//sends control signal to motor shield
  {
    int speed0;//speed left motor
    int speed1;//speed right motor
    int direction0;//direction left motor
    int direction1;//direction right motor
    calcultateSpeedDirection (x, y, velocity);//calculate parameters
    adjustMotor(0, direction0, speed0);//run left motor
    adjustMotor (1, direction1, speed1);//run right motor
  }
  void adjustMotor(uint8_t motor, uint8_t direct, uint8_t pwm)//sends signal to run motor
  {

  }
  void calcultateSpeedDirection (x, y, velocity)//converts input in controller-format to suitable combination of track movements
  {

  }

