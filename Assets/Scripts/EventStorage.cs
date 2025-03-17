using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using UI;
using Random = UnityEngine.Random;

public class EventStorage
{
    [JsonProperty] private List<GameEvent> _events;
    [JsonProperty] private List<GameEvent> _timedEvents;
    [JsonProperty] private List<GameEvent> _eventQueue;
    [JsonProperty] private List<GameEvent> _failEvents;
    [JsonProperty] private List<GameEvent> _winEvents;

    [JsonProperty] private int _currentTurn;
    [JsonProperty] private bool _isReady;
    
    [JsonProperty] public GameEvent CurrentEvent;

    private static List<GameEvent> LoadFile(string path)
    {
        TextAsset raw = ResourceLoader.GetResource<TextAsset>(path);
        return JsonConvert.DeserializeObject<List<GameEvent>>(raw.text);
    }
    
    public void Load()
    {
        _events = new();
        _timedEvents = new();
        _eventQueue = new();
        
        _events.AddRange(LoadFile("Events/Common.events"));

        _timedEvents = LoadFile("Events/Story.events");

        _failEvents = LoadFile("Events/Fail.events");
        _winEvents = LoadFile("Events/Win.events");
    }

    public void Load(List<GameEvent> events, List<GameEvent> timedEvents,
        List<GameEvent> failEvents, List<GameEvent> winEvents)
    {
        _eventQueue = new();
        
        _events = events;
        _timedEvents = timedEvents;
        _failEvents = failEvents;
        _winEvents = winEvents;
    }

    public void Save()
    {
        PlayerPrefs.SetString("EventStorage", JsonConvert.SerializeObject(this));
    }
    
    public void Init()
    {
        if (_isReady) return;
        
        foreach (GameEvent gameEvent in _events)
        {
            gameEvent.EnableDynamicChecking();
        }
        // No dynamic checks for win/fail events (they use another queue)
        // No dynamic checks for timed events (they use another queue)
        
        // Add to initial queue only available events
        foreach (GameEvent gameEvent in _events)
        {
            gameEvent.CheckLimits();
        }
        
        _eventQueue.Shuffle();
        
        _timedEvents.Sort((a, b) => a.TurnPosition.CompareTo(b.TurnPosition));

        _isReady = true;
    }

    public void EnqueueEvent(GameEvent gameEvent, bool toEnd = false)
    {
        if (_eventQueue.Contains(gameEvent)) return; // Prevent multiple enqueueing
        
        if (!_events.Contains(gameEvent)) throw new ArgumentException("Try to enqueue unknown event");

        int pos;
        if (gameEvent.IsTrigger)
            pos = 0;
        else if (toEnd)
            pos = _eventQueue.Count;
        else 
            pos = Random.Range(0, _eventQueue.Count + 1);

        _eventQueue.Insert(pos, gameEvent);
    }

    public void DequeueEvent(GameEvent gameEvent)
    {
        if (!_events.Contains(gameEvent)) throw new ArgumentException("Try to dequeue unknown event");

        _eventQueue.Remove(gameEvent);
    }

    public List<GameEvent> GetQueue()
    {
        List<GameEvent> res = new();
        res.AddRange(_eventQueue);
        return res;
    }
    
    public GameEvent GetNext()
    {
        if (_eventQueue.Count < 1)
        {
            if (_timedEvents.Count > 0)
            {
                GameEvent e = _timedEvents[0];
                _timedEvents.Remove(e);
                return e;
            }
            return null;
        }

        GameEvent res;
        
        if (_timedEvents.Count > 0 && _timedEvents[0].TurnPosition <= _currentTurn)
        {
            res = _timedEvents[0];
            _timedEvents.Remove(res);
            
            if (!res.IsAvailable())
            {
                return GetNext();
            }
        }
        else
        {
            res = _eventQueue[0];

            _eventQueue.Remove(res);
            if (!res.IsDisposable)
            {
                EnqueueEvent(res, true);
            }
            else
            {
                res.DisableDynamicChecking();
                _events.Remove(res);
            }
        }
        
        if (!res.SkipTurn)
            _currentTurn++;

        CurrentEvent = res;
        return res;
    }

    public GameEvent GetFail()
    {
        return _failEvents.Count < 1 ? null : _failEvents.FirstOrDefault(gameEvent => gameEvent.IsAvailable());
    }

    public GameEvent GetWin()
    {
        return _winEvents.Count < 1 ? null : _winEvents.FirstOrDefault(gameEvent => gameEvent.IsAvailable());
    }
}
