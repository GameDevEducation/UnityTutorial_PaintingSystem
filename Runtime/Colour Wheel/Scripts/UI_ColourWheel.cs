using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UI_ColourWheel : MonoBehaviour
{
    [SerializeField] RawImage ColourWheelImage;
    [SerializeField] int WheelBufferInPixels = 5;
    [SerializeField] Image ReticleImage;
    [SerializeField] UnityEvent<float, float> OnNewColourPicked = new();

    RectTransform ReticleTransform;
    int WheelTextureSize;
    float MaxDistance;
    float MaxDistanceSq;

    // Start is called before the first frame update
    void Start()
    {
        ReticleTransform = ReticleImage.GetComponent<RectTransform>();

        BuildColourWheelTexture();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void BuildColourWheelTexture()
    {
        Canvas OwningCanvas = ColourWheelImage.GetComponentInParent<Canvas>();
        Rect ActualRectangle = RectTransformUtility.PixelAdjustRect(ColourWheelImage.rectTransform, OwningCanvas);

        WheelTextureSize = Mathf.FloorToInt(Mathf.Min(ActualRectangle.width, ActualRectangle.height));

        Texture2D WheelTexture = new Texture2D(WheelTextureSize, WheelTextureSize, TextureFormat.RGB24, false);

        MaxDistance = (WheelTextureSize / 2f) - WheelBufferInPixels;
        MaxDistanceSq = MaxDistance * MaxDistance;

        // build the texture
        for (int Y = 0; Y < WheelTextureSize; Y++) 
        { 
            for (int X = 0; X < WheelTextureSize; X++) 
            {
                Vector2 VectorFromCentre = new Vector2(X - (WheelTextureSize / 2f),
                                                       Y - (WheelTextureSize / 2f));
                float DistanceFromCentreSq = VectorFromCentre.sqrMagnitude;

                if (DistanceFromCentreSq < MaxDistanceSq)
                {
                    float Angle = Mathf.Atan2(VectorFromCentre.y, VectorFromCentre.x);
                    if (Angle < 0)
                        Angle += Mathf.PI * 2f;

                    float Hue = Mathf.Clamp01(Angle / (Mathf.PI * 2f));
                    float Saturation = Mathf.Clamp01(Mathf.Sqrt(DistanceFromCentreSq) / MaxDistance);

                    WheelTexture.SetPixel(X, Y, Color.HSVToRGB(Hue, Saturation, 1f));
                }
                else
                    WheelTexture.SetPixel(X, Y, Color.white);
            }
        }

        WheelTexture.Apply();
        ColourWheelImage.texture = WheelTexture;
    }

    public void OnColourClicked(BaseEventData EventData)
    {
        if (EventData is PointerEventData)
        {
            Vector2 ScreenPosition = (EventData as PointerEventData).pointerCurrentRaycast.screenPosition;
            Vector2 LocalPosition = ColourWheelImage.rectTransform.InverseTransformPoint(ScreenPosition);

            float DistanceFromCentreSq = LocalPosition.sqrMagnitude;

            if (DistanceFromCentreSq > MaxDistanceSq)
                return;

            float Angle = Mathf.Atan2(LocalPosition.y, LocalPosition.x);
            if (Angle < 0)
                Angle += Mathf.PI * 2f;

            float Hue = Mathf.Clamp01(Angle / (Mathf.PI * 2f));
            float Saturation = Mathf.Clamp01(Mathf.Sqrt(DistanceFromCentreSq) / MaxDistance);

            OnNewColourPicked.Invoke(Hue, Saturation);
            UpdateReticle(Hue, Saturation);
        }
    }

    public void SetCurrentColour(Color InColour, float InHue, float InSaturation, float InValue)
    {
        UpdateReticle(InHue, InSaturation);

        ReticleImage.color = InColour;
    }

    void UpdateReticle(float InHue, float InSaturation)
    {
        Vector2 ReticlePosition = new Vector2(Mathf.Cos(InHue * Mathf.PI * 2f),
                                              Mathf.Sin(InHue * Mathf.PI * 2f));
        ReticlePosition *= MaxDistance * InSaturation;

        ReticleTransform.anchoredPosition = ReticlePosition;
    }
}
