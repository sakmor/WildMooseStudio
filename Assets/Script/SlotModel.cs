using System;
using System.Collections.Generic;
using UnityEngine;

public class SlotModel
{
	public List<ReelModel> Reels { get; private set; }
	public int Credits { get; private set; }
	public int Bet { get; private set; }
	public int WinAmount { get; private set; }
	public bool IsSpinning { get; private set; }
	public event Action OnSpinStarted;
	public event Action OnSpinFinished;
	public event Action OnCreditsChanged;
	public event Action OnWinAmountChanged;

	private SlotConfig config;
	private readonly System.Random random;

	public SlotModel()
	{
		Credits = 1000;
		Bet = 10;
		WinAmount = 0;
		IsSpinning = false;
		random = new System.Random();
	}

	public void Initialize(SlotConfig config)
	{
		this.config = config;
		Reels = new List<ReelModel>();
		for (int i = 0; i < config.reelCount; i++)
		{
			Reels.Add(new ReelModel(config.symbols, config.symbolsPerReel, random));
		}
	}

	public void SetBet(int bet)
	{
		if (bet > 0 && bet <= Credits)
		{
			Bet = bet;
			OnCreditsChanged?.Invoke();
		}
	}

	public void Spin()
	{
		if (IsSpinning || Credits < Bet || config == null) return;

		IsSpinning = true;
		Credits -= Bet;
		WinAmount = 0;
		OnSpinStarted?.Invoke();
		OnCreditsChanged?.Invoke();

		foreach (var reel in Reels)
		{
			reel.Spin();
		}

		CheckWin();
		IsSpinning = false;
		OnSpinFinished?.Invoke();
	}

	private void CheckWin()
	{
		// List<SlotConfig.SymbolData> middleRow = new List<SlotConfig.SymbolData>();
		// foreach (var reel in Reels)
		// {
		// 	middleRow.Add(reel.VisibleSymbols[1]);
		// }

		// if (middleRow[0].sprite == middleRow[1].sprite && middleRow[1].sprite == middleRow[2].sprite)
		// {
		// 	WinAmount = Mathf.FloorToInt(Bet * middleRow[0].payout);
		// 	Credits += WinAmount;
		// 	OnWinAmountChanged?.Invoke();
		// 	OnCreditsChanged?.Invoke();
		// }
	}
}

