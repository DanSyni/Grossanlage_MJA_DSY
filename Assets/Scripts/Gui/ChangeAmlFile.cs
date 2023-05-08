using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChangeAmlFile : MonoBehaviour
{
    SwitchCanvases switchCanvases;

    [SerializeField] FileBrowserUpdate fileBrowser;
    [SerializeField] TextMeshProUGUI initialAmlPath;
    [SerializeField] TextMeshProUGUI standardAmlPath;

    void Start()
    {
        switchCanvases = GetComponent<SwitchCanvases>();
    }

    /// <summary>
    /// This method opens the file browser and updates the UI in the initial aml file selection screen with the selected path afterwards.
    /// </summary>
    public void ChangeFileNoFileLoaded() {
        fileBrowser.OpenFileBrowser();
        string path = AmlAdapter.GetPathOfCurrentDocument();
        if (path != null) initialAmlPath.text = path;
        switchCanvases.RelodeScene();
    }

    /// <summary>
    /// This method opens the file browser and a info text containing the selected path afterwards.
    /// </summary>
    public void ChangeFile() {
        fileBrowser.OpenFileBrowser();
        string path = AmlAdapter.GetPathOfCurrentDocument();
        if (path != null) standardAmlPath.text = path;
        switchCanvases.OpenToAmlPath();
    }
}
