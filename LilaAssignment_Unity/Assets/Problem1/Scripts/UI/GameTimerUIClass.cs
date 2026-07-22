using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TicTacToe.Utils;

namespace TicTacToe.UI
{
    public class GameTimerUIClass : TimerClass
    {
        [SerializeField] Text m_timerText;

        protected override void Awake()
        {
            base.Awake();
            if (m_timerText == null)
                m_timerText = GetComponent<Text>();
        }

        //// Update is called once per frame
        //void Update()
        //{
        //    if (m_isTimerOn)
        //    {
        //        if(m_timerLeft > 0)
        //        {
        //            m_timerLeft -= Time.deltaTime;
        //            UpdateTimer(m_timerLeft);
        //        }
        //        else
        //        {
        //            m_timerLeft = 0;
        //            m_isTimerOn = false;
        //            //Trigger Times Up
        //            m_timerCompleted?.Invoke();
        //        } 
        //    }
        //}

        protected override void UpdateTimer(float currentTime)
        {
            currentTime += 1;
            float min = Mathf.FloorToInt(currentTime / 60);
            float sec = Mathf.FloorToInt(currentTime % 60);

            m_timerText.text = string.Format("{0:00} : {1:00}", min, sec);
        }

        public override void InitialiseTimer(int gameTimeInSec)
        {
            m_timerText.gameObject.SetActive(true);
            base.InitialiseTimer(gameTimeInSec - 1);
        }

        public override void StartTheTimer()
        {
            base.StartTheTimer();
        }

        public override void StopTheTimer()
        {
            base.StopTheTimer();
        }

        protected override void TimerCompleted()
        {
            base.TimerCompleted();
        }

        public override void HideTimer()
        {
            m_timerText.gameObject.SetActive(false);
            base.HideTimer();
        }
    }
}
