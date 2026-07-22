using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace TicTacToe.Utils
{
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static UnityMainThreadDispatcher _instance;
        private readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();

        public static UnityMainThreadDispatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[MainThreadDispatcher]");
                    _instance = go.AddComponent<UnityMainThreadDispatcher>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public void Enqueue(Action action)
        {
            _queue.Enqueue(action);
        }

        private void Update()
        {
            while (_queue.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }
    }
}
