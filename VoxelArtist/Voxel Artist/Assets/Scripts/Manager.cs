using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Manager : MonoBehaviour
{
    //Settings
    private int targetRefreshRate = 300;

    //Other
    public TextMeshProUGUI systemInfo;

    private void Start()
    {
        //Lock cursor
        Cursor.lockState = CursorLockMode.Locked;

        //VSync off
        QualitySettings.vSyncCount = 0;

        //Refresh rate
        Time.fixedDeltaTime = 1f / targetRefreshRate;
        Application.targetFrameRate = targetRefreshRate;
    }

    private void Update()
    {
        if (Time.frameCount % Application.targetFrameRate == 0) //update once per second
        {
            //FPS
            int fps = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);

            //Time
            int hour = System.DateTime.Now.Hour;
            string meridiem = "AM";
            if (hour > 12)
            {
                hour -= 12;
                meridiem = "PM";
            }

            //Concatenate and print
            systemInfo.text = "FPS: " + fps
                + " | Time: " + hour + ":" + System.DateTime.Now.Minute.ToString("d2") + " " + meridiem;
        }
    }
}