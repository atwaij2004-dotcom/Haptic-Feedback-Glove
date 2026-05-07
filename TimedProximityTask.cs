using UnityEngine;
using System.IO.Ports;

public class TimedProximityTask : MonoBehaviour
{
    public Transform wallButton;

    public float moveSpeed = 3f;
    public float pressDistance = 0.6f;
    public float buttonPressAmount = 0.18f;
    public float buttonPressSpeed = 8f;

    public string portName = "COM5";
    public int baudRate = 115200;

    public float tiltDeadZone = 8f;
    public float maxTilt = 35f;

    SerialPort serialPort;

    Vector3 startPosition;
    Vector3 buttonStartPosition;
    Vector3 buttonPressedPosition;

    bool trialRunning = false;
    bool trialComplete = false;
    bool buttonPressed = false;
    bool hapticMode = false;
    bool debugMode = false;

    float startTime;
    float completionTime;

    int failedAttempts = 0;
    int vibrationValue = 0;
    int lastSentValue = -1;

    float pitchValue = 0f;
    float rollValue = 0f;

    bool flexPressed = false;
    bool lastFlexPressed = false;
    bool flexPressedThisFrame = false;

    void Start()
    {
        startPosition = transform.position;
        buttonStartPosition = wallButton.position;
        buttonPressedPosition = buttonStartPosition + new Vector3(0f, 0f, buttonPressAmount);

        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.ReadTimeout = 10;
            serialPort.WriteTimeout = 50;
            serialPort.Open();
        }
        catch
        {
            Debug.LogWarning("Could not open serial port");
        }
    }

    void Update()
    {
        flexPressedThisFrame = false;

        ReadSerial();

        if (Input.GetKeyDown(KeyCode.H))
        {
            hapticMode = !hapticMode;

            if (!hapticMode)
            {
                SendMotor(0);
            }
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            debugMode = !debugMode;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetTrial();
        }

        UpdateVibration();
        SendMotor(vibrationValue);

        if (trialRunning && !trialComplete)
        {
            MoveHand();
            CheckPress();
        }

        MoveButton();
    }

    void ReadSerial()
    {
        if (serialPort == null || !serialPort.IsOpen)
        {
            return;
        }

        try
        {
            while (serialPort.BytesToRead > 0)
            {
                string line = serialPort.ReadLine().Trim();

                if (line.StartsWith("T:"))
                {
                    string data = line.Substring(2);
                    string[] parts = data.Split(',');

                    if (parts.Length >= 3)
                    {
                        float.TryParse(parts[0], out pitchValue);
                        float.TryParse(parts[1], out rollValue);

                        if (parts[2].StartsWith("F:"))
                        {
                            int flexState = int.Parse(parts[2].Substring(2));

                            flexPressed = flexState == 1;
                            flexPressedThisFrame = flexPressed && !lastFlexPressed;
                            lastFlexPressed = flexPressed;
                        }
                    }
                }
            }
        }
        catch
        {
        }
    }

    void SendMotor(int value)
    {
        if (serialPort == null || !serialPort.IsOpen)
        {
            return;
        }

        if (value == lastSentValue)
        {
            return;
        }

        try
        {
            serialPort.WriteLine("M:" + value);
            lastSentValue = value;
        }
        catch
        {
        }
    }

    void MoveHand()
    {
        float moveInput = 0f;

        if (pitchValue > tiltDeadZone)
        {
            moveInput = Mathf.InverseLerp(tiltDeadZone, maxTilt, pitchValue);
        }
        else if (pitchValue < -tiltDeadZone)
        {
            moveInput = -Mathf.InverseLerp(tiltDeadZone, maxTilt, -pitchValue);
        }

        if (Input.GetKey(KeyCode.W))
        {
            moveInput = 1f;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveInput = -1f;
        }

        transform.Translate(Vector3.forward * moveInput * moveSpeed * Time.deltaTime);
    }

    void CheckPress()
    {
        float distance = Vector3.Distance(transform.position, wallButton.position);
        bool nearButton = distance <= pressDistance;

        bool pressAttempt = flexPressedThisFrame || Input.GetKeyDown(KeyCode.Space);

        if (pressAttempt)
        {
            if (nearButton)
            {
                buttonPressed = true;
                trialComplete = true;
                trialRunning = false;
                completionTime = Time.time - startTime;
                SendMotor(70);
            }
            else
            {
                failedAttempts++;
            }
        }
    }

    void UpdateVibration()
    {
        if (!hapticMode || trialComplete)
        {
            vibrationValue = 0;
            return;
        }

        float distance = Vector3.Distance(transform.position, wallButton.position);

        if (distance > 10f)
        {
            vibrationValue = 0;
        }
        else if (distance > 6f)
        {
            vibrationValue = 40;
        }
        else if (distance > pressDistance)
        {
            vibrationValue = 50;
        }
        else
        {
            vibrationValue = 70;
        }
    }

    void MoveButton()
    {
        Vector3 target = buttonPressed ? buttonPressedPosition : buttonStartPosition;
        wallButton.position = Vector3.Lerp(wallButton.position, target, buttonPressSpeed * Time.deltaTime);
    }

    void ResetTrial()
    {
        transform.position = startPosition;
        wallButton.position = buttonStartPosition;

        trialRunning = true;
        trialComplete = false;
        buttonPressed = false;

        failedAttempts = 0;
        startTime = Time.time;
        completionTime = 0f;

        lastSentValue = -1;
        SendMotor(0);
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 28;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(20, 20, 1000, 40), "R = Start/Reset | H = Haptic On/Off", style);
        GUI.Label(new Rect(20, 60, 1000, 40), hapticMode ? "Mode: Haptic Feedback" : "Mode: Visual Only", style);

        if (trialRunning)
        {
            GUI.Label(new Rect(20, 100, 1000, 40), "Timer: " + (Time.time - startTime).ToString("F2") + " s", style);
            GUI.Label(new Rect(20, 140, 1000, 40), "Failed Attempts: " + failedAttempts, style);
        }
        else if (trialComplete)
        {
            GUI.Label(new Rect(20, 100, 1000, 40), "Completed Time: " + completionTime.ToString("F2") + " s", style);
            GUI.Label(new Rect(20, 140, 1000, 40), "Final Failed Attempts: " + failedAttempts, style);
        }
        else
        {
            GUI.Label(new Rect(20, 100, 1000, 40), "Press R to begin", style);
        }

        if (debugMode)
        {
            GUI.Label(new Rect(20, 220, 1000, 40), "Pitch: " + pitchValue.ToString("F2"), style);
            GUI.Label(new Rect(20, 260, 1000, 40), "Flex Pressed: " + flexPressed, style);
            GUI.Label(new Rect(20, 300, 1000, 40), "Vibration: " + vibrationValue, style);
        }
    }

    void OnApplicationQuit()
    {
        SendMotor(0);

        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
        }
    }
}
