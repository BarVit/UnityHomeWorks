using System.Collections.Generic;
using UnityEngine;

public class TargetGenerator : MonoBehaviour
{
    [SerializeField] private Target _prefab;

    private static int _counter = 1;

    public Target GetTarget()
    {
        string targetName = "Target";

        Target target = Instantiate(_prefab);

        target.gameObject.name = targetName + _counter++;
        target.transform.position = GetRandomPosition();

        return target;
    }

    private Vector3 GetRandomPosition()
    {
        float minNegativeX = -100f;
        float maxNegativeX = -50f;
        float minPositiveX = 50f;
        float maxPositiveX = 100f;
        float minNegativeZ = -100f;
        float maxNegativeZ = -50f;
        float minPositiveZ = 50f;
        float maxPositiveZ = 100f;
        float posY = 0.5f;

        return new Vector3(0,0,0);
    }
}