using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Brush", menuName = "Painting/Brush")]
public class BaseBrush : ScriptableObject
{
    public string DisplayName;
    public Texture2D BrushTexture;
    public bool bIsTintable = true;

    public enum EMode
    {
        Normal,
        Multiply,
        Screen,
        Overlay
    }

    public int Width { get; private set; } = -1;
    public int Height { get; private set; } = -1;

    Color[] CachedBrushData;

    public void SetScale(int InScaledWidth, int InScaledHeight)
    {
        bool IsDirty = false;

        if (Width < 0 || Height < 0)
        {
            Width = BrushTexture.width;
            Height = BrushTexture.height;
            IsDirty = true;
        }

        if (Width != InScaledWidth || Height != InScaledHeight)
        {
            Width = InScaledWidth;
            Height = InScaledHeight;
            IsDirty = true;
        }

        if (IsDirty)
        {
            CachedBrushData = new Color[Width * Height];

            for(int Y = 0; Y < Height; Y++)
            {
                float UV_Y = (float)Y / (float)Height;

                for (int X = 0; X < Width; X++)
                {
                    float UV_X = (float)X / (float)Width;

                    CachedBrushData[X + Y * Width] = BrushTexture.GetPixelBilinear(UV_X, UV_Y);
                }
            }
        }
    }

    public Color GetPixel(int InX, int InY)
    {
        return CachedBrushData[InX + (InY * Width)];
    }

    public Color Apply(Color InCurrentColour, Color InBrushColour, Color InTintColour, float InWeight, EMode InMode)
    {
        Color DesiredColor = bIsTintable ? InTintColour : InBrushColour;

        if (InMode == EMode.Multiply)
        {
            DesiredColor.r = BlendOp_Multiply(InCurrentColour.r, DesiredColor.r);
            DesiredColor.g = BlendOp_Multiply(InCurrentColour.g, DesiredColor.g);
            DesiredColor.b = BlendOp_Multiply(InCurrentColour.b, DesiredColor.b);
        }
        else if (InMode == EMode.Screen)
        {
            DesiredColor.r = BlendOp_Screen(InCurrentColour.r, DesiredColor.r);
            DesiredColor.g = BlendOp_Screen(InCurrentColour.g, DesiredColor.g);
            DesiredColor.b = BlendOp_Screen(InCurrentColour.b, DesiredColor.b);
        }
        else if (InMode == EMode.Overlay)
        {
            DesiredColor.r = BlendOp_Overlay(InCurrentColour.r, DesiredColor.r);
            DesiredColor.g = BlendOp_Overlay(InCurrentColour.g, DesiredColor.g);
            DesiredColor.b = BlendOp_Overlay(InCurrentColour.b, DesiredColor.b);
        }

        float Intensity = InWeight * (bIsTintable ? InBrushColour.r : 1f);

        return Color.Lerp(InCurrentColour, DesiredColor, Intensity);
    }

    protected float BlendOp_Multiply(float InImageColour, float InBrushColour)
    {
        return InImageColour * InBrushColour;
    }

    protected float BlendOp_Screen(float InImageColour, float InBrushColour)
    {
        return 1 - ((1 - InImageColour) * (1 - InBrushColour));
    }

    protected float BlendOp_Overlay(float InImageColour, float InBrushColour)
    {
        if (InImageColour < 0.5f)
            return 2 * InImageColour * InBrushColour;
        else
            return 1 - (2 * (1 - InImageColour) * (1 - InBrushColour));
    }
}
