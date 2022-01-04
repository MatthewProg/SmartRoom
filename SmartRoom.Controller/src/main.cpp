#include <Tlc5940.h>
#include "tlc_fades.h"
#include <Wire.h>

#define I2C_SLAVE 0x08
#define BUFFER_SIZE 64
#define FEEDBACK_SIZE 64

#define DEBUG_LOG 0

#define FADE_TIME 500

enum Receiver
{
  ARD,
  TLC,
  ID
};

uint8_t lastReceived[BUFFER_SIZE];
uint8_t feedback[FEEDBACK_SIZE];
uint8_t *overflowBuffer;

uint16_t overflowBufferSize = 0;
uint16_t currentFeedbackSize = 0;
uint16_t overflowSendIndex = 0;

byte *(*ids[32])(uint16_t *);

uint8_t arduino_values[17]; // 1-13 + A0-A3

void requestEvent();
void receiveEvent(int howMany);
void interpreteInput(uint8_t *input, int inSize);

/*  IDs  */
byte *id0(uint16_t *size)
{
  static char output[] = "0Hello World!0";
  *size = strlen(output);
  output[0] = (byte)0b11000000; //Feedback pkg
  output[(*size)-1] = (byte)0x03; //End of Text 
  return (byte *)(output);
}
/* - - - */

void setup()
{
#if DEBUG_LOG
  Serial.begin(9600);
#endif

  ids[0] = id0;

  Wire.begin(I2C_SLAVE);
  Wire.onRequest(requestEvent);
  Wire.onReceive(receiveEvent);

  Tlc.init(0);
}

void loop()
{
  tlc_updateFades();
  delay(1);
}

void requestEvent()
{
#if DEBUG_LOG
  Serial.print("Feedback(");
  Serial.print(currentFeedbackSize);
  Serial.print("): ");
  for (uint16_t x = 0; x < currentFeedbackSize; x++)
  {
    Serial.print((byte)feedback[x]);
    Serial.print(" ");
  }
  Serial.print("\nRemaining(");
  Serial.print(overflowBufferSize - overflowSendIndex);
  Serial.print(")\n");
#endif
  if (currentFeedbackSize > 0)
    Wire.write(feedback, currentFeedbackSize);

  if (overflowBufferSize - overflowSendIndex > 0)
  {
    if (overflowBufferSize - overflowSendIndex >= BUFFER_SIZE)
    {
      memcpy(feedback, overflowBuffer + overflowSendIndex, BUFFER_SIZE);
      overflowSendIndex += BUFFER_SIZE;
      currentFeedbackSize = BUFFER_SIZE;
    }
    else
    {
      memcpy(feedback, overflowBuffer + overflowSendIndex, overflowBufferSize - overflowSendIndex);
      free(overflowBuffer);
      overflowBufferSize = 0;
      currentFeedbackSize = overflowBufferSize - overflowSendIndex;
    }
  }
  else
  {
    if (currentFeedbackSize < BUFFER_SIZE)
      Wire.write((byte)0b01100000); //EOT

    currentFeedbackSize = 0;
  }
}

uint16_t binMap(uint16_t value, char shift)
{
  if (shift > 0)
    return (value << shift) + (value / (1 << shift));
  return (value >> (-shift));
}

void receiveEvent(int howMany)
{
  //clear arrays
  memset(feedback, 0, FEEDBACK_SIZE);
  memset(lastReceived, 0, howMany);

  int i = 0;
  while (Wire.available())
  {
    uint8_t tmp = Wire.read();

    lastReceived[i] = tmp;

    if (i < BUFFER_SIZE - 1)
      i++;
  }
#if DEBUG_LOG
  Serial.print("Received: ");
  for (int x = 0; x < i; x++)
  {
    Serial.print((byte)lastReceived[x]);
    Serial.print(" ");
  }
  Serial.print("\n");
#endif

  interpreteInput(lastReceived, i);
}

void setValueArduino(byte pin, byte value)
{
  if (pin > 17)
    return;

  pinMode(pin, OUTPUT);
  if (pin == 3 || pin == 5 || pin == 6 || (pin >= 9 && pin <= 11) || pin >= 14) //Analog write
  {
    arduino_values[pin - 1] = value;
    analogWrite(pin, value);
  }
  else //Digital write
  {
    value = binMap(value, -7);
    arduino_values[pin - 1] = (value == 1) ? 255 : 0;
    digitalWrite(pin, value);
  }

#if DEBUG_LOG
  Serial.print("To Arduino -> Port: ");
  Serial.print(pin);
  Serial.print(" Value: ");
  Serial.println(value);
#endif
}

void setValueTlc(byte pin, byte value, bool fade)
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
  {
    if (!tlc_isFading(pin))
      tlc_addFade(pin, Tlc.get(pin), binMap(value, 4), now, now + ((fade) ? FADE_TIME : 1));
    else
    {
      tlc_removeFades(pin);
      tlc_addFade(pin, Tlc.get(pin), binMap(value, 4), now, now + ((fade) ? FADE_TIME : 1));
    }
  }
}

byte *encodePinValue(byte pin, byte value, Receiver rec)
{
  static byte output[2] = {0, 0};

  output[0] = pin;
  output[1] = value;

  if (rec == Receiver::TLC)
    output[0] += 0b00100000;
  else if (rec == Receiver::ID)
    output[0] += 0b01000000;

  return output;
}

void writeToFeedback(byte *bytes, uint16_t size)
{
#if DEBUG_LOG
  Serial.print("WriteToFeedback: ");
  for (uint16_t x = 0; x < size; x++)
  {
    Serial.print(bytes[x]);
    Serial.print(" ");
  }
  Serial.write("\n");
#endif

  uint16_t i = 0;
  for (; i < size; i++)
  {
    if (currentFeedbackSize < FEEDBACK_SIZE)
    {
      feedback[currentFeedbackSize] = bytes[i];
      currentFeedbackSize++;
    }
    else
      break;
  }
  if (i != size)
  {
    realloc(overflowBuffer, overflowBufferSize + (size - i));
    for (; i < size; i++)
    {
      overflowBuffer[overflowBufferSize] = bytes[i];
      overflowBufferSize++;
    }
  }
}

void interpreteInput(uint8_t *input, int inSize)
{
  byte *(*idsToExec[32])(uint16_t *);
  uint8_t idsToExecIndex = 0;

  for (int i = 0; i < inSize;)
  {
    if (input[i] == 0b01100000) //EOT
      break;
    else if (input[i] >> 7 == 1) //Set PKG
    {
      bool tlc = ((input[i] & 0b01000000) >> 6);
      bool fade = ((input[i] & 0b00100000) >> 5);
      byte pin = (input[i] & 0b00011111);
      i++;

      if (tlc == false)
        setValueArduino(pin, input[i]);
      else
        setValueTlc(pin, input[i], fade);
    }
    else //Get PKG
    {
      byte pin = (input[i] & 0b00011111);
      if ((input[i] & 0b01100000) == 0b00000000) //Arduino pin
        writeToFeedback(encodePinValue(pin, arduino_values[pin], Receiver::ARD), 2);
      else if ((input[i] & 0b01100000) == 0b00100000) //Tlc pin
        writeToFeedback(encodePinValue(pin, binMap(Tlc.get(pin), -4), Receiver::TLC), 2);
      else if ((input[i] & 0b01100000) == 0b01000000) //ID
      {
        idsToExec[idsToExecIndex] = ids[pin];
        idsToExecIndex++;
      }
    }
    i++;
  }

  //Get values from ids
  for (uint8_t i = 0; i < idsToExecIndex; i++)
  {
    uint16_t size = 0;
    byte *value = (idsToExec[i])(&size);
    writeToFeedback(value, size);
  }
}