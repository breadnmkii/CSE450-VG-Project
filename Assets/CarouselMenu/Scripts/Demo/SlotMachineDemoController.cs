using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MS.Carousel.Core;

public class SlotMachineDemoController : MonoBehaviour
{
  //[SerializeField] CarouselConfigData _configData;
  [SerializeField] ScrollRect _scroll;
  [SerializeField] List<CarouselController> _carouselList;
  public float Force = 1000;
  public float MinRandomRatio = 0.5f;
  public float MaxRandomRatio = 0.5f;

  public void OnBackBtnDown()
  {
    SceneManager.LoadScene("Start");
  }

  public void OnSpinBtnDown()
  {
    var vel = Vector2.zero;
    if (_scroll.horizontal)
      vel.x = 1;
    if (_scroll.vertical)
      vel.y = 1;

    foreach (var controller in _carouselList)
    {
      var newVel = vel * (Force * Random.Range(MinRandomRatio, MaxRandomRatio));
      controller.ChangeVelocity(newVel);
    }
  }
}
