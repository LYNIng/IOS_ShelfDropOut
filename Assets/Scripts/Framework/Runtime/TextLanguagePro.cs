using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextLanguagePro : MonoBehaviour, IIMulLan
{
    public int LanguageID;

    void Awake()
    {
        if (MLangManager.Inited)
        {
            MLangManager.RegText(this);
        }
    }

    private void Start()
    {
        if (MLangManager.Inited)
            UpdateText();
    }


    void OnDestroy()
    {
        if (MLangManager.Inited)
            MLangManager.RemText(this);
    }

    public void UpdateText()
    {
        var Text = GetComponent<TextMeshProUGUI>();
        if (LanguageID == 0)
        {
            return;
        }
        if (Text == null)
        {
            Debug.Log($"{name} is null");
            return;
        }
        string str = MLangManager.GetLangStr(LanguageID);
        if (!string.IsNullOrEmpty(str))
            Text.text = str.Replace("\\n", "\n").Replace("/r/n","\n");

    }




}


