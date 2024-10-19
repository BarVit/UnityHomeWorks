using UnityEngine;

[RequireComponent(typeof(AlarmSystem))]
public class House : MonoBehaviour
{
    private AlarmSystem _alarmPlayer;

    private void Awake()
    {
        _alarmPlayer = GetComponent<AlarmSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Thief>() != null)
            _alarmPlayer.PlaySirene();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Thief>() != null)
            _alarmPlayer.StopSirene();
    }
}