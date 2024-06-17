using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig : ConfigBase<GameConfig>
{
    [SerializeField] private GameObject bubble; public static GameObject Bubble { get { return Instance.bubble; } }
}
