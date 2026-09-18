using Assets.Script.Constants;
using UnityEngine;
using UnityEngine.UI;

public class SideMenuAvailability : MonoBehaviour
{
    private void OnEnable() { Refresh(); InvokeRepeating("Refresh", 1, 1); }
    private void OnDisable() { CancelInvoke(); }

    public static bool IsAvailable(string action, bool storeReady, bool unsupportedUpgrade)
    {
        switch (action)
        {
            case "GoToAccount": case "Logout": return true;
            case "RateBtnClick": return MenuNavigation.PublicStoreListingAvailable;
            case "FacebookShare": return false;
            case "GoToPayment": return storeReady && !unsupportedUpgrade;
            default: return false;
        }
    }

    public void Refresh()
    {
        bool unsupportedUpgrade = Application.platform == RuntimePlatform.IPhonePlayer &&
            UserConstants.s_user != null && UserConstants.s_user.paymentStatus == PaymentStatus.s_monthly;
        foreach (var button in GetComponentsInChildren<Button>(true))
        {
            bool active = false;
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                active |= IsAvailable(button.onClick.GetPersistentMethodName(i), MyIAPManager.CanBuySubscriptions, unsupportedUpgrade);
            button.interactable = active;
            var group = button.GetComponent<CanvasGroup>();
            if (group == null) group = button.gameObject.AddComponent<CanvasGroup>();
            group.alpha = active ? 1 : .32f;
        }
    }
}
