using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject successUI;
    public GameObject loseUI;

    public void ShowSuccessUI()
    {
        successUI.SetActive(true);
        loseUI.SetActive(false);
    }

    public void ShowLoseUI()
    {
        successUI.SetActive(false);
        loseUI.SetActive(true);
    }
}