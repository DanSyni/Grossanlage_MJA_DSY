using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Aml.Engine.CAEX;

public class TestChangeFile : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] TextMeshProUGUI textWindow;
    [SerializeField] TextMeshProUGUI textForce;

    public void Start()
    {
        if (!AmlAdapter.GetInstance().HasValidAmlDocument()) return;
        //text.text = $"Current File: {AmlAdapter.GetPathOfCurrentDocument()}";
        textWindow.text = $"Current File: {AmlAdapter.GetPathOfCurrentDocument()}";
        textForce.text = $"Current File: {AmlAdapter.GetPathOfCurrentDocument()}";
    }

    public void Test() {
        AmlAdapter adapter = AmlAdapter.GetInstance();
        AttributeType erg = adapter.GetAttribute("SecurityDepartment", "camCount");

        if (erg == null) {
            text.text = "SecurityDepartment or camCount could not be found.";
        }
        else{
            text.text = $"The SecurityDepartment has {erg.Value} Cameras.";
        }
    }
}
