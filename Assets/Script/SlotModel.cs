using System;
using System.Collections.Generic;

public class SlotModel
{
	public List<Reel> Reels { get; private set; }
	public int Credits { get; private set; }
	public int Bet { get; private set; }
	public int WinAmount { get; private set; }
	public bool IsSpinning { get; private set; }
	public event Action OnSpinStarted;
	public event Action OnSpinFinished;
	public event Action OnCreditsChanged;
	public event Action OnWinAmountChanged;
	public event Action<int> OnWinDetected; // 修改：傳遞 payline 索引

	private readonly SlotConfig config;
	private readonly WinEvaluator winEvaluator;

	public SlotModel(SlotConfig config)
	{
		this.config = config;
		this.winEvaluator = new WinEvaluator(config);
		Credits = 1000;
		Bet = 10;
		WinAmount = 0;
		Reels = new List<Reel>();
		IsSpinning = false;
		for (int i = 0; i < config.reelCount; i++)
		{
			Reels.Add(new Reel(config.adjustedSymbolsPerReel));
		}
	}

	public void Spin()
	{
		IsSpinning = true;
		foreach (var reel in Reels)
		{
			reel.SetSymbols(GenerateRandomSymbols());
		}
		OnSpinStarted?.Invoke();
	}

	public void OnReelSpinStopped()
	{
		IsSpinning = false;
		CalculateWin();
		OnSpinFinished?.Invoke();
	}

	private List<SlotConfig.SymbolData> GenerateRandomSymbols()
	{
		List<SlotConfig.SymbolData> symbols = new List<SlotConfig.SymbolData>();
		for (int i = 0; i < config.adjustedSymbolsPerReel; i++)
		{
			symbols.Add(config.symbols[UnityEngine.Random.Range(0, config.symbols.Length)]);
		}
		return symbols;
	}

	private void CalculateWin()
	{
		int winAmount = 0;
		List<int> winningPaylineIndices;

		// 修改：接收獲勝的 payline 索引
		winEvaluator.CalculateWin(Reels, Bet, out winAmount, out winningPaylineIndices);

		WinAmount = winAmount;
		Credits += WinAmount - Bet;
		OnCreditsChanged?.Invoke();
		OnWinAmountChanged?.Invoke();

		// 修改：對每個獲勝的 payline 觸發事件
		foreach (int paylineIndex in winningPaylineIndices)
		{
			OnWinDetected?.Invoke(paylineIndex);
		}
	}
}