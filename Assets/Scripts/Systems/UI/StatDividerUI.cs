using System.Collections.Generic;
using UnityEngine;

public class StatDividerUI : MonoBehaviour
{
    [Header("Налаштування")]
    [Tooltip("Значення стата, через яке ставиться рисочка (50 для HP, 25 для стаміни)")]
    [SerializeField] private float statInterval = 25f;
    
    [Header("Посилання")]
    [Tooltip("RectTransform самої смуги (для визначення її фізичної ширини)")]
    [SerializeField] private RectTransform barRect;
    [Tooltip("Префаб рисочки (Image з твоїм спрайтом)")]
    [SerializeField] private GameObject dividerPrefab;
    [Tooltip("Контейнер для рисочок (Pivot має бути X:0, Y:0.5)")]
    [SerializeField] private RectTransform dividerContainer;

    private List<GameObject> activeDividers = new List<GameObject>();
    private Queue<GameObject> dividerPool = new Queue<GameObject>();

    /// <summary>
    /// Викликай цей метод з PlayerStatsUI.cs ТІЛЬКИ коли змінюється МАКСИМАЛЬНЕ значення стата.
    /// </summary>
    public void UpdateDividers(float maxStat)
    {
        if (maxStat <= 0 || statInterval <= 0) return;

        int requiredDividers = Mathf.FloorToInt((maxStat - 0.1f) / statInterval);
        float barWidth = barRect.rect.width;

        foreach (var divider in activeDividers)
        {
            divider.SetActive(false);
            dividerPool.Enqueue(divider);
        }
        activeDividers.Clear();

        for (int i = 1; i <= requiredDividers; i++)
        {
            GameObject divObj = GetDivider();
            RectTransform divRect = divObj.GetComponent<RectTransform>();
            
            float normalizedPos = (i * statInterval) / maxStat;
            float xPos = normalizedPos * barWidth;

            divRect.anchoredPosition = new Vector2(xPos, 0f);
        }
    }

    private GameObject GetDivider()
    {
        GameObject divObj;
        if (dividerPool.Count > 0)
        {
            divObj = dividerPool.Dequeue();
        }
        else
        {
            divObj = Instantiate(dividerPrefab, dividerContainer);
        }
        
        divObj.SetActive(true);
        activeDividers.Add(divObj);
        return divObj;
    }
}