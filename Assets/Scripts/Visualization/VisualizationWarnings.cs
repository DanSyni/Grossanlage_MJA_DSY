using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VisualizationWarnings : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI warningText;
    [SerializeField] GameObject canvas;

    private void Awake()
    {
        if (warningText == null || canvas == null) return;
        canvas.gameObject.SetActive(false);
    }

    public void DisplayWarning(string text) 
    {
        if (warningText == null || canvas == null) return;

        canvas.gameObject.SetActive(true);       
        warningText.text = $"Warnung: {text}";
    }
}
