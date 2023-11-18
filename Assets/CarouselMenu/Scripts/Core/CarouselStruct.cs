using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Carousel.Core
{ 
  [Serializable]
  public struct CarouselData
  {
    public GameObject SelectedCell;
    public MovementDirection Direction;
    public DraggingType DraggingType;
    public int CenterCellIndex;
    public int CellIndex;
    public int CellIndexOffset;
    public bool Dragging;
    public bool UpdateBoundary;
    public bool UpdateTransform;
    public bool UpdateSnapDirection;
    public Vector3 StartDraggingPosition;
    public Vector3 LastDragPos;
    public Rect Boundary;
    public Vector3 FinalPosition;
    public Vector3 FinalRotation;
    public Vector3 FinalScale;
    public Vector3 PositionOffset;
    public Quaternion FinalQuaternion;
    public float Angle;
    public float BounaryAngle;
    public float Distance;
  }
}
