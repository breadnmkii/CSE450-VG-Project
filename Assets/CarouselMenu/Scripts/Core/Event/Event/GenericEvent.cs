using System.Collections.Generic;
using UnityEngine;
//using Sirenix.OdinInspector;

namespace MS.Core
{
  public abstract class GenericEvent<T> : ScriptableObject, IEvent<T>
  {
    //[ShowInInspector]
    public List<IEventListener<T>> EventListeners { get; private set; } = new List<IEventListener<T>>();

    public bool DebugLog = false;

    public void Raise(T payload)
    {
      if (DebugLog)
      {
        Debug.Log($"Begin Event Raise for {this}");
        Debug.Log($"How many listeners: {EventListeners.Count} for {this}");
        foreach(var l in EventListeners)
        {
          Debug.Log(l);
        }
      }
      
      for (int i = EventListeners.Count - 1; i >= 0; i--)
      {
        if (DebugLog)
        {
          Debug.Log($"Calling {EventListeners[i]}");
        }
        
        EventListeners[i].OnEventRaised(payload);
      }
    }

    public override string ToString()
    {
      return string.Format("Event_{0}<{1}>", name, typeof(T));
    }
  }
}