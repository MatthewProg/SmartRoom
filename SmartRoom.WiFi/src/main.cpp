#include <ESP8266WiFi.h>
#include <WiFiClient.h>
#include <Wire.h>
#include <PolledTimeout.h>

//I2C config
#define SDA_PIN 0
#define SCL_PIN 2

//Wi-Fi config
#ifndef APSSID
#define APSSID "Smart Room"
#define APPSK "donteventry"
#endif

//Internal config
#define BUFFER_SIZE 64
#define FEEDBACK_SIZE 64
#define LOG_SERIAL 0

//I2C addreses
const int16_t I2C_MASTER = 0x42;
const int16_t I2C_SLAVE = 0x08;

//Wi-Fi & server config
const char *ssid = APSSID;
const char *password = APPSK;

const int port = 23;

WiFiServer server(port);
WiFiClient serverClient;

void setup()
{
#if LOG_SERIAL
  Serial.begin(9600);
  Serial.println();

  Serial.print("Establishing I2C connection... ");
  Wire.begin(SDA_PIN, SCL_PIN, I2C_MASTER);
  Serial.println("DONE");

  Serial.print("Configuring access point... ");
  WiFi.softAP(ssid, password);
  Serial.println("DONE");

  Serial.print("Starting server... ");
  server.begin();
  server.setNoDelay(true);
  Serial.println("DONE");

  IPAddress myIP = WiFi.softAPIP();
  Serial.print("Server at: ");
  Serial.print(myIP);
  Serial.printf(":%d\n", port);

  Serial.println("Waiting for clients...");
#else
  Wire.begin(SDA_PIN, SCL_PIN, I2C_MASTER);
  WiFi.softAP(ssid, password);
  server.begin();
  server.setNoDelay(true);
#endif
}

void loop()
{
#if LOG_SERIAL
  if (server.hasClient())
    Serial.println("Client connected!");
#endif

  if (!serverClient.connected())
  {
    serverClient = server.available();
    return;
  }

  if (serverClient.available())
  {
    //Get data from TCP
    uint8_t tcpRead[BUFFER_SIZE];
    int received = 0;
    bool isValue = false;
    while (serverClient.available())
    {
      uint8_t tmp = serverClient.read();

      if (isValue == false)
      {
        if ((tmp >> 7) == 1)
          isValue = true;
        else if (tmp == 0b01100000) //EOT
          break;
      }
      else
        isValue = false;

      tcpRead[received] = tmp;
      if (received < BUFFER_SIZE - 1)
        received++;
    }

#if LOG_SERIAL
    Serial.print("Client: ");
    for(int i = 0; i<received; i++)
    {
      Serial.print(tcpRead[i]);
      Serial.print(" ")
    }
    Serial.write("\n");
#endif

    Wire.beginTransmission(I2C_SLAVE);
    for (int iterator = 0; iterator < received; iterator++)
      Wire.write(tcpRead[iterator]);
    Wire.endTransmission();

#if LOG_SERIAL
    Serial.println("Send via I2C");
#endif

    delay(10);

    bool dataWaiting = true;
    bool isText = false;
    isValue = false;
    do
    {
      Wire.requestFrom(I2C_SLAVE, FEEDBACK_SIZE);

#if LOG_SERIAL
      Serial.println("Request send");
      Serial.print("Feedback RAW: ");
#endif

      uint8_t feedback[FEEDBACK_SIZE];
      int f = 0;
      while (Wire.available())
      {
        uint8_t tmp = Wire.read();
#if LOG_SERIAL
        Serial.print((byte)tmp);
        Serial.print(" ");
#endif

        if (isValue == false && isText == false)
        {
          if (tmp == 0b01100000) //EOT
          {
            dataWaiting = false;
            break;
          }
          else if ((tmp >> 7) == 0) //Value
            isValue = true;
          else if ((tmp >> 7) == 1) //Text
            isText = true;
        }
        else if (isValue == true)
          isValue = false;
        else if (isText == true && tmp == 0b00000011) //End of Text
          isText = false;

        feedback[f] = tmp;
        if (f < FEEDBACK_SIZE - 1)
          f++;
      }
      serverClient.write(feedback, f);
#if LOG_SERIAL
      Serial.print("\nFeedback send - ");
      Serial.print(f);
      Serial.println(" bytes");
#endif
    } while (dataWaiting);
    serverClient.write(0b01100000); //EOT
#if LOG_SERIAL
    Serial.println("End of Transmission");
#endif
  }
}