using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour
{
    [Header("Lesson Select Menu")]
    private Animator animator;
    public float displayDurationAfterHide = 1;
    public GameObject lessonPopupObject;
    public Button ShowButton;
    public Button HideButton;
    public Transform lessonListContainer;
    public GameObject lessonItemPrefab;


    [Header("Menu Buttons")]
    public Button switchUserButton;
    public Button quitButton;
    public Button tuneButton;
    public Button creditsButton;

    private int currentUserID;
    public TMP_Text greetingText;

    public float creditsMenuOpenTime;
    public float creditsMenuCloseTime;
    public Transform creditsMenu;

    void Start()
    {
        // Get userID
        currentUserID = PlayerPrefs.GetInt("CurrentUserID", -1);

        if (currentUserID == -1)
        {
            Debug.Log("No user logged in!");
            LoadSceneManager.Instance.LoadLogin();
            return;
        }

        // Add listeners to buttons
        tuneButton.onClick.AddListener(LoadTune);
        switchUserButton.onClick.AddListener(LoadLogin);
        quitButton.onClick.AddListener(ExitApplication);
        ShowButton.onClick.AddListener(ShowPopup);
        HideButton.onClick.AddListener(HidePopup);
        creditsButton.onClick.AddListener(OpenCreditsMenu);

        lessonPopupObject.SetActive(false);
        animator = lessonPopupObject.GetComponent<Animator>();

        User user = DatabaseHandler.Instance.GetUser(currentUserID);
        if (user != null)
        {
            greetingText.text = $"Wecome {user.Username}!";
        }
        else
        {
            greetingText.text = "Not logged in!";
        }
        
        LoadLessons();
    }

    public void LoadLessons()
    {
        var lessonsWithProgress = DatabaseHandler.Instance.GetLessonsWithProgress(currentUserID);

        // Clear any previous items
        foreach (Transform child in lessonListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var lessonData in lessonsWithProgress)
        {
            GameObject lessonItem = Instantiate(lessonItemPrefab, lessonListContainer);

            LessonItemUI itemUI = lessonItem.GetComponent<LessonItemUI>();

            if (itemUI != null)
            {
                itemUI.Setup(lessonData.Lesson, lessonData.Progress, currentUserID);
            } 
        }
    }
 
    public void ShowPopup()
    {
        lessonPopupObject.SetActive(true);
        animator.SetBool("Show", true);
    }

    public void HidePopup()
    {
        animator.SetBool("Show", false);
        StartCoroutine(DisableWithDelay(0.30f));
    }

    IEnumerator DisableWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        lessonPopupObject.SetActive(false);
    }

    public void LoadLogin()
    {
        LoadSceneManager.Instance.LoadLogin();
    }

    public void LoadTune()
    {
        LoadSceneManager.Instance.LoadTuning();
    }

    public void ExitApplication()
    {
        LoadSceneManager.Instance.ExitApplication();
    }

    // Credits Menu
    public void OpenCreditsMenu(){
        StartCoroutine(OpenCreditsMenuAnimation());
    }

    public void CloseCreditsMenu(){
        StartCoroutine(CloseCreditsMenuAnimation());
    }

    IEnumerator OpenCreditsMenuAnimation(){
        float elapsedTime = 0f;
        creditsMenu.localScale = Vector3.zero;
        creditsMenu.gameObject.SetActive(true);
        while(elapsedTime < creditsMenuOpenTime){
            elapsedTime += Time.deltaTime;
            creditsMenu.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, elapsedTime / creditsMenuOpenTime);
            yield return null;
        }
        creditsMenu.localScale = Vector3.one;
    }

    IEnumerator CloseCreditsMenuAnimation(){
        float elapsedTime = 0f;
        while(elapsedTime < creditsMenuCloseTime){
            elapsedTime += Time.deltaTime;
            creditsMenu.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, elapsedTime / creditsMenuCloseTime);
            yield return null;
        }
        creditsMenu.localScale = Vector3.zero;
        creditsMenu.gameObject.SetActive(false);
    }
}
