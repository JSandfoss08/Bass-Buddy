using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Data.Common;
using System;

public class UI_Login : MonoBehaviour
{
    [Header("Input Fields")]
    
    private string currentUsername;

    [Header("Login")]
    private bool loginPageActive = false;
    public TMP_Text loginGreetingText;
    public Button loginButton;
    public Button loginBackButton;
    public Toggle loginUserIsLeftHandedToggle;
    public GameObject loginUserPopupObject;
    public TMP_Text loginFeedbackText;
    public TMP_Text lastLoggedInAtText;
    public TMP_InputField loginPasswordInput;
    public Button removeUserButton;

    [Header("Add User")]
    private bool addUserPageActive = false;
    public Button addUserButton;
    public Button addUserConfirmButton;
    public Button addUserBackButton;
    public Toggle addUserIsLeftHandedToggle;
    public GameObject addUserPopupObject;
    public TMP_Text addUserFeedbackText;
    public TMP_InputField addUserUsernameInput;
    public TMP_InputField addUserPasswordInput;
    
    [Header("UI Elements")]
    public Transform userPanelContainer;
    public GameObject userItemPrefab;

    public Button quitApplicationButton;
    public int currentUserID = -1;

    void Start()
    {
        // Add listeners for button click events
        if (loginButton) loginButton.onClick.AddListener(OnLoginClicked);
        if (addUserConfirmButton) addUserConfirmButton.onClick.AddListener(OnRegisterClicked);
        if (addUserBackButton) addUserBackButton.onClick.AddListener(HideAddUserView);
        if (addUserButton) addUserButton.onClick.AddListener(ShowAddUserView);
        if (loginBackButton) loginBackButton.onClick.AddListener(HideLoginView);
        if (removeUserButton) removeUserButton.onClick.AddListener(OnRemoveUserClicked);
        if (quitApplicationButton) quitApplicationButton.onClick.AddListener(ExitApplication);

        if (addUserPopupObject) addUserPopupObject.SetActive(false);
        if (loginUserPopupObject) loginUserPopupObject.SetActive(false);

        loginFeedbackText.text = "";
        addUserFeedbackText.text = "";
        addUserIsLeftHandedToggle.isOn = false;

        // Wait for database to initialize before refreshing user list
        StartCoroutine(WaitAndRefreshUserList());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (loginPageActive)
                OnLoginClicked();
            else if (addUserPageActive)
                OnRegisterClicked();
        }
    }

    IEnumerator WaitAndRefreshUserList()
    {
        // Wait until DatabaseHandler is ready
        while (DatabaseHandler.Instance == null)
        {
            yield return null;
        }

        yield return new WaitForEndOfFrame();

        RefreshUserList();
    }

    void RefreshUserList()
    {
        if (DatabaseHandler.Instance == null)
        {
            Debug.LogError("DatabaseHandler is not available!");
            return;
        }

        // Clear existing items
        foreach (Transform child in userPanelContainer)
        {
            Destroy(child.gameObject);
        }

        // Get all users from database
        List<User> users = DatabaseHandler.Instance.GetAllUsers();

        Debug.Log($"Found {users.Count} users");

        // Create UI item for each user
        foreach (User user in users)
        {
            if (userItemPrefab == null)
            {
                Debug.LogError("userItemPrefab is not assigned!");
                break;
            }

            GameObject userItem = Instantiate(userItemPrefab, userPanelContainer);
            UserItemUI ui = userItem.GetComponent<UserItemUI>();

            if (ui != null)
            {
                ui.Setup(user, this, loginUserPopupObject);
            }
            else
            {
                Debug.LogError("UserItemUI component not found on userItemPrefab!");
            }
        }
    }

    void OnLoginClicked()
    {
        string username = currentUsername.Trim();
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowLoginFeedback("Please enter both username and password", Color.red);
            return;
        }

        currentUserID = DatabaseHandler.Instance.Login(username, password);

        if (currentUserID != -1)
        {
            ShowLoginFeedback("Login successful!", Color.green);
            PlayerPrefs.SetInt("CurrentUserID", currentUserID);
            PlayerPrefs.Save();

            bool IsLeftHanded = loginUserIsLeftHandedToggle.isOn;
            DatabaseHandler.Instance.UpdateUserHandedness(currentUserID, IsLeftHanded);

            // Login with slight delay to show feedback
            Invoke("Return", 1);
        }
        else
        {
            ShowLoginFeedback("Invalid username or password", Color.red);
        }
    }

    void OnRemoveUserClicked()
    {
        string username = currentUsername.Trim();
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowLoginFeedback("Please enter password to confirm user to delete", Color.red);
            return;
        }

        bool removed = DatabaseHandler.Instance.RemoveUser(currentUsername, password);

        if (removed)
        {
            RefreshUserList();
            HideLoginView();
            loginPasswordInput.text = "";
            loginFeedbackText.text = "";
        }   
    }

    void OnRegisterClicked()
    {
        string username = addUserUsernameInput.text.Trim();
        string password = addUserPasswordInput.text;
        bool isLeftHanded = addUserIsLeftHandedToggle.isOn;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowAddUserFeedback("Please enter both username and password", Color.red);
            return;
        }

        if (username.Length < 3)
        {
            ShowAddUserFeedback("Username must be at least 3 characters", Color.red);
            return;
        }

        if (password.Length < 6)
        {
            ShowAddUserFeedback("Password must be at least 6 characters", Color.red);
            return;
        }

        bool success = DatabaseHandler.Instance.AddUser(username, password, isLeftHanded);

        if (success)
        {
            ShowAddUserFeedback("Account created successfully! You can now login with your new account.", Color.green);
            addUserUsernameInput.text = "";
            addUserPasswordInput.text = "";
            
            // Refresh the user list to show new user
            RefreshUserList();
            
            // Hide the add user popup if it was open
            HideAddUserView();
            addUserFeedbackText.text = "";
        }
        else
        {
            ShowAddUserFeedback("Username already exists", Color.red);
        }
    }

    void ShowLoginFeedback(string message, Color color)
    {
        if (loginFeedbackText != null)
        {
            loginFeedbackText.text = message;
            loginFeedbackText.color = color;
        }
    }

    void ShowAddUserFeedback(string message, Color color)
    {
        if (addUserFeedbackText != null)
        {
            addUserFeedbackText.text = message;
            addUserFeedbackText.color = color;
        }
    }

    void ShowAddUserView()
    {
        if (addUserPopupObject != null)
        {
            addUserPageActive = true;
            addUserPopupObject.SetActive(true);
        }
    }

    void HideAddUserView()
    {
        if (addUserPopupObject != null)
        {
            addUserPageActive = false;
            addUserUsernameInput.text = "";
            addUserPasswordInput.text = "";
            addUserPopupObject.SetActive(false);
        }
    }

    public void ShowLoginView(User user)
    {
        if (loginUserPopupObject != null)
        {
            loginUserIsLeftHandedToggle.isOn = user.IsLeftHanded;
            loginPageActive = true;
            currentUsername = user.Username;
            lastLoggedInAtText.text = user.LastLoggedInAt.ToString("MMMM d, yyyy h:mmtt");
            loginGreetingText.text = $"Hello {currentUsername}!";
            Debug.Log(currentUsername);
            loginUserPopupObject.SetActive(true);
        }
    }

    void HideLoginView()
    {
        if (loginUserPopupObject != null)
        {
            loginPageActive = false;
            currentUsername = "";
            loginPasswordInput.text = "";
            loginUserPopupObject.SetActive(false);
        }
    }

    public void Return()
    {
        LoadSceneManager.Instance.LoadMainMenu();
    }

    public void ExitApplication()
    {
        LoadSceneManager.Instance.ExitApplication();
    }
}