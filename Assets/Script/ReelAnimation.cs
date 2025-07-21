using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
	private List<SlotConfig.SymbolData> targetSymbols; // 緩存 targetSymbols
	public event System.Action OnSpinStopped;
	private Coroutine winAnimationCoroutine;
	public List<float> SymbolPostions = new();

	private const float MinDeltaMove = 0.01f; // 每次至少移動這麼多距離

	#region 初始化

	public void Initialize(SlotConfig config, List<Image> symbolImages)
	{
		this.config = config;
		this.symbolImages = symbolImages;
		symbolImages.ForEach(symbol => this.SymbolPostions.Add(symbol.transform.localPosition.y));
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

	public void StartSpin(List<SlotConfig.SymbolData> symbols)
	{
		if (isSpinning) return;
		isSpinning = true;
		targetSymbols = symbols; // 緩存 targetSymbols

		if (winAnimationCoroutine != null)
		{
			StopCoroutine(winAnimationCoroutine);
			winAnimationCoroutine = null;
			foreach (var image in symbolImages)
			{
				image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
			}
		}

		StartCoroutine(SpinAnimation());
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

	private IEnumerator SpinAnimation()
	{
		float beginAccelDistance = fullCycleDistance;
		float constantDistance = spinSpeed * config.spinDuration;
		float finalDecelDistance = fullCycleDistance;

		yield return StartCoroutine(UpdateSymbolPositionsByDistance(beginAccelDistance, false, config.beginAccelerationCurve));
		yield return StartCoroutine(UpdateSymbolPositionsByDistance(constantDistance, false, null));
		yield return StartCoroutine(UpdateSymbolPositionsByDistance(finalDecelDistance, true, config.finalDecelerationCurve));

		isSpinning = false;
		OnSpinStopped?.Invoke();
	}


	private IEnumerator UpdateSymbolPositionsByDistance(float targetDistance, bool isFinalPhase, AnimationCurve speedCurve = null)
	{
		float totalMovedDistance = 0f;
		int symbolsSetCount = 0;

		List<SlotConfig.SymbolData> _targetSymbols = null;
		if (isFinalPhase)
		{
			_targetSymbols = targetSymbols.AsEnumerable().Reverse().ToList();
		}

		while (totalMovedDistance < targetDistance)
		{
			float progress = totalMovedDistance / targetDistance;
			float currentSpeed = speedCurve != null ? spinSpeed * speedCurve.Evaluate(progress) : spinSpeed;
			float deltaMove = Mathf.Max(currentSpeed * Time.deltaTime, MinDeltaMove);

			for (int i = 0; i < symbolImages.Count; i++)
			{
				RectTransform rect = symbolImages[i].GetComponent<RectTransform>();
				rect.anchoredPosition += new Vector2(0, -deltaMove);

				if (rect.anchoredPosition.y < -totalHeight / 2)
				{
					Vector3 pos = symbolImages[i].transform.localPosition;
					pos.y = SymbolPostions[0];
					symbolImages[i].transform.localPosition = pos;

					if (isFinalPhase && symbolsSetCount < targetSymbols.Count)
					{
						symbolImages[i].sprite = _targetSymbols[symbolsSetCount].sprite;
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

		if (isFinalPhase & config.isUseFinalSnap) SnapSymbolsToGridAndCheck();
	}

	private void SnapSymbolsToGridAndCheck()
	{
		for (int i = 0; i < symbolImages.Count; i++)
		{
			Image symbolImage = symbolImages[i];
			Vector3 pos = symbolImage.transform.localPosition;
			pos.y = SymbolPostions[i];
			symbolImage.transform.localPosition = pos;
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