using System;
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
    private Text sliderUITextLabel;
    public Text sliderMaxValueText;

    public DynamicSlider(GameObject dynamicSliderUI)
    {
        this.dynamicSliderUI = dynamicSliderUI;
        valueUpdateUIup = this.dynamicSliderUI.transform.Find("valueUpdateUI/up").gameObject;
        valueUpdateUIdown = this.dynamicSliderUI.transform.Find("valueUpdateUI/down").gameObject;
        sliderUI = this.dynamicSliderUI.transform.GetComponent<Slider>();
        sliderUITextLabel = null;
        sliderMaxValueText = null;
        valueUpdateUIup.SetActive(false);
        valueUpdateUIdown.SetActive(false);
    }

    public DynamicSlider(GameObject dynamicSliderUI, bool sliderWithLabel)
    {
        this.dynamicSliderUI = dynamicSliderUI;
        valueUpdateUIup = this.dynamicSliderUI.transform.Find("valueUpdateUI/up").gameObject;
        valueUpdateUIdown = this.dynamicSliderUI.transform.Find("valueUpdateUI/down").gameObject;
        sliderUI = this.dynamicSliderUI.transform.GetComponent<Slider>();
        if (sliderWithLabel)
        {
            sliderUITextLabel = sliderUI.transform.Find("HandleSlideArea/SliderPick/Text").GetComponent<Text>();
        }
        else
        {
            sliderUITextLabel = null;
        }
        valueUpdateUIup.SetActive(false);
        valueUpdateUIdown.SetActive(false);
    }

    public DynamicSlider(GameObject dynamicSliderUI, bool sliderWithLabel, bool sliderWithMaxLabel)
    {
        this.dynamicSliderUI = dynamicSliderUI;
        valueUpdateUIup = this.dynamicSliderUI.transform.Find("valueUpdateUI/up").gameObject;
        valueUpdateUIdown = this.dynamicSliderUI.transform.Find("valueUpdateUI/down").gameObject;
        sliderUI = this.dynamicSliderUI.transform.GetComponent<Slider>();
        if (sliderWithLabel)
        {
            sliderUITextLabel = sliderUI.transform.Find("HandleSlideArea/SliderPick/Text").GetComponent<Text>();
        }
        else
        {
            sliderUITextLabel = null;
        }
        if (sliderWithMaxLabel)
        {
            sliderMaxValueText = sliderUI.transform.Find("HandleSlideArea/MaxPick/MaxValueText").GetComponent<Text>();
        }
        else
        {
            sliderMaxValueText = null;
        }
        valueUpdateUIup.SetActive(false);
        valueUpdateUIdown.SetActive(false);
    }

    public float GetSliderValue()
    {
        return sliderUI.value;
    }

    public void UpdateSliderMaxValue(float SliderValue)
    {
        if (SliderValue > (0.8*sliderUI.maxValue))
        {
            //sliderUI.maxValue = System.Convert.ToInt32(sliderUI.maxValue + (0.5 * sliderUI.maxValue));
            sliderUI.maxValue *= 2;
            sliderMaxValueText.text = sliderUI.maxValue.ToString();
        }
    }

    public IEnumerator UpdateSliderValue(float targetSliderValue,Boolean arrows)
    {
        UpdateSliderMaxValue(targetSliderValue);
        if (GameGlobals.isSimulation)
        {
            sliderUI.value = targetSliderValue;
            yield return null;
        }

        //make sure that the target value is within the slider range
        //targetSliderValue = Mathf.Clamp01(targetSliderValue);
        

        float initialSliderValue = sliderUI.value;
        float currSliderValue = initialSliderValue;
        float growth = targetSliderValue - currSliderValue;
        //float t = 0;
        string textToDisplay;



        textToDisplay = string.Format("{0:+;-;+}{0,2:#;#;0}", growth);
        GameGlobals.impactOnCP = textToDisplay;
        if (arrows)
        {
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
        }
        //update progressivo
        /*
        while (Mathf.Abs(targetSliderValue - currSliderValue) > 0.02f)
        {
            currSliderValue = initialSliderValue + Mathf.Sin(t) * growth;
            sliderUI.value = currSliderValue;
            t += 0.2f;
            yield return new WaitForSeconds(0.1416f);
        }*/
        sliderUI.value = targetSliderValue; //to avoid rounding errors due to animation
        if (sliderUITextLabel != null)
        {
            sliderUITextLabel.text = sliderUI.value.ToString();
        }

        yield return null;
    }
}
