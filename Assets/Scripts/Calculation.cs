using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Calculation : MonoBehaviour
{ 
   
    //Values from the AML file
    //-Attributes of the PowerSupply
    float outPower;

    //-Attributes of the WaterSupply
    float inFlowRateW; //in m^3/h
    float outFlowRateW;
    float coolingOutW;
    float powerConsumptionW; //in kW
    float waterEfficiency;

    //-Attributes of the AEMSkid
    float waterConsumption; //in L/h
    float powerConsumptionE;
    float coolingAEM;
    float hydrogenOut; //production rate: in Nm^3

    //-Attributes of the Gassystem
    float inFlowRateG;
    float powerConsumptionG;

    //-Attributes of the Coolingsystem
    float powerConsumptionC;
    float outFlowRateC;
    float inFlowRateC;


    Dictionary<string, float> containerNums;
    private float ElecPower;
    [SerializeField] SwitchToGui legendTextNotStatic;
    static SwitchToGui legendText;

    private void Awake()
    {
        legendText = legendTextNotStatic;
    }

    //E=Electrolyseur W=water G=gas C=cooling

    //Berechnung der benoetigten Menge an Elektrolyseur Skids und Perif�rcontainern gegeben einer gewuenschten Electrolyseleistung/ Calculation of the needed number of elements given the wanted electrolyzer power
    private Dictionary<string, float> CalcNumFromElecPower(int wantedElecPower)
    {
        containerNums["numESkids"] = (int)Mathf.Ceil(wantedElecPower /powerConsumptionE);
        CalcNumFromElecSkids((int)containerNums["numESkids"]);
        return containerNums;
    }

    //Calculates the needed numbers of perifery containers, given a number of electrolysercontainer
    private void CalcNumFromElecSkids(int numE)
    {
        containerNums["numCool"] = (int)Mathf.Ceil((containerNums["numESkids"] * coolingAEM) / inFlowRateC);
        containerNums["neededWaterSupply"] = ((containerNums["numESkids"] * waterConsumption) + (containerNums["numESkids"] * coolingAEM)); //would have to be adjusted if the efficiency is not 100% and an attribut for that added in the AML
        Debug.Log($"Coolingwater: {(containerNums["numESkids"] * coolingAEM)}");
        Debug.Log($"neededWaterByElecAndCooling: {containerNums["neededWaterSupply"]}");
        containerNums["numWater"] = Mathf.Ceil((containerNums["neededWaterSupply"])/ outFlowRateW); //second one only for AEM skids: Mathf.Max(  , (int)Mathf.Ceil((coolingAEM * containerNums["numESkids"]) / coolingOutW))
        containerNums["numGas"] = Mathf.Ceil((containerNums["numESkids"] * hydrogenOut) / inFlowRateG);
        containerNums["neededPowerSupply"] = containerNums["numESkids"] * powerConsumptionE 
            + containerNums["numWater"] * powerConsumptionW 
            + containerNums["numGas"] * powerConsumptionG 
            + containerNums["numCool"] * powerConsumptionC;
        containerNums["numPower"] = (int)Mathf.Ceil(containerNums["neededPowerSupply"] / outPower);
        containerNums["neededWaterSupply"] = (containerNums["neededWaterSupply"] * (100f/waterEfficiency)) - (containerNums["numESkids"] * coolingAEM);
        Debug.Log($"neededWaterByInclEfficiency: {containerNums["neededWaterSupply"]}");
    }

    //Calculates the possible number of electrolyer containers, given a powersupply (& and calls perifery container calculation)
    private int CalcNumFromPower(int powerSupply)
    {
        containerNums["numESkids"] = (int)Mathf.Floor(powerSupply/((powerConsumptionG*hydrogenOut/inFlowRateG)
            +(powerConsumptionW* Mathf.Max((int)Mathf.Ceil(waterConsumption /outFlowRateW), (int)Mathf.Ceil(coolingAEM/coolingOutW)))
            +(powerConsumptionE)
            +(powerConsumptionC*Mathf.Max((int)Mathf.Ceil(coolingAEM / outFlowRateC), (int)Mathf.Ceil(coolingAEM / coolingOutW)))));
        CalcNumFromElecSkids((int)containerNums["numESkids"]);
        return (int)containerNums["numESkids"];
    }

    //Calculates the possible number of electrolyer containers, given a watersupply (& and calls perifery container calculation)
    private int CalcNumFromWater(int waterSupply)
    {
        // Debug.Log($"numWaterEnd: {containerNums["numWater"]}");
       // var usableWater = waterSupply * (waterEfficiency/100f);
        containerNums["numESkids"] = (int)Math.Floor(waterSupply/(((waterConsumption+coolingAEM)*(100/waterEfficiency))-coolingAEM)); //the Water needed by the cooling system is part of a watercycle and has only to be added in the beginning (or changed in regular periods)
        CalcNumFromElecSkids((int)containerNums["numESkids"]);
        return (int)containerNums["numESkids"];
    }

    //Calculates the possible number of electrolyer containers, given a powersupply and a watersupply seperately and chooses the minimum of the two(& and calls perifery container calculation)
    private Dictionary<string, float> CalcNumFromPowerWater(int powerSupply, int waterSupply)
    {
        int countSupplyableSkidsWithWater = CalcNumFromWater(waterSupply);
        int countSupplyableSkidsWithPower = CalcNumFromPower(powerSupply);

        containerNums["numESkids"] = Mathf.Min(countSupplyableSkidsWithWater, countSupplyableSkidsWithPower);

        //set limitation message in legend
        if (countSupplyableSkidsWithPower < countSupplyableSkidsWithWater) {
            legendText.SetLimitingDimension("Stromverfügbarkeit");
        }
        else {
            legendText.SetLimitingDimension("Wasserverfügbarkeit");
        }
        
        CalcNumFromElecSkids((int)containerNums["numESkids"]);
        return containerNums;
    }

    //selects the right calculation by checking which playerprefs are set
    public void ChooseCalc() {
        string keyElectricityAvailability = SaveInputs.PlayerPrefsKeys.electricityAvailability;
        string keyElectrolyzerPower = SaveInputs.PlayerPrefsKeys.electrolyzerPower;
        string keyWaterAvailability = SaveInputs.PlayerPrefsKeys.waterAvailability;

        if (PlayerPrefs.HasKey(keyElectricityAvailability) && PlayerPrefs.HasKey(keyWaterAvailability)) CalcNumFromPowerWater(PlayerPrefs.GetInt(keyElectricityAvailability) * 1000, PlayerPrefs.GetInt(keyWaterAvailability));
        else if (PlayerPrefs.HasKey(keyElectricityAvailability)) CalcNumFromPower(PlayerPrefs.GetInt(keyElectricityAvailability) * 1000);
        else if (PlayerPrefs.HasKey(keyWaterAvailability)) CalcNumFromWater(PlayerPrefs.GetInt(keyWaterAvailability));
        else if (PlayerPrefs.HasKey(keyElectrolyzerPower)) CalcNumFromElecPower(PlayerPrefs.GetInt(keyElectrolyzerPower) * 1000);
    }

    //controls the flow of the calculation process
    private void Calc()
    {
        containerNums = new Dictionary<string, float>() { { "numESkids", -1 }, { "numWater", -1 }, { "numGas", -1 }, { "numCool", -1 }, { "numPower", -1 }, { "neededWaterSupply", -1 }, { "neededPowerSupply", -1 } };
        ReadValues();
        ChooseCalc();

        Dictionary<string, float> numbers = containerNums;
        for (int i = 0; i < numbers.Count; i++)
        {
            var key = numbers.ElementAt(i).Key;
            var num = numbers.ElementAt(i).Value;
            Debug.Log($"Key: {key}, Value: {num}");
        }
        WriteToFile();
        SetLegendText(ElecPower, containerNums["neededWaterSupply"] / 1000, containerNums["neededPowerSupply"] / 1000);
        StartVisulization();
    }

    private void Start()
    {
        Calc();
    }

    //searches the needed attributes in the AML, reads and writes them in variables
    private void ReadValues()
    {
        var aml = AmlAdapter.GetInstance();
        if (!aml.HasValidAmlDocument()) return;

        //Values from the AML file
        //-Attributes of the PowerSupply
        var attribute = aml.GetAttribute("PowerSupply", "OutPower");
        if (attribute != null) outPower = float.Parse(attribute.Value, CultureInfo.InvariantCulture) * 1000; //MW in kw
        else AttrtibuteNotFound("PowerSupply", "OutPower");

        //-Attributes of the WaterSupply
        attribute = aml.GetAttribute("WaterSupply", "InFlowRate");
        Debug.Log($"{attribute}, {attribute.Value}");
        if (attribute != null) inFlowRateW = float.Parse(attribute.Value, CultureInfo.InvariantCulture) * 1000; //in m^3/h in L/h
        else AttrtibuteNotFound("WaterSupply", "InFlowRate");

        attribute = aml.GetAttribute("WaterSupply", "OutFlowRate");
        if (attribute != null) outFlowRateW = float.Parse(attribute.Value, CultureInfo.InvariantCulture) * 1000; //in m^3/h in L/h
        else AttrtibuteNotFound("WaterSupply", "OutFlowRate");

        attribute = aml.GetAttribute("WaterSupply", "Efficiency");
        if (attribute != null) { waterEfficiency = float.Parse(attribute.Value, CultureInfo.InvariantCulture); }//in %
        else
        {
            waterEfficiency = 100; //no Value
            AttrtibuteNotFound("WaterSupply", " WaterEfficiency");
        }
        Debug.Log($" waterEfficiency: { waterEfficiency}");

        attribute = aml.GetAttribute("WaterSupply", "PowerConsumption");
        if (attribute != null) powerConsumptionW = float.Parse(attribute.Value, CultureInfo.InvariantCulture); //in kW
        else AttrtibuteNotFound("WaterSupply", "PowerConsumption");

        //-Attributes of the Gassystem
        attribute = aml.GetAttribute("GasSystem", "InFlowRate");
        if (attribute != null) inFlowRateG = int.Parse(attribute.Value, CultureInfo.InvariantCulture);//da wir in der AML unter der Gasaufbereitung keine Attribute zu druck und temperatur gegeben haben, gehen wir von m^3/h=Nm^3/h aus. (Nm^3= Normkubikmeter)
        else AttrtibuteNotFound("GasSystem", "InFlowRate");

        attribute = aml.GetAttribute("GasSystem", "PowerConsumption");
        if (attribute != null) powerConsumptionG = float.Parse(attribute.Value, CultureInfo.InvariantCulture); //kW
        else AttrtibuteNotFound("GasSystem", "PowerConsumption");

        //-Attributes of the Coolingsystem
        attribute = aml.GetAttribute("CoolingSystem", "PowerConsumption");
        if (attribute != null && attribute.Value != null) powerConsumptionC = float.Parse(attribute.Value, CultureInfo.InvariantCulture);//kW
        else
        {
            AttrtibuteNotFound("CoolingSystem", "PowerConsumption");
            powerConsumptionC = 20;
        }

        attribute = aml.GetAttribute("CoolingSystem", "OutFlowRate");
        if (attribute != null) outFlowRateC = float.Parse(attribute.Value, CultureInfo.InvariantCulture) *1000; //in m^3/h in L/h
        else AttrtibuteNotFound("CoolingSystem", "OutFlowRate");
        Debug.Log($"outFlowRateC: {outFlowRateC}");

        attribute = aml.GetAttribute("CoolingSystem", "InFlowRate");
        if (attribute != null) inFlowRateC = float.Parse(attribute.Value, CultureInfo.InvariantCulture) * 1000; //in m^3/h in L/h
        else AttrtibuteNotFound("CoolingSystem", "OutFlowRate");

        //-Attributes of the AEMSkid
        attribute = aml.GetAttribute("EnapterSkid", "WaterConsumption");
        Debug.Log($"{attribute}, {attribute.Value}");
        if (attribute != null) waterConsumption = float.Parse(attribute.Value, CultureInfo.InvariantCulture); //in L/h
        else AttrtibuteNotFound("EnapterSkid", "WaterConsumption");

        attribute = aml.GetAttribute("EnapterSkid", "PowerConsumption");
        if (attribute != null) powerConsumptionE = float.Parse(attribute.Value, CultureInfo.InvariantCulture) * 1000; // MW in kw /
        else AttrtibuteNotFound("EnapterSkid", "PowerConsumption");
        Debug.Log($"{attribute}, {attribute.Value}");

        attribute = aml.GetAttribute("EnapterSkid", "CoolingWaterFlow"); //in L/h
        if (attribute != null) coolingAEM = float.Parse(attribute.Value, CultureInfo.InvariantCulture);
        else
        {
            AttrtibuteNotFound("EnapterSkid", "CoolingIn");
            coolingAEM = 2; //noValue
        }
        Debug.Log($"coolingInAEM: {coolingAEM}");

        attribute = aml.GetAttribute("EnapterSkid", "ProductionRate");
        if (attribute != null) hydrogenOut = float.Parse(attribute.Value, CultureInfo.InvariantCulture); //production rate: in Nm^3
        else AttrtibuteNotFound("EnapterSkid", "ProductionRate");
    }


    //hands the calculated quantities over to the AmlAdapter, for them to be written in the AML Instancehierachy
    public void WriteToFile()
    {
        Dictionary<AmlAdapter.AmlComponentNames, int> containerCount = new Dictionary<AmlAdapter.AmlComponentNames, int>();
        containerCount.Add(AmlAdapter.AmlComponentNames.EnapterSkid, (int)containerNums["numESkids"]);
        containerCount.Add(AmlAdapter.AmlComponentNames.HoellerSkid, 0);
        containerCount.Add(AmlAdapter.AmlComponentNames.PlugpowerSkid, 0);
        containerCount.Add(AmlAdapter.AmlComponentNames.WaterSupply, (int)containerNums["numWater"]);
        containerCount.Add(AmlAdapter.AmlComponentNames.GasSystem, (int)containerNums["numGas"]);
        containerCount.Add(AmlAdapter.AmlComponentNames.PowerSupply, (int)containerNums["numPower"]);
        containerCount.Add(AmlAdapter.AmlComponentNames.ProcessControlUnit, 1);
        containerCount.Add(AmlAdapter.AmlComponentNames.CoolingSystem, (int)containerNums["numCool"]);
        ElecPower = containerNums["numESkids"] * powerConsumptionE/ 1000;
        

        List<AmlAdapter.AmlAttribute> attributes = new List<AmlAdapter.AmlAttribute>();
        float requiredWaterSupply = containerNums["neededWaterSupply"] / 1000;
        float requiredPowerSupply = containerNums["neededPowerSupply"] / 1000;
        attributes.Add(new AmlAdapter.AmlAttribute("requiredWaterSupply", requiredWaterSupply.ToString("0.00"), "m^3/h", typeof(float)));
        attributes.Add(new AmlAdapter.AmlAttribute("requiredPowerSupply", requiredPowerSupply.ToString("0.00"), "MW", typeof(float)));

        AmlAdapter.GetInstance().WriteToAmlIH($"ElectrolyzerPlant{ElecPower}MW", containerCount, true, attributes);
    }

    //sets the Text in the Legend with the calculated needed supplys and possible electrolyser power
    private static void SetLegendText(float electrolyzerPower, float neededWaterSupply, float neededPowerSupply)
    {
      
        legendText.SetElectrolyzerPower(electrolyzerPower);
        legendText.SetWaterNeeded(neededWaterSupply);
        legendText.SetPowerNeeded(neededPowerSupply);
    }

    private void AttrtibuteNotFound(string obj, string attribute)
    {
        Debug.Log($"{attribute} could not be found as an Attribute of {obj} in the SystemUnitClassLib.");
    }

    //hands the calculated quantaties over to the visualization
    private void StartVisulization()
    {
        var data = new List<Tuple<Visualization.ComponentNames, int>>();
        data.Add(new Tuple<Visualization.ComponentNames, int>(Visualization.ComponentNames.EnapterSkid, (int)containerNums["numESkids"]));
        data.Add(new Tuple<Visualization.ComponentNames, int>(Visualization.ComponentNames.HoellerSkid, 0));
        data.Add(new Tuple<Visualization.ComponentNames, int>(Visualization.ComponentNames.PlugpowerSkid, 0));
        data.Add(new Tuple<Visualization.ComponentNames, int>(Visualization.ComponentNames.WaterSupply, (int)containerNums["numWater"]));
        data.Add(new Tuple<Visualization.ComponentNames, int>(Visualization.ComponentNames.GasSystem, (int)containerNums["numGas"]));
        data.Add(new Tuple<Visualization.ComponentNames, int>(Visualization.ComponentNames.PowerSupply, (int)containerNums["numPower"]));
        data.Add(new Tuple<Visualization.ComponentNames, int>(Visualization.ComponentNames.CoolingSystem, (int)containerNums["numCool"]));

        Visualization.Visualize(data);
    }
}
