/* Unity 3D program that displays multiple interactive instances of the WCSPP, Minimum Vertex Cover and TSP. 
 * This program aims to test human performance in problems of varying computational complexity.
 * 
 * Input files are stored in ./StreamingAssets/Input
 * User responses and other data are stored in ./StreamingAssets/Output
 * 
 * Run program in 1024 x 768 after build
 * 
 * Based on Knapsack and TSP code written by Pablo Franco and Karlo Doroc
 * Modified by Anthony Hsu for his Finance Honours research project 2018.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Linq; 
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class GameManager : MonoBehaviour 
{

	// Game Manager: It is a singleton (i.e. it is always one and the same it is nor destroyed nor duplicated)	
	public static GameManager gameManager=null;

	// The reference to the script managing the board (interface/canvas).
	public BoardManager boardScript;

    // Variables related to scene name and time
	// Current Scene
	public static string escena;
	// Time remaining on this scene
	public static float tiempo;
	// Total time for this scene
	public static float totalTime;
	// Time spent at the instance
	public static float timeSkip;

	// Current trial number in the current block
	public static int trial = 0;
	// The current trial number across all blocks
	public static int TotalTrial = 0;
	// Current block initialization
	public static int block = 0;
    // Current problem initialization
    public static int problem = 0;

    // True/False show timer or not
    private static bool showTimer;

	// Rest time between scenes. 1, 2, 3 are for interTrial, interBlock and interProblem respectively.
	public static float timeRest1;
	public static float timeRest2;
    public static float timeRest3;

    // Time given for each instance
    public static float timeQuestion;

	// Total number of trials in each block
	public static int numberOfTrials;
    // Total number of blocks
    public static int numberOfBlocks;

    // Skip button in case user do not want a break
    public static Button skipButton;

    //Number of instance files to be considered. From i1.txt to i24.txt.
    public static int numberOfInstances;

    // Total number of Problems
    private static int numberOfProblems = 3;
    // Current problem number: 0, 1, 2.
    private static int currentProblem = 0;
    // The order of the Problems to be presented
    public static List<string> problemOrder;
    // Name (a char) of the current problem
    public static string problemName;
	
    // The order of the Instances to be presented
	public static int[] tspRandomization;
    public static int[] wcsppRandomization;
    public static int[] mRandomization;
    
    // distance travelled
    public static int Distancetravelled;

    // An array of all the instances, i.e importing everything using the structure below 
    public static TSPInstance[] game_instances;

    // A struct that contains the parameters of each TSP instance
    public struct TSPInstance
	{
		public int[] cities;
		public float[] coordinatesx;
		public float[] coordinatesy;
		public int[,] distancematrix;
		public int[] distancevector;

		public int ncities;
		public int maxdistance;

		public string id;
		public string type;
		public string param;
	}



	// Use this for initialization
	void Start () 
	{
		//Makes the GameManager a Singleton
		if (gameManager == null) {
			gameManager = this;
		} else if (gameManager != this) {
			Destroy (gameObject);
		}

		DontDestroyOnLoad (gameObject);
        //Debug.Log("running Start");
		//Initializes the game
		boardScript = gameManager.GetComponent<BoardManager> ();
		InitGame();

		if (escena != "SetUp") 
		{
			InputOutputManager.SaveTimeStamp(escena);
		}
	}

	//Initializes the scene. One scene is setup, other is trial, other is Break....
	void InitGame()
	{
		/*
		Scene Order: escena
		0= setup
		1= trial game
		2= intertrial rest
		3= interblock rest
        4= interproblem rest
		5= end
		*/
		// Selects the active scene, and call it "escena" - that's Spanish for "scene".
		Scene scene = SceneManager.GetActiveScene();
        escena = scene.name;

        // The loop which runs the game, and drives you from one scene to another
        // If it's the first scene, upload parameters and instances (this happens only once), move incrememntally through >blocks< 1 at a time
        if (escena == "SetUp") 
		{
			block++;
			GameFunctions.SetupInitialScreen ();
		} 
		else if (escena == "Trial") 
		{
			trial++;
			TotalTrial++;
			showTimer = true;
			boardScript.SetupTrial();

			tiempo = timeQuestion;
			totalTime = tiempo;
		} 
		else if (escena == "InterTrialRest") 
		{
			showTimer = false;
			tiempo = timeRest1;
			totalTime = tiempo;
		} 
		else if (escena == "InterBlockRest") 
		{
			trial = 0;
			block++;
			showTimer = true;
			tiempo = timeRest2;
			totalTime = tiempo;
            skipButton = GameObject.Find("Skip").GetComponent<Button>();
            skipButton.onClick.AddListener(SkipClicked);
        }
        else if (escena == "InterProblemRest")
        {
            trial = 0;
            TotalTrial = 0;
            block = 0;
            showTimer = true;

            totalTime = tiempo = timeRest3;

            currentProblem++;
            problemName = problemOrder[currentProblem - 1];

            Text Quest = GameObject.Find("ProblemName").GetComponent<Text>();
            if (problemName == 't'.ToString())
            {
                Quest.text = "In the next phase: TSP";
            } 
            else if (problemName == 'w'.ToString())
            {
                Quest.text = "In the next phase: WCSPP";
            }
            else if (problemName == 'm'.ToString())
            {
                Quest.text = "In the next phase: MVC";
            }

            skipButton = GameObject.Find("Skip").GetComponent<Button>();
            skipButton.onClick.AddListener(SkipClicked);
        }

	}


	// Update is called once per frame
	void Update () 
	{
		if ((escena != "SetUp") && (escena != "end"))
        {
			StartTimer ();
			GameFunctions.PauseManager ();
		}
    }
    
	// Assigns the parameters in the dictionary to variables
	public static void AssignVariables(Dictionary<string,string> dictionary)
	{
		//Assigns Parameters - these are all going to be imported from input files
		timeRest1 = Convert.ToSingle (dictionary["timeRest1"]);
		timeRest2 = Convert.ToSingle (dictionary["timeRest2"]);
        timeRest3 = Convert.ToSingle(dictionary["timeRest3"]);
        timeQuestion = Int32.Parse(dictionary["timeQuestion"]);
		numberOfTrials = Int32.Parse(dictionary["numberOfTrials"]);
		numberOfBlocks = Int32.Parse(dictionary["numberOfBlocks"]);
        numberOfInstances = numberOfTrials * numberOfBlocks;

        // Getting TSP randomisation parameters. Code is extremely inefficient, future users should try to improve.
        int[] tspRandomizationNo0 = Array.ConvertAll(dictionary["tspRandomization"].Substring(1, dictionary["tspRandomization"].Length - 2).Split(','), int.Parse);
        tspRandomization = new int[tspRandomizationNo0.Length];

        for (int i = 0; i < tspRandomizationNo0.Length; i++)
        {
            tspRandomization[i] = tspRandomizationNo0[i] - 1;
        }

        // Getting WCSPP randomisation parameters
        int[] wcsppRandomizationNo0 = Array.ConvertAll(dictionary["wcsppRandomization"].Substring(1, dictionary["wcsppRandomization"].Length - 2).Split(','), int.Parse);
        wcsppRandomization = new int[wcsppRandomizationNo0.Length];

        for (int i = 0; i < wcsppRandomizationNo0.Length; i++)
        {
            wcsppRandomization[i] = wcsppRandomizationNo0[i] - 1;
        }

        // Getting m randomisation parameters
        int[] mRandomizationNo0 = Array.ConvertAll(dictionary["mRandomization"].Substring(1, dictionary["mRandomization"].Length - 2).Split(','), int.Parse);
        mRandomization = new int[mRandomizationNo0.Length];

        for (int i = 0; i < mRandomizationNo0.Length; i++)
        {
            mRandomization[i] = mRandomizationNo0[i] - 1;
        }

        problemOrder = dictionary["problemOrder"].Substring(1, dictionary["problemOrder"].Length - 2).Split(',').ToList();

        for (int i = 0; i < problemOrder.Count; i++)
        {
            problemOrder[i] = problemOrder[i].Replace("\'", "");
        }

        numberOfProblems = problemOrder.Count;
    }

    // Takes care of changing the Scene to the next one (Except for when in the setup scene)
    public static void ChangeToNextScene(List <Vector3> itemClicks, int skipped)
	{
        BoardManager.keysON = false;
        //Debug.Log(escena);
		if (escena == "SetUp")
        {
			InputOutputManager.LoadGame ();
            //Debug.Log("SetUp - Current Problem: " + currentProblem + " problemName: " + problemOrder[currentProblem]);
            problemName = problemOrder[currentProblem];
            SceneManager.LoadScene ("InterProblemRest");

        }
        else if (escena == "Trial")
        {
			Distancetravelled = BoardManager.distanceTravelledValue;

			if (skipped == 1) {
				timeSkip = timeQuestion - tiempo;
			} else {
				timeSkip = timeQuestion;
			}

            string itemsSelected = ExtractItemsSelected(itemClicks);
            InputOutputManager.Save(itemsSelected, timeSkip, "");

            InputOutputManager.SaveTimeStamp("ParticipantAnswer");

            InputOutputManager.SaveClicks(itemClicks);
            SceneManager.LoadScene("InterTrialRest");
        }
        else if (escena == "InterTrialRest")
        {
            ChangeToNextTrial ();
		}
        else if (escena == "InterBlockRest")
        {
			SceneManager.LoadScene ("Trial");
		}
        else if (escena == "InterProblemRest")
        {
            Debug.Log("InterProblem - Current Problem: "+ currentProblem+" problemName: "+ problemOrder[currentProblem-1]);
           
            ChangeToNextTrial();
        }
    }

	// Redirects to the next scene depending if the trials or blocks are over.
	public static void ChangeToNextTrial()
	{
        //Checks if trials are over
        if (trial < numberOfTrials)
        {
            SceneManager.LoadScene("Trial");
        }
        else if (block < numberOfBlocks - 1)
        {
            SceneManager.LoadScene("InterBlockRest");
        }
        else if ((block == numberOfBlocks - 1) && currentProblem < numberOfProblems)
        {
            SceneManager.LoadScene("InterProblemRest");
        }
        else
        {
			SceneManager.LoadScene ("End");
		}
	}

	// Extracts the items that were finally selected based on the sequence of clicks.
	private static string ExtractItemsSelected (List <Vector3> itemClicks){
		List<int> itemsIn = new List<int>();
		foreach(Vector3 clickito in itemClicks){
			if (clickito.z == 1) {
				itemsIn.Add (Convert.ToInt32 (clickito.x));
			} else if (clickito.z == 0) {
				itemsIn.Remove (Convert.ToInt32 (clickito.x));
			} else if (clickito.z == 3) {
				itemsIn.Clear ();
			}
		}

		string itemsInS = "";
		foreach (int i in itemsIn)
		{
			itemsInS = itemsInS + i + ",";
		}
		if(itemsInS.Length>0)
			itemsInS = itemsInS.Remove (itemsInS.Length - 1);

		return itemsInS;
	}

	// Updates the timer (including the graphical representation)
	// If time runs out in the trial or the break scene. It switches to the next scene.
	void StartTimer()
	{
		tiempo -= Time.deltaTime;
		if (showTimer) 
		{
			BoardFunctions.UpdateTimer();
		}
        
        //When the time runs out:
        if (tiempo < 0)
		{
            //Debug.Log("time ran out");
			ChangeToNextScene(BoardManager.itemClicks,0);
		}
	}

    static void SkipClicked()
    {
        Debug.Log("Skip Clicked");
        ChangeToNextScene(BoardManager.itemClicks, 0);
    }
}