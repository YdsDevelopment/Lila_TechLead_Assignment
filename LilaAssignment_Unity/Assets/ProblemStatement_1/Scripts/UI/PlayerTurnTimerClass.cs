using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TicTacToe.Utils;

namespace TicTacToe.UI
{
    public class PlayerTurnTimerClass : TimerClass
    {
        [SerializeField] Image m_timeBar;

        protected override void UpdateTimer(float currentTime)
        {
            //currentTime += 1;

            m_timeBar.fillAmount = Mathf.Clamp(currentTime / m_maxTurnTime, 0, 1.0f);
        }

        public override void InitialiseTimer(int playerTimeInSec)
        {
            base.InitialiseTimer(playerTimeInSec);
        }

        public override void StartTheTimer()
        {
            m_timeBar.fillAmount = 0;
            base.StartTheTimer();
        }

        public override void StopTheTimer()
        {
            m_timeBar.fillAmount = 0;
            base.StopTheTimer();
        }

        protected override void TimerCompleted()
        {
            base.TimerCompleted();
            m_timeBar.fillAmount = 0;
        }
    }
}