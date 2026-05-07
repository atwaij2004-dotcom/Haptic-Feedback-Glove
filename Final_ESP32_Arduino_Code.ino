#include <Wire.h>

const int motorPins[] = {4, 16, 17, 18, 19};
const int motorCount = 5;

const int pwmFreq = 5000;
const int pwmResolution = 8;
const int maxMotorIntensity = 70;

const int flexPins[] = {32, 33, 34, 35};
const int flexThresholds[] = {2287, 2284, 2232, 2205};

const int MPU_ADDR = 0x68;

unsigned long lastSendTime = 0;
const unsigned long sendInterval = 30;

void setup()
{
  Serial.begin(115200);

  Wire.begin(21, 22);

  Wire.beginTransmission(MPU_ADDR);
  Wire.write(0x6B);
  Wire.write(0);
  Wire.endTransmission(true);

  for (int i = 0; i < motorCount; i++)
  {
    ledcAttach(motorPins[i], pwmFreq, pwmResolution);
    ledcWrite(motorPins[i], 0);
  }
}

void loop()
{
  readMotorCommand();

  if (millis() - lastSendTime >= sendInterval)
  {
    lastSendTime = millis();

    float pitch, roll;
    readTilt(pitch, roll);

    int flexValues[4];
    int flexPressed[4];

    for (int i = 0; i < 4; i++)
    {
      flexValues[i] = analogRead(flexPins[i]);
      flexPressed[i] = flexValues[i] < flexThresholds[i] ? 1 : 0;
    }

    Serial.print("T:");
    Serial.print(pitch);
    Serial.print(",");
    Serial.print(roll);

    Serial.print(",F1:");
    Serial.print(flexPressed[0]);

    Serial.print(",F2:");
    Serial.print(flexPressed[1]);

    Serial.print(",F3:");
    Serial.print(flexPressed[2]);

    Serial.print(",F4:");
    Serial.println(flexPressed[3]);
  }
}

void readMotorCommand()
{
  if (Serial.available() > 0)
  {
    String input = Serial.readStringUntil('\n');
    input.trim();

    if (input.startsWith("M:"))
    {
      int value = input.substring(2).toInt();
      value = constrain(value, 0, maxMotorIntensity);

      for (int i = 0; i < motorCount; i++)
      {
        ledcWrite(motorPins[i], value);
      }
    }
  }
}

void readTilt(float &pitch, float &roll)
{
  Wire.beginTransmission(MPU_ADDR);
  Wire.write(0x3B);
  Wire.endTransmission(false);
  Wire.requestFrom(MPU_ADDR, 6, true);

  int16_t accX = Wire.read() << 8 | Wire.read();
  int16_t accY = Wire.read() << 8 | Wire.read();
  int16_t accZ = Wire.read() << 8 | Wire.read();

  float ax = accX / 16384.0;
  float ay = accY / 16384.0;
  float az = accZ / 16384.0;

  pitch = -atan2(ay, sqrt(ax * ax + az * az)) * 180.0 / PI;
  roll = atan2(ax, sqrt(ay * ay + az * az)) * 180.0 / PI;
}
