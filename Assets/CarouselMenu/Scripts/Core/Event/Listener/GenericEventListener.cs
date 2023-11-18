using UnityEngine;
using UnityEngine.Events;
using System;

namespace MS.Core
{
  public abstract class GenericEventListener<T, U> : MonoBehaviour, IEventListener<U> where T : IEvent<U>
  {
    public T Event;
    [Tooltip("Response to invoke when Event is raised")]
    public UnityEvent<U> Response;
    [Tooltip("Either listen at Awake or OnEnable")]
    public bool ListenAtAwake = false;

    private void Awake()
    {
      if (!ListenAtAwake)
        return;
      if (Event.EventListeners == null)
        return;
      if (!Event.EventListeners.Contains(this))
        Event.EventListeners.Add(this);
    }

    private void OnDestroy()
    {
      if (!ListenAtAwake)
        return;
      if (Event.EventListeners == null)
        return;
      if (Event.EventListeners.Contains(this))
        Event.EventListeners.Remove(this);
    }

    private void OnEnable()
    {
      if (ListenAtAwake)
        return;
      if (Event.EventListeners == null)
        return;
      if (!Event.EventListeners.Contains(this))
        Event.EventListeners.Add(this);
    }

    private void OnDisable()
    {
      if (ListenAtAwake)
        return;
      if (Event.EventListeners == null)
        return;
      if (Event.EventListeners.Contains(this))
        Event.EventListeners.Remove(this);
    }

    public virtual void OnEventRaised(U payload) 
    {
      Response.Invoke(payload);
    }

    public override String ToString()
    {
      return String.Format("Listener_{0}<{1}>", gameObject.name, typeof(T));
    }
  }
}