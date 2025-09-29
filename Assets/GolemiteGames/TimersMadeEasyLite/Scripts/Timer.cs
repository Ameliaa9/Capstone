using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    public UnityEvent onTimerEnd;

    [Range(0, 23)] public int hours;
    [Range(0, 59)] public int minutes;
    [Range(0, 59)] public int seconds;

    public enum CountMethod { CountDown, CountUp };
    public enum SeperatorType { Colon, Bullet, Slash };
    public enum OutputType { None, StandardText, TMPro, HorizontalSlider, Dial };

    [Tooltip("If checked, runs the timer on play")]
    public bool startAtRuntime = true;

    [Tooltip("Select what to display")]
    public bool hoursDisplay = false;
    public bool minutesDisplay = true;
    public bool secondsDisplay = true;

    [Space]
    [Tooltip("Select to count up or down")]
    public CountMethod countMethod;

    [Tooltip("Select the output type")]
    public OutputType outputType;

    public Text standardText;
    public TextMeshProUGUI textMeshProText;
    public Slider standardSlider;
    public Image dialSlider;

    private bool timerRunning = false;
    private bool timerPaused = false;

    public double timeRemaining;

    // ---------------- Awake ---------------- //
    private void Awake()
    {
        if (!standardText && GetComponent<Text>())
            standardText = GetComponent<Text>();

        if (!textMeshProText && GetComponent<TextMeshProUGUI>())
            textMeshProText = GetComponent<TextMeshProUGUI>();

        if (!standardSlider && GetComponent<Slider>())
            standardSlider = GetComponent<Slider>();

        if (!dialSlider && GetComponent<Image>())
            dialSlider = GetComponent<Image>();

        if (standardSlider)
        {
            standardSlider.maxValue = ReturnTotalSeconds();
            standardSlider.value = countMethod == CountMethod.CountDown
                ? standardSlider.maxValue
                : standardSlider.minValue;
        }

        if (dialSlider)
        {
            dialSlider.fillAmount = countMethod == CountMethod.CountDown ? 1f : 0f;
        }
    }

    // ---------------- Start ---------------- //
    void Start()
    {
        if (startAtRuntime)
        {
            StartTimer();
        }
        else
        {
            double displayTime = countMethod == CountMethod.CountDown ? ReturnTotalSeconds() : 0;
            if (standardText) standardText.text = DisplayFormattedTime(displayTime);
            if (textMeshProText) textMeshProText.text = DisplayFormattedTime(displayTime);
        }
    }

    // ---------------- Update ---------------- //
    void Update()
    {
        if (!timerRunning) return;

        if (countMethod == CountMethod.CountDown)
        {
            CountDown();
            if (standardSlider) StandardSliderDown();
            if (dialSlider) DialSliderDown();
        }
        else
        {
            CountUp();
            if (standardSlider) StandardSliderUp();
            if (dialSlider) DialSliderUp();
        }
    }

    // ---------------- Count Logic ---------------- //
    private void CountDown()
    {
        if (timeRemaining > 0.02)
        {
            timeRemaining -= Time.deltaTime;
            DisplayInTextObject();
        }
        else
        {
            timeRemaining = 0;
            timerRunning = false;
            onTimerEnd.Invoke();
            DisplayInTextObject();
        }
    }

    private void CountUp()
    {
        if (timeRemaining < ReturnTotalSeconds())
        {
            timeRemaining += Time.deltaTime;
            DisplayInTextObject();
        }
        else
        {
            onTimerEnd.Invoke();
            timeRemaining = ReturnTotalSeconds();
            DisplayInTextObject();
            timerRunning = false;
        }
    }

    // ---------------- Slider UI ---------------- //
    private void StandardSliderDown()
    {
        if (standardSlider.value > standardSlider.minValue)
            standardSlider.value -= Time.deltaTime;
    }

    private void StandardSliderUp()
    {
        if (standardSlider.value < standardSlider.maxValue)
            standardSlider.value += Time.deltaTime;
    }

    private void DialSliderDown()
    {
        float normalized = Mathf.InverseLerp(ReturnTotalSeconds(), 0, (float)timeRemaining);
        dialSlider.fillAmount = Mathf.Lerp(1, 0, normalized);
    }

    private void DialSliderUp()
    {
        float normalized = Mathf.InverseLerp(0, ReturnTotalSeconds(), (float)timeRemaining);
        dialSlider.fillAmount = Mathf.Lerp(0, 1, normalized);
    }

    // ---------------- Timer UI Display ---------------- //
    private void DisplayInTextObject()
    {
        string display = DisplayFormattedTime(timeRemaining);
        if (standardText) standardText.text = display;
        if (textMeshProText) textMeshProText.text = display;
    }

    public string DisplayFormattedTime(double remainingSeconds)
    {
        string output = "";

        RemainingSecondsToHHMMSSMMM(remainingSeconds, out float h, out float m, out float s);

        if (hoursDisplay)
        {
            output += $"{h:00}";
            if (minutesDisplay || secondsDisplay) output += ":";
        }

        if (minutesDisplay)
        {
            output += $"{m:00}";
            if (secondsDisplay) output += ":";
        }

        if (secondsDisplay)
        {
            output += $"{s:00}";
        }

        return output;
    }

    private static void RemainingSecondsToHHMMSSMMM(double totalSeconds, out float hours, out float minutes, out float seconds)
    {
        hours = Mathf.FloorToInt((float)totalSeconds / 3600);
        minutes = Mathf.FloorToInt(((float)totalSeconds / 60) - (hours * 60));
        seconds = Mathf.FloorToInt((float)totalSeconds - (hours * 3600) - (minutes * 60));
    }

    // ---------------- Control Methods ---------------- //
    public void StartTimer()
    {
        if (!timerRunning && !timerPaused)
        {
            ResetTimer();
            timerRunning = true;

            if (countMethod == CountMethod.CountDown)
            {
                ConvertToTotalSeconds(hours, minutes, seconds);
            }
            else
            {
                StartTimerCustom(0);
            }
        }
    }

    public void StopTimer()
    {
        timerRunning = false;
        ResetTimer();
    }

    private void StartTimerCustom(double timeToSet)
    {
        if (!timerRunning && !timerPaused)
        {
            timeRemaining = timeToSet;
            timerRunning = true;
        }
    }

    public void ResetTimer()
    {
        timerPaused = false;

        if (countMethod == CountMethod.CountDown)
        {
            timeRemaining = ReturnTotalSeconds();
            DisplayInTextObject();

            if (standardSlider)
            {
                standardSlider.maxValue = ReturnTotalSeconds();
                standardSlider.value = standardSlider.maxValue;
            }

            if (dialSlider)
                dialSlider.fillAmount = 1f;
        }
        else
        {
            timeRemaining = 0;
            DisplayInTextObject();

            if (standardSlider)
            {
                standardSlider.maxValue = ReturnTotalSeconds();
                standardSlider.value = standardSlider.minValue;
            }

            if (dialSlider)
                dialSlider.fillAmount = 0f;
        }
    }

    // ---------------- Time Helpers ---------------- //
    public float ReturnTotalSeconds()
    {
        return (hours * 3600) + (minutes * 60) + seconds;
    }

    public double ConvertToTotalSeconds(float h, float m, float s)
    {
        timeRemaining = (h * 3600) + (m * 60) + s;
        DisplayFormattedTime(timeRemaining);
        return timeRemaining;
    }

    // ---------------- Unity Editor ---------------- //
    private void OnValidate()
    {
        timeRemaining = ConvertToTotalSeconds(hours, minutes, seconds);
    }

    // ---------------- Expose Remaining Time ---------------- //
    public double GetRemainingSeconds()
    {
        return timeRemaining;
    }
}
