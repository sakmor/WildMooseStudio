using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReelView : MonoBehaviour
{
	private List<Image> symbolImages = new List<Image>();
	private ReelAnimation animation;
	private int extraSybolNumberLower = 1;
	private int extraSybolNumberUpper = 2;
	public void Initialize(SlotConfig config, GameObject symbolPrefab)
	{
		float startY = (config.symbolsPerReel - 2) * config.symbolHeight;
		for (int i = 0; i < config.symbolsPerReel; i++)
		{
			GameObject symbolObj = Instantiate(symbolPrefab, transform);
			symbolImages.Add(symbolObj.GetComponent<Image>());
			symbolObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, startY - i * config.symbolHeight);
		}
		animation = gameObject.AddComponent<ReelAnimation>();
		animation.Initialize(config, symbolImages);
	}

	public void StartSpin(List<SlotConfig.SymbolData> targetSymbols)
	{
		animation.StartSpin(targetSymbols);
	}

	public void StopSpin()
	{
		animation.StopSpin();
	}

	public void PlayWinAnimation(List<int> symbolIndices)
	{
		// 修改：傳遞多個符號索引
		animation.PlayWinAnimation(symbolIndices);
	}
}