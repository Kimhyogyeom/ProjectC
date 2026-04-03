using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 백그라운드 스레드에서 메인 스레드로 작업을 전달하는 유틸
/// </summary>
public class UnityMainThreadDispatcher : MonoBehaviour
{
    static UnityMainThreadDispatcher _instance;
    static readonly Queue<Action> _queue = new Queue<Action>();

    void Awake()
    {
        if (_instance != null) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        lock (_queue)
        {
            while (_queue.Count > 0)
                _queue.Dequeue()?.Invoke();
        }
    }

    public static void Enqueue(Action action)
    {
        lock (_queue)
        {
            _queue.Enqueue(action);
        }
    }
}
