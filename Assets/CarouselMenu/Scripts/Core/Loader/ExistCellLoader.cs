using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Carousel.Core
{
  public class ExistCellLoader : CarouselLoaderBase
  {
    public override void Load(CarouselController controller)
    {
      // Load cells from list
      for (int i = 0; i < controller.CellRoot.childCount; ++i)
      {
        GameObject go = controller.CellRoot.GetChild(i).gameObject;
        controller.AddCell(i, go);
      }

      controller.UpdateCellPositionsWithoutCheckingBoundary();
      if (_cellsLoadedEvent)
        _cellsLoadedEvent.Raise(new List<object> { });
    }

    public override void Unload(CarouselController controller)
    {
      while (controller.CellContainer.Count != 0)
      {
        controller.RemoveCell(0);
      }

      controller.CellContainer.Clear();
      if (_cellsUnloadedEvent)
        _cellsUnloadedEvent.Raise(new List<object> { });

    }
  }
}