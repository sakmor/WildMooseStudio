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
	public event System.Action OnSpinStopped;
	private Coroutine winAnimationCoroutine;

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

		// 停止所有正在進行的 WinAnimation 並恢復透明度
		if (winAnimationCoroutine != null)
		{
			StopCoroutine(winAnimationCoroutine);
			winAnimationCoroutine = null;
			foreach (var image in symbolImages)
			{
				image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
			}
		}

		StartCoroutine(SpinAnimation(targetSymbols));
		string _targetSymbols = "";
		targetSymbols.ForEach(e => _targetSymbols += "," + e.name);
		Debug.Log("Target：" + _targetSymbols);
	}

	public void StopSpin()
	{
		isSpinning = false;
		OnSpinStopped?.Invoke();
	}

	public void PlayWinAnimation(List<int> symbolIndices)
	{
		// 修改：傳遞多個符號索引
		if (winAnimationCoroutine != null)
		{
			StopCoroutine(winAnimationCoroutine);
		}
		winAnimationCoroutine = StartCoroutine(WinAnimation(symbolIndices));
	}

	#endregion

	#region 動畫邏輯

	private IEnumerator SpinAnimation(List<SlotConfig.SymbolData> targetSymbols)
	{
		yield return StartCoroutine(UpdateSymbolPositions(fullCycleDistance / spinSpeed, false, null, config.beginAccelerationCurve));
		yield return StartCoroutine(UpdateSymbolPositions(config.spinDuration, false, null, null));
		yield return StartCoroutine(UpdateSymbolPositions(fullCycleDistance / spinSpeed, true, targetSymbols, config.finalDecelerationCurve));

		isSpinning = false;
		OnSpinStopped?.Invoke();
	}
	public List<SlotConfig.SymbolData> resultSymbols = new List<SlotConfig.SymbolData>();
	private IEnumerator UpdateSymbolPositions(float duration, bool isFinalPhase, List<SlotConfig.SymbolData> targetSymbols, AnimationCurve speedCurve = null)
	{
		float elapsed = 0;
		int symbolsSetCount = 0;
	
		List<SlotConfig.SymbolData> _targetSymbols = null;
		if (isFinalPhase)
		{
			resultSymbols.Clear();
			_targetSymbols = targetSymbols.AsEnumerable().Reverse().ToList();
			symbolImages[topestSymbolImagesIndex].sprite = _targetSymbols[symbolsSetCount].sprite;
			resultSymbols.Insert(0,_targetSymbols[symbolsSetCount]);
			symbolsSetCount++;
		}

		while (elapsed < duration)
		{
			float currentSpeed = speedCurve != null ? spinSpeed * speedCurve.Evaluate(elapsed / duration) : spinSpeed;

			for (int i = 0; i < symbolImages.Count; i++)
			{
				RectTransform rect = symbolImages[i].GetComponent<RectTransform>();
				rect.anchoredPosition += new Vector2(0, -Time.deltaTime * currentSpeed);

				if (rect.anchoredPosition.y < -totalHeight / 2)
				{
					topestSymbolImagesIndex = i;
					rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y + fullCycleDistance);

					if (isFinalPhase && symbolsSetCount < targetSymbols.Count)
					{
						symbolImages[i].sprite = _targetSymbols[symbolsSetCount].sprite;
						resultSymbols.Insert(0, _targetSymbols[symbolsSetCount]);
						symbolsSetCount++;
					}
					else
					{
						symbolImages[i].sprite = config.symbols[UnityEngine.Random.Range(0, config.symbols.Length)].sprite;
					}
				}
			}

			elapsed += Time.deltaTime;
			yield return null;
		}

		if (isFinalPhase)
		{
			for (int i = 0; i < symbolImages.Count; i++)
			{
				RectTransform rect = symbolImages[i].GetComponent<RectTransform>();
				float targetY = Mathf.Ceil(rect.anchoredPosition.y / config.symbolHeight) * config.symbolHeight;
				rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, targetY);
			}

			bool isMatch = resultSymbols != null && targetSymbols != null && resultSymbols.SequenceEqual(targetSymbols);
			string resultSymbolNames = string.Join(",", resultSymbols.Select(e => e.name));
			string targetSymbolNames = string.Join(",", targetSymbols.Select(e => e.name));

			if (isMatch)
			{
				Debug.Log($"Result matches target: {resultSymbolNames}");
			}
			else
			{
				Debug.LogError($"Result does not match target! Result: {resultSymbolNames}, Target: {targetSymbolNames}");
			}
		}
	}

	private IEnumerator WinAnimation(List<int> symbolIndices)
	{
		// 修改：交替顯示每組符號的淡入淡出動畫
		if (symbolIndices == null || symbolIndices.Count == 0)
		{
			yield break;
		}

		// 恢復所有符號的透明度
		foreach (var image in symbolImages)
		{
			image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
		}

		while (true)
		{
			// 對每個符號索引執行淡入淡出
			foreach (int symbolIndex in symbolIndices)
			{
				if (symbolIndex >= 0 && symbolIndex < symbolImages.Count)
				{
					Image targetImage = symbolImages[symbolIndex];
					Color originalColor = targetImage.color;
					float duration = 0.2f;

					// 淡出
					float elapsed = 0f;
					while (elapsed < duration)
					{
						elapsed += Time.deltaTime;
						targetImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f - elapsed / duration);
						yield return null;
					}

					// 淡入
					elapsed = 0f;
					while (elapsed < duration)
					{
						elapsed += Time.deltaTime;
						targetImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, elapsed / duration);
						yield return null;
					}

					// 等待一小段時間以區分不同 payline 的動畫
					yield return new WaitForSeconds(0.1f);
				}
			}
		}
	}

	#endregion
}