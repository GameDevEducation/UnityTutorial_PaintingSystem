using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UI_ColourPicker : MonoBehaviour
{
    [SerializeField] UI_ColourWheel ColourWheel;
    [SerializeField] Color DefaultColour = Color.white;
    [SerializeField] List<UI_ColourPickerComponent> ColourPickerComponents;
    [SerializeField] UnityEvent<Color> OnColourChanged = new();

    public Color CurrentColour { get; private set; } = Color.white;

    float CurrentHue;
    float CurrentSaturation;
    float CurrentValue;

    // Start is called before the first frame update
    void Start()
    {
        Color.RGBToHSV(DefaultColour, out CurrentHue, out CurrentSaturation, out CurrentValue);

        ColourUpdated();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetNewColour(Color InNewColour, bool InAllowNotifications = true)
    {
        CurrentColour = InNewColour;
        Color.RGBToHSV(CurrentColour, out CurrentHue, out CurrentSaturation, out CurrentValue);

        ColourUpdated(InAllowNotifications);
    }

    public void OnColourWheelClicked(float InNewHue, float InNewSaturation)
    {
        CurrentHue = InNewHue;
        CurrentSaturation = InNewSaturation;

        ColourUpdated();
    }

    public void OnColourComponentChanged(EUIColourComponent InComponent, float InNewValue)
    {
        if (InComponent == EUIColourComponent.Hue)
            CurrentHue = InNewValue;
        else if (InComponent == EUIColourComponent.Saturation)
            CurrentSaturation = InNewValue;
        else if (InComponent == EUIColourComponent.Value)
            CurrentValue = InNewValue;

        ColourUpdated();
    }

    void ColourUpdated(bool InAllowNotifications = true)
    {
        CurrentColour = Color.HSVToRGB(CurrentHue, CurrentSaturation, CurrentValue);

        foreach (var ComponentUI in ColourPickerComponents)
        {
            ComponentUI.SetCurrentColour(CurrentColour, CurrentHue, CurrentSaturation, CurrentValue);
        }

        ColourWheel.SetCurrentColour(CurrentColour, CurrentHue, CurrentSaturation, CurrentValue);

        if (InAllowNotifications)
            OnColourChanged.Invoke(CurrentColour);
    }
}
