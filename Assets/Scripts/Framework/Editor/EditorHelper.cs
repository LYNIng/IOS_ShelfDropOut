
using System;
using UnityEditor;
using UnityEngine;

public class EditorHelper
{
    public static T GetOrCreateWindow<T>(string title, Action<T> onSetting = null) where T : EditorWindow
    {
        T window = EditorWindow.GetWindow<T>();
        if (window == null)
        {
            window = EditorWindow.CreateWindow<T>(title);
        }
        window.titleContent = new GUIContent(title);
        onSetting?.Invoke(window);
        return window;
    }
}
