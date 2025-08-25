using UnityEngine;

[CreateAssetMenu(menuName = "Clicker/BusinessConfig")]
public class BusinessConfigSO : ScriptableObject
{
    public string BusinessKey;
    public string Name => BusinessNameSO.Name;
    public NameSO BusinessNameSO;
    public float IncomeDelay; // seconds
    public int BaseCost;
    public int BaseIncome;
    public UpgradeSO Upgrade1;
    public UpgradeSO Upgrade2;
}
