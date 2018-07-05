using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Linq;
using Random = UnityEngine.Random;
//using UnityEditor;

public class InputOutputManager : MonoBehaviour
{

    // Setting up the variable participantID
    public static string participantID;

    // This is the randomisation number (#_param2.txt that is to be used for oder of instances for this participant)
    public static string randomisationID;

    // Current time, used in output file names
    public static string dateID = @System.DateTime.Now.ToString("dd MMMM, yyyy, HH-mm");

    // Starting string of the output file names
    private static string TSPidentifier;
    private static string WCSPPidentifier;
    private static string Midentifier;

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
        TSPidentifier = "TSP_" + participantID + "_" + randomisationID + "_" + dateID + "_";
        WCSPPidentifier = "WCSPP_" + participantID + "_" + randomisationID + "_" + dateID + "_";
        Midentifier = "M_" + participantID + "_" + randomisationID + "_" + dateID + "_";

        folderPathLoad = Application.dataPath + inputFolder;
        folderPathLoadInstances = Application.dataPath + inputFolderInstances;
        folderPathSave = Application.dataPath + outputFolder;

        Dictionary<string, string> dict = LoadParameters();
        GameManager.AssignVariables(dict);

        GameManager.tsp_instances = LoadTSPInstances(GameManager.numberOfInstances);
        GameManager.wcspp_instances = LoadWCSPPInstances(GameManager.numberOfInstances);
        SaveHeaders();
    }

    // What does this do??????
    private static int[,] StringToMatrix(string matrixS)
    {
        int[] convertor = Array.ConvertAll(matrixS.Substring(1, matrixS.Length - 2).Split(','), int.Parse);

        int vectorheight = Convert.ToInt32(Math.Sqrt(convertor.Length));
        int[,] arr = new int[vectorheight, vectorheight]; // note the swap
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

        // Reading time_param.txt
        using (StreamReader sr1 = new StreamReader(folderPathLoad + "time_param.txt"))
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
            sr1.Close();
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
            sr2.Close();
        }
        return dict;
    }

    // Reads all TSP instances from .txt files.
    // The instances are stored as tspinstances structs in an array called "tspinstances"
    private static GameManager.TSPInstance[] LoadTSPInstances(int numberOfInstances)
    {
        GameManager.TSPInstance[] tsp_instances = new GameManager.TSPInstance[numberOfInstances];

        for (int k = 0; k < numberOfInstances; k++)
        {
            // create a dictionary where all the variables and definitions are strings
            var dict = new Dictionary<string, string>();
            // Use streamreader to read the input files if there are lines to read
            using (StreamReader sr = new StreamReader(folderPathLoadInstances + "t" + (k + 1) + ".txt"))
            {
                string line;
                while (!string.IsNullOrEmpty((line = sr.ReadLine())))
                {
                    string[] tmp = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    // Add the key-value pair to the dictionary:
                    dict.Add(tmp[0], tmp[1]);
                }
                sr.Close();
            }

            //convert (most of them) to integers, with variables and literals being arrays and the others single literals
            tsp_instances[k].cities = Array.ConvertAll(dict["cities"].Substring(1, dict["cities"].Length - 2).Split(','), int.Parse);
            tsp_instances[k].coordinatesx = Array.ConvertAll(dict["coordinatesx"].Substring(1, dict["coordinatesx"].Length - 2).Split(','), float.Parse);
            tsp_instances[k].coordinatesy = Array.ConvertAll(dict["coordinatesy"].Substring(1, dict["coordinatesy"].Length - 2).Split(','), float.Parse);

            tsp_instances[k].distancevector = Array.ConvertAll(dict["distancevector"].Substring(1, dict["distancevector"].Length - 2).Split(','), int.Parse);
            tsp_instances[k].distancematrix = StringToMatrix(dict["distancevector"]);

            tsp_instances[k].ncities = int.Parse(dict["ncities"]);
            tsp_instances[k].maxdistance = int.Parse(dict["maxdistance"]);
        }

        return (tsp_instances);
    }

    // Reads all WCSPP instances from .txt files.
    // The instances are stored as tspinstances structs in an array called "tspinstances"
    private static GameManager.WCSPPInstance[] LoadWCSPPInstances(int numberOfInstances)
    {
        GameManager.WCSPPInstance[] wcspp_instances = new GameManager.WCSPPInstance[numberOfInstances];

        //for (int k = 0; k < numberOfInstances; k++)
        for (int k = 0; k < 2; k++)
        {
            // create a dictionary where all the variables and definitions are strings
            var dict = new Dictionary<string, string>();
            // Use streamreader to read the input files if there are lines to read
            using (StreamReader sr = new StreamReader(folderPathLoadInstances + "w" + (k + 1) + ".txt"))
            {
                string line;
                while (!string.IsNullOrEmpty((line = sr.ReadLine())))
                {
                    string[] tmp = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    // Add the key-value pair to the dictionary:
                    dict.Add(tmp[0], tmp[1]);
                }
                sr.Close();
            }

            //convert (most of them) to integers, with variables and literals being arrays and the others single literals
            wcspp_instances[k].cities = Array.ConvertAll(dict["cities"].Substring(1, dict["cities"].Length - 2).Split(','), int.Parse);
            wcspp_instances[k].coordinatesx = Array.ConvertAll(dict["coordinatesx"].Substring(1, dict["coordinatesx"].Length - 2).Split(','), float.Parse);
            wcspp_instances[k].coordinatesy = Array.ConvertAll(dict["coordinatesy"].Substring(1, dict["coordinatesy"].Length - 2).Split(','), float.Parse);

            wcspp_instances[k].distancevector = Array.ConvertAll(dict["distancevector"].Substring(1, dict["distancevector"].Length - 2).Split(','), int.Parse);
            wcspp_instances[k].distancematrix = StringToMatrix(dict["distancevector"]);

            wcspp_instances[k].weightvector = Array.ConvertAll(dict["weightvector"].Substring(1, dict["weightvector"].Length - 2).Split(','), int.Parse);
            wcspp_instances[k].weightmatrix = StringToMatrix(dict["weightvector"]);

            wcspp_instances[k].ncities = int.Parse(dict["ncities"]);
            wcspp_instances[k].maxweight = int.Parse(dict["maxweight"]);
            
            wcspp_instances[k].startcity = int.Parse(dict["startcity"]);
            wcspp_instances[k].endcity = int.Parse(dict["endcity"]);
        }

        return (wcspp_instances);
    }

    // Saves the headers for output files
    // In the trial file it saves:  1. The participant ID. 2. Instance details.
    // In the TimeStamp file it saves: 1. The participant ID. 2.The time onset of the stopwatch from which the time stamps are measured. 3. the event types description.
    private static void SaveHeaders()
    {
        // Trial Info file headers
        string[] lines = new string[2];
        lines[0] = "PartcipantID:" + participantID;
        lines[1] = "block;trial;timeSpent;itemsSelected;finaldistance;instanceNumber;error";
        using (StreamWriter outputFile = new StreamWriter(folderPathSave + TSPidentifier + "TrialInfo.txt", true))
        {
            foreach (string line in lines)
                outputFile.WriteLine(line);
            outputFile.Close();
        }

        // Time Stamps file headers
        string[] lines1 = new string[3];
        lines1[0] = "PartcipantID:" + participantID;
        lines1[1] = "InitialTimeStamp:" + GameFunctions.initialTimeStamp;
        lines1[2] = "block;trial;eventType;elapsedTime";
        using (StreamWriter outputFile = new StreamWriter(folderPathSave + TSPidentifier + "TimeStamps.txt", true))
        {
            foreach (string line in lines1)
                outputFile.WriteLine(line);
            outputFile.Close();
        }

        // Headers for Clicks file
        string[] lines2 = new string[3];
        lines2[0] = "PartcipantID:" + participantID;
        lines2[1] = "InitialTimeStamp:" + GameFunctions.initialTimeStamp;
        lines2[2] = "block;trial;citynumber(100=Reset);In(1)/Out(0)/Reset(3);time";
        using (StreamWriter outputFile = new StreamWriter(folderPathSave + TSPidentifier + "Clicks.txt", true))
        {
            foreach (string line in lines2)
                outputFile.WriteLine(line);
            outputFile.Close();
        }
    }


    // Saves the data of a trial to a .txt file with the participants ID as filename using StreamWriter.
    // If the file doesn't exist it creates it. Otherwise it adds on lines to the existing file.
    public static void Save(string itemsSelected, float timeSpent, string error)
    {
        // Get the instance number for this trial (take the block number, subtract 1 because indexing starts at 0. Then multiply it
        // by numberOfTrials (i.e. 10, 10 per block) and add the trial number of this block. Thus, the 2nd trial of block 2 will be
        // instance number 12 overall) and add 1 because the instanceRandomization is linked to array numbering in C#, which starts at 0;
        int instanceNum = GameManager.tspRandomization[GameManager.TotalTrial - 1] + 1;
        int finaldistance = GameManager.Distancetravelled;

        // what to save and the order in which to do so
        string dataTrialText = GameManager.block + ";" + GameManager.trial + ";" + timeSpent + ";" + itemsSelected + ";" + finaldistance + ";"
            + instanceNum + ";" + error;


        // where to save
        string[] lines = { dataTrialText };

        using (StreamWriter outputFile = new StreamWriter(folderPathSave + TSPidentifier + "TrialInfo.txt", true))
        {
            foreach (string line in lines)
                outputFile.WriteLine(line);
            outputFile.Close();
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

        string[] lines = { dataTrialText };

        using (StreamWriter outputFile = new StreamWriter(folderPathSave + TSPidentifier + "TimeStamps.txt", true))
        {
            foreach (string line in lines)
                outputFile.WriteLine(line);
            outputFile.Close();
        }

    }

    // Saves the time stamp of every click made on the items 
    // block ; trial ; clicklist (i.e. item number ; itemIn? (1: selcting; 0:deselecting) ; time of the click with respect to the begining of the trial)
    public static void SaveClicks(List<BoardManager.Click> itemClicks)
    {
        string folderPathSave = Application.dataPath + outputFolder;

        string[] lines = new string[itemClicks.Count];
        int i = 0;
        foreach (BoardManager.Click click in itemClicks)
        {
            lines[i] = GameManager.block + ";" + GameManager.trial + ";" + click.CityNumber + ";" + click.State + ";" + click.time;
            i++;
        }

        using (StreamWriter outputFile = new StreamWriter(folderPathSave + TSPidentifier + "Clicks.txt", true))
        {
            foreach (string line in lines)
                outputFile.WriteLine(line);
            outputFile.Close();
        }
    }

}
