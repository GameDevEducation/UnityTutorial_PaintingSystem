using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintingCommand_SetBrush : BasePaintingCommand
{
    BaseBrush BrushToSet;

    public PaintingCommand_SetBrush(BaseBrush InBrush, bool InIsUndoable = true) :
        base(InIsUndoable)
    {
        BrushToSet = InBrush;
    }

    public void Execute(ref BaseBrush OutBrush)
    {
        OutBrush = BrushToSet;
    }
}
