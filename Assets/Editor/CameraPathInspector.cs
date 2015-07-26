using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraPath))]
public class CameraPathInspector : Editor
{
    private const float HandleSize = 0.04f;
    private const float PickSize = 0.06f;

    private int _selectedIndex = -1;
    private CameraPath _spline;
    private Transform _handleTransform;
    private Quaternion _handleRotation;

    void OnEnable()
    {
        Tools.hidden = true;
    }

    void OnDisable()
    {
        Tools.hidden = false;
    }

    public override void OnInspectorGUI()
    {
        _spline = target as CameraPath;
        _selectedIndex = _spline.GetSelectedPoint(_spline.Time);
        if (_selectedIndex >= 0 && _spline.IsPathPoint(_selectedIndex))
        {
            _spline.SetPoint(_selectedIndex, _spline.transform.position);
            _spline.SetRotation(_selectedIndex, _spline.transform.rotation);
        }
        EditorGUI.BeginChangeCheck();
        _spline.Time = EditorGUILayout.Slider("Time", _spline.Time, 0f, 1f);
        if (EditorGUI.EndChangeCheck())
        {
            var selection = _spline.GetSelectedPoint(_spline.Time);
            _selectedIndex = selection;
        }

        if (GUILayout.Button("Add Point"))
        {
            Undo.RecordObjects(new Object[] { _spline.transform, _spline }, "Add Point");
            _spline.AddCurve();
            EditorUtility.SetDirty(_spline);
        }
    }


    private void OnSceneGUI()
    {
        _spline = target as CameraPath;
        _handleTransform = _spline.transform;
        _handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            _handleTransform.rotation : Quaternion.identity;

        Vector3 p0 = ShowPoint(0);
        for (int i = 1; i < _spline.ControlPointCount; i += 3)
        {
            Vector3 p1 = ShowPoint(i);
            Vector3 p2 = ShowPoint(i+1);
            Vector3 p3 = ShowPoint(i+2);

            Handles.color = Color.gray;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p2, p3);

            Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
            p0 = p3;
        }
    }

    private Vector3 ShowPoint(int index)
    {
        Vector3 point = _spline.GetPoint(index);
        float size = HandleUtility.GetHandleSize(point);
        Handles.color = _spline.IsPathPoint(index) ? Color.cyan : Color.gray;
        if (Handles.Button(point, _handleRotation, size * HandleSize, size * PickSize, Handles.DotCap))
        {
            _selectedIndex = index;
            _spline.Time = _spline.GetTime(index);
            Repaint();
        }
        if (_selectedIndex == index)
        {
            if (Tools.current == Tool.Move)
            {
                EditorGUI.BeginChangeCheck();
                point = Handles.DoPositionHandle(point, _handleRotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObjects(new Object[] { _spline.transform, _spline }, "Move Camera");
                    EditorUtility.SetDirty(_spline);
                    _spline.SetPoint(index, point);
                    _spline.Time = _spline.GetTime(index);
                }
            }
            else if (Tools.current == Tool.Rotate && _spline.IsPathPoint(_selectedIndex))
            {
                var rotation = _spline.GetRotation(index);
                EditorGUI.BeginChangeCheck();
                rotation = Handles.DoRotationHandle(rotation, point);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObjects(new Object[]{_spline.transform,_spline}, "Rotate Camera");

                    EditorUtility.SetDirty(_spline);
                    _spline.SetRotation(index, rotation);
                    _spline.Time = _spline.GetTime(index);
                }
            }
        }
        return point;
    }
}