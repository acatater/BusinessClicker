using Leopotam.EcsLite;

public sealed class PurchaseUpgradeSystem : IEcsRunSystem
{
    private readonly BusinessConfigSO[] _configs;
    private readonly RuntimeContext _context;

    public PurchaseUpgradeSystem(BusinessConfigSO[] configs, RuntimeContext context)
    {
        _configs = configs;
        _context = context;
    }

    public void Run(EcsSystems systems)
    {
        var world = systems.GetWorld();
        var requestsPool = world.GetPool<PurchaseUpgradeRequestComponent>();
        var filter = world.Filter<PurchaseUpgradeRequestComponent>().End();

        var upgradeStatesPool = world.GetPool<UpgradeStateComponent>();
        var businessPool = world.GetPool<BusinessComponent>();
        var balancePool = world.GetPool<BalanceComponent>();

        foreach (var reqEntity in filter)
        {
            ref var req = ref requestsPool.Get(reqEntity);
            int target = req.TargetBusinessEntity;
            int idx = req.UpgradeIndex;

            if (target == 0)
            {
                world.DelEntity(reqEntity);
                continue;
            }

            ref var businessComp = ref businessPool.Get(target);
            var config = _configs[businessComp.ConfigIndex];
            ref var upgradeStates = ref upgradeStatesPool.Get(target);
            ref var balance = ref balancePool.Get(_context.BalanceEntity);

            if (idx == 1 && !upgradeStates.Upgrade1Bought)
            {
                if (balance.Amount >= config.Upgrade1.Price)
                {
                    balance.Amount -= config.Upgrade1.Price;
                    upgradeStates.Upgrade1Bought = true;
                }
            }
            else if (idx == 2 && !upgradeStates.Upgrade2Bought)
            {
                if (balance.Amount >= config.Upgrade2.Price)
                {
                    balance.Amount -= config.Upgrade2.Price;
                    upgradeStates.Upgrade2Bought = true;
                }
            }

            world.DelEntity(reqEntity);
        }
    }
}
