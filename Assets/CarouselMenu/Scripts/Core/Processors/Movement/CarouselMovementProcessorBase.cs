using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MS.Carousel.Core
{
  public class CarouselMovementProcessorBase : ScriptableObject
  {
    public bool Enabled = true;
    protected float _width;
    protected float _height;
    protected Vector3 _newPos;
    protected RectTransform _tempRT;
    
    public virtual void CalculateCellTransformation
    (
      GameObject cell,
      int index,
      List<GameObject> cells,
      CarouselConfigData configData,
      ref CarouselData data,
      ScrollRect scroll,
      RectTransform panel,
      RectTransform center,
      RectTransform canvasRect,
      RectTransform scrollRectTransform
    )
    { }
  }
}