
using System.Collections.Generic;
public class Reel
{
	public List<SlotConfig.SymbolData> VisibleSymbols { get; private set; }

	public Reel(int symbolsPerReel)
	{
		VisibleSymbols = new List<SlotConfig.SymbolData>(symbolsPerReel);
	}

	public void SetSymbols(List<SlotConfig.SymbolData> symbols)
	{
		VisibleSymbols = symbols;
	}
}