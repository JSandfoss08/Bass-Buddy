using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JetBrains.Annotations;
using System;

public class UserItemUI : MonoBehaviour
{
    public TMP_Text usernameText;
    public Button userButton;
    private UI_Login ui;

    [Header("Popups")]
    public GameObject loginUserPopupObject;
    private User user;

    public void Start()
    {
        userButton.onClick.AddListener(ShowLoginView);
    }

    public void Setup(User userData, UI_Login ui_, GameObject popupObject)
    {
        ui = ui_;
        user = userData;
        usernameText.text = user.Username;
        loginUserPopupObject = popupObject;
    }

    public void ShowLoginView()
    {
        ui.ShowLoginView(user);
    }

    public int currentUserID = -1;

    List<(string username, string password)> users = new List<(string, string)>(){
        ("Alex", "Alex1"),
        ("Jared", "Jared1"),
        ("Myca", "Myca1")
    };

    // Characters that can be used for SecurityTest()
    // -Alex
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";

    // In the inspector window, right click on the top of this script component or left-click the three dots on the far right,
    // then click "Security Test" to run this method.
    // -Alex
    /*
    [ContextMenu("Security Test")]
    void SecuityTest(){
        Debug.Log("Starting Security Check");
        List<(string username, string password)> successfulLogins = new List<(string, string)>();

        Debug.Log("Starting checks for known users");
        foreach(var user in users){
            usernameText.text = user.username;
            for(int i = 0; i < users.Count; i++){
                passwordInput.text = users[i].password;
                if(TryLogin()){
                    // Requires two sets of parenthesis so Add() treats it as one object -Alex
                    successfulLogins.Add((usernameText.text, passwordInput.text));
                }
            }
        }
        Debug.Log("Finished checks for known users");
        
        Debug.Log("Starting random input checks");
        for(int i = 0; i < 100; i++){
            usernameText.text = GenerateRandomString(i);
            passwordInput.text = GenerateRandomString(i);
            if(TryLogin()){
                successfulLogins.Add((usernameText.text, passwordInput.text));
            }
        }
        Debug.Log("Finsihed random input checks");

        Debug.Log("Starting checks for all successful logins");
        bool failedSecurityCheck = false;
        foreach(var success in successfulLogins){
            bool known = false;
            foreach(var user in users){
                if(success.username == user.username && success.password == user.password){
                    known = true;
                    break;
                }
            }
            if(!known){
                Debug.LogWarning("Successful login for unknown user! username == " + success.username + " password == " + success.password);
                failedSecurityCheck = true;
            }
        }
        Debug.Log("Finsihed checks for all successful logins");
        if(failedSecurityCheck){
            Debug.Log("Security Check FAILED");
        }else{
            Debug.Log("Security Check PASSED");
        }

        // Clean up after testing -Alex
        usernameText.text = "";
        passwordInput.text = "";
    }

    string GenerateRandomString(int length)
    {
        string toReturn = "";
        for (int i = 0; i < length; i++)
        {
            // Random.Range(int, int) is inclusive of the min but exclusive of the max.
            // However, b/c all indexes are 0 to array.Length - 1, all characters can still be referenced.
            // -Alex
            toReturn += chars[UnityEngine.Random.Range(0, chars.Length)];
        }
        return toReturn;
    }

    // Calls to a Button's OnPress() can't return a type, it causes a runtime error. Workaround.
    // -Alex
    public void ButtonTryLogin() {
        if (TryLogin()) {
            LoadSceneManager.Instance.LoadMainMenu();
        }
    }
    

    // See ButtonTryLogin() comment, do not call this method for any Button's OnPress().
    // Keeping this method private avoids it appearing in the inspector, keep as so if possible.
    // -Alex
    bool TryLogin(){
        // Null reference error protection. May not be strictly necessary. -Alex
        if (usernameText.text == null) return false;
        if(passwordInput.text == null) return false;

        int user_id = DatabaseHandler.Instance.Login(usernameText.text, passwordInput.text);
        if (user_id != -1) return true;

        Debug.Log("Failed to login as " + usernameText.text + " with password " + passwordInput.text);
        return false;
    }
    */

    public void Return(){
        LoadSceneManager.Instance.LoadMainMenu();
    }
}