using DG.Tweening;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIToastParam : UIData
{
    public string msg;
}

[UISetting(UICanvasLayer.System_Camera)]
public class UIToast : UIBase<UIToastParam>
{
    [SerializeField]
    private GameObject txtToast;

    public override void OnInit()
    {
        base.OnInit();

        txtToast.SetActive(true);
        var toast = txtToast.GetComponentInChildren<TextMeshProUGUI>();

        toast.text = Data.msg;
        txtToast.transform.localScale = Vector3.one;
    }

    protected override async Task Show_Internal()
    {
        txtToast.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(txtToast.GetComponent<RectTransform>());
        var seq = DOTween.Sequence();
        seq.Append(txtToast.transform.DOScale(Vector3.one, 0.2f));
        seq.Join(transform.SetFade(0f).DOFade(1f, 0.2f));
        seq.AppendInterval(0.5f);
        seq.Append(txtToast.transform.DOLocalMoveY(300f, 0.5f).SetEase(Ease.OutQuad));
        seq.AppendCallback(() => Close());

        await seq.AsyncWaitForCompletion();
    }

    protected override async Task Hide_Internal()
    {
        await transform.GetOrAddComponent<CanvasGroup>().DOFade(0, 0.2f).AsyncWaitForCompletion();
    }

    protected override void OnShowed()
    {
        Close();
    }


}