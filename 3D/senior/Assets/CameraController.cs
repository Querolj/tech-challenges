using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    /// <summary>
    /// Log to display result of tests
    /// </summary>
    [SerializeField] private Text _log;

    /// <summary>
    /// Speed at which the camera should move to the next position
    /// </summary>
    [SerializeField] private float _move_speed = 2f;

    /// <summary>
    /// Camera to move on focused object
    /// </summary>
    private Camera _camera;

    /// <summary>
    /// Should be updating camera position next frame?
    /// </summary>
    private bool _update_position = false;

    /// <summary>
    /// Position of the camera before it start moving
    /// </summary>
    private Vector3 _initial_position;

    /// <summary>
    /// position to which the camera should move
    /// </summary>
    private Vector3 _next_position;

    /// <summary>
    /// Position of the camera between _initial_position and _next_position (from 0 to 1)
    /// </summary>
    private float _interpolate_value = 1f;

    /// <summary>
    /// Focused bounds by the camera 
    /// </summary>
    private List<Bounds> _focused_bounds;

    /// <summary>
    /// Number of failed tests
    /// </summary>
    private int _num_of_fails = 0;

    /// <summary>
    /// Set object to be focused by the camera, and tells the controller to update camera position next frame
    /// </summary>
    /// <param name="focused_bounds">bounds coming from mesh renderer of focused object</param>
    public void UpdateFocus(List<Bounds> focused_bounds)
    {
        _update_position = true;
        _focused_bounds = focused_bounds;
    }

    /// <summary>
    /// Erase every text in the log
    /// </summary>
    public void ResetLog()
    {
        _log.text = string.Empty;
        _log.color = Color.black;
        _num_of_fails = 0;
    }

    /// <summary>
    /// Set if the camera should be orthographic or not
    /// </summary>
    /// <param name="mode"></param>
    public void SetCameraMode()
    {
        _camera.orthographic = !_camera.orthographic;
        UpdateLogger();
    }
    //---------------------------------------------------------------------------------------------------------------------------------------

    private void Start()
    {
        _next_position = this.transform.position;
        _camera = this.GetComponent<Camera>();
        if (_camera == null)
        {
            Debug.LogError("CameraController Start(): _camera null");
        }
        ResetLog();
    }

    private void Update()
    {
        MoveCamera();
    }

    /// <summary>
    /// Move the camera smoothly from a position to another
    /// </summary>
    private void MoveCamera()
    {
        if (_interpolate_value >= 1f)
        {
            return;
        }
        else
        {
            _interpolate_value += _move_speed * Time.deltaTime;
        }

        if (_interpolate_value >= 1f)
        {
            _interpolate_value = 1;
            UpdateLogger();
        }

        this.gameObject.transform.position = Vector3.Lerp(_initial_position, _next_position, _interpolate_value);
    }

    private void OnPostRender()
    {
        if (_update_position)
        {
            UpdateCameraPosition();
            _update_position = false;
        }
    }

    /// <summary>
    /// Update camera target position according to focused object
    /// </summary>
    private void UpdateCameraPosition()
    {
        if (_focused_bounds == null)
        {
            return;
        }

        Bounds bounds = new Bounds(_focused_bounds[0].center, Vector3.zero);

        if (_focused_bounds.Count < 2)
        {
            float offset = 0f;
            if (_focused_bounds[0].size.y / Screen.height > _focused_bounds[0].size.x / Screen.width)
            {
                offset = _focused_bounds[0].size.y;
            }
            else
            {
                offset = _focused_bounds[0].size.x;
            }

            Vector3 next_position = bounds.center - _camera.transform.forward * _focused_bounds[0].size.z - _camera.transform.forward * offset;

            SetCameraNextPosition(next_position);
            return;
        }

        for (int i = 1; i < _focused_bounds.Count; i++)
        {
            bounds.Encapsulate(_focused_bounds[i].center);
        }

        Vector3 top = _focused_bounds[0].center;
        top.y += _focused_bounds[0].extents.y;
        Vector3 bottom = _focused_bounds[0].center;
        bottom.y -= _focused_bounds[0].extents.y;
        Vector3 right = _focused_bounds[0].center;
        right.x += _focused_bounds[0].extents.x;
        Vector3 left = _focused_bounds[0].center;
        left.x -= _focused_bounds[0].extents.x;
        Vector3 front = _focused_bounds[0].center;
        front.z += _focused_bounds[0].extents.z;
        Vector3 back = _focused_bounds[0].center;
        back.z -= _focused_bounds[0].extents.z;

        for (int i = 1; i < _focused_bounds.Count; i++)
        {
            Vector3 position_focused = _focused_bounds[i].center;
            if (top.y < position_focused.y + _focused_bounds[i].extents.y)
            {
                top = position_focused;
                top.y += _focused_bounds[i].extents.y;
            }

            if (bottom.y > position_focused.y - _focused_bounds[i].extents.y)
            {
                bottom = position_focused;
                bottom.y -= _focused_bounds[i].extents.y;
            }

            if (right.x < position_focused.x + _focused_bounds[i].extents.x)
            {
                right = position_focused;
                right.x += _focused_bounds[i].extents.x;
            }

            if (left.x > position_focused.x - _focused_bounds[i].extents.x)
            {
                left = position_focused;
                left.x -= _focused_bounds[i].extents.x;
            }

            if (front.z < position_focused.z + _focused_bounds[i].extents.z)
            {
                front = position_focused;
                front.x += _focused_bounds[i].extents.z;
            }

            if (back.z > position_focused.z - _focused_bounds[i].extents.z)
            {
                back = position_focused;
                back.x -= _focused_bounds[i].extents.z;
            }
        }

        bounds = new Bounds(top, Vector3.zero);
        bounds.Encapsulate(bottom);
        bounds.Encapsulate(right);
        bounds.Encapsulate(left);
        bounds.Encapsulate(front);
        bounds.Encapsulate(back);

        float scale = 1f;
        if (bounds.size.y / Screen.height > bounds.size.x / Screen.width)
        {
            scale = bounds.size.y;
        }
        else
        {
            scale = bounds.size.x;
        }

        Vector3 new_position = bounds.center - _camera.transform.forward * scale - _camera.transform.forward * (bounds.size.z / 2);
        SetCameraNextPosition(new_position);

    }

    private void SetCameraNextPosition(Vector3 next_position)
    {
        _initial_position = this.transform.position;
        _next_position = next_position;
        _interpolate_value = 0f;
    }

    /// <summary>
    /// Logger to display if object are visible or not
    /// </summary>
    private void UpdateLogger()
    {
        if (_focused_bounds == null || _focused_bounds.Count == 0)
        {
            return;
        }

        string log_not_visible = string.Empty;
        int num_not_visible = 0;
        foreach (Bounds focused_bounds in _focused_bounds)
        {
            if (!IsBoundsVisible(focused_bounds))
            {
                log_not_visible += "Object at position " + focused_bounds.center + " not visible.\n";
                num_not_visible++;
            }
        }
        int obj_count = _focused_bounds.Count;
        if (num_not_visible > 0)
        {
            _log.color = Color.yellow;
            _num_of_fails++;
        }

        _log.text = "Number of failed test : " + _num_of_fails + "\n";
        _log.text += "Visible objects " + (obj_count - num_not_visible).ToString() + "/" + obj_count + "\n";
        _log.text += log_not_visible;
    }

    /// <summary>
    /// Is bounds is entirely visible on the screen?
    /// </summary>
    /// <param name="bounds"></param>
    /// <returns></returns>
    private bool IsBoundsVisible(Bounds bounds)
    {
        float distance_camera_center = Vector3.Distance(bounds.center, this.transform.position);
        if (!_camera.orthographic && distance_camera_center > _camera.farClipPlane)
        {
            return false;
        }

        Vector3 top = bounds.center;
        top.y += bounds.extents.y;

        if (_camera.WorldToScreenPoint(top).y > Screen.height)
        {
            return false;
        }

        Vector3 bottom = bounds.center;
        bottom.y -= bounds.extents.y;

        if (_camera.WorldToScreenPoint(bottom).y < 0)
        {
            return false;
        }

        Vector3 left = bounds.center;
        left.x -= bounds.extents.x;

        if (_camera.WorldToScreenPoint(left).x < 0)
        {
            return false;
        }

        Vector3 right = bounds.center;
        right.x += bounds.extents.x;

        if (_camera.WorldToScreenPoint(right).x > Screen.width)
        {
            return false;
        }

        return true;
    }

}