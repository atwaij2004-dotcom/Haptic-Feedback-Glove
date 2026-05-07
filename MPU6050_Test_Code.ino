#include <Arduino.h>
#include <Wire.h>

const int MPU_ADDR = 0x68;
int16_t accX, accY, accZ;
int16_t gyroX, gyroY, gyroZ;
void setup() {
  Serial.begin(115200);
  Wire.begin(21, 22);
  Wire.beginTransmission(MPU_ADDR);
  Wire.write(0x6B);
  Wire.write(0);
  Wire.endTransmission(true);
}
void loop() {
  Wire.beginTransmission(MPU_ADDR);
  Wire.write(0x3B);
  Wire.endTransmission(false);

  Wire.requestFrom(MPU_ADDR, 14, true);
  accX = Wire.read() << 8 | Wire.read();
  accY = Wire.read() << 8 | Wire.read();
  accZ = Wire.read() << 8 | Wire.read();
  Wire.read();
  Wire.read();
  gyroX = Wire.read() << 8 | Wire.read();
  gyroY = Wire.read() << 8 | Wire.read();
  gyroZ = Wire.read() << 8 | Wire.read();
  Serial.print("Accel X: ");
  Serial.print(accX);
  Serial.print(" | Accel Y: ");
  Serial.print(accY);
  Serial.print(" | Accel Z: ");
  Serial.print(accZ);

  Serial.print(" | Gyro X: ");
  Serial.print(gyroX);
  Serial.print(" | Gyro Y: ");
  Serial.print(gyroY);
  Serial.print(" | Gyro Z: ");
  Serial.println(gyroZ);
  delay(200);
}
