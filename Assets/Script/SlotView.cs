using UnityEngine;
using UnityEngine.UI;

public class SlotView : MonoBehaviour
{
    [SerializeField] Image[] slotImages; // 一維數組，動態大小
    [SerializeField] SlotConfig config;
    [SerializeField] Text scoreText;
    [SerializeField] Button spinButton;
    int rows;
    int columns;

    public void Initialize(SlotConfig slotConfig, int rows, int columns)
    {
        config = slotConfig;
        this.rows = rows;
        this.columns = columns;
        if (slotImages.Length != rows * columns)
        {
            Debug.LogError($"slotImages 數量 ({slotImages.Length}) 與格子尺寸 ({rows}x{columns}) 不匹配");
        }
    }

    public void UpdateGrid(int[,] grid)
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                int index = i * columns + j; // 映射到一維索引
                slotImages[index].sprite = config.symbols[grid[i, j]].sprite;
            }
        }
    }

    public void UpdateScore(float score)
    {
        scoreText.text = $"{score:F2}";
    }

    public void SetSpinButton(System.Action onClick)
    {
        spinButton.onClick.AddListener(() => onClick?.Invoke());
    }
}