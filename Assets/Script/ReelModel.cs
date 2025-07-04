using System;
using System.Collections.Generic;
using UnityEngine;

public class ReelModel
{
	public List<SlotConfig.SymbolData> VisibleSymbols { get; private set; }
	private readonly SlotConfig.SymbolData[] symbols;
	private readonly System.Random random;
	private readonly int visibleCount;

	public ReelModel(SlotConfig.SymbolData[] symbols, int visibleCount, System.Random random)
	{
		this.symbols = symbols;
		this.visibleCount = visibleCount;
		this.random = random;
		VisibleSymbols = new List<SlotConfig.SymbolData>();
		for (int i = 0; i < visibleCount; i++)
		{
			VisibleSymbols.Add(GetRandomSymbol());
		}
	}

	public void Spin()
	{
		VisibleSymbols.Clear();
		for (int i = 0; i < visibleCount; i++)
		{
			VisibleSymbols.Add(GetRandomSymbol());
		}
	}

	private SlotConfig.SymbolData GetRandomSymbol()
	{
		float totalProbability = 0;
		foreach (var symbol in symbols)
		{
			totalProbability += symbol.probability;
		}

		float randomValue = (float)random.NextDouble() * totalProbability;
		float cumulative = 0;

		foreach (var symbol in symbols)
		{
			cumulative += symbol.probability;
			if (randomValue <= cumulative)
			{
				return symbol;
			}
		}
		return symbols[symbols.Length - 1];
	}
}