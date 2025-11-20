using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Doggo : MonoBehaviour
{
    public Animator animator;
    public float emotionDuration = 2f;
    public Image image;
    public Sprite DoggoHappySprite;
    public Sprite DoggoSadSprite;

    // Start is called before the first frame update
    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        image = GetComponent<Image>();
        image.sprite = DoggoHappySprite;
    }

    public void MakeHappy()
    {
        animator.SetBool("Happy", true);
        animator.SetBool("SuperHappy", false);
        animator.SetBool("Sad", false);
        image.sprite = DoggoHappySprite;
    }

    public void MakeSuperHappy()
    {
        animator.SetBool("SuperHappy", true);
        animator.SetBool("Sad", false);
        animator.SetBool("Happy", false);
        image.sprite = DoggoHappySprite;
    }

    public void MakeSuperHappyDuration()
    {
        IEnumerator coroutine = MakeSuperHappyCoroutine(emotionDuration);
        StartCoroutine(coroutine);
    }

    public void MakeSad()
    {
        image.sprite = DoggoSadSprite;
        animator.SetBool("Sad", true);
        animator.SetBool("Happy", false);
        animator.SetBool("SuperHappy", false);
    }

    public IEnumerator MakeSuperHappyCoroutine(float duration)
    {
        animator.SetBool("SuperHappy", true);
        animator.SetBool("Sad", false);
        animator.SetBool("Happy", false);

        yield return new WaitForSeconds(duration);

        animator.SetBool("SuperHappy", false);
    }
}
