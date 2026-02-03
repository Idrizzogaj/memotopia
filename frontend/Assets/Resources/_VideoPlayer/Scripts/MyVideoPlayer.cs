/*

   .-------.                             .--.    .-------.     .--.            .--.     .--.        
   |       |--.--.--------.-----.-----.--|  |    |_     _|--.--|  |_.-----.----|__|---.-|  |-----.
   |   -   |_   _|        |  _  |     |  _  |      |   | |  |  |   _|  _  |   _|  |  _  |  |__ --|
   |_______|__.__|__|__|__|_____|__|__|_____|      |___| |_____|____|_____|__| |__|___._|__|_____|
   © 2019 OXMOND / www.oxmond.com 

*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MyVideoPlayer : MonoBehaviour
{
    public GameObject btnPlay;
    public GameObject btnPause;

    private VideoPlayer videoPlayer;

    private void Awake()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    private void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        btnPause.SetActive(true);
        btnPlay.SetActive(false);
    }

    public void BtnPlayVideo()
    {
        if (videoPlayer.isPlaying)
            VideoStop();
        else
            VideoPlay();
    }

    private void VideoStop()
    {
        videoPlayer.Pause();
        btnPause.SetActive(false);
        btnPlay.SetActive(true);
    }

    private void VideoPlay()
    {
        videoPlayer.Play();
        btnPause.SetActive(true);
        btnPlay.SetActive(false);
    }
}