using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintingCommand_Draw : BasePaintingCommand
{
    List<Vector2> Points = new();
    bool IsAllowedToExtend = true;

    public PaintingCommand_Draw(Vector2 InUV) :
        base(true) // always undoable
    {
        Points.Add(InUV);

        UndoBehaviour = EUndoBehaviour.ReplayCommandList;
    }

    public void Extend(Vector2 InUV)
    {
        Points.Add(InUV);
    }

    public override bool CanExtend()
    {
        return IsAllowedToExtend;
    }

    public override void CloseCommand()
    {
        IsAllowedToExtend = false;
    }

    public void Execute(bool InIsExtension, System.Action<Vector2> InDrawingFn)
    {
        if (InIsExtension)
        {
            InDrawingFn(Points[^1]);
        }
        else
        {
            foreach(var Location in Points)
            {
                InDrawingFn(Location);
            }
        }        
    }
}
