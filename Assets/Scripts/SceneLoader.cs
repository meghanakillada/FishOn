// SceneLoader.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitToTitle() => SceneManager.LoadScene("Title");
    public void PlayGame() => SceneManager.LoadScene("Game");
    public void QuitApp() => Application.Quit(); // works in build
}