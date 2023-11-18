using System;

namespace MS.Carousel.Core
{ 
  [Serializable]
  public enum MovementDirection
  {
    Backward,
    Forward
  }

  [Serializable]
  public enum BoundaryUpdateType
  {
    // Automatically calculate the boundary based on cells 
    CalculatedByCells,
    // Use the rect from RectTransform 
    UseRectTransform
  }

  [Serializable]
  public enum DraggingType
  {
    X,
    Y
  }
}
