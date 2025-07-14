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
	private List<int> pendingPaylineIndices = new List<int>(); // 新增：儲存待處理的 payline 索引

	public void Initialize(SlotModel model)
	{
		this.model = model;
		model.OnSpinStarted += StartSpinAnimation;
		model.OnCreditsChanged += UpdateCreditsUI;
		model.OnWinAmountChanged += UpdateWinUI;
		model.OnWinDetected += AddPendingWinAnimation; // 修改：收集 payline 索引
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
			int index = i;
			reelView.GetComponent<ReelAnimation>().OnSpinStopped += () => OnReelSpinStopped(index);
			reelViews.Add(reelView);
			reelObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(startX + i * config.reelSpacing, 0);
		}
	}

	private void StartSpinAnimation()
	{
		// 新增：清空待處理的 payline 索引
		pendingPaylineIndices.Clear();
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
			// 新增：滾輪停止後播放所有待處理的動畫
			if (pendingPaylineIndices.Count > 0)
			{
				PlayWinAnimations();
			}
		}
	}

	private void AddPendingWinAnimation(int paylineIndex)
	{
		// 新增：收集獲勝的 payline 索引
		if (paylineIndex >= 0 && paylineIndex < config.paylines.Length)
		{
			pendingPaylineIndices.Add(paylineIndex);
		}
	}

	private void PlayWinAnimations()
	{
		// 修改：為每個滾輪傳遞所有獲勝 payline 的符號索引
		for (int i = 0; i < reelViews.Count; i++)
		{
			List<int> symbolIndices = new List<int>();
			foreach (int paylineIndex in pendingPaylineIndices)
			{
				if (paylineIndex < config.paylines.Length)
				{
					symbolIndices.Add(config.paylines[paylineIndex].symbolIndices[i]);
				}
			}
			reelViews[i].PlayWinAnimation(symbolIndices);
		}
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