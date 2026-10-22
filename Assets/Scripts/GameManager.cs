using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private void OnEnable()
    {
        PlayerInputs.OnRestartPressed += RestartScene;
        PlayerInputs.OnEscapePressed += ExitGame;
    }

    private void OnDisable()
    {
        PlayerInputs.OnRestartPressed -= RestartScene;
        PlayerInputs.OnEscapePressed -= ExitGame;
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ExitGame()
    {
        Application.Quit();
    }
}