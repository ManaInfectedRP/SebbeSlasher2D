using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro; // using text mesh for the clock display

using UnityEngine.Rendering; // used to access the volume component

namespace Sebbe
{
    public class DayNightScript : MonoBehaviour
    {
        public TextMeshProUGUI timeDisplay; // Display Time
        public TextMeshProUGUI dayDisplay; // Display Day
        public Volume ppv; // this is the post processing volume

        private float weightTime = 0f;

        public float seconds;
        public int minutes;
        public int hours;
        private int dayCount = 1;
        public float tick = 1f; // how fast time passes
        private bool isDay = true;

        public GameObject stars; // reference to the stars GameObject
        public GameObject[] lights;

        void Start()
        {
            ppv = GetComponent<Volume>();
        }

        void Update()
        {
        }

        void FixedUpdate()
        {
            seconds += tick * Time.fixedDeltaTime;

            if (seconds >= 60f)
            {
                seconds = 0f;
                minutes += 1;
            }

            if (minutes >= 60)
            {
                minutes = 0;
                hours += 1;
            }

            if (hours >= 24)
            {
                hours = 0;
                dayCount += 1;
            }

            ControlPPVWeight();

            // Update time display
            timeDisplay.text = string.Format("{0:00}:{1:00}", hours, minutes);
            dayDisplay.text = "Day " + dayCount.ToString();

            // Day-Night Cycle Logic
            if (hours >= 6 && hours < 18)
            {
                // Daytime
                isDay = true;
                weightTime = Mathf.Max(0f, weightTime - 0.01f);
                ppv.weight = weightTime;

                stars.SetActive(false);
                foreach (GameObject light in lights)
                {
                    light.SetActive(false);
                }
            }
            else
            {
                // Nighttime
                isDay = false;
                weightTime = Mathf.Min(1f, weightTime + 0.01f);
                ppv.weight = weightTime;

                stars.SetActive(true);
                foreach (GameObject light in lights)
                {
                    light.SetActive(true);
                }
            }
        }

        public void ControlPPVWeight()
        {
            if(hours >= 21 && hours < 22) // Dusk 21/9pm to 22/10pm
            {
                weightTime = Mathf.Min(1f, weightTime + 0.01f);
                ppv.weight = weightTime;
            }
            else if(hours >= 5 && hours < 6) // Dawn 5am to 6am
            {
                weightTime = Mathf.Max(0f, weightTime - 0.01f);
                ppv.weight = weightTime;
            }
        }
    }
}