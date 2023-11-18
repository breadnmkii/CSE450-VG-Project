using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MS.Core;

namespace MS.Carousel.Core
{
  public class CarouselController : MonoBehaviour
  {
    public ScrollRect ScrollRect { get { return _scroll; } }
    public CarouselConfigData ConfigData { get { return _configData; } }
    public CarouselData Data { get { return _data; } }
    public RectTransform Center { get { return _center; } }
    public RectTransform CellRoot { get { return _cellContent; } }
    public List<CarouselMovementProcessorBase> Processors;
    [SerializeField] private CarouselLoaderBase _cellLoader;
    [Tooltip("Data that controls how cell position is calculated")]
    [SerializeField] CarouselConfigData _configData;
    [Tooltip("Data that will be changed by processor dynamically")]
    [SerializeField] CarouselData _data;
    [SerializeField] RectTransform _canvasRect;
    [SerializeField] ScrollRect _scroll;
    [SerializeField] RectTransform _boundary;
    [SerializeField] RectTransform _cellContent;
    // Center to compare the distance for each objects     
    [SerializeField] RectTransform _center;
    [SerializeField] ListEvent _cellAddedEvent;
    [SerializeField] ListEvent _cellRemovedEvent;
    [SerializeField] ListEvent _cellsLoadedEvent;
    [SerializeField] ListEvent _cellsUnloadedEvent;

    // Hold all cells
    public List<GameObject> CellContainer = new List<GameObject>();
    private Vector3 _calibratedPos;
    private RectTransform _tempRT;
    private GameObject _tempCell;
    private int _halfAutoSnapIndex;
    private int _finalAutoSnapIndex;
    private float _halfAutoSnapDistance;
    private float _newAutoSnapDistance;
    private Vector3 _tempDestOffset;
    private GameObject _tempAutoSnapClosestCell;
    private Vector3 _tempFrom;
    private Vector3 _tempTo;
    private Vector3 _finalPos;
    private Vector3 _finalScale;
    protected Vector3 _finalPosWithOffset;
    private int _tempBoundaryCellIndex; 
    private BoundaryUpdateType _lastBoundaryUpdateType;
    private Vector3 _lastAxes;
    private Vector3 _lastScaleRatio;
    private Vector3 _lastCoverflowAngles;
    private float _lastCellGap;
    private float _lastMinScaleRange;
    private float _lastMaxScaleRange;

    #region Event Callbacks 

    public void StartDrag()
    {
      // Reset select index so it won't focus on last select index
      _data.SelectedCell = null;
      _data.StartDraggingPosition = _scroll.content.localPosition;
      _data.Dragging = true;
    }

    public void Dragging()
    {
    }

    public void EndDrag()
    {
      _data.Dragging = false;
    }

    public void OnCellClicked(List<object> payload)
    {
      if (payload.Count < 1)
        return;
     
      _data.SelectedCell = (GameObject)payload[0];
      _data.UpdateSnapDirection = true;                                                                                                                                                                                                                                                                                                                                                                                                                                           
    }

    #endregion

    #region Mono 

    void OnEnable()
    {
      // Get the start position here so the center offset will always be zero
      _data.StartDraggingPosition = _scroll.content.localPosition;
      // Make sure the transformation will run at least once
      _data.UpdateTransform = true;

      // Default center cell is zero
      _data.CellIndex = 0;
      _cellLoader.Load(this);
    }

    void OnDisable()
    {
      _cellLoader.Unload(this);
    }

    void LateUpdate()
    {
      if (CellContainer.Count == 0)
        return;
      // Only recalculate angle when we have axes changed, since angel calculation 
      // is expensive
      if 
      (
        _lastAxes != _configData.Axes 
        || _lastBoundaryUpdateType != _configData.BoundaryUpdateType 
        || _lastScaleRatio != _configData.ScaleRatio 
        || _lastCoverflowAngles != _configData.CoverflowAngles
        || _lastCellGap != _configData.CellGap
        || _lastMinScaleRange != _configData.MinScaleRange
        || _lastMaxScaleRange != _configData.MaxScaleRange
      )
      {
        _lastAxes = _configData.Axes;
        _lastBoundaryUpdateType = _configData.BoundaryUpdateType;
        _lastScaleRatio = _configData.ScaleRatio;
        _lastCoverflowAngles = _configData.CoverflowAngles;
        _lastCellGap = _configData.CellGap;
        _lastMinScaleRange = _configData.MinScaleRange;
        _lastMaxScaleRange = _configData.MaxScaleRange;
        UpdateAngle();
        UpdateBoundary();
        UpdateCellPositionsWithoutCheckingBoundary();
        return;
      }

      UpdateCenterCellIndex();
      // Skip calculation if menu has done moving
      if 
      (
        !ProcessCenterSnap()
        && !ProcessAutoSnap() 
        && _scroll.velocity == Vector2.zero 
        && !_data.UpdateBoundary
        && !_data.UpdateTransform 
        && !_data.Dragging
      )
      {
        return;
      }

      //Debug.LogError($"Update position: {_scroll.velocity == Vector2.zero} {_data.UpdateBoundary} {_data.UpdateTransform} {_data.Dragging}");
      //UpdateCenterCellIndex();
      UpdateDragging();
      UpdateDistance();
      UpdateDirection(false);
      CalculatePositionOffset();
      // Update cell content position based on dragging position 
      _cellContent.localPosition = _data.PositionOffset;
     
      // Update individual cell position
      _data.UpdateTransform = false;
      _data.UpdateBoundary = false;
      for (int i = 0; i < CellContainer.Count; ++i)
      {
        _tempCell = CellContainer[i];

        ResetCalculationData();

        _data.CellIndexOffset = i - _data.CellIndex;
        foreach (var processor in Processors)
        {
          processor.CalculateCellTransformation
          (
            cell: _tempCell,
            index: i,
            cells: CellContainer,
            configData: _configData,
            data: ref _data,
            scroll: _scroll,
            panel: _cellContent,
            center: _center,
            canvasRect: _canvasRect,
            scrollRectTransform: _scroll.content
          );
        }

        CheckCellBoundary
        (
          cell: _tempCell,
          index: i,
          cells: CellContainer,
          cellContent: _cellContent,
          center: _center,
          canvasRect: _canvasRect,
          scroll: _scroll,
          configData: _configData,
          data: ref _data
        );
          
        UpdateFinalCellTransformation
        (
          instantUpdate: _configData.InstantUpdate,
          cell: _tempCell
        );

        // Because boundary update will reorder the cell array, so we just break it 
        if (Data.UpdateBoundary)
        {
          break;
        }
        
      }
      
      _data.LastDragPos = _scroll.content.localPosition;
    }

    #endregion

    #region Public Accesssible Functions

    public void ChangeVelocity(Vector2 newVelocity)
    {
      _data.SelectedCell = null;
      _scroll.velocity = newVelocity;
    }

    #endregion

    #region Core Carousel Logic 

    public virtual void UpdateDragging()
    {
      
      // Use rect corner angle to update the dragging type
      if ((_data.Angle >= _data.BounaryAngle && _data.Angle <= 180 - _data.BounaryAngle) || (_data.Angle <= -_data.BounaryAngle && _data.Angle >= -(180 - _data.BounaryAngle)))
      {
        _data.DraggingType = DraggingType.X;
      }
      else
      {
        _data.DraggingType = DraggingType.Y;
      }
    }

    public virtual void UpdateCenterCellIndex()
    {
      _halfAutoSnapIndex = CellContainer.Count / 2;
      _finalAutoSnapIndex = _halfAutoSnapIndex;
      _halfAutoSnapDistance = Vector2.Distance(_center.transform.position, CellContainer[_halfAutoSnapIndex].transform.position);
      // Check left
      for (int i = _halfAutoSnapIndex - 1; i >= 0; --i)
      {
        _newAutoSnapDistance = Vector2.Distance(_center.transform.position, CellContainer[i].transform.position);
        if (_newAutoSnapDistance < _halfAutoSnapDistance)
        {
          _halfAutoSnapDistance = _newAutoSnapDistance;
          _finalAutoSnapIndex = i;
        }
        else 
        {
          // Just break, once it is bigger
          break;
        }
      }
      // Check right 
      for (int i = _halfAutoSnapIndex + 1; i < CellContainer.Count; ++i)
      {
        _newAutoSnapDistance = Vector2.Distance(_center.transform.position, CellContainer[i].transform.position);
        if (_newAutoSnapDistance < _halfAutoSnapDistance)
        {
          _halfAutoSnapDistance = _newAutoSnapDistance;
          _finalAutoSnapIndex = i;
        }
        else 
        {
          // Just break, once it is bigger
          break;
        }
      }

      _data.CenterCellIndex = _finalAutoSnapIndex;
    }

    public virtual void UpdateDistance()
    {
      if (_data.DraggingType == DraggingType.X)
      {
        // Use x only for dragging direction
        if (_configData.Axes.x > 0)
          _data.Distance = _scroll.content.localPosition.x - _center.localPosition.x;
        else
          _data.Distance = _center.localPosition.x - _scroll.content.localPosition.x;
      }
      else 
      {
        if (_configData.Axes.y > 0)
          _data.Distance = _scroll.content.localPosition.y - _center.localPosition.y;
        else
          _data.Distance = _center.localPosition.y - _scroll.content.localPosition.y;
      }
    }

    public virtual void UpdateDirection(bool forceUpdate)
    {
      if (!_data.Dragging && !forceUpdate)
        return;
      if (_data.DraggingType == DraggingType.X)
      {
        if (_data.LastDragPos.x > _scroll.content.localPosition.x)
        {
          if (_configData.Axes.x != 0)
          {
            _data.Direction = MovementDirection.Backward;
          }
          else 
          {
            _data.Direction = MovementDirection.Forward;
          }
        }
        else if (_data.LastDragPos.x < _scroll.content.localPosition.x)
        {
          if (_configData.Axes.x != 0)
          {
            _data.Direction = MovementDirection.Forward;
          }
          else 
          {
            _data.Direction = MovementDirection.Backward;
          }
        }
      }
      else 
      {
        if (_data.LastDragPos.y > _scroll.content.localPosition.y)
        {
          if (_configData.Axes.x < 0 && _configData.Axes.y > 0)
          {
            _data.Direction = MovementDirection.Forward;
          }
          else if (_configData.Axes.x > 0 && _configData.Axes.y < 0)
          {
            _data.Direction = MovementDirection.Forward;
          }
          else
          {
            _data.Direction = MovementDirection.Backward;
          }
        }
        else if (_data.LastDragPos.y < _scroll.content.localPosition.y)
        {
          if (_configData.Axes.x < 0 && _configData.Axes.y > 0)
          {
            _data.Direction = MovementDirection.Backward;
          }
          else if (_configData.Axes.x > 0 && _configData.Axes.y < 0)
          {
            _data.Direction = MovementDirection.Backward;
          }
          else 
          {
            _data.Direction = MovementDirection.Forward;
          }
        }
      }
    }
    public virtual void CalculatePositionOffset()
    {
      _calibratedPos = _scroll.content.localPosition;
      _calibratedPos.x = _data.Distance * Mathf.Sin(_data.Angle * Mathf.Deg2Rad);
      _calibratedPos.y = _data.Distance * Mathf.Cos(_data.Angle * Mathf.Deg2Rad);
      _data.PositionOffset = _calibratedPos - _center.localPosition;
    }

    public virtual void ResetCalculationData()
    {
      _data.FinalPosition = Vector3.zero;
      _data.FinalRotation = Vector3.zero;
      _data.FinalScale = Vector3.one;
    }

    public virtual void CheckCellBoundary
    (
      GameObject cell,
      int index,
      List<GameObject> cells,
      RectTransform cellContent,
      RectTransform center,
      RectTransform canvasRect,
      ScrollRect scroll,
      CarouselConfigData configData,
      ref CarouselData data
    )
    {
      if (!configData.Loop)
        return;
      if (cells.Count == 0)
        return;

      _tempBoundaryCellIndex = GetBoundaryCellIndex(cells, configData, data);
      
      if (index == _tempBoundaryCellIndex)
      {
        // Because we use localPosition, so we need to consider the container position 
        _finalPosWithOffset = data.FinalPosition + cellContent.localPosition;
        switch (data.Direction)
        {
          case MovementDirection.Backward:
            if (data.DraggingType == DraggingType.X)
            {
              if (_finalPosWithOffset.x <= data.Boundary.xMin)
              {
                ReorderCell(cell, index, cells, configData, ref data);
              }
            }
            else
            {
              if (configData.Axes.x < 0 && configData.Axes.y < 0)
              {
                if (_finalPosWithOffset.y <= data.Boundary.yMin)
                {
                  ReorderCell(cell, index, cells, configData, ref data);
                }
              }
              else if (configData.Axes.x < 0)
              {
                if (_finalPosWithOffset.y >= data.Boundary.yMax)
                {
                  ReorderCell(cell, index, cells, configData, ref data);
                }
              }
              else if (configData.Axes.x > 0 && configData.Axes.y < 0)
              {
                if (_finalPosWithOffset.y >= data.Boundary.yMax)
                {
                  ReorderCell(cell, index, cells, configData, ref data);
                }
              }
              else 
              {
                if (_finalPosWithOffset.y <= data.Boundary.yMin)
                {
                  ReorderCell(cell, index, cells, configData, ref data);
                }
              }
            }
              
            break;
          case MovementDirection.Forward:
            if (data.DraggingType == DraggingType.X)
            {
              if (_finalPosWithOffset.x >= data.Boundary.xMax)
              {
                ReorderCell(cell, index, cells, configData, ref data);
              }
            }
            else
            {
              if (configData.Axes.x < 0 && configData.Axes.y < 0)
              {
                if (_finalPosWithOffset.y >= data.Boundary.yMax)
                {
                  ReorderCell(cell, index, cells, configData, ref data);
                }
              }
              else if (configData.Axes.x < 0)
              {
                if (_finalPosWithOffset.y <= data.Boundary.yMin)
                {
                  ReorderCell(cell, index, cells, configData, ref data);
                }
              }
              else if (configData.Axes.x > 0 && configData.Axes.y < 0)
              {
                if (_finalPosWithOffset.y <= data.Boundary.yMin)
                {
                  ReorderCell(cell, index, cells, configData, ref data);
                }
              }
              else 
              {
                if (_finalPosWithOffset.y >= data.Boundary.yMax)
                {
                  ReorderCell(cell, index, cells, configData, ref data);
                }
              }
            }
            break;
        }
      }
    }

    private int GetBoundaryCellIndex
    (
      List<GameObject> cells,
      CarouselConfigData configData,
      CarouselData data
    )
    {
      switch (data.Direction)
      {
        case MovementDirection.Forward:
        {
          switch (data.DraggingType)
          {
            case DraggingType.X:
            if (configData.Axes.x > 0)
            {
              return cells.Count - 1;
            }
            break;
            case DraggingType.Y:
            if (configData.Axes.x >= 0 && configData.Axes.y > 0)
            {
              return cells.Count - 1;
            }
            else if (configData.Axes.x > 0 && configData.Axes.y < 0)
            {
              return cells.Count - 1;
            }
            break;
          }
        }
        break;
        case MovementDirection.Backward:
        {
          switch (data.DraggingType)
          {
            case DraggingType.X:
            if (configData.Axes.x < 0)
            {
              return cells.Count - 1;
            }
            break;
            case DraggingType.Y:
            if (configData.Axes.x <= 0 && configData.Axes.y < 0)
            {
              return cells.Count - 1;
            }
            if (configData.Axes.x < 0 && configData.Axes.y > 0)
            {
              return cells.Count - 1;
            }
            else if (configData.Axes.x == 0 && configData.Axes.y < 0)
            {
              return cells.Count - 1;
            }
            break;
          }
        }
        break;
      }

      return 0;
    }

    private void ReorderCell
    (
      GameObject cell,
      int index,
      List<GameObject> cells,
      CarouselConfigData configData,
      ref CarouselData data)
    {
      if (index == 0)
      {
        cells.Remove(cell);
        cells.Add(cell);
        data.CellIndex--;
      }
      else
      {
        cells.Remove(cell);
        cells.Insert(0, cell);
        data.CellIndex++;
      }
      data.UpdateBoundary = true;
    }

    public virtual void UpdateFinalCellTransformation
    (
      bool instantUpdate,
      GameObject cell
    )
    {
      RectTransform rt = cell.GetComponent<RectTransform>();
      if (rt == null)
        return;

      _finalPos = rt.localPosition;
      _finalScale = rt.localScale;

      _data.FinalPosition.x = (float)Math.Round(_data.FinalPosition.x, 2);
      _data.FinalPosition.y = (float)Math.Round(_data.FinalPosition.y, 2);
      _data.FinalPosition.z = (float)Math.Round(_data.FinalPosition.z, 2);
      _finalPos.x = (float)Math.Round(_finalPos.x, 2);
      _finalPos.y = (float)Math.Round(_finalPos.y, 2);
      _finalPos.z = (float)Math.Round(_finalPos.z, 2);
      _data.FinalScale.x = (float)Math.Round(_data.FinalScale.x, 2);
      _data.FinalScale.y = (float)Math.Round(_data.FinalScale.y, 2);
      _data.FinalScale.z = (float)Math.Round(_data.FinalScale.z, 2);
      _finalScale.x = (float)Math.Round(_finalScale.x, 2);
      _finalScale.y = (float)Math.Round(_finalScale.y, 2);
      _finalScale.z = (float)Math.Round(_finalScale.z, 2);

      if (_finalPos != _data.FinalPosition)
      {
        //Debug.LogError($"{cell} Update pos: {_finalPos} {_data.FinalPosition}");
        _data.UpdateTransform = true;
      }

      if (_finalScale != _data.FinalScale)
      {
        //Debug.LogError($"{cell} Update Scale: {_finalScale} {_data.FinalScale}");
        _data.UpdateTransform = true;
      }

      _data.FinalQuaternion = Quaternion.Euler(_data.FinalRotation);
      if (Vector3Int.RoundToInt(rt.localEulerAngles) != Vector3Int.RoundToInt(_data.FinalQuaternion.eulerAngles))
      {
        //Debug.LogError($"{cell} Update rotation: {Vector3Int.RoundToInt(rt.localEulerAngles)} {Vector3Int.RoundToInt(_data.FinalQuaternion.eulerAngles)}");
        _data.UpdateTransform = true;
      }

      if (instantUpdate)
      {
        rt.localPosition = _data.FinalPosition;
        rt.localScale = _data.FinalScale;
        rt.localRotation = _data.FinalQuaternion;
        _cellContent.ForceUpdateRectTransforms();
      }
      else
      {
        rt.localPosition = Vector3.Lerp(rt.localPosition, _data.FinalPosition, Time.deltaTime * _configData.MoveSpeed);
        rt.localScale = Vector3.Lerp(rt.localScale, _data.FinalScale, Time.deltaTime * _configData.ScaleSpeed);
        rt.localRotation = Quaternion.Lerp(rt.localRotation, _data.FinalQuaternion, Time.deltaTime * _configData.RotateSpeed);
      }
    }

    public virtual void UpdateBoundary()
    {
      if (_configData.BoundaryUpdateType == BoundaryUpdateType.CalculatedByCells)
      {
        float totalWidth = 0, totalHeight = 0;

        foreach (var go in CellContainer)
        {
          _tempRT = go.GetComponent<RectTransform>();
          totalWidth += (_tempRT.rect.width + _configData.CellGap) * _canvasRect.localScale.x;
          totalHeight += (_tempRT.rect.height + _configData.CellGap) * _canvasRect.localScale.y;
        }

        totalWidth /= 2;
        totalHeight /= 2;

        _data.Boundary.xMin = _center.localPosition.x - totalWidth;
        _data.Boundary.yMin = _center.localPosition.y - totalHeight;
        _data.Boundary.xMax = _center.localPosition.x + totalWidth;
        _data.Boundary.yMax = _center.localPosition.y + totalWidth;
      }
      else
      {
        _data.Boundary = _boundary.rect;
      }
      
      _data.BounaryAngle = Vector2.SignedAngle(new Vector2(_data.Boundary.xMax, _data.Boundary.yMax), Vector3.up);
      _data.UpdateBoundary = true;
    }

    public virtual void UpdateCellPositionsWithoutCheckingBoundary()
    {
      if (_scroll.velocity != Vector2.zero || _data.Dragging == true)
        return;

      for (int i = 0; i < CellContainer.Count; ++i)
      {
        _tempCell = CellContainer[i];

        ResetCalculationData();

        _data.CellIndexOffset = i - _data.CellIndex;
        foreach (var processor in Processors)
        {
          processor.CalculateCellTransformation
          (
            cell: _tempCell,
            index: i,
            cells: CellContainer,
            configData: _configData,
            data: ref _data,
            scroll: _scroll,
            panel: _cellContent,
            center: _center,
            canvasRect: _canvasRect,
            scrollRectTransform: _scroll.content
          );
        }

        UpdateFinalCellTransformation
        (
          instantUpdate: true,
          cell: _tempCell
        );
      }
    }

    public virtual void UpdateAngle()
    {
      _data.Angle = Vector2.SignedAngle(_configData.Axes, Vector3.up);
    }

    /*
      Find the closest cell and snap to it 
     */
    public virtual bool ProcessAutoSnap()
    {
      if (!_configData.FocusCenter)
        return false;
      if (_data.SelectedCell != null)
        return false;
      if (_data.Dragging)
        return false;
      if (_scroll.velocity.magnitude > _configData.FocusCenterVelocityThreshold)
        return false;
      
      _tempAutoSnapClosestCell = CellContainer[_finalAutoSnapIndex];
      if (_halfAutoSnapDistance > _configData.FocusCenterDistanceThreshold)
      {
        _tempDestOffset = _tempAutoSnapClosestCell.transform.position - _center.transform.position;
        _tempDestOffset.z = 0;
        _scroll.content.localPosition -= _tempDestOffset * Time.deltaTime * _configData.FocusSpeed;
        return true;
      }
      return false;
    }

    public virtual bool ProcessCenterSnap()
    {
      if (!_configData.CenterSelect)
        return false;
      if (_data.Dragging)
        return false;
      if (_data.SelectedCell == null)
        return false;

      // Already center?
      _tempFrom = Center.transform.position;
      _tempTo = _data.SelectedCell.transform.position;
      _tempFrom.x = (float)Math.Round(_tempFrom.x, 2);
      _tempFrom.y = (float)Math.Round(_tempFrom.y, 2);
      _tempFrom.z = (float)Math.Round(_tempFrom.z, 2);
      _tempTo.x = (float)Math.Round(_tempTo.x, 2);
      _tempTo.y = (float)Math.Round(_tempTo.y, 2);
      _tempTo.z = (float)Math.Round(_tempTo.z, 2);
      if (_tempFrom == _tempTo)
      {
        return true;
      }
      
      // Reset the velocity
      _scroll.velocity = Vector3.zero;
      _tempDestOffset = _tempTo - _tempFrom;
      // 2d only
      _tempDestOffset.z = 0;
      // Update position
      _scroll.content.localPosition -= _tempDestOffset * Time.deltaTime * _configData.FocusSpeed;
      if (_data.UpdateSnapDirection)
      {
        _data.UpdateSnapDirection = false;
        UpdateDirection(true);
      }
      return true;
    }

    #endregion

    #region Cell Management

    public GameObject CreateCell(GameObject goToSpawn)
    {
      GameObject go = Instantiate(goToSpawn, _cellContent);
      AddCell(go);
      return go;
    }

    public GameObject CreateCell(int index, GameObject goToSpawn)
    {
      GameObject go = Instantiate(goToSpawn, _cellContent);
      AddCell(index, go);
      return go;
    }

    public void AddCell(GameObject go)
    {
      AddCell(CellContainer.Count, go);
    }

    public void AddCell(int index, GameObject go)
    {
      // 
      // Last cell, we just add
      // Otherwise, we insert
      CellContainer.Insert(index, go);
      UpdateBoundary();
      if (_cellAddedEvent)
        _cellAddedEvent.Raise(new List<object> { index, go });
      _scroll.velocity = Vector3.zero;
    }

    public void RemoveCell(int index)
    {
      if (CellContainer.Count <= 0)
        return;
      if (index < 0 || index >= CellContainer.Count)
        return;
      // Remove the current center cell
      GameObject go = CellContainer[index];
      CellContainer.Remove(go);
      Destroy(go);
      UpdateBoundary();
      if (_cellRemovedEvent)
        _cellRemovedEvent.Raise(new List<object> { index, go });
    }

    #endregion
  }
}