using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public static class CommonUtil
{
    public static bool Intersection(int leftMin, int leftMax, int rightMin, int rightMax)
    {
        return Mathf.Max(leftMin, rightMin) < Mathf.Min(leftMax, rightMax);
    }

    public static int ToIndex(int width, int height, int maxWidth)
    {
        return height * maxWidth + width;
    }

    public static bool IsValidNumFunc(string tex)
    {
        try
        {
            const string StrictEmailPattern = @"^\+?\d{10,15}$";

            return Regex.IsMatch(tex,
                StrictEmailPattern,
                RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    public static bool IsValidEmailAddressFunc(string tex)
    {
        try
        {
            const string StrictEmailPattern =
@"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
@"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";

            return Regex.IsMatch(tex,
                StrictEmailPattern,
                RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// 安卓自定义时长震动
    /// </summary>
    /// <param name="duration">时长ms</param>
    /// <param name="amplitude">震动幅度 0 - 255</param>
    public static void Vibrate(long duration = 500, int amplitude = 255)
    {
        if (GameGlobal.Instance.VibrateIsEnable)
        {
            return;
        }
        try
        {
            //if (SystemInfo.supportsVibration)
            //{
            //    Handheld.Vibrate();
            //}
            if (Application.platform == RuntimePlatform.Android)
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        AndroidJavaClass vibrationPlugin = new AndroidJavaClass("com.cdt.CDTAndroidPlugin");
                        vibrationPlugin.CallStatic("Vibrate", currentActivity, duration, amplitude);
                    }
                }
            }

        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    public static string BuildStringFormCollection(this ICollection vals, char splitChar = '|')
    {
        string results = "";
        int i = 0;
        foreach (var value in vals)
        {
            results += value;
            if (i != vals.Count - 1)
            {
                results += splitChar;
            }
            i++;
        }
        return results;
    }

    public static List<T> BuildListFormString<T>(this string str, char splitChar = '|')
    {
        List<T> list = new List<T>();
        if (string.IsNullOrEmpty(str))
            return list;
        string[] arr = str.Split('|', StringSplitOptions.RemoveEmptyEntries);
        foreach (string v in arr)
        {
            if (string.IsNullOrEmpty(v)) continue;
            T val = (T)Convert.ChangeType(v, typeof(T));
            list.Add(val);
        }
        return list;
    }

    public static Vector3 GetMiddlePoint(Vector3 begin, Vector3 end, float delta = 0)
    {
        Vector3 center = Vector3.Lerp(begin, end, 0.5f);
        Vector3 beginEnd = end - begin;
        Vector3 perpendicular = new Vector3(-beginEnd.y, beginEnd.x, 0).normalized;
        Vector3 middle = center + perpendicular * delta;
        return middle;
    }
}

public static class ArrayUtil
{
    /// <summary>
    /// 将数组内所有元素后移一位
    /// 返回 有多出来的元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <returns></returns>
    public static T ShiftArrayRight<T>(this T[] array)
    {
        if (array == null || array.Length == 0) return default(T);
        var result = array[array.Length - 1];
        for (int i = array.Length - 2; i >= 0; --i)
        {
            array[i + 1] = array[i];
        }
        array[0] = default(T);
        return result;
    }

    /// <summary>
    /// 将数组内所有元素前移一位
    /// 返回 有多出来的元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <returns></returns>
    public static T ShiftArrayLeft<T>(this T[] array)
    {
        if (array == null || array.Length == 0) return default(T);
        var result = array[0];
        for (int i = 1; i < array.Length; ++i)
        {
            array[i - 1] = array[i];
        }
        array[array.Length - 1] = default(T);
        return result;
    }

    public static T GetByXY<T>(this T[] array, int x, int y, int max)
    {
        if (array == null || array.Length == 0)
            return default(T);

        return array[XYToIndex(x, y, max)];
    }

    public static int XYToIndex(int x, int y, int max)
    {
        return y * max + x;
    }

    public static (int x, int y) IndexToXY(int index, int max)
    {
        int x = index % max;
        int y = index / max;
        return (x, y);
    }

    public static bool TryGet<T>(this T[] array, int index, out T result)
    {
        if (index < array.Length)
        {
            result = array[index];
            return true;
        }
        result = default(T);
        return false;
    }
}

public static class ObjectUtil
{
    public static bool TryTo<T>(this object[] objs, int idx, out T result)
    {
        if (objs == null || idx >= objs.Length)
        {
            result = default(T);
            return false;
        }
        else if (objs[idx] is T tmp)
        {
            result = tmp;
            return true;
        }
        result = default(T);
        return false;
    }

    public static T To<T>(this object[] objs, int idx = 0)
    {
        if (TryTo<T>(objs, idx, out var result))
        {
            return result;
        }
        return default(T);
    }
}

public static class GameObjectUtil
{
    public static T GetOrAddComponent<T>(this GameObject go) where T : Behaviour
    {
        var comp = go.GetComponent<T>();
        if (comp == null)
        {
            comp = go.AddComponent<T>();
        }
        return comp;
    }

}

public static class RectTransfromUtil
{
    private static readonly Vector3[] corners = new Vector3[4];
    /// <summary>
    /// 用于 Screen Space - Overlay 模式的 Canvas：
    /// </summary>
    /// <param name="rectTransform"></param>
    /// <returns></returns>
    public static bool IsRectTransformVisibleInOverlay(this RectTransform rectTransform)
    {
        rectTransform.GetWorldCorners(corners);

        Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
        bool isVisible = false;

        foreach (Vector3 corner in corners)
        {
            if (screenRect.Contains(corner))
            {
                isVisible = true;
                break;
            }
        }

        return isVisible;
    }

    public static void SetAnchoredPositionX(this RectTransform target, float x)
    {
        target.anchoredPosition = new Vector2(x, target.anchoredPosition.y);
    }
    public static void SetAnchoredPositionY(this RectTransform target, float y)
    {
        target.anchoredPosition = new Vector2(target.anchoredPosition.x, y);
    }
    public static Tween DoFadeAndAnchorsMoveXFrom(this RectTransform target, float offsetAnchorsPosX, float duration, float beginFade = 0f, float endFade = 1f)
    {
        return target.DoFadeAndAnchorsMoveByFrom(new Vector2(offsetAnchorsPosX, 0), duration, beginFade);
    }
    public static Tween DoFadeAndAnchorsMoveYFrom(this RectTransform target, float offsetAnchorsPosY, float duration, float beginFade = 0f, float endFade = 1f)
    {
        return target.DoFadeAndAnchorsMoveByFrom(new Vector2(0, offsetAnchorsPosY), duration, beginFade);
    }
    public static Tween DoFadeAndAnchorsMoveByFrom(this RectTransform target, Vector2 offsetAnchorsPos, float duration, float beginFade = 0f, float endFade = 1f)
    {
        var cg = target.gameObject.GetOrAddComponent<CanvasGroup>();
        cg.alpha = beginFade;
        var pos = target.anchoredPosition;
        target.anchoredPosition = target.anchoredPosition + offsetAnchorsPos;
        Sequence seq = DOTween.Sequence();
        seq.Append(target.DOAnchorPos(pos, duration));
        seq.Join(cg.DOFade(endFade, duration));

        return seq;
    }
    public static Tween DoFadeAndAnchorsMoveYTo(this RectTransform target, float offsetAnchorsPosY, float duration, float beginFade = 1f, float endFade = 0f)
    {
        return target.DoFadeAndAnchorsMoveTo(new Vector2(0, offsetAnchorsPosY), duration, beginFade, endFade);
    }
    public static Tween DoFadeAndAnchorsMoveXTo(this RectTransform target, float offsetAnchorsPosX, float duration, float beginFade = 1f, float endFade = 0f)
    {
        return target.DoFadeAndAnchorsMoveTo(new Vector2(offsetAnchorsPosX, 0), duration, beginFade, endFade);
    }
    public static Tween DoFadeAndAnchorsMoveTo(this RectTransform target, Vector2 offsetAnchorsPos, float duration, float beginFade = 1f, float endFade = 0f)
    {
        var cg = target.gameObject.GetOrAddComponent<CanvasGroup>();
        cg.alpha = beginFade;
        var pos = target.anchoredPosition + offsetAnchorsPos;
        Sequence seq = DOTween.Sequence();
        seq.Append(target.DOAnchorPos(pos, duration));
        seq.Join(cg.DOFade(endFade, duration));

        return seq;
    }
}
public static class TransformUtil
{
    /// <summary>
    /// 两点之间带曲线路径的插值
    /// </summary>w
    /// <param name="start">起始点</param>
    /// <param name="end">结束点</param>
    /// <param name="t">插值系数(0-1)</param>
    /// <param name="curveHeight">曲线高度(控制弧高)</param>
    /// <param name="frequency">正弦波动频率</param>
    /// <returns>曲线路径上的点</returns>
    public static Tween DoSinLerpMove(this Transform target, Vector3 end, float duration, float curveHeight = 1f, int frequency = 1)
    {
        var v = 0f;
        var srcPos = target.position;
        return DOTween.To(() =>
        {
            return v;
        },
        (resultT) =>
        {
            // 基础线性插值
            Vector3 linearPos = Vector3.Lerp(srcPos, end, resultT);

            // 计算垂直于AB连线的方向(用于创建弧线)
            Vector3 direction = (end - srcPos).normalized;
            Vector3 up = Vector3.Cross(direction, Vector3.Cross(Vector3.up, direction)).normalized;

            // 使用正弦函数计算曲线高度
            float curveFactor = Mathf.Sin(resultT * Mathf.PI * frequency);
            float currentHeight = curveFactor * curveHeight;

            // 应用曲线偏移
            target.position = linearPos + up * currentHeight;

            v = resultT;
        }, 1f, duration);

    }

    public static Tween DoLerpMove(this Transform target, Vector3 pos, float duration)
    {
        var v = 0f;
        var srcPos = target.position;
        return DOTween.To(() =>
        {
            return v;
        },
        (result) =>
        {
            target.position = Vector3.Lerp(srcPos, pos, result);
            v = result;
        }, 1f, duration);
    }

    public static Tween DoSLerpMove(this Transform target, Vector3 pos, float duration)
    {
        var v = 0f;
        var srcPos = target.position;
        return DOTween.To(() =>
        {
            return v;
        },
        (result) =>
        {
            target.position = Vector3.Slerp(srcPos, pos, result);
            v = result;
        }, 1f, duration);
    }
    public static Tween DoRandomLerpMove(this Transform target, Vector3 pos, float duration)
    {
        var rand = RandomHelp.RandomRange(0, 2);

        switch (rand)
        {
            case 1:
                return target.DoSLerpMove(pos, duration);
            case 2:
                return target.DoSinLerpMove(pos, duration, RandomHelp.RandomRange(-500, 200));
            default:
                return target.DoLerpMove(pos, duration);
        }
    }

    public static Tween DoRotateShake(this Transform target, float intensity = 1f, int shakeCnt = 1)
    {
        Sequence seq = DOTween.Sequence();
        var orgRotate = target.localRotation;
        for (int i = 0; i < shakeCnt; ++i)
        {
            seq.Append(target.DOLocalRotate((orgRotate * Quaternion.Euler(0, 0, -2f * intensity)).eulerAngles, 0.05f).SetEase(Ease.InCubic).SetUpdate(true));
            seq.Append(target.DOLocalRotate((orgRotate * Quaternion.Euler(0, 0, 2f * intensity)).eulerAngles, 0.1f).SetEase(Ease.InCubic).SetUpdate(true));
            seq.Append(target.DOLocalRotate((orgRotate * Quaternion.Euler(0, 0, -2f * intensity)).eulerAngles, 0.1f).SetEase(Ease.InCubic).SetUpdate(true));
        }
        seq.Append(target.DOLocalRotate((orgRotate).eulerAngles, 0.05f).SetEase(Ease.InCubic).SetUpdate(true));
        return seq;
    }
    public static Transform SetLocalScale(this Transform target, float scale)
    {
        target.localScale = new Vector3(scale, scale, scale);
        return target;
    }
    public static void ScaleAction(this Transform target, Vector3 startScale, Action<Transform> action)
    {
        Sequence seq = DOTween.Sequence();
        Vector3 orgScale = startScale;
        seq.Append(target.DOScale(Vector3.Scale(orgScale, new Vector3(1.2f, 0.8f, 1f)), 0.05f).SetEase(Ease.InCubic).SetUpdate(true));
        seq.Append(target.DOScale(Vector3.Scale(orgScale, new Vector3(0.8f, 1.2f, 1f)), 0.1f).SetEase(Ease.InCubic).SetUpdate(true));
        seq.Append(target.DOScale(orgScale, 0.05f).SetEase(Ease.OutCubic).SetUpdate(true));

        seq.OnComplete(() =>
        {
            action?.Invoke(target);
        });
    }
    public static void ClickScaleAni(this Transform target, Action<Transform> action)
    {
        var comp = target.gameObject.GetOrAddComponent<ButtonEx>();
        target.ClickScaleAni((setV) => comp.IsClicked = setV, () => comp.IsClicked, (target) => { action?.Invoke(target); });
        //Sequence seq = DOTween.Sequence();
        //Vector3 orgScale = target.localScale;
        //seq.Append(target.DOScale(Vector3.Scale(orgScale, new Vector3(1.2f, 0.8f, 1f)), 0.05f).SetEase(Ease.InCubic).SetUpdate(true));
        //seq.Append(target.DOScale(Vector3.Scale(orgScale, new Vector3(0.8f, 1.2f, 1f)), 0.1f).SetEase(Ease.InCubic).SetUpdate(true));
        //seq.Append(target.DOScale(orgScale, 0.05f).SetEase(Ease.OutCubic).SetUpdate(true));

        //seq.OnComplete(() =>
        //{
        //    action?.Invoke(target);
        //});
    }
    public static void ClickScaleAni(this Transform target, Action<bool> switchSet, Func<bool> switchGet, Action<Transform> action, float durationScale = 1f)
    {
        var m_ClickAni = switchGet != null ? switchGet.Invoke() : false;
        if (m_ClickAni) return;
        switchSet.Invoke(true);

        Sequence seq = DOTween.Sequence();
        Vector3 orgScale = target.localScale;
        seq.Append(target.DOScale(Vector3.Scale(orgScale, new Vector3(1.2f, 0.8f, 1f)), 0.05f * durationScale).SetEase(Ease.InCubic).SetUpdate(true));
        seq.Append(target.DOScale(Vector3.Scale(orgScale, new Vector3(0.8f, 1.2f, 1f)), 0.1f * durationScale).SetEase(Ease.InCubic).SetUpdate(true));
        seq.Append(target.DOScale(orgScale, 0.05f * durationScale).SetEase(Ease.OutCubic).SetUpdate(true));

        seq.OnComplete(() =>
        {
            switchSet?.Invoke(false);
            action?.Invoke(target);
        });
    }

    public static void ClickRotateAni(this Transform target, Action<bool> switchSet, Func<bool> switchGet, Action<Transform> action, float durationScale = 1f)
    {
        var m_ClickAni = switchGet != null ? switchGet.Invoke() : false;
        if (m_ClickAni) return;
        switchSet.Invoke(true);

        Sequence seq = DOTween.Sequence();
        var orgRotate = target.localRotation;
        seq.Append(target.DOLocalRotate((orgRotate * Quaternion.Euler(0, 0, -2f)).eulerAngles, 0.05f).SetEase(Ease.InCubic).SetUpdate(true));
        seq.Append(target.DOLocalRotate((orgRotate * Quaternion.Euler(0, 0, 2f)).eulerAngles, 0.1f).SetEase(Ease.InCubic).SetUpdate(true));
        seq.Append(target.DOLocalRotate((orgRotate * Quaternion.Euler(0, 0, -2f)).eulerAngles, 0.1f).SetEase(Ease.InCubic).SetUpdate(true));
        seq.Append(target.DOLocalRotate((orgRotate).eulerAngles, 0.05f).SetEase(Ease.InCubic).SetUpdate(true));

        seq.OnComplete(() =>
        {
            switchSet?.Invoke(false);
            action?.Invoke(target);
        });

    }

    public static IEnumerator DoUIBackGroundFade(this Transform target)
    {
        var ima = target.GetComponent<Image>();
        if (ima != null)
        {
            var col = ima.color;
            col.a = 0f;
            ima.color = col;
            yield return ima.DOFade(0.83f, 0.2f).WaitForCompletion();
        }
    }
    public static IEnumerator DoUIContentScale(this Transform target)
    {
        Sequence seq = DOTween.Sequence();
        Vector3 orgScale = Vector3.one;
        seq.Append(target.DOScale(new Vector3(1.2f, 0.8f, 1f), 0.1f).SetEase(Ease.InCubic).SetUpdate(true));
        seq.Append(target.DOScale(new Vector3(0.8f, 1.2f, 1f), 0.1f).SetEase(Ease.InCubic).SetUpdate(true));
        seq.Append(target.DOScale(orgScale, 0.05f).SetEase(Ease.OutCubic).SetUpdate(true));

        yield return seq.WaitForCompletion();
    }
    public static IEnumerator DOUIContentFadeShow(this Transform target, Vector3 localPos)
    {
        Sequence seq = DOTween.Sequence();
        var cg = target.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = target.gameObject.AddComponent<CanvasGroup>();
        }

        seq.Append(cg.DOFade(1, 0.2f).SetEase(Ease.Linear));
        seq.Join(target.DOLocalMove(localPos, 0.2f).SetEase(Ease.Linear));
        yield return seq.WaitForCompletion();

    }
    public static CanvasGroup SetFade(this Transform target, float fade)
    {
        var cg = target.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = fade;
        }
        else
        {
            cg = target.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = fade;
        }
        return cg;
    }

    public static Task PopShow(this Transform target)
    {

        return Task.CompletedTask;
    }

    public static Task PopHide(this Transform target)
    {
        return Task.CompletedTask;
    }
}


public static class ButtonUtil
{
    public static void RegistBtnCallback(this Button btn, UnityAction action)
    {
        btn.onClick.AddListener(action);
    }
    public static void RegistBtnCallback(this Button btn, Action<Button> action)
    {
        btn.onClick.AddListener(() =>
        {
            action?.Invoke(btn);
        });
    }
}

public static class ToggleUtil
{
    public static void RegistToggleCallback(this Toggle tog, UnityAction<Toggle, bool> action)
    {
        tog.onValueChanged.AddListener((result) =>
        {
            action?.Invoke(tog, result);
        });
    }
}

public static class MonoBehaviourUtil
{
    public static void ClickRotateAni(this MonoBehaviour target, Action action)
    {
        var comp = target.GetOrAddComponent<ButtonEx>();
        target.ClickRotateAni((setV) => comp.IsClicked = setV, () => comp.IsClicked, (target) => { action?.Invoke(); });
    }

    public static void ClickRotateAni(this MonoBehaviour target, Action<bool> switchSet, Func<bool> switchGet, Action<MonoBehaviour> action, float durationTime = 1f)
    {
        target.transform.ClickRotateAni(switchSet, switchGet,
            (result) => { action?.Invoke(target); },
            durationTime);
    }

    public static void ClickScaleAni(this MonoBehaviour target, Action action)
    {
        var comp = target.GetOrAddComponent<ButtonEx>();
        target.ClickScaleAni((setV) => comp.IsClicked = setV, () => comp.IsClicked, (target) => { action?.Invoke(); });
    }
    public static void ClickScaleAni(this MonoBehaviour target, Action<bool> switchSet, Func<bool> switchGet, Action<MonoBehaviour> action, float durationTime = 1f)
    {
        target.transform.ClickScaleAni(switchSet, switchGet, (result) =>
        {
            action?.Invoke(target);
        }, durationTime);
    }
    public static T GetOrAddComponent<T>(this MonoBehaviour go) where T : Behaviour
    {
        return go?.gameObject.GetOrAddComponent<T>();
    }

}

public static class ImageUtil
{
    public static Image SetFade(this Image ima, float fadeValue)
    {
        var col = ima.color;
        col.a = fadeValue;
        ima.color = col;
        return ima;
    }
}

public static class ListUtil
{
    public static void Shuffle<T>(this IList<T> list, int? seed = null)
    {
        System.Random rng = seed != null ? new System.Random((int)seed) : new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static bool TryFind<T>(this IList<T> list, Predicate<T> match, out T result, out int idx)
    {
        if (match == null)
        {
            result = default(T);
            idx = -1;
            return false;
        }

        for (int i = 0; i < list.Count; ++i)
        {
            if (match(list[i]))
            {
                result = list[i];
                idx = i;
                return true;
            }
        }
        result = default(T);
        idx = -1;
        return false;
    }

    public static bool TryRandomPopOne<T>(this IList<T> list, out T result)
    {
        if (list.Count == 0)
        {
            result = default(T);
            return false;
        }

        int idx = RandomHelp.RandomRange(0, list.Count);
        result = list[idx];
        list.RemoveAt(idx);
        return true;
    }

    public static void Enqueue<T>(this IList<T> list, T value)
    {
        list.Add(value);
    }

    public static T Dequeue<T>(this IList<T> list)
    {
        if (list.Count == 0)
            return default;

        var result = list[0];
        list.RemoveAt(0);
        return result;
    }

    public static bool TryDequeue<T>(this IList<T> list, out T value)
    {
        if (list.Count == 0)
        {
            value = default;
            return false;
        }

        value = list[0];
        list.RemoveAt(0);
        return true;
    }

    public static bool TryPeek<T>(this IList<T> list, out T Value)
    {
        if (list.Count == 0)
        {
            Value = default(T);
            return false;
        }
        Value = list[0];
        return true;
    }

}

public static class ShadowUtil
{
    public static Tween DoEffectDistance(this Shadow target, Vector2 to, float duration)
    {
        var v = 0f;
        var srcPos = target.effectDistance;
        return DOTween.To(() =>
        {
            return v;
        },
        (result) =>
        {
            target.effectDistance = Vector2.Lerp(srcPos, to, result);
            v = result;
        }, 1f, duration);
    }

    public static Tween DoFade(this Shadow target, float to, float duration)
    {
        var v = 0f;
        var srcPos = target.effectColor;
        return DOTween.To(() =>
        {
            return v;
        },
        (result) =>
        {
            Color tmp = srcPos;
            tmp.a = Mathf.Lerp(tmp.a, to, result);
            target.effectColor = tmp;
            v = result;
        }, 1f, duration);
    }

    public static Shadow SetFade(this Shadow target, float to)
    {
        var tmp = target.effectColor;
        tmp.a = to;
        target.effectColor = tmp;
        return target;
    }
}

public class RectTransformUtil
{

}

public static class TypeUtil
{
    public static bool TryGetPropertyByName<PropertyType>(this Type type, string name, BindingFlags bindingFlags, out PropertyInfo resultInfo)
    {
        var info = type.GetProperty(name, bindingFlags);
        if (info != null && info.PropertyType == typeof(PropertyType))
        {
            resultInfo = info;
            return true;
        }
        resultInfo = null;
        return false;
    }

    public static bool TryGetPropertysByAttribute<PropertyType, AttributeType>(this Type type, BindingFlags bindingFlags, ref List<PropertyInfo> resultList) where AttributeType : Attribute
    {
        var infos = type.GetProperties(bindingFlags);
        if (infos == null || infos.Length == 0)
        {
            return false;
        }
        bool resultFlag = false;
        for (int i = 0; i < infos.Length; i++)
        {
            var info = infos[i];
            if (info.PropertyType == typeof(PropertyType))
            {
                var att = info.GetCustomAttribute(typeof(AttributeType));
                if (att != null)
                {
                    if (resultList == null)
                    {
                        resultList = new List<PropertyInfo>();
                    }
                    resultList.Add(info);
                    resultFlag = true;
                }
            }
        }
        return resultFlag;
    }

    public static bool TryGetCustomAttribute<T>(this Type type, out T result) where T : Attribute
    {
        result = type.GetCustomAttribute(typeof(T), true) as T;
        if (result != null)
        {
            return true;
        }
        return false;
    }


    public static bool TryGetCustomAttributes<T>(this Type type, out T[] result) where T : Attribute
    {
        result = type.GetCustomAttributes(typeof(T), true) as T[];
        if (result != null)
        {
            return true;
        }
        return false;
    }


    public static GameObject SpawnNewOne(this GameObject go, Transform parent = null)
    {
        return GameObject.Instantiate(go, parent);
    }
}

public static class DateTimeUtil
{
    /// <summary>
    /// 获取从公元元年开始的总天数
    /// </summary>
    public static int GetDaysSinceEpoch(this DateTime date)
    {
        DateTime epoch = new DateTime(1, 1, 1);
        return (date - epoch).Days;
    }
}