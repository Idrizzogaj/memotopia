using System;
using System.Reflection;
using Assets.Script.Constants;
using Assets.Script.Models;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class PurchaseNavigationValidation
{
    public static void Validate()
    {
        var priorUser = UserConstants.s_user;
        string token = PlayerPrefs.GetString("token");
        int scene = SceneManager.GetActiveScene().handle;
        var root = new GameObject("Purchase callback validation");
        root.SetActive(false); // Never initialize a real store in this test.
        try
        {
            var user = new User { id = 2147483588, paymentStatus = PaymentStatus.s_monthly };
            UserConstants.s_user = user;
            var manager = root.AddComponent<MyIAPManager>();
            var view = root.AddComponent<PurchaseButton>();
            view.monthlySubscribe = Child(root, "Monthly");
            view.yearlySubscribe = Child(root, "Yearly");
            view.ContinuesBtn = Child(root, "Continue");
            view.headInfo1 = Child(root, "Heading1").AddComponent<Text>();
            view.headInfo2 = Child(root, "Heading2").AddComponent<Text>();
            view.ContinuesBtnText = Child(root, "Continue text").AddComponent<Text>();
            foreach (var reason in new[] { PurchaseFailureReason.UserCancelled, PurchaseFailureReason.PurchasingUnavailable, PurchaseFailureReason.Unknown })
            {
                manager.OnPurchaseFailed(null, reason);
                Require(SceneManager.GetActiveScene().handle == scene, "failed/cancelled purchase navigated away");
                Require(ReferenceEquals(UserConstants.s_user, user) && PlayerPrefs.GetString("token") == token, "purchase changed login");
            }
            manager.OnPurchaseDeferred(null);
            user.paymentStatus = PaymentStatus.s_yearly;
            typeof(MyIAPManager).GetMethod("RefreshPurchaseViews", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(manager, null);
            Require(view.headInfo2.text == "YEARLY SUBSCRIBED" && !view.ContinuesBtn.activeSelf, "successful status refresh missing");
            Require(SceneManager.GetActiveScene().handle == scene && PlayerPrefs.GetString("token") == token, "status refresh navigated or logged out");
            Debug.Log("PURCHASE_NAVIGATION_VALIDATION_PASSED: cancelled, unavailable, failed, deferred and in-place premium refresh preserve session and scene.");
        }
        finally { UserConstants.s_user = priorUser; UnityEngine.Object.DestroyImmediate(root); }
    }
    private static GameObject Child(GameObject parent, string name)
    {
        var child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(parent.transform, false);
        return child;
    }
    private static void Require(bool condition, string message)
    { if (!condition) throw new Exception("Purchase navigation: " + message); }
}
