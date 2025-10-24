// ReSharper disable CheckNamespace

public enum GameEvent : ushort
{
    EnterNextLevel = FrameworksMsg.Max + 1,

    RefreshCoin,
    RefreshSuperCoin,
    RefreshCards,

    MatchCntPlus,

    GamePlayStart,
    GamePlayEnd,

    CallNum,
    ValidClickNum,
    InvalidClickNum,

    RefreshUI,
    AddGameScore,
    AddChargesCount,
    OnChargesHitCardItem,
}