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
#define APPSK  "donteventry"
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

void setup() {
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
  Serial.printf(":%d\n",port);

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
      if(server.hasClient())
       Serial.println("Client connected!");
    #endif
  
        
      if (!serverClient.connected()) {
        serverClient = server.available();
        return;
      }

      if(serverClient.available())
      {
        //Get data from TCP
        uint8_t tcpRead[BUFFER_SIZE];
        int i = 0;
        int elementsInside = 0;
        while (serverClient.available())
        {
          uint8_t tmp = serverClient.read();
          if(i==0) elementsInside = (tmp & 63) + 1;
          tcpRead[i] = tmp;

          if(i<BUFFER_SIZE-1) i++;
        }

        #if LOG_SERIAL
        Serial.print("Client: ");
        Serial.write(tcpRead,i);
        Serial.write("\n");
        #endif

        //Decide what to do
        if((tcpRead[0] & 128) >> 7 == 0) //Set value
        {
          Wire.beginTransmission(I2C_SLAVE);
          for(int iterator = 0;iterator<elementsInside;iterator++)
            Wire.write(tcpRead[iterator]);
          Wire.endTransmission();
          #if LOG_SERIAL
          Serial.println("Send via I2C");
          #endif
        }
        else if((tcpRead[0] & 128) >> 7 == 1) //Get value
        {
          
          Wire.beginTransmission(I2C_SLAVE);
          for(int iterator = 0;iterator<elementsInside;iterator++)
            Wire.write(tcpRead[iterator]);
          Wire.endTransmission();

          #if LOG_SERIAL
          Serial.println("Send via I2C");
          #endif

          delay(10);
          Wire.requestFrom(I2C_SLAVE, FEEDBACK_SIZE);

          #if LOG_SERIAL
          Serial.println("Request send");
          Serial.print("Feedback RAW: ");
          #endif

          uint8_t feedback[FEEDBACK_SIZE];
          int f=0;
          int elementsInsideFeedback = 1;
          while(Wire.available())
          {
            uint8_t tmp = Wire.read();
              #if LOG_SERIAL
              Serial.print(f);
              Serial.print(":");
              Serial.print((byte)tmp);
              Serial.print(" ");
              #endif
              if(f!=elementsInsideFeedback+1)
              {
                if(f==0) elementsInsideFeedback = tmp;
                if(f!=0) feedback[f-1] = tmp;
                if(f<FEEDBACK_SIZE-1)f++; 
              }

          }
           #if LOG_SERIAL
          Serial.print("\nFeedback -> Size: ");
          Serial.print(elementsInsideFeedback);
          Serial.print(" Elements: ");
          for(int x=0;x<elementsInsideFeedback;x++) { Serial.print((byte)feedback[x]); Serial.print(" "); }
          Serial.print("\n");
          #endif
          //serverClient.write(elementsInsideFeedback); OLD
          serverClient.write(feedback,elementsInsideFeedback);
        }
        serverClient.write(0b01100000); //EOT
      }
}
