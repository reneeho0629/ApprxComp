/* Unity 3D program that displays multiple interactive instances of the WCSPP, Minimum Vertex Cover and TSP. 
 * This program aims to test human performance in problems of varying computational complexity.
 * 
 * Input files are stored in ./StreamingAssets/Input
 * User responses and other data are stored in ./StreamingAssets/Output
 * 
 * Run program in 1680x1050 (or higher) after build
 * 
 * Based on Knapsack and TSP code written by Pablo Franco and Karlo Doroc
 * Modified by Anthony Hsu for his Finance Honours research project 2018.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Game Manager: It is a singleton (i.e. it is always one and the same it is not destroyed nor duplicated)
    public static GameManager gameManager = null;

    // The reference to the script managing the board (interface/canvas).
    public BoardManager boardScript;

    // Variables related to scene name and time
    // Current Scene
    public static string escena;

    // Time remaining on this scene
    public static float tiempo;

    // Total time for this scene
    public static float totalTime;

    // Time used for the instance
    public static float timeTaken;

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

    // Number of instance files to be considered. From i1.txt to i24.txt.
    public static int numberOfInstances;

    // Total number of Problems
    private static int numberOfProblems = 3;

    // Current problem number: 0, 1, 2.
    private static int currentProblem = 0;

    // Name (a char) of the current problem
    public static string problemName;

    // The order of the Instances to be presented
    public static int[] tspRandomization;
    public static int[] wcsppRandomization;
    public static int[] mtspRandomization;

    // A list of floats to record participant performance
    // Performance should always be equal to or greater than 1.
    // Due to the way it's calculated (participant answer/optimal solution), performance closer to 1 is better.
    public static List<float> perf = new List<float>();
    public static float performance;

    // The order of the Problems to be presented
    public static List<string> problemOrder;

    // distance travelled
    public static int Distancetravelled;

    // current weight
    public static int weightValue;

    // binary variable to keep track of whether the submission was due to time out or user choice
    public static int timedOut;

    // An array of all the TSP instances, i.e importing everything using the struct below 
    public static TSPInstance[] tspInstances;
    public static TSPInstance[] mtspInstances;

    // A struct that contains the parameters of each TSP instance
    public struct TSPInstance
    {
        // Cities and their coordinates
        public int[] cities;
        public float[] coordinatesx;
        public float[] coordinatesy;

        public int[,] distancematrix;

        public int ncities;

        public int solution;
    }

    // An array of all the instances, i.e importing everything using the structure below 
    public static WCSPPInstance[] wcsppInstances;

    // A struct that contains the parameters of each WCSPP instance
    public struct WCSPPInstance
    {
        public int[] cities;
        public float[] coordinatesx;
        public float[] coordinatesy;

        public int[,] distancematrix;

        public int[,] weightmatrix;

        public int ncities;
        public int maxweight;

        public int startcity;
        public int endcity;

        public int solution;
    }

    // Keep track of total payment
    // Default value is the show up fee
    public static float payAmount = 5f;

    // Use this for initialization
    void Start()
    {
        // Makes the GameManager a Singleton
        if (gameManager == null)
        {
            gameManager = this;
        }
        else if (gameManager != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        // Initializes the game
        boardScript = gameManager.GetComponent<BoardManager>();
        InitGame();

        if (escena != "SetUp")
        {
            InputOutputManager.SaveTimeStamp(escena);
        }
    }

    // Initializes the scene. One scene is setup, other is trial, other is Break....
    void InitGame()
    {
        /* Scene Order
         * 0= Setup
         * 1= Trial game
         * 2= Intertrial rest
         * 3= Interblock rest
         * 4= Interproblem rest
         * 5= End
         * 6= Payment
         */

        // Selects the active scene, and call it "escena" - that's Spanish for "scene".
        Scene scene = SceneManager.GetActiveScene();
        escena = scene.name;

        // The loop which runs the game, and drives you from one scene to another
        // If it's the first scene, upload parameters and instances (this happens only once), move incrememntally through >blocks< 1 at a time
        if (escena == "SetUp")
        {
            showTimer = false;
            block++;
            GameFunctions.SetupInitialScreen();
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

            problemName = problemOrder[currentProblem];
            currentProblem++;

            Text nombre = GameObject.Find("ProblemName").GetComponent<Text>();
            if (problemName == 't'.ToString())
            {
                nombre.text = "Next Problem: TSP";
            }
            else if (problemName == 'w'.ToString())
            {
                nombre.text = "Next Problem: WCSPP";
            }
            else if (problemName == 'm'.ToString())
            {
                nombre.text = "Next Problem: Metric TSP";
            }

            skipButton = GameObject.Find("Skip").GetComponent<Button>();
            skipButton.onClick.AddListener(SkipClicked);
        }
        else if (escena == "End")
        {
            showTimer = false;

            skipButton = GameObject.Find("Skip").GetComponent<Button>();
            skipButton.onClick.AddListener(SkipClicked);
        }
        else if (escena == "Payment")
        {
            showTimer = false;

            Text one = GameObject.Find("Uno").GetComponent<Text>();
            one.text = DisplayPerf(0);

            Text two = GameObject.Find("Dos").GetComponent<Text>();
            two.text = DisplayPerf(1);

            Text three = GameObject.Find("Tres").GetComponent<Text>();
            three.text = DisplayPerf(2);

            Text pay = GameObject.Find("PayText").GetComponent<Text>();
            pay.text = "Total Payment: $" + payAmount.ToString();
        }
    }

    // Function to display user performance (last scene)
    public static string DisplayPerf(int problemNumber)
    {
        string perfText = "";

        string probName = problemOrder[problemNumber];

        if (probName == 't'.ToString())
        {
            // TSP instance
            perfText += "TSP:";
        }
        else if (probName == 'w'.ToString())
        {
            // WCSPP Instance
            perfText += "WCSPP:";
        }
        else if (probName == 'm'.ToString())
        {
            // M Instance
            perfText += "MTSP:";
        }

        for (int i = problemNumber * numberOfInstances; i < numberOfInstances + problemNumber * numberOfInstances; i++)
        {
            payAmount += perf[i];
            perfText += " " + perf[i] + ";";
        }
        return perfText;
    }

    // Update is called once per frame
    void Update()
    {
        if ((escena != "SetUp") && (escena != "Payment") && (escena != "End"))
        {
            StartTimer();
        }
    }

    // Assigns the parameters in the dictionary to variables
    public static void AssignVariables(Dictionary<string, string> dictionary)
    {
        // Assigns Parameters - these have been imported from input files
        timeRest1 = Convert.ToSingle(dictionary["timeRest1"]);
        timeRest2 = Convert.ToSingle(dictionary["timeRest2"]);
        timeRest3 = Convert.ToSingle(dictionary["timeRest3"]);
        timeQuestion = int.Parse(dictionary["timeQuestion"]);
        numberOfTrials = int.Parse(dictionary["numberOfTrials"]);
        numberOfBlocks = int.Parse(dictionary["numberOfBlocks"]);
        numberOfInstances = numberOfTrials * numberOfBlocks;

        // Getting TSP randomisation parameters. 
        tspRandomization = StrToInt(dictionary["tspRandomization"]);

        // Getting WCSPP randomisation parameters
        wcsppRandomization = StrToInt(dictionary["wcsppRandomization"]);

        // Getting m randomisation parameters
        mtspRandomization = StrToInt(dictionary["mRandomization"]);

        // Getting problem randomisation parameters
        problemOrder = StrToStr(dictionary["problemOrder"]);
    }

    // Function to convert a string to a list of ints
    public static int[] StrToInt(string valueStr)
    {
        int[] targetList = Array.ConvertAll(valueStr.Substring(1, valueStr.Length - 2).Split(','), int.Parse);

        for (int i = 0; i < targetList.Length; i++)
        {
            --targetList[i];
        }

        return targetList;
    }
    
    // Function to convert a string to a list of substrings
    public static List<string> StrToStr(string valueStr)
    {
        return valueStr.Substring(1, valueStr.Length - 2).Split(',').ToList();
    }

    // Takes care of changing the Scene to the next one (Except for when in the setup scene)
    public static void ChangeToNextScene(List<BoardManager.Click> itemClicks, bool skipped)
    {
        BoardManager.keysON = false;
        if (escena == "SetUp")
        {
            InputOutputManager.LoadGame();
            problemName = problemOrder[currentProblem];
            SceneManager.LoadScene("InterProblemRest");
        }
        else if (escena == "Trial")
        {
            if (skipped)
            {
                timeTaken = timeQuestion - tiempo;
            }
            else
            {
                timeTaken = timeQuestion;
            }

            // Save participant answer
            performance = (float)Distancetravelled / BoardManager.solution;
            if (timedOut == 1)
            {
                performance = 0;
            }
            perf.Add(performance);

            InputOutputManager.SaveTrialInfo(ExtractItemsSelected(itemClicks), timeTaken);
            InputOutputManager.SaveTimeStamp("ParticipantAnswer");
            InputOutputManager.SaveClicks(itemClicks);

            // Load next scene
            SceneManager.LoadScene("InterTrialRest");
        }
        else if (escena == "InterTrialRest")
        {
            ChangeToNextTrial();
        }
        else if (escena == "InterBlockRest")
        {
            SceneManager.LoadScene("Trial");
        }
        else if (escena == "InterProblemRest")
        {
            ChangeToNextTrial();
        }
        else if (escena == "End")
        {
            SceneManager.LoadScene("Payment");
        }
    }

    // Redirects to the next scene depending if the trials or blocks are over.
    public static void ChangeToNextTrial()
    {
        // Checks if trials are over
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
            SceneManager.LoadScene("End");
        }
    }

    // Extracts the items that were finally selected based on the sequence of clicks.
    private static string ExtractItemsSelected(List<BoardManager.Click> itemClicks)
    {
        List<int> itemsIn = new List<int>();
        foreach (BoardManager.Click click in itemClicks)
        {
            if (click.State == 1)
            {
                itemsIn.Add(Convert.ToInt32(click.CityNumber));
            }
            else if (click.State == 0)
            {
                itemsIn.Remove(Convert.ToInt32(click.CityNumber));
            }
            else if (click.State == 3)
            {
                itemsIn.Clear();
            }
        }

        string itemsInS = string.Empty;
        foreach (int i in itemsIn)
        {
            itemsInS = itemsInS + i + ",";
        }

        if (itemsInS.Length > 0)
        {
            itemsInS = itemsInS.Remove(itemsInS.Length - 1);
        }

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

        // When the time runs out:
        if (tiempo < 0)
        {
            timedOut = 1;
            ChangeToNextScene(BoardManager.itemClicks, false);
        }
    }

    // Change to next scene if the user clicks skip
    static void SkipClicked()
    {
        Debug.Log("Skip Clicked");
        ChangeToNextScene(BoardManager.itemClicks, true);
    }
}