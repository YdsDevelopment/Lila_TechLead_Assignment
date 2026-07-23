using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{
    [System.Serializable]
    public class TicTacToeTile : MonoBehaviour
    {
        [SerializeField] private int _row;
        [SerializeField] private int _column;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private Color _initialTileColor;
        [SerializeField] public Color _winhighlightTileColor;
        [SerializeField] public Color _labelOColor;
        [SerializeField] public Color _labelXColor;
        [SerializeField] public GameObject _wrongMoveHighlight;
        private Button _button;
        public int Row => _row;
        public int Column => _column;
        public Button Button
        {
            get
            {
                if (_button == null)
                    _button = GetComponent<Button>();
                return _button;
            }
        }

        public TextMeshProUGUI Label
        {
            get
            {
                if (_label == null)
                    _label = GetComponentInChildren<TextMeshProUGUI>();
                return _label;
            }
        }

        public int GetRow()
        {
            return _row;
        }

        public int getRow()
        {
            return _row;
        }

        public int GetColumn()
        {
            return _column;
        }

        public int getColumn()
        {
            return _column;
        }

        public void Initialise()
        {
            Button.image.color = _initialTileColor;
        }

        public void HighlightWrongMove()
        {
            StartCoroutine(showHighlight());
        }

        private IEnumerator showHighlight()
        {
            var wait = new WaitForSeconds(0.2f);
            if (_wrongMoveHighlight)
            {
                _wrongMoveHighlight.SetActive(true);
                yield return wait;
                _wrongMoveHighlight.SetActive(false);
                yield return wait;
                _wrongMoveHighlight.SetActive(true);
                yield return wait;
                _wrongMoveHighlight.SetActive(false);
            }
        }
    }
}
