

using UnityEngine;

[CreateAssetMenu(fileName = nameof(LevelConfig), menuName = "Config/LevelConfig")]
public class LevelConfig : ConfigBase<LevelConfig>
{
    [SerializeField] private LevelInfo[] levelInfos; public static LevelInfo[] LevelInfos { get { return Instance.levelInfos; } }

}
