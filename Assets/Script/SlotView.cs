using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotView : MonoBehaviour
{
	[SerializeField] SlotConfig config;
	[SerializeField] Transform reelContainer;
	[SerializeField] GameObject symbolPrefab;
	[SerializeField] Text creditsText;
	[SerializeField] Text betText;
	[SerializeField] Text winText;

	private List<ReelView> reelViews = new List<ReelView>();
	private SlotModel model;

	public void Initialize(SlotModel model)
	{
		this.model = model;
		model.OnSpinStarted += StartSpinAnimation;
		model.OnCreditsChanged += UpdateCreditsUI;
		model.OnWinAmountChanged += UpdateWinUI;
		model.OnWinDetected += PlayWinAnimation;
		SetupReels();
		UpdateCreditsUI();
		UpdateBetUI();
		UpdateWinUI();
	}

	private void SetupReels()
	{
		float totalWidth = (config.reelCount - 1) * config.reelSpacing;
		float startX = -totalWidth / 2;
		for (int i = 0; i < config.reelCount; i++)
		{
			GameObject reelObj = new GameObject($"Reel_{i}");
			reelObj.AddComponent<RectTransform>();
			reelObj.transform.SetParent(reelContainer, false);
			ReelView reelView = reelObj.AddComponent<ReelView>();
			reelView.Initialize(config, symbolPrefab);
			int index=i;
			reelView.GetComponent<ReelAnimation>().OnSpinStopped += () => OnReelSpinStopped(index);
			reelViews.Add(reelView);
			reelObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX + i * config.reelSpacing, 0);
		}
	}

	private void StartSpinAnimation()
	{
		for (int i = 0; i < reelViews.Count; i++)
		{
			reelViews[i].StartSpin(model.Reels[i].VisibleSymbols);
		}
	}



	private void OnReelSpinStopped(int reelIndex)
	{
		if (reelIndex == reelViews.Count - 1) 
		{
			model.OnReelSpinStopped();
		}
	}

	private void PlayWinAnimation(int reelIndex)
	{
		reelViews[reelIndex].PlayWinAnimation();
	}

	private void UpdateCreditsUI()
	{
		creditsText.text = $"Credits: {model.Credits}";
	}

	private void UpdateBetUI()
	{
		betText.text = $"Bet: {model.Bet}";
	}

	private void UpdateWinUI()
	{
		winText.text = $"Win: {model.WinAmount}";
	}
}