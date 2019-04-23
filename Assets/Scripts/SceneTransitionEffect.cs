using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public abstract class SceneTransitionEffect
{
    protected GameObject transitionScreen;

    public SceneTransitionEffect()
    {
        transitionScreen = Object.Instantiate(Resources.Load<GameObject>("Prefabs/TransitionScreen"));
        Object.DontDestroyOnLoad(transitionScreen);
        transitionScreen.SetActive(false);
    }
    public abstract void LoadSceneWithEffect(string sceneName, LoadSceneMode mode);
}


public class Fade: SceneTransitionEffect{

    private float fadeRate;
    private float presentationDelay;
    private Color currFadeColor;

    private Image transitionImage;
    private bool isFading;

    public Fade(Color transitionColor, float fadeRate, float presentationDelay) : base()
    {
        currFadeColor = transitionColor;
        currFadeColor.a = 0.0f;
        this.fadeRate = fadeRate;
        this.presentationDelay = presentationDelay;

        transitionImage = transitionScreen.GetComponentInChildren<Image>();
    }
    public Fade(Sprite transitionSprite, float fadeRate, float presentationDelay) : this(Color.white, fadeRate, presentationDelay)
    {
        this.transitionImage.sprite = transitionSprite;
    }

    public override void LoadSceneWithEffect(string sceneName, LoadSceneMode mode)
    {
        transitionScreen.gameObject.SetActive(true);
        if (!isFading)
        {
            transitionImage.StartCoroutine(ApplyFade(sceneName, mode));
        }
        else //do not fade again just load scene
        {
            SceneManager.LoadScene(sceneName, mode);
        }
    }
   
    public void DisableFade()
    {
        transitionScreen.gameObject.SetActive(false);
    }

    //animation fixed update time of 24 fps
    IEnumerator ApplyFade(string sceneName, LoadSceneMode mode)
    {
       
        isFading = true;
        currFadeColor.a = 0.0f;
        transitionImage.color = currFadeColor;
        yield return ApplyFadeIn();
        yield return new WaitForSeconds(presentationDelay / 2.0f);
        currFadeColor.a = 1.0f;
        transitionImage.color = currFadeColor;
        SceneManager.LoadScene(sceneName, mode);
        yield return new WaitForSeconds(presentationDelay / 2.0f);
        yield return ApplyFadeOut();
        currFadeColor.a = 0.0f;
        transitionImage.color = currFadeColor;
        isFading = false;
    }
    IEnumerator ApplyFadeIn() {
        while (!(currFadeColor.a > 1.0f))
        {
            Debug.Log(currFadeColor.a);
            currFadeColor.a += fadeRate;
            transitionImage = transitionScreen.GetComponentInChildren<Image>();
            transitionImage.color = currFadeColor;
            yield return new WaitForSeconds(0.0417f);
        }
        yield return null;
    }
    IEnumerator ApplyFadeOut()
    {
        while (!(currFadeColor.a < 0.0f))
        {
            currFadeColor.a -= fadeRate;
            transitionImage.color = currFadeColor;
            yield return new WaitForSeconds(0.0417f);
        }
    }

}

public class AnimatedEffect : SceneTransitionEffect {

    public AnimatedEffect(Animator animator) : base()
    {
        transitionScreen.gameObject.AddComponent<Animator>().runtimeAnimatorController = animator.runtimeAnimatorController;
        //this.gameObject.SetActive(false);
    }

    public override void LoadSceneWithEffect(string sceneName, LoadSceneMode mode)
    {
        transitionScreen.gameObject.SetActive(true);
        transitionScreen.GetComponent<Image>().StartCoroutine(ApplyFade(sceneName, mode));
    }

    public void DisableFade()
    {
        transitionScreen.gameObject.SetActive(false);
    }

    //animation fixed update time of 24 fps
    IEnumerator ApplyFade(string sceneName, LoadSceneMode mode)
    {
        yield return PlayAnimationFirstHalf();
        SceneManager.LoadScene(sceneName, mode);
        yield return PlayAnimationSecondHalf();
    }
    IEnumerator PlayAnimationFirstHalf()
    {
        transitionScreen.gameObject.GetComponent<Animator>().Play(0);
        return null;
    }
    IEnumerator PlayAnimationSecondHalf()
    {
        transitionScreen.gameObject.GetComponent<Animator>().Play(0);
        return null;
    }


}