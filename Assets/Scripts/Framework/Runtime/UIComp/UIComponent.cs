using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIComponent : MonoBehaviour
{
#if UNITY_EDITOR


    public enum UIComponentType
    {
        No = 0,
        Image,
        Button,
        Text,
        TextMeshProUGUI,
        Transform,
        RectTransform,
        GameObject,
        Slider,
        InputField,
        ScrollRect,
        Dropdown,
        TabGroup,
        TextLanguagePro,
        Toggle,
        TMP_InputField,
    }

    public static int MaxUIComponentNum = (int)UIComponentType.TMP_InputField + 1;


    public int m_Component;
    public string m_Comment;


    public object GetValue()
    {

        switch (m_Component)
        {
            case (int)UIComponentType.No:
                return null;
            case (int)UIComponentType.Image:
                return gameObject.GetComponent<Image>();
            case (int)UIComponentType.Button:
                return gameObject.GetComponent<Button>();
            case (int)UIComponentType.Text:
                return gameObject.GetComponent<Text>();
            case (int)UIComponentType.TextMeshProUGUI:
                return gameObject.GetComponent<TextMeshProUGUI>();
            case (int)UIComponentType.Transform:
                return gameObject.transform;
            case (int)UIComponentType.RectTransform:
                return gameObject.GetComponent<RectTransform>();
            case (int)UIComponentType.GameObject:
                return gameObject;
            case (int)UIComponentType.Slider:
                return gameObject.GetComponent<Slider>();
            case (int)UIComponentType.InputField:
                return gameObject.GetComponent<InputField>();
            case (int)UIComponentType.ScrollRect:
                return gameObject.GetComponent<ScrollRect>();
            case (int)UIComponentType.Dropdown:
                return gameObject.GetComponent<Dropdown>();
            case (int)UIComponentType.TextLanguagePro:
                return gameObject.GetComponent<TextLanguagePro>();
            case (int)UIComponentType.TabGroup:
                return gameObject.GetComponent<TabGroup>();
            case (int)UIComponentType.Toggle:
                return gameObject.GetComponent<Toggle>();
            case (int)UIComponentType.TMP_InputField:
                return gameObject.GetComponent<TMP_InputField>();
        }
        return null;
    }
#endif
}
