using UnityEngine;

public class ShopInteractable : Interactable
{
    [SerializeField] private Material _defaultMat;
    [SerializeField] private Material _hoverMat;
    private Renderer _r;

    public override void Awake()
    {
        base.Awake();
        _r = GetComponent<Renderer>();
    }

    public override void Interact()
    {
        base.Interact();
        PlayerInventory.Instance?.OpenInventory(InvType.Shop);
    }

    public override void RaycastHover()
    {
        base.RaycastHover();
        if (_r != null) _r.material = _hoverMat;
    }

    public override void RaycastUnhover()
    {
        base.RaycastUnhover();
        if (_r != null) _r.material = _defaultMat;
    }
}