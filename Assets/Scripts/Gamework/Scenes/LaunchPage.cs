using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.VFX;
using static Unity.Burst.Intrinsics.X86.Avx;

public class LaunchPage : MonoBehaviour
{
    [SerializeField] private bool isDebug;
    [SerializeField] private RectTransform loadingBar;
    [SerializeField] private TextMeshProUGUI percentText;
    private AsyncOperation _operation;
    private bool _isCompleteLoading;
    private bool _isCompleteScene;
    private float _deltaTimeBar;
    private int _initCount;
    private int _initCompleteCount;

    private Image imaLoadingBar;

#if UNITY_EDITOR
    public bool simLanguage;
    public MLangManager.MultiLanguageEnum simLanguageType;
    public static bool S_SimLanguge;
    public static MLangManager.MultiLanguageEnum S_LanguageType;
#endif

    private void Awake()
    {
#if UNITY_EDITOR
        S_SimLanguge = simLanguage;
        S_LanguageType = simLanguageType;
#endif

        Application.targetFrameRate = 60;
        Debug.unityLogger.logEnabled = isDebug;

        if (loadingBar != null)
        {
            imaLoadingBar = loadingBar.GetComponent<Image>();
            imaLoadingBar.fillAmount = curAmount;
        }
    }

    private float curAmount = 0f;

    private void SetLoadingBar(float amount)
    {
        curAmount = amount;
        if (loadingBar != null)
        {
            //loadingBar.sizeDelta = Vector2.Lerp(new Vector2(61f, 60f), new Vector2(909.06f, 60f), amount);
            imaLoadingBar.fillAmount = curAmount;
        }
    }

    private async void Start()
    {
        try
        {
            SetLoadingBar(0f);
            _initCount = 1;

            //MaxManager.Instance.Initialize();
            //AdjustManager.Instance.Initialize(() => _initCompleteCount++);
            //ConfigManager.Instance.DownloadParameters(() => _initCompleteCount++);



            await Managers.Instance.AsyncInit();


            StartCoroutine(GotoPlayScene(() => _initCompleteCount++));
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }


    private void Update()
    {
        // 时间递增进度
        _deltaTimeBar += Time.deltaTime / 15f;
        // 初始化加载进度
        var initFillAmount = (float)_initCompleteCount / (float)_initCount;
        // 时间和初始化取最大的进度
        var realFillAmount = Mathf.Clamp01(Mathf.Max(_deltaTimeBar, initFillAmount));
        // 插值模拟进度，加载进度平滑，加0.1f平滑一点
        var lerpFillAmount = Mathf.Lerp(curAmount, realFillAmount + 0.1f, Time.deltaTime);
        lerpFillAmount = Mathf.Clamp01(lerpFillAmount);
        SetLoadingBar(lerpFillAmount);
        //percentText.text = $"Loading...<color=#4FE41D>{Mathf.CeilToInt(lerpFillAmount * 100)}%</color>";
        if (percentText != null)
            percentText.text = $"{Mathf.CeilToInt(lerpFillAmount * 100)}%";

        if (_isCompleteScene && realFillAmount >= 1f && lerpFillAmount >= 0.99f && !_isCompleteLoading)
        {
            _isCompleteLoading = true;
            OnLoadDone();
        }
    }

    private IEnumerator GotoPlayScene(Action callback)
    {
        yield return null;

        _operation = SceneManager.LoadSceneAsync("GamePage", LoadSceneMode.Single);
        _operation!.allowSceneActivation = false;

        while (_operation.progress < 0.9f)
        {
            yield return null;
        }
        SoundPlayer.Instance.PlaySound(SoundName.Bgm);
        Debug.Log("Scene load complete");
        _isCompleteScene = true;
        callback?.Invoke();
    }
    private void OnLoadDone()
    {
        Debug.Log("LoadDone");

        #region AB面判断

        //if (!ConfigManager.Instance.GetBoolean("b"))
        //{
        //    AppliationCfg.BOn = false;
        //    Debug.Log("BM: oc");
        //}

        //if (!AdjustManager.Instance.IsNonOrganic())
        //{
        //    AppliationCfg.BOn = false;
        //    Debug.Log("BM: af");
        //}

        //if (AppliationCfg.BOn)
        //{
        //    MsgManager.Instance.Initialize();
        //    Debug.Log("MobileNotifications");
        //    if (ConfigManager.Instance.GetBoolean("h"))
        //    {
        //        CMT.CMTSDK.Start("https://www.ghostdrop.online/", false);
        //    }

        //    AppliationCfg.webViewURL = ConfigManager.Instance.GetString("l");
        //    AppliationCfg.RateOn = ConfigManager.Instance.GetBoolean("r");
        //}
        #endregion

        _operation.allowSceneActivation = true;
        //AdjustManager.TrackGameInfo(AppliationCfg.BOn);
    }

}
