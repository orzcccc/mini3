using System;
using GameFramework;
using GameFramework.Event;
using UnityGameFramework.Runtime;

/// <summary>
/// 事件系统业务门面，封装 UGF EventComponent。
/// </summary>
public sealed class EventMgr : Singleton<EventMgr>
{
    public void AddEvent(int eventId, EventHandler<GameEventArgs> handler)
    {
        if (handler == null)
        {
            return;
        }

        GetEventComponent().Subscribe(eventId, handler);
    }

    public void RemoveEvent(int eventId, EventHandler<GameEventArgs> handler)
    {
        if (handler == null)
        {
            return;
        }

        GetEventComponent().Unsubscribe(eventId, handler);
    }

    public bool Check(int eventId, EventHandler<GameEventArgs> handler)
    {
        if (handler == null)
        {
            return false;
        }

        return GetEventComponent().Check(eventId, handler);
    }

    public int Count(int eventId)
    {
        return GetEventComponent().Count(eventId);
    }

    public void Trigger(object sender, GameEventArgs e)
    {
        if (e == null)
        {
            return;
        }

        GetEventComponent().Fire(sender, e);
    }

    public void TriggerNow(object sender, GameEventArgs e)
    {
        if (e == null)
        {
            return;
        }

        GetEventComponent().FireNow(sender, e);
    }

    public void SetDefaultHandler(EventHandler<GameEventArgs> handler)
    {
        GetEventComponent().SetDefaultHandler(handler);
    }

    private static EventComponent GetEventComponent()
    {
        EventComponent eventComponent = GameEntry.GetComponent<EventComponent>();
        if (eventComponent == null)
        {
            throw new Exception("EventComponent is not found. Please check GameFramework scene setup.");
        }

        return eventComponent;
    }
}
