using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePaintingCommand
{
    public enum EUndoBehaviour
    {
        FindLast,
        ReplayCommandList
    }

    public bool IsUndoable { get; private set; } = true;
    public EUndoBehaviour UndoBehaviour = EUndoBehaviour.FindLast;

    public BasePaintingCommand(bool InIsUndoable = true)
    {
        IsUndoable = InIsUndoable;
    }

    public virtual bool CanExtend()
    {
        return false;
    }

    public virtual void CloseCommand()
    {
    }
}
