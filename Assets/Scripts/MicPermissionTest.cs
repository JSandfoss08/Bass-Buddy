using UnityEngine;

public class MicPermissionTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Microphone permission: " + Application.HasUserAuthorization(UserAuthorization.Microphone));
        StartCoroutine(RequestMicAccess());
    }

    private System.Collections.IEnumerator RequestMicAccess()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);

        if (Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            Debug.Log("Microphone access granted!");
            var mic = Microphone.devices.Length > 0 ? Microphone.devices[0] : "none";
            Debug.Log("First mic: " + mic);
        }
        else
        {
            Debug.LogWarning("Microphone access denied!");
        }
    }
}