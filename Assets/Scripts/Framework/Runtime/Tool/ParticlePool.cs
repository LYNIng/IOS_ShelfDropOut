using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class MonoParticlePoolParent : MonoSingleton<MonoParticlePoolParent>
{
    public override bool DontDestory => false;
}


public class ParticlePool
{
    private ObjectPool<GameObject> _pool;
    private string _path;
    private GameObject _srcGO;

    private int _initState = -1;

    private AssetLoader<GameObject> assetLoader;

    private UICanvasLayer? canvasLayer;
    private int layerOffset = 0;
    public float AutoReleaseTime { get; set; } = 2f;
    public bool UseDefaultPoolParent { get; private set; } = true;
    public Transform PoolPartent { get; private set; } = null;
    public ParticlePool(string path, bool callInit = true)
    {
        _path = path;
        if (callInit) _ = AsyncInit();
    }

    public void SetLayerSort(UICanvasLayer canvasLayer, int offset = 1)
    {
        this.canvasLayer = canvasLayer;
        this.layerOffset = offset;
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

    public GameObject Spawn(bool autoRelease = true, Action<GameObject> onSet = null)
    {
        var result = _pool.Get();

        var sp = result.GetComponentInChildren<ParticleSystem>();
        if (autoRelease)
        {
            var tc = result.GetOrAddComponent<TimeCounter>();
            tc.StartCounter(sp.main.duration + AutoReleaseTime, () => { _Back(result); });
        }
        if (canvasLayer.HasValue)
        {
            var arr = result.GetComponentsInChildren<ParticleSystemRenderer>();
            for (int i = 0; i < arr.Length; i++)
            {
                var psRenderer = arr[i];
                if (psRenderer != null)
                {
                    psRenderer.sortingOrder = (int)canvasLayer.Value + layerOffset;
                }
            }
            var tArr = result.GetComponentsInChildren<TrailRenderer>();
            for (int i = 0; i < tArr.Length; i++)
            {
                var tr = tArr[i];
                if (tr != null)
                {
                    tr.sortingOrder = (int)canvasLayer.Value + layerOffset;
                }
            }
        }
        onSet?.Invoke(result.gameObject);
        result.gameObject.SetActive(true);
        return result;
    }

    private void _Back(GameObject get)
    {
        get.gameObject.SetActive(false);
        if (PoolPartent != null)
            get.transform.SetParent(PoolPartent);
        else if (UseDefaultPoolParent)
        {
            get.transform.SetParent(MonoParticlePoolParent.Instance.transform);
        }
        _pool.Release(get);
    }

    public void Back(GameObject obj)
    {
        if (obj.TryGetComponent<TimeCounter>(out var tc))
        {
            tc.StopCounter();
        }
        _Back(obj);
    }

    public void AutoRelease(GameObject obj)
    {
        var tc = obj.GetOrAddComponent<TimeCounter>();
        var sp = obj.GetComponentInChildren<ParticleSystem>();
        tc.StartCounter(sp.main.duration + AutoReleaseTime, () => { _Back(obj); });
    }

    public void DestroyAll()
    {
        _pool.Dispose();
    }
}
