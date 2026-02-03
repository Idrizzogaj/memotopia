using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarFillScript : MonoBehaviour
{
    public Text Score;

    public Transform leftStar;
    public Transform midStar;
    public Transform rightStar;

    public GameObject leftStarG;
    public GameObject midStarG;
    public GameObject rightStarG;

    [SerializeField] private float lCurrentAmount;
    [SerializeField] private float lCurrentScore;
    [SerializeField] private float lSpeed;

    [SerializeField] private float mCurrentAmount;
    [SerializeField] private float mCurrentScore;
    [SerializeField] private float mSpeed;

    [SerializeField] private float rCurrentAmount;
    [SerializeField] private float rCurrentScore;
    [SerializeField] private float rSpeed;

    float threeStar = 75f / 100f * 180f;
    float twoStar = 50f / 100f * 180f;
    float mtime = 0.2f;
    float rtime = 0.2f;
    int ScoreInt = 0;

    // Start is called before the first frame update
    void Start()
    {
        ScoreInt = Int16.Parse(Score.text);
    }

    // Update is called once per frame
    void Update()
    {
        leftStarG.SetActive(true);
        // current score osht sa perqind don mu mbush load-i
        if (lCurrentAmount < lCurrentScore)
        {
            // 35 osht speed per mu mbush load
            lCurrentAmount += lSpeed * Time.deltaTime;
        }

        leftStar.GetComponent<Image>().fillAmount = lCurrentAmount / 100;

        if(ScoreInt >= twoStar)
        {
            if (mtime >= 0)
            {
                mtime -= Time.deltaTime;
                return;
            }
            else
            {
                midStarG.SetActive(true);
                // current score osht sa perqind don mu mbush load-i
                if (mCurrentAmount < mCurrentScore)
                {
                    // 35 osht speed per mu mbush load
                    mCurrentAmount += mSpeed * Time.deltaTime;
                }

                midStar.GetComponent<Image>().fillAmount = mCurrentAmount / 100;
            }
        }
        

        if(ScoreInt >= threeStar)
        {
            if (rtime >= 0)
            {
                rtime -= Time.deltaTime;
                return;
            }
            else
            {
                rightStarG.SetActive(true);
                // current score osht sa perqind don mu mbush load-i
                if (rCurrentAmount < rCurrentScore)
                {
                    // 35 osht speed per mu mbush load
                    rCurrentAmount += rSpeed * Time.deltaTime;
                }

                rightStar.GetComponent<Image>().fillAmount = rCurrentAmount / 100;
            }
        }
    }
}
