using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Linq; 
using Random = UnityEngine.Random;
//using UnityEditor;

public class InputOutputManager : MonoBehaviour {

	// Setting up the variable participantID
	public static string participantID = "Empty";

	// This is the randomisation number (#_param2.txt that is to be used for oder of instances for this participant)
	public static string randomisationID = "Empty";

    // Current time, used in output file names
	public static string dateID = @System.DateTime.Now.ToString("dd MMMM, yyyy, HH-mm");

	// Starting string of the output file names
	private static string identifierName;

	// Input and Outout Folders with respect to the Application.dataPath;
	private static string inputFolder = "/StreamingAssets/Input/";
	private static string inputFolderInstances = "/StreamingAssets/Input/Instances/";
	private static string outputFolder = "/StreamingAssets/Output/";

	// Complete folder path of the inputs and ouputs
	private static string folderPathLoad;
	private static string folderPathLoadInstances;
	private static string folderPathSave;

	public static void LoadGame()
    {
		identifierName = "TSP_" + participantID + "_" + randomisationID + "_" + dateID + "_";

		folderPathLoad = Application.dataPath + inputFolder;
		folderPathLoadInstances = Application.dataPath + inputFolderInstances;
		folderPathSave = Application.dataPath + outputFolder;

		Dictionary<string, string> dict = LoadParameters ();
		GameManager.AssignVariables(dict);

		GameManager.game_instances = LoadInstances (GameManager.numberOfInstances);
		SaveHeaders ();
	}

    // What does this do??????
	private static int[,] StringToMatrix(string matrixS)
	{
		int[] convertor = Array.ConvertAll (matrixS.Substring (1, matrixS.Length - 2).Split (','), int.Parse);

		int vectorheight = Convert.ToInt32(Math.Sqrt (convertor.Length));
		int[,] arr = new int[vectorheight,vectorheight]; // note the swap
		int x = 0;
		int y = 0;
		for (int i = 0; i < convertor.Length; ++i)
		{
			arr[y, x] = convertor[i]; // note the swap
			x++;
			if (x == vectorheight)
			{
				x = 0;
				y++;
			}
		}
		return arr;

	}

	// Loads the parameters from the text files: param.txt
	private static Dictionary<string, string> LoadParameters()
	{
        // Store parameters in a dictionary
		var dict = new Dictionary<string, string>();

        // Reading param.txt
        using (StreamReader sr1 = new StreamReader(folderPathLoad + "param.txt"))
        {

            // (This loop reads every line until EOF or the first blank line.)
            string line1;
            while (!string.IsNullOrEmpty((line1 = sr1.ReadLine())))
            {
                // Split each line around ':'
                string[] tmp = line1.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                // Add the key-value pair to the dictionary:
                dict.Add(tmp[0], tmp[1]);
            }
        }

        // Reading param2.txt within the Input folder
        using (StreamReader sr2 = new StreamReader(folderPathLoadInstances + randomisationID + "_param2.txt"))
        {
            // (This loop reads every line until EOF or the first blank line.)
            string line2;
            while (!string.IsNullOrEmpty((line2 = sr2.ReadLine())))
            {
                // Split each line around ':'
                string[] tmp = line2.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                // Add the key-value pair to the dictionary:
                dict.Add(tmp[0], tmp[1]);
            }
        }
		return dict;
	}

	// Reads all instances from .txt files.
	// The instances are stored as tspinstances structs in an array called "tspinstances"
	private static GameManager.GameInstance[] LoadInstances(int numberOfInstances)
	{
		GameManager.GameInstance[] game_instances = new GameManager.GameInstance[numberOfInstances];

		for (int k = 1; k <= numberOfInstances; k++) {
			// create a dictionary where all the variables and definitions are strings
			var dict = new Dictionary<string, string> ();
			// Use streamreader to read the input files if there are lines to read
			using (StreamReader sr = new StreamReader (folderPathLoadInstances + "i"+ k + ".txt")) {
				string line;
				while (!string.IsNullOrEmpty ((line = sr.ReadLine ()))) {
					string[] tmp = line.Split (new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
					// Add the key-value pair to the dictionary:
					dict.Add (tmp [0], tmp [1]);//int.Parse(dict[tmp[1]]);
				}
			}


			// the following are all recorded as strings (hence the S at the end) 
			string citiesS;
			string coordinatesxS;
			string coordinatesyS;
			string distancevectorS;
			string ncitiesS;
			string maxdistanceS;

			//grab all of those parameters as strings
			dict.TryGetValue ("cities", out citiesS);
			dict.TryGetValue ("coordinatesx", out coordinatesxS);
			dict.TryGetValue ("coordinatesy", out coordinatesyS);
			dict.TryGetValue ("distancevector", out distancevectorS);
			dict.TryGetValue ("ncities", out ncitiesS);
			dict.TryGetValue ("maxdistance", out maxdistanceS);

			//convert (most of them) to integers, with variables and literals being arrays and the others single literals
			game_instances [k-1].cities = Array.ConvertAll (citiesS.Substring (1, citiesS.Length - 2).Split (','), int.Parse);
			game_instances [k-1].coordinatesx = Array.ConvertAll (coordinatesxS.Substring (1, coordinatesxS.Length - 2).Split (','), float.Parse);
			game_instances [k-1].coordinatesy = Array.ConvertAll (coordinatesyS.Substring (1, coordinatesyS.Length - 2).Split (','), float.Parse);

			game_instances [k-1].distancevector = Array.ConvertAll (distancevectorS.Substring (1, distancevectorS.Length - 2).Split (','), int.Parse);
			game_instances [k - 1].distancematrix = StringToMatrix(distancevectorS);

			game_instances [k-1].ncities = int.Parse (ncitiesS);
			game_instances [k-1].maxdistance = int.Parse (maxdistanceS);

			dict.TryGetValue ("MZN", out game_instances [k - 1].id);
			dict.TryGetValue ("param", out game_instances [k - 1].param);
			dict.TryGetValue ("type", out game_instances [k - 1].type);
		}

		return(game_instances);
	}

	/// <summary>
	/// Saves the headers for both files (Trial Info and Time Stamps)
	/// In the trial file it saves:  1. The participant ID. 2. Instance details.
	/// In the TimeStamp file it saves: 1. The participant ID. 2.The time onset of the stopwatch from which the time stamps are measured. 3. the event types description.
	/// </summary>
	private static void SaveHeaders()
	{
		// Trial Info file headers
		string[] lines = new string[2];
		lines[0]="PartcipantID:" + participantID;
		lines [1] = "block;trial;timeSpent;itemsSelected;finaldistance;instanceNumber;error";
		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "TrialInfo.txt",true)) 
		{
			foreach (string line in lines)
				outputFile.WriteLine(line);
		}


		// Time Stamps file headers
		string[] lines1 = new string[3];
		lines1[0]="PartcipantID:" + participantID;
		lines1[1] = "InitialTimeStamp:" + GameFunctions.initialTimeStamp;
		lines1[2]="block;trial;eventType;elapsedTime";
		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "TimeStamps.txt",true)) 
		{
			foreach (string line in lines1)
				outputFile.WriteLine(line);
		}

		// Headers for Clicks file
		string[] lines2 = new string[3];
		lines2[0]="PartcipantID:" + participantID;
		lines2[1] = "InitialTimeStamp:" + GameFunctions.initialTimeStamp;
		lines2[2]="block;trial;citynumber(100=Reset);In(1)/Out(0)/Reset(3);time"; 
		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "Clicks.txt",true)) {
			foreach (string line in lines2)
				outputFile.WriteLine(line);
		}

	}


	// Saves the data of a trial to a .txt file with the participants ID as filename using StreamWriter.
	// If the file doesn't exist it creates it. Otherwise it adds on lines to the existing file.
	// Each line in the File has the following structure: "trial;timeSpent".
	public static void Save(string itemsSelected, float timeSpent, string error) 
	{
		// Get the instance number for this trial (take the block number, subtract 1 because indexing starts at 0. Then multiply it
		// by numberOfTrials (i.e. 10, 10 per block) and add the trial number of this block. Thus, the 2nd trial of block 2 will be
		// instance number 12 overall) and add 1 because the instanceRandomization is linked to array numbering in C#, which starts at 0;
		int instanceNum = GameManager.instanceRandomization [GameManager.TotalTrial - 1] + 1;
		int finaldistance = GameManager.Distancetravelled;
        
		// what to save and the order in which to do so
		string dataTrialText = GameManager.block + ";" + GameManager.trial + ";" + timeSpent + ";" + itemsSelected + ";" + finaldistance +";" 
			+ instanceNum + ";" + error;


		// where to save
		string[] lines = {dataTrialText};
		string folderPathSave = Application.dataPath + outputFolder;

		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName +"TrialInfo.txt",true)) 
		{
			foreach (string line in lines)
				outputFile.WriteLine(line);
		}

		//Options of streamwriter include: Write, WriteLine, WriteAsync, WriteLineAsync
	}


	/// <summary>
	/// Saves the time stamp for a particular event type to the "TimeStamps" File
	/// All these saves take place in the Data folder, where you can create an output folder
	/// </summary>
	/// Event type: 1=ItemsNoQuestion;11=ItemsWithQuestion;2=AnswerScreen;21=ParticipantsAnswer;3=InterTrialScreen;4=InterBlockScreen;5=EndScreen
	public static void SaveTimeStamp(string eventType) 
	{
		string dataTrialText = GameManager.block + ";" + GameManager.trial + ";" + eventType + ";" + GameFunctions.TimeStamp();

		string[] lines = {dataTrialText};
		string folderPathSave = Application.dataPath + outputFolder;

		//This location can be used by unity to save a file if u open the game in any platform/computer:      Application.persistentDataPath;
		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "TimeStamps.txt",true)) 
		{
			foreach (string line in lines)
				outputFile.WriteLine(line);
		}

	}
		
	// Saves the time stamp of every click made on the items 
	// block ; trial ; clicklist (i.e. item number ; itemIn? (1: selcting; 0:deselecting) ; time of the click with respect to the begining of the trial)
	public static void SaveClicks(List<Vector3> clicksList) {
		string folderPathSave = Application.dataPath + outputFolder;

		string[] lines = new string[clicksList.Count];
		int i = 0;
		foreach (Vector3 clickito in clicksList) {
			lines[i]= GameManager.block + ";" + GameManager.trial + ";" + clickito.x + ";" + clickito.z + ";" + clickito.y ; 
			i++;
		}

		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "Clicks.txt",true)) {
			foreach (string line in lines)
				outputFile.WriteLine(line);
		} 
	}

 
	// In case of an error: Skip trial and go to next one.
	// Receives as input a string with the errorDetails which is saved in the output file.
	public static void ErrorInScene(string errorDetails){
		Debug.Log (errorDetails);
		BoardManager.keysON = false;
		InputOutputManager.Save ("", GameManager.totalTime, errorDetails);
		GameManager.ChangeToNextTrial ();
	}

}
