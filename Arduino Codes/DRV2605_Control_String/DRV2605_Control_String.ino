#include <Wire.h>
#include "Adafruit_DRV2605.h"

const int Motor_PWM_Pins = 16;
const uint32_t Default_LRA_Frequency = 100;
Adafruit_DRV2605 drv;
const int SDA_Pin = 21;
// const int SCL_Pins[] = {23, 22, 19, 18, 5, 17, 0, 11, 10, 9, 8, 32, 35, 34, 39, 36}; //GPIO 2 is available, but the led blinks when GPIO2 connects to I2C pins
const int SCL_Pins[] = {23, 22, 19, 18, 5, 17, 0, 13, 10, 9, 33, 32, 14, 27, 26, 25};
// const int SCL_Pins[] = {23}; // For debug use
//const int SCL_Pins[] = {23, 22, 32, 13, 19, 15, 18, 4, 0, 14, 10, 9}; // For old 12 motor version

// 1(22) 2(19) 5(17) 6(0) 8+9(10,9) 10(8) 11(32) (35) (34) (39) (36)
// 7 8 11 not working - 0513
// 21 16 for other uses
// 8 7 6 because why not
// 2 4 12 13 14 15 25 26 27: read only when wifi mode
// 34 35  36 39 only for input
// 2 12 can't connect when upload
int MotorCount = sizeof(SCL_Pins) / sizeof(int);

void SetMotorVoltage(uint8_t Fraction = 0x89)
{
  drv.writeRegister8(0x16, Fraction);
  drv.writeRegister8(0x17, Fraction);
}

void SetFreq(uint32_t Freq = Default_LRA_Frequency)
{
  //DRV2605 Input PWM Frequency: 10-250kHz
  if(Freq >= 1950){
    Freq = 1900;    //DRV2605 cannot support > 250kHz PWM signals
  }else if(Freq < 10){
    Freq = 20;      //DRV2605 cannot support < 10Hz PWM signals
  }
  Freq *= 128;
  ledcSetup(0, Freq, 8);  //Clear PWM Signal
  ledcAttachPin(Motor_PWM_Pins, 0);
  ledcWrite(0, 1);
}

void setup() 
{
  Serial.begin(115200);
  Serial.printf("Motor Count: %d\n", MotorCount);
  Wire.begin();
  for (int i = 0; i < MotorCount; ++i)
  {
    Serial.printf("ID %2d = %d\n", i, SCL_Pins[i]);
    SwitchMotor(i);
    /*Setup DRV2605*/
    drv.begin();
    drv.useLRA();
    drv.setMode(DRV2605_MODE_PWMANALOG);
    
    //LRA_OPEN_LOOP
    drv.writeRegister8(DRV2605_REG_CONTROL3, drv.readRegister8(DRV2605_REG_CONTROL3) | 0x01);
    /*Setup DRV2605*/
    SetMotorVoltage(0);
    SetFreq(Default_LRA_Frequency); 
  }
}
void SwitchMotor(int ID)
{
  if (ID < 0 || ID >= MotorCount)
    return;
  Wire.begin(SDA_Pin, SCL_Pins[ID]);
}

void loop() 
{
  while (Serial.available())
  {  
    // Input format: "n_action LRA_ID Freq Pow ..."
    int n_action = Serial.parseInt();
    int ID = 0;
    int Frequency = 100;
    uint8_t Power = 0;
    for(int i = 0 ; i < n_action ; i++){
      ID = Serial.parseInt();
      // Serial.printf("Selected Motor: %d\n", ID);
      Frequency = Serial.parseInt();
      // Serial.printf("Selected Freq: %d\n", Frequency);
      Power = Serial.parseInt();
      // Serial.printf("Selected Power: %d\n\n", Power);
      SwitchMotor(ID);
      SetFreq(Frequency);
      SetMotorVoltage(Power);      
    }
    // while (Serial.available())
      Serial.read();  //Read redundant data in b uffer
    
    
  }
}
