using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MS.Carousel.Core;


public class CarouselDemoController : MonoBehaviour
{
  [SerializeField] GameObject _cell;
  [SerializeField] CarouselConfigData _configData;
  [SerializeField] CarouselController _carouselController;
  public Toggle AutoFocusToggle;
  public Toggle SelectFocusToggle;
  public Toggle LoopToggle;
  public Slider CoverFlowAngleXSlider;
  public Slider CoverFlowAngleYSlider;
  public Slider CoverFlowAngleZSlider;
  public Slider CellGapSlider;
  public Slider ScaleXSlider;
  public Slider ScaleYSlider;
  public Slider AxisXSlider;
  public Slider AxisYSlider;

  public float Force = 1000;
  public float MinRandomRatio = 0.5f;
  public float MaxRandomRatio = 0.5f;

  public void AddCenterCell()
  {
    GameObject go = Instantiate(_cell, _carouselController.CellRoot);
    _carouselController.AddCell(_carouselController.Data.CenterCellIndex, go);
  }

  public void RemoveCenterCell()
  {
    _carouselController.RemoveCell(_carouselController.Data.CenterCellIndex);
  }

  public void OnSpin()
  {
    var newVel = _configData.Axes * (Force * Random.Range(MinRandomRatio, MaxRandomRatio));
    _carouselController.ChangeVelocity(newVel);
  }

  public void OnBackBtnDown()
  {
    SceneManager.LoadScene("Start");
  }
  
  public void OnScaleValueChanged()
  {
    _configData.ScaleRatio = new Vector3(ScaleXSlider.value, ScaleYSlider.value, _configData.ScaleRatio.z);
  }
  
  public void OnCellGapSliderValueChanged()
  {
    _configData.CellGap = CellGapSlider.value;
  }
  
  public void OnCoverFlowSliderValueChanged()
  {
    _configData.CoverflowAngles = new Vector3(CoverFlowAngleXSlider.value, CoverFlowAngleYSlider.value, CoverFlowAngleZSlider.value);
  }
  
  public void OnAutoFocusToggleValueChanged()
  {
    _configData.FocusCenter = AutoFocusToggle.isOn;
  }
  
  public void OnSelectFocusToggleValueChanged()
  {
    _configData.CenterSelect = SelectFocusToggle.isOn;
  }
  
  public void OnShouldLoopValueChanged()
  {
    _configData.Loop = LoopToggle.isOn;
  }
  
  public void OnAxisXValueChanged()
  {
    _configData.Axes.x = AxisXSlider.value;
  }

  public void OnAxisYValueChanged()
  {
    _configData.Axes.y = AxisYSlider.value;
  }

  public void OnScaleProcessorValueChanged(bool value)
  {
    foreach(var processor in _carouselController.Processors)
    {
      if (processor is Carousel2DScaleProcessor)
      {
        processor.Enabled = value;
      }
    }
  }

  public void OnCoverflowRotationProcessorValueChanged(bool value)
  {
    foreach(var processor in _carouselController.Processors)
    {
      if (processor is Carousel2DCoverflowRotationProcessor)
      {
        processor.Enabled = value;
      }
    }
  }

  public void OnCellDown(int arrayIndex, int cellIndex)
  {
    Debug.Log("Select: " + arrayIndex + " cell index:" + cellIndex);
  }
}