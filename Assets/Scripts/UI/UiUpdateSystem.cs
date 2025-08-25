using Leopotam.EcsLite;
using TMPro;

public sealed class UiUpdateSystem : IEcsRunSystem
{
    private readonly BusinessConfigSO[] _configs;
    private readonly RuntimeContext _context;
    private readonly TMP_Text _balanceText;

    public UiUpdateSystem(BusinessConfigSO[] configs, RuntimeContext context, TMP_Text balanceText)
    {
        _configs = configs;
        _context = context;
        _balanceText = balanceText;
    }

    public void Run(EcsSystems systems)
    {
        var world = systems.GetWorld();

        var balancePool = world.GetPool<BalanceComponent>();

        ref var balance = ref balancePool.Get(_context.BalanceEntity);
        _balanceText.text = $"Баланс: ${balance.Amount:F0}";

        var filter = world.Filter<BusinessComponent>().Inc<IncomeComponent>().Inc<PriceComponent>().Inc<UpgradeStateComponent>().Inc<UiLinkComponent>().End();
        var businessPool = world.GetPool<BusinessComponent>();
        var incomePool = world.GetPool<IncomeComponent>();
        var pricePool = world.GetPool<PriceComponent>();
        var upgradeStatesPool = world.GetPool<UpgradeStateComponent>();
        var uiPool = world.GetPool<UiLinkComponent>();

        foreach (var entity in filter)
        {
            ref var businessComp = ref businessPool.Get(entity);
            ref var income = ref incomePool.Get(entity);
            ref var price = ref pricePool.Get(entity);
            ref var upgradeStates = ref upgradeStatesPool.Get(entity);
            ref var ui = ref uiPool.Get(entity);

            var config = _configs[businessComp.ConfigIndex];

            var upgradeInfo1 = new UpgradeInfo(config.Upgrade1.UpgradeNameSO.Name, config.Upgrade1.Price, config.Upgrade1.IncomeMultiplier, upgradeStates.Upgrade1Bought);

            var upgradeInfo2 = new UpgradeInfo(config.Upgrade2.UpgradeNameSO.Name, config.Upgrade2.Price, config.Upgrade2.IncomeMultiplier, upgradeStates.Upgrade2Bought);

            ui.View.UpdateView(businessComp.Level, businessComp.Progress, income.CurrentIncome, price.NextLevelPrice, upgradeInfo1, upgradeInfo2);
        }
    }
}

public readonly struct UpgradeInfo
{
    public readonly string Name;
    public readonly int Price;
    public readonly float Multiplier;
    public readonly bool Bought;

    public UpgradeInfo(string name, int price, float multiplier, bool bought)
    {
        Name = name;
        Price = price;
        Multiplier = multiplier;
        Bought = bought;
    }
}
