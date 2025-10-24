using UnityEngine;


public abstract class Singleton<T> where T : Singleton<T>, new()
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new T();
                _instance.OnAwake();
                _instance.RegistCommand();
            }
            return _instance;
        }
    }

    protected virtual void OnAwake() { }
    protected virtual void RegistCommand() { }
    protected virtual void UnRegistCommand() { }
    protected virtual void OnClear() { }
    public void Clear()
    {
        UnRegistCommand();
        OnClear();
    }
}

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public abstract bool DontDestory { get; }

    private void Awake()
    {
        if (instance == null)
            instance = this as T;


        if (DontDestory)
        {
            if (transform.root != transform)
            {
                DontDestroyOnLoad(transform.root.gameObject);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
            }
        }


        OnAwake();
        RegistCommand();
    }

    private void OnDestroy()
    {
        UnRegistCommand();
        BeforOnDestroy();
        instance = null;
    }

    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<T>();

                if (instance == null)
                {
                    instance = new GameObject(typeof(T).ToString()).GetOrAddComponent<T>();
                }
            }
            return instance;
        }
    }

    protected virtual void OnAwake() { }
    protected virtual void BeforOnDestroy() { }
    protected virtual void RegistCommand() { }
    protected virtual void UnRegistCommand() { }

}
