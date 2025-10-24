//=============================================
//更新时间：2025-10-13 11:35:14
//备    注：此代码为工具生成 请勿手工修改
//=============================================

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
public class LangCfg
{
    protected bool LangCfg_HasKey => true;
    protected Dictionary<int, LangCfgData> m_LangCfgdict;
    private Dictionary<int, int> m_LangCfgIDX_KEY;
    public int DataCount => m_LangCfgdict.Count;
    public bool IsLoaded { get; set; }
    private AssetLoader<TextAsset> textAssetLoader;
    public async Task AsyncInitLangCfg()
    {
        textAssetLoader = new AssetLoader<TextAsset>($"Configs/{GetType().Name}.txt");
        await textAssetLoader.AsyncLoad();
        LangCfgDataLoaded(textAssetLoader.Asset.text);
    }
    private void LangCfgDataLoaded(string d)
    {
        m_LangCfgdict = new Dictionary<int, LangCfgData>();
        m_LangCfgIDX_KEY = new Dictionary<int, int>();
        string[] allLines = d.Split('\n');
        for (int index = 0; index < allLines.Length; ++index)
        {
            string content = allLines[index].Trim();

            var strArr = content.Split("__", System.StringSplitOptions.RemoveEmptyEntries);

            if (CreateLangCfgData(strArr, out var mKey, out var mData))
            {
                if (LangCfg_HasKey)
                {
                    if (m_LangCfgdict.TryAdd(mKey, mData))
                    {
                        m_LangCfgIDX_KEY.Add(index, mKey);
                        //读取成功
                        //Debug.Log($"读取成功 {mKey} {mData}");
                    }
                    else
                    {
                        Debug.Log($"添加到容器失败 {mKey}");
                    }
                }
                else
                {
                    Debug.Log($"没有 Key 检查Excel 并定义Key");
                }

            }
            else
            {
                Debug.Log($"创建Data失败 {string.Concat(strArr)}");
            }
        }
    }
    public LangCfg.LangCfgData GetLangCfgData(int id)
    {
        return m_LangCfgdict[id];
    }
    public LangCfg(bool callInit = true)
    {
        if (callInit) _ = AsyncInitLangCfg();
    }
    public struct LangCfgData
    {
        public int ID;
        public string EN;
        public string JA;
        public string KR;
        public string ES;
        public string PT;
        public string DE;
        public string FR;
        public string RU;
    }
    protected bool CreateLangCfgData(string[] strArr, out int resultKey, out LangCfgData resultData)
    {
        resultData = new LangCfgData();
        resultData.ID = int.Parse(strArr[0]);
        resultData.EN = strArr[1];
        resultData.JA = strArr[2];
        resultData.KR = strArr[3];
        resultData.ES = strArr[4];
        resultData.PT = strArr[5];
        resultData.DE = strArr[6];
        resultData.FR = strArr[7];
        resultData.RU = strArr[8];
        resultKey = resultData.ID;
        return true;
    }
    /// <summary>
    /// 語言id
    /// </summary>
    public int GetIDByIndex(int idx) => m_LangCfgIDX_KEY[idx];
    /// <summary>
    /// 英文
    /// </summary>
    public string GetENByID(int ID) => m_LangCfgdict[ID].EN;
    /// <summary>
    /// 日本语
    /// </summary>
    public string GetJAByID(int ID) => m_LangCfgdict[ID].JA;
    /// <summary>
    /// 韩国语
    /// </summary>
    public string GetKRByID(int ID) => m_LangCfgdict[ID].KR;
    /// <summary>
    /// 西班牙语
    /// </summary>
    public string GetESByID(int ID) => m_LangCfgdict[ID].ES;
    /// <summary>
    /// 葡萄牙语
    /// </summary>
    public string GetPTByID(int ID) => m_LangCfgdict[ID].PT;
    /// <summary>
    /// 德语
    /// </summary>
    public string GetDEByID(int ID) => m_LangCfgdict[ID].DE;
    /// <summary>
    /// 法兰西语
    /// </summary>
    public string GetFRByID(int ID) => m_LangCfgdict[ID].FR;
    /// <summary>
    /// 俄语
    /// </summary>
    public string GetRUByID(int ID) => m_LangCfgdict[ID].RU;
}