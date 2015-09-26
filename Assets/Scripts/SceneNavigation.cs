using UnityEngine;

public class SceneNavigation : MonoBehaviour
{
    private const float NavigationScale = 0.1f;
    private const float VelocityThreshold = 0.001f;

    public CameraPath CameraPath;
    public float NavigationSpeed = 0.6f;
    public float NavigationMaxSpeed = 20f;
    public float Friction = 0.9f;

    private Vector2 _pointerPosition;
    private bool _pointerDown;
    private float _velocity;

    void Update()
    {
        ProcessInput();
        UpdateCameraPath();     
    }

    private void ProcessInput()
    {
        var pointerPosition = GetPointerPosition();
        var pointerDown = GetPointerDown();

        if (pointerDown && _pointerDown)
        {
            SetVelocity(pointerPosition - _pointerPosition);
        }
        else if (pointerDown && !_pointerDown)
        {
            SetVelocity(Vector2.zero);
        }

        _pointerPosition = pointerPosition;
        _pointerDown = pointerDown;
    }

    private void UpdateCameraPath()
    {
        if (Mathf.Abs(_velocity) >= VelocityThreshold)
        {
            CameraPath.Time -= _velocity;
            _velocity *= Friction;
        }
    }

    private Vector2 GetPointerPosition()
    {
        return Input.mousePosition/Screen.dpi;
    }

    private bool GetPointerDown()
    {
        return Input.touchCount > 0 || Input.GetMouseButton(0);
    }

    private void SetVelocity(Vector2 delta)
    {
        _velocity = Mathf.Lerp(_velocity,Mathf.Clamp(delta.x * NavigationScale * NavigationSpeed, -NavigationMaxSpeed * NavigationScale, NavigationMaxSpeed * NavigationScale),0.8f);
    }
}
