using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Text.RegularExpressions;

public class SaveInputs : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] TMP_InputField electrolyzerPower;
    [SerializeField] TMP_InputField electricityAvailability;
    [SerializeField] TMP_InputField width;
    [SerializeField] TMP_InputField length;
    [SerializeField] TMP_InputField waterAvailability;
    [Header("Validation Texts")]
    [SerializeField] TextMeshProUGUI valElectricity;
    [SerializeField] TextMeshProUGUI valElectrolyzer;
    [SerializeField] TextMeshProUGUI valWidth;
    [SerializeField] TextMeshProUGUI valLength;
    [SerializeField] TextMeshProUGUI valWater;
    [SerializeField] TextMeshProUGUI valEverything;



    int electrolyzerPowerInput;
    string electrolyzerPowerText;

    int electricityAvailabilitytInput;
    string electricityAvailabilitytText;

    int widthInput;
    string widthText;

    int lengthInput;
    string lengthText;

    int waterAvailabilityInput;
    string waterAvailabilityText;

    int result;
    //0: Electricity , 1: Electrolyzeur, 2: width, 3: length, 4: water
    int[] validInputs = { 0, 0, 0, 0, 0}; //0 = empty, 1 = correct Input, 2 = not number Input, 3 = negative input
    private bool wrongInputs = false;
    private bool everythingEmpty = false;
    private bool combinationCorrect = false;

    private const string WarningNumber = "Bitte geben Sie eine Zahl ein";
    private const string WarningPosNumber = "Bitte geben Sie eine positive Zahl ein";


    //Voreinstellung
    
    public struct PlayerPrefsKeys
    {
        public const string electricityAvailability = "ElectricityAvailability";
        public const string electrolyzerPower = "ElectrolyzerPower";
        public const string width = "Width";
        public const string length = "Length";
        public const string waterAvailability = "WaterAvailability";
        
    }


    //method to save Valid Inputs
    public void SaveInput()
    {
        
        //check if Electricy Input it's a number 
        if(int.TryParse(electricityAvailability.text, out result))
        {
            //Parse Input into string and set validaiton variable to correct 
            electricityAvailabilitytInput = int.Parse(electricityAvailability.text);
            electricityAvailabilitytText = electricityAvailabilitytInput.ToString();
            //check if Input is positive
            if (electricityAvailabilitytInput <= 0)
            {
                validInputs[0] = 3;
                valElectricity.text = WarningPosNumber;
                return;
            }
            validInputs[0] = 1;
            //save Input to PlayerPrefs
            PlayerPrefs.SetInt(PlayerPrefsKeys.electricityAvailability, electricityAvailabilitytInput);
            //empty the error text field (needed when last input was incorrect)
            valElectricity.text = $"";
        }
        //check if if it's empty
        else if (electricityAvailability.text == "")
        {
            //set validation variable to empty
            validInputs[0] = 0;
            //empty excisting PlayerPref
            PlayerPrefs.DeleteKey(PlayerPrefsKeys.electricityAvailability);
            //empty the error text field (needed when last input was not a number)
            valElectricity.text = $"";
        }
        else
        {
            //set error text field
            valElectricity.text = WarningNumber;
            //set validation variable to incorrect
            validInputs[0] = 2;
        }

        //check Electrolyzer Input
        if (int.TryParse(electrolyzerPower.text, out result))
        {
            electrolyzerPowerInput = int.Parse(electrolyzerPower.text);
            electrolyzerPowerText = electrolyzerPowerInput.ToString();
            if (electrolyzerPowerInput <= 0)
            {
                validInputs[1] = 3;
                valElectrolyzer.text = WarningPosNumber;
                return;
            }
            PlayerPrefs.SetInt(PlayerPrefsKeys.electrolyzerPower, electrolyzerPowerInput);
            validInputs[1] = 1;
            valElectrolyzer.text = $"";

        }
        else if (electrolyzerPower.text == "")
        {
            validInputs[1] = 0;
            PlayerPrefs.DeleteKey(PlayerPrefsKeys.electrolyzerPower);
            valElectrolyzer.text = $"";
        }
        else
        {
            valElectrolyzer.text = WarningNumber;
            validInputs[1] = 2;
        }

        //check width Input
        if (int.TryParse(width.text, out result))
        {
            widthInput = int.Parse(width.text);
            widthText = widthInput.ToString();
            if (widthInput <= 0)
            {
                validInputs[2] = 3;
                valWidth.text = WarningPosNumber;
                return;
            }
            PlayerPrefs.SetInt(PlayerPrefsKeys.width, widthInput);
            validInputs[2] = 1;
            valWidth.text = $"";
        }
        else if (width.text == "")
        {
            validInputs[2] = 0;
            PlayerPrefs.DeleteKey(PlayerPrefsKeys.width);
            valWidth.text = $"";
        }
        else
        {
            valWidth.text = WarningNumber;
            validInputs[2] = 2;
        }

        //check length Input
        if (int.TryParse(length.text, out result))
        {
            lengthInput = int.Parse(length.text);
            lengthText = lengthInput.ToString();
            if (lengthInput <= 0)
            {
                validInputs[3] = 3;
                valLength.text = WarningPosNumber;
                return;
            }
            PlayerPrefs.SetInt(PlayerPrefsKeys.length, lengthInput);
            validInputs[3] = 1;
            valLength.text = $"";
        }
        else if (length.text == "")
        {
            validInputs[3] = 0;
            PlayerPrefs.DeleteKey(PlayerPrefsKeys.length);
            valLength.text = $"";
        }
        else
        {
            valLength.text = WarningNumber;
            validInputs[3] = 2;
        }

        //check waterAvailability Input
        if (int.TryParse(waterAvailability.text, out result))
        {
            waterAvailabilityInput = int.Parse(waterAvailability.text);
            waterAvailabilityText = waterAvailabilityInput.ToString();
            if (waterAvailabilityInput <= 0)
            {
                validInputs[4] = 3;
                valWater.text = WarningPosNumber;
                return;
            }
            PlayerPrefs.SetInt(PlayerPrefsKeys.waterAvailability, waterAvailabilityInput);
            validInputs[4] = 1;
            valWater.text = $"";
        }
        else if (waterAvailability.text == "")
        {
            validInputs[4] = 0;
            PlayerPrefs.DeleteKey(PlayerPrefsKeys.waterAvailability);
            valWater.text = $"";
        }
        else
        {
            valWater.text = WarningNumber;
            validInputs[4] = 2;
        }

        //check if one of the inputs is not valid
        for (int i = 0; i < validInputs.Length; i++)
        {
            result += validInputs[i];
            if (validInputs[i] == 2 || validInputs[i] == 3)
            {
                wrongInputs = true;
                valEverything.text = $"";
            }
        }

        //0: Electricity , 1: Electrolyzeur, 2: width, 3: length, 4: water
        string test = $"{validInputs[0]}{validInputs[1]}{validInputs[2]}{validInputs[3]}{validInputs[4]}";
        Regex regex = new Regex("^((10|01)(0|1){2}0|(0|1)0(0|1){2}1)$"); //should validate strings that ((10|01)(0|1){2}) sets exclusivly 0 or 1, 2 and 3 are irrelevant, 4 is set to 0, or (0|1)0(0|1){2}1) where 1 is ste to 0 or 1, 2 is set to 0, 2 and 3 are irrelevant and 4 is set to 1
        if (regex.Match(test).Success) combinationCorrect = true;
        else
        {
            combinationCorrect = false;
            valEverything.text = $"Bitte geben Sie eine valide Kombination ein";
        }

        //check if all Input Fields are empty
        if (result == 0)
        {
            everythingEmpty = true;
            valEverything.text = $"Bitte geben Sie mindesten eine Eingabe an";
        }

    }

    public void setStandardParameter(string elPower,string givenWidth, string givenLength)
    {
        electrolyzerPower.text = elPower;
        width.text = givenWidth;
        length.text = givenLength;
    }

    //methods used in SwitchCanvas-script to check if everything is valid
    public bool GetWrongInputsBool()
    {
        return wrongInputs;
    }

    public bool GetEverythingEmpty()
    {
        return everythingEmpty;
    }

    public bool GetCorrectCombination()
    {

        return combinationCorrect;
    }

    //reset variables to check again
    public void ResetInputValidation()
    {
        for (int i = 0; i < validInputs.Length; i++)
        {
            validInputs[i] = 0;
        }
        result = 0;
        wrongInputs = false;
        everythingEmpty = false;
        combinationCorrect = false;
    }

}
