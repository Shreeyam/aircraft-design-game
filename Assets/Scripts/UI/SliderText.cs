using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class SliderText : MonoBehaviour
    {
        public string formatString = "{0}";
        Text sliderText;
        public float multiplier = 1.0f;

        private void Start()
        {
            sliderText = GetComponent<Text>();
        }

        public void textUpdate(float value)
        {
            sliderText.text = String.Format(formatString, value * multiplier);
        }

    }
}
