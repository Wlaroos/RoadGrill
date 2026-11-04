using UnityEngine;
using UnityEngine.EventSystems;

public class BasicButton : BaseInteractiveUI
{
    public UnityEngine.Events.UnityEvent OnClick;
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        OnClick?.Invoke();
    }
}