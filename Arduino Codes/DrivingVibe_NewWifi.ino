#include <Wire.h>
#include <WiFi.h>
#include <esp_wifi.h>
#include "Adafruit_DRV2605.h"

// AP mode
//const char ssid[]="DrivingVibe-Board1"; //修改為WiFi網路名稱
const char ssid[]="DrivingVibe-Board2"; //修改為WiFi網路名稱
const char pwd[]="55555555"; //修改為WiFi密碼

WiFiServer server(80);

const int Motor_PWM_Pins = 14;
const uint32_t Default_PWM_Frequency = 100;
const int SDA_Pins[] = {16, 05, 32, 33, 17, 23};
const int SCL_Pins[] = {18, 19, 21, 22};
//const int table[] = {11, 10, 9, 8, 15, 14, 13, 12, 4, 5, 6, 7, 3, 2, 1, 0}; //white band
const int table[] = {3, 2, 1, 0, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15}; //black band
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
  
  //WRITE_PERI_REG(RTC_CNTL_BROWN_OUT_REG, 0); //disable brownout detector
  tcpip_adapter_init(); //Initialize TCP_IP Module
  esp_event_loop_init(NULL, NULL);  //Initialize Callback Function
  wifi_init_config_t InitConfig = WIFI_INIT_CONFIG_DEFAULT(); //Init InitConfig
  esp_wifi_init(&InitConfig);                           //Init Wi-Fi Module
  esp_wifi_set_storage(WIFI_STORAGE_RAM);               //Commit Changes to RAM
  esp_wifi_set_mode(WIFI_MODE_AP);                      //Set as AP Mode
  esp_wifi_set_ps(WIFI_PS_NONE);                        //Disable Modem Sleep
  esp_wifi_set_protocol(WIFI_IF_AP, WIFI_PROTOCOL_11N); //Set as 802.11n
  esp_wifi_set_bandwidth(ESP_IF_WIFI_AP, WIFI_BW_HT40); //Set as 40MHz
  esp_wifi_set_max_tx_power(80);                        //80 * 0.25dBm = 20dB(Max)
  
  wifi_config_t ap_config;                              //Set Config
  strcpy((char *)ap_config.ap.ssid, ssid);
  ap_config.ap.ssid_len = sizeof(ssid) - 1;          //Count SSID Length
  strcpy((char *)ap_config.ap.password, pwd);
  ap_config.ap.channel = 0;
  ap_config.ap.authmode = WIFI_AUTH_WPA_WPA2_PSK;
  ap_config.ap.ssid_hidden = 0;
  ap_config.ap.max_connection = 2;
  ap_config.ap.beacon_interval = 100;

  esp_wifi_set_config(WIFI_IF_AP, &ap_config);          //Set AP Config
  esp_wifi_start();                                     //Start Wi-Fi Module
  
  server.begin();
  server.setNoDelay(true);
  Serial.println(F("Server started"));
}
void SwitchMotor(int fake_ID)
{
  int ID = table[fake_ID];
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
      uint8_t Power = 0;
      client.readBytes(buf, 16);
      for(int i = 0 ; i < MotorCount ; i++){
        Power = (uint8_t)buf[i];
        SwitchMotor(i);
        SetFreq(170);
        SetMotorVoltage(Power);
      }
      delay(1);
    }
    delay(1);
  }
  client.stop();
  Serial.println("Client disconnected");
}
