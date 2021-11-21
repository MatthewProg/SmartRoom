#include <Tlc5940.h>
#include "tlc_fades.h"
#include <Wire.h>

#define I2C_SLAVE 0x08
#define BUFFER_SIZE 64
#define FEEDBACK_SIZE 64

#define DEBUG_LOG 0

#define FADE_TIME 500

uint8_t lastReceived[BUFFER_SIZE];
uint8_t feedback[FEEDBACK_SIZE];
int currentFeedbackSize = 0;

int arduino_values[17]; // 1-13 + A0-A3

void requestEvent();
void receiveEvent(int howMany);
void interpreteInput(uint8_t input[], int inSize);

void setup() {
  #if DEBUG_LOG
  Serial.begin(9600);
  #endif

  Wire.begin(I2C_SLAVE);
  Wire.onRequest(requestEvent);
  Wire.onReceive(receiveEvent);

  Tlc.init(0);
}

void loop() {
  tlc_updateFades();
  delay(1);
}

void requestEvent() {
  #if DEBUG_LOG
  Serial.print("Feedback(");
  Serial.print(currentFeedbackSize);
  Serial.print("): ");
  for(int x=0;x<currentFeedbackSize;x++)
  {
    Serial.print((byte)feedback[x]);
    Serial.print(" ");
  }
  Serial.write("\n");
  #endif
  Wire.write(currentFeedbackSize);
  Wire.write(feedback, currentFeedbackSize);
}

void receiveEvent(int howMany)
{
  //clear arrays
  memset(feedback,0,FEEDBACK_SIZE);
  memset(lastReceived,0,howMany);
  
  int i = 0;
  while (Wire.available()) 
  {
      uint8_t tmp = Wire.read();

      lastReceived[i] = tmp;

      if(i<BUFFER_SIZE-1)i++;
  }
  #if DEBUG_LOG
  Serial.print("Received: ");
  for(int x=0;x<i;x++)
  {
    Serial.print((byte)lastReceived[x]);
    Serial.print(" ");
  }
  Serial.print("\n");
  #endif

  interpreteInput(lastReceived,i);
}

void setValueArduino(byte pin, byte value)
{
  arduino_values[pin] = value;
  value = map(value, 0, 255, 0, 1);

  #if DEBUG_LOG
  Serial.print("To Arduino -> Port: ");
  Serial.print(pin);
  Serial.print(" Value: ");
  Serial.println(value);
  #endif

  pinMode(pin, OUTPUT);
  digitalWrite(pin, value);
}

void setValueTlc(byte pin, int value, bool fade)
{
  #if DEBUG_LOG
  Serial.print("To Tlc -> Port: ");
  Serial.print(pin);
  Serial.print(" Value: ");
  Serial.print(value);
  Serial.print(" Fade: ");
  Serial.println(fade);
  #endif

  uint32_t now = millis();
  if (tlc_fadeBufferSize < TLC_FADE_BUFFER_LENGTH - 2) 
    if (!tlc_isFading(pin))
      tlc_addFade(pin, Tlc.get(pin), value, now, now + ((fade) ? FADE_TIME : 1));
    else
    {
      tlc_removeFades(pin);
      tlc_addFade(pin, Tlc.get(pin), value, now, now + ((fade) ? FADE_TIME : 1));
    }
  
}

void interpreteInput(uint8_t input[], int inSize)
{
  if((input[0] & 128) >> 7 == 0) //Set value
  {
    for(int i=1;i<inSize;i+=2)
    {
      byte pin = input[i];
      pin = pin << 3;
      pin = pin >> 3; 

      #if DEBUG_LOG
      Serial.print("Input -> Port: ");
      Serial.print(pin);
      Serial.print(" Value: ");
      Serial.println((byte)input[i+1]);
      #endif

      if((input[i] & 128) >> 7 == 0) //Pass To Arduino  
        setValueArduino(pin, input[i+1]);
      else
      {
        bool fade = (input[i] & 64) >> 6;
        setValueTlc(pin, map(input[i+1], 0, 255, 0, 4095), fade);
      }
      
    }
  }
  else //Get value
  {
    currentFeedbackSize = 0;

    for(int i=1;i<inSize;i++)
    {
      if(i>=inSize) break;
      currentFeedbackSize+=2;

      byte pin = input[i];
      pin = pin << 3;
      pin = pin >> 3; 

      #if DEBUG_LOG
      Serial.print("Feedback -> Port: ");
      Serial.print(pin);
      #endif

      if((input[i] & 128) >> 7 == 0) //Get Arduino 
      {
        feedback[(i*2)-2] = pin;
        feedback[(i*2)-1] = 255; //CHANGE THIS TO GETTING OUTPUT VALUE!
        #if DEBUG_LOG
        Serial.print(" Value: ");
        Serial.print("255");
        Serial.write("\n");
        #endif
      }
      else //Get Tlc
      {
        feedback[(i*2)-2] = pin;
        feedback[(i*2)-1] = map(Tlc.get(pin), 0, 4095, 0, 255);
        #if DEBUG_LOG
        Serial.print(" Value: ");
        Serial.print((byte)feedback[(i*2)-1]);
        Serial.write("\n");
        #endif
      }
    }
  }
  
}