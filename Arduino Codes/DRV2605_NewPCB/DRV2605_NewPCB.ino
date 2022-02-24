#include <Wire.h>
#include "Adafruit_DRV2605.h"

const int Motor_PWM_Pins = 16;
const uint32_t Default_LRA_Frequency = 100;
Adafruit_DRV2605 drv;
const int SDA_Pins[] = {0, 5, 9, 10, 17, 23};
const int SCL_Pins[] = {18, 19, 21, 22};
int SDA_Counts = sizeof(SDA_Pins) / sizeof(int);
int SCL_Counts = sizeof(SCL_Pins) / sizeof(int);
int MotorCount = SDA_Counts * SCL_Counts;

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
  int SDA_ID = ID / 4;
  int SCL_ID = ID % 4;
  if (SDA_ID >= SDA_Counts || SCL_ID >= SCL_Counts)
    return;
  Wire.begin(SDA_Pins[SDA_ID], SCL_Pins[SCL_ID]);
}

void loop() 
{
  while (Serial.available())
  {
    /*
    // Input format: "LRA_ID Freq Pow"
    int ID = Serial.parseInt();
    Serial.printf("Selected Motor: %d\n", ID);
    int Frequency = Serial.parseInt();
    Serial.printf("Selected Freq: %d\n", Frequency);
    uint8_t Power = Serial.parseInt();
    Serial.printf("Selected Power: %d\n\n", Power);
    SwitchMotor(ID);
    SetFreq(Frequency);
    SetMotorVoltage(Power);
    */


    /*
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
    */
    
    
    // Input format: "LRA_ID(1byte) Freq(1byte) Pow(1byte)"
    int ID = 0;
    int Frequency = 100;
    uint8_t Power = 0;
    char buf[4];
    Serial.readBytes(buf, 3);
    ID = (int)buf[0];
    Frequency = (int)buf[1] * 2;
    Power = (uint8_t)buf[2];
    SwitchMotor(ID);
    SetFreq(Frequency);
    SetMotorVoltage(Power);
  }
}
