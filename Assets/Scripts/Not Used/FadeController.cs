using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FadeController
{
    public class FadeController : MonoBehaviour
    {
        public float fadeSpeed;//透明度變化速率
        public Image rawImage;
        public RectTransform rectTransform;
        public bool isFading;
        public bool isFadingIn;
        public bool isFadingOut;

        void Start()
        {
            rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);//使背景滿屏
            rawImage.color = Color.clear;
            fadeSpeed = 1;
            isFading = false;
            isFadingIn = false;
            isFadingOut = false;
        }

        void Update()
        {
            if (isFading)
            {
                DoFade();
            }
        }
        
        public bool FadeIn(float fadeSpeed)
        {
            if(isFading && !isFadingIn)   //finished
            {
                isFading = false;
                return false; 
            }
            else     //fading
            {
                if (!isFading) //begin
                {
                    this.fadeSpeed = fadeSpeed;
                    isFading = true;
                    isFadingIn = true;
                }
                return true;
            }
        }

        public bool FadeOut(float fadeSpeed)
        {
            if (isFading && !isFadingOut)   //finished
            {
                isFading = false;
                return false;
            }
            else     //fading
            {
                if (!isFading) //begin
                {
                    this.fadeSpeed = fadeSpeed;
                    isFading = true;
                    isFadingOut = true;
                }
                return true;
            }
        }

        private void DoFade()
        {
            if (isFadingIn == true)
            {
                rawImage.color = Color.Lerp(rawImage.color, Color.clear, Time.deltaTime * fadeSpeed);
                if (rawImage.color.a < 0.001f)
                {
                    isFadingIn = false;
                    rawImage.color = Color.clear;
                }
            }
            else if (isFadingOut == true)
            {
                rawImage.color = Color.Lerp(rawImage.color, Color.black, Time.deltaTime * fadeSpeed);//漸暗
                if (rawImage.color.a > 0.999f)
                {
                    isFadingOut = false;
                    rawImage.color = Color.black;
                }
            }
        }
    }
}
