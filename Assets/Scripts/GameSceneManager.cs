using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSceneManager{

    private SceneTransitionEffect transitionEffect;

    public GameSceneManager()
    {
        if (GameGlobals.autoPlay)
        {
            this.transitionEffect = new Instant();
        }
        else
        {
            this.transitionEffect = new Fade(Color.black, 0.1f, 0.5f);
        }
        //this.transitionEffect = new Fade(Resources.Load<Sprite>("Textures/loadingScreen"), 0.1f, 0.5f);
    }

    public void LoadStartScene()
    {
        transitionEffect.LoadSceneWithEffect("startScene", LoadSceneMode.Single);
    }
    public void LoadPlayersSetupScene()
    {
        transitionEffect.LoadSceneWithEffect("playersSetupScene", LoadSceneMode.Single);
    }
    public void LoadMainScene()
    {
        transitionEffect.LoadSceneWithEffect("mainScene", LoadSceneMode.Single);
    }
    public void LoadEndScene()
    {
        transitionEffect.LoadSceneWithEffect("endScene", LoadSceneMode.Single);
    }
}
