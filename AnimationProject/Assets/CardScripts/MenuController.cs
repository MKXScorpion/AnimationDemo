using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject MenuPanel;
    public GameObject easyPanel, mediumPanel, hardPanel;
    public void SelectEasy()
    {
        GameSettings.rows = 3;
        GameSettings.columns = 3;
        GameSettings.timeLimit = 60f;
        easyPanel.SetActive(true);
        MenuPanel.SetActive(false);
    }

    public void SelectMedium()
    {
        GameSettings.rows = 5;
        GameSettings.columns = 4;
        GameSettings.timeLimit = 90f;
        SceneManager.LoadScene("MindMorga");
        mediumPanel.SetActive(true);
        MenuPanel.SetActive(false);
    }

    public void SelectHard()
    {
        GameSettings.rows = 5;
        GameSettings.columns = 5;
        GameSettings.timeLimit = 120f;
        SceneManager.LoadScene("MindMorga");
        hardPanel.SetActive(true);
        MenuPanel.SetActive(false);
    }

    public void SelectLevel(int levelIndex)
    {
        GameSettings.level = levelIndex;
        SceneManager.LoadScene("MindMorga");
    }

    public void BackMainMenu()
    {
        MenuPanel.SetActive(true);
        easyPanel.SetActive(false);
        mediumPanel.SetActive(false);
        hardPanel.SetActive(false);
    }
}
