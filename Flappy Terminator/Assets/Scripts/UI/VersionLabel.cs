using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class VersionLabel : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<TMP_Text>().text = "v" + Application.version;
    }
}
