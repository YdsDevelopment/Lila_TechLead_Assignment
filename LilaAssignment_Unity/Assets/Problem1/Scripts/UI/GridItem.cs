using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{
    [System.Serializable]
    public class GridItem : MonoBehaviour
    {
        [SerializeField] private int _row;
        [SerializeField] private int _column;

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
    }
}
