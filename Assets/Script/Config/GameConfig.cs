using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = nameof(GameConfig), menuName = "Config/GameConfig")]
public class GameConfig : ConfigBase<GameConfig>
{
    [SerializeField] private Bubble bubble; public static Bubble Bubble { get { return Instance.bubble; } }
    [SerializeField] private List<BubbleInfo> bubbleInfos; public static List<BubbleInfo> BubbleInfos { get { return Instance.bubbleInfos; } }

}
