using Leopotam.EcsLite;

public sealed class PurchaseLevelSystem : IEcsRunSystem
{
    private readonly BusinessConfigSO[] _configs;
    private readonly RuntimeContext _context;

    public PurchaseLevelSystem(BusinessConfigSO[] configs, RuntimeContext context)
    {
        _configs = configs;
        _context = context;
    }

    public void Run(EcsSystems systems)
    {
        var world = systems.GetWorld();
        var requestPool = world.GetPool<PurchaseLevelRequestComponent>();
        var filter = world.Filter<PurchaseLevelRequestComponent>().End();

        var pricePool = world.GetPool<PriceComponent>();
        var businessPool = world.GetPool<BusinessComponent>();
        var balancePool = world.GetPool<BalanceComponent>();

        foreach (var reqEntity in filter)
        {
            ref var req = ref requestPool.Get(reqEntity);
            int target = req.TargetBusinessEntity;

            if (target == 0)
            {
                world.DelEntity(reqEntity);
                continue;
            }

            ref var price = ref pricePool.Get(target);
            ref var balance = ref balancePool.Get(_context.BalanceEntity);

            if (balance.Amount >= price.NextLevelPrice && price.NextLevelPrice > 0)
            {
                balance.Amount -= price.NextLevelPrice;
                if (balance.Amount < 0) balance.Amount = 0;

                ref var businessComponent = ref businessPool.Get(target);
                businessComponent.Level++;
                businessComponent.Bought = true;

                var config = _configs[businessComponent.ConfigIndex];
                price.NextLevelPrice = (businessComponent.Level + 1) * config.BaseCost;
            }

            world.DelEntity(reqEntity);
        }
    }
}
