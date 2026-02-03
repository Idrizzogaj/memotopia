using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.Script.Constants;
using Assets.Script.Controllers;
using Assets.Script.Models;
using Assets.Script.Utilities;
using UnityEngine.SceneManagement;
using Facebook.Unity;
using System.Collections.Generic;
using UnityEngine.SignInWithApple;

public class AuthView : MonoBehaviour
{
    public InputField usernameLogIn;
    public InputField passwordLogIn;

    public InputField usernameSignUp;
    public InputField emailSignUp;
    public InputField firstPasswordSignUp;
    public InputField secondPasswordSignUp;

    public InputField emailForgotPassword;
    public Text forgotPasswordText;

    public GameObject errorLogin;
    public GameObject errorSignUp;

    public GameObject iOSSocialLogin;
    public GameObject AndroidSocialLogin;

    public Toggle toggleTermsAndConditions;

    public GameObject logInScreen;
    public GameObject signUpScreen;
    public GameObject forgotPasswordScreen;

    LoadingManager _loadingScreen;
    AuthAPIController _authApiController;
    DataController _dataController;
    ValidationUtilities _validationUtilities;
    UserAPIController _userAPIController;
    string aToken;

    void Awake()
    {
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
        }
#if UNITY_IPHONE
        iOSSocialLogin.SetActive(true);
        AndroidSocialLogin.SetActive(false);
#endif
#if UNITY_ANDROID
        iOSSocialLogin.SetActive(false);
        AndroidSocialLogin.SetActive(true);
#endif
    }

    // Start is called before the first frame update
    void Start()
    {
        _authApiController = gameObject.AddComponent<AuthAPIController>();
        _dataController = gameObject.AddComponent<DataController>();
        _validationUtilities = gameObject.AddComponent<ValidationUtilities>();
        _userAPIController = gameObject.AddComponent<UserAPIController>();
        _loadingScreen = GameObject.Find("LoadingCanvas").GetComponent<LoadingManager>();
        _dataController.MenuLastPage = NavigationConstants.s_homeNav;

        if (!String.IsNullOrEmpty(_dataController.Token))
        {
            getUser();
        }
        else
        {
            _loadingScreen.EndLoading();
        }
    }

    void Update() {
        if (usernameLogIn.isFocused || passwordLogIn.isFocused && errorLogin.active)
            errorLogin.SetActive(false);

        if (usernameSignUp.isFocused || emailSignUp.isFocused || firstPasswordSignUp.isFocused || secondPasswordSignUp.isFocused  && errorSignUp.active)
            errorSignUp.SetActive(false);
    }

    public void loginWithFacebook()
    {
        var perms = new List<string>() { "public_profile", "email" };
        FB.LogInWithReadPermissions(perms, AuthCallback);
    }

    public void loginWithApple()
    {
    #if UNITY_IOS
        var siwa = gameObject.GetComponent<SignInWithApple>();
        siwa.Login(OnAppleLogin);
    #endif
    }

    public void logIn()
    {
        if (usernameLogIn.text.Trim() == String.Empty || passwordLogIn.text.Trim() == String.Empty)
        {
            HandleConditionWithAlertMessages();
            return;
        }

        try
        {
            _authApiController.PerformLogin(usernameLogIn.text, passwordLogIn.text,
                (OnSuccess) =>
                {
                    _loadingScreen.GoToSceneWithLoading(SceneName.s_gameMenu);
                },
                (OnFailure) =>
                {
                    _loadingScreen.EndLoading();
                    HandleConditionWithAlertMessages(OnFailure);
                }
            );
        }
        catch (UserException e)
        {
            Debug.Log(e.Message);
        }
    }

    public void signUp()
    {
        AlertMessageContainer alertMessageContainer = null;

        if(usernameSignUp.text.Trim() == String.Empty || emailSignUp.text.Trim() == String.Empty || firstPasswordSignUp.text.Trim() == String.Empty || secondPasswordSignUp.text.Trim() == String.Empty)
        {
            HandleConditionWithAlertMessages(null, false);
            return;
        }

        if (usernameSignUp.text.Trim().Length < 5 || usernameSignUp.text.Trim().Length > 25)
        {
            alertMessageContainer = new AlertMessageContainer() { ErrorMessage = ErrorMessages.s_shortUsernameCode, StatusCode = null };
            HandleConditionWithAlertMessages(alertMessageContainer);
            return;
        }

        if (firstPasswordSignUp.text.Trim().Length < 8 || firstPasswordSignUp.text.Trim().Length > 20)
        {
            alertMessageContainer = new AlertMessageContainer() { ErrorMessage = ErrorMessages.s_shortPasswordCode, StatusCode = null };
            HandleConditionWithAlertMessages(alertMessageContainer);
            return;
        }

        if (!_validationUtilities.IsValidEmail(emailSignUp.text.Trim()))
        {
            alertMessageContainer = new AlertMessageContainer() { ErrorMessage = ErrorMessages.s_invalidEmailCode, StatusCode = null };
            HandleConditionWithAlertMessages(alertMessageContainer);
            return;
        }

        if(firstPasswordSignUp.text.Trim() != secondPasswordSignUp.text.Trim())
        {
            alertMessageContainer = new AlertMessageContainer() { ErrorMessage = ErrorMessages.s_passwordsShouldMatchCode, StatusCode = null };
            HandleConditionWithAlertMessages(alertMessageContainer);
            return;
        }

        if(toggleTermsAndConditions.isOn == false)
        {
            alertMessageContainer = new AlertMessageContainer() { ErrorMessage = ErrorMessages.s_toggleNotClicked, StatusCode = null };
            HandleConditionWithAlertMessages(alertMessageContainer);
            return;
        }

        try
        {
            _authApiController.PerformSignup(usernameSignUp.text, firstPasswordSignUp.text, emailSignUp.text,
                (OnSuccess) =>
                {
                    _loadingScreen.GoToSceneWithLoading(SceneName.s_gameMenu);
                },
                (OnFailure) =>
                {
                    _loadingScreen.EndLoadingWithDelay();
                    HandleConditionWithAlertMessages(OnFailure, false);
                }
            );
        }
        catch (UserException e)
        {
            Debug.Log(e.Message);
        }
    }

    public void getUser()
    {
        try
        {
            _userAPIController.GetUser(
                (OnSuccess) =>
                {
                    _loadingScreen.GoToSceneWithLoading(SceneName.s_gameMenu);
                },
                (OnFailure) =>
                {
                    _loadingScreen.EndLoadingWithDelay();
                    HandleConditionWithAlertMessages(OnFailure);
                }
            );
        }
        catch (UserException e)
        {
            Debug.Log(e.Message);
        }
    }

    public void forgotPassword()
    {
        if (!_validationUtilities.IsValidEmail(emailForgotPassword.text.Trim()))
        {
            // alertMessageContainer = new AlertMessageContainer() { ErrorMessage = ErrorMessages.s_invalidEmailCode, StatusCode = null };
            // HandleConditionWithAlertMessages(alertMessageContainer);
            return;
        }

        try
        {
            _authApiController.ForgotPassword(emailForgotPassword.text,
                (OnSuccess) =>
                {
                    forgotPasswordText.gameObject.SetActive(true);
                },
                (OnFailure) =>
                {
                    forgotPasswordText.text = "this email doesn't exist";
                    forgotPasswordText.gameObject.SetActive(true);
                    // HandleConditionWithAlertMessages(OnFailure);
                }
            );
        }
        catch (UserException e)
        {
            Debug.Log(e.Message);
        }
    }

    public void goToScreen(string screen)
    {
        switch(screen)
        {
            case "logIn":
                signUpScreen.SetActive(false);
                forgotPasswordScreen.SetActive(false);
                logInScreen.SetActive(true);
                break;
            case "signUp":
                logInScreen.SetActive(false);
                forgotPasswordScreen.SetActive(false);
                signUpScreen.SetActive(true);
                break;
            case "forgotPassword":
                logInScreen.SetActive(false);
                signUpScreen.SetActive(false);
                forgotPasswordScreen.SetActive(true);
                break;
        }
    }

    private void HandleConditionWithAlertMessages(AlertMessageContainer response = null, bool isLogin = true, bool isSocialLogin = false)
    {
        if(isSocialLogin)
        {
            errorLogin.GetComponent<Text>().text = ErrorMessages.s_somethingWentWrong;
            errorLogin.SetActive(true);
        }

        if (response != null) 
        {
            if(response.ErrorMessage == ErrorMessages.s_invalidCredentials)
            {
                errorLogin.GetComponent<Text>().text = ErrorMessages.s_wrongCredentials;
                errorLogin.SetActive(true);
            }
            else if(response.ErrorMessage == ErrorMessages.s_userRegisterBadInputCode)
            {
                errorSignUp.GetComponent<Text>().text = ErrorMessages.s_userRegisterBadInputMessage;
                errorSignUp.SetActive(true);
            }
            else if (response.ErrorMessage == ErrorMessages.s_invalidEmailCode)
            {
                errorSignUp.GetComponent<Text>().text = ErrorMessages.s_invalidEmailMessage;
                errorSignUp.SetActive(true);
            }
            else if (response.ErrorMessage == ErrorMessages.s_shortUsernameCode)
            {
                errorSignUp.GetComponent<Text>().text = ErrorMessages.s_shortUsernameMessage;
                errorSignUp.SetActive(true);
            }
            else if (response.ErrorMessage == ErrorMessages.s_shortPasswordCode)
            {
                errorSignUp.GetComponent<Text>().text = ErrorMessages.s_shortPasswordMessage;
                errorSignUp.SetActive(true);
            }
            else if (response.ErrorMessage == ErrorMessages.s_passwordsShouldMatchCode)
            {
                errorSignUp.GetComponent<Text>().text = ErrorMessages.s_passwordsShouldMatchMessage;
                errorSignUp.SetActive(true);
            }
            else if (response.ErrorMessage == ErrorMessages.s_toggleNotClicked)
            {
                errorSignUp.GetComponent<Text>().text = ErrorMessages.s_toggleNotClickedMessage;
                errorSignUp.SetActive(true);
            }
            else if (response.ErrorMessage == ErrorMessages.s_invalidProviderCode)
            {
                errorSignUp.GetComponent<Text>().text = ErrorMessages.s_invalidProviderMessage;
                errorSignUp.SetActive(true);
            }
            else
            {
                errorSignUp.GetComponent<Text>().text = response.ErrorMessage;
                errorSignUp.SetActive(true);
            }
        }
        else 
        {
            if(isLogin)
            {
                errorLogin.GetComponent<Text>().text = ErrorMessages.s_fillAllFields;
                errorLogin.SetActive(true);
            }
            else 
            {
                errorSignUp.GetComponent<Text>().text = ErrorMessages.s_fillAllFields;
                errorSignUp.SetActive(true);
            }
        }
    }

    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            // ...
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }

    private void AuthCallback(ILoginResult result)
    {
        if (result.Error != null)
        {
            Debug.Log(result.Error);
        }
        else
        {
            if (FB.IsLoggedIn)
            {
                aToken = AccessToken.CurrentAccessToken.TokenString;
                FB.API("/me?fields=id", HttpMethod.GET, GetFbUserIdAndLogin);
            }
            else
            {
                Debug.Log("FB is not loggedin");
            }
        }
    }

    private void GetFbUserIdAndLogin(IResult result)
    {
        if (result.Error == null)
        {
            string id = (string)result.ResultDictionary["id"];

            try
            {
                _authApiController.PerformSocialLogin(id, aToken, "FACEBOOK",
                    (OnSuccess) =>
                    {
                        _loadingScreen.GoToSceneWithLoading(SceneName.s_gameMenu);
                    },
                    (OnFailure) =>
                    {
                        _loadingScreen.EndLoading();
                        HandleConditionWithAlertMessages(null, false, true);
                    }
                );
            }
            catch (UserException e)
            {
                Debug.Log(e.Message);
            }

            //FB.API("/"+id, HttpMethod.GET, DisplayFriends);
        }
        else
        {
            Debug.Log(result.Error);
        }
    }

    private void DisplayFriends(IResult result)
    {
        if (result.Error == null)
        {
            print(result);
        }
        else
        {
            Debug.Log(result.Error);
        }
    }

    private void DisplayProfilePic(IGraphResult result)
    {

        if (result.Texture != null)
        {
            //Image ProfilePic = DialogProfilePic.GetComponent<Image>();
            //ProfilePic.sprite = Sprite.Create(result.Texture, new Rect(0, 0, 200, 200), new Vector2());    
        }
    }

    private void OnAppleLogin(SignInWithApple.CallbackArgs args)
    {
        if (args.error != null)
        {
            Debug.Log("Errors occurred: " + args.error);
            return;
        }

        UserInfo userInfo = args.userInfo;

        try
        {
            _authApiController.PerformSocialLogin(userInfo.userId, userInfo.idToken, "APPLE",
                (OnSuccess) =>
                {
                    _loadingScreen.GoToSceneWithLoading(SceneName.s_gameMenu);
                },
                (OnFailure) =>
                {
                    _loadingScreen.EndLoading();
                    HandleConditionWithAlertMessages(null, false, true);
                }
            );
        }
        catch (UserException e)
        {
            Debug.Log(e.Message);
        }

        // Print out information about the user who logged in.
        Debug.Log(
            string.Format("Display Name: {0}\nEmail: {1}\nUser ID: {2}\nID Token: {3}", userInfo.displayName ?? "",
                userInfo.email ?? "", userInfo.userId ?? "", userInfo.idToken ?? ""));
    }

    public void linkBtn()
    {
        Application.OpenURL("http://memotopia.com/pp/privacy-and-policy.pdf?fbclid=IwAR09a-hC9bZbhZtb2gFGvOwAPs9jMDWenQ_wozqDADBhKW1qo2Us_jL7jBg");
    }
}
