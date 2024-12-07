using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayTimer : MonoBehaviour
{
    public TMP_Text textTimer;

    private float timer = 600.0f;                                   // 10 minutes
    private bool isTimer = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(isTimer & timer >= 1.0f){
            timer -= Time.deltaTime;
            DisplayTime();
        }
    }

    void DisplayTime(){
        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer - minutes * 60);

        textTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void StartTimer(){
        isTimer = true;
    }

    public void StopTimer(){
        isTimer = false;
        timer += 5.0f;                                  // 5sec increment
        DisplayTime();
    }

}
