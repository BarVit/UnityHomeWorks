using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class TrailReset : MonoBehaviour
{
    private TrailRenderer _trail;

    private void Awake()
    {
        _trail = GetComponent<TrailRenderer>();
    }

    private void OnEnable()
    {
        _trail.Clear();
    }
}
