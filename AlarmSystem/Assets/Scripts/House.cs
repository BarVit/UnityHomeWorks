using UnityEngine;

[RequireComponent(typeof(Signalization))]
public class House : MonoBehaviour
{
    private Signalization _alarmPlayer;

    private void Awake()
    {
        _alarmPlayer = GetComponent<Signalization>();
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