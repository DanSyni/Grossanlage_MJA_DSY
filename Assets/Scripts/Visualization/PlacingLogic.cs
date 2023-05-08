using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacingLogic : MonoBehaviour
{
    GameObject containers;
    GameObject structures;
    GameObject hierarchyParent;
    [SerializeField] bool enableTest = false;
    //offsets in order to counteract the falsely set pivots of the models
    private Vector3 containerOffset = new Vector3 ((float)2.48,0,0);
    [SerializeField] private Vector3 structureOffset;           
    private Vector3 rotatedContainerOffset = new Vector3 ((float)0,0,(float)12.3-(float)0.1);

    private void Awake()
    {
        hierarchyParent = new GameObject("PlantElements");
        containers = new GameObject("Containers");
        containers.transform.SetParent(hierarchyParent.transform);
        structures = new GameObject("Structures");
        structures.transform.SetParent(hierarchyParent.transform);
        TooltipSystem.Hide();
    }
    public void Start()
    {
        if (enableTest)
            for(int i = 0; i < 5; i++)
                Place(new Pose(Vector3.zero + (Vector3.right  * i), Quaternion.identity), "WaterSupply", false); 
    }

    /// <summary>
    /// Place a container in the current scene.
    /// </summary>
    /// <param name="pose">The position of the container.</param>
    /// <param name="containerName">The aml name of the container to be placed</param>
    /// <param name="isRotated">Determines if the container should be rotated by 180 degrees</param>
    public void Place(Pose pose, string containerName, bool isRotated) {
        AmlAdapter amlAdapter = AmlAdapter.GetInstance();
        if (amlAdapter == null) { 
            Debug.Log("Placing failed.");
            return;
        }

        GameObject container = amlAdapter.GetColladaGameObject(containerName);
        
        if (container == null) {
            Debug.Log("Container could not be loaded.");
            return;
        }

        if (isRotated)
        {
            GameObject prefab = Instantiate(container, pose.position + rotatedContainerOffset, pose.rotation, containers.transform);
            prefab.transform.Rotate(0, 180, 0);
            Rigidbody currentRb = prefab.AddComponent<Rigidbody>();
            //currentRb.useGravity = false;
            BoxCollider currentBc = prefab.AddComponent<BoxCollider>();
            TooltipTrigger currentSc = prefab.AddComponent<TooltipTrigger>();

            
            
                currentSc.header = "Hallo";
                currentSc.content = "Lukas";

            
        }

        else
        {
            GameObject prefab = Instantiate(container, pose.position + containerOffset, pose.rotation, containers.transform);
            
            Rigidbody currentRb = prefab.AddComponent<Rigidbody>();
            //currentRb.useGravity = false;
            BoxCollider currentBc = prefab.AddComponent<BoxCollider>();
            TooltipTrigger currentSc = prefab.AddComponent<TooltipTrigger>();
            currentSc.header = "Hallo";
            currentSc.content = "Lukas";
        }
        
    }

    /// <summary>
    /// Place all containers specified by the tuples in the list in the current scene.
    /// </summary>
    /// /// <param name="data">A list containing the data for the containers to be placed. <br></br>
    /// Pose: position of the container<br></br>
    /// string: aml name of the container type<br></br>
    /// bool: determines if the container should be rotated by 180 degrees
    /// </param>
    public void Place(List<Tuple<Pose, string, bool>> data) {
        foreach (Tuple<Pose, string, bool> tuple in data) { 
            Place(tuple.Item1, tuple.Item2, tuple.Item3);
        }
    }

    /// <summary>
    /// Place initial structures at the provided poses.
    /// </summary>
    /// <param name="list">The poses of the initial structures</param>
    public void PlaceInitialStruct(List<Pose> list) {
        AmlAdapter amlAdapter = AmlAdapter.GetInstance();
        GameObject initialStructure = amlAdapter.GetColladaGameObject("InitialStructure");
        foreach (var pose in list) {
            GameObject prefab = Instantiate(initialStructure, pose.position + structureOffset, Quaternion.identity, structures.transform);
            prefab.transform.Rotate(0, -90, 0);
            BoxCollider currentBc = prefab.AddComponent<BoxCollider>();
            currentBc.size = new Vector3(16.6f, 0.58f, 8.23f);
            currentBc.center = new Vector3(8f, 4.4f, -3.98f);
        }
    }

    /// <summary>
    /// Place extension structures at the provided poses.
    /// </summary>
    /// <param name="list">The poses of the extension structures</param>
    public void PlaceExtensionStruct(List<Pose> list)
    {
        AmlAdapter amlAdapter = AmlAdapter.GetInstance();
        GameObject extensionStructure = AmlAdapter.GetInstance().GetColladaGameObject("ExtensionStructure");
        foreach (var pose in list) {
            GameObject prefab = Instantiate(extensionStructure, pose.position + structureOffset, Quaternion.identity, structures.transform);
            prefab.transform.Rotate(0, -90, 0);
            BoxCollider currentBc = prefab.AddComponent<BoxCollider>();
            currentBc.size = new Vector3(16.6f, 0.58f, 8.23f);
            currentBc.center = new Vector3(8f, 4.4f, -3.98f);
        }
    }

    /// <summary>
    /// Place piping-structures at the provided poses.
    /// </summary>
    /// <param name="list">The poses of the piping structures</param>
    public void PlacePipingStruct(List<Pose> list)
    {
        AmlAdapter amlAdapter = AmlAdapter.GetInstance();
        GameObject pipingStructure = AmlAdapter.GetInstance().GetColladaGameObject("PipingStructure");
        foreach (var pose in list)
        {
            Instantiate(pipingStructure,pose.position, Quaternion.identity, structures.transform).transform.Rotate(0, 180, 0);
        }
    }

    /// <summary>
    /// Place stair-structures at the provided poses.
    /// </summary>
    /// <param name="list">The poses of the stair structures</param>
    public void PlaceStairStruct(List<Pose> list)
    {
        AmlAdapter amlAdapter = AmlAdapter.GetInstance();
        GameObject stairStructure = AmlAdapter.GetInstance().GetColladaGameObject("StairStructure");
        foreach (var pose in list)
        {
            Instantiate(stairStructure, pose.position, Quaternion.identity, structures.transform).transform.Rotate(0, -90, 0);
        }
    }


    /// <summary>
    /// Instantiate the ground of the plant. <br></br>
    /// The middle is at length / 2 and width / 2. The surface is at y = 0.
    /// </summary>
    /// <param name="length">The length of the ground</param>
    /// <param name="width">The width of the ground</param>
    public void PlaceFoor(double length, double width, double securityDistance, float lengthPipingStruct,bool stacked){
        
        if (length <= 0 || width <= 0) return;

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.SetParent(hierarchyParent.transform);



        //var boxCollider = floor.GetComponent<BoxCollider>();
        //boxCollider.center = new Vector3(500, 100, 500);
        //boxCollider.size = new Vector3(1000, 200, 1000);

        if (stacked == false)
        {
            floor.transform.position = new Vector3((float)(width / 2) - (float)securityDistance/2, -0.5f, (float)(length / 2) - lengthPipingStruct - (float)securityDistance/2);
        }

        else
        {
            floor.transform.position = new Vector3((float)(width/2) - 1.9f, -0.5f, (float)(length / 2) - lengthPipingStruct -(float)0.3);
        }
        floor.transform.localScale = new Vector3((float)width, 1, (float)length);

        Material material = new Material(Shader.Find("HDRP/Lit"));//I'm using the HDRP
        Color c;
        ColorUtility.TryParseHtmlString("#666460", out c);
        material.color = c;
        MeshRenderer renderer = floor.GetComponent<MeshRenderer>();
        renderer.material = material;
    }
}
