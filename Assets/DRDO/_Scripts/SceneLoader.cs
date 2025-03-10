using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    
    public void LoadMainMenu(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }
}
