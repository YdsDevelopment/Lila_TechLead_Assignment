using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{
    public class BoardRenderer : MonoBehaviour
    {
        [SerializeField]
        private List<TicTacToeTile> _cells = new List<TicTacToeTile>();

        private void Awake()
        {
            Debug.Log("BoardRenderer Awake Called");
            if (_cells == null || _cells.Count == 0)
                GetAllTheGridItems();
            Reset();
            initialiseNetworkEvents();
        }

        private void initialiseNetworkEvents()
        {
            Debug.Log("GameManager regidtered :initialiseNetworkEvents");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRoomStateRestored += OnRoomStateRestoredHandler;
                GameManager.Instance.OnGameStarted += OnGameStartedHandler;
                GameManager.Instance.OnMoveResult += OnMoveResultHandler;
                Debug.Log("GameManager regidtered :initialiseNetworkEvents");
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null){
                GameManager.Instance.OnRoomStateRestored -= OnRoomStateRestoredHandler;
                GameManager.Instance.OnGameStarted -= OnGameStartedHandler;
                GameManager.Instance.OnMoveResult -= OnMoveResultHandler;
            }
        }

        private void OnGameStartedHandler(GameStartedPayload payload)
        {
            Debug.Log("[OnGameStartedHandler] Game started");
            if (payload == null) return;
            Reset();
            // var emptyBoard = new PlayerSymbol?[3, 3];
            var board = NetworkManager.Instance.Client.NullableBoardFromPayload(payload.board);
            UpdateBoard(board);
        }

        private void OnMoveResultHandler(MoveResultPayload payload)
        {
            if (payload.board == null) return;
            var board = NetworkManager.Instance.Client.NullableBoardFromPayload(payload.board);
            if(payload.success == false && payload.error != "")
            {
                var move = payload.move;
                if (move != null && move.playerId == NetworkManager.Instance.GetOrCreatePlayerId())
                {
                    var gridItem = getTileWithRowAndColumn(move.row, move.col);
                    if (gridItem)
                    {
                        gridItem.HighlightWrongMove();
                    }
                }
            }
            UpdateBoard(board);
            if (payload.winningCells != null)
                HighlightWinningCells(payload.winningCells);
            SetInteractable(!(payload.winner != null || payload.isDraw || payload.timeoutWin));
        }

        private void OnRoomStateRestoredHandler(RoomStatePayload payload)
        {
            if (payload.board == null) return;
            var board = NetworkManager.Instance.Client.NullableBoardFromPayload(payload.board);
            UpdateBoard(board);
            if (payload.winningCells != null)
                HighlightWinningCells(payload.winningCells);
            bool isActive = payload.status == "ACTIVE" || payload.status == "PLAYING";
            SetInteractable(isActive);
        }

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
                var text = gridItem.Label;

                if (symbol == null)
                {
                    if (text != null) text.text = "";
                    cell.interactable = true;
                    cell.onClick.RemoveAllListeners();
                    cell.onClick.AddListener(() => MakeMove(gridItem.Row, gridItem.Column));
                }
                else
                {
                    if (text != null)
                    {
                        text.text = symbol == PlayerSymbol.X ? "X" : "O";
                        text.color = symbol == PlayerSymbol.X
                            ? gridItem._labelXColor
                            : gridItem._labelOColor;
                    }
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
                            img.color = gridItem._winhighlightTileColor;
                        break;
                    }
                }
            }
        }

        public void SetInteractable(bool active)
        {
            if (_cells == null || _cells.Count == 0) return;
            Debug.Log("Room State : " + active);
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
                var text = gridItem.Label;
                if (text != null) text.text = "";
                gridItem.Button.interactable = false;
                gridItem.Initialise();
                gridItem.Button.onClick.RemoveAllListeners();
            }
        }

        public TicTacToeTile getTileWithRowAndColumn(int row, int col)
        {
            if (_cells == null) return null;
            foreach (var tile in _cells)
            {
                if (tile != null && tile.Row == row && tile.Column == col)
                    return tile;
            }
            return null;
        }

        private void GetAllTheGridItems()
        {
            var gridItems = gameObject.GetComponentsInChildren<TicTacToeTile>();
            _cells.AddRange(gridItems);
        }
    }
}
