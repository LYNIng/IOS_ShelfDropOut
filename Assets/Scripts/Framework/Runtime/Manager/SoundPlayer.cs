using UnityEngine;

public enum SoundName
{
    Bgm = 0,
    Click,
    Comp,
    Cp,
    Pop,
    Succ,
    Fail
}
/// <summary>
/// AudioCtrlMgr
/// </summary>
public class SoundPlayer : MonoSingleton<SoundPlayer>
{
    public override bool DontDestory => true;

    private AudioSource _curBGMAudoiSource;

    public void PlaySound(SoundName eSound)
    {
        Debug.Log($"call Play {eSound}");
        if (transform.childCount <= (int)eSound) return;
        if (eSound == SoundName.Bgm )
        {

            if (_curBGMAudoiSource != null)
            {
                _curBGMAudoiSource.Stop();
                _curBGMAudoiSource = null;
            }
            if (GameGlobal.Instance.MusicIsEnable)
            {
                var audios = this.transform.GetChild((int)eSound).GetComponent<AudioSource>();
                audios.Play();
                _curBGMAudoiSource = audios;
            }
            else
            {
                this.transform.GetChild((int)eSound).GetComponent<AudioSource>().Stop();
            }
            return;
        }
        if (!GameGlobal.Instance.SoundIsEnable)
        {
            return;
        }
        this.transform.GetChild((int)eSound).GetComponent<AudioSource>().Play();
    }

    public void StopSound(SoundName eSound)
    {
        this.transform.GetChild((int)eSound).GetComponent<AudioSource>().Stop();
    }



}
