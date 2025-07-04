using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ReelView : MonoBehaviour
{
    private List<Image> symbolImages = new List<Image>();
    private bool isSpinning;
    private float spinDuration = 2f;
    private float symbolHeight = 100f;

    public void Initialize(int symbolsPerReel, GameObject symbolPrefab, SlotConfig.SymbolData[] symbols)
    {
        for (int i = 0; i < symbolsPerReel; i++)
        {
            GameObject symbolObj = Instantiate(symbolPrefab, transform);
            symbolImages.Add(symbolObj.GetComponent<Image>());
            symbolObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -i * symbolHeight);
        }
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
        while (elapsed < spinDuration)
        {
            foreach (var image in symbolImages)
            {
                RectTransform rect = image.GetComponent<RectTransform>();
                rect.anchoredPosition += new Vector2(0, -Time.deltaTime * 1000);
                if (rect.anchoredPosition.y < -symbolHeight * (symbolImages.Count - 1))
                {
                    rect.anchoredPosition += new Vector2(0, symbolHeight * symbolImages.Count);
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < symbolImages.Count; i++)
        {
            symbolImages[i].sprite = targetSymbols[i].sprite;
            symbolImages[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -i * symbolHeight);
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