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
            case "FacebookShare": return true; // now "Invite friends" (phone share sheet)
            case "GoToPayment": return storeReady && !unsupportedUpgrade;
            default: return false;
        }
    }

    // The old "Share on Facebook" entry becomes "Invite friends": new label, no Facebook logo.
    private static void RelabelInvite(Button button)
    {
        bool isInvite = false;
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            isInvite |= button.onClick.GetPersistentMethodName(i) == "FacebookShare";
        if (!isInvite) return;
        // The Android scene has the old Facebook button switched off; "Invite friends" must be visible there too.
        if (!button.gameObject.activeSelf) button.gameObject.SetActive(true);
        foreach (var text in button.GetComponentsInChildren<Text>(true))
            if (text.text != "Invite friends") text.text = "Invite friends";
        foreach (var image in button.GetComponentsInChildren<Image>(true))
            if (image.gameObject != button.gameObject && image.gameObject.name.Contains("Facebook"))
                image.gameObject.SetActive(false);
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
            RelabelInvite(button);
            var group = button.GetComponent<CanvasGroup>();
            if (group == null) group = button.gameObject.AddComponent<CanvasGroup>();
            group.alpha = active ? 1 : .32f;
        }
    }
}
