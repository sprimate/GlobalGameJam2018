using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasGroupFader : MonoBehaviour
{
    public enum State { Off, FadeIn, On, FadeOut }
    private State state = State.Off;

    CanvasGroup canvasGroup;
    float fadeDuration = 1;

    public void Init(bool on, float fadeDuration = 0.25f)
    {
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        SetCanvasGroup(on);
        canvasGroup.alpha = on ? 1 : 0;
        alphaStart = canvasGroup.alpha;
        this.fadeDuration = fadeDuration;
    }

    public void Crossfade(bool on)
    {
        state = on ? State.FadeIn : State.FadeOut;
    }

    void SetCanvasGroup(bool on)
    {
        canvasGroup.interactable = on;
        canvasGroup.blocksRaycasts = on;
        state = on ? State.On : State.Off;
    }

    State lastState;
    float timer;
    float tRatio;
    float alpha;
    float alphaStart;
    void Update()
    {
        if (state != lastState)
        {
            alphaStart = canvasGroup.alpha;
            timer = Time.unscaledTime;
            if (state == State.FadeIn)
            {
                SetCanvasGroup(true);
                state = State.FadeIn;
            }
        }
        tRatio = (Time.unscaledTime - timer) / fadeDuration;

        switch (state)
        {
            case State.Off:
            case State.On:
                break;
            case State.FadeIn:
                alpha = Mathf.Lerp(alphaStart, 1, tRatio);
                if (tRatio >= 1)
                {
                    state = State.On;
                    alpha = 1;
                }

                canvasGroup.alpha = alpha;
                break;

            case State.FadeOut:
                alpha = Mathf.Lerp(alphaStart, 0, tRatio);
                if (tRatio >= 1)
                {
                    state = State.Off;
                    alpha = 0;
                    SetCanvasGroup(false);
                }

                canvasGroup.alpha = alpha;
                break;
            default:
                break;
        }

        lastState = state;
    }

}