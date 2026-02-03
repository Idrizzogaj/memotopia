using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GlobalScoreView : MonoBehaviour
{
    [SerializeField]
    private RectTransform contentRect;

    private void Start()
    {
        if ((int)(Math.Round((float)Screen.width / (float)Screen.height * 4)) == 3)
        {
            contentRect.offsetMin = new Vector2(100, 20); // new Vector2(left, bottom);
            contentRect.offsetMax = new Vector2(-100, -150); // new Vector2(-right, -top);
        }
        else
        {
            contentRect.offsetMin = new Vector2(100, 100); // new Vector2(left, bottom);
            contentRect.offsetMax = new Vector2(-100, -200); // new Vector2(-right, -top);

        }
    }
}
