using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR

public class EditorPathHelper
{
    /// <summary>
    /// 
    /// </summary>
    public static string Editor_DatasFolderPath { get; private set; }
        = $"{Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - 6))}/Datas";


    public static string Editor_ExcelCharacterFileFolderPath { get; private set; }
        = $"{Application.dataPath}/LanguageCharacter";

    /// <summary>
    /// Assets/Res/Configs
    /// </summary>
    public static string Editor_ExcelConfigsFolderPath { get; private set; }
        = $"{Application.dataPath.Substring(0, Application.dataPath.Length - 6)}/Excel/Configs";
    public static string Export_ExcelConfigsDataFolderPath { get; private set; }
        = $"{Application.dataPath}/Res/Configs";
    public static string Export_ExcelConfigsScriptsFolderPath { get; private set; }
    = $"{Application.dataPath}/Scripts/Gamework/AutoCode/Configs";
    public static string Editor_ExcelDataDefineFolderPath { get; private set; }
    = $"{Application.dataPath.Substring(0, Application.dataPath.Length - 6)}/Excel/DataDefines";
    public static string Editor_AutoCodePath { get; private set; }
        = $"{Application.dataPath}/Scripts/AutoCode";
    public static string Editor_HotUpdate_AutoCode_GenerateCodePath { get; private set; }
    = $"{Application.dataPath}/HotUpdate/AutoCode/GenerateCode";
    /// <summary>
    /// 
    /// </summary>
    public static string Editor_ResDev_DataPath { get; private set; }
    = $"{Application.dataPath}/Res_Dev/Datas";

}

#endif
