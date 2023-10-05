using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.UI;
using UnityEngine;


// Basic manager for multilane scene.
// It controls the lane changing part and the correlated animation.
// Not involving actuall character behaviors.
// Todo: figure out whether need to do the rhythm check part in this class or not. Need to discuss detailed control.

public class BossHPBar : MonoBehaviour
{
    public BossBehavior Boss;

    Slider slider;

    bool init;

    private void Start()
    {
        init = false;
        slider = gameObject.GetComponent<Slider>();
    }

    private void Update()
    {
        if (!init)
        {
            slider.maxValue = Boss.GetMaxHP();
            slider.value = Boss.GetMaxHP();
            slider.minValue = 0;
        }
        slider.value = Boss.getHP();
    }
}
