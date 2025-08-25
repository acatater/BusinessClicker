using Leopotam.EcsLite;
using UnityEngine;

public sealed class IncomeProgressSystem : IEcsRunSystem
{
    private readonly BusinessConfigSO[] _configs;
    private readonly RuntimeContext _context;

    public IncomeProgressSystem(BusinessConfigSO[] configs, RuntimeContext context)
    {
        _configs = configs;
        _context = context;
    }

    public void Run(EcsSystems systems)
    {
        var world = systems.GetWorld();

        var filter = world.Filter<BusinessComponent>().Inc<IncomeComponent>().Inc<PriceComponent>().Inc<UpgradeStateComponent>().End();
        var businessPool = world.GetPool<BusinessComponent>();
        var incomePool = world.GetPool<IncomeComponent>();
        var pricePool = world.GetPool<PriceComponent>();
        var upgradeStatesPool = world.GetPool<UpgradeStateComponent>();
        var balancePool = world.GetPool<BalanceComponent>();

        foreach (var entity in filter)
        {
            ref var businessComp = ref businessPool.Get(entity);
            ref var income = ref incomePool.Get(entity);
            ref var price = ref pricePool.Get(entity);
            ref var upgradeStates = ref upgradeStatesPool.Get(entity);

            if (!businessComp.Bought || businessComp.Level <= 0)
            {
                income.CurrentIncome = 0;
                businessComp.Progress = 0f;
                continue;
            }

            var config = _configs[businessComp.ConfigIndex];
            float delay = Mathf.Max(0.001f, config.IncomeDelay);
            businessComp.Progress += Time.deltaTime / delay;

            float multiplier = 0f;
            if (upgradeStates.Upgrade1Bought)
                multiplier += config.Upgrade1.IncomeMultiplier;
            if (upgradeStates.Upgrade2Bought)
                multiplier += config.Upgrade2.IncomeMultiplier;

            income.CurrentIncome = (int)(businessComp.Level * config.BaseIncome * (1f + multiplier));

            if (businessComp.Progress >= 1f)
            {
                ref var balance = ref balancePool.Get(_context.BalanceEntity);
                balance.Amount += income.CurrentIncome;
                businessComp.Progress -= 1f;
                if (businessComp.Progress < 0f) businessComp.Progress = 0f;
            }

            price.NextLevelPrice = (businessComp.Level + 1) * config.BaseCost;
        }
    }
}
