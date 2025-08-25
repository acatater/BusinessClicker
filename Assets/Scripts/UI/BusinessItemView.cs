using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Leopotam.EcsLite;

public class BusinessItemView : MonoBehaviour
{
    [Header("UI refs")]
    [SerializeField] private TMP_Text _title;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _incomeText;
    [SerializeField] private TMP_Text _lvlUpText;
    [SerializeField] private Image _progress;
    [SerializeField] private Button _buyLevelButton;
    [SerializeField] private Button _upgrade1Button;
    [SerializeField] private Button _upgrade2Button;
    [SerializeField] private TMP_Text _upgrade1Text;
    [SerializeField] private TMP_Text _upgrade2Text;

    private EcsWorld _world;
    private int _businessEntity;

    public void Init(EcsWorld world, int businessEntity, string title)
    {
        _world = world;
        _businessEntity = businessEntity;

        _title.text = title;

        _buyLevelButton.onClick.RemoveAllListeners();
        _buyLevelButton.onClick.AddListener(OnBuyLevelClicked);

        _upgrade1Button.onClick.RemoveAllListeners();
        _upgrade1Button.onClick.AddListener(OnUpgrade1Clicked);

        _upgrade2Button.onClick.RemoveAllListeners();
        _upgrade2Button.onClick.AddListener(OnUpgrade2Clicked);
    }

    private void OnBuyLevelClicked()
    {
        var reqEntity = _world.NewEntity();
        var pool = _world.GetPool<PurchaseLevelRequestComponent>();
        ref var req = ref pool.Add(reqEntity);
        req.TargetBusinessEntity = _businessEntity;
    }

    private void OnUpgrade1Clicked()
    {
        var reqEntity = _world.NewEntity();
        var pool = _world.GetPool<PurchaseUpgradeRequestComponent>();
        ref var req = ref pool.Add(reqEntity);
        req.TargetBusinessEntity = _businessEntity;
        req.UpgradeIndex = 1;
    }

    private void OnUpgrade2Clicked()
    {
        var reqEntity = _world.NewEntity();
        var pool = _world.GetPool<PurchaseUpgradeRequestComponent>();
        ref var req = ref pool.Add(reqEntity);
        req.TargetBusinessEntity = _businessEntity;
        req.UpgradeIndex = 2;
    }

    public void UpdateView(int level, float progress, float income, int nextPrice, UpgradeInfo upgradeInfo1, UpgradeInfo upgradeInfo2)
    {
        _levelText.text = $"Lvl\n{level}";
        _progress.fillAmount = progress;
        _incomeText.text = $"Доход\n${income:F0}";
        _lvlUpText.text = nextPrice > 0 ? "LVL UP\n" + $"Цена: ${nextPrice}" : "—";

        // upgrade 1
        {
            _upgrade1Button.interactable = !upgradeInfo1.Bought;
            string text = $"{upgradeInfo1.Name}\nДоход: +{upgradeInfo1.Multiplier * 100f:F0}%";

            if (upgradeInfo1.Bought)
            {
                text += "\nКуплено";
            }
            else
            {
                text += $"\nЦена: ${upgradeInfo1.Price}";
            }

            _upgrade1Text.text = text;
        }

        // upgrade 2
        {
            _upgrade2Button.interactable = !upgradeInfo2.Bought;
            string text = $"{upgradeInfo2.Name}\nДоход: +{upgradeInfo2.Multiplier * 100f:F0}%";

            if (upgradeInfo2.Bought)
            {
                text += "\nКуплено";
            }
            else
            {
                text += $"\nЦена: ${upgradeInfo2.Price}";
            }

            _upgrade2Text.text = text;
        }
    }
}
