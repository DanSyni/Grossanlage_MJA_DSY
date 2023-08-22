using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Aml.Engine.CAEX;
using System;
using System.Xml.XPath;
using UnityEngine.SceneManagement;
using Aml.Engine.CAEX.Extensions;

/// <summary>
/// Class <c>AmlAdapter</c> wraps the relevant .aml file and provides methods to access the data more easily. 
/// </summary>
public class AmlAdapter : MonoBehaviour
{
    private CAEXDocument caexDocument;
    private const string SavedDocPathKey = "SavedPathToAml";

    private static AmlAdapter instance;
    public enum AmlComponentNames{ ProcessControlUnit, WaterSupply, GasSystem, CoolingSystem, PowerSupply, EnapterSkid, HoellerSkid, PlugpowerSkid };

    private void Awake()
    {
        if (instance != null) Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);
        instance = this;
        Debug.Log($"Trying to load old aml file: {LoadOldAmlFile()}");
    }

    /// <summary>
    /// Returns the currently saved AmlAdapter instance.
    /// </summary>
    /// <returns>The current class instance, needed to perform many further operations on the aml data.</returns>
    public static AmlAdapter GetInstance() {
        return instance;
    }

    /// <summary>
    /// Returns if a valid aml document is currently loaded.
    /// </summary>
    /// <returns>Returns true if a valid aml document is currently loaded.</returns>
    public Boolean HasValidAmlDocument() {
        return caexDocument != null;
    }

    /// <summary>
    /// Returns the path of the last selected aml document. 
    /// This is not a guarantee that this file still exists or that it got loaded successfully.
    /// </summary>
    /// <returns>The path to the last selected aml document.</returns>
    public static string GetPathOfCurrentDocument() {
        string path = PlayerPrefs.GetString(SavedDocPathKey, "");
        if (path == "") return null;
        return path;
    }

    /// <summary>
    /// This method gets called if an operation on the aml document should be executed
    /// and there is currently no valid aml document loaded.
    /// </summary>
    private void HandleNoDocument() {
        SceneManager.LoadScene("GUI");
    }

    /// <summary>
    /// Load an aml file that was just selected by the user and save the path.
    /// </summary>
    /// <param name="path">The path of the document that was selected</param>
    /// <returns>True if there is now a valid document; otherwise false</returns>
    public static Boolean LoadNewAmlFile(string path) {
        if (!path.Contains(".aml")) return false;

        try {
            CAEXDocument tempDoc = CAEXDocument.LoadFromFile(path);
            instance.caexDocument = tempDoc;
            PlayerPrefs.SetString(SavedDocPathKey, path);

            return instance.HasValidAmlDocument();
        }
        catch (System.IO.FileNotFoundException e) {
            Debug.Log($"{e}, {path}");
            return false;
        }
    }

    private static Boolean LoadOldAmlFile() {
        string path = PlayerPrefs.GetString(SavedDocPathKey);
        if (path == null) return false;

        try {
            CAEXDocument doc = CAEXDocument.LoadFromFile(path);
            instance.caexDocument = doc;
            return true;
        }
        catch (System.IO.FileNotFoundException e) {
            Debug.Log(e);
            return false;
        }
    }

    /// <summary>
    /// This is a prototype for encapsulating attributes for the plant. In future versions this could be used to pass AmlAttributes as parameters to the method.
    /// </summary>
    public readonly struct AmlAttribute {
        public AmlAttribute(string name, string value,  string unit, Type dataType) {
            Name = name;
            Value = value;
            DataType = dataType;
            Unit = unit;
        }

        public string Name { get; }
        public string Value { get; }
        public Type DataType { get; }
        public string Unit { get; }
    }

    /// <summary>
    /// Add a representation of a plant to the instance hierarchy of the currently selected aml document.
    /// </summary>
    /// <param name="plantName">The name of the parent node which represents the plant</param>
    /// <param name="countUnits">A dictionary containing the numbers of required components like the electrolyzers or cooling units. The corresponding number of elements will be added to the instance hierarchy.</param>
    /// <param name="overrideIH">Determins if the current instance hierarchy will be overwritten. When overwriting only the new plant will be present in the instance hierarchy. If set to false the new plant will be appended as a new element and everything else remains untouched.</param>
    public void WriteToAmlIH(string plantName, Dictionary<AmlComponentNames, int> countUnits, bool overrideIH)
    {
        WriteToAmlIH(plantName, countUnits, overrideIH, null);
    }

    /// <summary>
    /// Add a representation of a plant to the instance hierarchy of the currently selected aml document.
    /// </summary>
    /// <param name="plantName">The name of the parent node which represents the plant</param>
    /// <param name="countUnits">A dictionary containing the numbers of required components like the electrolyzers or cooling units. The corresponding number of elements will be added to the instance hierarchy.</param>
    /// <param name="overrideIH">Determins if the current instance hierarchy will be overwritten. When overwriting only the new plant will be present in the instance hierarchy. If set to false the new plant will be appended as a new element and everything else remains untouched.</param>
    /// <param name="attributes">A list of attributes that will be added to the root element of the instance hierarchy</param>
    public void WriteToAmlIH(string plantName, Dictionary<AmlComponentNames, int> countUnits, bool overrideIH, List<AmlAttribute> attributes) 
    {
        if (!HasValidAmlDocument())
        {
            HandleNoDocument();
            return;
        }
        //create the hierarchy node
        var hierarchies = caexDocument.CAEXFile.InstanceHierarchy;
        if (overrideIH)
        {
            hierarchies.Remove();
        }
        var newIH = caexDocument.CAEXFile.New_InstanceHierarchy(plantName);
        var plantRoot = newIH.InternalElement.Append("PlantRoot");

        ArrayList categories = new ArrayList();
        categories.Add((name: "PlantUnit_Control", amlElementNames: new AmlComponentNames[] { AmlComponentNames.ProcessControlUnit }));
        categories.Add((name: "PlantUnit_Water", amlElementNames: new AmlComponentNames[] { AmlComponentNames.WaterSupply }));
        categories.Add((name: "PlantUnit_Gas", amlElementNames: new AmlComponentNames[] { AmlComponentNames.GasSystem }));
        categories.Add((name: "PlantUnit_Cooling", amlElementNames: new AmlComponentNames[] { AmlComponentNames.CoolingSystem }));
        categories.Add((name: "PlantUnit_Power", amlElementNames: new AmlComponentNames[] { AmlComponentNames.PowerSupply }));
        categories.Add((name: "AEM_Units", amlElementNames: new AmlComponentNames[] { AmlComponentNames.EnapterSkid }));
        categories.Add((name: "PEM_Units", amlElementNames: new AmlComponentNames[] { AmlComponentNames.HoellerSkid, AmlComponentNames.PlugpowerSkid }));

        //iterate over the categories, create it in the aml file and append the corresponding count of elements
        InternalElementType electrolysisCategory = null;
        foreach ((string name, AmlComponentNames[] amlElementNames) category in categories)
        {
            //skip category if no corresponding key is set in the parameter countUnits
            bool minOneKeyIsInDictionary = false;
            foreach (var key in category.amlElementNames)
            {
                if (countUnits.ContainsKey(key))
                {
                    minOneKeyIsInDictionary = true;
                    break;
                }
            }
            if (!minOneKeyIsInDictionary) continue;

            //create the category
            InternalElementType currentCategory;
            if (category.name == "AEM_Units" || category.name == "PEM_Units")
            {
                if (electrolysisCategory == null) electrolysisCategory = plantRoot.InternalElement.Append("PlantUnit_Electrolysis");
                currentCategory = electrolysisCategory.InternalElement.Append(category.name);
            }
            else
            {
                currentCategory = plantRoot.InternalElement.Append(category.name);
            }

            //fill the category with elements
            foreach (AmlComponentNames amlElementName in category.amlElementNames)
            {
                var currentClass = SearchElement(GetComponentName(amlElementName));

                if (!countUnits.ContainsKey(amlElementName)) continue;
                for (int i = 0; i < countUnits[amlElementName]; i++)
                {
                    var instance = currentClass.CreateClassInstance();
                    instance.Name = $"{instance.Name}_{i + 1}";
                    currentCategory.Insert(instance);
                }

            }
        }

        //add relevant attributes to the root node (plantRoot)
        if (attributes != null) {
            foreach (var amlAttribute in attributes) { 
                var attributeElement = plantRoot.Attribute.Append(amlAttribute.Name); 
                attributeElement.AttributeDataType = AttributeTypeType.ClrToXmlType(amlAttribute.DataType);
                attributeElement.Value = amlAttribute.Value;
                attributeElement.AttributeValue = amlAttribute.Value;
                attributeElement.Unit = amlAttribute.Unit;
            }
        }

        string path = GetPathOfCurrentDocument();
        if (path == null)
        {
            Debug.Log("The writing to the AML File failed.");
            return;
        }

        caexDocument.SaveToFile(path, true);
        Debug.Log("Saved Aml file");
    }

    private string GetComponentName(AmlComponentNames name) {
        switch (name) {
            case AmlComponentNames.ProcessControlUnit: return "ProcessControlUnit";
            case AmlComponentNames.WaterSupply: return "WaterSupply";
            case AmlComponentNames.GasSystem: return "GasSystem";
            case AmlComponentNames.CoolingSystem: return "CoolingSystem";
            case AmlComponentNames.PowerSupply: return "PowerSupply";
            case AmlComponentNames.EnapterSkid: return "EnapterSkid";
            case AmlComponentNames.HoellerSkid: return "HoellerSkid";
            case AmlComponentNames.PlugpowerSkid: return "PlugpowerSkid";

            default: return null;
        }
    }

    /// <summary>
    /// Search in the SystemUnitClassLib after the named element.
    /// </summary>
    /// <param name="name">The name of the searched element.</param>
    /// <returns>The element from the hierachry speciefied by the name or null if no suiting element could be found.</returns>
    public SystemUnitFamilyType SearchElement(string name)
    {
        if (name == null) return null;
        if (!HasValidAmlDocument()) {
            HandleNoDocument();
            return null;
        }

        var systemUnitClassLib = caexDocument.CAEXFile.SystemUnitClassLib;
        foreach (var element in systemUnitClassLib) {
            var result = SearchElement(name, element.GetEnumerator());
            if (result != null) return result;
        }
        return null;
    }

    private SystemUnitFamilyType SearchElement(string name, IEnumerator<SystemUnitFamilyType> enumerator)
    {
        if (name == null || enumerator == null) return null;
        if (!HasValidAmlDocument())
        {
            HandleNoDocument();
            return null;
        }

        for (int i = 0; enumerator.MoveNext(); i++)
        {
            var element = enumerator.Current;
            if (element.Name == name)
            {
                return element;
            }
            else {
                var result = SearchElement(name, element.GetEnumerator());
                if (result != null) return result;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns an attribute of the passed element specified by its name.
    /// </summary>
    /// <param name="element">The element to be searched.</param>
    /// <param name="attributeName">The name of the attribute.</param>
    /// <returns>The attribute or null if it could not be found.</returns>
    public AttributeType GetAttribute(IObjectWithAttributes element, string attributeName) {
        if (element == null ||attributeName == null) return null;
        if (!HasValidAmlDocument())
        {
            HandleNoDocument();
            return null;
        }

        foreach (var attribute in element.AttributeAndDescendants) {
            if (attribute.Name == attributeName) return attribute;
        }
        return null;
    }

    /// <summary>
    /// Returns an attribute of the Aml element specified by its name.
    /// </summary>
    /// <param name="elementName">The name of the element to be searched.</param>
    /// <param name="attributeName">The name of the attribute.</param>
    /// <returns>The attribute or null if it could not be found.</returns>
    public AttributeType GetAttribute(string elementName, string attributeName) {
        if (elementName == null || attributeName == null) return null;
        if (!HasValidAmlDocument())
        {
            HandleNoDocument();
            return null;
        }

        var element = SearchElement(elementName);
        if (element == null) return null;

        return GetAttribute(element, attributeName);
    }

    /// <summary>
    /// Returns all attributes of the Aml element specified by its name.
    /// </summary>
    /// <param name="elementName">The name of the element to be searched.</param>
    /// <param name="attributeName">The name of the attribute.</param>
    /// <returns>The attribute or null if it could not be found.</returns>
    public List<string> GetAllAttributes(string elementName)
    {
        var attList = new List<string>();
        var attValueList = new List<string>();
        if (elementName == null) return null;
        if (!HasValidAmlDocument())
        {
            HandleNoDocument();
            return null;
        }
        var element = SearchElement(elementName);
        if (element == null) return null;

        foreach (var attribute in element.AttributeAndDescendants)
        {
            var currentAtt = GetAttribute(element, attribute.Name);
            var currentValue = currentAtt.Value;
            var currentUnit = currentAtt.Unit;
            if (currentValue == null)
            {
                currentValue = "0";
            }
            if (currentUnit == null)
            {
                currentUnit = "";
            }
            string currentAttString = currentAtt.ToString();
            string currentValueString = currentValue.ToString();
            string currentUnitString = currentUnit.ToString();
            attList.Add(currentAttString+": "+currentValueString + " "+ currentUnitString);
        }
        return attList;
    }

    /// <summary>
    /// Returns the ExternalInterfaceType specified by the parameters.
    /// </summary>
    /// <param name="nameOfParent">The name of the parent element to be searched.</param>
    /// <param name="interfaceName">The name of the ExternalInterface.</param>
    /// <returns>The ExternalInterfaceType or null if it could not be found.</returns>
    public ExternalInterfaceType GetExternalInterface(string nameOfParent, string interfaceName) {
        if (nameOfParent == null || interfaceName == null) return null;
        if (!HasValidAmlDocument())
        {
            HandleNoDocument();
            return null;
        }

        var parent = SearchElement(nameOfParent);
        if (parent == null) return null;
        return GetExternalInterface(parent, interfaceName);
    }

    /// <summary>
    /// Returns the ExternalInterfaceType specified by the parameters.
    /// </summary>
    /// <param name="parent">The parent element to be searched.</param>
    /// <param name="interfaceName">The name of the ExternalInterface.</param>
    /// <returns>The ExternalInterfaceType or null if it could not be found.</returns>
    public ExternalInterfaceType GetExternalInterface(SystemUnitFamilyType parent, string interfaceName){
        if (parent == null || interfaceName == null) return null;
        if (!HasValidAmlDocument())
        {
            HandleNoDocument();
            return null;
        }

        var externalInterfaces = parent.ExternalInterfaceAndDescendants;
        foreach (var externalInterface in externalInterfaces) {
            if (externalInterface.Name == interfaceName) {
                return externalInterface;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns an attribute of an ExternalInteface element specified by its name.
    /// </summary>
    /// <param name="nameOfParent">The name of the parent of the ExternalInteface element.</param>
    /// <param name="interfaceName">The name of the ExternalInterface element.</param>
    /// <param name="attributeName">The name of the searched attribute.</param>
    /// <returns>The attribute or null if it could not be found.</returns>
    public AttributeType GetAttributeOfExternalInterface(string nameOfParent, string interfaceName, string attributeName) {
        if (nameOfParent == null || interfaceName == null || attributeName == null) return null;
        if (!HasValidAmlDocument())
        {
            HandleNoDocument();
            return null;
        }

        var externalInterface = GetExternalInterface(nameOfParent, interfaceName);
        if (externalInterface == null) return null;

        return GetAttribute(externalInterface, attributeName);
    }

    /// <summary>
    /// Returns a reference to the GameObject of an aml element. To work properly, the element must have a CADDocument ExternalInterface and link to a .dae file.
    /// 
    /// <example>
    /// <code>
    /// var gameObject = amlTest.GetColladaGameObject("GasSystem");
    /// if (gameObject != null) Instantiate(gameObject);
    /// </code>
    /// </example>
    /// 
    /// </summary>
    /// <param name="parentName">The name of the aml element.</param>
    /// <returns>The reference to the GameObject of the aml element or null if it could not be found.</returns>
    public GameObject GetColladaGameObject(string parentName)
    {
        if (parentName == null) return null;
        if (!HasValidAmlDocument())
        {
            HandleNoDocument();
            return null;
        }

        var pathAttribute = GetAttributeOfExternalInterface(parentName, "CADDocument", "refURI");
        if (pathAttribute == null) return null;

        string path = pathAttribute.Value;
        //only .dae files should be valid here
        if (path.Substring(path.Length - 4) != ".dae") 
        {
            Debug.Log($"{path} is not a .dae file. No GameObject could be created.");
            return null;
        }
        //.dae has to be removed from the path in order to use Resources.Load()
        path = path.Substring(0, path.Length - 4);

        var file = Resources.Load(path);
        
        try {
            GameObject g = (GameObject)file;
            return g;
        }
        catch (InvalidCastException e)
        {
            Debug.Log(e);
            return null;
        }
    }

    /// <summary>
    /// This method can be used to extract data from a .xml file containing the position data of the connectors of the containers.
    /// </summary>
    /// <param name="parentName">The name of the aml element.</param>
    /// <returns>A list of ConnectionPosData objects of the total data in the PositionXML file or null if no fitting data could be found.</returns>
    public List<ConnectionPosData> GetXMLPos(string parentName)
    {
        if (!HasValidAmlDocument())
        {
            HandleNoDocument();
            return null;
        }

        var pathAttribute = GetAttributeOfExternalInterface(parentName, "PositionXML", "refURI");
        if (pathAttribute == null) return null;
        
        string path = $"Assets/Resources/{pathAttribute.Value}";

        var doc = new XPathDocument(path);
        if (doc == null) return null;
        var navigator = doc.CreateNavigator();
        XPathNodeIterator parameters = navigator.Select("/ParamWithValueList/parameters/ParamWithValue");

        List<ConnectionPosData> data = new List<ConnectionPosData>();

        //iterate over every <ParamWithValue> element in the xml
        for (int i = 0; parameters.MoveNext(); i++) {
            var nav2 = parameters.Clone().Current;
             if (!nav2.MoveToFirstChild()) return null;

            string name = "";
            string typeCode = "";
            string value = "";
            Boolean isKey;
            //iterate over the inner Elements and create ConnectionPosData objects containing the information
            do
            {
                switch (nav2.Name) {
                    case "name": name = nav2.Value; break;
                    case "typeCode": typeCode = nav2.Value; break;
                    case "value": value = nav2.Value; break;
                    case "isKey": isKey = (nav2.Value).Equals("true");
                            data.Add(new ConnectionPosData(name, typeCode, value, isKey));
                            break;
                    default: break;
                }
            } while (nav2.MoveToNext());
        }

        return data;
    }

    /// <summary>
    /// This method can be used to extract data from a .xml file containing the position data of the connectors of the containers.
    /// </summary>
    /// <param name="parentName">The name of the aml element.</param>
    /// <param name="attributeName">The name of the searched ParamWithValue element in the xml file.</param>
    /// <returns>A ConnectionPosData object containing the data of an attribute in the PositionXML file or null if no fitting data could be found.</returns>
    public ConnectionPosData GetXMLPos(string parentName, string attributeName) {
        if (!HasValidAmlDocument())
        {
            HandleNoDocument();
            return null;
        }

        var pathAttribute = GetAttributeOfExternalInterface(parentName, "PositionXML", "refURI");
        if (pathAttribute == null) return null;

        string path = $"Assets/Resources/{pathAttribute.Value}";

        var doc = new XPathDocument(path);
        if (doc == null) return null;
        var navigator = doc.CreateNavigator();
        XPathNodeIterator parameters = navigator.Select("/ParamWithValueList/parameters/ParamWithValue");

        //iterate over every <ParamWithValue> element in the xml
        for (int i = 0; parameters.MoveNext(); i++)
        {
            var nav2 = parameters.Clone().Current;
            if (!nav2.MoveToFirstChild()) return null;

            string name = "";
            string typeCode = "";
            string value = "";
            Boolean isKey;
            //iterate over the inner Elements and create ConnectionPosData objects containing the information
            do
            {
                switch (nav2.Name)
                {
                    case "name": name = nav2.Value; break;
                    case "typeCode": typeCode = nav2.Value; break;
                    case "value": value = nav2.Value; break;
                    case "isKey":
                        isKey = (nav2.Value).Equals("true");
                        if (name == attributeName) return new ConnectionPosData(name, typeCode, value, isKey);
                        break;
                    default: break;
                }
            } while (nav2.MoveToNext());
        }

        return null;
    }

    /// <summary>
    /// A wrapper class specialized on the position data, which encapsulates the data of the connectors of the containers.
    /// </summary>
    public class ConnectionPosData {
        private string name;
        public string Name { get { return name; } }
        private string typeCode;
        public string TypeCode { get { return typeCode; } }
        private string value;
        public string Value { get { return value; } }
        private Boolean isKey;
        public Boolean IsKey { get { return isKey;  } }
        public ConnectionPosData(string name, string typeCode, string value, Boolean isKey){
            this.name = name;
            this.typeCode = typeCode;
            this.value = value;
            this.isKey = isKey;
        }

        public override string ToString() {
            return $"{name}, {typeCode}, {value}, {isKey}";
        }
    }
}