

using UnityEngine;
using System.IO.Ports;

public class ReactionTimeTask : MonoBehaviour
{
    public Renderer buttonRenderer;

    public Material redMaterial;
    public Material greenMaterial;

    public string portName = "COM5";
    public int baudRate = 115200;

    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;

    public int hapticCueIntensity = 70;

    SerialPort serialPort;

    bool hapticMode = false;
    bool trialRunning = false;
    bool cueActive = false;
    bool trialComplete = false;

    bool flexPressed = false;
    bool lastFlexPressed = false;
    bool flexPressedThisFrame = false;

    float trialStartTime;
    float cueTime;
    float reactionTime;
    float randomWaitTime;

    int falseStarts = 0;
    int trialNumber = 0;

    void Start()
    {
        SetButtonRed();

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
            SendMotor(0);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartTrial();
        }

        if (trialRunning && !cueActive && !trialComplete)
        {
            if (Time.time - trialStartTime >= randomWaitTime)
            {
                ActivateCue();
            }
        }

        if (flexPressedThisFrame || Input.GetKeyDown(KeyCode.Space))
        {
            HandleResponse();
        }
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

                if (line.StartsWith("F:"))
                {
                    int flexState = int.Parse(line.Substring(2));

                    flexPressed = flexState == 1;
                    flexPressedThisFrame = flexPressed && !lastFlexPressed;
                    lastFlexPressed = flexPressed;
                }
            }
        }
        catch
        {
        }
    }

    void StartTrial()
    {
        trialNumber++;

        trialRunning = true;
        cueActive = false;
        trialComplete = false;

        reactionTime = 0f;
        randomWaitTime = Random.Range(minWaitTime, maxWaitTime);
        trialStartTime = Time.time;

        SetButtonRed();
        SendMotor(0);
    }

    void ActivateCue()
    {
        cueActive = true;
        cueTime = Time.time;

        if (hapticMode)
        {
            SendMotor(hapticCueIntensity);
        }
        else
        {
            SetButtonGreen();
        }
    }

    void HandleResponse()
    {
        if (!trialRunning || trialComplete)
        {
            return;
        }

        if (!cueActive)
        {
            falseStarts++;
            return;
        }

        reactionTime = Time.time - cueTime;

        trialComplete = true;
        trialRunning = false;
        cueActive = false;

        SendMotor(0);
        SetButtonRed();

        Debug.Log("Trial: " + trialNumber +
                  " | Mode: " + (hapticMode ? "Haptic" : "Visual") +
                  " | Reaction Time: " + reactionTime.ToString("F3") +
                  " s | False Starts: " + falseStarts);
    }

    void SetButtonRed()
    {
        if (buttonRenderer != null && redMaterial != null)
        {
            buttonRenderer.material = redMaterial;
        }
    }

    void SetButtonGreen()
    {
        if (buttonRenderer != null && greenMaterial != null)
        {
            buttonRenderer.material = greenMaterial;
        }
    }

    void SendMotor(int value)
    {
        if (serialPort == null || !serialPort.IsOpen)
        {
            return;
        }

        value = Mathf.Clamp(value, 0, 70);

        try
        {
            serialPort.WriteLine("M:" + value);
        }
        catch
        {
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 28;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(20, 20, 1000, 40), "R = Start Trial | H = Change Mode", style);
        GUI.Label(new Rect(20, 60, 1000, 40), hapticMode ? "Mode: Haptic Cue" : "Mode: Visual Cue", style);
        GUI.Label(new Rect(20, 100, 1000, 40), "Trial: " + trialNumber, style);
        GUI.Label(new Rect(20, 140, 1000, 40), "False Starts: " + falseStarts, style);

        if (trialRunning && !cueActive)
        {
            GUI.Label(new Rect(20, 180, 1000, 40), "Status: Wait...", style);
        }
        else if (trialRunning && cueActive)
        {
            GUI.Label(new Rect(20, 180, 1000, 40), "Status: React Now", style);
        }
        else if (trialComplete)
        {
            GUI.Label(new Rect(20, 180, 1000, 40), "Reaction Time: " + reactionTime.ToString("F3") + " s", style);
            GUI.Label(new Rect(20, 220, 1000, 40), "Press R for next trial", style);
        }
        else
        {
            GUI.Label(new Rect(20, 180, 1000, 40), "Press R to begin", style);
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
