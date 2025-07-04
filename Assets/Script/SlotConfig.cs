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
	public int reelCount = 3;
	public int symbolsPerReel = 3;
	public SymbolData[] symbols; // 所有符號的資料
	public float reelSpacing = 150f;
	public float symbolHeight = 100f;
	public float spinDuration = 2f;
}