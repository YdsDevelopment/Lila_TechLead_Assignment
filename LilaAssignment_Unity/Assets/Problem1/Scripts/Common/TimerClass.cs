using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TicTacToe.Utils
{
    public class TimerClass : MonoBehaviour
    {
        protected float m_timerLeft;
        protected bool m_isTimerOn = false;

        protected float m_maxTurnTime;

        public UnityEvent m_timerStarted;
        public UnityEvent<float> m_timerStopped;
        public UnityEvent m_timerCompleted;

        Coroutine m_timerCoroutine = null;

        public float _remainingTime
        {
            get
            {
                return m_timerLeft;
            }
        }

        public bool _isTimerRunning
        {
            get
            {
                return m_isTimerOn;
            }
        }

        protected virtual void Awake()
        {
            m_isTimerOn = false;
            m_timerLeft = 0;
        }

        protected virtual void UpdateTimer(float currentTime)
        {
            currentTime += 1;

            //m_timeBar.fillAmount = Mathf.Clamp(currentTime / m_maxTurnTime, 0, 1.0f);
        }

        public virtual void InitialiseTimer(int timeInSec)
        {
            if (m_isTimerOn)
                return;

            timeInSec = timeInSec <= 0 ? 0 : timeInSec;

            m_maxTurnTime = (float)timeInSec;
            m_timerLeft = m_maxTurnTime;
            UpdateTimer(m_timerLeft);
        }

        public virtual void StartTheTimer()
        {
            if (m_isTimerOn)
                return;
            m_isTimerOn = true;
            m_timerLeft = m_maxTurnTime;
            UpdateTimer(m_timerLeft);
            m_timerCoroutine = StartCoroutine(RunTheTimer());
            m_timerStarted?.Invoke();
        }

        public virtual void StartTheTimer(int remainingTime)
        {
            if (m_isTimerOn)
                return;
            m_isTimerOn = true;
            m_timerLeft = m_maxTurnTime;
            m_timerLeft = remainingTime;
            UpdateTimer(m_timerLeft);
            m_timerCoroutine = StartCoroutine(RunTheTimer());
            m_timerStarted?.Invoke();
        }

        public virtual void StopTheTimer()
        {
            if (!m_isTimerOn)
                return;
            m_isTimerOn = false;
            if (m_timerCoroutine != null)
                StopCoroutine(m_timerCoroutine);
            m_timerCoroutine = null;
            m_timerStopped?.Invoke(m_timerLeft);
            m_timerLeft = 0;
            UpdateTimer(m_timerLeft);
        }

        protected virtual void TimerCompleted()
        {
            //Trigger Times Up
            m_timerCompleted?.Invoke();
        }

        IEnumerator RunTheTimer()
        {
            var wait = new WaitForEndOfFrame();
            while (m_isTimerOn)
            {
                if (m_timerLeft > 0)
                {
                    m_timerLeft -= Time.deltaTime;
                    UpdateTimer(m_timerLeft);
                }
                else
                {
                    m_timerLeft = 0;
                    m_isTimerOn = false;
                    TimerCompleted();
                }
                yield return wait;
            }
        }

        public virtual void HideTimer()
        {

        }
    }
}
