#include <Wire.h>
#include<WiFi.h>

const char ssid[]="ML Lighthouse"; //修改為你家的WiFi網路名稱
const char pwd[]="ntuhcilab"; //修改為你家的WiFi密碼
WiFiServer server(80);

void SetFreq(uint32_t Freq = 300)
{
  //DRV2605 Input PWM Frequency: 10-250kHz
  assert (Freq <= 1950); //DRV2605 cannot support > 250kHz PWM signals
  assert (Freq != 0);   //DRV2605 cannot support < 10Hz PWM signals
  Freq *= 128;
  ledcSetup(0, Freq, 8);  //Clear PWM Signal
  ledcAttachPin(14, 0);
  ledcWrite(0, 1);
}

void setup() {
  Serial.begin(115200);

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
  
  // Serial.printf("1 for digitalWrite HIGH, 2 for digitalWrite LOW \n");
  Serial.printf("1 for SetFreq(500)\n");
  pinMode(14, OUTPUT);

}
void loop() 
{
  WiFiClient client = server.available();
  if (!client) {
    return;
  }
  while (client.connected()) {
    while (client.available()) {
      int input = client.parseInt();
      if(input == 1){
        client.printf("get a 1, SetFreq(500) \n");
        SetFreq(800);
      }else if(input == 2){
        client.printf("get a 2 \n");
        Serial.printf("get a 2 \n");
        SetFreq(300);
      }
    }
    delay(10);
  }
  client.stop();
  Serial.println("Client disconnected");
}
