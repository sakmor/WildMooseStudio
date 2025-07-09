using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ReelAnimation : MonoBehaviour
{
	private List<Image> symbolImages;
	private SlotConfig config;
	private bool isSpinning;
	private float totalHeight;
	private float fullCycleDistance;
	private float spinSpeed;
	private int topestSymbolImagesIndex = 0;


	#region 初始化

	public void Initialize(SlotConfig config, List<Image> symbolImages)
	{
		this.config = config;
		this.symbolImages = symbolImages;
		CalculateSpinParameters();


	}

	private void CalculateSpinParameters()
	{
		totalHeight = (config.symbolsPerReel - 1) * config.symbolHeight;
		fullCycleDistance = totalHeight + config.symbolHeight;
		spinSpeed = (config.spinCycles * fullCycleDistance) / config.spinDuration;
	}

	#endregion

	#region 公開方法

	public void StartSpin(List<SlotConfig.SymbolData> targetSymbols)
	{
		if (isSpinning) return;
		isSpinning = true;
		StartCoroutine(SpinAnimation(targetSymbols));
		targetSymbols.ForEach(e => Debug.Log(e.name));
	}

	public void StopSpin()
	{
		isSpinning = false;
	}

	public void PlayWinAnimation()
	{
		StartCoroutine(WinAnimation());
	}

	#endregion

	#region 動畫邏輯

	private IEnumerator SpinAnimation(List<SlotConfig.SymbolData> targetSymbols)
	{

		// 初始滾動階段（單輪，緩加速）
		yield return StartCoroutine(UpdateSymbolPositions(fullCycleDistance / spinSpeed, false, null, config.beginAccelerationCurve));

		// 快速滾動階段（多輪，緩減速）
		yield return StartCoroutine(UpdateSymbolPositions(config.spinDuration, false, null, null));

		// 最終符號顯示與對齊階段（單輪）
		yield return StartCoroutine(UpdateSymbolPositions(fullCycleDistance / spinSpeed, true, targetSymbols, config.finalDecelerationCurve));

		isSpinning = false;
	}

	private IEnumerator UpdateSymbolPositions(float duration, bool isFinalPhase, List<SlotConfig.SymbolData> targetSymbols, AnimationCurve speedCurve = null)
	{
		float elapsed = 0;
		int symbolsSetCount = 0;
	

		// 設置targetSymbol -1
		if (isFinalPhase)
		{
			targetSymbols.Reverse();
			symbolImages[topestSymbolImagesIndex].sprite = targetSymbols[symbolsSetCount].sprite;
			symbolsSetCount++;
		}

		while (elapsed < duration)
		{
			// 計算當前速度（若有速度曲線，則根據曲線調整）
			float currentSpeed = speedCurve != null ? spinSpeed * speedCurve.Evaluate(elapsed / duration) : spinSpeed;

			for (int i = 0; i < symbolImages.Count; i++)
			{
				RectTransform rect = symbolImages[i].GetComponent<RectTransform>();
				rect.anchoredPosition += new Vector2(0, -Time.deltaTime * currentSpeed);

				if (rect.anchoredPosition.y < -totalHeight / 2)
				{
					topestSymbolImagesIndex = i;
					// rect.anchoredPosition += new Vector2(0, fullCycleDistance);
					rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y + fullCycleDistance);

					// 設置targetSymbol -2
					if (isFinalPhase && symbolsSetCount < targetSymbols.Count)
					{
						symbolImages[i].sprite = targetSymbols[symbolsSetCount].sprite;
						symbolsSetCount++;
					}
					else
					{
						symbolImages[i].sprite = config.symbols[UnityEngine.Random.Range(0, config.symbols.Length)].sprite;
					}
				}
			}
			if (isFinalPhase)
			{
				for (int i = 0; i < symbolImages.Count; i++)
				{
					RectTransform rect = symbolImages[i].GetComponent<RectTransform>();
					float targetY = Mathf.Round(rect.anchoredPosition.y / config.symbolHeight) * config.symbolHeight;
					rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, targetY);
				}
			}
			elapsed += Time.deltaTime;
			yield return null;
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

	#endregion
}