using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{
    public class BoardRenderer : MonoBehaviour
    {
        [SerializeField] private Button[,] _cells;

        public void UpdateBoard(PlayerSymbol?[,] board)
        {
            if (_cells == null) return;

            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    var cell = _cells[r, c];
                    if (cell == null) continue;

                    var symbol = board[r, c];
                    var text = cell.GetComponentInChildren<Text>();

                    if (symbol == null)
                    {
                        if (text != null) text.text = "";
                        cell.interactable = true;
                        cell.onClick.RemoveAllListeners();
                        int row = r, col = c;
                        cell.onClick.AddListener(() => MakeMove(row, col));
                    }
                    else
                    {
                        if (text != null) text.text = symbol == PlayerSymbol.X ? "X" : "O";
                        cell.interactable = false;
                        cell.onClick.RemoveAllListeners();
                    }
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
            if (winningCells == null || _cells == null) return;
            foreach (var cell in winningCells)
            {
                var img = _cells[cell[0], cell[1]].GetComponent<Image>();
                if (img != null)
                    img.color = Color.green;
            }
        }

        public void SetInteractable(bool active)
        {
            if (_cells == null) return;
            foreach (var cell in _cells)
            {
                if (cell != null)
                    cell.interactable = active;
            }
        }

        public void Reset()
        {
            if (_cells == null) return;
            foreach (var cell in _cells)
            {
                if (cell == null) continue;
                var text = cell.GetComponentInChildren<Text>();
                if (text != null) text.text = "";
                var img = cell.GetComponent<Image>();
                if (img != null) img.color = Color.white;
                cell.interactable = false;
                cell.onClick.RemoveAllListeners();
            }
        }
    }
}
