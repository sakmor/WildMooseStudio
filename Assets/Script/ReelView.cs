using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ReelView : MonoBehaviour
{
    [SerializeField] GameObject slotSymbolPrefab; // SlotSymbol.prefab
    [SerializeField] RectTransform container; // 滾輪容器
    List<Image> symbolImages; // 符號的 Image 列表
    SlotConfig config;
    int symbolsPerReel;
    float spaceY;
    float spinDuration;
    float spinSpeed;

    public void Initialize(SlotConfig slotConfig, int symbolsPerReel, float spaceY, float spinDuration, float spinSpeed)
    {
        config = slotConfig;
        this.symbolsPerReel = symbolsPerReel;
        this.spaceY = spaceY;
        this.spinDuration = spinDuration;
        this.spinSpeed = spinSpeed;

        symbolImages = new List<Image>();
        for (int k = 0; k < symbolsPerReel; k++)
        {
            GameObject symbolObj = Instantiate(slotSymbolPrefab, container);
            RectTransform symbolRt = symbolObj.GetComponent<RectTransform>();
            symbolRt.anchoredPosition = new Vector2(0, k * spaceY); // Y 軸位置
            Image symbolImage = symbolObj.GetComponent<Image>();
            symbolImage.sprite = config.symbols[Random.Range(0, config.symbols.Length)].sprite; // 初始隨機符號
            symbolImages.Add(symbolImage);
        }
    }

    public IEnumerator Spin(int targetSymbolIndex, float stopTime)
    {
        float elapsed = 0;
        while (elapsed < stopTime)
        {
            elapsed += Time.deltaTime;
            container.anchoredPosition -= new Vector2(0, spinSpeed * Time.deltaTime);
            if (container.anchoredPosition.y <= -spaceY * (symbolsPerReel - 1))
            {
                container.anchoredPosition += new Vector2(0, spaceY * symbolsPerReel); // 循環重置
                UpdateSymbols();
            }
            yield return null;
        }

        // 對齊最終符號
        float targetY = Mathf.Round(container.anchoredPosition.y / spaceY) * spaceY;
        container.anchoredPosition = new Vector2(0, targetY);
        symbolImages[0].sprite = config.symbols[targetSymbolIndex].sprite; // 設置最終符號
    }

    void UpdateSymbols()
    {
        foreach (Image img in symbolImages)
        {
            img.sprite = config.symbols[Random.Range(0, config.symbols.Length)].sprite;
        }
    }

    public void SetSymbol(int symbolIndex)
    {
        symbolImages[0].sprite = config.symbols[symbolIndex].sprite; // 設置 Y=0 的符號
    }
}