using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class SpinePool
{
    private ObjectPool<GameObject> _pool;
    private string _path;
    private GameObject _srcGO;

    private int _initState = -1;
    private AssetLoader<GameObject> assetLoader;
    public SpinePool(string path, bool callInit = true)
    {
        _path = path;
        if (callInit) _ = AsyncInit();
    }

    public async Task AsyncInit()
    {
        if (_initState >= 0)
        {
            return;
        }
        _initState = 0;

        assetLoader = new AssetLoader<GameObject>(_path);
        await assetLoader.AsyncLoad();
        _initState = 1;
        _srcGO = assetLoader.Asset;
        _pool = new ObjectPool<GameObject>(() =>
        {
            var tmp = GameObject.Instantiate(_srcGO);
            return tmp;
        });

    }

    public SkeletonGraphic Get(bool autoRelease = true, float releaseTime = 2f)
    {
        var result = _pool.Get();

        var sp = result.GetComponentInChildren<SkeletonGraphic>();
        //sp.AnimationState.ClearTracks();
        if (autoRelease)
        {
            var tc = result.GetOrAddComponent<TimeCounter>();
            tc.StartCounter(releaseTime, () => { _Back(result); });
        }

        result.gameObject.SetActive(true);
        return sp;
    }

    private void _Back(GameObject get)
    {
        GameObject.Destroy(get);
        //get.gameObject.SetActive(false);
        //_pool.Release(get);
    }

    public void Back(GameObject obj)
    {
        if (obj.TryGetComponent<TimeCounter>(out var tc))
        {
            tc.StopCounter();
        }
        _Back(obj);
    }


}
