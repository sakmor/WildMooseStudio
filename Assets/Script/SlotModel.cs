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
	public event Action<int> OnWinDetected;

	private SlotConfig config;

	public SlotModel(SlotConfig config)
	{
		this.config = config;
		Credits = 1000;
		Bet = 10;
		WinAmount = 0;
		Reels = new List<Reel>();
		IsSpinning = false; 
		for (int i = 0; i < config.reelCount; i++)
		{
			Reels.Add(new Reel(config.symbolsPerReel));
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
		for (int i = 0; i < config.symbolsPerReel; i++)
		{
			symbols.Add(config.symbols[UnityEngine.Random.Range(0, config.symbols.Length)]);
		}
		return symbols;
	}

	private void CalculateWin()
	{
		WinAmount = 0;
		for (int i = 0; i < Reels.Count; i++)
		{
			var symbols = Reels[i].VisibleSymbols;
			if (symbols.Count == config.symbolsPerReel && symbols[0].sprite == symbols[1].sprite && symbols[1].sprite == symbols[2].sprite)
			{
				WinAmount += (int)(Bet * symbols[0].payout);
				OnWinDetected?.Invoke(i);
			}
		}
		Credits += WinAmount - Bet;
		OnCreditsChanged?.Invoke();
		OnWinAmountChanged?.Invoke();
	}
}