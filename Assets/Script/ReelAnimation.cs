using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReelAnimation : MonoBehaviour
{
    private List<Image> symbolImages;
    private SlotConfig config;
    private bool isSpinning;

    public void Initialize(SlotConfig config, List<Image> symbolImages)
    {
        this.config = config;
        this.symbolImages = symbolImages;
    }

    public void StartSpin(List<SlotConfig.SymbolData> targetSymbols)
    {
        if (isSpinning) return;
        isSpinning = true;
        StartCoroutine(SpinAnimation(targetSymbols));
    }

    public void StopSpin(List<SlotConfig.SymbolData> targetSymbols)
    {
        isSpinning = false;
    }

    private IEnumerator SpinAnimation(List<SlotConfig.SymbolData> targetSymbols)
    {
        float elapsed = 0;
        float totalHeight = (config.symbolsPerReel - 1) * config.symbolHeight;
        while (elapsed < config.spinDuration)
        {
            foreach (var image in symbolImages)
            {
                RectTransform rect = image.GetComponent<RectTransform>();
                rect.anchoredPosition += new Vector2(0, -Time.deltaTime * config.spinSpeed);
                if (rect.anchoredPosition.y < -totalHeight / 2)
                {
                    rect.anchoredPosition += new Vector2(0, totalHeight + config.symbolHeight);
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        float startY = (config.symbolsPerReel - 1) * config.symbolHeight / 2;
        for (int i = 0; i < symbolImages.Count; i++)
        {
            symbolImages[i].sprite = targetSymbols[i].sprite;
            symbolImages[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, startY - i * config.symbolHeight);
        }

        if (targetSymbols[0].sprite == targetSymbols[1].sprite && targetSymbols[1].sprite == targetSymbols[2].sprite)
        {
            StartCoroutine(WinAnimation());
        }
    }

    private IEnumerator WinAnimation()
    {
        for (int i = 0; i < 6; i++)
        {
            foreach (var image in symbolImages)
            {
                image.enabled = !image.enabled;
            }
            yield return new WaitForSeconds(0.2f);
        }
        foreach (var image in symbolImages)
        {
            image.enabled = true;
        }
    }
}