using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;

public class LoomRuntime : MonoBehaviour
{
	public static int maxThreads = 8;
	static int numThreads;
    List<Action> _currentActions = new List<Action>();
    private List<Action> _actions = new List<Action>();
    private List<DelayedQueueItem> _delayed = new  List<DelayedQueueItem>();
    List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();
	private static LoomRuntime _current;
	private int _count;
    static bool initialized;

	public static LoomRuntime Current
	{
		get
		{
			Initialize();
			return _current;
		}
	}
	
	void Awake ()
	{
		_current = this;
		initialized = true;
	}
	
	static void Initialize ()
	{
		if (!initialized)
		{
			if(!Application.isPlaying)
				return;
            
			initialized = true;
            GameObject g = new GameObject("Loom");
			_current = g.AddComponent<LoomRuntime>();
		}	
	}
	
	public struct DelayedQueueItem
	{
		public float time;
		public Action action;
	}
	
	public static void QueueOnMainThread (Action action)
	{
		QueueOnMainThread( action, 0f);
	}

	public static void QueueOnMainThread (Action action, float time)
	{
		if(time != 0)
		{
			lock(Current._delayed)
			{
				Current._delayed.Add(new DelayedQueueItem { time = Time.time + time, action = action});
			}
		}
		else
		{
			lock (Current._actions)
			{
				Current._actions.Add(action);
			}
		}
	}
	
	public static Thread RunAsync (Action a)
	{
		Initialize();

		while(numThreads >= maxThreads)
		{
			Thread.Sleep(1);
		}

		Interlocked.Increment(ref numThreads);
		ThreadPool.QueueUserWorkItem(RunAction, a);
		return null;
	}
	
	private static void RunAction (object action)
	{
		try
		{
			((Action)action)();
		}
		catch
		{
		}
		finally
		{
			Interlocked.Decrement(ref numThreads);
		}	
	}
	
	void OnDisable ()
	{
		if (_current == this)
			_current = null;
	}
	
	void Update ()
	{
		lock (_actions)
		{
			_currentActions.Clear();
			_currentActions.AddRange(_actions);
			_actions.Clear();
		}

		foreach(var a in _currentActions)
		{
			a();
		}

		lock(_delayed)
		{
			_currentDelayed.Clear();
			_currentDelayed.AddRange(_delayed.Where(d=>d.time <= Time.time));
			
            foreach(var item in _currentDelayed)
				_delayed.Remove(item);
		}

		foreach(var delayed in _currentDelayed)
		{
			delayed.action();
		}
	}
}

