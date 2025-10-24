using System.Collections.Generic;
using UnityEngine;


public static class DataManager
{
    public static void SetDataByBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }
    public static bool GetDataByBool(string key, bool defaultValue = false)
    {
        return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
    }
    public static int GetDataByInt(string key, int value = 0)
    {
        return PlayerPrefs.GetInt(key, value);
    }
    public static void SetDataByInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }

    public static float GetDataByFloat(string key, float value = 0)
    {
        return PlayerPrefs.GetFloat(key, value);
    }

    public static void SetDataByFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
    }

    public static string GetDataByString(string key)
    {
        return PlayerPrefs.GetString(key, string.Empty);
    }

    public static void SetDataByString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }

}
