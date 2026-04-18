using UnityEngine;
using UnityEngine.SceneManagement;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public void switchScenes(string sceneName)
    {
        
        SceneManager.LoadScene(sceneName);
    }
}
