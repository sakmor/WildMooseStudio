using System.Collections.Generic;
using UnityEngine;

public class WinEvaluator
{
    private readonly SlotConfig config;

    public WinEvaluator(SlotConfig config)
    {
        this.config = config;
    }

    public void CalculateWin(List<Reel> reels, int bet, out int winAmount, out List<int> winningReels)
    {
        winAmount = 0;
        winningReels = new List<int>();

        // 檢查每條 payline
        foreach (var payline in config.paylines)
        {
            bool isWin = true;
            SlotConfig.SymbolData? firstSymbol = null; // 修改：使用可為 null 的結構類型

            // 檢查 payline 中的每個符號
            for (int i = 0; i < payline.symbolIndices.Length; i++)
            {
                int symbolIndex = payline.symbolIndices[i];
                if (symbolIndex >= config.symbolsPerReel || i >= reels.Count)
                {
                    isWin = false;
                    break;
                }

                var symbol = reels[i].VisibleSymbols[symbolIndex];
                if (i == 0)
                {
                    firstSymbol = symbol; // 修改：直接賦值給 firstSymbol
                }
                else if (firstSymbol.HasValue && symbol.sprite != firstSymbol.Value.sprite)
                {
                    isWin = false;
                    break;
                }
            }

            // 如果連線成功，計算贏得金額並記錄獲勝滾輪
            if (isWin && firstSymbol.HasValue)
            {
                winAmount += (int)(bet * firstSymbol.Value.payout);
                for (int i = 0; i < reels.Count; i++)
                {
                    if (!winningReels.Contains(i))
                    {
                        winningReels.Add(i);
                    }
                }
            }
        }
    }
}