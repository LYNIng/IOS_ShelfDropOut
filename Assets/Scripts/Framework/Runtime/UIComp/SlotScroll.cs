using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SlotScroll : MonoBehaviour
{
    [Header("Settings")]
    public float scrollSpeed = 500f;          // 滚动速度
    public float spacing = 10f;              // 元素间隔
    public bool loopIndividually = true;     // 每个元素单独循环
    public int stopIndex = 0;               // 停止的索引

    [Header("References")]
    public RectTransform content;            // 内容的RectTransform
    public List<RectTransform> slotItems;    // 所有槽位元素

    private bool isScrolling = false;
    private float[] targetPositions;
    private float contentHeight;
    private int itemCount;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        itemCount = slotItems.Count;
        if (itemCount == 0) return;

        // 计算内容高度
        contentHeight = itemCount * (slotItems[0].rect.height + spacing) - spacing;
        content.sizeDelta = new Vector2(content.sizeDelta.x, contentHeight);

        // 初始化位置
        for (int i = 0; i < itemCount; i++)
        {
            float yPos = -i * (slotItems[i].rect.height + spacing);
            slotItems[i].anchoredPosition = new Vector2(0, yPos);
        }

        // 计算目标位置
        CalculateTargetPositions();
    }

    void CalculateTargetPositions()
    {
        targetPositions = new float[itemCount];
        float itemHeight = slotItems[0].rect.height;

        for (int i = 0; i < itemCount; i++)
        {
            // 计算停止位置
            float stopPosition = stopIndex * (itemHeight + spacing);
            targetPositions[i] = -i * (itemHeight + spacing) + stopPosition;

            // 如果启用单独循环，调整位置使其在可视范围内
            if (loopIndividually)
            {
                while (targetPositions[i] > 0)
                    targetPositions[i] -= contentHeight;
                while (targetPositions[i] < -contentHeight)
                    targetPositions[i] += contentHeight;
            }
        }
    }

    public void StartScroll()
    {
        if (!isScrolling)
        {
            isScrolling = true;
            StartCoroutine(ScrollToTarget());
        }
    }

    public void StopAt(int index)
    {
        stopIndex = index;
        CalculateTargetPositions();
    }

    IEnumerator ScrollToTarget()
    {
        float[] startPositions = new float[itemCount];
        for (int i = 0; i < itemCount; i++)
        {
            startPositions[i] = slotItems[i].anchoredPosition.y;
        }

        float progress = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime * scrollSpeed / contentHeight;
            progress = Mathf.Clamp01(progress);

            for (int i = 0; i < itemCount; i++)
            {
                float newY = Mathf.Lerp(startPositions[i], targetPositions[i], progress);
                slotItems[i].anchoredPosition = new Vector2(0, newY);

                // 如果启用单独循环，处理循环逻辑
                if (loopIndividually)
                {
                    if (newY > 0)
                    {
                        newY -= contentHeight;
                        startPositions[i] -= contentHeight;
                        slotItems[i].anchoredPosition = new Vector2(0, newY);
                    }
                    else if (newY < -contentHeight)
                    {
                        newY += contentHeight;
                        startPositions[i] += contentHeight;
                        slotItems[i].anchoredPosition = new Vector2(0, newY);
                    }
                }
            }

            yield return null;
        }

        // 确保最终位置准确
        for (int i = 0; i < itemCount; i++)
        {
            slotItems[i].anchoredPosition = new Vector2(0, targetPositions[i]);
        }

        isScrolling = false;
    }
}