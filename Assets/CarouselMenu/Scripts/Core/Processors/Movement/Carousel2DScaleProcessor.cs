using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MS.Carousel.Core
{
  [CreateAssetMenu(menuName = "Carousel/Processor/2DScale")]
  public class Carousel2DScaleProcessor : CarouselMovementProcessorBase
  {
    private float _dis;
    private float _scaleRatio;
    private Vector3 _final;

    public override void CalculateCellTransformation
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
    {
      if (!Enabled)
        return;
      _tempRT = cell.GetComponent<RectTransform>();
      if (_tempRT == null)
        return;

      // Scale
      _dis = Vector3.Distance(_tempRT.position, center.position);
      if (_dis == 0)
        return;
      // Make sure it is not too small nor not too big
      _scaleRatio = Mathf.Clamp(1 - Mathf.Abs
      (
        _dis / (data.Boundary.width / 2)), 
        configData.MinScaleRange, 
        configData.MaxScaleRange
      );

      _final = new Vector3
      (
        _scaleRatio * configData.ScaleRatio.x,
        _scaleRatio * configData.ScaleRatio.y,
        _scaleRatio * configData.ScaleRatio.z
      );

      data.FinalScale = Vector3.Scale(data.FinalScale, _final);

    }
  }
}