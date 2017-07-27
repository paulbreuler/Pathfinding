using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridColor : MonoBehaviour
{

    public Color defaultColor;
    public Color currentColor;

    private Renderer rend;
    // Use this for initialization
    void Awake()
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

    public void UpdateColor(Walkable isWalkable)
    {
        if (isWalkable == Walkable.Passable)
        {
            rend.material.color = defaultColor;
        }
        else
        {
            rend.material.color = Color.red;
        }
    }
}
