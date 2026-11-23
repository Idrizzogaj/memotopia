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

        PresentImage(transform, choosenArtboardNumber);
    }

    // Shared with Saga's instance-scoped animation receiver; legacy events stay unchanged.
    public static void PresentImage(Transform cell, int imageNumber)
    {
        cell.GetChild(0).GetChild(0).gameObject.SetActive(true);
        Image picture = cell.GetChild(0).GetChild(0).GetComponent<Image>();
        // Numbers and words are shown as wide text (a card would be too small inside the diamond).
        bool textFace = CardFaces.KindOf(imageNumber) != CardKind.Image;
        picture.sprite = textFace ? CardFaces.WideFace(imageNumber) : CardFaces.Face(imageNumber);
        picture.preserveAspect = true;
        // The tile sprite includes a large shadow. Keep the entire opaque card
        // inside its white diamond face, including the card's four corners.
        RectTransform pictureRect = picture.rectTransform;
        // A wide rectangle fits the diamond when its width + height is at most the diamond's width (about 0.52 of the tile).
        pictureRect.anchorMin = textFace ? new Vector2(0.30f, 0.44f) : new Vector2(0.37f, 0.38f);
        pictureRect.anchorMax = textFace ? new Vector2(0.70f, 0.56f) : new Vector2(0.63f, 0.64f);
        pictureRect.anchoredPosition = Vector2.zero;
        pictureRect.sizeDelta = Vector2.zero;
        cell.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
    }

    public void HideImage()
    {
        HideFace(transform);
        if (afterShownImage != null)
            afterShownImage();
    }

    public static void HideFace(Transform cell)
    {
        cell.GetChild(0).GetChild(0).gameObject.SetActive(false);
        cell.GetComponent<Image>().color = new Color32(255, 255, 255, 0);
    }

    public void AnimationFinished()
    {
        if (onAnimationFinished != null)
            onAnimationFinished();
    }
}
