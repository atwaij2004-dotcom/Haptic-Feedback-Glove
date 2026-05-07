# Haptic Glove Project

This repository contains the code used for my final year haptic glove project.

The project uses an ESP32, flex sensors, an MPU6050 motion sensor and ERM vibration motors to create a haptic glove interface for Unity-based tasks.

## Repository Structure

### Test Codes for Input Sensors

This section contains the individual test codes used to check the input sensors before combining them into the final system.

- `MPU6050_Test_Code.ino` tests accelerometer and gyroscope readings from the MPU6050.
- `Flex_Sensor_Test_Code.ino` tests the analogue readings from the four flex sensors.

### Final ESP32 Code

This section contains the final Arduino code uploaded to the ESP32.

- `Final_ESP32_Arduino_Code.ino` reads the MPU6050 and four flex sensors, sends the processed data to Unity over serial, and receives motor vibration commands from Unity.

### Game Scripts

This section contains the Unity C# scripts used for the haptic glove demonstrations.

- `TimedProximityTask.cs` controls the proximity-based haptic feedback task.
- `ReactionTimeTask.cs` controls the reaction-time task comparing visual and haptic cues.
