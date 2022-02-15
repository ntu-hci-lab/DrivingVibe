using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scenes.FadeController;
namespace Cinemachine
{
    public class DollyCartSpeedController : MonoBehaviour
    {
        public float velocity = 0;
        private float brakeTime = 0;
        private float brakeSpeed = 0;
        private float accelerateTime = 0;
        private float accelerateSpeed = 0;
        private float speedLimitationTarget = 0;

        public GameObject hint;
        public bool driving = false;
        public float starttimer = 0;
        public float stoptimer = 0;
        private float stopV = 0;
        public float stoptime = 0;
        public float acctimer = 0;
        public float timer = 0;
        public int timeCount = 0;
        public float fadeSpeed = 1;
        private bool fadeInStatus = false;
        private bool fadeOutStatus = false;
        public AnimationCurve startClip = AnimationCurve.Linear(0, 0, 1, 0);
        public AnimationCurve stopClip = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve accClip = AnimationCurve.Linear(0, 0, 1, 0);
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void FixedUpdate()
        {
            GetComponent<CinemachineDollyCart>().m_Speed = velocity / 3600 *1000;
            if (brakeTime != 0)
            {
                velocity -= brakeSpeed;
                brakeTime -= 0.02f;
                if (velocity < speedLimitationTarget)
                {
                    velocity = speedLimitationTarget;
                    brakeTime = 0;
                }
                if (brakeTime < 0) brakeTime = 0;
            }/*
            if (accelerateTime != 0)
            {
                velocity += accelerateSpeed;
                accelerateTime -= 0.02f;
                if (velocity > speedLimitationTarget)
                {
                    velocity = speedLimitationTarget;
                    accelerateTime = 0;
                }
                if (accelerateTime < 0) accelerateTime = 0;
            }*/

            if (driving && velocity!=0)
            {
                timer += Time.deltaTime;
            }

            //timer
            if (timer > stoptime)
            {
                stop();
                driving = false;
                timer = 0;
            }
            if(starttimer > 0)
            {
                velocity = startClip.Evaluate(10- starttimer)*100;
                starttimer -= 0.02f;
                if (starttimer < 0) starttimer = 0;
            }
            if (stoptimer > 0 && !fadeOutStatus && !fadeInStatus)
            {
                velocity = stopClip.Evaluate(stoptimer/3) * stopV;
                stoptimer -= 0.02f;
                if (stoptimer < 0)
                {
                    fadeOutStatus = GameObject.Find("FadePanel").GetComponent<FadeController>().FadeOut(fadeSpeed);
                    stoptimer = 0;
                    velocity = 0;
                    //hint.GetComponent<HintPlay>().palyHint();
                }
            }else if(fadeOutStatus)
            {
                fadeOutStatus = GameObject.Find("FadePanel").GetComponent<FadeController>().FadeOut(fadeSpeed);
            }
            if(acctimer > 0)
            {
                velocity = accelerateSpeed + (speedLimitationTarget - accelerateSpeed) * accClip.Evaluate((accelerateTime-acctimer) / accelerateTime);
                acctimer -= 0.02f;
                if (acctimer < 0) acctimer = 0;
            }
            if (Input.GetKeyDown("s") && !fadeInStatus && !fadeOutStatus)
            {
                //fade in
                fadeInStatus = GameObject.Find("FadePanel").GetComponent<FadeController>().FadeIn(fadeSpeed);
                start();
                driving = true;
            }else if (fadeInStatus)
            {
                fadeInStatus = GameObject.Find("FadePanel").GetComponent<FadeController>().FadeIn(fadeSpeed);
            }
            if (Input.GetKeyDown("k"))
            {
                stop();
                driving = false;
            }
            
            /*
            if(GetComponent<DollyStartController>().hide && velocity == 0 && timeCount < 4)
            {
                timeCount++;
                start();
                driving = true;
            }*/
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "speedDown")
            {
                brakeTime = other.gameObject.GetComponent<Status>().brakeTime;
                speedLimitationTarget = other.gameObject.GetComponent<Status>().speedLimitation;
                brakeSpeed = (velocity - speedLimitationTarget) / brakeTime * 0.02f;
                starttimer = 0;
            }
            if (other.gameObject.tag == "speedUp")
            {
                accelerateTime = other.gameObject.GetComponent<Status>().accelerateTime;
                acctimer = accelerateTime;
                speedLimitationTarget = other.gameObject.GetComponent<Status>().speedLimitation;
                accelerateSpeed = velocity;
                starttimer = 0;
                //accelerateSpeed = (speedLimitationTarget - velocity) / accelerateTime * 0.02f;        
            }
            if (other.gameObject.tag == "rabbit")
            {
                other.gameObject.transform.GetChild(0).gameObject.SetActive(true);
            }
        }

        public void start()
        {
            starttimer = 10;
            /*
            accelerateTime = 10;
            speedLimitationTarget = 100;
            accelerateSpeed = (speedLimitationTarget - velocity) / accelerateTime * 0.02f;*/
        }

        public void stop()
        {
            stoptimer = 3;
            stopV = velocity;
            starttimer = 0;
            /*
            speedLimitationTarget = 0;
            brakeSpeed = (velocity - speedLimitationTarget) / brakeTime * 0.02f;*/
        }
    }
}
