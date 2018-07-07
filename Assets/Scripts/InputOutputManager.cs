using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class InputOutputManager : MonoBehaviour
{
    // Setting up the variable participantID
    public static string participantID;

    // This is the randomisation number (#_param2.txt that is to be used for oder of instances for this participant)
    public static string randomisationID;

    // Current time, used in output file names
    public static string dateID = @System.DateTime.Now.ToString("dd MMMM, yyyy, HH-mm");

    // Starting string of the output file names
    private static string tspIdentifier;
    private static string wcsppIdentifier;
    private static string mIdentifier;

    // Input and Outout Folders with respect to the Application.dataPath;
    private static string inputFolder = "/StreamingAssets/Input/";
    private static string inputFolderInstances = "/StreamingAssets/Input/Instances/";
    private static string outputFolder = "/StreamingAssets/Output/";

    // Complete folder path of the inputs and ouputs
    private static string folderPathLoad;
    private static string folderPathLoadInstances;
    private static string folderPathSave;

    public static Dictionary<string, string> dict;

    public static void LoadGame()
    {
        tspIdentifier = "TSP_" + participantID + "_" + randomisationID + "_" + dateID + "_";
        wcsppIdentifier = "WCSPP_" + participantID + "_" + randomisationID + "_" + dateID + "_";
        mIdentifier = "M_" + participantID + "_" + randomisationID + "_" + dateID + "_";

        folderPathLoad = Application.dataPath + inputFolder;
        folderPathLoadInstances = Application.dataPath + inputFolderInstances;
        folderPathSave = Application.dataPath + outputFolder;

        // Load all time & randomisation parameters
        dict = LoadParameters();

        // Process time & randomisation parameters
        GameManager.AssignVariables(dict);

        // Load and process all instance parameters
        GameManager.tspInstances = LoadTSPInstances(GameManager.numberOfInstances);
        GameManager.wcsppInstances = LoadWCSPPInstances(GameManager.numberOfInstances);

        // Create output file headers
        SaveHeaders();
    }

    // Helper function that converts distance/weight Strings to Matrix
    // Each entry in the matrix has the format: [departurecity, destinationcity]
    private static int[,] StringToMatrix(string matrixS)
    {
        int[] convertor = Array.ConvertAll(matrixS.Substring(1, matrixS.Length - 2).Split(','), int.Parse);

        int vectorheight = Convert.ToInt32(Math.Sqrt(convertor.Length));
        int[,] arr = new int[vectorheight, vectorheight];
        int x = 0;
        int y = 0;
        for (int i = 0; i < convertor.Length; i++)
        {
            arr[x, y] = convertor[i];
            y++;
            if (y == vectorheight)
            {
                y = 0;
                x++;
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
            while (!string.IsNullOrEmpty(line1 = sr1.ReadLine()))
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

            while (!string.IsNullOrEmpty(line2 = sr2.ReadLine()))
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
        GameManager.TSPInstance[] tspInstances = new GameManager.TSPInstance[numberOfInstances];

        for (int k = 0; k < numberOfInstances; k++)
        {
            // create a dictionary where all the variables and definitions are strings
            var dict = new Dictionary<string, string>();

            // Use streamreader to read the input files if there are lines to read
            using (StreamReader sr = new StreamReader(folderPathLoadInstances + "t" + (k + 1) + ".txt"))
            {
                string line;
                while (!string.IsNullOrEmpty(line = sr.ReadLine()))
                {
                    string[] tmp = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                    // Add the key-value pair to the dictionary:
                    dict.Add(tmp[0], tmp[1]);
                }

                sr.Close();
            }

            // Convert (most of them) to integers, with variables and literals being arrays and the others single literals
            tspInstances[k].cities = Array.ConvertAll(dict["cities"].Substring(1, dict["cities"].Length - 2).Split(','), int.Parse);
            tspInstances[k].coordinatesx = Array.ConvertAll(dict["coordinatesx"].Substring(1, dict["coordinatesx"].Length - 2).Split(','), float.Parse);
            tspInstances[k].coordinatesy = Array.ConvertAll(dict["coordinatesy"].Substring(1, dict["coordinatesy"].Length - 2).Split(','), float.Parse);

            tspInstances[k].distancevector = Array.ConvertAll(dict["distancevector"].Substring(1, dict["distancevector"].Length - 2).Split(','), int.Parse);
            tspInstances[k].distancematrix = StringToMatrix(dict["distancevector"]);

            tspInstances[k].ncities = int.Parse(dict["ncities"]);
            tspInstances[k].maxdistance = int.Parse(dict["maxdistance"]);
        }

        return tspInstances;
    }

    // Reads all WCSPP instances from .txt files.
    // The instances are stored as tspinstances structs in an array called "tspinstances"
    private static GameManager.WCSPPInstance[] LoadWCSPPInstances(int numberOfInstances)
    {
        GameManager.WCSPPInstance[] wcsppInstances = new GameManager.WCSPPInstance[numberOfInstances];

        // for (int k = 0; k < numberOfInstances; k++)
        for (int k = 0; k < 2; k++)
        {
            // create a dictionary where all the variables and definitions are strings
            var dict = new Dictionary<string, string>();

            // Use streamreader to read the input files if there are lines to read
            using (StreamReader sr = new StreamReader(folderPathLoadInstances + "w" + (k + 1) + ".txt"))
            {
                string line;
                while (!string.IsNullOrEmpty(line = sr.ReadLine()))
                {
                    string[] tmp = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                    // Add the key-value pair to the dictionary:
                    dict.Add(tmp[0], tmp[1]);
                }

                sr.Close();
            }

            // Convert (most of them) to integers, with variables and literals being arrays and the others single literals
            wcsppInstances[k].cities = Array.ConvertAll(dict["cities"].Substring(1, dict["cities"].Length - 2).Split(','), int.Parse);
            wcsppInstances[k].coordinatesx = Array.ConvertAll(dict["coordinatesx"].Substring(1, dict["coordinatesx"].Length - 2).Split(','), float.Parse);
            wcsppInstances[k].coordinatesy = Array.ConvertAll(dict["coordinatesy"].Substring(1, dict["coordinatesy"].Length - 2).Split(','), float.Parse);

            wcsppInstances[k].distancevector = Array.ConvertAll(dict["distancevector"].Substring(1, dict["distancevector"].Length - 2).Split(','), int.Parse);
            wcsppInstances[k].distancematrix = StringToMatrix(dict["distancevector"]);

            wcsppInstances[k].weightvector = Array.ConvertAll(dict["weightvector"].Substring(1, dict["weightvector"].Length - 2).Split(','), int.Parse);
            wcsppInstances[k].weightmatrix = StringToMatrix(dict["weightvector"]);

            wcsppInstances[k].ncities = int.Parse(dict["ncities"]);
            wcsppInstances[k].maxweight = int.Parse(dict["maxweight"]);

            wcsppInstances[k].startcity = int.Parse(dict["startcity"]);
            wcsppInstances[k].endcity = int.Parse(dict["endcity"]);
        }

        return wcsppInstances;
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
        using (StreamWriter outputFile = new StreamWriter(folderPathSave + tspIdentifier + "TrialInfo.txt", true))
        {
            foreach (string line in lines)
            {
                outputFile.WriteLine(line);
            }

            outputFile.Close();
        }

        // Time Stamps file headers
        string[] lines1 = new string[3];
        lines1[0] = "PartcipantID:" + participantID;
        lines1[1] = "InitialTimeStamp:" + GameFunctions.initialTimeStamp;
        lines1[2] = "block;trial;eventType;elapsedTime";
        using (StreamWriter outputFile = new StreamWriter(folderPathSave + tspIdentifier + "TimeStamps.txt", true))
        {
            foreach (string line in lines1)
            {
                outputFile.WriteLine(line);
            }

            outputFile.Close();
        }

        // Headers for Clicks file
        string[] lines2 = new string[3];
        lines2[0] = "PartcipantID:" + participantID;
        lines2[1] = "InitialTimeStamp:" + GameFunctions.initialTimeStamp;
        lines2[2] = "block;trial;citynumber(100=Reset);In(1)/Out(0)/Reset(3);time";
        using (StreamWriter outputFile = new StreamWriter(folderPathSave + tspIdentifier + "Clicks.txt", true))
        {
            foreach (string line in lines2)
            {
                outputFile.WriteLine(line);
            }

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

        // Where to save
        string[] lines = { dataTrialText };

        using (StreamWriter outputFile = new StreamWriter(folderPathSave + tspIdentifier + "TrialInfo.txt", true))
        {
            foreach (string line in lines)
            {
                outputFile.WriteLine(line);
            }

            outputFile.Close();
        }

        // Options of streamwriter include: Write, WriteLine, WriteAsync, WriteLineAsync
    }

    // Saves the time stamp for a particular event type to the "TimeStamps" File
    // Event type: 1=ItemsNoQuestion; 11=ItemsWithQuestion; 2=AnswerScreen; 21=ParticipantsAnswer; 3=InterTrialScreen; 4=InterBlockScreen; 5=EndScreen
    public static void SaveTimeStamp(string eventType)
    {
        string dataTrialText = GameManager.block + ";" + GameManager.trial + ";" + eventType + ";" + GameFunctions.TimeStamp();

        string[] lines = { dataTrialText };

        using (StreamWriter outputFile = new StreamWriter(folderPathSave + tspIdentifier + "TimeStamps.txt", true))
        {
            foreach (string line in lines)
            {
                outputFile.WriteLine(line);
            }

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

        using (StreamWriter outputFile = new StreamWriter(folderPathSave + tspIdentifier + "Clicks.txt", true))
        {
            foreach (string line in lines)
            {
                outputFile.WriteLine(line);
            }

            outputFile.Close();
        }
    }
}
