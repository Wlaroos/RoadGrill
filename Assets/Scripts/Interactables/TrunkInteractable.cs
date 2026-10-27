using System.Collections;
using UnityEngine;

public class TrunkInteractable : Interactable
{
    [SerializeField] private float _openAngle;
    [SerializeField] private float _animationDuration = 0.5f;
    private bool _istrunkOpen = false;
    private Coroutine _animationRoutine;

    public override void Awake()
    {
        base.Awake();
    }
    public override void Interact()
    {
        base.Interact();

        Quaternion targetRotation;

        // Toggle state
        if (_istrunkOpen)
        {
            _istrunkOpen = false;
            targetRotation = Quaternion.identity;
        }
        else
        {
            _istrunkOpen = true;
            targetRotation = Quaternion.Euler(-_openAngle, 0, 0);
        }

        // Stop any ongoing animation before starting a new one
        if (_animationRoutine != null)
        {
            StopCoroutine(_animationRoutine);
        }

        // Start smooth rotation coroutine
        _animationRoutine = StartCoroutine(AnimateRotation(targetRotation));
    }

    private IEnumerator AnimateRotation(Quaternion targetRotation)
    {
        Quaternion startRotation = transform.localRotation;
        float elapsedTime = 0f;

        while (elapsedTime < _animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / _animationDuration);
            
            // Smooth step for smooth start and end
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        // Set at end
        transform.localRotation = targetRotation;
        _animationRoutine = null;
    }
}