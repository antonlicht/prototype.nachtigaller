using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraPath : MonoBehaviour
{
    [SerializeField]
    private Vector3[] _points;

    [SerializeField] 
    private Quaternion[] _rotations;

    [SerializeField]
    private float _time;
    public float Time
    {
        get { return _time; }
        set
        {
            _time = Mathf.Clamp01(value);
            SetCameraPosition();
            SetCameraRotation();
        }
    }

    public int ControlPointCount
    {
        get
        {
            return _points.Length;
        }
    }

    void Update()
    {
        SetCameraPosition();
        SetCameraRotation();
    }

    public void SetCameraPosition()
    {
        if (enabled)
        {
            transform.position = GetPoint(_time);
        }
    }

    public void SetCameraRotation()
    {
        if (enabled)
        {
            transform.rotation = GetRotation(_time);
        }
    }

    public Vector3 GetPoint(int index)
    {
        return _points[index];
    }


    public void SetPoint(int index, Vector3 point)
    {
        if (IsPathPoint(index))
        {
            Vector3 delta = point - _points[index];
            if (index > 0)
            {
                _points[index - 1] += delta;
            }
            if (index + 1 < _points.Length)
            {
                _points[index + 1] += delta;
            }
        }
        _points[index] = point;

        EnforceMode(index);
    }
    public Quaternion GetRotation(int index)
    {
        return _rotations[GetPathPoint(index)/3];
    }

    public void SetRotation(int index, Quaternion rotation)
    {
        if (IsPathPoint(index))
        {
            _rotations[index/3] = rotation;
        }
    }

    public bool IsPathPoint(int index)
    {
        return index % 3 == 0;
    }

    public int GetSelectedPoint(float time)
    {
        var floatIndex = time * CurveCount * 3;
        var intIndex = (int) floatIndex;
        if (Math.Abs(floatIndex - intIndex) <= 0f)
        {
            return intIndex;
        }
        return -1;
    }

    public int GetPathPoint(int index)
    {
        if (index%3 <= 1)
            return (index/3)*3;
        return index + 1;
    }

    private void EnforceMode(int index)
    {
        int modeIndex = (index + 1) / 3;
        if (modeIndex == 0 || modeIndex == (_points.Length - 1)/3)
        {
            return;
        }

        int middleIndex = modeIndex * 3;
        int fixedIndex, enforcedIndex;
        if (index <= middleIndex)
        {
            fixedIndex = middleIndex - 1;
            enforcedIndex = middleIndex + 1;
        }
        else
        {
            fixedIndex = middleIndex + 1;
            enforcedIndex = middleIndex - 1;
        }

        Vector3 middle = _points[middleIndex];
        Vector3 enforcedTangent = middle - _points[fixedIndex];
        _points[enforcedIndex] = middle + enforcedTangent;
    }

    void Reset()
    {
        var offset = new Vector3(1f, 0f, 0f);
        _time = 0;

        _points = new Vector3[] {
			transform.position,
			transform.position+offset,
			transform.position+2f*offset,
			transform.position+3*offset
		};

        _rotations = new Quaternion[]
        {
            transform.rotation,
            transform.rotation
        };
    }

    public int CurveCount
    {
        get
        {
            return (_points.Length - 1) / 3;
        }
    }

    public void AddCurve()
    {
        Vector3 point = _points[_points.Length - 1];
        Array.Resize(ref _points, _points.Length + 3);
        point.x += 1f;
        _points[_points.Length - 3] = point;
        point.x += 1f;
        _points[_points.Length - 2] = point;
        point.x += 1f;
        _points[_points.Length - 1] = point;

        Array.Resize(ref _rotations, _rotations.Length + 1);
        _rotations[_rotations.Length - 1] = _rotations[_rotations.Length - 2];

        EnforceMode(_points.Length - 4);
        _time = _time*(CurveCount - 1)/CurveCount;
    }

    public Vector3 GetPoint(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = _points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i =  Mathf.FloorToInt(t);
            t -= i;
            i *= 3;
        }
        return Bezier.GetPoint( _points[i], _points[i + 1], _points[i + 2], _points[i + 3], t);
    }

    public Quaternion GetRotation(float t)
    {
        int i;
        if (t >= 1f)
        {
            return _rotations[_rotations.Length - 1];
        }
        t = Mathf.Clamp01(t) * CurveCount;
        i = Mathf.FloorToInt(t);
        t -= i;
        return Quaternion.Lerp(_rotations[i],_rotations[i+1],t);
    }

    public float GetTime(int index)
    {
        return index/(float)(CurveCount*3);
    }

    public Vector3 GetVelocity(float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = _points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return Bezier.GetFirstDerivative(
            _points[i], _points[i + 1], _points[i + 2], _points[i + 3], t);
    }

    public Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }
}