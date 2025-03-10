using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartMultiPlayerGame()
    {
        SceneManager.LoadSceneAsync(1);
    }
    public void StartSinglePlayerGame()
    {
        SceneManager.LoadSceneAsync(2);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
