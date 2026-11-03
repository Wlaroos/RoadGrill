using UnityEngine;

public class ChopTableInteractable : Interactable
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
    }

    public override void RaycastHover()
    {
        base.RaycastHover();

        _r.material = _hoverMat;
    }
    public override void RaycastUnhover()
    {
        base.RaycastUnhover();

        _r.material = _defaultMat;
    }
}