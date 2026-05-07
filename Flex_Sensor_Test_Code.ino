const int flexPins[] = {32, 33, 34, 35};
const int flexCount = 4;

void setup()
{
  Serial.begin(115200);
}

void loop()
{
  for (int i = 0; i < flexCount; i++)
  {
    int value = analogRead(flexPins[i]);

    Serial.print("Flex ");
    Serial.print(i + 1);
    Serial.print(": ");
    Serial.print(value);

    if (i < flexCount - 1)
    {
      Serial.print(" | ");
    }
  }

  Serial.println();
  delay(200);
}
