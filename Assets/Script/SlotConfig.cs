using UnityEngine;
using System;

[CreateAssetMenu(fileName = "SlotConfig", menuName = "SlotGame/SlotConfig")]
public class SlotConfig : ScriptableObject
{
    [Serializable]
    public struct SymbolData
    {
        public Sprite sprite; // 符號圖案
        public float probability; // 出現機率
        public float payout; // 賠率
    }

    public SymbolData[] symbols; // 所有符號的資料
}