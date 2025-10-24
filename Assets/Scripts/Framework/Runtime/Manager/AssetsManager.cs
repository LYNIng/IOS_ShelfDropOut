using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;


public interface IPreloadAssetLoader : ILoadAsset
{
    bool TryGetPreloadAsset<T>(string assetName, out T resultAsset) where T : UnityEngine.Object;

}

public interface ILoadAsset
{
    object[] AssetPaths { get; }
    float LoadProgress { get; }
    bool IsDone { get; }

    void Unload();
    Task AsyncLoad();

    void OnLoaded<T>(T obj);

}


public class SceneLoader : ILoadAsset
{
    public object[] AssetPaths { get; private set; }

    public float LoadProgress => Handle.IsValid() ? Handle.PercentComplete : 0;

    public bool IsDone => Handle.IsValid() && Handle.IsDone;
    public bool IsLoading => !IsDone && IsCallLoad;

    public bool IsUnload => !IsDone && !IsLoading;

    public event Func<Task> onActivateBefore;

    private bool IsCallLoad = false;

    private LoadSceneMode _loadMode;
    private bool _activateOnLoad;
    private int _priority;

    public AsyncOperationHandle<SceneInstance> Handle { get; private set; }
    public async Task AsyncLoad()
    {
        if (IsDone) return;
        if (IsCallLoad) return;
        IsCallLoad = true;
        try
        {
            Handle = Addressables.LoadSceneAsync(AssetPaths[0], _loadMode, _activateOnLoad, _priority);
            await Handle.Task;

            if (Handle.Status == AsyncOperationStatus.Succeeded)
            {
                if (onActivateBefore != null)
                    await onActivateBefore.Invoke();

                if (_activateOnLoad)
                {
                    OnLoaded(Handle.Result);
                }
                else
                {
                    var activateOp = Handle.Result.ActivateAsync();
                    while (!activateOp.isDone)
                    {
                        await Task.Yield();
                    }

                    OnLoaded(Handle.Result);
                }

            }
            else
            {
                Debug.LogException(Handle.OperationException);
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
        IsCallLoad = false;
    }

    public void OnLoaded<T>(T obj)
    {

    }

    public void Unload()
    {
        if (Handle.IsValid())
        {
            Addressables.UnloadSceneAsync(Handle).Completed += (unloadHandle) =>
            {
                unloadHandle.Release();
            };
        }
        IsCallLoad = false;
    }

    /// <summary>
    /// 初始化 <see cref="SceneLoader"/> 类的新实例，用于加载具有指定参数的场景。
    /// </summary>
    /// <param name="sceneKey">场景的 地址（字符串，在 Addressables Groups 窗口中设置）。场景资源的 GUID。一个 Label（标签），但如果标签对应多个场景，会报错。一个已存在的 AssetReference 对象。</param>
    /// <param name="loadMode">指定场景加载的模式。例如，决定场景是叠加加载还是替换当前场景。</param>
    /// <param name="activateOnLoad">指示场景在加载后是否应立即激活。默认值为 <see langword="false"/>。</param>
    /// <param name="priority">加载场景的优先级。值越高，优先级越高。默认值为 100。</param>
    public SceneLoader(object sceneKey, LoadSceneMode loadMode, bool activateOnLoad = false, int priority = 100)
    {
        AssetPaths = new object[] { sceneKey };
        _loadMode = loadMode;
        _activateOnLoad = activateOnLoad;
        _priority = priority;
    }
}

public class AssetLoader<T> : ILoadAsset where T : UnityEngine.Object
{
    public AsyncOperationHandle<T> Handle { get; protected set; }
    public object[] AssetPaths { get; private set; }
    public float LoadProgress => Handle.IsValid() ? Handle.PercentComplete : 0f;
    public bool IsDone => Handle.IsValid() && Handle.IsDone;
    private bool IsCallLoad = false;
    public T Asset { get; private set; }
    public void Unload()
    {
        if (Handle.IsValid())
        {
            Addressables.Release(Handle);
        }
        Asset = null;
        IsCallLoad = false;
    }
    public async Task AsyncLoad()
    {
        if (IsDone) return;
        if (IsCallLoad) return;
        IsCallLoad = true;
        try
        {
            Handle = Addressables.LoadAssetAsync<T>(AssetPaths[0]);
            await Handle.Task;
            OnLoaded(Handle.Result);
            //Asset = Handle.Result;
        }
        catch (Exception e)
        {
            Debug.LogError($"LoadAsset Error: {e}");
        }
    }
    public void OnLoaded<TObject>(TObject obj)
    {
        if (obj is T result)
        {
            Asset = result;
        }
    }
    public AssetLoader(object assetPath)
    {
        AssetPaths = new object[] { assetPath };
    }
}

public class AssetsLoader<T> : IPreloadAssetLoader where T : UnityEngine.Object
{
    public AsyncOperationHandle<IList<T>> Handle { get; protected set; }
    public object[] AssetPaths { get; private set; }
    public float LoadProgress => Handle.IsValid() ? Handle.PercentComplete : 0f;
    public bool IsDone => Handle.IsValid() && Handle.IsDone;
    private bool IsCallLoad = false;
    public Dictionary<string, T> Assets { get; private set; }
    public void Unload()
    {
        if (Handle.IsValid())
        {
            Handle.Release();
            Handle = default;
        }
        Assets.Clear();
        IsCallLoad = false;
    }
    public async Task AsyncLoad()
    {
        if (IsDone) return;
        if (IsCallLoad) return;

        IsCallLoad = true;

        if (Assets == null)
            Assets = new Dictionary<string, T>();

        try
        {
            Handle = AssetsManager.LoadAssetsAsync<T>(this);
            await Handle.Task;

        }
        catch (Exception e)
        {
            Debug.LogError($"LoadAssets Error: {e}");
        }

    }
    public void OnLoaded<TObject>(TObject obj)
    {
        if (obj is T result)
        {
            if (Assets.ContainsKey(result.name))
            {
                //Debug.LogWarning($"LoadAssets Warning: Asset with Type {result.GetType()} name {result.name} already exists. Overwriting.");
                //Assets[result.name] = result;
                Assets.Add(result.name + $"_{result.GetType()}", result);
                return;
            }
            Assets.Add(result.name, result);
        }
    }

    public bool TryGetPreloadAsset<AssetType>(string assetName, out AssetType resultAsset) where AssetType : UnityEngine.Object
    {
        if (Assets.TryGetValue(assetName, out var result))
        {
            if (result is AssetType tmp)
            {
                resultAsset = tmp;
                return true;
            }
            else
            {
                var newAssetName = assetName + $"_{typeof(AssetType)}";
                if (Assets.TryGetValue(newAssetName, out T resultTmp))
                {
                    if (resultTmp is AssetType tmp2)
                    {
                        resultAsset = tmp2;
                        return true;
                    }
                }

                Debug.Log($"TryGetPreloadAsset 类型错误 调用是{typeof(AssetType)} - 资源是{result.GetType()}");
            }
        }
        resultAsset = null;
        return false;
    }

    public AssetsLoader(object[] assetsInfo)
    {
        AssetPaths = assetsInfo;
    }


}

public class AssetsLoader : AssetsLoader<UnityEngine.Object>
{
    public AssetsLoader(object[] assetsInfo) : base(assetsInfo)
    {

    }

}

public class AssetsManager : MonoSingleton<AssetsManager>, IManager
{
    public static bool Inited { get; private set; } = false;

    public override bool DontDestory => true;


    public static async Task<T> AsyncLoadAsset<T>(string path)
    {
        var handle = Addressables.LoadAssetAsync<T>(path);

        await handle.Task;

        return handle.Result;
    }

    public static async Task<GameObject> AsyncInstantiate(string path, Transform parent = null)
    {
        var handle = Addressables.InstantiateAsync(path, parent);
        await handle.Task;
        return handle.Result;
    }

    public static AsyncOperationHandle<IList<T>> LoadAssetsAsync<T>(ILoadAsset loader)
    {
        AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(
            loader.AssetPaths.AsEnumerable(),
            loader.OnLoaded,
            Addressables.MergeMode.Union,
            true);
        return handle;
    }

    /// <summary>
    /// 这个作为预加载的时机 在object 创建之前调用 
    /// 然后通过SetPreloadAssetLoaderToObj 来赋值 PreloadAssetsLoader
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static async Task<IPreloadAssetLoader> AsyncCreatePreloadAssetsLoader(Type preloadType)
    {
        var objType = preloadType;
        if (objType.TryGetCustomAttribute(out PreloadAssetsAttribute result))
        {
            List<object> paths = new List<object>();
            paths.AddRange(result.paths);
            if (result.loadSwitch && result.loadSwitchPaths != null)
            {
                paths.AddRange(result.loadSwitchPaths);
            }

            var loaderPro = objType.GetProperty("PreloadAssetsLoader", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (loaderPro != null && loaderPro.PropertyType == typeof(IPreloadAssetLoader))
            {
                var loader = new AssetsLoader(paths.ToArray());

                await loader.AsyncLoad();
                return loader;
            }

        }
        else if (objType.TryGetCustomAttribute(out PreloadSpritesAttribute spritesAttribute))
        {
            List<object> paths = new List<object>();
            paths.AddRange(spritesAttribute.paths);
            if (spritesAttribute.loadSwitch && spritesAttribute.loadSwitchPaths != null)
            {
                paths.AddRange(spritesAttribute.loadSwitchPaths);
            }

            var loaderPro = objType.GetProperty("PreloadAssetsLoader", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (loaderPro != null && loaderPro.PropertyType == typeof(IPreloadAssetLoader))
            {
                var loader = new AssetsLoader<Sprite>(paths.ToArray());

                await loader.AsyncLoad();
                return loader;
            }
        }
        return null;
    }

    /// <summary>
    /// 将IPreloadAssetLoader 赋值到 PreloadAssetLoader 属性中
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="loader"></param>
    public static void SetPreloadAssetLoaderToObj(object obj, IPreloadAssetLoader loader)
    {
        if (obj == null || loader == null) return;
        var objType = obj.GetType();
        if (objType.TryGetCustomAttribute(out PreloadAssetsAttribute result))
        {
            var loaderPro = objType.GetProperty("PreloadAssetsLoader", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (loaderPro != null && loaderPro.PropertyType == typeof(IPreloadAssetLoader))
            {
                loaderPro.SetValue(obj, loader);
            }
        }
    }

    public static async Task AsyncCreatePreloadAssets(object obj)
    {
        var objType = obj.GetType();
        if (objType.TryGetCustomAttribute(out PreloadAssetsAttribute result))
        {
            var loaderPro = objType.GetProperty("PreloadAssetsLoader", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (loaderPro != null && loaderPro.PropertyType == typeof(IPreloadAssetLoader))
            {
                var loader = new AssetsLoader<GameObject>(result.paths);
                loaderPro.SetValue(obj, loader);
                await loader.AsyncLoad();
            }
        }
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
/// <summary>
/// 预加载资源
/// </summary>
public class PreloadAssetsAttribute : Attribute
{
    public object[] paths;
    public object[] loadSwitchPaths;
    public bool loadSwitch;
    public ushort assetTag;
    public PreloadAssetsAttribute(params object[] paths)
    {
        this.paths = paths;
        loadSwitch = false;
        this.assetTag = 0;
    }
    public PreloadAssetsAttribute(ushort assetTag, params object[] paths)
    {
        this.paths = paths;
        loadSwitch = false;
        this.assetTag = assetTag;
    }
    public PreloadAssetsAttribute(object[] loadSwitchPaths, params object[] paths)
    {
        this.loadSwitch = AppRunSetting.HasBExpTag;
        this.loadSwitchPaths = loadSwitchPaths;
        this.paths = paths;
    }

    public PreloadAssetsAttribute(object[] loadSwitchPaths, ushort assetTag, params object[] paths)
    {
        this.loadSwitch = AppRunSetting.HasBExpTag;
        this.loadSwitchPaths = loadSwitchPaths;
        this.paths = paths;
        this.assetTag = assetTag;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class PreloadSpritesAttribute : Attribute
{
    public object[] paths;
    public object[] loadSwitchPaths;
    public bool loadSwitch;
    public PreloadSpritesAttribute(params object[] paths)
    {
        this.paths = paths;
        loadSwitch = false;
    }
    public PreloadSpritesAttribute(bool loadSwitch, object[] loadSwitchPaths, params object[] paths)
    {
        this.loadSwitch = loadSwitch;
        this.loadSwitchPaths = loadSwitchPaths;
        this.paths = paths;
    }

}

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
public class ReceivePreloadAssetsAttribute : Attribute
{
    public ushort assetTag;
    public ReceivePreloadAssetsAttribute(ushort assetTag)
    {
        this.assetTag = assetTag;
    }
}