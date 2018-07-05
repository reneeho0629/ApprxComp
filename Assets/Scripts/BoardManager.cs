using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// This script (a component of GameManager) initializes the board (i.e. screen).
public class BoardManager : MonoBehaviour
{
    // Create a canvas where all the items are going to be placed
    private GameObject canvas;

    // Prefab of the Item interface configuration
    public static GameObject CityItemPrefab;
    public static GameObject StartCityPrefab;
    public static GameObject EndCityPrefab;
    public static GameObject LineItemPrefab;
    public static GameObject TextPrefab;

    // Current counters
    public Text DistanceText;
    public Text WeightText;
    public static int distanceTravelledValue;
    public static int weightValue;

    //Coordinate vectors for this trial. ONLY INTEGERS allowed.
    private float[] cox;
    private float[] coy;
    private int[] cities;
    private int[,] distances;
    private int[,] weights;

    // Should the key be working? Initially disabled
    public static bool keysON = false;

    // Reset button
    public Button Reset;

    // Struct with the relevant parameters of an Item.
    // gameItem is the game object; 
    // centre is the location of the vertex; 
    // CityNumber is the number/index of the vertex; 
    // CityButton is a clickable button.
    public struct Item
    {
        public GameObject gameItem;
        public Vector2 center;
        public int CityNumber;
        public Button CityButton;
    }

    // The Items for the scene are stored in an array of structs
    private Item[] Items;

    // A list to store the unity coordinates of the vertices
    public List<Vector2> unitycoord = new List<Vector2>();

    //  A list to store the previous city numbers
    public List<int> previouscities = new List<int>();

    // The list of all the button clicks. Each event contains the following information:
    // ItemNumber (a number between 1 and the number of items.)
    // Item is being selected In=1; Out=0 
    // Time of the click with respect to the beginning of the trial 
    public static List<Click> itemClicks = new List<Click>();

    public struct Click
    {
        // Citynumber (100=Reset). State: In(1)/Out(0)/Reset(3). Time in seconds
        public int CityNumber;
        public int State;
        public float time;
    }

    // Used to draw connecting lines
    public GameObject[] lines = new GameObject[100];
    public LineRenderer[] newLine = new LineRenderer[100];

    // To keep track of the number of cities visited
    public int citiesvisited = 0;

    // Current Instance number
    public int currInstance;

    // Initializes the instance for this trial:
    // 1. Sets the question string using the instance (from the .txt files)
    // 2. The weight and value vectors are uploaded
    // 3. The instance prefab is uploaded
    void SetInstance()
    {
        CityItemPrefab = (GameObject)Resources.Load("CityItem");
        StartCityPrefab = (GameObject)Resources.Load("StartCityItem");
        EndCityPrefab = (GameObject)Resources.Load("EndCityItem");
        LineItemPrefab = (GameObject)Resources.Load("LineItem");
        TextPrefab = (GameObject)Resources.Load("TextItem");

        //  TSP instance
        if (GameManager.problemName == 't'.ToString())
        {
            SetTSP();
        }
        // WCSPP Instance
        else if (GameManager.problemName == 'w'.ToString())
        {
            SetWCSPP();
        }
        // M Instance
        else if (GameManager.problemName == 'm'.ToString())
        {
            SetM();
        }
    }


    void SetTSP()
    {
        Debug.Log("Setting up TSP Instance: Block " + (GameManager.block + 1) + "/" + GameManager.numberOfBlocks + ", Trial " + GameManager.trial + "/" + GameManager.numberOfTrials + " , Total Trial " + GameManager.TotalTrial);
        // current instance
        currInstance = GameManager.tspRandomization[GameManager.TotalTrial - 1];

        // Display current distance
        DistanceText = GameObject.Find("DistanceText").GetComponent<Text>();

        // Display reset button
        Reset = GameObject.Find("Reset").GetComponent<Button>();
        Reset.onClick.AddListener(ResetClicked);

        // Coordinate of the cities
        cox = GameManager.tsp_instances[currInstance].coordinatesx;
        coy = GameManager.tsp_instances[currInstance].coordinatesy;
        unitycoord = BoardFunctions.CoordinateConvertor(cox, coy);

        cities = GameManager.tsp_instances[currInstance].cities;
        distances = GameManager.tsp_instances[currInstance].distancematrix;

        // Number of objects
        int objectCount = coy.Length;

        // Store objects in a list
        Items = new Item[objectCount];
        for (int i = 0; i < objectCount; i++)
        {
            Item ItemToLocate = GenerateItem(i, unitycoord[i]);
            Items[i] = ItemToLocate;
        }
    }
    void SetWCSPP()
    {
        Debug.Log("Setting up WCSPP Instance: Block " + (GameManager.block + 1) + "/" + GameManager.numberOfBlocks + ", Trial " + GameManager.trial + "/" + GameManager.numberOfTrials + " , Total Trial " + GameManager.TotalTrial);
        currInstance = GameManager.wcsppRandomization[GameManager.TotalTrial - 1];

        // Display Max distance
        Text Quest = GameObject.Find("Question").GetComponent<Text>();
        Quest.text = "Max weight: " + GameManager.wcspp_instances[currInstance].maxweight + "kg";

        // Display current distance
        DistanceText = GameObject.Find("DistanceText").GetComponent<Text>();

        // Display current weight
        WeightText = GameObject.Find("WeightText").GetComponent<Text>();

        // Display reset button
        Reset = GameObject.Find("Reset").GetComponent<Button>();
        Reset.onClick.AddListener(ResetClicked);

        // Coordinate of the cities
        cox = GameManager.wcspp_instances[currInstance].coordinatesx;
        coy = GameManager.wcspp_instances[currInstance].coordinatesy;
        unitycoord = BoardFunctions.CoordinateConvertor(cox, coy);

        cities = GameManager.wcspp_instances[currInstance].cities;
        distances = GameManager.wcspp_instances[currInstance].distancematrix;
        weights = GameManager.wcspp_instances[currInstance].weightmatrix;

        // Number of objects
        int objectCount = coy.Length;

        // Store objects in a list
        Items = new Item[objectCount];
        for (int i = 0; i < objectCount; i++)
        {
            Item ItemToLocate = GenerateItem(i, unitycoord[i]);
            Items[i] = ItemToLocate;
            for (int j = 0; j < GameManager.wcspp_instances[currInstance].ncities; j++)
            {
                DrawSlimLine(i, j);

            }
        }
    }

    void SetM()
    {
        Debug.Log("Setting up MVC Instance: Block " + (GameManager.block + 1) + "/" + GameManager.numberOfBlocks + ", Trial " + GameManager.trial + "/" + GameManager.numberOfTrials + " , Total Trial " + GameManager.TotalTrial);

    }

    // Instantiates an Item and places it on the position from the input
    Item GenerateItem(int ItemNumber, Vector2 itemPosition)
    {
        GameObject instance = Instantiate(CityItemPrefab, itemPosition, Quaternion.identity) as GameObject;
        if (GameManager.problemName == 'w'.ToString() && ItemNumber == GameManager.wcspp_instances[currInstance].startcity)
        {
            instance = Instantiate(StartCityPrefab, itemPosition, Quaternion.identity) as GameObject;
        }
        else if (GameManager.problemName == 'w'.ToString() && ItemNumber == GameManager.wcspp_instances[currInstance].endcity)
        {
            instance = Instantiate(EndCityPrefab, itemPosition, Quaternion.identity) as GameObject;
        }

        canvas = GameObject.Find("Canvas");
        instance.transform.SetParent(canvas.GetComponent<Transform>(), false);

        Item ItemInstance;
        ItemInstance.gameItem = instance;
        ItemInstance.CityButton = ItemInstance.gameItem.GetComponent<Button>();
        ItemInstance.CityNumber = cities[ItemNumber];
        ItemInstance.center = itemPosition;

        //Setting the position in a separate line is importatant in order to set it according to global coordinates.
        BoardFunctions.PlaceItem(ItemInstance, itemPosition);

        return (ItemInstance);
    }

    // Macro function that initializes the Board
    public void SetupTrial()
    {
        previouscities.Clear();
        itemClicks.Clear();
        GameManager.Distancetravelled = 0;
        distanceTravelledValue = 0;
        SetInstance();

        keysON = true;
    }

    // Sets the triggers for pressing the corresponding keys
    // Perhaps a good practice thing to do would be to create a "close scene" function that takes as parameter the answer and closes everything (including keysON=false) and then forwards to 
    // changeToNextScene(answer) on game manager
    private void SetKeyInput()
    {
        if (GameManager.escena == "Trial")
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                InputOutputManager.SaveTimeStamp("ParticipantSkip");
                GameManager.ChangeToNextScene(itemClicks, true);
            }
        }
        else if (GameManager.escena == "SetUp")
        {

            if (Input.GetKeyDown(KeyCode.Space))
            {
                GameFunctions.SetTimeStamp();
                GameManager.ChangeToNextScene(itemClicks, false);
            }
        }
        else
        {
            Debug.Log("Skipped to next scene... I wonder why");
            GameManager.ChangeToNextScene(itemClicks, false);
        }
    }

    //if clicking on the first city, light it up. after that, clicking on a city will fill the destination city, indicating you've travelled to it, and draw a
    //connecting line between the city of departure and the destination
    public void ClickOnItem(Item ItemToLocate)
    {
        if (GameManager.problemName == 't'.ToString())
        {
            TSPclick(ItemToLocate);
        }

        if (GameManager.problemName == 'w'.ToString())
        {
            if (ClickValid(ItemToLocate))
            {
                WCSPPclick(ItemToLocate);
            }

        }

        if (GameManager.problemName == 'm'.ToString())
        {
            Mclick(ItemToLocate);
        }
    }

    void TSPclick(Item ItemToLocate)
    {
        // If the city is NEW or is the ORIGIN city
        if (!previouscities.Contains(ItemToLocate.CityNumber) || (previouscities.Count() == cities.Length && previouscities.First() == ItemToLocate.CityNumber))
        {
            if (previouscities.Count() == 0)
            {
                LightFirstCity(ItemToLocate);
            }
            else
            {
                DrawLine(ItemToLocate);
            }

            AddCity(ItemToLocate);

            SetDistanceText();
        }
        else if (previouscities.Last() == ItemToLocate.CityNumber)
        {
            EraseLine(ItemToLocate);
        }
    }

    bool ClickValid(Item ItemToLocate)
    {
        //distances[GameManager.wcspp_instances[currInstance].startcity, GameManager.wcspp_instances[currInstance].endcity] = 0;
        // Allow the click if the this is the first city and is the Start City
        if (previouscities.Count() == 0)
        {
            return ItemToLocate.CityNumber == GameManager.wcspp_instances[currInstance].startcity;
        }
        // Allow the click if the user is trying to cancel the last click, unless user is trying to unclick start city
        else if (previouscities.Last() == ItemToLocate.CityNumber && previouscities.Count() != 1)
        {
            EraseLine(ItemToLocate);
            return false;
        }
        // Disallow further clicks if the last city has already been clicked and the user is not trying to cancel the last click
        else if (previouscities.Last() == GameManager.wcspp_instances[currInstance].endcity)
        {
            return false;
        }
        // In any other case...
        // First check if the two distance between the two cities is non zero
        else if (distances[previouscities.Last(), ItemToLocate.CityNumber] != 0)
        {
            // Then check if the current weight is less than the max weight
            if ((Weight() + weights[previouscities.Last(), ItemToLocate.CityNumber]) > GameManager.wcspp_instances[currInstance].maxweight)
            {
                return false;
            }
            return true;
        }
        return false;
    }

    void WCSPPclick(Item ItemToLocate)
    {
        Debug.Log(previouscities.Count());
        // If clicking the Start city
        if (previouscities.Count() == 0)
        {
            //Debug.Log(ItemToLocate.CityNumber + "and" + currInstance + "also"+ GameManager.wcspp_instances[currInstance].startcity);
            AddCity(ItemToLocate);

            SetDistanceText();
        }
        // If the Start city has already been clicked
        else if (!previouscities.Contains(ItemToLocate.CityNumber))
        {
            DrawLine(ItemToLocate);

            AddCity(ItemToLocate);

            SetDistanceText();
        }
    }

    void Mclick(Item ItemToLocate)
    {
        if (!previouscities.Contains(ItemToLocate.CityNumber) || (previouscities.Count() == cities.Length && previouscities.First() == ItemToLocate.CityNumber))
        {
            if (previouscities.Count() == 0)
            {
                LightFirstCity(ItemToLocate);
            }
            else
            {
                DrawLine(ItemToLocate);
            }
            AddCity(ItemToLocate);

            SetDistanceText();
        }
        else if (previouscities.Last() == ItemToLocate.CityNumber)
        {
            EraseLine(ItemToLocate);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (keysON)
        {
            SetKeyInput();
        }
    }

    // Add current city to previous cities
    void AddCity(Item ItemToLocate)
    {
        previouscities.Add(ItemToLocate.CityNumber);
        citiesvisited = previouscities.Count();

        Click newclick;
        newclick.CityNumber = ItemToLocate.CityNumber;
        newclick.State = 1;
        newclick.time = GameManager.timeQuestion - GameManager.tiempo;
        itemClicks.Add(newclick);
    }

    public int DistanceTravelled()
    {
        int[] individualdistances = new int[previouscities.Count()];
        if (previouscities.Count() > 1)
        {
            for (int i = 0; i < (previouscities.Count() - 1); i++)
            {
                individualdistances[i] = distances[previouscities[i], previouscities[i + 1]];
            }
        }

        int distancetravelled = individualdistances.Sum();
        distanceTravelledValue = distancetravelled;
        return distancetravelled;
    }

    public int Weight()
    {
        int[] individualweights = new int[previouscities.Count()];
        if (previouscities.Count() > 1)
        {
            for (int i = 0; i < (previouscities.Count() - 1); i++)
            {
                individualweights[i] = weights[previouscities[i], previouscities[i + 1]];
            }
        }

        int totalweight = individualweights.Sum();
        weightValue = totalweight;
        return weightValue;
    }

    void SetDistanceText()
    {
        int distanceT = DistanceTravelled();
        DistanceText.text = "Distance so far: " + distanceT.ToString() + "km";

        if (GameManager.problemName == 'w'.ToString())
        {
            int weightT = Weight();
            WeightText.text = "Weight so far: " + weightT.ToString() + "kg";
        }

    }

    // Turn the light on around the first city to be clicked on
    private void LightFirstCity(Item ItemToLocate)
    {
        Light myLight = ItemToLocate.gameItem.GetComponent<Light>();
        myLight.enabled = true;
    }

    void DrawLine(Item ItemToLocate)
    {
        int cityofdestination = ItemToLocate.CityNumber;
        int cityofdeparture = previouscities[previouscities.Count() - 1];

        Vector2 coordestination = unitycoord[cityofdestination];
        Vector2 coordeparture = unitycoord[cityofdeparture];

        Vector3[] coordinates = new Vector3[2];
        coordinates[0] = coordestination;
        coordinates[1] = coordeparture;

        GameObject instance = Instantiate(LineItemPrefab, new Vector2(0, 0), Quaternion.identity) as GameObject;

        canvas = GameObject.Find("Canvas");
        instance.transform.SetParent(canvas.GetComponent<Transform>(), false);
        instance.GetComponent<LineRenderer>().sortingOrder = 1;
        lines[citiesvisited] = instance;
        newLine[citiesvisited] = lines[citiesvisited].GetComponent<LineRenderer>();
        newLine[citiesvisited].SetPositions(coordinates);
    }

    void DrawSlimLine(int cityofdestination, int cityofdeparture)
    {
        Vector2 coordestination = unitycoord[cityofdestination];
        Vector2 coordeparture = unitycoord[cityofdeparture];

        Vector3[] coordinates = new Vector3[2];
        coordinates[0] = coordestination;
        coordinates[1] = coordeparture;

        GameObject instance = Instantiate(LineItemPrefab, new Vector2(0, 0), Quaternion.identity) as GameObject;
        canvas = GameObject.Find("Canvas");
        instance.transform.SetParent(canvas.GetComponent<Transform>(), false);
        instance.GetComponent<LineRenderer>().startWidth = 0.01f;
        instance.GetComponent<LineRenderer>().endWidth = 0.01f;
        instance.GetComponent<LineRenderer>().material.color = Color.white;
        instance.GetComponent<LineRenderer>().sortingOrder = 0;
        instance.GetComponent<LineRenderer>().SetPositions(coordinates);

        int wt = weights[cityofdeparture, cityofdestination];
        if (wt != 0)
        {
            GameObject info = Instantiate(TextPrefab, new Vector2(0, 0), Quaternion.identity) as GameObject;
            info.transform.SetParent(canvas.GetComponent<Transform>(), false);
            info.transform.position = (coordestination + coordeparture) / 2;
            info.GetComponent<Text>().text = wt.ToString();
        }

    }

    // If double click on the previous city then change the destination city back to vacant, and delete the connecting line between the two cities
    void EraseLine(Item ItemToLocate)
    {
        if (previouscities.Count == 1 && GameManager.problemName != 'w'.ToString())
        {
            ItemToLocate.gameItem.GetComponent<Light>().enabled = false;
        }

        Destroy(lines[citiesvisited - 1]);
        previouscities.RemoveAt(previouscities.Count() - 1);
        citiesvisited--;
        SetDistanceText();

        // Save the click
        Click newclick;
        newclick.CityNumber = ItemToLocate.CityNumber;
        newclick.State = 0;
        newclick.time = GameManager.timeQuestion - GameManager.tiempo;
        itemClicks.Add(newclick);
    }

    // Turn off the light for the first city
    private void Lightoff()
    {
        foreach (Item Item in Items)
        {
            if (Item.CityNumber == previouscities[0] && GameManager.problemName != 'w'.ToString())
            {
                Light myLight =
                    Item.gameItem.GetComponent<Light>();
                myLight.enabled = false;
            }
        }
    }


    // Resets everything
    public void ResetClicked()
    {
        if (previouscities.Count() != 0)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                DestroyObject(lines[i]);
            }
            Lightoff();
            previouscities.Clear();
            SetDistanceText();
            citiesvisited = 0;

            Click newclick;
            newclick.CityNumber = 100;
            newclick.State = 3;
            newclick.time = GameManager.timeQuestion - GameManager.tiempo;
            itemClicks.Add(newclick);
        }
    }
}
