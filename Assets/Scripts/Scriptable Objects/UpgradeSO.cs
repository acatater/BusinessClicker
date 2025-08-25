using UnityEngine;

[CreateAssetMenu(menuName = "Clicker/Upgrade")]
public class UpgradeSO : ScriptableObject
{
    public string Name => UpgradeNameSO.Name;
    public NameSO UpgradeNameSO;
    public int Price;
    [Tooltip("0.25 = 25%")]
    public float IncomeMultiplier;
}