using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestWriteToFile : MonoBehaviour
{
    public void WriteToFile() {
        Dictionary<AmlAdapter.AmlComponentNames, int> containerCount = new Dictionary<AmlAdapter.AmlComponentNames, int>();
        containerCount.Add(AmlAdapter.AmlComponentNames.EnapterSkid, 3);
        containerCount.Add(AmlAdapter.AmlComponentNames.HoellerSkid, 5);
        containerCount.Add(AmlAdapter.AmlComponentNames.PlugpowerSkid, 1);
        containerCount.Add(AmlAdapter.AmlComponentNames.WaterSupply, 2); 
        containerCount.Add(AmlAdapter.AmlComponentNames.GasSystem, 4);
        containerCount.Add(AmlAdapter.AmlComponentNames.PowerSupply, 3);
        containerCount.Add(AmlAdapter.AmlComponentNames.ProcessControlUnit, 1);
        containerCount.Add(AmlAdapter.AmlComponentNames.CoolingSystem, 2);
        //AmlAdapter.GetInstance().WriteToAmlIH("HierKönnteIhrAnlagenNameStehen", containerCount, true); can be used if no attributes should be added

        List<AmlAdapter.AmlAttribute> attributes = new List<AmlAdapter.AmlAttribute>();
        attributes.Add(new AmlAdapter.AmlAttribute("requiredArea", $"{245}", "m^2", typeof(int))); // maybe required width / height is better?
        attributes.Add(new AmlAdapter.AmlAttribute("requiredWaterSupply", $"{100.3f}", "m³/h", typeof(float)));
        AmlAdapter.GetInstance().WriteToAmlIH("HierKönnteIhrAnlagenNameStehen", containerCount, true, attributes);
    }
}
