using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Texture2DWrapper
{
    Texture2D LinkedTexture;
    int LinkedTextureWidth;

    public bool IsDirty { get; private set; } = true;

    Color[] PixelData;

    public Texture2DWrapper(Texture2D InTexture)
    {
        LinkedTexture = InTexture;
        LinkedTextureWidth = LinkedTexture.width;

        PixelData = LinkedTexture.GetPixels();
    }

    public void SetPixel(int InX, int InY, Color InColour)
    {
        PixelData[InX + (InY * LinkedTextureWidth)] = InColour;

        IsDirty = true;
    }

    public Color GetPixel(int InX, int InY) 
    {
        return PixelData[InX + (InY * LinkedTextureWidth)];
    }

    public void Apply()
    {
        if (IsDirty)
        {
            IsDirty = false;

            LinkedTexture.SetPixels(PixelData);

            LinkedTexture.Apply();
        }
    }
}

public class PaintableCanvas : MonoBehaviour
{
    enum EPaintingMode
    {
        Off,
        Draw
    }

    [SerializeField] float RaycastDistance = 10.0f;
    [SerializeField] LayerMask PaintableCanvasLayerMask = ~0;
    [SerializeField] MeshFilter CanvasMeshFilter;
    [SerializeField] MeshRenderer CanvasMeshRenderer;
    [SerializeField] int PixelsPerMetre = 200;
    [SerializeField] Color CanvasDefaultColour = Color.white;
    [SerializeField] float BrushScale = 0.25f;
    [SerializeField] float BrushWeight = 0.25f;

    [SerializeField] UnityEvent<Color, bool> OnSyncUIWithCanvasColour = new();
    [SerializeField] UnityEvent<BaseBrush, bool> OnSyncUIWithBrush = new();
    [SerializeField] UnityEvent<BaseBrush.EMode, bool> OnSyncUIWithBrushMode = new();
    [SerializeField] UnityEvent<bool> OnSyncUndoAvailable = new();
    [SerializeField] UnityEvent<bool> OnSyncRedoAvailable = new();

    List<BasePaintingCommand> CommandStack = new();
    List<BasePaintingCommand> RedoList = new();

    BasePaintingCommand MostRecentCommand => CommandStack[^1];

    EPaintingMode PaintingMode_PrimaryMouse = EPaintingMode.Draw;

    int CanvasWidthInPixels;
    int CanvasHeightInPixels;

    Texture2DWrapper PaintableTexture;

    BaseBrush ActiveBrush;
    Color? ActiveColour;
    BaseBrush.EMode? ActiveBrushMode;

    // Start is called before the first frame update
    void Start()
    {
        CanvasWidthInPixels = Mathf.CeilToInt(CanvasMeshFilter.mesh.bounds.size.x * CanvasMeshFilter.transform.localScale.x * PixelsPerMetre);
        CanvasHeightInPixels = Mathf.CeilToInt(CanvasMeshFilter.mesh.bounds.size.y * CanvasMeshFilter.transform.localScale.y * PixelsPerMetre);

        Texture2D LocalTexture = new Texture2D(CanvasWidthInPixels, CanvasHeightInPixels, TextureFormat.ARGB32, false);
        CanvasMeshRenderer.material.mainTexture = LocalTexture;
        PaintableTexture = new Texture2DWrapper(LocalTexture);

        AppendCommand(new PaintingCommand_ClearToColour(CanvasDefaultColour, false));
    }

    // Update is called once per frame
    void Update()
    {
        if (ActiveBrush != null)
        {
            if (PaintingMode_PrimaryMouse == EPaintingMode.Draw)
            {
                if (Input.GetMouseButton(0))
                    Update_PerformDrawing(PaintingMode_PrimaryMouse);
                else if (MostRecentCommand.CanExtend())
                    MostRecentCommand.CloseCommand();
            }

        }

        if (PaintableTexture.IsDirty)
            PaintableTexture.Apply();
    }

    RaycastHit[] HitResults = new RaycastHit[1];
    void Update_PerformDrawing(EPaintingMode PaintingMode)
    {
        Ray DrawingRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.RaycastNonAlloc(DrawingRay, HitResults, RaycastDistance, PaintableCanvasLayerMask) > 0)
        {
            if (MostRecentCommand.CanExtend() && (MostRecentCommand is PaintingCommand_Draw))
            {
                var DrawCommand = MostRecentCommand as PaintingCommand_Draw;
                DrawCommand.Extend(HitResults[0].textureCoord);

                ExecuteCommandInternal_Draw(DrawCommand, true);
            }
            else
            {
                AppendCommand(new PaintingCommand_Draw(HitResults[0].textureCoord));
            }
        }
    }

    public void SelectBrush(BaseBrush InBrush)
    {
        AppendCommand(new PaintingCommand_SetBrush(InBrush, ActiveBrush != null));
    }

    public void SelectBrushMode(BaseBrush.EMode InBrushMode)
    {
        AppendCommand(new PaintingCommand_SetBrushMode(InBrushMode, ActiveBrushMode != null));
    }

    public void SetColour(Color InColour)
    {
        AppendCommand(new PaintingCommand_SetColour(InColour, ActiveColour != null));
    }

    public void UndoLastCommand()
    {
        if (!MostRecentCommand.IsUndoable)
            return;

        var RemovedCommand = MostRecentCommand;
        CommandStack.RemoveAt(CommandStack.Count - 1);
        RedoList.Add(RemovedCommand);

        OnSyncUndoAvailable.Invoke(MostRecentCommand.IsUndoable);
        OnSyncRedoAvailable.Invoke(true);

        if (RemovedCommand.UndoBehaviour == BasePaintingCommand.EUndoBehaviour.ReplayCommandList)
        {
            ReplayAllCommands();
        }
        else if (RemovedCommand.UndoBehaviour == BasePaintingCommand.EUndoBehaviour.FindLast)
        {
            for (int Index = CommandStack.Count - 1; Index >= 0; Index--) 
            { 
                var Command = CommandStack[Index];

                if (Command.GetType() == RemovedCommand.GetType())
                {
                    ExecuteCommand(Command, true);
                    return;
                }
            }
        }
    }

    public void RedoLastCommand()
    {
        if (RedoList.Count == 0)
            return;

        var CommandToRedo = RedoList[^1];
        RedoList.RemoveAt(RedoList.Count - 1);

        ExecuteCommand(CommandToRedo, true);
        CommandStack.Add(CommandToRedo);

        OnSyncUndoAvailable.Invoke(MostRecentCommand.IsUndoable);
        OnSyncRedoAvailable.Invoke(RedoList.Count > 0);
    }

    void AppendCommand(BasePaintingCommand InCommand)
    {
        RedoList.Clear();
        OnSyncRedoAvailable.Invoke(false);

        ExecuteCommand(InCommand, false);
        CommandStack.Add(InCommand);

        OnSyncUndoAvailable.Invoke(MostRecentCommand.IsUndoable);
    }

    void ReplayAllCommands()
    {
        foreach(var Command in CommandStack) 
        {
            ExecuteCommand(Command, true);
        }
    }

    void ExecuteCommand(BasePaintingCommand InCommand, bool IsUndoOrReplay)
    {
        if (InCommand is PaintingCommand_ClearToColour)
        {
            (InCommand as PaintingCommand_ClearToColour).Execute(PaintableTexture, CanvasWidthInPixels, CanvasHeightInPixels);
        }
        else if (InCommand is PaintingCommand_SetColour)
        {
            (InCommand as PaintingCommand_SetColour).Execute(ref ActiveColour);

            if (IsUndoOrReplay)
                OnSyncUIWithCanvasColour.Invoke(ActiveColour.Value, false);
        }
        else if (InCommand is PaintingCommand_SetBrush)
        {
            (InCommand as PaintingCommand_SetBrush).Execute(ref ActiveBrush);

            if (IsUndoOrReplay)
                OnSyncUIWithBrush.Invoke(ActiveBrush, false);

            int ScaledBrushWidth = Mathf.RoundToInt(ActiveBrush.BrushTexture.width * BrushScale);
            int ScaledBrushHeight = Mathf.RoundToInt(ActiveBrush.BrushTexture.height * BrushScale);

            ActiveBrush.SetScale(ScaledBrushWidth, ScaledBrushHeight);
        }
        else if (InCommand is PaintingCommand_Draw)
        {
            ExecuteCommandInternal_Draw((InCommand as PaintingCommand_Draw));
        }
        else if (InCommand is PaintingCommand_SetBrushMode)
        {
            (InCommand as PaintingCommand_SetBrushMode).Execute(ref ActiveBrushMode);

            if (IsUndoOrReplay)
                OnSyncUIWithBrushMode.Invoke(ActiveBrushMode.Value, false);
        }
    }

    void ExecuteCommandInternal_Draw(PaintingCommand_Draw InDrawCommand, bool InIsExtension = false)
    {
        InDrawCommand.Execute(InIsExtension, (Vector2 InLocation) =>
        {
            int DrawingOriginX = Mathf.RoundToInt(InLocation.x * CanvasWidthInPixels);
            int DrawingOriginY = Mathf.RoundToInt(InLocation.y * CanvasHeightInPixels);

            for (int BrushY = 0; BrushY < ActiveBrush.Height; BrushY++)
            {
                int PixelY = DrawingOriginY + BrushY - (ActiveBrush.Height / 2);
                if (PixelY < 0 || PixelY >= CanvasHeightInPixels)
                    continue;

                for (int BrushX = 0; BrushX < ActiveBrush.Width; BrushX++)
                {
                    int PixelX = DrawingOriginX + BrushX - (ActiveBrush.Width / 2);
                    if (PixelX < 0 || PixelX >= CanvasWidthInPixels)
                        continue;

                    Color BrushPixel = ActiveBrush.GetPixel(BrushX, BrushY);
                    Color CanvasPixel = PaintableTexture.GetPixel(PixelX, PixelY);

                    CanvasPixel = ActiveBrush.Apply(CanvasPixel, BrushPixel, ActiveColour.Value, BrushWeight * Time.deltaTime, ActiveBrushMode.Value);
                    PaintableTexture.SetPixel(PixelX, PixelY, CanvasPixel);
                }
            }
        });      
    }
}
