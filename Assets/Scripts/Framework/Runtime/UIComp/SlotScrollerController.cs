using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotScrollController : MonoBehaviour
{
    [Header("Settings")]
    public float scrollSpeed = 500f;          // 滚动速度
    public float decelerationRate = 0.95f;    // 减速速率
    public float stopThreshold = 0.1f;        // 停止阈值
    public int targetIndex = 0;               // 目标停止索引

    [Header("References")]
    public RectTransform content;             // 滚动内容
    public RectTransform[] slots;            // 所有slot的数组
    public float slotHeight;                  // 每个slot的高度

    private bool isScrolling = false;
    private float currentSpeed;
    private float contentStartY;
    private int currentCenterIndex;

    private void Start()
    {
        if (content == null || slots == null || slots.Length == 0)
        {
            Debug.LogError("SlotScrollController: Missing references!");
            return;
        }

        slotHeight = slots[0].rect.height;
        contentStartY = content.anchoredPosition.y;

        // 初始化位置
        ResetContentPosition();
    }

    // 重置内容位置
    private void ResetContentPosition()
    {
        Vector2 pos = content.anchoredPosition;
        pos.y = contentStartY;
        content.anchoredPosition = pos;
    }

    // 开始滚动
    public void StartScroll()
    {
        if (isScrolling) return;

        ResetContentPosition();
        currentSpeed = scrollSpeed;
        isScrolling = true;
        StartCoroutine(ScrollRoutine());
    }

    // 停止在指定索引
    public void StopAt(int index)
    {
        if (index < 0 || index >= slots.Length)
        {
            Debug.LogWarning($"Invalid target index: {index}");
            return;
        }

        targetIndex = index;
        StartCoroutine(StopAtTargetRoutine());
    }

    // 滚动协程
    private IEnumerator ScrollRoutine()
    {
        while (isScrolling)
        {
            // 移动内容
            Vector2 pos = content.anchoredPosition;
            pos.y += currentSpeed * Time.deltaTime;
            content.anchoredPosition = pos;

            // 减速
            currentSpeed *= decelerationRate;

            // 检查是否停止
            if (Mathf.Abs(currentSpeed) < stopThreshold)
            {
                isScrolling = false;
                SnapToNearestSlot();
            }

            // 循环处理
            HandleLooping();

            yield return null;
        }
    }

    // 停止到目标的协程
    private IEnumerator StopAtTargetRoutine()
    {
        // 先确保在滚动状态
        if (!isScrolling)
        {
            currentSpeed = scrollSpeed;
            isScrolling = true;
        }

        // 计算目标位置
        float targetY = contentStartY - (targetIndex * slotHeight);

        // 减速滚动到目标位置
        while (isScrolling)
        {
            Vector2 pos = content.anchoredPosition;
            float distance = targetY - pos.y;

            // 调整速度
            currentSpeed = distance * 5f; // 调整这个系数可以改变减速曲线

            // 移动内容
            pos.y += currentSpeed * Time.deltaTime;
            content.anchoredPosition = pos;

            // 检查是否到达目标
            if (Mathf.Abs(distance) < stopThreshold)
            {
                isScrolling = false;
                pos.y = targetY;
                content.anchoredPosition = pos;
                currentCenterIndex = targetIndex;
                Debug.Log($"Stopped at index: {targetIndex}");
            }

            // 循环处理
            HandleLooping();

            yield return null;
        }
    }

    // 对齐到最近的slot
    private void SnapToNearestSlot()
    {
        Vector2 pos = content.anchoredPosition;
        float currentY = pos.y;

        // 计算最近的slot索引
        int nearestIndex = Mathf.RoundToInt((contentStartY - currentY) / slotHeight);
        nearestIndex = Mathf.Clamp(nearestIndex, 0, slots.Length - 1);

        // 计算目标位置
        float targetY = contentStartY - (nearestIndex * slotHeight);

        // 平滑移动到目标位置
        StartCoroutine(SmoothMoveTo(targetY));
        currentCenterIndex = nearestIndex;
    }

    // 平滑移动到目标位置
    private IEnumerator SmoothMoveTo(float targetY)
    {
        Vector2 pos = content.anchoredPosition;
        float startY = pos.y;
        float duration = 0.3f; // 移动持续时间
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // 使用缓动函数

            pos.y = Mathf.Lerp(startY, targetY, t);
            content.anchoredPosition = pos;

            yield return null;
        }

        pos.y = targetY;
        content.anchoredPosition = pos;
    }

    // 处理循环逻辑
    private void HandleLooping()
    {
        Vector2 pos = content.anchoredPosition;

        // 向上循环
        if (pos.y > contentStartY + slotHeight)
        {
            pos.y -= slotHeight * slots.Length;
            content.anchoredPosition = pos;
        }
        // 向下循环
        else if (pos.y < contentStartY - slotHeight * (slots.Length - 1))
        {
            pos.y += slotHeight * slots.Length;
            content.anchoredPosition = pos;
        }
    }

    // 获取当前中心slot的索引
    public int GetCurrentCenterIndex()
    {
        return currentCenterIndex;
    }
}