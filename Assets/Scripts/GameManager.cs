using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject charPanel;
    public GameObject smokeButton;
    public GameObject arrowButton;
    public GameObject suppButton;

    private void Awake()
    {
        Time.timeScale = 0.00000000001f;
    }
    public void SmokerChosen()
    {
        charPanel.SetActive(false);
        smokeButton.SetActive(true);
        Time.timeScale = 1f;
    }
    public void SovaChosen()
    {
        charPanel.SetActive(false);
        arrowButton.SetActive(true);
        Time.timeScale = 1f;
    }
    public void SuppChosen()
    {
        charPanel.SetActive(false);
        suppButton.SetActive(true);
        Time.timeScale = 1f;
    }
}
