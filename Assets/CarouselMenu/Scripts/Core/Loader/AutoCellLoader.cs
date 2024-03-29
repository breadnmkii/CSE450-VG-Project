using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Carousel.Core
{
  public class AutoCellLoader : CarouselLoaderBase
  {
    [SerializeField] private GameObject[] _cells;
    [SerializeField] private int _totalCells;

    public override void Load(CarouselController controller)
    {
      // Auto create cells
      if (_cells.Length>0)
      {
        for (int i = 0; i < _cells.Length; ++i)
        {
          GameObject go = Instantiate(_cells[i], controller.CellRoot);
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