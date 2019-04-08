using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicSlider
{
    private GameObject dynamicSliderUI;
    private GameObject valueUpdateUI;
    private Slider sliderUI;

    public DynamicSlider(GameObject dynamicSliderUI)
    {
        this.dynamicSliderUI = dynamicSliderUI;
        valueUpdateUI = this.dynamicSliderUI.transform.Find("valueUpdateUI").gameObject;
        sliderUI = this.dynamicSliderUI.transform.GetComponent<Slider>();
    }

    public IEnumerator UpdateSliderValue(float targetSliderValue)
    {
        //make sure that the target value is within the slider range
        targetSliderValue = Mathf.Clamp01(targetSliderValue);

        float initialSliderValue = sliderUI.value;
        float currSliderValue = initialSliderValue;
        float growth = targetSliderValue - currSliderValue;
        float t = 0;
        while (Mathf.Abs(targetSliderValue - currSliderValue) > 0.02f)
        {
            currSliderValue = initialSliderValue + Mathf.Sin(t) * growth;
            sliderUI.value = currSliderValue;
            t += 0.2f;
            yield return new WaitForSeconds(0.0416f);
        }
        yield return null;
    }
}
