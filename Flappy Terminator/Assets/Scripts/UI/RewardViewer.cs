using UnityEngine;

public class RewardViewer : MonoBehaviour
{
    [SerializeField] private LocationRunner _runner;
    [SerializeField] private CompactNumber _xp;
    [SerializeField] private CompactNumber _money;

    private void OnEnable()
    {
        _runner.RewardChanged += Show;
    }

    private void Start()
    {
        Show(_runner.LocationReward);
    }

    private void OnDisable()
    {
        _runner.RewardChanged -= Show;
    }

    private void Show(Reward reward)
    {
        _xp.Show(reward.Xp);
        _money.Show(reward.Money);
    }
}
