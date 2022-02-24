#include <Wire.h>
#include<WiFi.h>
#include "Adafruit_DRV2605.h"

const char ssid[]="ML Lighthouse"; //修改為WiFi網路名稱
const char pwd[]="ntuhcilab"; //修改為WiFi密碼
// NTUST 708: "VRLab_5G" "nccu115114"
//const char ssid[]="VRLab_5G"; //修改為WiFi網路名稱
//const char pwd[]="nccu115114"; //修改為WiFi密碼
// also NTUST 708: "IB708" "ilovetaiwan"
//const char ssid[]="IB708"; //修改為WiFi網路名稱
//const char pwd[]="ilovetaiwan"; //修改為WiFi密碼

WiFiServer server(80);

const int Motor_PWM_Pins = 14;
const uint32_t Default_PWM_Frequency = 100;
const int SDA_Pins[] = {16, 05, 32, 33, 17, 23};
const int SCL_Pins[] = {18, 19, 21, 22};
int SDA_Counts = sizeof(SDA_Pins) / sizeof(int);
int SCL_Counts = sizeof(SCL_Pins) / sizeof(int);
//int MotorCount = SDA_Counts * SCL_Counts;
int MotorCount = 16; // only the first 16 motors are used
Adafruit_DRV2605 drv;

void SetMotorVoltage(uint8_t Fraction = 0x89)
{
  drv.writeRegister8(0x16, Fraction);
  drv.writeRegister8(0x17, Fraction);
}

void SetFreq(uint32_t Freq = Default_PWM_Frequency)
{
  //DRV2605 Input PWM Frequency: 10-250kHz
  assert (Freq <= 1950); //DRV2605 cannot support > 250kHz PWM signals
  assert (Freq != 0);   //DRV2605 cannot support < 10Hz PWM signals
  Freq *= 128;
  ledcSetup(0, Freq, 8);  //Clear PWM Signal
  ledcAttachPin(Motor_PWM_Pins, 0);
  ledcWrite(0, 1);
}

void setup() 
{
  Serial.begin(115200);
  Serial.printf("Setup goes burrrrrrrrrrrrrrrrrrrrrrrrrrhhh.\n");
  Serial.printf("Motor Count: %d\n", MotorCount);
  Wire.begin();
  for (int i = 0; i < MotorCount; ++i)
  {
    int SDA_ID = i / 4;
    int SCL_ID = i % 4;
    Serial.printf("Initializing Motor ID %2d: SCL Group: %d(pin %2d) SDA Group: %d(pin %2d)\n", i, SCL_ID, SCL_Pins[SCL_ID], SDA_ID, SDA_Pins[SDA_ID]);
    SwitchMotor(i);
    /*Setup DRV2605*/
    drv.begin();
    drv.useLRA();
    drv.setMode(DRV2605_MODE_PWMANALOG);
    
    //LRA_OPEN_LOOP
    drv.writeRegister8(DRV2605_REG_CONTROL3, drv.readRegister8(DRV2605_REG_CONTROL3) | 0x01);
    /*Setup DRV2605*/
    SetMotorVoltage(0);
    SetFreq(Default_PWM_Frequency); 
  }
  WiFi.mode(WIFI_STA); //設置WiFi模式
  WiFi.begin(ssid,pwd);

  Serial.print("WiFi connecting");

  //當WiFi連線時會回傳WL_CONNECTED，因此跳出迴圈時代表已成功連線
  while(WiFi.status()!=WL_CONNECTED){
    Serial.print(".");
    delay(500);   
  }

  Serial.println("");
  Serial.print("IP address:");
  Serial.println(WiFi.localIP()); //讀取IP位址
  Serial.print("WiFi RSSI:");
  Serial.println(WiFi.RSSI()); //讀取WiFi強度
  
  server.begin();
  Serial.println(F("Server started"));
  
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
  WiFiClient client = server.available();
  if (!client) {
    return;
  }
  client.setNoDelay(true);
  while (client.connected()) {
    while (client.available()) {
      /*
      //one LRA per input, costum frequency
      int ID = 0;
      int Frequency = 100;
      uint8_t Power = 0;
      char buf[4];
      client.readBytes(buf, 3);
      ID = (int)buf[0];
      Frequency = (int)buf[1] * 2;
      Power = (uint8_t)buf[2];
      SwitchMotor(ID);
      SetFreq(Frequency);
      SetMotorVoltage(Power);
      */

      // Input format: "Intensity#0, Intensity#1,..., Intensity#15" in bytes
      char buf[17];
      client.readBytes(buf, 16);
      for(int i = 0 ; i < MotorCount ; i++){
        Power = (uint8_t)buf[i];
        SwitchMotor(i);
        SetFreq(170);
        SetMotorVoltage(Power);
      }
    }
  }
  client.stop();
  Serial.println("Client disconnected");
}
