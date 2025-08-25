public struct BusinessComponent
{
    public int ConfigIndex;
    public int Level;
    public float Progress;
    public bool Bought;
}

public struct IncomeComponent
{
    public int CurrentIncome;
}

public struct PriceComponent
{
    public int NextLevelPrice;
}

public struct UpgradeStateComponent
{
    public bool Upgrade1Bought;
    public bool Upgrade2Bought;
}

public struct UiLinkComponent
{
    public BusinessItemView View;
}

public struct BalanceComponent
{
    public int Amount;
}

public struct PurchaseLevelRequestComponent
{
    public int TargetBusinessEntity;
}

public struct PurchaseUpgradeRequestComponent
{
    public int TargetBusinessEntity;
    public int UpgradeIndex;
}
