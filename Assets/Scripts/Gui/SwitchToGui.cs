using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class SwitchToGui : MonoBehaviour
{
    [SerializeField] GameObject toolTips;
    [SerializeField] GameObject legende;
    [SerializeField] GameObject warning;

    [SerializeField] TextMeshProUGUI legendText;
    




    bool tipsEnabled = false;
    bool legendeEnabled = false;

    private double _length;
    private double _width;
    private int _numberContainers;
    private int _numberELContainers;
    private int _numberPSContainers;
    private int _numberWTContainers;
    private int _numberGPContainers;
    private int _numberCLContainers;
    private int _numberInterfaces;
    private int _stackingNumber;
    private int _numberHelpstructures;

    private float _electricityNeeded;
    private float _elektrolyseurPower;
    private float _hydrogenOutput;
    private float _waterNeeded;

    private string _limitingDimension;


    // Start is called before the first frame update
    void Start()
    {
        toolTips.SetActive(tipsEnabled);
        legende.SetActive(legendeEnabled);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            SceneManager.LoadScene("GUI");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
          
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            legendeEnabled = !legendeEnabled;
            legende.SetActive(legendeEnabled);
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            tipsEnabled = !tipsEnabled;
            toolTips.SetActive(tipsEnabled);
        }


    }

    //Use this Methods to set the Variables for the key text
    public void SetPlantLength(double length)
    {
        _length = length;
        SetLegendText();
    }

    public void SetPlantWidth(double width)
    {
        _width = width;
        SetLegendText();
    }

    public void SetCountContainer(int containerCount, int containerELCount, int containerPSCount, int containerWTCount, int containerGPCount, int containerCLCount)
    {
        _numberContainers = containerCount;
        _numberELContainers = containerELCount;
        _numberPSContainers = containerPSCount;
        _numberWTContainers = containerWTCount;
        _numberGPContainers = containerGPCount;
        _numberCLContainers = containerCLCount;
        SetLegendText();
    }

    public void SetStackingHeight(int stackingHeight)
    {
        _stackingNumber = stackingHeight;
        SetLegendText();
    }

    public void SetCountHelpstructures(int helpstrucutreCount)
    {
        _numberHelpstructures = helpstrucutreCount;
        SetLegendText();
    }

    public void SetPowerNeeded(float powerNeeded)
    {
        _electricityNeeded = powerNeeded;
        SetLegendText();
    }

    public void SetElectrolyzerPower(float electrolyseurPower)
    {
        _elektrolyseurPower = electrolyseurPower;
        SetLegendText();
    }

    public void SetWaterNeeded(float waterNeeded)
    {
        _waterNeeded = waterNeeded;
        SetLegendText();
    }

    public void SetLimitingDimension(string limitingDimension) { 
        _limitingDimension = limitingDimension;
        SetLegendText();
    }

    //Set Key Text
    private void SetLegendText()
    {
        legendText.text = "";
        if (_length != 0 && _width != 0)
        {
            legendText.text += "<u>" + $"Größe der Anlage:" + "</u>" +
                $"\nLänge: {_length:#0.00} m \nBreite: {_width:#0.00} m \n \n";
        }
        if (_numberContainers != 0)
        {
            legendText.text += "<u>"+$"Strukturelle Angaben: " + "</u>" +
                $"\nGesamtanzahl der Container: {_numberContainers}"+
                $"\n◦ Anzahl an EL-Skids: {_numberELContainers} \n◦ Anzahl an PowerSupply-Skids: {_numberPSContainers}" +
                $"\n◦ Anzahl an Watertreatment-Skids: {_numberWTContainers} \n◦ Anzahl an Gaspurification-Skids: {_numberGPContainers} \n◦ Anzahl an Cooling-Skids: {_numberCLContainers}" +
                $"\n◦ Stapelhöhe der Container: {_stackingNumber} \n◦ Anzahl an Schnittstellen: {_numberInterfaces} \nAnzahl der Stützstrukturen: {_numberHelpstructures}\n\n";
                
        }
        if(_electricityNeeded != 0 && _elektrolyseurPower != 0 && _waterNeeded != 0)
        {
            legendText.text += "<u>" + $"Leistungs- und Ressourcenangaben: " + "</u>" +
                $"\nBenötigter Strom: {_electricityNeeded:#0.00} MW \nElektrolyseur Leistung: {_elektrolyseurPower:#0.00} MW \nBenötigtes Wasser: {_waterNeeded:#0.00} m\xB3/h \n" +
                $"Abschätzung Abwärme: {Math.Round((_elektrolyseurPower)/3,2)} MW";
        }
        if (_limitingDimension != null && _limitingDimension != "")
        {
            legendText.text += $"Limitierender Faktor: {_limitingDimension}";
        }

        
    }
}
