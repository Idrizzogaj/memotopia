// This script is created as an addition to control scrollbar size
// there where cases when we had more items in the content of the scroll view
// and in those cases because scrollbar was getting smaller and smaller
// in those cases, it was getting negative value

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScrollbarController : MonoBehaviour
{
    private Scrollbar scrollbar;
    private Scene scene;
    private float minScrollbarSize;

    private void Start()
    {
        scrollbar = GetComponent<Scrollbar>();
        scene = SceneManager.GetActiveScene();
        // if normal/phone scene or ipad
        if (scene.name == "BoxesScene")
            minScrollbarSize = 0.67f;
        else
            minScrollbarSize = 0.89f;
    }

    //called from ScrollRect onValueChanged
    public void UpdateSize()
    {
        if (scrollbar.size < minScrollbarSize)
            scrollbar.size = minScrollbarSize;
    }
}
