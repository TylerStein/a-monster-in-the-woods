using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwapper : MonoBehaviour
{
    [SerializeField] private string _gameSceneName = "GameScene";

    public void StartGame()
    {
        SceneManager.LoadScene(_gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
