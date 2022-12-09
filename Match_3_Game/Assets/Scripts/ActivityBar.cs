using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivityBar : MonoBehaviour
{
    public float value;
    public LevelMenu dailyLimit;
    public Slider slider;

    public void Update()
    {
        slider.maxValue = dailyLimit.max;
        slider.value = dailyLimit.numOfClicks;
    }
}
