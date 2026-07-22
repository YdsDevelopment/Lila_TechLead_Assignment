using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{
    public class CanvasGameOverPanel : MonoBehaviour
    {
        public Text _resultText;

        public virtual void OnPlayerWon(string playerName)
        {
            if (_resultText != null)
            {
                _resultText.text = string.Format("{0} won", playerName);
            }
            this.gameObject.SetActive(true);
        }

        public virtual void OnGameDraw()
        {
            if (_resultText != null)
            {
                _resultText.text = "Match Drawn";
            }

            this.gameObject.SetActive(true);
        }
    }
}
