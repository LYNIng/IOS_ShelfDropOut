using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIComponent))]
public class UIComponentEditor : Editor
{
    public const string ExtendFileName = "_";

    #region Static

    [UnityEditor.Callbacks.DidReloadScripts(0)]
    static void OnScriptReload()
    {
        s_OnScriptReload = true;
    }


    [MenuItem("GameObject/Add Bind &b", false, -1)]
    public static void AddBind()
    {
        foreach (var o in Selection.objects.OfType<GameObject>())
        {
            if (o)
            {
                var uiMark = o.GetComponent<UIComponent>();

                if (!uiMark)
                {
                    o.AddComponent<UIComponent>();
                }

            }
        }
    }

    #endregion

    string path = "/Scripts/Gamework/AutoCode/UICode";
    UIComponent m_UIBind;
    UIBase m_UIPanelBase;
    List<UIComponent> m_List;

    private List<UIComponent> GetUIComponents()
    {
        if (m_List == null)
            m_List = new List<UIComponent>();
        else
            m_List.Clear();

        UIComponent[] ur = m_UIPanelBase.GetComponentsInChildren<UIComponent>(true);
        for (int i = 0; i < ur.Length; i++)
            m_List.Add(ur[i]);
        return m_List;
    }

    static bool s_OnScriptReload = false;

    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
        {
            GUILayout.Label("只在编辑时运行");
            return;
        }
        if (target == null) return;

        m_UIBind = target as UIComponent;

        m_UIPanelBase = GetUIPanelBase(m_UIBind.transform);

        if (m_UIPanelBase == null)
        {
            GUILayout.Label($"这是一个UI组件,需要父节点拥有继承自{typeof(UIBase)}的组件才能起作用.");
            return;
        }
        EditorGUI.BeginChangeCheck();
        string[] strs = GetUIComponentStrs();

        m_UIBind.m_Component = EditorGUILayout.Popup("组件类型:", m_UIBind.m_Component, strs);

        GUILayout.BeginHorizontal();


        m_UIBind.m_Comment = EditorGUILayout.TextField(m_UIBind.m_Comment, GUILayout.Height(20));

        if (GUILayout.Button("生成代码文件", GUILayout.Height(20), GUILayout.Width(80)))
        {

            string fileName = m_UIPanelBase.GetType().Name;
            CreateCode(path, fileName, m_UIPanelBase.GetType());
        }

        GUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }

        if (s_OnScriptReload)
        {
            GUIComponentSetValue();
        }

    }


    private UIBase GetUIPanelBase(Transform tran)
    {
        if (tran.parent == null)
            return null;

        UIBase uiPanel = tran.parent.GetComponent<UIBase>();
        if (uiPanel != null)
        {
            return uiPanel;
        }
        else
        {
            return GetUIPanelBase(tran.parent);
        }
    }

    private string[] GetUIComponentStrs()
    {
        string[] str = new string[UIComponent.MaxUIComponentNum];
        for (int i = 0; i < str.Length; i++)
        {
            str[i] = ((UIComponent.UIComponentType)i).ToString();
        }
        return str;
    }

    private void GetItemCode(int uIComponent, StringBuilder sbr)
    {
        List<UIComponent> tempList = new List<UIComponent>();
        var list = GetUIComponents();
        foreach (var item in list)
        {
            if (item.m_Component == uIComponent)
            {
                tempList.Add(item);
            }
        }

        if (tempList.Count > 0)
        {
            foreach (var item in tempList)
            {
                if (uIComponent != 0)
                {
                    sbr.Append("    /// <summary>\r\n");
                    sbr.Append($"    /// {item.m_Comment}\r\n");
                    sbr.Append("    /// <summary>\r\n");
                    sbr.Append($"    public {((UIComponent.UIComponentType)uIComponent).ToString()} {item.gameObject.name};\r\n");
                    sbr.Append("\r\n");
                }
            }
        }
    }

    void CreateCode(string filePath, string fileName, Type uiType)
    {
        StringBuilder sbr = new StringBuilder();
        sbr.Append("\r\n");
        sbr.Append("//===================================================\r\n");
        sbr.AppendFormat("//创建时间：{0}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        sbr.Append("//备    注：此代码为工具生成 请勿手工修改\r\n");
        sbr.Append("//===================================================\r\n");
        sbr.Append("using UnityEngine;\r\n");
        sbr.Append("using UnityEngine.UI;\r\n");
        sbr.Append("using TMPro;\r\n");
        sbr.Append("\r\n");
        sbr.Append("/// <summary>\r\n");
        sbr.AppendFormat("/// {0}\r\n", fileName);
        sbr.Append("/// </summary>\r\n");
        if (uiType.BaseType.IsGenericType)
        {
            sbr.AppendFormat("public partial class {0} : UIBase<{1}>\r\n", fileName, uiType.BaseType.GetGenericArguments()[0]);
        }
        else
            sbr.AppendFormat("public partial class {0} : UIBase\r\n", fileName);
        sbr.Append("{\r\n");
        sbr.Append("\r\n");
        sbr.Append($"    public const string UIName = \"{fileName}\";\r\n");
        sbr.Append("\r\n");

        for (int i = 0; i < UIComponent.MaxUIComponentNum; i++)
            GetItemCode(i, sbr);

        sbr.Append("}\r\n");
        string folderPath = Application.dataPath + $"{filePath}";

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        using (FileStream fs = new FileStream(Application.dataPath + string.Format("/{0}/{1}{2}.cs", filePath, fileName, ExtendFileName), FileMode.Create))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(sbr.ToString());
            }
        }

        AssetDatabase.Refresh();
    }


    void GUIComponentSetValue()
    {

        if (m_UIPanelBase != null)
        {
            var panel = m_UIPanelBase.GetType();
            for (int i = 1; i < UIComponent.MaxUIComponentNum; i++)
            {

                List<UIComponent> tempList = new List<UIComponent>();
                var list = GetUIComponents();
                foreach (var item in list)
                {
                    if (item.m_Component == i)
                    {
                        tempList.Add(item);
                    }
                }

                if (tempList.Count > 0)
                {
                    foreach (var item in tempList)
                    {
                        var property = panel.GetField(item.gameObject.name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (property != null)
                        {
                            try
                            {
                                property.SetValue(m_UIPanelBase, item.GetValue());
                            }
                            catch (Exception ex)
                            {
                                Debug.Log($"{item.gameObject.name} | {item.GetValue()}");
                                Debug.LogException(ex);
                            }
                        }
                    }
                }
            }
        }
    }
}
