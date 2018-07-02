using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameFunctions : MonoBehaviour {

	// Stopwatch to calculate time of events.
	private static System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

	// Time at which the stopwatch started. Time of each event is calculated according to this moment.
	public static string initialTimeStamp;

    // The Start button
    public Button startButton;

    // Function that displays Participant ID, Randomisation ID, and Start prompts in order
    public static void SetupInitialScreen()
	{
		// Start and Rand inputs, initially set to inactive
		GameObject start = GameObject.Find("Start") as GameObject;
		start.SetActive (false);

		GameObject rand = GameObject.Find("RandomisationID") as GameObject;
		rand.SetActive (false);
 
        // Participant ID Input
        InputField pID = GameObject.Find ("ParticipantID").GetComponent<InputField>();

		InputField.SubmitEvent se = new InputField.SubmitEvent();
		se.AddListener((value)=>SubmitPID(value,start,rand));
		pID.onEndEdit = se;

		//Randomisation ID Input
		InputField rID = rand.GetComponent<InputField>();

		InputField.SubmitEvent se2 = new InputField.SubmitEvent();
		se2.AddListener((value)=>SubmitRandID(value,start));
		rID.onEndEdit = se2;
    }

    // Get Participant ID and set Randomisation ID as active
	private static void SubmitPID(string pIDs, GameObject start, GameObject rand)
	{
		GameObject pID = GameObject.Find ("ParticipantID");
		GameObject pIDT = GameObject.Find ("Participant ID Text");
		pID.SetActive (false);
		pIDT.SetActive (true);

		//Set Participant ID
		InputOutputManager.participantID=pIDs;
		Text inputID = pIDT.GetComponent<Text>();
		inputID.text = "Randomisation Number";

		//Activate Randomisation Listener
		rand.SetActive (true);
	}

    // Get Randomisation ID and set Start as active
    private static void SubmitRandID(string rIDs, GameObject start)
	{
		GameObject rID = GameObject.Find ("RandomisationID");
		GameObject rIDT = GameObject.Find ("Randomisation ID Text");
		rID.SetActive (false);
		rIDT.SetActive (true);

		//Set Participant ID
		InputOutputManager.randomisationID=rIDs;

		//Activate Start Button and listener
		start.SetActive (true);

        BoardManager.keysON = true;

        Button startButton = GameObject.Find("Start").GetComponent<Button>();
        startButton.onClick.AddListener(StartClicked);

	}

	//To pause press Control+P
	//Pauses/Unpauses the game. Unpausing takes you directly to next trial
	//Warning! When Unpausing the following happens:
	//If paused/unpaused in scene 1 (while items are shown) then saves the trialInfo with an error: "pause" without information on the items selected.
	//If paused/unpaused on ITI or IBI then it generates a new row in trial Info with an error ("pause"). i.e. there are now 2 rows for the trial.
	public static void PauseManager(){
		if (( Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl)) && Input.GetKeyDown (KeyCode.P) ){
			Time.timeScale = (Time.timeScale == 1) ? 0 : 1;
			if(Time.timeScale==1){
				InputOutputManager.ErrorInScene("Pause");
			}
		}
	}

	// Starts the stopwatch. Time of each event is calculated according to this moment.
	// Sets "initialTimeStamp" to the time at which the stopwatch started.
	public static void SetTimeStamp()
	{
		initialTimeStamp=@System.DateTime.Now.ToString("HH-mm-ss-fff");
		stopWatch.Start ();
		Debug.Log ("Start Time:" + initialTimeStamp);
	}

	// Calculates time elapsed
	/// <returns>The time elapsed in milliseconds since the "setTimeStamp()".</returns>
	public static string TimeStamp()
	{
		long milliSec = stopWatch.ElapsedMilliseconds;
		string stamp = milliSec.ToString();
		return stamp;
	}

    // Click Start button and move to next scene
    public static void StartClicked()
    {
        Debug.Log("Start Button Clicked");
        GameFunctions.SetTimeStamp();
        GameManager.ChangeToNextScene(BoardManager.itemClicks, 0);
    }
}
