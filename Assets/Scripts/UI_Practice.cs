using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_Practice : MonoBehaviour
{
    public RectTransform leftHandedFlipObject;
    public GameObject[] strings;

    [Header("External References")]
    public LessonGameplay practiceManager;

    [Header("Play Instructions")]
    public RectTransform pressParent;
    public TMP_Text targetName;
    public RectTransform displayParent;
    public RectTransform timingIndicator;
    public Animator timingIndicatorAnimator;
    public Vector2 timingIndicatorStart, timingIndicatorEnd;
    public float timingIndicatorAheadOffset;

    [Header("Completion UI")]
    public float endMenuDelay;
    public GameObject completionPanel;
    public TMP_Text completionTitleText;
    public TMP_Text completionScoreText;
    public TMP_Text completionDetailsText;
    public Button returnButton;
    public Button retryButton;
    public Doggo doggo;
    public ParticleSystem[] particleSystems;

    [Header("Progress UI")]
    public Image timeProgressBar;
    public Image correctnessProgressBar;
    public float correctnessProgressBarFillDuration = 0.8f;
    public TMP_Text progressText;
    public TMP_Text testedNoteCountText;

    [Header("Play Accuracy Feedback")]
    public TMP_Text timingFeedbackText;
    public float feedbackTextAppearTime;
    public float feedbackTextLifetime;
    public float feedbackTextDissappearTime;
    public float displayApplyTime = 0.125f;
    public float displayRevertTime = 1f;

    [Header("Pause Menu")]
    public Transform pauseMenu;
    public Transform pauseMenuUnpauseButton;
    public TMP_Text pauseMenuCountdown;
    public float pauseMenuUnpauseDelay;
    public float pauseMenuOpenTime;
    public float pauseMenuCloseTime;
    public Button pauseButton;
    public Button practiceReturnButton;
    private bool isPaused = false;

    private Coroutine currentCoroutine;
    private Coroutine pauseCoroutine;
    private bool lessonEnd = false;

    void Start()
    {
        // Hide completion panel at start
        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }

        // Setup button listeners
        if (returnButton != null)
            returnButton.onClick.AddListener(Return);
        if (retryButton != null)
            retryButton.onClick.AddListener(RetryLesson);
        if (practiceReturnButton != null)
            practiceReturnButton.onClick.AddListener(Return);
        if (pauseButton != null)
            pauseButton.onClick.AddListener(Pause);

        timingFeedbackText.text = "";

        if (timeProgressBar != null)
            timeProgressBar.fillAmount = 0;
        if (correctnessProgressBar != null)
            correctnessProgressBar.fillAmount = 0;
    }

    [ContextMenu("Load Random Note")]
    void LoadRandomNote(){
        LoadNote(Note.GetRandomNote(), 5f);
    }

    [ContextMenu("Load Random Chord")]
    void LoadRandomChord()
    {
        LoadChord(Chord.GetRandomChord(), 5f);
    }

    public void LoadNote(Note note, float timeUntilPlay){
        ClearGuitar();

        if(note == Note.None){
            EndLesson();
            return;
        }

        targetName.text = note.GetName();

        int stringIndex = note.GetStringIndex();
        int fretIndex = note.GetFretIndex();

        Debug.Log("Loading note " + note.GetName() + " with coords " + fretIndex + ", " + stringIndex);

        pressParent.GetChild(fretIndex + 5 * stringIndex).gameObject.SetActive(true);

        LoadTimingIndicator(note.GetName(), timeUntilPlay);
    }

    public void LoadChord(Chord chord, float timeUntilPlay)
    {
        ClearGuitar();

        targetName.text = chord.GetName();

        if(chord == Chord.None)
        {
            HideTimingIndicator();
            return;
        }

        Debug.Log("Loading chord " + chord.GetName());

        Note[] notes = chord.GetNotes();
        for(int i = 0; i < notes.Length; i++)
        {
            pressParent.GetChild(notes[i].GetFretIndex() + 5 * notes[i].GetStringIndex()).gameObject.SetActive(true);
        }

        LoadTimingIndicator(chord.GetName(), timeUntilPlay);
    }

    [ContextMenu("Clear Chord")]
    public void ClearGuitar()
    {
        for (int i = 0; i < pressParent.childCount; i++)
        {
            pressParent.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void FlipDominantHand()
    {
        if (leftHandedFlipObject != null)
        {
            leftHandedFlipObject.localScale = new Vector3(-leftHandedFlipObject.localScale.x, leftHandedFlipObject.localScale.y, leftHandedFlipObject.localScale.z);
        }
    }

    // Display feedback calls and animations
    public void DisplaySuccess()
    {
        timingFeedbackText.text = "Good!";
        timingFeedbackText.color = Color.green;
        AnimateFeedbackText();
        SetDisplayColor(Color.green);
        StartCoroutine(AnimateDisplayColor(Color.white, Color.green, Color.white));
    }

    public void DisplayFailure(bool tooLate)
    {
        if (tooLate)
            timingFeedbackText.text = "Too Late!";
        else
            timingFeedbackText.text = "Wrong note!";
        timingFeedbackText.color = Color.red;
        AnimateFeedbackText();
        SetDisplayColor(Color.red);
        StartCoroutine(AnimateDisplayColor(Color.white, Color.red, Color.white));
    }

    void SetDisplayColor(Color color)
    {
        foreach (Image image in displayParent.GetComponentsInChildren<Image>())
        {
            image.color = color;
        }
    }

    // Feedback text animations
    void AnimateFeedbackText(){
        StartCoroutine(ShowFeedbackText());
    }

    IEnumerator ShowFeedbackText(){
        float elapsed = 0f;
        while(elapsed < feedbackTextAppearTime){
            Color color = timingFeedbackText.color;
            color.a = elapsed / feedbackTextAppearTime;
            timingFeedbackText.color = color;
            // Debug.Log("Appearing: " + timingFeedbackText.alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        // timingFeedbackText.alpha = 255;
        elapsed = 0f;

        while(elapsed < feedbackTextLifetime){
            elapsed += Time.deltaTime;
            // Debug.Log("Showing: " + elapsed);
            yield return null;
        }

        StartCoroutine(HideFeedbackText());
    }

    IEnumerator HideFeedbackText(){
        float remaining = feedbackTextDissappearTime;
        while(remaining >= 0){
            Color color = timingFeedbackText.color;
            color.a = remaining / feedbackTextAppearTime;
            timingFeedbackText.color = color;
            // Debug.Log("Dissappearing: " + timingFeedbackText.alpha);
            remaining -= Time.deltaTime;
            yield return null;
        }
        // timingFeedbackText.alpha = 0;
    }

    void EndLesson()
    {
        targetName.text = "";
        timingFeedbackText.text = "";
    }

    // Internally delayed
    IEnumerator ShowLessonEndWithDelay(bool passed, int correctNotes, int totalNotes, float delay)
    {
        float elapsed = 0f;
        while(elapsed < delay){
            elapsed += Time.deltaTime;
            yield return null;
        }

        lessonEnd = true;
        if (completionPanel == null)
        {
            Debug.LogWarning("Completion panel not assigned!");
            yield break;
        }

        // Calculate percentage
        float percentage = (correctNotes / (float)totalNotes) * 100f;

        // Show the completion panel
        completionPanel.SetActive(true);

        // Set the text
        if (completionTitleText != null)
        {
            if (passed)
            {
                completionTitleText.text = "Lesson Complete!";
                if (doggo != null)
                    doggo.MakeSuperHappy();
            }  
            else
            {
                completionTitleText.text = "Try Again!.";
                if (doggo != null)
                    doggo.MakeSad();
            } 
        }

        if (completionScoreText != null)
        {
            // Show percentage with color based on performance
            string scoreColor = GetScoreColor(percentage);
            completionScoreText.text = $"<color={scoreColor}>{percentage:F1}%</color>";
        }

        if (completionDetailsText != null)
        {
            completionDetailsText.text = $"You played {correctNotes} out of {totalNotes} notes correctly!";
        }

        // Hide the guitar display
        ClearGuitar();
        HideTimingIndicator();
    }

    // Actual call to show lesson completion screen
    public void ShowLessonEnd(bool passed, int correctNotes, int totalNotes){
        // Uses a coroutine to enforce a delay in the menu's appearance
        // There's probably a better way of doing this, I'm open to suggestions
        // -Alex
        StartCoroutine(ShowLessonEndWithDelay(passed, correctNotes, totalNotes, endMenuDelay));

        if (passed && particleSystems.Length > 0)
        {
            foreach (ParticleSystem particleSystem in particleSystems)
            {
                particleSystem.Play();
            }
        }  
    }

    // Helper method to get color based on score
    string GetScoreColor(float percentage)
    {
        if (percentage >= 90f) return "green";
        if (percentage >= 70f) return "yellow";
        if (percentage >= 50f) return "orange";
        return "red";
    }

    // Updates progress during lesson
    public void UpdateProgress(int correctNotes, int testedNoteCount, int totalNotes)
    {
        if (progressText != null && !lessonEnd)
        {
            progressText.text = $"{correctNotes} / {totalNotes}";
        }

        if (testedNoteCountText != null)
        {
            testedNoteCountText.text = $"{testedNoteCount} / {totalNotes}";
        }

        if (correctnessProgressBar != null)
        {
            float targetFill = Mathf.Clamp01((float)correctNotes / totalNotes);

            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(AnimateCorrectnessProgressBar(targetFill));
        }
    }
    
    public IEnumerator AnimateCorrectnessProgressBar(float targetFill)
    {
        float startFill = correctnessProgressBar.fillAmount;
        float elapsed = 0f;

        while (elapsed < correctnessProgressBarFillDuration)
        {
            elapsed += Time.deltaTime;
            
            float t = Mathf.Clamp01(elapsed / correctnessProgressBarFillDuration);
            correctnessProgressBar.fillAmount = Mathf.Lerp(startFill, targetFill, Mathf.SmoothStep(0f, 1f, t));
            correctnessProgressBar.color = Color.Lerp(Color.red, Color.green, correctnessProgressBar.fillAmount);
            yield return null;
        }

        correctnessProgressBar.fillAmount = targetFill;
        currentCoroutine = null;
    }

    public void StartProgressBar(float duration)
    {
        StopCoroutine(nameof(AnimateProgressBar));
        StartCoroutine(AnimateProgressBar(duration));
    }

    private IEnumerator AnimateProgressBar(float duration)
    {
        float startFill = 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            timeProgressBar.fillAmount = Mathf.Lerp(startFill, 1f, elapsed / duration);
            yield return null;
        }

        timeProgressBar.fillAmount = 1f;
    }

    IEnumerator AnimateDisplayColor(Color startColor, Color targetColor, Color endColor)
    {
        SetDisplayColor(startColor);

        float elapsedTime = 0f;
        while (elapsedTime < displayApplyTime)
        {
            SetDisplayColor(Color.Lerp(startColor, targetColor, elapsedTime / displayApplyTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        SetDisplayColor(targetColor);

        elapsedTime = 0f;
        while (elapsedTime < displayRevertTime)
        {
            SetDisplayColor(Color.Lerp(targetColor, startColor, elapsedTime / displayRevertTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        SetDisplayColor(endColor);
    }
    
    public IEnumerator ShakeString()
    {
        int stringIndex = practiceManager.targetNote.GetStringIndex();
        GameObject stringObject = strings[stringIndex];
        Vector3 startPostion = stringObject.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < 0.5f)
        {
            elapsedTime += Time.unscaledDeltaTime;

            Vector3 newPos = new Vector3(
                stringObject.transform.position.x,
                stringObject.transform.position.y + Mathf.Sin(elapsedTime * 130f),
                stringObject.transform.position.z);

            stringObject.transform.position = newPos;
            yield return null;
        }
        stringObject.transform.position = startPostion;
    }

    // Timing calls and animations
    void LoadTimingIndicator(string name, float timeUntilPlay)
    {
        timingIndicator.GetComponentInChildren<TMP_Text>().text = name;
        StartCoroutine(AnimateTimingIndicator(timeUntilPlay));
    }

    IEnumerator AnimateTimingIndicator(float timeUntilPlay)
    {
        float elapsedTime = 0f;
        float indicatorTime = timeUntilPlay - timingIndicatorAheadOffset;
        while (elapsedTime < indicatorTime)
        {
            timingIndicator.anchoredPosition = Vector2.Lerp(timingIndicatorStart, timingIndicatorEnd, elapsedTime / indicatorTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        timingIndicatorAnimator.SetTrigger("Hit");
        StartCoroutine(ShakeString());
    }

    void HideTimingIndicator(){
        timingIndicator.gameObject.SetActive(false);
    }

    void DisplayTimingIndicator()
    {
        timingIndicator.gameObject.SetActive(true);
    }

    public void Return()
    {
        Audio_Input_Manager.Instance.DeactivateMicrophoneInput();
        LoadSceneManager.Instance.LoadMainMenu();
    }

    public void Unpause(){
        if (pauseCoroutine == null)
            pauseCoroutine = StartCoroutine(UnpauseCountdownAnimation());
    }

    public void Pause(){
        if (isPaused)
        {
            StopCoroutine(pauseCoroutine);
            pauseCoroutine = null;

            pauseMenuCountdown.gameObject.SetActive(false);
            pauseMenuUnpauseButton.gameObject.SetActive(true);
            pauseMenu.gameObject.SetActive(true);
            
            pauseMenu.localScale = Vector3.one;
        }
        else
        {
            StartCoroutine(OpenPauseMenuAnimation());
        }
    }

    IEnumerator OpenPauseMenuAnimation(){
        isPaused = true;
        practiceManager.Pause();
        pauseMenuCountdown.gameObject.SetActive(false);
        pauseMenuUnpauseButton.gameObject.SetActive(true);

        float elapsedTime = 0f;
        pauseMenu.localScale = Vector3.zero;
        pauseMenu.gameObject.SetActive(true);
        while(elapsedTime < pauseMenuOpenTime){
            elapsedTime += Time.unscaledDeltaTime;
            pauseMenu.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, elapsedTime / pauseMenuOpenTime);
            yield return null;
        }
        pauseMenu.localScale = Vector3.one;
    }

    IEnumerator ClosePauseMenuAnimation(){
    float elapsedTime = 0f;
        while(elapsedTime < pauseMenuCloseTime){
            elapsedTime += Time.unscaledDeltaTime;
            pauseMenu.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, elapsedTime / pauseMenuCloseTime);
            yield return null;
        }
        pauseMenu.localScale = Vector3.zero;
        pauseMenu.gameObject.SetActive(false);
        practiceManager.Unpause();
        isPaused = false;
    }

    IEnumerator UnpauseCountdownAnimation(){
        pauseMenuCountdown.gameObject.SetActive(true);
        pauseMenuUnpauseButton.gameObject.SetActive(false);
        
        float timeRemaining = pauseMenuUnpauseDelay;
        while(timeRemaining > 0){
            timeRemaining -= Time.unscaledDeltaTime;
            pauseMenuCountdown.text = Mathf.CeilToInt(timeRemaining).ToString();
            yield return null;
        }
        StartCoroutine(ClosePauseMenuAnimation());
    }
    
    // Retry the lesson
    void RetryLesson()
    {
        // Reload the current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}