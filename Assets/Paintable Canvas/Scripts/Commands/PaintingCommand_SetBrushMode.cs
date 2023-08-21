using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintingCommand_SetBrushMode : BasePaintingCommand
{
    BaseBrush.EMode ModeToSet;

    public PaintingCommand_SetBrushMode(BaseBrush.EMode InModeToSet, bool InIsUndoable = true) :
        base(InIsUndoable)
    {
        ModeToSet = InModeToSet;
    }

    public void Execute(ref BaseBrush.EMode? OutMode)
    {
        OutMode = ModeToSet;
    }
}
