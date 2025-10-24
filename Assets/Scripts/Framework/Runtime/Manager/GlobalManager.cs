using System;
using System.Threading.Tasks;
using UnityEngine;

public enum GameAssetType : byte
{
    Coin,
    SuperCoin,
    Cards,
    SevenIcon,
    Star,
    Box,
    VCard,
}

public partial class GameGlobal : Singleton<GameGlobal>
{
    public int Cards
    {
        get
        {
            return DataManager.GetDataByInt("Cards", 120);
        }
        private set
        {
            DataManager.SetDataByInt("Cards", value);

        }
    }
    #region Coin


    public int Coin
    {
        get
        {
            return DataManager.GetDataByInt("Coin", 0);
        }
        private set
        {
            DataManager.SetDataByInt("Coin", value);

        }
    }

    public int PeekCoinValue(int level = 0)
    {
        if (level <= 0)
        {
            return RandomHelp.RandomRange(5, 10);
        }
        else if (level == 1)
        {

            return RandomHelp.RandomRange(10, 30);
        }
        else
        {
            return RandomHelp.RandomRange(30, 60);
        }
    }

    public bool TryCostCoin(int value)
    {
        if (Coin >= value)
        {
            Coin -= value;
            return true;
        }
        return false;
    }

    #endregion

    public int SuperCoin
    {
        get
        {
            return DataManager.GetDataByInt("SuperCoin", 0);
        }
        private set
        {
            DataManager.SetDataByInt("SuperCoin", value);
        }
    }

    public int MatchCnt
    {
        get
        {
            return DataManager.GetDataByInt("MatchCnt", 0);
        }
        set
        {
            DataManager.SetDataByInt("MatchCnt", value);
            RedPointMgr.Instance.RefreshRedPoint(RedPointMgr.IDS.RP_Task);
            MessageDispatch.CallMessageCommand((ushort)GameEvent.MatchCntPlus);
        }
    }

    /// <summary>
    /// 获取SC次数
    /// </summary>
    public int GetSuperCoinCount
    {
        get
        {
            return DataManager.GetDataByInt("GetSuperCount", 0);
        }
        set
        {
            DataManager.SetDataByInt("GetSuperCount", value);
        }
    }

    public bool IsGameRunning
    {
        get;
        set;
    }

    public bool IsGuiding
    {
        get;
        set;
    }

    public bool IsFirstPlayGame
    {
        get
        {
            return DataManager.GetDataByBool("IsFristPlayGame", true);
        }
        set
        {
            DataManager.SetDataByBool("IsFristPlayGame", value);
        }
    }


    protected override void OnAwake()
    {
        base.OnAwake();

    }

    public string LoginAddress
    {
        get
        {
            return DataManager.GetDataByString("LoginAddress");
        }
        set
        {
            DataManager.SetDataByString("LoginAddress", value);
        }
    }

    public string LoginNumber
    {
        get
        {
            return DataManager.GetDataByString("LoginNumber");
        }
        set
        {
            DataManager.SetDataByString("LoginNumber", value);
        }
    }

    public int GoldVCardCount
    {
        get
        {
            return DataManager.GetDataByInt("GoldVCardCount");
        }
        set
        {
            DataManager.SetDataByInt("GoldVCardCount", value);
        }
    }

    #region GameSetting

    public bool VibrateIsEnable
    {
        get
        {
            return DataManager.GetDataByBool("VibrateIsEnable", true);
        }
        set
        {
            DataManager.SetDataByBool("VibrateIsEnable", value);
        }
    }

    public bool MusicIsEnable
    {
        get
        {
            return DataManager.GetDataByBool("MusicIsEnable", true);
        }
        set
        {
            DataManager.SetDataByBool("MusicIsEnable", value);
        }
    }

    public bool SoundIsEnable
    {
        get
        {
            return DataManager.GetDataByBool("SoundIsEnable", true);
        }
        set
        {
            DataManager.SetDataByBool("SoundIsEnable", value);
        }
    }

    #endregion

    #region SuperCoin
    public int PeekSuperCoinValue()
    {
        //int t = 50000;
        //int exVal = SuperCoin % t;
        //var tEsp = GameGlobal.Instance.TreableSuperCoinSwitch ? 3 : 1;
        //if (exVal < 10000)
        //{
        //    return RandomHelp.RandomRange(3000, 3501) * tEsp;
        //}
        //else if (exVal < 25000)
        //{
        //    return RandomHelp.RandomRange(1300, 1501) * tEsp;
        //}
        //else if (exVal < 35000)
        //{
        //    return RandomHelp.RandomRange(800, 1001) * tEsp;
        //}
        //else if (exVal < 42000)
        //{
        //    return RandomHelp.RandomRange(300, 501) * tEsp;
        //}
        //else if (exVal < 48000)
        //{
        //    return RandomHelp.RandomRange(200, 301) * tEsp;
        //}
        //else if (exVal < t)
        //{
        //    return RandomHelp.RandomRange(10, 101) * tEsp;
        //}
        //return 10;

        int resultValue = 0;
        switch (GetSuperCoinCount)
        {
            case 1:
                resultValue = 3000;
                break;
            case 2:
                resultValue = RandomHelp.RandomRange(1000, 1600);
                break;
            case 3:
                resultValue = RandomHelp.RandomRange(1000, 1600);
                break;
            default:
                resultValue = GetSuperCoinCount % 12 == 0 ? RandomHelp.RandomRange(550, 1060) : RandomHelp.RandomRange(300, 560);
                break;
        }


        return resultValue;
    }

    public static int PeekLuckyCardSuperCoinValue()
    {
        return RandomHelp.RandomRange(300, 888);
    }

    public static int PeekLuckyCardCoinValue()
    {
        return RandomHelp.RandomRange(10, 50);
    }

    public void CostSuperCoin(int val)
    {
        SuperCoin -= val;
        MessageDispatch.CallMessageCommand((ushort)GameEvent.RefreshSuperCoin);
    }


    #endregion

    #region Reward

    public void GetReward(GameAssetType gameAsset, int value, bool playEf = false, Vector3? efPos = null)
    {
        Debug.Log($"onGetReward {gameAsset} - {value}");
        if (gameAsset == GameAssetType.Coin)
        {
            Coin += value;
            MessageDispatch.CallMessageCommand((ushort)GameEvent.RefreshCoin);

        }
        else if (gameAsset == GameAssetType.SuperCoin)
        {
            SuperCoin += value;
            GetSuperCoinCount++;
            MessageDispatch.CallMessageCommand((ushort)GameEvent.RefreshSuperCoin);

        }
        else if (gameAsset == GameAssetType.Cards)
        {
            Cards += value;
            MessageDispatch.CallMessageCommand((ushort)GameEvent.RefreshCards);
        }
    }

    public async Task AsyncGetReward(GameAssetType gameAsset, int value, bool playEf = false, Vector3? efPos = null)
    {
        if (gameAsset == GameAssetType.Coin)
        {
            Coin += value;
            await MessageDispatch.AsyncCallMessageCommand((ushort)GameEvent.RefreshCoin);
        }
        else if (gameAsset == GameAssetType.SuperCoin)
        {
            SuperCoin += value;
            await MessageDispatch.AsyncCallMessageCommand((ushort)GameEvent.RefreshSuperCoin);
        }
        else if (gameAsset == GameAssetType.Cards)
        {
            Cards += value;
            await MessageDispatch.AsyncCallMessageCommand((ushort)GameEvent.RefreshCards);
        }
    }

    #endregion

    #region Items
    public async Task CostAsset(GameAssetType assetType, int value)
    {
        if (assetType == GameAssetType.Coin)
        {
            GameGlobal.Instance.Coin -= value;
            MessageDispatch.CallMessageCommand((ushort)GameEvent.RefreshCoin);

        }
        else if (assetType == GameAssetType.SuperCoin)
        {
            GameGlobal.Instance.SuperCoin -= value;
            MessageDispatch.CallMessageCommand((ushort)GameEvent.RefreshSuperCoin);

        }
        else if (assetType == GameAssetType.Cards)
        {
            GameGlobal.Instance.Cards -= value;
            await MessageDispatch.AsyncCallMessageCommand((ushort)GameEvent.RefreshCards);
        }
    }

    #endregion

    #region Tools

    #endregion

    #region Channel
    public int CAChannel
    {
        get
        {
            return DataManager.GetDataByInt("CAChannel");
        }
        set
        {
            DataManager.SetDataByInt("CAChannel", value);
        }
    }

    #endregion
}

public partial class GameGlobalAsset : MonoSingleton<GameGlobalAsset>, IManager, IManagerInit
{
    public static Transform GlobalPartent => Instance.transform;

    public static ParticlePool UIWavePortalBlueParticle { get; private set; }
    public static ParticlePool UIChargeParticle { get; private set; }
    public static ParticlePool UINumToChargeParticle { get; private set; }

    public static ParticlePool UIStarPoofParticle { get; private set; }

    //public static ParticlePool UIPowerupActivateBlue { get; private set; }
    public static ParticlePool Sparkle { get; private set; }
    public static ParticlePool UISwordHitMagicBlue { get; private set; }

    public static ParticlePool UIWow { get; private set; }
    public static ParticlePool UISparkle_Yellow { get; private set; }
    public static ParticlePool UIExCoin { get; private set; }
    public static ParticlePool UIBackGroundParticleMore { get; private set; }
    public static ParticlePool UIPop { get; private set; }

    public override bool DontDestory => true;

    public async Task<bool> AsyncInit()
    {
        UIWavePortalBlueParticle = new ParticlePool("Prefabs/Particles/UIWavePortalBlue.prefab", false);


        UINumToChargeParticle = new ParticlePool("Prefabs/Particles/UINumToChargeParticle.prefab", false);
        UINumToChargeParticle.SetLayerSort(UICanvasLayer.Main_Camera, 2);

        UIChargeParticle = new ParticlePool("Prefabs/Particles/UIChargeParticle.prefab", false);
        UIChargeParticle.SetLayerSort(UICanvasLayer.Main_Camera, 2);

        UIStarPoofParticle = new ParticlePool("Prefabs/Particles/UIStarPoof.prefab", false);
        UIStarPoofParticle.SetLayerSort(UICanvasLayer.Main_Camera, 3);

        Sparkle = new ParticlePool("Prefabs/Particles/Sparkle.prefab", false);
        Sparkle.SetLayerSort(UICanvasLayer.Main_Camera, 2);

        //UIPowerupActivateBlue = new ParticlePool("Prefabs/Particles/UIPowerupActivateBlue.prefab");

        UISwordHitMagicBlue = new ParticlePool("Prefabs/Particles/UISwordHitMagicBlue.prefab");

        UIWow = new ParticlePool("Prefabs/Particles/UIWow.prefab");

        UISparkle_Yellow = new ParticlePool("Prefabs/Particles/UISparkle_Yellow.prefab");

        UIExCoin = new ParticlePool("Prefabs/Particles/UIExCoin.prefab");

        UIBackGroundParticleMore = new ParticlePool("Prefabs/Particles/UIBackGroundParticleMore.prefab");
        UIBackGroundParticleMore.AutoReleaseTime = 4f;

        UIPop = new ParticlePool("Prefabs/Particles/UIPopParticle.prefab");

        await Task.WhenAll(
            UIWavePortalBlueParticle.AsyncInit(),
            UIChargeParticle.AsyncInit(),
            UINumToChargeParticle.AsyncInit(),
            UIStarPoofParticle.AsyncInit(),
            Sparkle.AsyncInit(),
            UISwordHitMagicBlue.AsyncInit()
            );

        return true;
    }
}
