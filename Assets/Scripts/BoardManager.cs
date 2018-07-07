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
    public static GameObject BigTextPrefab;

    // Current counters
    public Text DistanceText;
    public Text WeightText;
    public static int distanceTravelledValue;
    public static int weightValue;

    // Coordinate vectors for this trial. ONLY INTEGERS allowed.
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

    public static Color thinLineColor = Color.white;

    // The items for the scene are stored in an array of structs
    private Item[] items;

    // A list to store the unity coordinates of the vertices
    public List<Vector2> unitycoord = new List<Vector2>();

    // A list to store the previous city numbers
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
        BigTextPrefab = (GameObject)Resources.Load("BigTextItem");

        if (GameManager.problemName == 't'.ToString())
        {
            // TSP instance
            SetTSP();
        }
        else if (GameManager.problemName == 'w'.ToString())
        {
            // WCSPP Instance
            SetWCSPP();
        }
        else if (GameManager.problemName == 'm'.ToString())
        {
            // M Instance
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
        cox = GameManager.tspInstances[currInstance].coordinatesx;
        coy = GameManager.tspInstances[currInstance].coordinatesy;
        unitycoord = BoardFunctions.CoordinateConvertor(cox, coy);

        cities = GameManager.tspInstances[currInstance].cities;
        distances = GameManager.tspInstances[currInstance].distancematrix;

        // Number of objects
        int objectCount = coy.Length;

        // Store objects in a list
        items = new Item[objectCount];
        for (int i = 0; i < objectCount; i++)
        {
            Item itemToLocate = GenerateItem(i, unitycoord[i]);
            items[i] = itemToLocate;
        }
    }

    void SetWCSPP()
    {
        Debug.Log("Setting up WCSPP Instance: Block " + (GameManager.block + 1) + "/" + GameManager.numberOfBlocks + ", Trial " + GameManager.trial + "/" + GameManager.numberOfTrials + " , Total Trial " + GameManager.TotalTrial);
        currInstance = GameManager.wcsppRandomization[GameManager.TotalTrial - 1];

        // Display Max distance
        Text quest = GameObject.Find("Question").GetComponent<Text>();
        quest.text = "Max weight: " + GameManager.wcsppInstances[currInstance].maxweight + "kg";

        // Display current distance
        DistanceText = GameObject.Find("DistanceText").GetComponent<Text>();
        DistanceText.color = Color.green;

        // Display current weight
        WeightText = GameObject.Find("WeightText").GetComponent<Text>();
        WeightText.color = Color.red;

        // Display reset button
        Reset = GameObject.Find("Reset").GetComponent<Button>();
        Reset.onClick.AddListener(ResetClicked);

        // Coordinate of the cities
        cox = GameManager.wcsppInstances[currInstance].coordinatesx;
        coy = GameManager.wcsppInstances[currInstance].coordinatesy;
        unitycoord = BoardFunctions.CoordinateConvertor(cox, coy);

        cities = GameManager.wcsppInstances[currInstance].cities;
        distances = GameManager.wcsppInstances[currInstance].distancematrix;
        weights = GameManager.wcsppInstances[currInstance].weightmatrix;

        // Number of objects
        int objectCount = coy.Length;

        // Store objects in a list
        items = new Item[objectCount];
        for (int i = 0; i < objectCount; i++)
        {
            for (int j = 0; j < objectCount; j++)
            {
                if (distances[i, j] != 0)
                {
                    DrawSlimLine(i, j);
                }
            }
        }

        for (int i = 0; i < objectCount; i++)
        {
            Item itemToLocate = GenerateItem(i, unitycoord[i]);
            items[i] = itemToLocate;
        }
    }

    void SetM()
    {
        Debug.Log("Setting up MVC Instance: Block " + (GameManager.block + 1) + "/" + GameManager.numberOfBlocks + ", Trial " + GameManager.trial + "/" + GameManager.numberOfTrials + " , Total Trial " + GameManager.TotalTrial);
    }

    // Instantiates an Item and places it on the position from the input
    Item GenerateItem(int itemNumber, Vector2 itemPosition)
    {
        GameObject instance = Instantiate(CityItemPrefab, itemPosition, Quaternion.identity) as GameObject;
        if (GameManager.problemName == 'w'.ToString() && itemNumber == GameManager.wcsppInstances[currInstance].startcity)
        {
            instance = Instantiate(StartCityPrefab, itemPosition, Quaternion.identity) as GameObject;
        }
        else if (GameManager.problemName == 'w'.ToString() && itemNumber == GameManager.wcsppInstances[currInstance].endcity)
        {
            instance = Instantiate(EndCityPrefab, itemPosition, Quaternion.identity) as GameObject;
        }

        canvas = GameObject.Find("Canvas");
        instance.transform.SetParent(canvas.GetComponent<Transform>(), false);

        Item itemInstance;
        itemInstance.gameItem = instance;
        itemInstance.CityButton = itemInstance.gameItem.GetComponent<Button>();
        itemInstance.CityNumber = cities[itemNumber];
        itemInstance.center = itemPosition;

        // Setting the position in a separate line is importatant in order to set it according to global coordinates.
        BoardFunctions.PlaceItem(itemInstance, itemPosition);

        return itemInstance;
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

    // If clicking on the first city, light it up. after that, clicking on a city will fill the destination city,
    // indicating you've travelled to it, and draw a connecting line between the city of departure and the destination
    public void ClickOnItem(Item itemToLocate)
    {
        if (GameManager.problemName == 't'.ToString())
        {
            TSPclick(itemToLocate);
        }

        if (GameManager.problemName == 'w'.ToString())
        {
            if (ClickValid(itemToLocate))
            {
                WCSPPclick(itemToLocate);
            }
        }

        if (GameManager.problemName == 'm'.ToString())
        {
            Mclick(itemToLocate);
        }
    }

    void TSPclick(Item itemToLocate)
    {
        // If the city is NEW or is the ORIGIN city
        if (!previouscities.Contains(itemToLocate.CityNumber) || (previouscities.Count() == cities.Length && previouscities.First() == itemToLocate.CityNumber))
        {
            if (previouscities.Count() == 0)
            {
                LightFirstCity(itemToLocate);
            }
            else
            {
                DrawLine(itemToLocate);
            }

            AddCity(itemToLocate);

            SetDistanceText();
        }
        else if (previouscities.Last() == itemToLocate.CityNumber)
        {
            EraseLine(itemToLocate);
        }
    }

    bool ClickValid(Item itemToLocate)
    {
        if (previouscities.Count() == 0)
        {   
            // Allow the click if the this is the first city and is the Start City
            return itemToLocate.CityNumber == GameManager.wcsppInstances[currInstance].startcity;
        }
        else if (previouscities.Last() == itemToLocate.CityNumber && previouscities.Count() != 1)
        {
            // Allow the click if the user is trying to cancel the last click, unless user is trying to unclick start city
            EraseLine(itemToLocate);
            return false;
        }
        else if (previouscities.Last() == GameManager.wcsppInstances[currInstance].endcity)
        {
            // Disallow further clicks if the last city has already been clicked and the user is not trying to cancel the last click
            return false;
        }
        else if (distances[previouscities.Last(), itemToLocate.CityNumber] != 0)
        {
            // In any other case...
            // First check if the two distance between the two cities is non zero
            // Then check if the current weight is less than the max weight
            if ((Weight() + weights[previouscities.Last(), itemToLocate.CityNumber]) > GameManager.wcsppInstances[currInstance].maxweight)
            {
                //StartCoroutine(ShowMessage());

                GameObject heavyText = Instantiate(BigTextPrefab, new Vector2(0, -480), Quaternion.identity) as GameObject;
                //Destroy(heavyText, 2);
                //yield return new WaitForSeconds(2f);
                //heavyText.SetActive(false);
                heavyText.transform.SetParent(canvas.GetComponent<Transform>(), false);
                Destroy(heavyText, 2);
                Debug.Log("here");
                return false;
            }

            return true;
        }

        return false;
    }

    void WCSPPclick(Item itemToLocate)
    {
        if (previouscities.Count() == 0)
        {
            // If clicking the Start city
            AddCity(itemToLocate);

            SetDistanceText();
        }
        else if (!previouscities.Contains(itemToLocate.CityNumber))
        {      
            // If the Start city has already been clicked
            DrawLine(itemToLocate);

            AddCity(itemToLocate);

            SetDistanceText();
        }
    }

    void Mclick(Item itemToLocate)
    {
        if (!previouscities.Contains(itemToLocate.CityNumber) || (previouscities.Count() == cities.Length && previouscities.First() == itemToLocate.CityNumber))
        {
            if (previouscities.Count() == 0)
            {
                LightFirstCity(itemToLocate);
            }
            else
            {
                DrawLine(itemToLocate);
            }

            AddCity(itemToLocate);

            SetDistanceText();
        }
        else if (previouscities.Last() == itemToLocate.CityNumber)
        {
            EraseLine(itemToLocate);
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
    void AddCity(Item itemToLocate)
    {
        previouscities.Add(itemToLocate.CityNumber);
        citiesvisited = previouscities.Count();

        Click newclick;
        newclick.CityNumber = itemToLocate.CityNumber;
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
    private void LightFirstCity(Item itemToLocate)
    {
        Light myLight = itemToLocate.gameItem.GetComponent<Light>();
        myLight.enabled = true;
    }

    void DrawLine(Item itemToLocate)
    {
        int cityofdestination = itemToLocate.CityNumber;
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

    void DrawSlimLine(int cityofdeparture, int cityofdestination)
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
        instance.GetComponent<LineRenderer>().material.color = thinLineColor;
        instance.GetComponent<LineRenderer>().sortingOrder = 0;
        instance.GetComponent<LineRenderer>().SetPositions(coordinates);

        int wt = weights[cityofdeparture, cityofdestination];
        if (wt != 0)
        {
            GameObject weight = Instantiate(TextPrefab, new Vector2(0, 0), Quaternion.identity) as GameObject;
            weight.transform.SetParent(canvas.GetComponent<Transform>(), false);
            weight.transform.position = ((coordestination + coordeparture) / 2) - new Vector2(0.31f, 0);
            weight.GetComponent<Text>().text = "W:" + wt.ToString();
            weight.GetComponent<Text>().color = new Color(1f, 1f - (wt / 1000f), 0f);

            GameObject distance = Instantiate(TextPrefab, new Vector2(0, 0), Quaternion.identity) as GameObject;
            distance.transform.SetParent(canvas.GetComponent<Transform>(), false);
            distance.transform.position = ((coordestination + coordeparture) / 2) + new Vector2(0.31f, 0);
            distance.GetComponent<Text>().text = "D:" + distances[cityofdeparture, cityofdestination].ToString();
            distance.GetComponent<Text>().color = new Color(0f, 1f, 1f - (distances[cityofdeparture, cityofdestination] / 1500f));
        }
    }

    // If double click on the previous city then change the destination city back to vacant, and delete the connecting line between the two cities
    void EraseLine(Item itemToLocate)
    {
        if (previouscities.Count == 1 && GameManager.problemName != 'w'.ToString())
        {
            itemToLocate.gameItem.GetComponent<Light>().enabled = false;
        }

        Destroy(lines[citiesvisited - 1]);
        previouscities.RemoveAt(previouscities.Count() - 1);
        citiesvisited--;
        SetDistanceText();

        // Save the click
        Click newclick;
        newclick.CityNumber = itemToLocate.CityNumber;
        newclick.State = 0;
        newclick.time = GameManager.timeQuestion - GameManager.tiempo;
        itemClicks.Add(newclick);
    }

    // Turn off the light for the first city
    private void Lightoff()
    {
        foreach (Item item in items)
        {
            if (item.CityNumber == previouscities[0] && GameManager.problemName != 'w'.ToString())
            {
                Light myLight =
                    item.gameItem.GetComponent<Light>();
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
