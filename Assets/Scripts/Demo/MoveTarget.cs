using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTarget : MonoBehaviour {

    Vector3 newPosition;
    void Start()
    {
        newPosition = transform.position;
    }

    void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                newPosition = hit.point;
                transform.position = newPosition + Vector3.up;
            }
        }
    }
}
