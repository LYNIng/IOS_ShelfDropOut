using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public partial class UIGlobal
{
    public static bool AutoSetAdaptation
    {
        get => true;
    }

    public static int UIResolution_Width
    {
        get
        {
            return DataManager.GetDataByInt("UIResolution_Width", 1080);
        }
        set
        {
            DataManager.SetDataByInt("UIResolution_Width", value);
        }
    }

    public static int UIResolution_Height
    {
        get
        {
            return DataManager.GetDataByInt("UIResolution_Height", 1920);
        }
        set
        {
            DataManager.SetDataByInt("UIResolution_Height", value);
        }
    }
}

public enum EBackgroundMask
{
    /// <summary>
    /// 没有遮挡
    /// </summary>
    None,
    /// <summary>
    /// 有遮挡
    /// </summary>
    Black_75F,
    /// <summary>
    /// 有遮挡,透明的
    /// </summary>
    Transparency,

}

public enum EUIGroupTag
{
    None,

    GamePop = 0x1,
}

[AttributeUsage(AttributeTargets.Class)]
public class UISettingAttribute : Attribute
{
    public UICanvasLayer DefaultLayer { get; private set; }
    public bool HideOnClose { get; private set; }

    public EBackgroundMask BackgroundMask { get; private set; }

    public EUIGroupTag UIGroupTag { get; private set; }
    public UISettingAttribute(UICanvasLayer defaultLayer,
        bool hideOnClose = false,
        EBackgroundMask backgroundMask = EBackgroundMask.None,
        EUIGroupTag UIGroupTag = EUIGroupTag.None)
    {
        DefaultLayer = defaultLayer;
        HideOnClose = hideOnClose;
        BackgroundMask = backgroundMask;
        this.UIGroupTag = UIGroupTag;
    }
}

public enum UICanvasLayer
{
    Default_Camera = 50 * 0,
    Background_Camera = 50 * 1,
    Main_Camera = 50 * 2,
    Popup_Camera = 50 * 3,
    Overlay_Camera = 50 * 4,
    System_Camera = 50 * 5,
    Top_Camera = 50 * 6,


    Default_Overlay = 50 * 7,      // 默认层
    Default_Global = 50 * 8, // 全局默认层

    Background_Overlay = 50 * 9,   // 背景层
    Background_Global = 50 * 10, // 全局背景层   

    Main_Overlay = 50 * 11,         // 主界面层
    Main_Global = 50 * 12, // 全局主界面层

    Popup_Overlay = 50 * 13,        // 弹窗层
    Popup_Global = 50 * 14, // 全局弹窗层

    Overlay_Overlay = 50 * 15,      // 顶部覆盖层（如提示、引导等）
    Overlay_Global = 50 * 16, // 全局顶部覆盖层（如提示、引导等）

    System_Overlay = 50 * 17,       // 系统层（如加载、全局遮罩等）
    System_Global = 50 * 18, // 全局系统层（如加载、全局遮罩等）

    Top_Overlay = 50 * 19,           // 最高层（如GM面板、调试等）
    Top_Global = 50 * 20, // 全局最高层（如GM面板、调试等）   
}

public abstract class UIData
{

}

[DisallowMultipleComponent]
public abstract class UIBase : MonoBehaviour
{
    public IPreloadAssetLoader PreloadAssetsLoader { get; protected set; }

    public enum UIState
    {
        None,
        Open,
        Hide,
        Close
    }

    public UICanvasLayer DefaultCanvasLayer { get; protected set; }

    public UICanvasLayer CanvasLayer { get; protected set; }

    protected UIState state { get; set; } = UIState.None;

    public bool IsOpen { get => state == UIState.Open; }
    public bool IsHide { get => state == UIState.Hide; }
    public bool HideOnClose { get; protected set; } = false;

    public EBackgroundMask BackgroundMask { get; protected set; }

    public EUIGroupTag UIGroupTag { get; protected set; }

    public event Action onClosed;

    public virtual async Task OnAsyncPreload()
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Data赋值之后的初始化时机
    /// </summary>
    public virtual void OnInit() { }

    public void SetCanvasLayer(UICanvasLayer layer)
    {
        if (CanvasLayer == layer) return;
        CanvasLayer = layer;
        var canvasTrans = UIManager.GetCanvasLayerTransform(layer);
        transform.SetParent(canvasTrans, false);

    }

    public void SetCanvasLayerByDefault()
    {
        SetCanvasLayer(DefaultCanvasLayer);
    }

    public async void Close()
    {
        await UIManager.CloseUIAsync(this);
    }
    public async Task AsyncClose()
    {
        await UIManager.CloseUIAsync(this);
    }

    public async Task OnShowAsync()
    {
        if (IsOpen) return;
        state = UIState.Open;
        gameObject.SetActive(false);
        await ShowBackgroundMask();
        gameObject.SetActive(true);
        await Show_Internal();
        OnShowed();
    }

    /// <summary>
    /// 这个方法是提供UIManager调用,
    /// 其他地方不要调用
    /// 如果要关闭请使用AsyncClose来关闭
    /// </summary>
    /// <returns></returns>
    public async Task<bool> OnCloseAsync()
    {
        if (!IsOpen) return false;

        if (HideOnClose)
        {
            state = UIState.Hide;
            await Hide_Internal();
            await HideBackgroundMask();
            if (imaMask != null)
            {
                Destroy(imaMask.gameObject);
                imaMask = null;
            }
            gameObject.SetActive(false);
            onClosed?.Invoke();
            return false;
        }
        else
        {
            state = UIState.Close;
            await Hide_Internal();
            await HideBackgroundMask();
            Destroy(gameObject);
            onClosed?.Invoke();
            return true;
        }
    }

    private Image imaMask;
    private void DestroyMask()
    {
        if (imaMask != null)
        {
            Destroy(imaMask.gameObject);
            imaMask = null;
        }
    }
    private Image GetImaMask()
    {
        if (imaMask != null) return imaMask;
        var maskGO = new GameObject("MaskGO", typeof(RectTransform));
        maskGO.transform.SetParent(transform.parent);
        maskGO.transform.SetSiblingIndex(transform.GetSiblingIndex());
        var maskRT = maskGO.GetOrAddComponent<RectTransform>();
        maskRT.anchorMin = Vector2.zero;
        maskRT.anchorMax = Vector2.one;
        maskRT.offsetMax = Vector2.zero;
        maskRT.offsetMin = Vector2.zero;
        imaMask = maskGO.GetOrAddComponent<Image>();
        return imaMask;
    }

    private async Task ShowBackgroundMask()
    {
        if (BackgroundMask == EBackgroundMask.None)
        {
            await Task.CompletedTask;
        }
        else if (BackgroundMask == EBackgroundMask.Black_75F)
        {
            imaMask = GetImaMask();
            imaMask.color = Color.white * 0.1f;
            await imaMask.SetFade(0f).DOFade(0.75f, 0.2f).AsyncWaitForCompletion();
        }
        else if (BackgroundMask == EBackgroundMask.Transparency)
        {
            imaMask = GetImaMask();
            imaMask.color = Color.white * 0f;
        }
    }

    private async Task HideBackgroundMask()
    {
        if (BackgroundMask == EBackgroundMask.None)
        {
            await Task.CompletedTask;
        }
        else if (BackgroundMask == EBackgroundMask.Black_75F)
        {
            await imaMask.DOFade(0f, 0.2f).AsyncWaitForCompletion();
        }
        else if (BackgroundMask == EBackgroundMask.Transparency)
        {
            await Task.CompletedTask;
        }
    }

    protected virtual async Task Show_Internal()
    {
        await transform.SetFade(0f).DOFade(1f, 0.2f).AsyncWaitForCompletion();
    }

    protected virtual async Task Hide_Internal()
    {
        await transform.GetOrAddComponent<CanvasGroup>().DOFade(0, 0.2f).AsyncWaitForCompletion();
    }
    /// <summary>
    /// 注册相关的操作可以放在这里
    /// </summary>
    protected virtual void OnShowed() { }
    /// <summary>
    /// 有需要反注册的操作可以放在这里
    /// </summary>
    protected virtual void OnHideed() { }

    private void OnDestroy()
    {
        if (imaMask != null)
        {
            Destroy(imaMask.gameObject);
            imaMask = null;
        }
        UIManager.UIDestroy(this);

    }

    public bool TryGetPreloadAsset<T>(string assetName, out T resultAsset) where T : UnityEngine.Object
    {
        if (PreloadAssetsLoader != null && PreloadAssetsLoader.TryGetPreloadAsset(assetName, out resultAsset))
        {

            return true;
        }
        resultAsset = null;
        return false;
    }

    public virtual async Task<object> WaitClose()
    {
        while (IsOpen)
            await Task.Yield();
        return null;
    }

}

public abstract class UIBase<T> : UIBase where T : UIData
{
    public T Data { get; protected set; }
}


public class UIManager : MonoSingleton<UIManager>, IManager, IManagerInit
{
    public static Camera UIMainCamera { get; set; } = null;

    private const string UIPrefabsPath = "Prefabs/UI/{0}.prefab";

    public const string DefaultToastPath = "Prefabs/UI/UIToast.prefab";

    private static Dictionary<Type, UIBase> UIDict = new Dictionary<Type, UIBase>();

    private static Dictionary<UICanvasLayer, Transform> canvasLayerDict = new Dictionary<UICanvasLayer, Transform>();

    private static AssetLoader<GameObject> canvasLoader;

    private class UIQueueItem
    {
        public Type UIType;
        public UIData UIData;

    }

    private static List<UIQueueItem> UIQueue = new List<UIQueueItem>();

    public override bool DontDestory => true;

    public bool inited { get; private set; } = false;

    public static bool IsUIOpen<T>()
    {
        return IsUIOpen(typeof(T));
    }

    public static bool IsUIOpen(Type uiType)
    {
        return UIDict.TryGetValue(uiType, out var ui) && ui.IsOpen;
    }

    public static bool TryGetUI<T>(out T result) where T : UIBase
    {
        if (TryGetUI(typeof(T), out UIBase ui) && ui is T tmp)
        {
            result = tmp;
            return true;
        }
        result = null;
        return false;
    }

    public static bool TryGetUI(Type uiType, out UIBase result)
    {
        if (UIDict.TryGetValue(uiType, out var ui))
        {
            result = ui;
            return true;
        }
        result = null;
        return false;
    }

    public static async void OpenUI<T>(UIData uiData = null, Action<T> onResult = null) where T : UIBase
    {
        var ui = await OpenUIAsync<T>(uiData);
        onResult?.Invoke(ui);
    }
    public static async void OpenUI(Type uiType, UIData uiData = null, Action<UIBase> onResult = null)
    {
        var ui = await OpenUIAsync(uiType, uiData);
        onResult?.Invoke(ui);
    }

    public static async Task OpenMultiUIAsync(Type[] uiType)
    {
        Task[] tasks = new Task[uiType.Length];

        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = OpenUIAsync(uiType[i]);
        }

        await Task.WhenAll(tasks);
    }

    public static async Task<T> OpenUIAsync<T>(UIData uiData = null) where T : UIBase
    {
        return await OpenUIAsync(typeof(T), uiData) as T;
    }

    public static async Task<UIBase> OpenUIAsync(Type uiType, UIData uiData = null)
    {
        try
        {
            if (UIDict.TryGetValue(uiType, out var ui))
            {
                MessageDispatch.BindMessage(ui);
                await ui.OnShowAsync();
                return ui;
            }

            var uiName = uiType.Name;
            var uiPath = string.Format(UIPrefabsPath, uiName);

            UICanvasLayer defaultCanvasLayer = UICanvasLayer.Default_Overlay;
            EBackgroundMask eBackgroundMask = EBackgroundMask.None;
            EUIGroupTag uigroupTag = EUIGroupTag.None;
            if (uiType.TryGetCustomAttribute(out UISettingAttribute result))
            {
                defaultCanvasLayer = result.DefaultLayer;
                eBackgroundMask = result.BackgroundMask;
                uigroupTag = result.UIGroupTag;
            }
            Transform canvasRoot = GetCanvasLayerTransform(defaultCanvasLayer);
            var preloader = await AssetsManager.AsyncCreatePreloadAssetsLoader(uiType);
            var resultGO = await AssetsManager.AsyncInstantiate(uiPath);
            resultGO.transform.SetParent(canvasRoot, false);
            resultGO.transform.SetAsLastSibling();
            var uiBase = resultGO.GetComponent<UIBase>();
            if (uiBase == null)
            {
                preloader.Unload();
                throw new Exception($"OpenUI Error: {uiName} does not have a UIBase component.");
            }

            var defCLPro = uiType.GetProperty("DefaultCanvasLayer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (defCLPro != null) defCLPro.SetValue(uiBase, defaultCanvasLayer);
            var CLPro = uiType.GetProperty("CanvasLayer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (CLPro != null) CLPro.SetValue(uiBase, defaultCanvasLayer);
            var bgMaskPro = uiType.GetProperty("BackgroundMask", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (bgMaskPro != null) bgMaskPro.SetValue(uiBase, eBackgroundMask);
            var UIGroupTagPro = uiType.GetProperty("UIGroupTag", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (UIGroupTagPro != null) UIGroupTagPro.SetValue(uiBase, uigroupTag);

            AssetsManager.SetPreloadAssetLoaderToObj(uiBase, preloader);

            BindData(uiBase, uiData);
            MessageDispatch.BindMessage(uiBase);

            UIDict.Add(uiType, uiBase);
            //await uiBase.OnAsyncPreload();
            uiBase.OnInit();

            await uiBase.OnShowAsync();
            return uiBase;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }
    }

    public static void OpenUIByQueue<T>(UIData uiData = null)
    {
        UIQueue.Enqueue(new UIQueueItem { UIType = typeof(T), UIData = uiData });
        if (UIQueue.Count == 1)
        {
            OpenUI(typeof(T), uiData);
        }
    }
    public static async void CloseUI(UIBase ui)
    {
        await CloseUIAsync(ui.GetType());
    }
    public static async void CloseUI(Type type)
    {
        await CloseUIAsync(type);
    }
    public static async void CloseUI<T>()
    {
        await CloseUIAsync<T>();
    }

    public static async Task CloseMultiUIAsync(Type[] uiType)
    {
        Task[] tasks = new Task[uiType.Length];
        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = CloseUIAsync(uiType[i]);
        }

        await Task.WhenAll(tasks);
    }
    public static async Task CloseMultiUIAsyncByUIGroupTag(EUIGroupTag tag)
    {
        List<Task> tasks = null; ;
        foreach (var kvp in UIDict)
        {
            var uibase = kvp.Value;
            if ((uibase.UIGroupTag & tag) != 0)
            {
                if (tasks == null)
                    tasks = new List<Task>();
                tasks.Add(uibase.AsyncClose());
            }
        }
        if (tasks != null)
            await Task.WhenAll(tasks);
    }
    public static async Task CloseUIAsync(UIBase uiBase)
    {
        if (toastUIList != null)
        {
            if (toastUIList.Remove(uiBase))
            {
                var removeFlag = await uiBase.OnCloseAsync();
                if (removeFlag)
                {
                    UIDestroy(uiBase);
                }
                return;
            }
        }

        await CloseUIAsync(uiBase.GetType());
    }

    public static async Task CloseUIAsync<T>()
    {
        await CloseUIAsync(typeof(T));
    }

    public static async Task CloseUIAsync(Type uiType)
    {
        if (UIDict.TryGetValue(uiType, out var uiBase))
        {
            MessageDispatch.UnBindMessage(uiBase);
            var removeFlag = await uiBase.OnCloseAsync();
            if (removeFlag)
            {
                UIDestroy(uiBase);
            }

            if (UIQueue.TryPeek(out UIQueueItem item))
            {
                if (uiType == item.UIType)
                {
                    UIQueue.Dequeue();

                    if (UIQueue.TryPeek(out UIQueueItem item2))
                    {
                        await OpenUIAsync(item2.UIType, item2.UIData);
                    }
                }
            }
        }
    }



    public static Transform GetCanvasLayerTransform(UICanvasLayer layer)
    {
        if (canvasLayerDict.TryGetValue(layer, out var layerTransform))
        {
            return layerTransform;
        }
        else
        {
            var canvasGo = canvasLoader.Asset.SpawnNewOne();
            var canvas = canvasGo.GetComponent<Canvas>();
            canvasGo.name = layer.ToString();
            if (layer.IsCamera())
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = UIMainCamera == null ? Camera.main : UIMainCamera;
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            SetCanvasScaler(canvas);

            if (layer.IsGlobal())
            {
                DontDestroyOnLoad(canvasGo);
            }

            canvas.sortingOrder = (int)layer;
            layerTransform = canvasGo.transform;
            canvasLayerDict.Add(layer, layerTransform);
        }

        return layerTransform;
    }

    public static void SetCanvasScaler(Canvas canvas)
    {
        var canvasScaler = canvas.gameObject.GetOrAddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(UIGlobal.UIResolution_Width, UIGlobal.UIResolution_Height);
        if (UIGlobal.AutoSetAdaptation)
        {
            var t = (float)Screen.width / (float)Screen.height;
            if (t > 0.6f)
            {
                canvasScaler.matchWidthOrHeight = 1f;
            }
            else
            {
                canvasScaler.matchWidthOrHeight = 0f;
            }
            // 强制重新构建布局
            LayoutRebuilder.ForceRebuildLayoutImmediate(canvas.GetComponent<RectTransform>());
            // 或者对整个画布进行刷新
            Canvas.ForceUpdateCanvases();
        }
    }

    private static void BindData(UIBase uiBase, UIData uiData)
    {
        if (uiData == null) return;
        var type = uiBase.GetType();
        var property = type.GetProperty("Data", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property == null) return;
        property.SetValue(uiBase, uiData);

    }


    public async Task<bool> AsyncInit()
    {
        if (inited) return true;

        try
        {
            canvasLoader = new AssetLoader<GameObject>("Prefabs/UI/Canvas.prefab");
            await canvasLoader.AsyncLoad();


            inited = true;

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"UIManager AsyncInit Error: {e}");
            return false;
        }
    }

    public static void UIDestroy(UIBase uiBase)
    {

        if (UIDict.Remove(uiBase.GetType()))
        {

        }
    }

    private static List<UIBase> toastUIList;

    private static Dictionary<string, AssetLoader<GameObject>> toastLoaderDict;

    public static void ShowToast(string toast, string toastUIPath = DefaultToastPath)
    {
        _ = AsyncShowToast(toast, toastUIPath);
    }

    public static async Task AsyncShowToast(string toast, string toastUIPath = DefaultToastPath)
    {
        if (string.IsNullOrEmpty(toastUIPath))
        {
            return;
        }

        if (toastLoaderDict == null)
        {
            toastLoaderDict = new Dictionary<string, AssetLoader<GameObject>>();
        }

        if (!toastLoaderDict.TryGetValue(toastUIPath, out var loader))
        {
            loader = new AssetLoader<GameObject>(toastUIPath);
            toastLoaderDict.Add(toastUIPath, loader);
            await loader.AsyncLoad();
        }
        var prefabCompBase = loader.Asset.GetComponent<UIBase>();
        var uiType = prefabCompBase.GetType();
        var uiName = uiType.Name;
        UICanvasLayer defaultCanvasLayer = UICanvasLayer.Default_Overlay;
        EBackgroundMask eBackgroundMask = EBackgroundMask.None;
        EUIGroupTag uigroupTag = EUIGroupTag.None;
        if (uiType.TryGetCustomAttribute(out UISettingAttribute result))
        {
            defaultCanvasLayer = result.DefaultLayer;
            eBackgroundMask = result.BackgroundMask;
            uigroupTag = result.UIGroupTag;
        }

        Transform canvasRoot = GetCanvasLayerTransform(defaultCanvasLayer);
        var preloader = await AssetsManager.AsyncCreatePreloadAssetsLoader(uiType);
        var resultGO = loader.Asset.SpawnNewOne(canvasRoot);
        resultGO.transform.SetAsLastSibling();
        var uiBase = resultGO.GetComponent<UIBase>();
        if (uiBase == null)
        {
            preloader.Unload();
            throw new Exception($"OpenUI Error: {uiName} does not have a UIBase component.");
        }

        try
        {
            var defCLPro = uiType.GetProperty("DefaultCanvasLayer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (defCLPro != null) defCLPro.SetValue(uiBase, defaultCanvasLayer);
            var CLPro = uiType.GetProperty("CanvasLayer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (CLPro != null) CLPro.SetValue(uiBase, defaultCanvasLayer);
            var bgMaskPro = uiType.GetProperty("BackgroundMask", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (bgMaskPro != null) bgMaskPro.SetValue(uiBase, eBackgroundMask);
            var UIGroupTagPro = uiType.GetProperty("UIGroupTag", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (UIGroupTagPro != null) UIGroupTagPro.SetValue(uiBase, uigroupTag);

            AssetsManager.SetPreloadAssetLoaderToObj(uiBase, preloader);

            BindData(uiBase, new UIToastParam { msg = toast });
            MessageDispatch.BindMessage(uiBase);

            if (toastUIList == null)
                toastUIList = new List<UIBase>();
            toastUIList.Add(uiBase);
            //await uiBase.OnAsyncPreload();
            uiBase.OnInit();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }


        await uiBase.OnShowAsync();

    }
}

public static class UIManagerUtil
{
    public static bool IsGlobal(this UICanvasLayer layer)
    {
        return layer == UICanvasLayer.Default_Global ||
            layer == UICanvasLayer.Background_Global ||
            layer == UICanvasLayer.Main_Global ||
            layer == UICanvasLayer.Overlay_Global ||
            layer == UICanvasLayer.Popup_Global ||
            layer == UICanvasLayer.Top_Global ||
            layer == UICanvasLayer.System_Global;
    }

    public static bool IsOverlay(this UICanvasLayer layer)
    {
        return layer == UICanvasLayer.Default_Overlay ||
            layer == UICanvasLayer.Background_Overlay ||
            layer == UICanvasLayer.Main_Overlay ||
            layer == UICanvasLayer.Overlay_Overlay ||
            layer == UICanvasLayer.Popup_Overlay ||
            layer == UICanvasLayer.Top_Overlay ||
            layer == UICanvasLayer.System_Overlay;
    }

    public static bool IsCamera(this UICanvasLayer layer)
    {
        return layer == UICanvasLayer.Default_Camera ||
            layer == UICanvasLayer.Background_Camera ||
            layer == UICanvasLayer.Main_Camera ||
            layer == UICanvasLayer.Overlay_Camera ||
            layer == UICanvasLayer.Popup_Camera ||
            layer == UICanvasLayer.Top_Camera ||
            layer == UICanvasLayer.System_Camera;
    }
}
