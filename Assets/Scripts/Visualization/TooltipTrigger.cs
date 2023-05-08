using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string content;
    public string header;

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        TooltipSystem.Show(content, header);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        TooltipSystem.Hide();
    }
}
