using System;

public partial class GameGlobal
{
    public int curSignIn
    {
        get
        {
            return DataManager.GetDataByInt("Asset_Signin", 0);
        }
        set
        {
            DataManager.SetDataByInt("Asset_Signin", value);
        }
    }

    public int signInLastDay
    {
        get
        {
            return DataManager.GetDataByInt("Asset_Signin_Last_Day", 0);
        }
        set
        {
            DataManager.SetDataByInt("Asset_Signin_Last_Day", value);
        }
    }
}

public class SignInManager : Singleton<SignInManager>
{
    public int GetSignInState(int inputDay)
    {
        int year = DateTime.Now.Year * 10000;
        int month = DateTime.Now.Month * 100;
        int day = DateTime.Now.Day;
        int signInDay = year + month + day;

        var _curSignIn = GameGlobal.Instance.curSignIn;
        var _signInLastDay = GameGlobal.Instance.signInLastDay;

        if (_curSignIn > inputDay)
        {
            //拿过的
            return 2;
        }
        else if (_curSignIn == inputDay && signInDay > _signInLastDay)
        {
            //能拿的
            return 1;
        }
        else
        {
            //不能拿的
            return 0;
        }
    }

    public bool CallSignIn(int signDay)
    {
        int year = DateTime.Now.Year * 10000;
        int month = DateTime.Now.Month * 100;
        int day = DateTime.Now.Day;
        int signInDay = year + month + day;

        var _curSignIn = GameGlobal.Instance.curSignIn;
        var _signInLastDay = GameGlobal.Instance.signInLastDay;


        if (signInDay > _signInLastDay && _curSignIn == signDay)
        {
            _signInLastDay = signInDay;
            _curSignIn = signDay;

            GameGlobal.Instance.curSignIn = _curSignIn + 1;
            GameGlobal.Instance.signInLastDay = _signInLastDay;

            RedPointMgr.Instance.RefreshRedPoint(RedPointMgr.IDS.RP_Task);
            return true;
        }
        return false;
    }

    public bool SignInRpShouldShow()
    {
        int year = DateTime.Now.Year * 10000;
        int month = DateTime.Now.Month * 100;
        int day = DateTime.Now.Day;
        int signInDay = year + month + day;

        //var _curSignIn = DataCtrlMgr.GetDataByInt(GameConst.DATA_ASSET_SIGNIN, 0);
        var _signInLastDay = GameGlobal.Instance.signInLastDay;

        if (signInDay > _signInLastDay)
        {
            return true;
        }

        return false;
    }
}
