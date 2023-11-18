using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MS.Core;

public class ScriptableEventCellController : MonoBehaviour
{
  [SerializeField] Text _desc;
  [SerializeField] ListEvent _cellEvent;
  [SerializeField] int _index;

  public void UpdateCell(string desc, int index)
  {
    if (_desc == null)
      return;
    _desc.text = desc;
    _index = index;
  }

  public void OnBtnClicked()
  {
    _cellEvent?.Raise(new List<object> { this.gameObject, _index});
  }
}
