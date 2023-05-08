using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SwitchCanvases : MonoBehaviour
{
    //Canvases and script
    [SerializeField] GameObject amlPath;
    [SerializeField] GameObject gui_Eingabe;
    [SerializeField] GameObject help_window;
    [SerializeField] GameObject amlStart;
    [SerializeField] GameObject menu_dropdown;
    [SerializeField] AmlAdapter amlAdapter;
  

    [SerializeField] SaveInputs saveInputs;


    bool inputWrong;
    bool windowOpen = false;
    

    // Start is called before the first frame update
    void Start()
    {
        saveInputs.setStandardParameter("1000", "150", "200");

        //go diretly to GUI scene if there is a valid AML-File
        if (amlAdapter.HasValidAmlDocument()){
            amlStart.SetActive(false);
            amlPath.SetActive(false);
            gui_Eingabe.SetActive(true);
            help_window.SetActive(false);
            menu_dropdown.SetActive(false);
            
        }
        //go to AML-start-Scene if not
        else{
            amlStart.SetActive(true);
            amlPath.SetActive(false);
            gui_Eingabe.SetActive(false);
            help_window.SetActive(false);
            menu_dropdown.SetActive(false);
        }

        //make sure that the cursor is visible and can be moved freely 
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Cursor should be movable");
    }

    //Methods to open/close windows(canvases)
    public void OpenToAmlPath()
    {
        amlPath.SetActive(true);
    }

    public void CloseAmlPath()
    {
        amlPath.SetActive(false);
    }

    public void OpenHelpWindow()
    {
        help_window.SetActive(true);
    }

    public void CloseHelpWindow()
    {
        help_window.SetActive(false);
    }

    private void SwitchToInput()
    {
        
        gui_Eingabe.SetActive(true);
        amlStart.SetActive(false);
    }

    public void SwitchToDropdown()
    {
        if (!windowOpen)
        {
            menu_dropdown.SetActive(true);
            windowOpen = true;
        }
        else
        {
            menu_dropdown.SetActive(false);
            windowOpen = false;
        }
    }

    //switch to visualisation scene if the input is valid
    public void SwitchToVisualisation()
    {
        if (saveInputs.GetWrongInputsBool() == true || saveInputs.GetEverythingEmpty() == true|| saveInputs.GetCorrectCombination() == false )
        {
            saveInputs.ResetInputValidation();
        }
        else
        {
            SceneManager.LoadScene("PlantVisualisation");
        }       
    }

    //only switch from AML-start to GUI is there is valid AML-file
    public void RelodeScene()
    {
        if (amlAdapter.HasValidAmlDocument() == true)
        {
            Invoke("SwitchToInput", 2f);
        }           
    }

   

    //quit Application
    public void CloseBuild()
    {
        Application.Quit();
    }
}
