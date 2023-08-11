using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    EPaintingMode PaintingMode_PrimaryMouse = EPaintingMode.Draw;

    int CanvasWidthInPixels;
    int CanvasHeightInPixels;

    Texture2D PaintableTexture;

    BaseBrush ActiveBrush;
    Color ActiveColour = Color.magenta;

    // Start is called before the first frame update
    void Start()
    {
        CanvasWidthInPixels = Mathf.CeilToInt(CanvasMeshFilter.mesh.bounds.size.x * CanvasMeshFilter.transform.localScale.x * PixelsPerMetre);
        CanvasHeightInPixels = Mathf.CeilToInt(CanvasMeshFilter.mesh.bounds.size.y * CanvasMeshFilter.transform.localScale.y * PixelsPerMetre);

        PaintableTexture = new Texture2D(CanvasWidthInPixels, CanvasHeightInPixels, TextureFormat.ARGB32, false);
        for (int Y = 0; Y < CanvasHeightInPixels; Y++)
        {
            for (int X = 0; X < CanvasWidthInPixels; X++) 
            {
                PaintableTexture.SetPixel(X, Y, CanvasDefaultColour);
            }
        }
        PaintableTexture.Apply();

        CanvasMeshRenderer.material.mainTexture = PaintableTexture;
    }

    // Update is called once per frame
    void Update()
    {
        if (ActiveBrush != null)
        {
            if (PaintingMode_PrimaryMouse == EPaintingMode.Draw && Input.GetMouseButton(0))
            {
                Update_PerformDrawing(PaintingMode_PrimaryMouse);
            }
        }
    }

    RaycastHit[] HitResults = new RaycastHit[1];
    void Update_PerformDrawing(EPaintingMode PaintingMode)
    {
        Ray DrawingRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.RaycastNonAlloc(DrawingRay, HitResults, RaycastDistance, PaintableCanvasLayerMask) > 0)
        {
            PerformDrawingWith(ActiveBrush, ActiveColour, HitResults[0].textureCoord);
        }
    }

    void PerformDrawingWith(BaseBrush ActiveBrush, Color ActiveColour, Vector2 LocationUV)
    {
        int DrawingOriginX = Mathf.RoundToInt(LocationUV.x * CanvasWidthInPixels);
        int DrawingOriginY = Mathf.RoundToInt(LocationUV.y * CanvasHeightInPixels);

        int ScaledBrushWidth  = Mathf.RoundToInt(ActiveBrush.BrushTexture.width * BrushScale);
        int ScaledBrushHeight = Mathf.RoundToInt(ActiveBrush.BrushTexture.height * BrushScale);

        for (int BrushY = 0; BrushY < ScaledBrushHeight; BrushY++) 
        {
            int PixelY = DrawingOriginY + BrushY - (ScaledBrushHeight / 2);
            if (PixelY < 0 || PixelY >= CanvasHeightInPixels)
                continue;

            float BrushUV_Y = (float)BrushY / (float)ScaledBrushHeight;

            for (int BrushX = 0; BrushX < ScaledBrushWidth; BrushX++)
            {
                int PixelX = DrawingOriginX + BrushX - (ScaledBrushWidth / 2);
                if (PixelX < 0 || PixelX >= CanvasWidthInPixels)
                    continue;

                // calculate the brush UV to lookup
                float BrushUV_X = (float) BrushX / (float) ScaledBrushWidth;

                Color BrushPixel = ActiveBrush.BrushTexture.GetPixelBilinear(BrushUV_X, BrushUV_Y);
                Color CanvasPixel = PaintableTexture.GetPixel(PixelX, PixelY);

                CanvasPixel = ActiveBrush.Apply(CanvasPixel, BrushPixel, ActiveColour, BrushWeight * Time.deltaTime);
                PaintableTexture.SetPixel(PixelX, PixelY, CanvasPixel);
            }
        }

        PaintableTexture.Apply();
    }

    public void SelectBrush(BaseBrush InBrush)
    {
        ActiveBrush = InBrush;
    }

    public void SetColour(Color InColour)
    {
        ActiveColour = InColour;
    }
}
