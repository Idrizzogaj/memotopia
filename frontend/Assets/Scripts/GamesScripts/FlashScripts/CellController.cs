using UnityEngine;
using UnityEngine.UI;
using System;
public class CellController : MonoBehaviour
{
    public static Func<int> onShowImage;
    public static Action afterShownImage;
    public static Action onAnimationFinished;
    private int choosenArtboardNumber;

    public void ShowImage()
    {
        if (onShowImage != null)
            choosenArtboardNumber = onShowImage();
        else
            Debug.LogError("Empty Function Delegate on CellController.cs");

        transform.GetChild(0).transform.GetChild(0).transform.gameObject.SetActive(true);
        transform.GetChild(0).transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(
            "ImagesWithoutBackground\\Artboard-" + choosenArtboardNumber.ToString()) as Sprite;
        transform.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
    }

    public void HideImage()
    {
        transform.GetChild(0).transform.GetChild(0).transform.gameObject.SetActive(false);
        gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 0);
        if (afterShownImage != null)
            afterShownImage();
    }

    public void AnimationFinished()
    {
        if (onAnimationFinished != null)
            onAnimationFinished();
    }
}
