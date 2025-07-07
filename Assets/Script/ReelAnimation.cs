using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReelAnimation : MonoBehaviour
{
	private List<Image> symbolImages;
	private SlotConfig config;
	private bool isSpinning;

	public void Initialize(SlotConfig config, List<Image> symbolImages)
	{
		this.config = config;
		this.symbolImages = symbolImages;
	}

	public void StartSpin(List<SlotConfig.SymbolData> targetSymbols)
	{
		if (isSpinning) return;
		isSpinning = true;
		StartCoroutine(SpinAnimation(targetSymbols));
	}

	public void StopSpin()
	{
		isSpinning = false;
	}

	public void PlayWinAnimation()
	{
		StartCoroutine(WinAnimation());
	}

	private IEnumerator SpinAnimation(List<SlotConfig.SymbolData> targetSymbols)
	{
		float elapsed = 0;
		float totalHeight = (config.symbolsPerReel - 1) * config.symbolHeight;
		float startY = (config.symbolsPerReel - 1) * config.symbolHeight / 2;
		float cycleDistance = totalHeight + config.symbolHeight;
		// 修改：使用基於 spinCycles 的 spinSpeed
		float spinSpeed = (config.spinCycles * cycleDistance) / config.spinDuration;

		// 主動畫階段
		while (elapsed < config.spinDuration)
		{
			foreach (var image in symbolImages)
			{
				RectTransform rect = image.GetComponent<RectTransform>();
				rect.anchoredPosition += new Vector2(0, -Time.deltaTime * spinSpeed);
				if (rect.anchoredPosition.y < -totalHeight / 2)
				{
					rect.anchoredPosition += new Vector2(0, totalHeight + config.symbolHeight);
					image.sprite = config.symbols[UnityEngine.Random.Range(0, config.symbols.Length)].sprite;
				}
			}
			elapsed += Time.deltaTime;
			yield return null;
		}

		// 修改：最終階段，滾動一輪並顯示最終符號
		float finalElapsed = 0;
		float finalDuration = cycleDistance / spinSpeed; // 單輪滾動時間
		List<bool> symbolSet = new List<bool>(new bool[symbolImages.Count]); // 追蹤是否已設置最終符號

		while (finalElapsed < finalDuration)
		{
			for (int i = 0; i < symbolImages.Count; i++)
			{
				RectTransform rect = symbolImages[i].GetComponent<RectTransform>();
				rect.anchoredPosition += new Vector2(0, -Time.deltaTime * spinSpeed);
				if (rect.anchoredPosition.y < -totalHeight / 2)
				{
					rect.anchoredPosition += new Vector2(0, totalHeight + config.symbolHeight);
					if (!symbolSet[i]) // 僅設置一次最終符號
					{
						symbolImages[i].sprite = targetSymbols[i].sprite;
						symbolSet[i] = true;
					}
				}
			}
			finalElapsed += Time.deltaTime;
			yield return null;
		}

		// 修改：確保最終精確對齊
		UpdateSymbols(targetSymbols);
	}

	private void UpdateSymbols(List<SlotConfig.SymbolData> targetSymbols)
	{
		float startY = (config.symbolsPerReel - 1) * config.symbolHeight / 2;
		for (int i = 0; i < symbolImages.Count; i++)
		{
			symbolImages[i].sprite = targetSymbols[i].sprite;
			symbolImages[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, startY - i * config.symbolHeight);
		}
	}

	private IEnumerator WinAnimation()
	{
		for (int i = 0; i < 6; i++)
		{
			foreach (var image in symbolImages)
			{
				image.enabled = !image.enabled;
			}
			yield return new WaitForSeconds(0.2f);
		}
		foreach (var image in symbolImages)
		{
			image.enabled = true;
		}
	}
}