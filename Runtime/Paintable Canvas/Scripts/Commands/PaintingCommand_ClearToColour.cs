using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintingCommand_ClearToColour : BasePaintingCommand
{
    Color ColourToClearTo;

    public PaintingCommand_ClearToColour(Color InColour, bool InIsUndoable = true) :
        base(InIsUndoable)
    {
        ColourToClearTo = InColour;
    }

    public void Execute(Texture2DWrapper InTexture, int InWidth, int InHeight)
    {
        for (int Y = 0; Y < InHeight; Y++)
        {
            for (int X = 0; X < InWidth; X++)
            {
                InTexture.SetPixel(X, Y, ColourToClearTo);
            }
        }
    }
}
