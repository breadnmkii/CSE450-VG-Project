using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MS.Carousel.Core
{
  [CreateAssetMenu(menuName = "Carousel/Processor/2DPosition")]
  public class Carousel2DPositionProcessor : CarouselMovementProcessorBase
  {
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
      data.FinalPosition += CalculateCellPosition
      (
        cell: cell,
        configData: configData,
        data: ref data,
        totalCells: cells.Count,
        cellIndexOffset: data.CellIndexOffset,
        positionOffset: data.PositionOffset,
        center: center,
        canvasRect: canvasRect
      );
    }

    Vector3 CalculateCellPosition
    (
      GameObject cell,
      CarouselConfigData configData,
      ref CarouselData data,
      int totalCells,
      int cellIndexOffset,
      Vector3 positionOffset,
      RectTransform center,
      RectTransform canvasRect
    )
    {
      _tempRT = cell.GetComponent<RectTransform>();
      if (_tempRT == null)
        return Vector3.zero;

      _newPos = Vector3.zero;
      // Consider the canvas scale 
      _width = (_tempRT.rect.width + configData.CellGap) * canvasRect.localScale.x;
      _height = (_tempRT.rect.width + configData.CellGap) * canvasRect.localScale.x;
      _newPos.x = _width * cellIndexOffset * configData.Axes.x;
      _newPos.y = _height * cellIndexOffset * configData.Axes.y;
      return center.localPosition + _newPos + positionOffset;
    }
  }
}