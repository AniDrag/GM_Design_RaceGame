using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartMultiPlayerGame()
    {
        SceneManager.LoadSceneAsync(1);
        Camera.Multiplayer = true;
    }
    public void StartSinglePlayerGame()
    {
        SceneManager.LoadSceneAsync(2);
        Camera.Multiplayer = false;
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public void StartMainMenu()
    {
        SceneManager.LoadSceneAsync(0);

    }
}
