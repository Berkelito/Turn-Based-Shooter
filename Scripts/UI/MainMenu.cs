using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private bool stopTimeOnEnable = true;

    public void ToggleObject(GameObject gameObject)
    { 
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }
    public void LoadLevel(string name)
    { 
        References.Instance.SceneManager.LoadScene(name);
    }

    private void OnEnable()
    {
        if (stopTimeOnEnable)
            Time.timeScale = 0;
    }

    private void OnDisable()
    {
        if (stopTimeOnEnable)
            Time.timeScale = 1.0f;
    }

    public void Quit()
    { 
        Application.Quit();
    }
}
