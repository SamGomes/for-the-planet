using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicSlider
{
    private GameObject dynamicSliderUI;
    private GameObject valueUpdateUIup;
    private GameObject valueUpdateUIdown;
    private Slider sliderUI;

    public DynamicSlider(GameObject dynamicSliderUI)
    {
        this.dynamicSliderUI = dynamicSliderUI;
        valueUpdateUIup = this.dynamicSliderUI.transform.Find("valueUpdateUI/up").gameObject;
        valueUpdateUIdown = this.dynamicSliderUI.transform.Find("valueUpdateUI/down").gameObject;
        sliderUI = this.dynamicSliderUI.transform.GetComponent<Slider>();
        valueUpdateUIup.SetActive(false);
        valueUpdateUIdown.SetActive(false);
    }

    public float GetSliderValue()
    {
        return sliderUI.value;
    }

    public IEnumerator UpdateSliderValue(float targetSliderValue)
    {
        if (GameGlobals.isSimulation)
        {
            sliderUI.value = targetSliderValue;
            yield return null;
        }

        //make sure that the target value is within the slider range
        targetSliderValue = Mathf.Clamp01(targetSliderValue);

        float initialSliderValue = sliderUI.value;
        float currSliderValue = initialSliderValue;
        float growth = targetSliderValue - currSliderValue;
        float t = 0;

        string textToDisplay = string.Format("{0:+;-;+}{0,2:#;#;0}", growth*100.0f) + " %";
        if (growth > 0)
        {
            valueUpdateUIup.SetActive(false);
            valueUpdateUIup.SetActive(true);
            valueUpdateUIup.GetComponentInChildren<Text>().text = textToDisplay;
        }
        else if (growth < 0)
        {
            valueUpdateUIdown.SetActive(false);
            valueUpdateUIdown.SetActive(true);
            valueUpdateUIdown.GetComponentInChildren<Text>().text = textToDisplay;
        }

        while (Mathf.Abs(targetSliderValue - currSliderValue) > 0.02f)
        {
            currSliderValue = initialSliderValue + Mathf.Sin(t) * growth;
            sliderUI.value = currSliderValue;
            t += 0.2f;
            yield return new WaitForSeconds(0.0416f);
        }
        sliderUI.value = targetSliderValue; //to avoid rounding errors due to animation
        
        yield return null;
    }
}
