using Assets.Script.Constants;
using Assets.Script.Controllers;
using UnityEngine;
using UnityEngine.UI;

public class MenuAnimations : MonoBehaviour
{
    public GameObject homeScreen;
    public GameObject trainingScreen;
    public GameObject sideMenuScreen;

    public GameObject openMenuHomePosition;
    public GameObject canvasContainer;
    public GameObject openMenuBtn;
    public GameObject closeMenuBtn;

    private bool _isMenuOpen = false;
    private bool _isMenuClosed = false;
    private float _startTime;
    // Sa ma i madh speed aq ma kadale shkon
    private float _timeSpeed = 0.5f;
    private float _totalTime;

    void Update()
    {

        _totalTime = (Time.time - _startTime) / _timeSpeed;

        if (_isMenuClosed)
        {
            homeScreen.transform.position = Vector2.Lerp(homeScreen.transform.position, openMenuHomePosition.transform.position, _totalTime);

            // e zvoglon screenin tu e qit onash
            homeScreen.transform.localScale += new Vector3(-0.1f, -0.1f, 0f) * _timeSpeed * Time.deltaTime;

            if (_totalTime >= 0.7)
            {
                _isMenuClosed = false;
            }
        }

        if (_isMenuOpen)
        {
            homeScreen.transform.position = Vector2.Lerp(homeScreen.transform.position, canvasContainer.transform.position, _totalTime);

            // e rrit screenin tu e qit nvend
            homeScreen.transform.localScale += new Vector3(0.1f, 0.1f, 0f) * _timeSpeed * Time.deltaTime;

            if (_totalTime >= 0.7)
            {
                sideMenuScreen.SetActive(false);
                _isMenuOpen = false;
            }
        }
    }

    public void OpenMenu()
    {
        sideMenuScreen.SetActive(true);
        _isMenuClosed = true;
        _startTime = Time.time;
        openMenuBtn.SetActive(false);
        closeMenuBtn.SetActive(true);
    }

    public void CloseMenu()
    {
        _isMenuOpen = true;
        _startTime = Time.time;
        openMenuBtn.SetActive(true);
        closeMenuBtn.SetActive(false);
    }
}
