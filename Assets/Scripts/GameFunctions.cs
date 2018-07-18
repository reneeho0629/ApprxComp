using UnityEngine;
using UnityEngine.UI;

public class GameFunctions : MonoBehaviour
{
    // Time at which the stopwatch started. Time of each event is calculated according to this moment.
    public static string initialTimeStamp;

    // The Start button
    public Button startButton;

    // Stopwatch to calculate time of events.
    private static System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

    // Function that displays Participant ID, Randomisation ID, and Start prompts in order
    public static void SetupInitialScreen()
    {
        // Start and Rand inputs, initially set to inactive
        GameObject start = GameObject.Find("Start") as GameObject;
        start.SetActive(false);

        GameObject rand = GameObject.Find("RandomisationID") as GameObject;
        rand.SetActive(false);

        // Participant ID Input
        InputField pID = GameObject.Find("ParticipantID").GetComponent<InputField>();

        InputField.SubmitEvent se = new InputField.SubmitEvent();
        se.AddListener((value) => SubmitPID(value, start, rand));
        pID.onEndEdit = se;

        // Randomisation ID Input
        InputField rID = rand.GetComponent<InputField>();

        InputField.SubmitEvent se2 = new InputField.SubmitEvent();
        se2.AddListener((value) => SubmitRandID(value, start));
        rID.onEndEdit = se2;
    }

    // Get Participant ID and set Randomisation ID as active
    private static void SubmitPID(string pIDs, GameObject start, GameObject rand)
    {
        GameObject pID = GameObject.Find("ParticipantID");
        GameObject pIDT = GameObject.Find("Participant ID Text");
        pID.SetActive(false);
        pIDT.SetActive(true);

        // Set Participant ID
        InputOutputManager.participantID = pIDs;
        Text inputID = pIDT.GetComponent<Text>();
        inputID.text = "Randomisation Number";

        // Activate Randomisation Listener
        rand.SetActive(true);
    }

    // Get Randomisation ID and set Start as active
    private static void SubmitRandID(string rIDs, GameObject start)
    {
        GameObject rID = GameObject.Find("RandomisationID");
        GameObject rIDT = GameObject.Find("Randomisation ID Text");
        rID.SetActive(false);
        rIDT.SetActive(true);

        // Set Participant ID
        InputOutputManager.randomisationID = rIDs;

        // Activate Start Button and listener
        start.SetActive(true);

        BoardManager.keysON = true;

        Button startButton = GameObject.Find("Start").GetComponent<Button>();
        startButton.onClick.AddListener(StartClicked);
    }

    // Starts the stopwatch. Time of each event is calculated according to this moment.
    // Sets "initialTimeStamp" to the time at which the stopwatch started.
    public static void SetTimeStamp()
    {
        initialTimeStamp = @System.DateTime.Now.ToString("HH-mm-ss-fff");
        stopWatch.Start();
    }

    // Calculates time elapsed
    public static string TimeStamp()
    {
        long milliSec = stopWatch.ElapsedMilliseconds;
        return (milliSec / 1000f).ToString();
    }

    // Click Start button and move to next scene
    public static void StartClicked()
    {
        Debug.Log("Start Button Clicked");
        SetTimeStamp();
        GameManager.ChangeToNextScene(BoardManager.itemClicks, false);
    }
}
