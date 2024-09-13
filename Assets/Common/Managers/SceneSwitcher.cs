using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "SceneSwitcher", menuName = "ScriptableObject/Manager/SceneSwitcher")]
public class SceneSwitcher : ScriptableObject
{
    public void SwitchToIntro()
    {
        Debug.Log($"Switch to Intro scene");
        SceneManager.LoadScene(SceneName.Intro.ToString(), LoadSceneMode.Single);
    }
    public void SwitchToLobby()
    {
        Debug.Log($"Switch to Lobby scene");
        SceneManager.LoadScene(SceneName.Lobby.ToString(), LoadSceneMode.Single);
    }
    public void SwitchToGame()
    {
        Debug.Log($"Switch to Game scene");
        SceneManager.LoadScene(SceneName.Game.ToString(), LoadSceneMode.Single);
    }
    public void SwitchToResult()
    {
        Debug.Log($"Switch to Result scene");
        SceneManager.LoadScene(SceneName.Result.ToString(), LoadSceneMode.Single);
    }
}
