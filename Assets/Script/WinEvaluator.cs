using System.Collections.Generic;
using UnityEngine;

public class WinEvaluator
{
	private readonly SlotConfig config;

	public WinEvaluator(SlotConfig config)
	{
		this.config = config;
	}

	public void CalculateWin(List<Reel> reels, int bet, out int winAmount, out List<int> winningPaylineIndices)
	{
		winAmount = 0;
		winningPaylineIndices = new List<int>();

		// 檢查每條 payline
		for (int paylineIndex = 0; paylineIndex < config.paylines.Length; paylineIndex++)
		{
			var payline = config.paylines[paylineIndex];
			bool isWin = true;
			SlotConfig.SymbolData? firstSymbol = null;

			// 檢查 payline 中的每個符號
			for (int i = 0; i < payline.symbolIndices.Length; i++)
			{
				int symbolIndex = payline.symbolIndices[i];
				if (symbolIndex >= config.adjustedSymbolsPerReel || i >= reels.Count)
				{
					isWin = false;
					break;
				}

				var symbol = reels[i].VisibleSymbols[symbolIndex];
				if (i == 0)
				{
					firstSymbol = symbol;
				}
				else if (firstSymbol.HasValue && symbol.sprite != firstSymbol.Value.sprite)
				{
					isWin = false;
					break;
				}
			}

			// 如果連線成功，計算贏得金額並記錄 payline 索引
			if (isWin && firstSymbol.HasValue)
			{
				winAmount += (int)(bet * firstSymbol.Value.payout);
				winningPaylineIndices.Add(paylineIndex);
			}
		}
	}
}