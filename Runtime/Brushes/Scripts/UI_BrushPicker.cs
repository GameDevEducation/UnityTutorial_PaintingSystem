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
            NewBrushUILogic.OnBrushSelected.AddListener(OnBrushSelectedWithUI);
        }

        if (Brushes.Count > 0)
            OnBrushSelectedInternal(Brushes[0]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetNewBrush(BaseBrush InBrush, bool InAllowNotifications = true)
    {
        OnBrushSelectedInternal(InBrush, InAllowNotifications);
    }

    void OnBrushSelectedWithUI(BaseBrush InBrush)
    {
        OnBrushSelectedInternal(InBrush, true);
    }

    void OnBrushSelectedInternal(BaseBrush InBrush, bool InAllowNotifications = true)
    {
        foreach(var BrushUI in BrushUIElements)
        {
            BrushUI.SetIsSelected(InBrush);
        }

        if (InAllowNotifications)
            OnBrushChanged.Invoke(InBrush);
    }
}
