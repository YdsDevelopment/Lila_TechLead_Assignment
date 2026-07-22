using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{
    public class BoardRenderer : MonoBehaviour
    {
        [SerializeField]
        private List<GridItem> _cells = new List<GridItem>();

        public void UpdateBoard(PlayerSymbol?[,] board)
        {
            if (_cells == null || _cells.Count == 0) return;

            for (int i = 0; i < _cells.Count; i++)
            {
                var gridItem = _cells[i];
                if (gridItem == null) continue;

                var cell = gridItem.Button;
                if (cell == null) continue;

                var symbol = board[gridItem.Row, gridItem.Column];
                var text = cell.GetComponentInChildren<Text>();

                if (symbol == null)
                {
                    if (text != null) text.text = "";
                    cell.interactable = true;
                    cell.onClick.RemoveAllListeners();
                    cell.onClick.AddListener(() => MakeMove(gridItem.Row, gridItem.Column));
                }
                else
                {
                    if (text != null) text.text = symbol == PlayerSymbol.X ? "X" : "O";
                    cell.interactable = false;
                    cell.onClick.RemoveAllListeners();
                }
            }
        }

        private void MakeMove(int row, int col)
        {
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.Client.MakeMove(row, col);
                SetInteractable(false);
            }
        }

        public void HighlightWinningCells(List<List<int>> winningCells)
        {
            if (winningCells == null || _cells == null || _cells.Count == 0) return;
            foreach (var cell in winningCells)
            {
                foreach (var gridItem in _cells)
                {
                    if (gridItem.Row == cell[0] && gridItem.Column == cell[1])
                    {
                        var img = gridItem.Button.GetComponent<Image>();
                        if (img != null)
                            img.color = Color.green;
                        break;
                    }
                }
            }
        }

        public void SetInteractable(bool active)
        {
            if (_cells == null || _cells.Count == 0) return;
            foreach (var gridItem in _cells)
            {
                if (gridItem?.Button != null)
                    gridItem.Button.interactable = active;
            }
        }

        public void Reset()
        {
            if (_cells == null || _cells.Count == 0) return;
            foreach (var gridItem in _cells)
            {
                if (gridItem?.Button == null) continue;
                var text = gridItem.Button.GetComponentInChildren<Text>();
                if (text != null) text.text = "";
                var img = gridItem.Button.GetComponent<Image>();
                if (img != null) img.color = Color.white;
                gridItem.Button.interactable = false;
                gridItem.Button.onClick.RemoveAllListeners();
            }
        }

        public void Start()
        {
            if(_cells == null || _cells.Count == 0)
            {
                GetAllTheGridItems();
            }
        }

        private void GetAllTheGridItems()
        {
            var gridItems = gameObject.GetComponentsInChildren<GridItem>();
            _cells.AddRange(gridItems);
        }
    }
}
