using Leopotam.EcsLite;
using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

public sealed class SaveLoadSystem : IEcsInitSystem
{
    private readonly BusinessConfigSO[] _configs;
    private readonly string _path;
    private readonly RuntimeContext _context;

    public SaveLoadSystem(BusinessConfigSO[] configs, string path, RuntimeContext context)
    {
        _configs = configs;
        _path = path;
        _context = context;
    }

    void IEcsInitSystem.Init(EcsSystems systems)
    {
        Load(_context.World);
    }

    public void Save(EcsWorld world)
    {
        var save = new SaveData();

        // balance
        var balancePool = world.GetPool<BalanceComponent>();

        if (balancePool.Has(_context.BalanceEntity))
        {
            ref var bal = ref balancePool.Get(_context.BalanceEntity);
            save.Balance = bal.Amount;
        }
        else
        {
            save.Balance = 0;
        }

        // businesses
        var businessPool = world.GetPool<BusinessComponent>();
        var upgradeStatesPool = world.GetPool<UpgradeStateComponent>();

        var filter = world.Filter<BusinessComponent>().End();
        var list = new List<BusinessSave>();
        foreach (var entity in filter)
        {
            ref var businessComp = ref businessPool.Get(entity);
            var config = _configs[businessComp.ConfigIndex];

            ref var upgradeStates = ref upgradeStatesPool.Get(entity);
            var bs = new BusinessSave
            {
                BusinessKey = config.BusinessKey,
                Level = businessComp.Level,
                Progress = businessComp.Progress,
                Upgrade1Bought = upgradeStates.Upgrade1Bought,
                Upgrade2Bought = upgradeStates.Upgrade2Bought
            };
            list.Add(bs);
        }

        save.Businesses = list.ToArray();

        try
        {
            File.WriteAllText(_path, JsonUtility.ToJson(save));
        }
        catch (Exception ex)
        {
            Debug.LogError($"Save failed: {ex.Message}");
        }
    }

    public void Load(EcsWorld world)
    {
        if (!File.Exists(_path))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(_path);
            var save = JsonUtility.FromJson<SaveData>(json);
            ApplySaveToWorld(world, save);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Load failed: {ex.Message}");
        }
    }

    private void ApplySaveToWorld(EcsWorld world, SaveData save)
    {
        var balancePool = world.GetPool<BalanceComponent>();

        if (balancePool.Has(_context.BalanceEntity))
        {
            ref var balance = ref balancePool.Get(_context.BalanceEntity);
            balance.Amount = save.Balance;
        }

        var saveMap = new Dictionary<string, BusinessSave>();
        if (save.Businesses != null)
        {
            foreach (var bs in save.Businesses)
                saveMap[bs.BusinessKey] = bs;
        }

        var businessPool = world.GetPool<BusinessComponent>();
        var upgradeStatesPool = world.GetPool<UpgradeStateComponent>();
        var pricePool = world.GetPool<PriceComponent>();
        var incomePool = world.GetPool<IncomeComponent>();

        var filter = world.Filter<BusinessComponent>().End();
        foreach (var entity in filter)
        {
            ref var business = ref businessPool.Get(entity);
            ref var upgradeStates = ref upgradeStatesPool.Get(entity);
            ref var price = ref pricePool.Get(entity);
            ref var income = ref incomePool.Get(entity);

            var config = _configs[business.ConfigIndex];
            if (saveMap.TryGetValue(config.BusinessKey, out var bs))
            {
                business.Level = bs.Level;
                business.Progress = bs.Progress;
                business.Bought = bs.Level > 0;
                upgradeStates.Upgrade1Bought = bs.Upgrade1Bought;
                upgradeStates.Upgrade2Bought = bs.Upgrade2Bought;

                float multiplier = 0f;
                if (upgradeStates.Upgrade1Bought)
                    multiplier += config.Upgrade1.IncomeMultiplier;
                if (upgradeStates.Upgrade2Bought)
                    multiplier += config.Upgrade2.IncomeMultiplier;

                income.CurrentIncome = (int)(business.Level * config.BaseIncome * (1f + multiplier));
                price.NextLevelPrice = (business.Level + 1) * config.BaseCost;
            }
        }
    }
}
