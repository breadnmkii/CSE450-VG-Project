using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MS.Carousel.Core
{
  [CustomEditor(typeof(CarouselController))]
  public class CarouselControllerEditor : Editor
  {
    private Vector3 _x1, _x2, _y1, _y2;
    private Vector3[] _verts = new Vector3[4];
    void OnSceneGUI()
    {
      CarouselController t = target as CarouselController;
      if (!t.ConfigData.ShowBoundaryRect)
        return;
      var boundary = t.Data.Boundary;

      _x1.x = boundary.xMin+t.Center.position.x;
      _x1.y = boundary.yMin+t.Center.position.y;
      _y1.x = boundary.xMax+t.Center.position.x;
      _y1.y = boundary.yMin+t.Center.position.y;
      _x2.x = boundary.xMax+t.Center.position.x;
      _x2.y = boundary.yMax+t.Center.position.y;
      _y2.x = boundary.xMin+t.Center.position.x;
      _y2.y = boundary.yMax+t.Center.position.y;
      _x1.z = _y1.z = _x2.z = _y2.z = t.Center.position.z;
      
      _verts[0] = _x1;
      _verts[1] = _y1;
      _verts[2] = _x2;
      _verts[3] = _y2;

      var outlineColor = Color.yellow;
      Handles.DrawSolidRectangleWithOutline(_verts, new Color(0.5f, 0.5f, 0.5f, 0.1f), outlineColor);
    }
  }
}