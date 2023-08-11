using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Brush", menuName = "Painting/Brush")]
public class BaseBrush : ScriptableObject
{
    public string DisplayName;
    public Texture2D BrushTexture;
    public bool bIsTintable = true;

    public Color Apply(Color InCurrentColour, Color InBrushColour, Color InTintColour, float InWeight)
    {
        Color DesiredColor = bIsTintable ? InTintColour : InBrushColour;

        float Intensity = InWeight * (bIsTintable ? InBrushColour.r : 1f);

        return Color.Lerp(InCurrentColour, DesiredColor, Intensity);
    }
}
