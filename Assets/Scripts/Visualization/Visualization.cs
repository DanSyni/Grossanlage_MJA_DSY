using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Aml.Engine.CAEX;
using System.Globalization;

public class Visualization : MonoBehaviour
{
    public enum ComponentNames { WaterSupply, GasSystem, CoolingSystem, PowerSupply, EnapterSkid, HoellerSkid, PlugpowerSkid};
    private static Dictionary<ComponentNames, string> amlNames = new Dictionary<ComponentNames, string>()
    {
        {ComponentNames.WaterSupply, "WaterSupply"},
        {ComponentNames.GasSystem, "GasSystem"},
        {ComponentNames.CoolingSystem, "CoolingSystem"},
        {ComponentNames.PowerSupply, "PowerSupply"},
        {ComponentNames.EnapterSkid, "EnapterSkid"},
        {ComponentNames.HoellerSkid, "HoellerSkid"},
        {ComponentNames.PlugpowerSkid, "PlugpowerSkid"}
    };

    static PlacingLogic placingLogic;
    static VisualizationWarnings visualizationWarnings;
    [SerializeField] SwitchToGui legendTextNotStatic;
    static SwitchToGui legendText;

    //dimensions of the used containers
    private static double containerWidth;
    private static double containerLength;
    private static double containerHeight;

    //user input entered in the gui for the width and length of the plant.
    private static int userInputPlantWidth;
    private static int userInputPlantLength;

    //length of rows with and without the maintenance corridor, set in the algorithm
    private static double lengthRowWithoutCorridor;
    private static double lengthRowWithCorridor;

    //determines the length of the corridor
    private const float corridorFactor = 1.3f;
    private const float lengthPipingStructure = 3;
    private const double widthPipingStructure = 8.1;
    private const float stairStructWidth = 1.9f;
    private const double stairStructLength = 8.1;

    private void Awake()
    {
        legendText = legendTextNotStatic;
        placingLogic = GetComponent<PlacingLogic>();
        visualizationWarnings = GetComponent<VisualizationWarnings>();

        LoadContainerDimensions();
    }

    /// <summary>
    /// Interface for generating a layout and instantiating it in the scene.
    /// </summary>
    /// <param name="list">Contains the names and the amount of the components that should be placed.</param>
    public static void Visualize(List<Tuple<ComponentNames, int>> list)
    {
        GenerateLayout(list);
    }

    /// <summary>
    /// Read the container dimensions of the used container from the aml file. 
    /// The inputs get saved in the attributes containerWidth, containerLength abd containerHeight.
    /// </summary>
    private void LoadContainerDimensions()
    {
        AmlAdapter amlAdapter = AmlAdapter.GetInstance();

        //read the dimensons of the container from the aml file
        const string containerNameInAml = "40ft_Container";
        AttributeType widthAttr = amlAdapter.GetAttribute(containerNameInAml, "Width");
        AttributeType lengthAttr = amlAdapter.GetAttribute(containerNameInAml, "Length");
        AttributeType heightAttr = amlAdapter.GetAttribute(containerNameInAml, "Height"); 
        if (heightAttr == null) heightAttr = amlAdapter.GetAttribute(containerNameInAml, "Heigth"); //WRONG WRITTEN NAME IN AML, CAN BE REMOVED AFTER RENAMING

        //set the values in the corresponding attributes
        int factor_mm_in_m = 1000;
        if (widthAttr != null) containerWidth = double.Parse(widthAttr.Value, CultureInfo.InvariantCulture) / factor_mm_in_m;
        if (lengthAttr != null) containerLength = double.Parse(lengthAttr.Value, CultureInfo.InvariantCulture) / factor_mm_in_m;
        if (heightAttr != null) containerHeight = double.Parse(heightAttr.Value, CultureInfo.InvariantCulture) / factor_mm_in_m;
        
    }

    /// <summary>
    /// Check each container and determine the maximum SaftyZone
    /// </summary>
    /// <returns>The maximum security distance needed in meters.</returns>
    private static double CalculateMaxSecurityDistance() {
        AmlAdapter amlAdapter = AmlAdapter.GetInstance();
        double maxSecurityDistance = 0;
        foreach (string amlName in amlNames.Values)
        {
            const string attributeName = "RectSafteyZone";
            AttributeType attribute = amlAdapter.GetAttribute(amlName, attributeName);
            if (attribute == null) continue;

            double securityDistance = double.Parse(attribute.Value, CultureInfo.InvariantCulture);
            if (securityDistance > maxSecurityDistance) maxSecurityDistance = securityDistance;
        }
        return maxSecurityDistance;
    }

    /// <summary>
    /// Add up all container numbers to calculate the total number of containers needed.
    /// </summary>
    /// <param name="list">A list with the names of needed Containers and the corresponding amount.</param>
    /// <returns>The total sum of needed containers</returns>
    private static int CalculateTotalContainerCount(List<Tuple<ComponentNames, int>> list, string returningElement) {
        var containerList = new List<int>();
        int containerCount = 0;
        int containerELCount = 0;
        int containerPSCount = 0;
        int containerWTCount = 0;
        int containerGPCount = 0;
        int containerCLCount = 0;

        foreach (var tuple in list) { 
            containerCount += tuple.Item2;
            if (tuple.Item1.ToString() == "GasSystem")
            {
                containerGPCount += tuple.Item2;
            }
            else if (tuple.Item1.ToString() == "HoellerSkid" || tuple.Item1.ToString() == "PlugpowerSkid" || tuple.Item1.ToString() == "EnapterSkid")
            {
                containerELCount += tuple.Item2;
            }
            else if (tuple.Item1.ToString() == "WaterSupply")
            {
                containerWTCount += tuple.Item2;
            }
            else if (tuple.Item1.ToString() == "CoolingSystem")
            {
                containerCLCount += tuple.Item2;
            }
            else if (tuple.Item1.ToString() == "PowerSupply")
            {
                containerPSCount += tuple.Item2;
            }
        }

        containerList.Add(containerCount);
        containerList.Add(containerELCount);
        containerList.Add(containerPSCount);
        containerList.Add(containerWTCount);
        containerList.Add(containerGPCount);
        containerList.Add(containerCLCount);

        switch (returningElement)
        {
            case "containerCount":
                return containerList[0];
                break;

            case "containerELCount":
                return containerList[1];
                break;

            case "containerPSCount":
                return containerList[2];
                break;

            case "containerWTCount":
                return containerList[3];
                break;

            case "containerGPCount":
                return containerList[4];
                break;

            case "containerCLCount":
                return containerList[5];
                break;

        }

        return containerList[1];

    }

    /// <summary>
    /// Read and save the length and with constraints entered by the user from the PlayerPrefs.
    /// If there is none, set the corresponding constraint to -1.
    /// </summary>
    private static void ReadUserInputPlantDimensions() {
        string widthKey = SaveInputs.PlayerPrefsKeys.width;
        string lengthKey = SaveInputs.PlayerPrefsKeys.length;

        userInputPlantWidth = PlayerPrefs.GetInt(widthKey, -1);
        userInputPlantLength = PlayerPrefs.GetInt(lengthKey, -1);
    }

    /// <summary>
    /// Calculates the amount of needed collumns based on the container count. It aims to create a square. The algorithm does not account for constraints.
    /// </summary>
    /// <param name="countContainers">The total amount of containers that will be placed in the layout.</param>
    /// <returns>The amount of needed columns in the layout.</returns>
    private static int CalcutlateNeededColumnsNoConstraints(int countContainers) {
        int result = (int)Math.Ceiling(Math.Sqrt(countContainers) *0.4);
        return Math.Max(1, result);
    }

    /// <summary>
    /// Calculates the amount of needed collumns based on the container count. The algorithm takes one constraint for the length into account. The width is not limited.
    /// </summary>
    /// <param name="countContainers">The amount of containers that should fit in the plant layout.</param>
    /// <param name="limitedDimension">The maximum plant length</param>
    /// <returns>The amount of needed columns in the layout or -1 if no column fits with the constraint.</returns>
    private static int CalculateNeededColumnsOneLimitation(int countContainers, int limitedDimension) {
        ///calculate the needed columns with the algorithm for no constraints. check the constraint afterwards. decrease the columns until it fits.
        for (int neededCollumns = CalcutlateNeededColumnsNoConstraints(countContainers); neededCollumns > 0; neededCollumns--) {
            if (CalculatePlantLength(neededCollumns) <= limitedDimension) {
                return neededCollumns;
            }
        }

        return -1; //also possibility that even one column is too long
    }


    /// <summary>
    /// Calculates the amount of needed collumns based on the container count. The algorithm takes a constraint for the length and for the width into account.
    /// </summary>
    /// <param name="countContainers">The amount of containers that should fit in the plant layout.</param>
    /// <param name="length">The constraint for the length of the plant.</param>
    /// <param name="width">The constraint for the width of the plant.</param>
    /// <param name="securityDistance">The security distance that </param>
    /// <returns>
    /// bool: true if container stacking will be needed, otherwise false<br></br>
    /// int: neededCollumns<br></br>
    /// int: maxContainersPerColumn
    /// </returns>
    private static Tuple<bool, int, int> CalculateNeededColumnsTwoLimitations(int countContainers, int length, int width, double securityDistance) {
        Debug.Log($"constraints: {length}, {width}");
        int neededCollumns = CalculateNeededColumnsOneLimitation(countContainers, length);

        //test, if the width limitation is respected
        int maxContainersPerCollumn = (int)Math.Ceiling(((double)(countContainers) / neededCollumns));
        double neededWidth = CalculatePlantWidth(maxContainersPerCollumn, securityDistance);
        if (neededWidth <= width) {
            //length and width limitation are respected
            Debug.Log($"containerStackingNeeded: false, neededCollumns: {neededCollumns}, maxContainersPerColumn: {maxContainersPerCollumn}, neededHeight: {neededWidth}");
            return new Tuple<bool, int, int>(false, neededCollumns, maxContainersPerCollumn);
        }

        Debug.Log($"containerStackingNeeded: false, neededCollumns: {neededCollumns}, maxContainersPerColumn: {maxContainersPerCollumn}, neededHeight: {neededWidth}");
        //stacking will be needed
        return new Tuple<bool, int, int>(true, neededCollumns, maxContainersPerCollumn);
    }

    /// <summary>
    /// Return the needed plant length for a given amount of collumns.
    /// </summary>
    /// <param name="neededCollumns">The amount of collumns.</param>
    /// <returns>The length of the plant in meters.</returns>
    private static double CalculatePlantLength(int neededCollumns) {
        int countCollumnsWithoutCorridor = neededCollumns / 2;
        int countCollumnsWithCorridor = neededCollumns / 2;
        if (neededCollumns % 2 == 1) countCollumnsWithCorridor++;

        return (lengthRowWithCorridor) * countCollumnsWithCorridor + (lengthRowWithoutCorridor) * countCollumnsWithoutCorridor; ;
    }

    /// <summary>
    /// Calculate the needed plant width for an amount of containers in a column with the use of a certain security distance.
    /// </summary>
    /// <param name="maxContainersPerColumn">The amount of containers that should fit in a column.</param>
    /// <param name="securityDistance">The security distance used between the containers.</param>
    /// <returns>The needed plant width for the collumn count</returns>
    private static double CalculatePlantWidth(int maxContainersPerColumn, double securityDistance) {
        return maxContainersPerColumn * containerWidth + (maxContainersPerColumn - 1) * securityDistance;
    }

    /// <summary>
    /// Algorithm for generating and instantiating a layout.
    /// </summary>
    /// <param name="list">Contains the names and the amount of the components that should be placed.</param>
    private static void GenerateLayout(List<Tuple<ComponentNames, int>> list)
    {
        
        int countContainers = CalculateTotalContainerCount(list,"containerCount");
        int countELContainers = CalculateTotalContainerCount(list,"containerELCount");
        int countWTContainers = CalculateTotalContainerCount(list,"containerWTCount");
        int countGPContainers = CalculateTotalContainerCount(list,"containerGPCount");
        int countCLContainers = CalculateTotalContainerCount(list,"containerCLCount");
        int countPSContainers = CalculateTotalContainerCount(list,"containerPSCount");
        if (countContainers == 0) visualizationWarnings.DisplayWarning("Keine Container können durch die Restriktionen versorgt werden");

        //init values needed in the algorithm
        double securityDistance = CalculateMaxSecurityDistance();
        lengthRowWithoutCorridor = containerLength + securityDistance + lengthPipingStructure;
        lengthRowWithCorridor =  containerLength+containerLength*corridorFactor; //generate enough space to exchange the containers

        //calculate the collumn count or execute a algorithm for generating a layout with stacking of containers
        ReadUserInputPlantDimensions();
        int[] countEntriesPerRow;
        int countAllColumns = -1;
        int countCollumnsWithoutCorridor;
        int countColumnsWithCorridor;
        if (userInputPlantLength == -1 || userInputPlantWidth == -1) {
            if (userInputPlantLength == -1 && userInputPlantWidth == -1)
            {
                //no plant dimension is constrained
                countAllColumns = CalcutlateNeededColumnsNoConstraints(countContainers);
            }
            else if (userInputPlantLength == -1 ^ userInputPlantWidth == -1)
            {
                //one plant dimension is constrained
                int limitedDimension;
                if (userInputPlantLength != -1)
                {
                    limitedDimension = userInputPlantLength;
                }
                else
                {
                    limitedDimension = userInputPlantWidth;
                }

                countAllColumns = CalculateNeededColumnsOneLimitation(countContainers, limitedDimension);
            }           
        }
        else 
        {
            //both dimensions are constrained
            Tuple<bool, int, int> tuple = CalculateNeededColumnsTwoLimitations(countContainers, userInputPlantLength, userInputPlantWidth, securityDistance);
            if (tuple.Item1) {
                //Execute the algorithm for generating a layout with stacking of containers if the containers do not fit in one layer.
                GenerateStackingLayout(countContainers, countELContainers, countPSContainers, countWTContainers, countGPContainers, countCLContainers, list, securityDistance);
                return;
            }
            countAllColumns = tuple.Item2;
        }

        //-------------------------------------------- only for layouts with max one constraint (length) from here on --------------------------------------------

        //split the total column count into columns with and without a maintenance corridor 
        countCollumnsWithoutCorridor = countAllColumns / 2;
        countColumnsWithCorridor = countAllColumns / 2;
        if (countAllColumns % 2 == 1) countColumnsWithCorridor++;
        Debug.Log($"berechnete Columns: {countAllColumns}");

        

        //catch too small length constraint
        if (countAllColumns < 1)
        {
            Debug.Log("There is not enough space for at least one column of containers.");
            visualizationWarnings.DisplayWarning("Die Anlagenlänge reicht nicht aus");
            return;
        }

        //calculate the container distribution for the columns (containers per column)
        countEntriesPerRow = new int[countAllColumns];
        int countColumnsLeft = countAllColumns;
        int tempCountContainers = countContainers;
        int countPipingStructPerRow = 0;
        int countPipingRows = 1;

        for (int i = 0; i < countAllColumns; i++) {
            countEntriesPerRow[i] = (int) Math.Ceiling(((double)(tempCountContainers) / countColumnsLeft));
            countColumnsLeft--;
            tempCountContainers -= countEntriesPerRow[i];
        }


        //calculate the container positions
        var preparedData = new List<Tuple<Pose, string, bool>>();
        var pipingStructures = new List<Pose>(); 

        bool isWithCorridor = true; //current row contains corridor
        double yPosOfColumn = 0;
        //iterate over the columns
        for (int i = 0; i < countAllColumns; i++) {
            double xPosOfContainer = 0;
            bool containerShouldBeRotated = i % 2 == 0;
            countPipingStructPerRow = 0;
            
            if (i % 2 == 0)
            {
                countPipingRows++;
                Debug.Log($"Counted Piping Rows: {countPipingRows}");
            }
           
            //iterate over the container position in a column
            for (int j = 0; j < countEntriesPerRow[i]; j++) {
                //calculate the position of the current container
                Pose pose = new Pose(new Vector3((float)(xPosOfContainer), 0, (float)yPosOfColumn), Quaternion.identity); 
                preparedData.Add(new Tuple<Pose, string, bool>(pose, DeterminNextContainer(list), containerShouldBeRotated));

                if (isWithCorridor && j%2==0)
                {
                    Pose posePipingStructure = new Pose(new Vector3((float)(xPosOfContainer)-(float)securityDistance/2, 0, (float)yPosOfColumn-(float)securityDistance/2), Quaternion.identity);
                    pipingStructures.Add(posePipingStructure);
                    countPipingStructPerRow++;
                    
                }

                else if (i == countAllColumns - 1 && isWithCorridor == false && j % 2 == 0)
                {
                   
                    Pose posePipingStructure = new Pose(new Vector3((float)(xPosOfContainer) - (float)(securityDistance/2), 0, (float)yPosOfColumn + (float)securityDistance / 2 + (float)containerLength +(float)lengthPipingStructure), Quaternion.identity);
                    pipingStructures.Add(posePipingStructure);
                    countPipingStructPerRow++;
                }
                
                xPosOfContainer += containerWidth + securityDistance;
            }
            
            //add the right length of the column to the total length
            if (isWithCorridor){
                //current row contains corridor
                yPosOfColumn += lengthRowWithCorridor;
            }
            else {
                yPosOfColumn += lengthRowWithoutCorridor;
            }
            //alternate columns with and without a maintenance corridor 
            isWithCorridor = !isWithCorridor;
        }

        //calculate the final plant length and with a
        double lengthPlant = (lengthRowWithCorridor) * countColumnsWithCorridor + (lengthRowWithoutCorridor) * countCollumnsWithoutCorridor;
        if (countAllColumns % 2 == 0)
        {
            lengthPlant += lengthPipingStructure;
        }
        double widthPlant = countPipingStructPerRow * widthPipingStructure;
        widthPlant = Math.Max(widthPlant, 0);
        Debug.Log($"Dimensions of the plant: length: {lengthPlant}, width: {widthPlant}");

        //instantiate all calculated objects in the scene
        placingLogic.Place(preparedData);
        placingLogic.PlacePipingStruct(pipingStructures);
        placingLogic.PlaceFoor(lengthPlant ,widthPlant, securityDistance, lengthPipingStructure ,false);



        //set legend text 
        SetLegendText(countContainers, countELContainers, countPSContainers, countWTContainers, countGPContainers, countCLContainers, 0, widthPlant, lengthPlant, 1);
    }

    /// <summary>
    /// Algorithm for generating and instantiating a layout. The layout makes use of stacking.
    /// </summary>
    /// <param name="countContainers">The count of containers that should fit in the layout</param>
    /// <param name="list">A list of container types and counts</param>
    /// <param name="securityDistance">The security distance used between the containers</param>
    private static void GenerateStackingLayout(int countContainers, int countELContainers, int countPSContainers, int countWTContainers, int countGPContainers, int countCLContainers, List<Tuple<ComponentNames, int>> list, double securityDistance) {   
        //init const values that do not exist in the aml file
        const double offsetWidthPerStructure = 8.0;
        const double widthPerStructure = 8.2;
        const double lengthStruct = 16.6;
        const double heightStruct = 4.5;
        //const double widthPipingStruct = 3.0;
        //const double widthStairStruct = 2;

        double plantLength = 0;
        double plantWidth = 0;

        //double securityDistance = (offsetWidthPerStructure - 2 * containerWidth) / 2;
        Debug.Log($"security distance: {securityDistance}");

        //calculate the amount of possible columns and the occupied length
        double widthRowWithoutCorridorAndStruct = lengthStruct + lengthPipingStructure  ;
        double widthRowWithCorridorAndStruct = lengthStruct + corridorFactor * containerLength;
        int possibleColumnCountWithStacking = 0;
        double tempLength = 0;
        while (true) {
            double nextWidth;
            if (possibleColumnCountWithStacking % 2 == 0) {
                nextWidth = widthRowWithCorridorAndStruct;
            }
            
            else {
                nextWidth = widthRowWithoutCorridorAndStruct;
            }


            if (tempLength + nextWidth > userInputPlantLength) {
                plantLength = tempLength;

                if ((possibleColumnCountWithStacking)% 2 == 0){
                    plantLength += lengthPipingStructure;
                }

                break;
            } 
            
            possibleColumnCountWithStacking++;
            tempLength += nextWidth;
        }

        //catch insufficient plant length 
        if (plantLength == 0) {
            Debug.Log("The plant length is not sufficient for one row.");
            visualizationWarnings.DisplayWarning("Die Anlagenlänge reicht nicht aus");
            return;
        }


        //catch insufficient plant width
        if (userInputPlantWidth < widthPerStructure) {
            Debug.Log("Stacking is needed but a struct wont fit into the available space. The width is insufficient.");
            visualizationWarnings.DisplayWarning("Die Anlagenbreite reicht nicht aus");
            return;
        }

        //calculate containers per column per height level
        int countContainersPerColumnLevel = 0;
        const int countContainersPerStruct = 2;
        countContainersPerColumnLevel += countContainersPerStruct;

        while (true) {
            int neededStucts = (countContainersPerColumnLevel + countContainersPerStruct) / 2;
            if (neededStucts * offsetWidthPerStructure > userInputPlantWidth - (widthPerStructure - offsetWidthPerStructure)) break;

            countContainersPerColumnLevel += countContainersPerStruct;
        }

        //calculate the distribution of containers on the columns
        var countContainerPerColumn  = new int[possibleColumnCountWithStacking];
        var tempContainerCount = countContainers;
        while (tempContainerCount > 0) {
            for (int i = 0; i < possibleColumnCountWithStacking; i++){
                if (tempContainerCount < countContainersPerColumnLevel) {
                    countContainerPerColumn[i] += tempContainerCount;
                    tempContainerCount = 0;
                    break;
                }
                countContainerPerColumn[i] += countContainersPerColumnLevel;
                tempContainerCount -= countContainersPerColumnLevel;
            }
        }

        //place the containers and the structs
        var containerData = new List<Tuple<Pose, string, bool>>();
        var initialStructures = new List<Pose>();
        var extensionStructures = new List<Pose>();
        var pipingStructures = new List<Pose>();
        var stairStructures = new List<Pose>();

        int maxCurrentStackingHeight = 0;
        int countPipingStruct = 0;
        double xPos = 0;    
        //iterate over each column
        for (int column = 0; column < possibleColumnCountWithStacking; column++) {
            var tempContainersLeftInColumn = countContainerPerColumn[column];
            double yPos = 0;
            bool rowWithCorridor = column % 2 == 0;
            

            //iterate over every spot on the ground in the current column
            for (int containerStackingSpot = 0; containerStackingSpot < countContainersPerColumnLevel; containerStackingSpot++){
                float spotsLeft = countContainersPerColumnLevel - containerStackingSpot;
                int stackingHeight = (int)Math.Ceiling(tempContainersLeftInColumn / spotsLeft);
                if (stackingHeight > maxCurrentStackingHeight) maxCurrentStackingHeight = stackingHeight;
                tempContainersLeftInColumn -= stackingHeight;
                
                

                //iterate over each height level where a container should be placed
                for (int height = 0; height < stackingHeight; height++)
                {
                    if (containerStackingSpot % 2 == 0 && height > 0)
                    {
                       //spawn structure here
                       Pose pose = new Pose(new Vector3((float)yPos, (float)heightStruct * (height - 1), (float)xPos), Quaternion.identity);
                       if (containerStackingSpot == 0) initialStructures.Add(pose);
                       else extensionStructures.Add(pose);
                    }

                    //spawn Container
                    Vector3 positionContainer = new Vector3((float)((securityDistance / 2) + yPos), (float)(height * heightStruct), (float)(xPos + (lengthStruct - containerLength) / 2));
                    string nextContainerType = DeterminNextContainer(list);
                    if (nextContainerType == "")
                    {
                        Debug.Log("ContainerType could not be determined.");
                    }
                    else
                    {
                        containerData.Add(new Tuple<Pose, string, bool>(new Pose(positionContainer, Quaternion.identity), nextContainerType, rowWithCorridor));
                    }
                }

                //Spawn Piping Structure
                for (int height = 0; height < stackingHeight+1; height++)
                {
                    
                   if (rowWithCorridor==false && column%2!=0 && column!=possibleColumnCountWithStacking-1)
                   {
                        break;
                   }

                   if (containerStackingSpot % 2 == 0 && height > 0)
                   {
                        if (column == possibleColumnCountWithStacking - 1 && rowWithCorridor == false)
                        {
                            Pose poseLastPipingStructure = new Pose(new Vector3((float)yPos, (float)heightStruct * (height - 1), (float)xPos - (float)0.3 + (float)lengthStruct + (float)lengthPipingStructure), Quaternion.identity);
                            pipingStructures.Add(poseLastPipingStructure);
                            
                        }

                        else
                        {
                            Pose posePipingStructure = new Pose(new Vector3((float)yPos, (float)heightStruct * (height - 1), (float)xPos - (float)0.3), Quaternion.identity);
                            pipingStructures.Add(posePipingStructure);
                        }
                            
                   }

                   if(height==0 && column == 0 && containerStackingSpot % 2 == 0)
                   {
                        countPipingStruct++;
                        Debug.Log($"Counbted PipingStrucutes{countPipingStruct}");
                   }
                                       
                }

                //Spawn Stair Structure
                for (int height = 0; height < stackingHeight; height++)
                {
                    if (containerStackingSpot == 0 && height > 0)
                    {
                        Pose poseStairStructure = new Pose(new Vector3((float)yPos+(float)-1.876, (float)heightStruct * (height - 1), (float)xPos + (float)lengthStruct / 2), Quaternion.identity);
                        stairStructures.Add(poseStairStructure);
                    }  
                }
                yPos += securityDistance + containerWidth;
            }

            if (rowWithCorridor) xPos += widthRowWithCorridorAndStruct;
            else xPos += widthRowWithoutCorridorAndStruct;
        }

        //calculate the resulting plant width
        plantWidth = widthPipingStructure * countPipingStruct + stairStructWidth;


        //display a warning if the maximum stacking height exceeds the limit 
        const int maxAllowedStackingHeight = 4;
        if (maxCurrentStackingHeight > maxAllowedStackingHeight) visualizationWarnings.DisplayWarning($"Die Container werden {maxCurrentStackingHeight} mal gestapelt");

        //instantiate all calculated objects in the scene
        placingLogic.Place(containerData);
        placingLogic.PlaceExtensionStruct(extensionStructures);
        placingLogic.PlaceInitialStruct(initialStructures);
        placingLogic.PlacePipingStruct(pipingStructures);
        placingLogic.PlaceStairStruct(stairStructures);
        placingLogic.PlaceFoor(plantLength, plantWidth,securityDistance,lengthPipingStructure, true);
        Debug.Log($"Length: {plantLength}, Width: {plantWidth}");

        //set legend text
        int countHelpStructures = extensionStructures.Count + initialStructures.Count;
      
        SetLegendText(countContainers, countELContainers, countPSContainers, countWTContainers, countGPContainers, countCLContainers, countHelpStructures, plantWidth, plantLength, maxCurrentStackingHeight) ;
    }

    /// <summary>
    /// Calculate the next container that should be placed. <br></br>
    /// ATTENTION: This method modifies the provieded list!
    /// </summary>
    /// <param name="list">A list containing the left containers that should be placed in the scene.</param>
    /// <returns>The Aml name of the next container or "" if no next container could be determined.</returns>
    private static string DeterminNextContainer(List<Tuple<ComponentNames, int>> list) {
        //the order of elements in this array determines the priority of the corresponding container type
        ComponentNames[] priorities = { ComponentNames.WaterSupply, ComponentNames.GasSystem, ComponentNames.CoolingSystem, ComponentNames.PowerSupply, ComponentNames.EnapterSkid, ComponentNames.HoellerSkid, ComponentNames.PlugpowerSkid};

        foreach (var priority in priorities) {
            for(int i = 0; i < list.Count; i++) {
                var listEntry = list[i];
                if (priority == listEntry.Item1) {
                    Tuple<ComponentNames, int> oldTuple = listEntry;
                    if (oldTuple.Item2 <= 0) continue;
                    list.RemoveAt(i);
                    if (oldTuple.Item2 != 1) list.Insert(0, new Tuple<ComponentNames, int>(oldTuple.Item1, oldTuple.Item2 - 1));
                    return amlNames[oldTuple.Item1];
                }
            }
        }
        return "";     
    }

    private static void SetLegendText(int countContainers, int containerELCount, int containerPSCount, int containerWTCount, int containerGPCount, int containerCLCount, int countHelpStructures, double plantWidth, double plantLength, int stackingHeight) {
        legendText.SetCountContainer(countContainers, containerELCount, containerPSCount, containerWTCount, containerGPCount, containerCLCount);
        legendText.SetCountHelpstructures(countHelpStructures);
        legendText.SetPlantLength(plantLength);
        legendText.SetPlantWidth(plantWidth);
        legendText.SetStackingHeight(stackingHeight);
    }

}