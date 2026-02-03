using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.Script.Constants;
using Assets.Script.Controllers;
using Assets.Script.Models;
using Assets.Script.Utilities;
using UnityEngine.SceneManagement;

public class AccountView : MonoBehaviour
{
    public GameObject[] avatarRings;
    public InputField newUsername;
    public GameObject errorNewUsername;
    public GameObject changeUsernameText;
    public GameObject saveBtn;

    UserAPIController _userAPIController;

    String avatarImageName = "";

    MenuNavigation mn;


    // Use this for initialization
    void Start()
    {
        _userAPIController = gameObject.AddComponent<UserAPIController>();
        newUsername.placeholder.GetComponent<Text>().text = UserConstants.s_user.username;
        mn = gameObject.GetComponent<MenuNavigation>();
        pickUserAvatar();
    }

    // Update is called once per frame
    void Update()
    {
        if (newUsername.text.Trim() != String.Empty || avatarImageName != String.Empty)
        {
            saveBtn.GetComponent<Button>().interactable = true;
        }
        else
        {
            saveBtn.GetComponent<Button>().interactable = false;
        }

        if (newUsername.isFocused && errorNewUsername.active)
        {
            errorNewUsername.SetActive(false);
            changeUsernameText.SetActive(true);
        }
    }

    public void OnClickedSelect(GameObject go)
    {

        for (int i = 0; i < avatarRings.Length; i++)
        {
            avatarRings[i].SetActive(false);
        }

        go.SetActive(true);

        switch (go.name)
        {
            case "AvatarRing_1":
                avatarImageName = "MaskGroup54";
                break;
            case "AvatarRing_2":
                avatarImageName = "MaskGroup55";
                break;
            case "AvatarRing_3":
                avatarImageName = "MaskGroup56";
                break;
            case "AvatarRing_4":
                avatarImageName = "MaskGroup58";
                break;
            case "AvatarRing_5":
                avatarImageName = "Group16016";
                break;
            case "AvatarRing_6":
                avatarImageName = "MaskGroup57";
                break;
        }
    }

    public void UpdateAccount()
    {
        try
        {
            _userAPIController.UpdateAccount(newUsername.text, avatarImageName,
                (OnSuccess) =>
                {
                    //_loadingScreen.EndLoading();
                    //SceneManager.LoadScene(SceneName.s_gameMenu);
                    print("done");
                    mn.GoToHome();
                },
                (OnFailure) =>
                {
                    //_loadingScreen.EndLoading();
                    HandleConditionWithAlertMessages(OnFailure);
                }
            );
            avatarImageName = "";
            newUsername.text = "";
        }
        catch (UserException e)
        {
            Debug.Log(e.Message);
        }
    }

    private void pickUserAvatar()
    {
        for (int i = 0; i < avatarRings.Length; i++)
        {
            avatarRings[i].SetActive(false);
        }

        switch (UserConstants.s_user.avatar)
        {
            case "MaskGroup54":
                avatarRings[0].SetActive(true);
                break;
            case "MaskGroup55":
                avatarRings[1].SetActive(true);
                break;
            case "MaskGroup56":
                avatarRings[2].SetActive(true);
                break;
            case "MaskGroup58":
                avatarRings[3].SetActive(true);
                break;
            case "Group16016":
                avatarRings[4].SetActive(true);
                break;
            case "MaskGroup57":
                avatarRings[4].SetActive(true);
                break;
        }
    }

    private void HandleConditionWithAlertMessages(AlertMessageContainer response = null)
    {
        if (response.ErrorMessage == ErrorMessages.s_usernameExistsCode)
        {
            errorNewUsername.GetComponent<Text>().text = ErrorMessages.s_usernameExistsMessage;
            changeUsernameText.SetActive(false);
            errorNewUsername.SetActive(true);
        }
    }
}
