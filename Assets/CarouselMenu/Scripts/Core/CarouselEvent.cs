using System;
using UnityEngine;
using UnityEngine.Events;

namespace MS.Carousel.Core
{
  /*
    public class CarouselConstants : MonoBehaviour
    {
        public enum CarouselType
        {
            Linear,
            ScaledLinear,
            CoverFlow,
            ScaledCoverFlow,
            Wheel,
        };
    }
  */
    [Serializable]
    public class CarouselEvent : UnityEvent<int, int> { }
}
