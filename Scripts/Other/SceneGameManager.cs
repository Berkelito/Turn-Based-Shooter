using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneGameManager : MonoBehaviour
{
    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }
}
