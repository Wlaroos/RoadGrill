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

    public override void OnHover()
    {
        base.OnHover();

        _r.material = _hoverMat;
    }
    public override void OnUnhover()
    {
        base.OnUnhover();

        _r.material = _defaultMat;
    }
}