using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public enum EUIColourComponent
{
    Hue,
    Saturation,
    Value
}

public class UI_ColourPickerComponent : MonoBehaviour
{
    [SerializeField] EUIColourComponent ComponentType;
    [SerializeField] TextMeshProUGUI ComponentLabel;
    [SerializeField] Slider ComponentSlider;
    [SerializeField] UnityEvent<EUIColourComponent, float> OnColourComponentChanged = new();

    Image SliderHandleImage;

    // Start is called before the first frame update
    void Start()
    {        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSliderChanged(float newValue)
    {
        OnColourComponentChanged.Invoke(ComponentType, newValue);
    }

    public void SetCurrentColour(Color InColour, float InHue, float InSaturation, float InValue)
    {
        float ValueToSet = ComponentSlider.value;

        if (ComponentType == EUIColourComponent.Hue) 
            ValueToSet = InHue;
        else if (ComponentType == EUIColourComponent.Saturation)
            ValueToSet = InSaturation;
        else if (ComponentType == EUIColourComponent.Value)
            ValueToSet = InValue;

        ComponentSlider.SetValueWithoutNotify(ValueToSet);

        if (SliderHandleImage == null)
            SliderHandleImage = ComponentSlider.handleRect.GetComponent<Image>();

        SliderHandleImage.color = InColour;
    }
}
