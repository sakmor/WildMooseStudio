using UnityEngine;
using System;

public class SlotModel
{
    public int[,] Grid { get; private set; }
    public float Score { get; private set; }
    public event Action OnGridUpdated;
    public event Action<float> OnScoreUpdated;
    SlotConfig config;
    int rows;
    int columns;

    public void Initialize(SlotConfig slotConfig, int rows, int columns)
    {
        config = slotConfig;
        this.rows = rows;
        this.columns = columns;
        Grid = new int[rows, columns];
    }

    public void Spin()
    {
        float[] probabilities = new float[config.symbols.Length];
        float total = 0;
        for (int i = 0; i < config.symbols.Length; i++)
        {
            probabilities[i] = config.symbols[i].probability;
            total += probabilities[i];
        }
        for (int i = 0; i < probabilities.Length; i++)
        {
            probabilities[i] /= total; // 正規化機率
        }

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Grid[i, j] = GetRandomSymbol(probabilities);
            }
        }
        CheckWin();
        OnGridUpdated?.Invoke();
    }

    int GetRandomSymbol(float[] probabilities)
    {
        float randomValue = UnityEngine.Random.value;
        float cumulative = 0;
        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulative += probabilities[i];
            if (randomValue <= cumulative)
            {
                return i;
            }
        }
        return probabilities.Length - 1;
    }

    void CheckWin()
    {
        float newScore = 0;
        for (int i = 0; i < rows; i++)
        {
            bool isWin = true;
            for (int j = 1; j < columns; j++)
            {
                if (Grid[i, j] != Grid[i, 0])
                {
                    isWin = false;
                    break;
                }
            }
            if (isWin)
            {
                newScore += config.symbols[Grid[i, 0]].payout;
            }
        }
        Score += newScore;
        OnScoreUpdated?.Invoke(Score);
    }
}