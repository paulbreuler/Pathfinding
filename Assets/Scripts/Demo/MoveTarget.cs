using UnityEngine;

public class MoveTarget : MonoBehaviour {

    Vector3 _newPosition;
    void Start()
    {
        _newPosition = transform.position;
    }

    void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                _newPosition = hit.point;
                transform.position = _newPosition + Vector3.up;
            }
        }
    }
}
