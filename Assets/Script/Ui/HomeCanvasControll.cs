using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeCanvasControll : MonoBehaviour
{
    [SerializeField] private Button play;
    [SerializeField] private TextMeshProUGUI playText;
    private int currentLevel;

    private void Awake()
    {
        play.onClick.AddListener(OnClickPlay);
        currentLevel = PlayerPrefs.GetInt("LevelID", 0);
        playText.text = "Level " + (currentLevel + 1);
    }

    public void OnClickPlay()
    {
        SceneTransition.Instance.LoadScene(1);
    }
}
