using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UI_BrushPicker : MonoBehaviour
{
    [SerializeField] List<BaseBrush> Brushes = new();
    [SerializeField] GameObject BrushUIPrefab;
    [SerializeField] Transform BrushUIRoot;
    [SerializeField] UnityEvent<BaseBrush> OnBrushChanged = new();

    List<UI_BrushElement> BrushUIElements = new();

    // Start is called before the first frame update
    void Start()
    {
        foreach(var Brush in Brushes)
        {
            var NewBrushUIGO = GameObject.Instantiate(BrushUIPrefab, BrushUIRoot);
            var NewBrushUILogic = NewBrushUIGO.GetComponent<UI_BrushElement>();

            BrushUIElements.Add(NewBrushUILogic);

            NewBrushUILogic.BindToBrush(Brush);
            NewBrushUILogic.OnBrushSelected.AddListener(OnBrushSelectedInternal);
        }

        if (Brushes.Count > 0)
            OnBrushSelectedInternal(Brushes[0]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnBrushSelectedInternal(BaseBrush InBrush)
    {
        foreach(var BrushUI in BrushUIElements)
        {
            BrushUI.SetIsSelected(InBrush);
        }

        OnBrushChanged.Invoke(InBrush);
    }
}
