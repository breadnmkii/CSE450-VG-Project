using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Carousel.Core
{
  [CreateAssetMenu(menuName = "Carousel/Data/ConfigData")]
  public class CarouselConfigData : ScriptableObject
  {
    [Tooltip("Cell should snap to center after scrolling is finished")]
    public bool FocusCenter = true;
    [Tooltip("Cell should snap to center when being seleted")]
    public bool CenterSelect = true;
    [Tooltip("Boundary Type")]
    public BoundaryUpdateType BoundaryUpdateType = BoundaryUpdateType.CalculatedByCells;
    [Tooltip("Loop carousel")]
    public bool Loop = true;
    [Tooltip("Instant Update Cell Position")]
    public bool InstantUpdate = false;
    public float CellGap = 25f;
    public float FocusSpeed = 5f;
    public float ScaleSpeed = 5f;
    public float RotateSpeed = 5f;
    public float MoveSpeed = 10f;
    [Tooltip("When the scroll velocity is below this threshold, start focusing")]
    public float FocusCenterVelocityThreshold = 50f;
    [Tooltip("When the distance between focus cell & center is below the threshold, stop focusing")]
    public float FocusCenterDistanceThreshold = 0.1f;
    [Tooltip("Scale multiplication against final calculation over distance")]
    public Vector3 ScaleRatio = new Vector3(1, 1, 1);
    public Vector3 CoverflowAngles = new Vector3(0, 80, 0);
    [Tooltip("Vertical/Horizontal Scroll Axes")]
    public Vector3 Axes = new Vector3(0, 1, 0);
    [Range(0.1f, 100)]
    public float MinScaleRange = 0.1f;
    [Range(0.1f, 100)]
    public float MaxScaleRange = 100f;

    [Header("Debug Editor")]
    [Tooltip("Show boundary rect for debugger purpose")]
    public bool ShowBoundaryRect = false;
    public Color BoundaryRectOutlineColor = Color.yellow;
  }
}