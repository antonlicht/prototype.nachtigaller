using UnityEngine;
using UnityEngine.EventSystems;

public class SceneNavigation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private const float NavigationScale = 0.001f;
    private const float VelocityThreshold = 0.001f;

    public CameraPath CameraPath;
    public float NavigationSpeed = 0.6f;
    public float NavigationMaxSpeed = 20f;
    public float Friction = 0.9f;

    private Vector2 _lastInput;
    private float _velocity;
    private bool _swiping;

    void Update()
    {
        if (_swiping)
        {
            Vector2 input = Input.mousePosition;
            Navigate(input - _lastInput);
            _lastInput = input;
        }
        if (Mathf.Abs(_velocity) >= VelocityThreshold)
        {
            CameraPath.Time -= _velocity;
            _velocity *= Friction;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _swiping = true;
        _lastInput = Input.mousePosition;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _swiping = false;
    }

    private void Navigate(Vector2 delta)
    {
        _velocity = Mathf.Lerp(_velocity,Mathf.Clamp(delta.x * NavigationScale * NavigationSpeed, -NavigationMaxSpeed * NavigationScale, NavigationMaxSpeed * NavigationScale),0.8f);
    }
}
