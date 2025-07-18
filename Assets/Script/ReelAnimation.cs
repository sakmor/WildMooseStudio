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

	private const float MinDeltaMove = 0.01f; // 🚨 每次至少移動這麼多距離

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
		string _targetSymbols = string.Join(",", targetSymbols.Select(e => e.name));
		Debug.Log("Target：" + _targetSymbols);
	}

	public void StopSpin()
	{
		isSpinning = false;
		OnSpinStopped?.Invoke();
	}

	public void PlayWinAnimation(List<int> symbolIndices)
	{
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
		float beginAccelDistance = fullCycleDistance;
		float constantDistance = spinSpeed * config.spinDuration;
		float finalDecelDistance = fullCycleDistance;

		yield return StartCoroutine(UpdateSymbolPositionsByDistance(beginAccelDistance, false, null, config.beginAccelerationCurve));
		yield return StartCoroutine(UpdateSymbolPositionsByDistance(constantDistance, false, null, null));
		yield return StartCoroutine(UpdateSymbolPositionsByDistance(finalDecelDistance, true, targetSymbols, config.finalDecelerationCurve));

		isSpinning = false;
		OnSpinStopped?.Invoke();
	}

	public List<SlotConfig.SymbolData> resultSymbols = new List<SlotConfig.SymbolData>();

	private IEnumerator UpdateSymbolPositionsByDistance(float targetDistance, bool isFinalPhase, List<SlotConfig.SymbolData> targetSymbols, AnimationCurve speedCurve = null)
	{
		float totalMovedDistance = 0f;
		int symbolsSetCount = 0;

		List<SlotConfig.SymbolData> _targetSymbols = null;
		if (isFinalPhase)
		{
			resultSymbols.Clear();
			_targetSymbols = targetSymbols.AsEnumerable().Reverse().ToList();
			symbolImages[topestSymbolImagesIndex].sprite = _targetSymbols[symbolsSetCount].sprite;
			resultSymbols.Insert(0, _targetSymbols[symbolsSetCount]);
			symbolsSetCount++;
		}

		while (totalMovedDistance < targetDistance)
		{
			float progress = totalMovedDistance / targetDistance;
			float currentSpeed = speedCurve != null ? spinSpeed * speedCurve.Evaluate(progress) : spinSpeed;
			float deltaMove = Mathf.Max(currentSpeed * Time.deltaTime, MinDeltaMove); // ⬅️ 加上最小值保護

			for (int i = 0; i < symbolImages.Count; i++)
			{
				RectTransform rect = symbolImages[i].GetComponent<RectTransform>();
				rect.anchoredPosition += new Vector2(0, -deltaMove);

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

			totalMovedDistance += deltaMove;
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
		if (symbolIndices == null || symbolIndices.Count == 0)
			yield break;

		foreach (var image in symbolImages)
		{
			image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
		}

		while (true)
		{
			foreach (int symbolIndex in symbolIndices)
			{
				if (symbolIndex >= 0 && symbolIndex < symbolImages.Count)
				{
					Image targetImage = symbolImages[symbolIndex];
					Color originalColor = targetImage.color;
					float duration = 0.2f;

					float elapsed = 0f;
					while (elapsed < duration)
					{
						elapsed += Time.deltaTime;
						targetImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f - elapsed / duration);
						yield return null;
					}

					elapsed = 0f;
					while (elapsed < duration)
					{
						elapsed += Time.deltaTime;
						targetImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, elapsed / duration);
						yield return null;
					}

					yield return new WaitForSeconds(0.1f);
				}
			}
		}
	}

	#endregion
}
