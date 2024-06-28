using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : Singleton<SceneTransition>
{

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        GameObject obj = new GameObject(typeof(SceneTransition).ToString());
        obj.AddComponent<SceneTransition>();

    }

    public void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void ReLoadScene()
    {
        LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
