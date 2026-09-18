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
        Image picture = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        picture.sprite = MajorSystemImages.Load(choosenArtboardNumber);
        picture.preserveAspect = true;
        // The tile sprite includes a large shadow. Keep the entire opaque card
        // inside its white diamond face, including the card's four corners.
        RectTransform pictureRect = picture.rectTransform;
        pictureRect.anchorMin = new Vector2(0.37f, 0.38f);
        pictureRect.anchorMax = new Vector2(0.63f, 0.64f);
        pictureRect.anchoredPosition = Vector2.zero;
        pictureRect.sizeDelta = Vector2.zero;
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
