using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = nameof(GameConfig), menuName = "Config/GameConfig")]
public class GameConfig : ConfigBase<GameConfig>
{
    [SerializeField] private Bubble bubble; public static Bubble Bubble { get { return Instance.bubble; } }
}
