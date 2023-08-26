using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintingCommand_SetColour : BasePaintingCommand
{
    Color ColourToSet;

    public PaintingCommand_SetColour(Color InColour, bool InIsUndoable = true) :
        base(InIsUndoable)
    {
        ColourToSet = InColour;
    }

    public void Execute(ref Color? OutColour)
    {
        OutColour = ColourToSet;
    }
}
