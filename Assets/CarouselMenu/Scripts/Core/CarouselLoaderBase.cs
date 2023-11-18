using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MS.Core;

namespace MS.Carousel.Core
{
  public abstract class CarouselLoaderBase : MonoBehaviour
  {
    [SerializeField] protected ListEvent _cellsLoadedEvent;
    [SerializeField] protected ListEvent _cellsUnloadedEvent;
    public abstract void Load(CarouselController controller);
    public abstract void Unload(CarouselController controller);
  }
}