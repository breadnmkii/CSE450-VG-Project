using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneUIController : MonoBehaviour
{
  public void OnSlotMachineDemoBtnDown()
  {
    SceneManager.LoadScene("SlotMachine");
  }

  public void OnCoreDemoBtnDown()
  {
    SceneManager.LoadScene("CoreDemo");
  }
}
