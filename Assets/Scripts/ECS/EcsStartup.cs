using UnityEngine;
using TMPro;
using Leopotam.EcsLite;
using System.IO;

public sealed class EcsStartup : MonoBehaviour
{
    [Header("Configs")]
    [SerializeField] private BusinessConfigSO[] _configs;

    [Header("UI")]
    [SerializeField] private BusinessItemView _businessItemPrefab;
    [SerializeField] private Transform _listParent;
    [SerializeField] private TMP_Text _balanceText;

    [Header("Save")]
    [SerializeField] private string _saveFileName = "clicker_save.json";
    [SerializeField] private int _startBalance = 0;

    private EcsWorld _world;
    private EcsSystems _systems;

    private RuntimeContext _context;

    private SaveLoadSystem _saveSystemInstance;

    private void Awake()
    {
        _world = new EcsWorld();
        _systems = new EcsSystems(_world);
        _context = new RuntimeContext();
    }

    private void Start()
    {
        Application.targetFrameRate = 60; // default is always 30 without it

        // balance singleton
        int balanceEntity = _world.NewEntity();
        var balancePool = _world.GetPool<BalanceComponent>();
        ref var balance = ref balancePool.Add(balanceEntity);
        balance.Amount = _startBalance;
        _context.BalanceEntity = balanceEntity;
        _context.World = _world;

        // business entities + UI
        for (int i = 0; i < _configs.Length; i++)
        {
            var config = _configs[i];
            int entity = _world.NewEntity();

            var businessPool = _world.GetPool<BusinessComponent>();
            ref var bc = ref businessPool.Add(entity);
            bc.ConfigIndex = i;
            bc.Level = (i == 0) ? 1 : 0;
            bc.Progress = 0f;
            bc.Bought = (i == 0);

            var incomePool = _world.GetPool<IncomeComponent>();
            ref var income = ref incomePool.Add(entity);
            income.CurrentIncome = bc.Level * config.BaseIncome;

            var pricePool = _world.GetPool<PriceComponent>();
            ref var price = ref pricePool.Add(entity);
            price.NextLevelPrice = (bc.Level + 1) * config.BaseCost;

            var upgradePool = _world.GetPool<UpgradeStateComponent>();
            ref var upgradeStates = ref upgradePool.Add(entity);
            upgradeStates.Upgrade1Bought = false;
            upgradeStates.Upgrade2Bought = false;

            var uiPool = _world.GetPool<UiLinkComponent>();
            ref var uiLink = ref uiPool.Add(entity);

            var v = Instantiate(_businessItemPrefab, _listParent);
            string title = config.Name;
            v.Init(_world, entity, title);
            uiLink.View = v;
        }

        _saveSystemInstance = new SaveLoadSystem(_configs, Path.Combine(Application.persistentDataPath, _saveFileName), _context);

        _systems
            .Add(new PurchaseLevelSystem(_configs, _context))
            .Add(new PurchaseUpgradeSystem(_configs, _context))
            .Add(new IncomeProgressSystem(_configs, _context))
            .Add(new UiUpdateSystem(_configs, _context, _balanceText))
            .Add(_saveSystemInstance)
            .Init();
    }

    private void Update()
    {
        _systems?.Run();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            _saveSystemInstance?.Save(_world);
        }
    }

    private void OnApplicationQuit()
    {
        _saveSystemInstance?.Save(_world);
        _systems?.Destroy();
        _world?.Destroy();
    }
}

public class RuntimeContext
{
    public EcsWorld World;
    public int BalanceEntity;
}
