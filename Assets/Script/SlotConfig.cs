using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "SlotConfig", menuName = "SlotGame/SlotConfig")]
public class SlotConfig : ScriptableObject
{
	[Serializable]
	public struct SymbolData
	{
		public string name;
		public Sprite sprite; // 符號圖案
		public float probability; // 出現機率
		public float payout; // 賠率
	}

	[Serializable]
	public struct Payline
	{
		public int[] symbolIndices; // 每條 payline 的符號索引（每個滾輪一個索引）
	}

	[Serializable]
	private struct PaylineJson
	{
		public int[] symbolIndices; // 單條 payline 的 JSON 結構
	}

	[Serializable]
	private struct PaylinesJson
	{
		public PaylineJson[] paylines; // JSON 檔案的結構，包含多條 payline
	}

	public int reelCount = 3;
	public int symbolsPerReel = 3;
	public SymbolData[] symbols; // 所有符號的資料
	[SerializeField] private TextAsset paylineJsonFile; // 修改：單個 JSON 檔案
	public Payline[] paylines { get; private set; } // 動態生成的 paylines 陣列
	public float reelSpacing = 150f;
	public float symbolHeight = 100f;
	public float spinDuration = 2f;
	public int spinCycles = 5;
	public AnimationCurve beginAccelerationCurve = AnimationCurve.Linear(0, 0, 1, 1); // 緩加速曲線
	public AnimationCurve finalDecelerationCurve = AnimationCurve.Linear(0, 1, 1, 0); // 緩減速曲線
	public bool isUseFinalSnap;

	private void OnEnable()
	{
		// 從單個 JSON 檔案加載 paylines
		if (paylineJsonFile != null)
		{
			try
			{
				PaylinesJson jsonData = JsonUtility.FromJson<PaylinesJson>(paylineJsonFile.text);
				if (jsonData.paylines != null && jsonData.paylines.Length > 0)
				{
					List<Payline> loadedPaylines = new List<Payline>();
					foreach (var jsonPayline in jsonData.paylines)
					{
						if (jsonPayline.symbolIndices != null && jsonPayline.symbolIndices.Length == reelCount)
						{
							// 驗證每個索引是否有效
							bool isValid = jsonPayline.symbolIndices.All(index => index >= 0 && index < symbolsPerReel);
							if (isValid)
							{
								loadedPaylines.Add(new Payline { symbolIndices = jsonPayline.symbolIndices });
							}
							else
							{
								Debug.LogWarning($"Invalid symbol indices in payline: {string.Join(",", jsonPayline.symbolIndices)}. Indices must be between 0 and {symbolsPerReel - 1}.");
							}
						}
						else
						{
							Debug.LogWarning($"Invalid payline data in {paylineJsonFile.name}: symbolIndices length must match reelCount ({reelCount}).");
						}
					}
					paylines = loadedPaylines.ToArray();
				}
				else
				{
					Debug.LogWarning($"No paylines found in {paylineJsonFile.name}. Using default paylines.");
					SetDefaultPaylines();
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to parse payline JSON {paylineJsonFile.name}: {e.Message}");
				SetDefaultPaylines();
			}
		}
		else
		{
			Debug.LogWarning("No payline JSON file assigned. Using default paylines.");
			SetDefaultPaylines();
		}
	}

	private void SetDefaultPaylines()
	{
		paylines = new Payline[]
		{
			new Payline { symbolIndices = new int[] { 0, 0, 0 } }, // 頂部橫線
            new Payline { symbolIndices = new int[] { 1, 1, 1 } }, // 中間橫線
            new Payline { symbolIndices = new int[] { 2, 2, 2 } }, // 底部橫線
        };
	}
}