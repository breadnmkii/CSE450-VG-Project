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
    public static BossHPBar instance;

    public MusicScoreManager MSM;

    Slider slider;
    bool init;

    void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        init = false;
        slider = gameObject.GetComponent<Slider>();
    }

    private void Update()
    {
        if (!init)
        {
            slider.maxValue = MSM.GetTotalNotes() ;
            slider.value = MSM.GetTotalNotes() - MSM.GetRemainingNotes();
            slider.minValue = MSM.GetTotalNotes() - MSM.GetRemainingNotes();
            init = true;
            Debug.Log("Total: "+ slider.maxValue.ToString());
            Debug.Log("Remain: "+ MSM.GetRemainingNotes().ToString());
        }
        slider.value = MSM.GetTotalNotes() - MSM.GetRemainingNotes();
    }
}
