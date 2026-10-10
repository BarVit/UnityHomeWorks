using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float _hoverScale = 1.06f;
    [SerializeField] private float _speed = 10f;

    private Selectable _selectable;
    private float _target = 1f;

    private void Awake()
    {
        _selectable = GetComponent<Selectable>();
    }

    private void Update()
    {
        float scale = Mathf.Lerp(transform.localScale.x, _target, _speed * Time.unscaledDeltaTime);

        transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void OnDisable()
    {
        _target = 1f;
        transform.localScale = Vector3.one;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_selectable == null || _selectable.IsInteractable())
            _target = _hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _target = 1f;
    }
}
