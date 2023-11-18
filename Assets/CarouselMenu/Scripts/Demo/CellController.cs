using System;
using UnityEngine;
using UnityEngine.UI;

public class CellEventArgs : EventArgs
{
  public int Index;

  public CellEventArgs(int i)
  {
    Index = i;
  }
}

public class CellController : MonoBehaviour
{
  public event EventHandler<CellEventArgs> Clicked;
  public int _index;
  public Text _desc;

  public void UpdateDesc(string desc)
  {
    if (_desc == null)
      return;
    _desc.text = desc;
  }

  public void OnBtnDown()
  {
    Clicked?.Invoke(this, new CellEventArgs(_index));
  }
}