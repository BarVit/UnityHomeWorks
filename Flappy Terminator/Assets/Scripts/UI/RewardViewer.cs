using TMPro;
using UnityEngine;

public class RewardViewer : MonoBehaviour
{
    [SerializeField] private LocationRunner _runner;
    [SerializeField] private TMP_Text _xp;
    [SerializeField] private TMP_Text _money;

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
        _xp.text = reward.Xp.ToString();
        _money.text = reward.Money.ToString();
    }
}
