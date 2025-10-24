using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EditorCommon
{
    public class EditorCommon
    {

    }

    public static class RectTransformExtensions
    {
        [MenuItem("Tools/RectTransform/根据子物体调整大小")]
        private static void AdjustSizeToChildren()
        {
            var rtTrans = Selection.activeTransform as RectTransform;
            AdjustRectTransformSize(rtTrans);
        }

        [MenuItem("Tools/RectTransform/重置为最小尺寸 %R")]
        private static void ResetToMinSize()
        {
            var rtTrans = Selection.activeTransform as RectTransform;
            //RectTransform rectTransform = (RectTransform)command.context;
            ResetRectTransform(rtTrans);
        }

        private static void AdjustRectTransformSize(RectTransform parentRT)
        {
            if (parentRT.childCount == 0)
            {
                Debug.LogWarning("没有子物体可以计算尺寸");
                return;
            }

            // 获取所有直接子物体
            var children = parentRT.Cast<RectTransform>()
                .Where(child => child.gameObject.activeInHierarchy)
                .ToArray();

            Vector4 bounds = CalculateChildrenBounds(parentRT, children);

            float width = bounds.z - bounds.x;
            float height = bounds.w - bounds.y;

            // 保留原始轴心点
            Vector2 originalPivot = parentRT.pivot;
            Vector2 originalAnchorMin = parentRT.anchorMin;
            Vector2 originalAnchorMax = parentRT.anchorMax;

            // 临时设置为拉伸模式以便调整尺寸
            parentRT.anchorMin = new Vector2(0, 0);
            parentRT.anchorMax = new Vector2(0, 0);
            parentRT.pivot = new Vector2(0, 0);

            parentRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            parentRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            // 恢复原始设置
            parentRT.anchorMin = originalAnchorMin;
            parentRT.anchorMax = originalAnchorMax;
            parentRT.pivot = originalPivot;

            Undo.RecordObject(parentRT, "Adjust RectTransform Size to Children");
        }

        private static Vector4 CalculateChildrenBounds(RectTransform parent, RectTransform[] children)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (RectTransform child in children)
            {
                Vector3[] corners = new Vector3[4];
                child.GetWorldCorners(corners);

                foreach (Vector3 corner in corners)
                {
                    Vector3 localCorner = parent.InverseTransformPoint(corner);
                    minX = Mathf.Min(minX, localCorner.x);
                    minY = Mathf.Min(minY, localCorner.y);
                    maxX = Mathf.Max(maxX, localCorner.x);
                    maxY = Mathf.Max(maxY, localCorner.y);
                }
            }

            return new Vector4(minX, minY, maxX, maxY);
        }

        private static void CenterAllChildren(RectTransform parent)
        {
            if (parent.childCount == 0) return;

            Vector4 bounds = CalculateChildrenBounds(parent,
                parent.Cast<RectTransform>().ToArray());

            Vector2 center = new Vector2(
                (bounds.x + bounds.z) * 0.5f,
                (bounds.y + bounds.w) * 0.5f
            );

            foreach (RectTransform child in parent)
            {
                Vector2 localPos = child.anchoredPosition;
                child.anchoredPosition = new Vector2(
                    localPos.x - center.x,
                    localPos.y - center.y
                );
            }

            Undo.RecordObject(parent, "Center Children in RectTransform");
        }

        private static void ResetRectTransform(RectTransform rectTransform)
        {
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            Undo.RecordObject(rectTransform, "Reset RectTransform");
        }
    }

}
