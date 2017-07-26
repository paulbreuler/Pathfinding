using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridColor : MonoBehaviour
{

    public Color defaultColor;
    public Color currentColor;

    private Renderer rend;
    // Use this for initialization
    void Start()
    {
        rend = GetComponent<Renderer>();
        defaultColor = new Color();
        ColorUtility.TryParseHtmlString("#00FFFF6F", out defaultColor);
        rend.material.color = defaultColor;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateColor(bool isWalkable)
    {
        if (isWalkable)
        {
            rend.material.color = defaultColor;
        }
        else
        {
            rend.material.color = Color.red;
        }
    }
}
