using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UI_BrushModeElement : MonoBehaviour
{
    [SerializeField] BaseBrush.EMode LinkedMode;
    [SerializeField] UnityEvent<BaseBrush.EMode> OnBrushModeSelected = new();

    [SerializeField] Color SelectedColour = Color.white;
    [SerializeField] Color NotSelectedColour = Color.white;

    public BaseBrush.EMode Mode => LinkedMode;

    Button LinkedButton;
    Image LinkedButtonImage;

    private void Awake()
    {
        LinkedButton = GetComponent<Button>();
        LinkedButtonImage = GetComponent<Image>();
    }

    public void SetIsSelected(bool InIsSelected)
    {
        LinkedButtonImage.color = InIsSelected ? SelectedColour : NotSelectedColour;
    }

    public void OnClicked()
    {
        OnBrushModeSelected.Invoke(LinkedMode);
    }
}
