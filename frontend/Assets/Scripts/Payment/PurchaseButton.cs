using Assets.Script.Constants;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PurchaseButton : MonoBehaviour
{
    public bool monthly = false;
    public bool yealy = false;
	public bool isUpgrade = false;

    public Image monthyBackground;
    public Image yearlyBackground;
    public Text monthlyPrice;
    public Text monthlyName;
    public Text yearlyPrice;
    public Text yearlyName;
    public Text headInfo1;
    public Text headInfo2;
    public Text ContinuesBtnText;

    public GameObject monthlySubscribe;
    public GameObject yearlySubscribe;
    public GameObject ContinuesBtn;

    public MyIAPManager iapManager;

    private void Start()
    {
        CheckView();
    }

    public void CheckView()
    {
        if (UserConstants.s_user.paymentStatus == PaymentStatus.s_yearly)
        {
            monthlySubscribe.SetActive(false);
            yearlySubscribe.SetActive(false);
            ContinuesBtn.SetActive(false);
            headInfo1.text = "You are already";
            headInfo2.text = "YEARLY SUBSCRIBED";
        }
        else if (UserConstants.s_user.paymentStatus == PaymentStatus.s_monthly)
        {
            monthlySubscribe.SetActive(false);
            yearlySubscribe.SetActive(false);
            headInfo1.text = "You are already";
            headInfo2.text = "MONTHLY SUBSCRIBED";
            ContinuesBtnText.text = "CHANGE IT TO YEARLY";
            yealy = true;
            isUpgrade = true;
            monthly = false;
        }
        else if (UserConstants.s_user.paymentStatus == PaymentStatus.s_lifetime)
        {
            monthlySubscribe.SetActive(false);
            yearlySubscribe.SetActive(false);
            ContinuesBtn.SetActive(false);
            headInfo1.text = "You are already";
            headInfo2.text = "Premium User";
        }
        else if(UserConstants.s_user.paymentStatus == PaymentStatus.s_none)
        {
            monthlySubscribe.SetActive(true);
            yearlySubscribe.SetActive(true);
            ContinuesBtn.SetActive(true);
            headInfo2.text = "Go Premium";
        }
    }

    public void ClickPurchasedButton()
    {
        if (monthly)
        {
            iapManager.BuyMonthlySubscription();
        }
        else if (yealy)
        {
            iapManager.BuyYearlySubscription(isUpgrade);
        }
    }

    public void ClickMonthlySubscription()
    {
        monthly = true;
        yealy = false;
        monthyBackground.sprite = Resources.Load<Sprite>("Memotopia_UI/PaymentImages/selected");
        yearlyBackground.sprite = Resources.Load<Sprite>("Memotopia_UI/PaymentImages/unselected");
        monthlyPrice.color = new Color32(27, 95, 255, 255);
        monthlyName.color = new Color32(27, 95, 255, 255);
        yearlyPrice.color = new Color(0, 0, 0);
        yearlyName.color = new Color(0, 0, 0);
    }

    public void ClickYearlySubscription()
    {
        monthly = false;
        yealy = true;
        monthyBackground.sprite = Resources.Load<Sprite>("Memotopia_UI/PaymentImages/unselected");
        yearlyBackground.sprite = Resources.Load<Sprite>("Memotopia_UI/PaymentImages/selected");
        yearlyPrice.color = new Color32(27, 95, 255, 255);
        yearlyName.color = new Color32(27, 95, 255, 255);
        monthlyPrice.color = new Color(0, 0, 0);
        monthlyName.color = new Color(0, 0, 0);
    }

    public void linkBtn()
    {
        Application.OpenURL("http://memotopia.com/pp/privacy-and-policy.pdf?fbclid=IwAR09a-hC9bZbhZtb2gFGvOwAPs9jMDWenQ_wozqDADBhKW1qo2Us_jL7jBg");
    }
}
