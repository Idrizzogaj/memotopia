using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using Assets.Script.Constants;
using Assets.Script.Controllers;
using UnityEngine.SceneManagement;

public class MyIAPManager : MonoBehaviour, IStoreListener
{
    public static MyIAPManager instance;

    public PurchaseButton purchaseButton;
    public MenuNavigation menuNavigation;

    private static IStoreController m_StoreController;          // The Unity Purchasing system.
    private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.
    private IAppleExtensions m_AppleExtensions;
    private IGooglePlayStoreExtensions m_GoogleExtensions;

    private UserAPIController userAPIController;
    private LoadingManager loadingScreen;

    // ProductIDs
    public static string monthlySubscription = SubscriptionConstants.s_monthlySubscription;
    public static string yearlySubscription = SubscriptionConstants.s_yearlySubscription;

    private void Awake()
    {
        Singleton();
    }

    private void Singleton()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // If we haven't set up the Unity Purchasing reference
        if (m_StoreController == null)
            InitializePurchasing();
        userAPIController = GetComponent<UserAPIController>();

    }

    public void InitializePurchasing()
    {
        if (IsInitialized())
            return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct(monthlySubscription, ProductType.Subscription);
        builder.AddProduct(yearlySubscription, ProductType.Subscription);

        MyDebug("Starting Initialized...");
        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    public void BuyMonthlySubscription()
    {
        loadingScreen = GameObject.Find("LoadingCanvas").GetComponent<LoadingManager>();
        loadingScreen.StartLoading();
        BuyProductID(monthlySubscription);
    }

    public void BuyYearlySubscription(bool isUpgrade)
    {
        loadingScreen = GameObject.Find("LoadingCanvas").GetComponent<LoadingManager>();
        loadingScreen.StartLoading();
        if (isUpgrade)
        {
            try
            {
                Product oldProduct = m_StoreController.products.WithID(monthlySubscription);
                Product newProduct = m_StoreController.products.WithID(yearlySubscription);
                switch (Application.platform)
                {
                    case RuntimePlatform.IPhonePlayer:
                        //TODO::Both solutions below are not working as per now 23 Jan 2022 - Ilir 
                        //First Try
                        //BuyProductID(yearlySubscription);
                        // Second try
                        //SubscriptionManager.UpdateSubscriptionInAppleStore(newProduct, "testDevPayload", (newProductArg, unknownArg) =>
                        //         m_StoreController.InitiatePurchase(newProductArg));
                        break;
                    case RuntimePlatform.Android:
                        IGooglePlayStoreExtensions m_GooglePlayStoreExtensions = m_StoreExtensionProvider.GetExtension<IGooglePlayStoreExtensions>();

                        SubscriptionManager.UpdateSubscriptionInGooglePlayStore(oldProduct, newProduct,
                        (productInfos, newProductId) =>
                        {
                            m_GooglePlayStoreExtensions.UpgradeDowngradeSubscription(oldProduct.definition.id,
                                                                                newProduct.definition.id);
                        });
                        break;
                }
            }
            catch (Exception)
            {
                Application.Quit();
            }
        }
        else
        {
            MyDebug("No Upgrade");
            BuyProductID(yearlySubscription);
        }
    }

    private void BuyProductID(string productId)
    {
        if (IsInitialized())
        {
            UnityEngine.Purchasing.Product product = m_StoreController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                MyDebug(string.Format("Purchasing product:" + product.definition.id.ToString()));
                m_StoreController.InitiatePurchase(product);
            }
            else
            {
                MyDebug("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        else
        {
            MyDebug("BuyProductID FAIL. Not initialized.");
        }
    }

    public void RestorePurchases()
    {
        m_StoreExtensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions(result =>
        {
            if (result)
            {
                MyDebug("Restore purchases succeeded.");
            }
            else
                MyDebug("Restore purchases failed.");
        });

        if (!IsInitialized())
        {
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }
    }

    public void ListProducts()
    {
        foreach (UnityEngine.Purchasing.Product item in m_StoreController.products.all)
        {
            if (item.receipt != null)
                MyDebug("Receipt found for Product = " + item.definition.id.ToString());
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        MyDebug("OnInitialized: PASS");

        m_StoreController = controller;
        m_StoreExtensionProvider = extensions;
        m_AppleExtensions = extensions.GetExtension<IAppleExtensions>();
        m_GoogleExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();

        m_GoogleExtensions?.SetDeferredPurchaseListener(OnPurchaseDeferred);

        Dictionary<string, string> dict = m_AppleExtensions.GetIntroductoryPriceDictionary();
        
        int i = 0;
        foreach (UnityEngine.Purchasing.Product item in controller.products.all)
        {
            if (item.receipt != null)
            {
                string intro_json = (dict == null || !dict.ContainsKey(item.definition.storeSpecificId)) ? null : dict[item.definition.storeSpecificId];
                if (item.definition.type == ProductType.Subscription)
                {
                    i++;
                    SubscriptionManager p = new SubscriptionManager(item, intro_json);
                    SubscriptionInfo info = p.getSubscriptionInfo();
                    MyDebug("SubInfo: " + info.getProductId().ToString());
                    MyDebug("isSubscribed: " + info.isSubscribed().ToString());
                    MyDebug("isFreeTrial: " + info.isFreeTrial().ToString());
                    MyDebug("expireDate: " + info.getExpireDate().ToString());
                    MyDebug("purchaseDate: " + info.getPurchaseDate().ToString());

                    if ((info.isExpired().Equals(true) || info.isCancelled().Equals(true)) && UserConstants.s_user.paymentStatus != PaymentStatus.s_lifetime)
                        PaymentStatusToNone();
                }
            }
        }

        if (i == 0 && UserConstants.s_user.paymentStatus != PaymentStatus.s_lifetime)
            PaymentStatusToNone();
    }

    public void OnPurchaseDeferred(Product product)
    {
        MyDebug("Deferred product " + product.definition.id.ToString());
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
        MyDebug("OnInitializeFailed InitializationFailureReason:" + error);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        try
        {
            var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
            var result = validator.Validate(args.purchasedProduct.receipt);

            MyDebug("Validate = " + result.ToString());
            foreach (IPurchaseReceipt productReceipt in result)
            {
                MyDebug("Valid receipt for " + productReceipt.productID.ToString());
                if (String.Equals(args.purchasedProduct.definition.id, monthlySubscription, StringComparison.Ordinal))
                {
                    UpdatePaymentStatus(PaymentStatus.s_monthly);
                    loadingScreen = GameObject.Find("LoadingCanvas").GetComponent<LoadingManager>();
                    loadingScreen.EndLoading();
                    return PurchaseProcessingResult.Complete;
                }
                else if (String.Equals(args.purchasedProduct.definition.id, yearlySubscription, StringComparison.Ordinal))
                {
                    UpdatePaymentStatus(PaymentStatus.s_yearly);
                    loadingScreen = GameObject.Find("LoadingCanvas").GetComponent<LoadingManager>();
                    loadingScreen.EndLoading();
                    return PurchaseProcessingResult.Complete;
                }
                else
                {
                    Debug.Log("Purchase Failed");
                }
            }
        }
        catch (Exception e)
        {
            MyDebug("Error is " + e.Message.ToString());
        }

        MyDebug(string.Format("ProcessPurchase: " + args.purchasedProduct.definition.id));

        loadingScreen = GameObject.Find("LoadingCanvas").GetComponent<LoadingManager>();
        loadingScreen.EndLoading();

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, PurchaseFailureReason failureReason)
    {
        MyDebug(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));

        SceneManager.LoadScene(SceneName.s_loginScene);
        loadingScreen = GameObject.Find("LoadingCanvas").GetComponent<LoadingManager>();
        loadingScreen.EndLoading();
    }

    private void MyDebug(string debug)
    {
        Debug.Log(debug);
    }

    private void UpdatePaymentStatus(string paymentStatus)
    {
        try
        {
            if (userAPIController == null)
                userAPIController = GetComponent<UserAPIController>();
                userAPIController.UpdatePaymentStatus(
                paymentStatus,
                (OnSuccess) =>
                {
                    print(OnSuccess);
                    SceneManager.LoadScene(SceneName.s_loginScene);
                },
                (OnFailure) =>
                {
                    print(OnFailure);
                    SceneManager.LoadScene(SceneName.s_loginScene);
                }
            );
        }
        catch (UserException e)
        {
            Debug.Log(e.Message);
        }
    }

    private void PaymentStatusToNone()
    {
        try
        {
            if (userAPIController == null)
                userAPIController = GetComponent<UserAPIController>();

            userAPIController.UpdatePaymentStatus(
                PaymentStatus.s_none,
                (OnSuccess) =>
                {
                    loadingScreen = GameObject.Find("LoadingCanvas").GetComponent<LoadingManager>();
                    loadingScreen.EndLoadingWithDelay();
                    SceneManager.LoadScene(SceneName.s_gameMenu);
                    
                },
                (OnFailure) =>
                {
                    loadingScreen = GameObject.Find("LoadingCanvas").GetComponent<LoadingManager>();
                    loadingScreen.EndLoadingWithDelay();
                }
            );
        }
        catch (UserException e)
        {
            Debug.Log(e.Message);
        }
    }
}

//https://forum.unity.com/threads/google-play-subscription-upgrade-downgrade.528434/
//https://forum.unity.com/threads/ios-subscription-upgrade-downgrade.804687/#post-7809135