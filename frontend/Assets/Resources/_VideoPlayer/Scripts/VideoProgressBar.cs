using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoProgressBar : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public VideoPlayer video;
    private Slider tracking;
    private bool slide;

    void Start()
    {
        tracking = GetComponent<Slider>();   
    }

    void Update()
    {
        if(!slide && video.isPlaying)
            tracking.value = (float)video.frame / (float)video.frameCount;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        slide = true;
        video.Pause();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        float frame = (float)tracking.value * (float)video.frameCount;
        video.frame = (long)frame;
        slide = false;
        video.Play();
    }
}
