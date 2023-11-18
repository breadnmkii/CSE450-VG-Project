using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MS.Carousel.Core
{
  [CreateAssetMenu(menuName = "Carousel/Processor/2DRotation")]
  public class Carousel2DCoverflowRotationProcessor : CarouselMovementProcessorBase
  { 
    private float _dis;
    private GameObject _centerCell;
    public override void CalculateCellTransformation
    (
      GameObject cell,
      int cellIndex,
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
      if (data.CenterCellIndex > cellIndex)
      {
        data.FinalRotation = configData.CoverflowAngles;
      }
      else if (data.CenterCellIndex < cellIndex)
      {
        data.FinalRotation = -configData.CoverflowAngles;
      }
      else 
      {
        data.FinalRotation = Vector3.zero;
      }
    }
  }
}