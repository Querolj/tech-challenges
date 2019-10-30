using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manage gameobjects in the scene
/// </summary>
public class ObjectManager : MonoBehaviour
{
    /// <summary>
    /// Object to move the camera
    /// </summary>
    [SerializeField] private CameraController _camera_controller;

    /// <summary>
    /// prefabs to add randomly to the scene
    /// </summary>
    [SerializeField] private List<GameObject> _prefabs = new List<GameObject>();

    /// <summary>
    /// Random position range to which prefabs can be instantiated
    /// </summary>
    [SerializeField] private Vector3 _random_range_position;

    /// <summary>
    /// Bounds of the MeshRenderer in the scene
    /// </summary>
    private List<Bounds> _focused_bounds = new List<Bounds>();

    /// <summary>
    /// object manager by ObjectManager
    /// </summary>
    private List<GameObject> _focused_gos = new List<GameObject>();

    private bool _auto_test = false;


    /// <summary>
    /// Add a game object with a random position
    /// </summary>
    public void AddRandomObject()
    {
        if (_prefabs.Count == 0)
        {
            Debug.LogError("ObjectManager AddRandomObject() : No prefabs registered");
            return;
        }

        int index = Random.Range(0, _prefabs.Count);
        GameObject new_object = Instantiate(_prefabs[index]);

        Vector3 random_position = new Vector3(Random.Range(-_random_range_position.x, _random_range_position.x),
                                            Random.Range(-_random_range_position.y, _random_range_position.y),
                                            Random.Range(-_random_range_position.z, _random_range_position.z));
        new_object.transform.position = random_position;
        new_object.transform.parent = this.transform;
        MeshRenderer focused_mesh = new_object.GetComponent<MeshRenderer>();

        if (focused_mesh == null)
        {
            Debug.LogError("ObjectManager AddRandomObject : instantiated prefab object has no MeshRenderer component");
            return;
        }
        _focused_bounds.Add(focused_mesh.bounds);
        _focused_gos.Add(focused_mesh.gameObject);

        new_object.name += _focused_bounds.Count;

        _camera_controller.UpdateFocus(_focused_bounds);
    }

    /// <summary>
    /// Launch random recurrent test next frame
    /// </summary>
    public void AutomaticTest()
    {
        if (!_auto_test)
        {
            InvokeRepeating("RandomTest", 0f, 0.05f);
            _auto_test = true;
        }
        else
        {
            CancelInvoke();
            _auto_test = false;
        }
    }

    /// <summary>
    /// Delete every object in the scene controlled by ObjectManager
    /// </summary>
    public void ResetScene()
    {
        for (int i = _focused_gos.Count - 1; i >= 0; i--)
        {
            GameObject focused_go = _focused_gos[i];
            _focused_gos.RemoveAt(i);
            GameObject.Destroy(focused_go);
        }

        _focused_bounds.Clear();
        _camera_controller.ResetLog();
        CancelInvoke();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------

    private void Start()
    {
        MeshRenderer[] children_transform = this.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in children_transform)
        {
            if (renderer != null)
            {
                _focused_bounds.Add(renderer.bounds);
                _focused_gos.Add(renderer.gameObject);
            }
        }
    }

    private void RandomTest()
    {
        AddRandomObject();
    }


}
