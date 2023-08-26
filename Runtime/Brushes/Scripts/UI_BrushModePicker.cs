using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UI_BrushModePicker : MonoBehaviour
{
    [SerializeField] UnityEvent<BaseBrush.EMode> OnBrushModeSelected = new();
    [SerializeField] List<UI_BrushModeElement> BrushModeUIElements;

    private void Start()
    {
        SetBrushMode(BaseBrush.EMode.Normal, true);
    }

    public void SetBrushMode(BaseBrush.EMode InMode, bool InAllowNotifications = true)
    {
        foreach(var BrushModeUI in BrushModeUIElements)
        {
            BrushModeUI.SetIsSelected(BrushModeUI.Mode == InMode);
        }

        if (InAllowNotifications)
            OnModeButtonClicked(InMode);
    }

    public void OnModeButtonClicked(BaseBrush.EMode InMode)
    {
        OnBrushModeSelected.Invoke(InMode);

        foreach (var BrushModeUI in BrushModeUIElements)
        {
            BrushModeUI.SetIsSelected(BrushModeUI.Mode == InMode);
        }
    }
}
