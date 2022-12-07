using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelMenu : MonoBehaviour
{
    public Text score;
    public Text time;
    public float max;
    public float numOfClicks;
    public float storedClicks;

    public void Start()
    {
        time.text = TimeManager.sharedInstance.GetCurrentDateTime().ToString();
        storedClicks = PlayerPrefs.GetFloat("StoredClicks", 0);
        numOfClicks = storedClicks;
        score.text = storedClicks.ToString() + "/" + max.ToString();
    }
    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Debug.Log("QUIT");
        Application.Quit();
    }

    public void UpdateNumber()
    {
        if (numOfClicks < max)
        {
            numOfClicks++;
            score.text = numOfClicks.ToString() + "/" + max.ToString();
            if (numOfClicks > storedClicks)
            {
                PlayerPrefs.SetFloat("StoredClicks", numOfClicks);
            }
        }   
    }

    public void ResetNumber()
    {
        PlayerPrefs.DeleteKey("StoredClicks");
        numOfClicks = 0f;
        score.text = "0/" + max.ToString();
    }
}
