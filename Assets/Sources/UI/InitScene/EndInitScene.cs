using UnityEngine;
using UnityEngine.SceneManagement;

public class EndInitScene : MonoBehaviour
{
    public UIFade FadeINOUT;
    public void OnEndInitScene()
    {
        //FadeINOUT.FadeIn();
        GoToMainMenu();
    }

    void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
