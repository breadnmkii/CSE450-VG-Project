using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Carousel.Core
{
  public class AutoCellLoader : CarouselLoaderBase
  {
    [SerializeField] private GameObject _cell;
    [SerializeField] private int _totalCells;

    public override void Load(CarouselController controller)
    {
      // Auto create cells
      if (_cell != null)
      {
        for (int i = 0; i < _totalCells; ++i)
        {
          GameObject go = Instantiate(_cell, controller.CellRoot);
          controller.AddCell(i, go);
          go.name = "" + controller.CellContainer.Count;
          var cellController = go.GetComponent<ScriptableEventCellController>();
          if (cellController)
          {
            cellController.UpdateCell("" + controller.CellContainer.Count, controller.CellContainer.Count);
          }
        }
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