using UnityEngine;

public class RewardViewer : MonoBehaviour
{
    [SerializeField] private LocationRunner _runner;
    [SerializeField] private Economy _economy;
    [SerializeField] private CompactNumber _xp;
    [SerializeField] private CompactNumber _money;

    private void OnEnable()
    {
        _runner.RewardChanged += OnRewardChanged;
        _economy.Changed += Show;
    }

    private void Start()
    {
        Show();
    }

    private void OnDisable()
    {
        _runner.RewardChanged -= OnRewardChanged;
        _economy.Changed -= Show;
    }

    private void OnRewardChanged(Reward reward)
    {
        Show();
    }

    private void Show()
    {
        Reward pending = _runner.PendingReward;

        _xp.Show(_economy.TotalXp + pending.Xp);
        _money.Show(_economy.Money + pending.Money);
    }
}
