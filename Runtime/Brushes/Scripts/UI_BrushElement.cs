using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

public class UI_BrushElement : MonoBehaviour
{
    [SerializeField] RawImage BrushImage;
    [SerializeField] TextMeshProUGUI BrushName;
    [SerializeField] Image BackgroundImage;
    [SerializeField] Color DefaultColour;
    [SerializeField] Color SelectedColour;

    public UnityEvent<BaseBrush> OnBrushSelected = new();

    BaseBrush LinkedBrush;

    public void BindToBrush(BaseBrush InBrush)
    {
        LinkedBrush = InBrush;
        BrushImage.texture = InBrush.BrushTexture;
        BrushName.text = InBrush.DisplayName;
    }

    public void SetIsSelected(BaseBrush InBrush)
    {
        BackgroundImage.color = InBrush == LinkedBrush ? SelectedColour : DefaultColour;
    }

    public void OnBrushElementClicked(BaseEventData InEventData)
    {
        if (InEventData is PointerEventData)
        {
            OnBrushSelected.Invoke(LinkedBrush);
        }
    }
}
