
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayCanvasControl : Singleton<GamePlayCanvasControl>
{
    [SerializeField] WinPopup winPopup;
    [SerializeField] TextMeshProUGUI gemText;
    [SerializeField] Image header;

    private int gemCount;
    LevelInfo levelInfo;

    public float headerDis;
    public int GemCount { 

        get { return gemCount; }

        set
        {
            gemCount = value;
            gemText.text = "Gem: " + gemCount + "/" + levelInfo.GemCount ;
        }
    }
    private int curentLevelID;
    private void Start()
    {
        levelInfo = GameManager.Instance.levelInfo;

        if (levelInfo.LevelType != LevelType.CollectGem) gemText.gameObject.SetActive(false);
        else
        {
            GemCount = 0;
        }

        headerDis = GetHeightInWorldSpace(header.rectTransform);
    }
    public void ActiveWinPopup()
    {
        int levelID = curentLevelID + 1;
        if (levelID > 1)
        {
            levelID = 1;
        }
        PlayerPrefs.SetInt("LevelID", levelID);

        winPopup.gameObject.SetActive(true);
    }

    public void CheckGemCount()
    {
        if (levelInfo.LevelType != LevelType.CollectGem) return;

        if (gemCount >= levelInfo.GemCount)
        {
            GameManager.Instance.OnEndGame();
        }
    }

    float GetHeightInWorldSpace(RectTransform rectTransform)
    {
        float localHeight = rectTransform.rect.height;
        Vector3 wordPos = rectTransform.position;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(wordPos);
        Vector3 topPos = Camera.main.ScreenToWorldPoint(screenPos + Vector3.up * localHeight/2);
        Vector3 bottomPos = Camera.main.ScreenToWorldPoint(screenPos - Vector3.up * localHeight/2);

        float heightInWorldSpace = topPos.y - bottomPos.y;
        return heightInWorldSpace;
    }

}
