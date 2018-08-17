using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// This script (a component of GameManager) initializes the board (i.e. screen).
public class BoardManager : MonoBehaviour
{
    // Create a canvas where all the items are going to be placed
    public static GameObject canvas;

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

    // Coordinate vectors for this trial. ONLY INTEGERS allowed.
    public static float[] cox;
    public static float[] coy;
    public static int ncities;
    public static int[,] distances;
    public static int[,] weights;
    public static int solution;

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
    public static List<Vector2> unitycoord = new List<Vector2>();

    // A list to store the previous city numbers
    public static List<int> previouscities = new List<int>();

    // The list of all the button clicks. Each event contains the following information:
    // ItemNumber (a number between 1 and the number of items.)
    // Item is being selected In=1; Out=0 
    // Time of the click with respect to the beginning of the trial 
    public static List<Click> itemClicks = new List<Click>();

    public struct Click
    {
        // Citynumber (citynumber or 100=Reset). State: In(1)/Out(0)/Invalid(2)/Reset(3). Time in seconds
        public int CityNumber;
        public int State;
        public float time;
    }

    // Used to draw connecting lines
    public GameObject[] lines = new GameObject[100];
    public LineRenderer[] newLine = new LineRenderer[100];

    // To keep track of the number of cities visited
    public static int citiesvisited = 0;

    // Current Instance number
    public static int currInstance;

    // Macro function that initializes the Board
    public void SetupTrial()
    {
        previouscities.Clear();
        itemClicks.Clear();
        GameManager.Distancetravelled = 0;
        GameManager.weightValue = 0;
        GameManager.timedOut = 1;
        citiesvisited = 0;
        SetInstance();

        keysON = true;
    }

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

        // Display reset button
        Reset = GameObject.Find("Reset").GetComponent<Button>();
        Reset.onClick.AddListener(ResetClicked);

        if (GameManager.problemName == 't'.ToString())
        {
            // TSP instance
            Debug.Log("Setting up Random TSP Instance: Block " + (GameManager.block + 1) + "/" + GameManager.numberOfBlocks + ", Trial " + GameManager.trial + "/" + GameManager.numberOfTrials + " , Total Trial " + GameManager.TotalTrial + " , Current Instance " + (GameManager.tspRandomization[GameManager.TotalTrial - 1] + 1));

            // current instance
            currInstance = GameManager.tspRandomization[GameManager.TotalTrial - 1];
            SetTSP(GameManager.tspInstances[currInstance]);
        }
        else if (GameManager.problemName == 'w'.ToString())
        {
            // WCSPP Instance
            Debug.Log("Setting up WCSPP Instance: Block " + (GameManager.block + 1) + "/" + GameManager.numberOfBlocks + ", Trial " + GameManager.trial + "/" + GameManager.numberOfTrials + " , Total Trial " + GameManager.TotalTrial + " , Current Instance " + (GameManager.wcsppRandomization[GameManager.TotalTrial - 1] + 1));

            // current instance
            currInstance = GameManager.wcsppRandomization[GameManager.TotalTrial - 1];
            SetWCSPP(GameManager.wcsppInstances[currInstance]);
        }
        else if (GameManager.problemName == 'm'.ToString())
        {
            // Metric TSP Instance
            Debug.Log("Setting up Euclidean TSP Instance: Block " + (GameManager.block + 1) + "/" + GameManager.numberOfBlocks + ", Trial " + GameManager.trial + "/" + GameManager.numberOfTrials + " , Total Trial " + GameManager.TotalTrial + " , Current Instance " + (GameManager.mtspRandomization[GameManager.TotalTrial - 1] + 1));

            // current instance
            currInstance = GameManager.mtspRandomization[GameManager.TotalTrial - 1];
            SetTSP(GameManager.mtspInstances[currInstance]);
        }

        SetDistanceText();
    }

    // Funciton to set up regular & Metric TSP instance
    void SetTSP(GameManager.TSPInstance currentInstance)
    {
        // Display current distance
        DistanceText = GameObject.Find("DistanceText").GetComponent<Text>();

        // Coordinate of the cities
        cox = currentInstance.coordinatesx;
        coy = currentInstance.coordinatesy;
        unitycoord = BoardFunctions.CoordinateConvertor(cox, coy);
        
        // Number of objects
        ncities = currentInstance.ncities;
        distances = currentInstance.distancematrix;

        solution = currentInstance.solution;

        // Store objects in a list
        items = new Item[ncities];
        for (int i = 0; i < ncities; i++)
        {
            for (int j = i; j < ncities; j++)
            {
                if (distances[i, j] != 0)
                {
                    DrawSlimLine(i, j, 0.01f);
                }
            }
        }

        for (int i = 0; i < ncities; i++)
        {
            items[i] = GenerateItem(i, unitycoord[i]);
        }
    }

    // Funciton to set up WCSPP instance
    void SetWCSPP(GameManager.WCSPPInstance currentInstance)
    {
        // Display Max distance
        Text quest = GameObject.Find("Question").GetComponent<Text>();
        quest.text = "Max cost: $" + GameManager.wcsppInstances[currInstance].maxweight;

        // Display current distance
        DistanceText = GameObject.Find("DistanceText").GetComponent<Text>();
        DistanceText.color = Color.yellow;

        // Display current weight
        WeightText = GameObject.Find("WeightText").GetComponent<Text>();
        WeightText.color = Color.green;

        // Coordinate of the cities
        cox = currentInstance.coordinatesx;
        coy = currentInstance.coordinatesy;
        unitycoord = BoardFunctions.CoordinateConvertor(cox, coy);

        // Number of objects
        ncities = currentInstance.ncities;
        distances = currentInstance.distancematrix;
        weights = currentInstance.weightmatrix;

        solution = currentInstance.solution;
        
        // Store objects in a list
        items = new Item[ncities];
        for (int i = 0; i < ncities; i++)
        {
            for (int j = i; j < ncities; j++)
            {
                if (distances[i, j] != 0)
                {
                    DrawSlimLine(i, j, 0.01f);
                }
            }
        }

        for (int i = 0; i < ncities; i++)
        {
            items[i] = GenerateItem(i, unitycoord[i]);
        }
    }

    // Instantiates an Item and places it on the position from the input
    Item GenerateItem(int itemNumber, Vector2 itemPosition)
    {
        GameObject instance = Instantiate(CityItemPrefab, itemPosition, Quaternion.identity) as GameObject;
        if (GameManager.problemName == 'w'.ToString() && itemNumber == GameManager.wcsppInstances[currInstance].startcity)
        {
            //Debug.Log(GameManager.wcsppInstances[currInstance].startcity);
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
        itemInstance.CityNumber = itemNumber;
        itemInstance.center = itemPosition;

        // Setting the position in a separate line is importatant in order to set it according to global coordinates.
        BoardFunctions.PlaceItem(itemInstance, itemPosition);

        return itemInstance;
    }

    // Sets the triggers for pressing the corresponding keys
    // Perhaps a good practice thing to do would be to create a "close scene" function that takes as parameter the answer and closes everything (including keysON=false) and then forwards to 
    // changeToNextScene(answer) on game manager
    private void SetKeyInput()
    {
        if (GameManager.escena == "Trial")
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && SubmissionValid(false))
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

    // Used to determine if the participant's solution follows all rules
    public static bool SubmissionValid(bool suppress)
    {
        if (GameManager.problemName == 'w'.ToString())
        {
            // Last city clicked is the destination
            if (previouscities.Count() != 0 && previouscities.Last() == GameManager.wcsppInstances[currInstance].endcity)
            {
                GameManager.timedOut = 0;
                return true;
            }
            if (!suppress)
            {
                GameObject invalidText = Instantiate(BigTextPrefab, new Vector2(0, -480), Quaternion.identity) as GameObject;
                invalidText.transform.SetParent(canvas.GetComponent<Transform>(), false);
                invalidText.GetComponent<Text>().text = "Your have not reached the destination";
                Destroy(invalidText, 2);
            }
        }
        else if (GameManager.problemName == 't'.ToString())
        {
            // all cities have been selected
            if (citiesvisited == GameManager.tspInstances[currInstance].ncities + 1)
            {
                GameManager.timedOut = 0;
                return true;
            }
            if (!suppress)
            {
                GameObject notAll = Instantiate(BigTextPrefab, new Vector2(0, -480), Quaternion.identity) as GameObject;
                notAll.transform.SetParent(canvas.GetComponent<Transform>(), false);
                notAll.GetComponent<Text>().text = "You have not selected every city in this problem";
                Destroy(notAll, 2);
            }
        }
        else if (GameManager.problemName == 'm'.ToString())
        {
            // all cities have been selected
            if (citiesvisited == GameManager.mtspInstances[currInstance].ncities + 1)
            {
                GameManager.timedOut = 0;
                return true;
            }
            if (!suppress)
            {
                GameObject notAll = Instantiate(BigTextPrefab, new Vector2(0, -480), Quaternion.identity) as GameObject;
                notAll.transform.SetParent(canvas.GetComponent<Transform>(), false);
                notAll.GetComponent<Text>().text = "You have not selected every city in this problem";
                Destroy(notAll, 2);
            }
        }

        return false;
    }

    // If clicking on the first city, light it up. after that, clicking on a city will fill the destination city,
    // indicating you've travelled to it, and draw a connecting line between the city of departure and the destination
    public void ClickOnItem(Item itemToLocate)
    {
        if (GameManager.problemName == 'w'.ToString())
        {
            if (ClickValid(itemToLocate))
            {
                WCSPPclick(itemToLocate);
            }
        }
        else if (GameManager.problemName == 't'.ToString() || GameManager.problemName == 'm'.ToString())
        {
            TSPclick(itemToLocate);
        }
    }

    // Funciton to determine if a TSP click is valid and draw connecting lines
    void TSPclick(Item itemToLocate)
    {
        if (previouscities.Count() != 0 && previouscities.Last() == itemToLocate.CityNumber)
        {
            // If deselecting a city
            EraseLine(itemToLocate);
        }
        else if (previouscities.Count() == ncities && previouscities.First() == itemToLocate.CityNumber)
        {
            // Allow the last click of the start city to complete the circuit
            DrawLine(itemToLocate);

            AddCity(itemToLocate);

            SetDistanceText();
        }
        else if (previouscities.Contains(itemToLocate.CityNumber))
        {
            // Disallow clicks on cities already clicked
            AddInvalid(itemToLocate, 3);
            GameObject selectedText = Instantiate(BigTextPrefab, new Vector2(0, -480), Quaternion.identity) as GameObject;
            selectedText.transform.SetParent(canvas.GetComponent<Transform>(), false);
            selectedText.GetComponent<Text>().text = "You have already selected this city";
            Destroy(selectedText, 2);
        }
        else
        {
            // If the city is new (not already selected)
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
    }

    // Funciton to determine if a WCSPP click is valid
    bool ClickValid(Item itemToLocate)
    {
        if (previouscities.Count() == 0)
        {
            // Allow the click if the this is the first city and is the Start City
            if (itemToLocate.CityNumber != GameManager.wcsppInstances[currInstance].startcity)
            {
                AddInvalid(itemToLocate, 4);
                GameObject notStartText = Instantiate(BigTextPrefab, new Vector2(0, -480), Quaternion.identity) as GameObject;
                notStartText.transform.SetParent(canvas.GetComponent<Transform>(), false);
                notStartText.GetComponent<Text>().text = "You must click the 'Start' city first";
                Destroy(notStartText, 2);
            }

            return itemToLocate.CityNumber == GameManager.wcsppInstances[currInstance].startcity;
        }
        else if (previouscities.Last() == itemToLocate.CityNumber && previouscities.Count() != 1)
        {
            // Allow the click if the user is trying to cancel the last click, unless user is trying to unclick start city
            EraseLine(itemToLocate);
            return false;
        }
        else if (previouscities.Last() == itemToLocate.CityNumber && previouscities.Count() == 1)
        {
            // Disallow the click if the user is trying to unclick start city
            AddInvalid(itemToLocate, 5);
            GameObject unclickFirstText = Instantiate(BigTextPrefab, new Vector2(0, -480), Quaternion.identity) as GameObject;
            unclickFirstText.transform.SetParent(canvas.GetComponent<Transform>(), false);
            unclickFirstText.GetComponent<Text>().text = "You have already selected the 'Start' city";
            Destroy(unclickFirstText, 2);
            return false;
        }
        else if (previouscities.Last() == GameManager.wcsppInstances[currInstance].endcity)
        {
            // Disallow further clicks if the last city has already been clicked and the user is not trying to cancel the last click
            AddInvalid(itemToLocate, 6);
            GameObject finishedText = Instantiate(BigTextPrefab, new Vector2(0, -480), Quaternion.identity) as GameObject;
            finishedText.transform.SetParent(canvas.GetComponent<Transform>(), false);
            finishedText.GetComponent<Text>().text = "You have already reached the destination";
            Destroy(finishedText, 2);
            return false;
        }
        else if (previouscities.Contains(itemToLocate.CityNumber))
        {
            // Disallow clicks on cities already clicked
            AddInvalid(itemToLocate, 7);
            GameObject selectedText = Instantiate(BigTextPrefab, new Vector2(0, -480), Quaternion.identity) as GameObject;
            selectedText.transform.SetParent(canvas.GetComponent<Transform>(), false);
            selectedText.GetComponent<Text>().text = "You have already selected this city";
            Destroy(selectedText, 2);
            return false;
        }
        else if (distances[previouscities.Last(), itemToLocate.CityNumber] != 0)
        {
            // In any other case... check if the current weight is less than the max weight
            if ((GameManager.weightValue + weights[previouscities.Last(), itemToLocate.CityNumber]) > GameManager.wcsppInstances[currInstance].maxweight)
            {
                AddInvalid(itemToLocate, 8);
                GameObject heavyText = Instantiate(BigTextPrefab, new Vector2(0, -480), Quaternion.identity) as GameObject;
                heavyText.transform.SetParent(canvas.GetComponent<Transform>(), false);
                heavyText.GetComponent<Text>().text = "This path is too expensive";
                Destroy(heavyText, 2);
                return false;
            }

            return true;
        }
        else if (distances[previouscities.Last(), itemToLocate.CityNumber] == 0)
        {
            AddInvalid(itemToLocate, 9);
            GameObject noPath = Instantiate(BigTextPrefab, new Vector2(0, -480), Quaternion.identity) as GameObject;
            noPath.transform.SetParent(canvas.GetComponent<Transform>(), false);
            noPath.GetComponent<Text>().text = "Invalid path, these two cities cannot be connected directly";
            Destroy(noPath, 2);
            return false;
        }

        AddInvalid(itemToLocate, 10);
        GameObject invalidText = Instantiate(BigTextPrefab, new Vector2(0, -480), Quaternion.identity) as GameObject;
        invalidText.transform.SetParent(canvas.GetComponent<Transform>(), false);
        invalidText.GetComponent<Text>().text = "Invalid click, ask the researcher if you are unsure";
        Destroy(invalidText, 2);
        return false;
    }

    // Adds invalid clicks to the list of clicks
    void AddInvalid(Item itemToLocate, int reasonCode)
    {
        /* Here are the reasons why a click might be invalid....
         * TSP:
         * 3=have already selected this city
         * 
         * WCSPP:
         * 4=must click the 'Start' city first
         * 5=have already selected the 'Start' city
         * 6=have already reached the destination
         * 7=have already selected this city
         * 8=Weight Limit Exceeded
         * 9=Invalid path, the two cities cannot be connected directly
         * 10=BEWARE: this should not occur... any other error is a 10.
         */
        Click newclick;
        newclick.CityNumber = itemToLocate.CityNumber;
        newclick.State = reasonCode;
        newclick.time = GameManager.timeQuestion - GameManager.tiempo;
        itemClicks.Add(newclick);
    }

    // Funciton to connect WCSPP cities and run relevant functions
    void WCSPPclick(Item itemToLocate)
    {
        if (previouscities.Count() == 0)
        {
            // If clicking the Start city
            AddCity(itemToLocate);

            SetDistanceText();
        }
        else
        {
            // If the Start city has already been clicked
            DrawLine(itemToLocate);

            AddCity(itemToLocate);

            SetDistanceText();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (keysON)
        {
            SetKeyInput();
        }
        if (SubmissionValid(true))
        {
            GameManager.timedOut = 2;
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

    // Function to calculate total distance thus far
    public void CalcDistance()
    {
        int[] individualdistances = new int[previouscities.Count()];
        if (previouscities.Count() > 1)
        {
            for (int i = 0; i < (previouscities.Count() - 1); i++)
            {
                individualdistances[i] = distances[previouscities[i], previouscities[i + 1]];
            }
        }

        GameManager.Distancetravelled = individualdistances.Sum();
    }

    // Function to calculate total WCSPP weight thus far
    public void CalcWeight()
    {
        int[] individualweights = new int[previouscities.Count()];
        if (previouscities.Count() > 1)
        {
            for (int i = 0; i < (previouscities.Count() - 1); i++)
            {
                individualweights[i] = weights[previouscities[i], previouscities[i + 1]];
            }
        }

        GameManager.weightValue = individualweights.Sum();
    }

    // Function to display distance and weight in Unity
    void SetDistanceText()
    {
        CalcDistance();

        if (GameManager.problemName == 'w'.ToString() || GameManager.problemName == 'm'.ToString())
        {
            DistanceText.text = "Time so far: " + GameManager.Distancetravelled.ToString() + " Minutes";
        }
        else
        {
            DistanceText.text = "Time so far: " + GameManager.Distancetravelled.ToString() + " Minutes";
        }

        if (GameManager.problemName == 'w'.ToString())
        {
            CalcWeight();
            WeightText.text = "Cost so far: $" + GameManager.weightValue.ToString();
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
        instance.GetComponent<LineRenderer>().startWidth = 0.06f;
        instance.GetComponent<LineRenderer>().endWidth = 0.06f;
        instance.GetComponent<LineRenderer>().sortingOrder = 1;
        lines[citiesvisited] = instance;
        newLine[citiesvisited] = lines[citiesvisited].GetComponent<LineRenderer>();
        newLine[citiesvisited].SetPositions(coordinates);
    }

    // Function to draw slim lines in WCSPP instances (to represent the valid connections) and to display distance & weight information
    public static void DrawSlimLine(int cityofdeparture, int cityofdestination, float linewidth)
    {
        Vector2 coordestination = unitycoord[cityofdestination];
        Vector2 coordeparture = unitycoord[cityofdeparture];

        Vector3[] coordinates = new Vector3[2];
        coordinates[0] = coordestination;
        coordinates[1] = coordeparture;

        GameObject instance = Instantiate(LineItemPrefab, new Vector2(0, 0), Quaternion.identity) as GameObject;
        canvas = GameObject.Find("Canvas");
        instance.transform.SetParent(canvas.GetComponent<Transform>(), false);
        instance.GetComponent<LineRenderer>().startWidth = linewidth;
        instance.GetComponent<LineRenderer>().endWidth = linewidth;
        instance.GetComponent<LineRenderer>().material.color = thinLineColor;
        instance.GetComponent<LineRenderer>().sortingOrder = 0;
        instance.GetComponent<LineRenderer>().SetPositions(coordinates);

        if (GameManager.problemName == 't'.ToString() || GameManager.problemName == 'm'.ToString())
        {
            // TSP instance
            int dt = distances[cityofdeparture, cityofdestination];
            GameObject distance = Instantiate(TextPrefab, new Vector2(0, 0), Quaternion.identity) as GameObject;
            distance.transform.SetParent(canvas.GetComponent<Transform>(), false);
            distance.transform.position = ((coordestination + coordeparture) / 2);
            if (GameManager.problemName == 't'.ToString())
            {
                distance.GetComponent<Text>().text = "T:" + dt.ToString();
                if (dt > 500)
                {
                    distance.GetComponent<Text>().color = new Color(1f, 1f - (dt - 500) / 500f, 0f);
                }
                else
                {
                    distance.GetComponent<Text>().color = new Color((dt - 100) / 400f, 1f, 0f);
                }
            }
            else
            {
                distance.GetComponent<Text>().text = "T:" + dt.ToString();
                if (dt > 500)
                {
                    distance.GetComponent<Text>().color = new Color(1f, 1f - (dt - 500) / 500f, 0f);
                }
                else
                {
                    distance.GetComponent<Text>().color = new Color((dt - 100) / 400f, 1f, 0f);
                }
            }
        }
        else if (GameManager.problemName == 'w'.ToString())
        {
            /*
            // WCSPP Instance
            int wt = weights[cityofdeparture, cityofdestination];
            GameObject weight = Instantiate(TextPrefab, new Vector2(0, 0), Quaternion.identity) as GameObject;
            weight.transform.SetParent(canvas.GetComponent<Transform>(), false);
            weight.transform.position = ((coordestination + coordeparture) / 2);
            weight.GetComponent<Text>().text = "$" + wt.ToString();
            if (wt > 50)
            {
                weight.GetComponent<Text>().color = new Color(1f, 1f - (wt - 50) / 50f, 0f);
            }
            else
            {
                weight.GetComponent<Text>().color = new Color((wt - 10) / 40f, 1f, 0f);
            }*/
            
            // WCSPP Instance
            int wt = weights[cityofdeparture, cityofdestination];
            GameObject weight = Instantiate(TextPrefab, new Vector2(0, 0), Quaternion.identity) as GameObject;
            weight.transform.SetParent(canvas.GetComponent<Transform>(), false);
            weight.transform.position = ((coordestination + coordeparture) / 2) - new Vector2(0.23f, 0);
            weight.GetComponent<Text>().text = "$" + wt.ToString();
            weight.GetComponent<Text>().color = Color.green;

            int dt = distances[cityofdeparture, cityofdestination];
            GameObject distance = Instantiate(TextPrefab, new Vector2(0, 0), Quaternion.identity) as GameObject;
            distance.transform.SetParent(canvas.GetComponent<Transform>(), false);
            distance.transform.position = ((coordestination + coordeparture) / 2) + new Vector2(0.23f, 0);
            distance.GetComponent<Text>().text = "T:" + dt.ToString();
            distance.GetComponent<Text>().color = Color.yellow;
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
                Destroy(lines[i]);
            }

            Lightoff();
            previouscities.Clear();
            SetDistanceText();
            citiesvisited = 0;

            Click newclick;
            newclick.CityNumber = 100;
            newclick.State = 2;
            newclick.time = GameManager.timeQuestion - GameManager.tiempo;
            itemClicks.Add(newclick);
        }
    }
}
