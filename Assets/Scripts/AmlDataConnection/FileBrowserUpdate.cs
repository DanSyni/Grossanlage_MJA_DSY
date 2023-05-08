using AnotherFileBrowser.Windows;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FileBrowserUpdate : MonoBehaviour
{
    /// <summary>
    /// Open a dialog window for the user to select an aml file. 
    /// On selection the File will be tried to opend. The success can be checked via <c>AmlAdapter.HasValidAmlDocument()</c>.
    ///
    /// <para>Works currently only on Windows.</para>
    /// </summary>
    public void OpenFileBrowser()
    {
        var bp = new BrowserProperties();
        bp.filter = "AML files (*.aml) | *.aml";
        bp.filterIndex = 0;

        new FileBrowser().OpenFileBrowser(bp, path =>
        {
            Debug.Log($"Opening file: {AmlAdapter.LoadNewAmlFile(path)}");
        });
    }
}
