using System.Collections.Generic;

namespace MS.Core
{
  public interface IEventListener<T>
  {
    public void OnEventRaised(T payload);
  }

  public interface IEvent<T>
  {
    public List<IEventListener<T>> EventListeners { get; }
    public void Raise(T payload);
  }
}