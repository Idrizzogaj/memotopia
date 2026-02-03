using UnityEngine;

public class TrainingAnimations : MonoBehaviour
{
    [SerializeField] GameObject pairsBtn, pairsBtnNext, flashBtn, flashBtnNext, boxesBtn, boxesBtnNext;

    private float _startTime;
    // Sa ma i madh speed aq ma kadale shkon
    private float _timeSpeed = 0.9f;

    private bool _b = true;

    // Start is called before the first frame update
    void Start()
    {
        _startTime = Time.time;
        _b = true;
    }

    // Update is called once per frame
    void Update()
    {
        float t = (Time.time - _startTime) / _timeSpeed;

        if (_b)
        {
            pairsBtn.transform.position = Vector2.Lerp(pairsBtn.transform.position, pairsBtnNext.transform.position, t);
            flashBtn.transform.position = Vector2.Lerp(flashBtn.transform.position, flashBtnNext.transform.position, t);
            boxesBtn.transform.position = Vector2.Lerp(boxesBtn.transform.position, boxesBtnNext.transform.position, t);

            if (t > 0.8)
                _b = false;
        }
    }
}
