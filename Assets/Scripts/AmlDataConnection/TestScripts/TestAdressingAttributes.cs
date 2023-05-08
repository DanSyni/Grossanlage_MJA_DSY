using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAdressingAttributes : MonoBehaviour
{
    // Example for addressing attributes with the AmlAdapter class
    void Start()
    {
        var amlTest = AmlAdapter.GetInstance();
        if (!amlTest.HasValidAmlDocument()) return;
        var attribute = amlTest.GetAttribute("PlugpowerSkid", "PowerConsumption");
        Debug.Log($"{attribute}, {attribute.Value}");

        Debug.Log(amlTest.GetExternalInterface("PlugpowerSkid", "HydrogenOut"));
        Debug.Log(amlTest.GetAttributeOfExternalInterface("PlugpowerSkid", "CADDocument", "refURI").Value);

        //PlugpowerSkid, HoellerSkid, EnapterSkid, GasSystem, ...
        var testGameObject = amlTest.GetColladaGameObject("GasSystem");
        /*
        GameObject g = Instantiate(testGameObject);
        g.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        */
        var xml = amlTest.GetXMLPos("WaterSupply");
        foreach (var element in xml) {
            //Debug.Log(element.Name);
        }
        Debug.Log($"Für WaterSupply wurden {xml.Count} Einträge in PositionXML gefunden");

        Debug.Log(amlTest.GetXMLPos("PowerSupply", "DPowerSupplyOut3"));
    }
}
