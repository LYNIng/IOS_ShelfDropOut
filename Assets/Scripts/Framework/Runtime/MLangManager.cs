using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;


/// <summary>
/// PayType
/// </summary>
public enum __ppType
{
    /// <summary>
    /// CashApp
    /// </summary>
    CA_ = 0,
    /// <summary>
    /// PayPal
    /// </summary>
    PP_,
    /// <summary>
    /// PIX
    /// </summary>
    P_,
    /// <summary>
    /// Nubank
    /// </summary>
    NB_,
    /// <summary>
    /// PayPay
    /// </summary>
    PP__,
    /// <summary>
    /// LinePay
    /// </summary>
    LP_,
    /// <summary>
    /// KakaoPay
    /// </summary>
    KP_,
    /// <summary>
    /// SumsungPay
    /// </summary>
    SP_,
    /// <summary>
    /// GoogleWallet
    /// </summary>
    GW_,
    /// <summary>
    /// QIWIWallet
    /// </summary>
    QW_,
    /// <summary>
    /// YandexMoney
    /// </summary>
    YM_,
    //MercadoPago,
    //Ualá,
    //TrueMoney,
    //RabbitLINEPay,
    //MOMO,
    //ZaloPay,
    //Ininal,
    //FastPay,
    //DANA,
    //OVO,
    //GCash,
    //GooglePay,
}


public interface IIMulLan
{
    void UpdateText();
}

public class MLangManager : Singleton<MLangManager>, IManager, IManagerInit
{

    public enum MultiLanguageEnum
    {
        CN = 0,
        TC = 1,
        EN = 2,
        JA = 3,
        KO = 4,
        ES = 5,
        PT = 6,
        DE = 7,
        FR = 8,
        RU = 9,
        Max,

        //测试  阿拉伯语 
        AR = 100,
        //测试 
    }
    public static bool Inited { get; private set; }
    private LangCfg m_mlConfig;



    private MultiLanguageEnum m_CurrentLanguage;

    private Dictionary<MultiLanguageEnum, Func<int, string>> m_Dic;

    private List<IIMulLan> m_AllTextLanguageList;


    public List<MultiLanguageEnum> AllLanguages { get; private set; }

    //private Dictionary<LanguageEnum, Font> m_fontDict;



    private readonly Dictionary<MultiLanguageEnum, List<__ppType>> PTDict = new Dictionary<MultiLanguageEnum, List<__ppType>>()
        {
            {MultiLanguageEnum.EN, new List<__ppType>(){ __ppType.CA_, __ppType.PP_ }},
            {MultiLanguageEnum.FR, new List<__ppType>(){ __ppType.PP_, __ppType.GW_ }},
            {MultiLanguageEnum.JA, new List<__ppType>(){ __ppType.PP__, __ppType.LP_ }},
            {MultiLanguageEnum.KO, new List<__ppType>(){ __ppType.KP_, __ppType.SP_ }},
            {MultiLanguageEnum.PT, new List<__ppType>(){ __ppType.P_, __ppType.NB_ }},
            {MultiLanguageEnum.RU, new List<__ppType>(){ __ppType.QW_, __ppType.YM_ }},
            {MultiLanguageEnum.ES, new List<__ppType>(){ __ppType.PP_, __ppType.GW_ }},
            {MultiLanguageEnum.DE, new List<__ppType>(){ __ppType.PP_, __ppType.GW_ }},

        };

    //private static Dictionary<string, string> cToPhCode = new Dictionary<string, string>()
    //    {
    //        {"US", "+1"},
    //        {"CA", "+1"},
    //        {"CN", "+86"},
    //        {"JP", "+81"},
    //        {"KR", "+82"},
    //        {"UK", "+44"},
    //        {"FR", "+33"},
    //        {"DE", "+49"},
    //        {"IT", "+39"},
    //        {"RU", "+7"},
    //        {"BR", "+55"},
    //        {"IN", "+91"},
    //    // 添加更多...
    //    };

    //public static string GetCurrentCountry()
    //{
    //    RegionInfo region = new RegionInfo(CultureInfo.CurrentCulture.LCID);
    //    return region.Name; // 返回国家代码，如"US"、"CN"等
    //}

    //public static string GetCurrentCountryToPhoneCode()
    //{
    //    var tmp = GetCurrentCountry();
    //    if (cToPhCode.TryGetValue(tmp, out var result))
    //    {
    //        return result;
    //    }
    //    return "+1";
    //}

    //public List<__ppType> GetPayTypesList()
    //{
    //    if (PTDict.TryGetValue(CurrentLanguage, out var list))
    //    {
    //        return list;
    //    }
    //    else
    //    {
    //        return PTDict[MultiLanguageEnum.EN];
    //    }
    //}

    public List<__ppType> GetPPTypeList()
    {
        return PTDict[CurrentLanguage];
    }

    public MLangManager()
    {


    }

    private void SetLanIt(MultiLanguageEnum language, Font font, Func<int, string> getStringCallback)
    {
        if (m_Dic == null)
            m_Dic = new Dictionary<MultiLanguageEnum, Func<int, string>>();

        m_Dic.Add(language, (int id) =>
        {
            if (id > 0)
            {
                return getStringCallback?.Invoke(id);
            }
            else
                return $"no id {id}";
        });
    }

    public async Task Initialize()
    {
        if (Inited) return;
        Inited = true;

#if UNITY_EDITOR
        //if (LaunchPage.S_SimLanguge)
        //{
        //    CurrentLanguage = LaunchPage.S_LanguageType;
        //    return;
        //}

#endif
        var lang = PlayerPrefs.GetInt("LastLang", -1);
        bool RecordLastLanguageStatus = false;
        //SetCurrentLanguage(lastInt);
        if (lang == -1)
        {
            MultiLanguageEnum tmp = MultiLanguageEnum.EN;
            switch (Application.systemLanguage)
            {
                case SystemLanguage.English:
                    tmp = MultiLanguageEnum.EN;
                    break;
                case SystemLanguage.Japanese:
                    tmp = MultiLanguageEnum.JA;
                    break;
                case SystemLanguage.Korean:
                    tmp = MultiLanguageEnum.KO;
                    break;
                case SystemLanguage.Estonian:
                    tmp = MultiLanguageEnum.ES;
                    break;
                case SystemLanguage.Portuguese:
                    tmp = MultiLanguageEnum.PT;
                    break;
                case SystemLanguage.German:
                    tmp = MultiLanguageEnum.DE;
                    break;
                case SystemLanguage.French:
                    tmp = MultiLanguageEnum.FR;
                    break;
                case SystemLanguage.Russian:
                    tmp = MultiLanguageEnum.RU;
                    break;
                default:
                    tmp = MultiLanguageEnum.EN;
                    break;
            }
            m_CurrentLanguage = tmp;
        }
        else
        {
            Instance.m_CurrentLanguage = (MultiLanguageEnum)lang;
            if (RecordLastLanguageStatus)
                PlayerPrefs.SetInt("LastLang", (int)lang);
        }

        await Task.Run(async () =>
        {
            while (!m_mlConfig.IsLoaded)
            {
                await Task.Yield();
            }
        });

    }

    //    public void InitLoad()
    //    {
    //        if (Inited) return;
    //        Inited = true;

    //#if UNITY_EDITOR
    //        if (LaunchScene.S_SimLanguge)
    //        {
    //            CurrentLanguage = LaunchScene.S_LanguageType;
    //            return;
    //        }

    //#endif

    //        var lang = PlayerPrefs.GetInt("LastLang", -1);
    //        bool RecordLastLanguageStatus = false;
    //        //SetCurrentLanguage(lastInt);
    //        if (lang == -1)
    //        {
    //            MultiLanguageEnum tmp = MultiLanguageEnum.EN;
    //            switch (Application.systemLanguage)
    //            {
    //                case SystemLanguage.English:
    //                    tmp = MultiLanguageEnum.EN;
    //                    break;
    //                case SystemLanguage.Japanese:
    //                    tmp = MultiLanguageEnum.JA;
    //                    break;
    //                case SystemLanguage.Korean:
    //                    tmp = MultiLanguageEnum.KO;
    //                    break;
    //                case SystemLanguage.Estonian:
    //                    tmp = MultiLanguageEnum.ES;
    //                    break;
    //                case SystemLanguage.Portuguese:
    //                    tmp = MultiLanguageEnum.PT;
    //                    break;
    //                case SystemLanguage.German:
    //                    tmp = MultiLanguageEnum.DE;
    //                    break;
    //                case SystemLanguage.French:
    //                    tmp = MultiLanguageEnum.FR;
    //                    break;
    //                case SystemLanguage.Russian:
    //                    tmp = MultiLanguageEnum.RU;
    //                    break;
    //                default:
    //                    tmp = MultiLanguageEnum.EN;
    //                    break;
    //            }
    //            m_CurrentLanguage = tmp;
    //        }
    //        else
    //        {
    //            Instance.m_CurrentLanguage = (MultiLanguageEnum)lang;
    //            if (RecordLastLanguageStatus)
    //                PlayerPrefs.SetInt("LastLang", (int)lang);
    //        }

    //    }

    //public bool TryGetPaySprite_T1(ppType payType, out Sprite result)
    //{
    //    result = null;
    //    if (AssetMgr.TryGetPayImageConfig(AssetMgr.PayImageConfig_T1, out var tmp))
    //    {
    //        return tmp.TryGetImage(payType, out result);
    //    }
    //    return false;
    //}

    //public bool TryGetPaySprite_T2(ppType payType, out Sprite result)
    //{
    //    result = null;
    //    if (AssetMgr.TryGetPayImageConfig(AssetMgr.PayImageConfig_T2, out var tmp))
    //    {
    //        return tmp.TryGetImage(payType, out result);
    //    }
    //    return false;
    //}

    public static MultiLanguageEnum CurrentLanguage
    {
        get
        {

            return Instance.m_CurrentLanguage;
        }
        set
        {
            int language = (int)value;
            if (language == -1)
            {
                switch (Application.systemLanguage)
                {
                    case SystemLanguage.English:
                        language = (int)MultiLanguageEnum.EN;
                        break;
                    case SystemLanguage.Japanese:
                        language = (int)MultiLanguageEnum.JA;
                        break;
                    case SystemLanguage.Korean:
                        language = (int)MultiLanguageEnum.KO;
                        break;
                    case SystemLanguage.Estonian:
                        language = (int)MultiLanguageEnum.ES;
                        break;
                    case SystemLanguage.Portuguese:
                        language = (int)MultiLanguageEnum.PT;
                        break;
                    case SystemLanguage.German:
                        language = (int)MultiLanguageEnum.DE;
                        break;
                    case SystemLanguage.French:
                        language = (int)MultiLanguageEnum.FR;
                        break;
                    case SystemLanguage.Russian:
                        language = (int)MultiLanguageEnum.RU;
                        break;
                    default:
                        language = (int)MultiLanguageEnum.EN;
                        break;
                }
            }
            Instance.m_CurrentLanguage = (MultiLanguageEnum)language;
            int count = Instance.m_AllTextLanguageList.Count;
            for (int i = 0; i < count; i++)
            {
                Instance.m_AllTextLanguageList[i].UpdateText();
            }
            PlayerPrefs.SetInt("LastLang", (int)Instance.m_CurrentLanguage);

            Debug.Log($"设置 {Instance.m_CurrentLanguage}");
        }
    }

    /// <summary>
    /// GetLanguageText
    /// </summary>
    /// <param name="id"></param>
    /// <param name="isReplaceUn"></param>
    /// <returns></returns>
    public static string GetLangStr(int id, bool isReplaceUn = true)
    {
        if (!Inited)
        {
            return id.ToString();
        }
        if (isReplaceUn)
        {
            return Instance.m_Dic[CurrentLanguage](id).Replace("\\n", "\n");
        }
        else
        {
            return Instance.m_Dic[CurrentLanguage](id);
        }

    }
    /// <summary>
    /// GetGoldSymbol
    /// </summary>
    /// <returns></returns>
    public static string GetInfo_Gold_Symbol()
    {
        return GetLangStr(1000);
    }

    /// <summary>
    /// RegisterText
    /// </summary>
    /// <param name="textLanguage"></param>
    internal static void RegText(IIMulLan textLanguage)
    {
        if (!Instance.m_AllTextLanguageList.Contains(textLanguage))
            Instance.m_AllTextLanguageList.Add(textLanguage);
    }
    /// <summary>
    /// RemoveText
    /// </summary>
    /// <param name="textLanguage"></param>
    internal static void RemText(IIMulLan textLanguage)
    {
        if (Instance.m_AllTextLanguageList.Contains(textLanguage))
            Instance.m_AllTextLanguageList.Remove(textLanguage);
    }

    public async Task<bool> AsyncInit()
    {
        m_mlConfig = new LangCfg();
        await m_mlConfig.AsyncInitLangCfg();

        m_AllTextLanguageList = new List<IIMulLan>();
        m_CurrentLanguage = MultiLanguageEnum.EN;

        SetLanIt(MultiLanguageEnum.EN, null, m_mlConfig.GetENByID);
        SetLanIt(MultiLanguageEnum.JA, null, m_mlConfig.GetJAByID);
        SetLanIt(MultiLanguageEnum.KO, null, m_mlConfig.GetKRByID);

        SetLanIt(MultiLanguageEnum.ES, null, m_mlConfig.GetESByID);
        SetLanIt(MultiLanguageEnum.PT, null, m_mlConfig.GetPTByID);
        SetLanIt(MultiLanguageEnum.DE, null, m_mlConfig.GetDEByID);
        SetLanIt(MultiLanguageEnum.FR, null, m_mlConfig.GetFRByID);
        SetLanIt(MultiLanguageEnum.RU, null, m_mlConfig.GetRUByID);


        AllLanguages = new List<MultiLanguageEnum>(m_Dic.Keys);
        Inited = true;
        return true;
    }
}

public static class StringMultiLanguageUtil
{
    public static float ToPriceValue(this int price)
    {
        float result = price / 100f;
        switch (MLangManager.CurrentLanguage)
        {
            case MLangManager.MultiLanguageEnum.EN: //美国
                return (result * 1);
            case MLangManager.MultiLanguageEnum.FR: //法国
                return (result * 1);
            case MLangManager.MultiLanguageEnum.JA: //日本
                return (result * 140);
            case MLangManager.MultiLanguageEnum.KO: //韩国
                return (result * 1350);
            case MLangManager.MultiLanguageEnum.PT: //巴西
                return (result * 5);
            case MLangManager.MultiLanguageEnum.RU: //俄罗斯
                return (result * 60);
            case MLangManager.MultiLanguageEnum.ES: //西班牙
                return (result * 1);
            case MLangManager.MultiLanguageEnum.DE: //德国
                return (result * 1);
            default: //默认英语
                return (result * 1);
        }
    }

    public static string ToPriceStrF0(this int price)
    {
        if (!MLangManager.Inited)
        {
            float t = price / 100f;
            return $"${(t * 1).ToString("f0")}";
        }

        var symbol = MLangManager.GetLangStr(1000);
        string str = "";
        float result = price / 100f;
        switch (MLangManager.CurrentLanguage)
        {
            case MLangManager.MultiLanguageEnum.EN: //美国
                str = $"{symbol}{(result * 1).ToString("f0")}";
                break;
            case MLangManager.MultiLanguageEnum.FR: //法国
                str = $"{(result * 1).ToString("f0")}{symbol}";
                break;
            case MLangManager.MultiLanguageEnum.JA: //日本
                str = $"{symbol}{(result * 140).ToString("f0")}";
                break;
            case MLangManager.MultiLanguageEnum.KO: //韩国
                str = $"{symbol}{(result * 1350).ToString("f0")}";
                break;
            case MLangManager.MultiLanguageEnum.PT: //巴西
                str = $"{symbol}{(result * 5).ToString("f0")}";
                break;
            case MLangManager.MultiLanguageEnum.RU: //俄罗斯
                str = $"{symbol}{(result * 60).ToString("f0")}";
                break;
            case MLangManager.MultiLanguageEnum.ES: //西班牙
                str = $"{(result * 1).ToString("f0")}{symbol}";
                break;
            case MLangManager.MultiLanguageEnum.DE: //德国
                str = $"{(result * 1).ToString("f0")}{symbol}";
                break;
            default: //默认英语
                str = $"{symbol}{(result * 1).ToString("f0")}";
                break;
        }

        return str;
    }

    public static string ToPriceStr(this int price)
    {
        if (!MLangManager.Inited)
        {
            float t = price / 100f;
            return $"${(t * 1).ToString("f2")}";
        }

        var symbol = MLangManager.GetLangStr(1000);
        string str = "";
        float result = price / 100f;
        switch (MLangManager.CurrentLanguage)
        {
            case MLangManager.MultiLanguageEnum.EN: //美国
                str = $"{symbol}{(result * 1).ToString("f2")}";
                break;
            case MLangManager.MultiLanguageEnum.FR: //法国
                str = $"{(result * 1).ToString("f2")}{symbol}";
                break;
            case MLangManager.MultiLanguageEnum.JA: //日本
                str = $"{symbol}{(result * 140).ToString("f0")}";
                break;
            case MLangManager.MultiLanguageEnum.KO: //韩国
                str = $"{symbol}{(result * 1350).ToString("f0")}";
                break;
            case MLangManager.MultiLanguageEnum.PT: //巴西
                str = $"{symbol}{(result * 5).ToString("f2")}";
                break;
            case MLangManager.MultiLanguageEnum.RU: //俄罗斯
                str = $"{symbol}{(result * 60).ToString("f2")}";
                break;
            case MLangManager.MultiLanguageEnum.ES: //西班牙
                str = $"{(result * 1).ToString("f2")}{symbol}";
                break;
            case MLangManager.MultiLanguageEnum.DE: //德国
                str = $"{(result * 1).ToString("f2")}{symbol}";
                break;
            default: //默认英语
                str = $"{symbol}{(result * 1).ToString("f2")}";
                break;
        }

        return str;
    }

    /// <summary>
    /// int 转多语言字符串
    /// </summary>
    /// <param name="multiLangID"></param>
    /// <returns></returns>
    public static string ToMulStr(this int multiLangID)
    {
        return MLangManager.GetLangStr(multiLangID);
    }

    public static string ToMulStrFormat(this int multiLangID, params object[] format)
    {
        return string.Format(multiLangID.ToMulStr(), format);
    }

    public static string ToNum(this int num, ushort assetType)
    {
        if (assetType == 0)
        {
            return num.ToPriceStrF0();
        }
        else
        {
            return $"x{num.ToString()}";
        }
    }
}

