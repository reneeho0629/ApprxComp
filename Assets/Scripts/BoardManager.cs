using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// This script (a component of GameManager) initializes the board (i.e. screen).
public class BoardManager : MonoBehaviour {


	// Create a canvas where all the board is going to be placed
	private GameObject canvas;

	// Prefab of the Item interface configuration
	public static GameObject TSPItemPrefab;

	// Prefab of the Item interface configuration
	public static GameObject LineItemPrefab;

	// Current distance counter
	public Text DistanceText;
	public static int distanceTravelledValue;

	//Coordinate vectors for this trial. ONLY INTEGERS allowed.
	private float[] cox;
	private float[] coy;
	private int[] cities;
	private int[,] distances;

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
	public List<Vector2> unitycoord = new List<Vector2> ();

	//  A list to store the previous city numbers
	public List<int> previouscities = new List<int> ();

	// The list of all the button clicks. Each event contains the following information:
	// ItemNumber (a number between 1 and the number of items.)
	// Item is being selected In=1; Out=0 
	// Time of the click with respect to the beginning of the trial 
	public static List <Vector3> itemClicks =  new List<Vector3> ();

    // Used to draw connecting lines
	public GameObject[] lines= new GameObject[100];
	public LineRenderer[] newLine= new LineRenderer[100];
    
    // To keep track of the number of cities visited
	public int citiesvisited = 0;
		
	// Initializes the instance for this trial:
	// 1. Sets the question string using the instance (from the .txt files)
	// 2. The weight and value vectors are uploaded
	// 3. The instance prefab is uploaded
	void SetInstance()
	{
		int randInstance = GameManager.instanceRandomization[GameManager.TotalTrial-1];

		//Display Max distance
		Text Quest = GameObject.Find("Question").GetComponent<Text>();
		Quest.text = "Max: " + GameManager.game_instances[randInstance].maxdistance + "km";
        DistanceText = GameObject.Find ("DistanceText").GetComponent<Text>();
		Reset = GameObject.Find("Reset").GetComponent<Button>();
		Reset.onClick.AddListener(ResetClicked);

		cox = GameManager.game_instances [randInstance].coordinatesx;
		coy = GameManager.game_instances [randInstance].coordinatesy;
		unitycoord = BoardFunctions.CoordinateConvertor(cox,coy);

		cities = GameManager.game_instances [randInstance].cities;
		distances = GameManager.game_instances [randInstance].distancematrix;

		TSPItemPrefab = (GameObject)Resources.Load ("TSPItem");
		LineItemPrefab = (GameObject)Resources.Load ("LineButton");

		int objectCount =coy.Length;
		Items = new Item[objectCount];
		for(int i=0; i < objectCount;i=i+1)
		{
			//int objectPositioned = 0;
			Item ItemToLocate = GenerateItem (i, unitycoord[i]);//66: Change to different Layer?
			Items[i] = ItemToLocate;
		}
	}


	// Instantiates an Item and places it on the position from the input
	Item GenerateItem(int ItemNumber ,Vector2 randomPosition)
	{
		GameObject instance = Instantiate (TSPItemPrefab, randomPosition, Quaternion.identity) as GameObject;

		canvas=GameObject.Find("Canvas");
		instance.transform.SetParent (canvas.GetComponent<Transform> (),false);

		Item ItemInstance;
		ItemInstance.gameItem = instance;
		ItemInstance.CityButton = ItemInstance.gameItem.GetComponent<Button> ();
		ItemInstance.CityNumber = cities[ItemNumber];
		ItemInstance.center = randomPosition;

		//Setting the position in a separate line is importatant in order to set it according to global coordinates.
		BoardFunctions.PlaceItem (ItemInstance,randomPosition);

		return(ItemInstance);
	}
    
	// Macro function that initializes the Board
	public void SetupTrial()
	{
		previouscities.Clear();
		itemClicks.Clear ();
		GameManager.Distancetravelled = 0;
		distanceTravelledValue = 0;
		SetInstance ();

		keysON = true;
	}
		
	// Sets the triggers for pressing the corresponding keys
	// Perhaps a good practice thing to do would be to create a "close scene" function that takes as parameter the answer and closes everything (including keysON=false) and then forwards to 
	// changeToNextScene(answer) on game manager
	private void SetKeyInput(){
		if (GameManager.escena == "Trial") {
			if (Input.GetKeyDown (KeyCode.UpArrow)) {
				InputOutputManager.SaveTimeStamp ("ParticipantSkip");
				GameManager.ChangeToNextScene (itemClicks, 1);
			}
		} else if (GameManager.escena == "SetUp") {

            if (Input.GetKeyDown (KeyCode.Space)) {
				GameFunctions.SetTimeStamp ();
				GameManager.ChangeToNextScene (itemClicks, 0);
			}
		} else
        {
            GameManager.ChangeToNextScene(itemClicks, 0);
        }
	}

	//if clicking on the first city, light it up. after that, clicking on a city will fill the destination city, indicating you've travelled to it, and draw a
	//connecting line between the city of departure and the destination
	public void ClickOnItem(Item ItemToLocate)
	{
		if (!previouscities.Contains (ItemToLocate.CityNumber) || (previouscities.Count () == cities.Length && previouscities.First () == ItemToLocate.CityNumber)) {
			if (CityFirst (previouscities.Count ())) {
				LightFirstCity (ItemToLocate);
			} else {
				DrawLine (ItemToLocate);
			}
			AddCity (ItemToLocate);
			itemClicks.Add (new Vector3 (ItemToLocate.CityNumber, GameManager.timeQuestion - GameManager.tiempo,1));
			SetDistanceText ();
		} else if (previouscities.Last () == ItemToLocate.CityNumber) {
			EraseLine (ItemToLocate);
			itemClicks.Add (new Vector3 (ItemToLocate.CityNumber, GameManager.timeQuestion - GameManager.tiempo,0));
		}
	}

	// Use this for initialization
	void Start () 
	{

	}

	// Update is called once per frame
	void Update () 
	{
		if (keysON) 
		{
			SetKeyInput ();
		}
	}

	// Add current city to previous cities
	void AddCity(Item ItemToLocate)
	{
		previouscities.Add (ItemToLocate.CityNumber);
		citiesvisited = previouscities.Count ();
	}

	public int DistanceTravelled()
	{
		int[] individualdistances = new int[previouscities.Count()];
		if (previouscities.Count() < 2) {
		} else {
			for (int i = 0; i < (previouscities.Count ()-1); i++) {
				individualdistances [i] = distances [previouscities[i], previouscities[i+1]];
			}
		}

		int distancetravelled = individualdistances.Sum ();
		distanceTravelledValue = distancetravelled;
		return distancetravelled;
	}
		
	void SetDistanceText ()
	{
		int distanceT = DistanceTravelled();
		DistanceText.text = "Distance so far: " + distanceT.ToString () + "km";
	}
		
	//determining whether the city is the first one to have been clicked in that instance i.e. where is the starting point
	bool CityFirst(int citiesvisited)
	{
        return citiesvisited == 0;
	}

	//turn the light on around the first city to be clicked on
	private void LightFirstCity(Item ItemToLocate)
	{
		Light myLight = ItemToLocate.gameItem.GetComponent<Light> ();
		myLight.enabled = !myLight.enabled;
	}
		
	void DrawLine(Item ItemToLocate) 
	{
		int cityofdestination = ItemToLocate.CityNumber;
		int cityofdeparture = previouscities[previouscities.Count()-1];

		Vector2 coordestination = unitycoord [cityofdestination];
		Vector2 coordeparture = unitycoord [cityofdeparture];

		Vector3[] coordinates = new Vector3[2];
		coordinates [0] = coordestination;
		coordinates [1] = coordeparture;
			
		GameObject instance = Instantiate (LineItemPrefab, new Vector2(0,0), Quaternion.identity) as GameObject;

		canvas=GameObject.Find("Canvas");
		instance.transform.SetParent (canvas.GetComponent<Transform> (),false);

		lines[citiesvisited] = instance;
		newLine[citiesvisited] = lines[citiesvisited].GetComponent<LineRenderer> ();
		newLine[citiesvisited].SetPositions(coordinates);
	}

	// If double click on the previous city then change the destination city back to vacant, and delete the connecting line between the two cities
	void EraseLine(Item ItemToLocate)
	{
		if (previouscities.Count == 1) {
			ItemToLocate.gameItem.GetComponent<Light> ().enabled = false;
		}
		Destroy (lines[citiesvisited-1]);
		previouscities.RemoveAt (previouscities.Count () - 1);
		citiesvisited --;
		SetDistanceText ();

	}

    // Turn off the light for the first city
	private void Lightoff(){
		foreach(Item Item1 in Items){
			if (Item1.CityNumber == previouscities[0]){
				Light myLight = Item1.gameItem.GetComponent<Light> ();
				myLight.enabled = false;
			}
		}
	}


    // Resets everything
    public void ResetClicked(){
        if (previouscities.Count() != 0) {
            for (int i = 0; i < lines.Length; i++)
            {
                DestroyObject(lines[i]);
            }
            Lightoff();
            previouscities.Clear();
            SetDistanceText();
            citiesvisited = 0;
            itemClicks.Add(new Vector3(100, GameManager.timeQuestion - GameManager.tiempo, 3));
        }
	}
}
