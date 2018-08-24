using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class PointerEventsController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /* This section is responsible for the highlighting function when the pointer is hovering over a city
     * No user data is recorded.
     */
    public static int fromcity;
    
    // Used to draw highlighing lines
    public static GameObject[] templines = new GameObject[100];

    public static GameObject[] tempWeights = new GameObject[100];
    public static GameObject[] tempDistances = new GameObject[100];

    public void OnPointerEnter(PointerEventData eventData)
    {
        fromcity = int.Parse(eventData.pointerCurrentRaycast.gameObject.GetComponent<Text>().text);

        for (int tocity = 0; tocity < BoardManager.ncities; tocity++)
        {
            HighlightLine(fromcity, tocity, 0.02f, Color.blue);
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        for (int tocity = 0; tocity < BoardManager.ncities; tocity++)
        {
            Destroy(templines[tocity]);
            Destroy(tempWeights[tocity]);
            Destroy(tempDistances[tocity]);
        }
    }

    // Function to draw slim lines in WCSPP instances (to represent the valid connections) and to display distance & weight information
    public static void HighlightLine(int cityofdeparture, int cityofdestination, float linewidth, Color textcol)
    {
        Vector2 coordestination = BoardManager.unitycoord[cityofdestination];
        Vector2 coordeparture = BoardManager.unitycoord[cityofdeparture];

        Vector3[] coordinates = new Vector3[2];
        coordinates[0] = coordestination;
        coordinates[1] = coordeparture;

        if (BoardManager.distances[cityofdeparture, cityofdestination] != 0)
        {
            templines[cityofdestination] = Instantiate(BoardManager.LineItemPrefab, new Vector2(0, 0), Quaternion.identity) as GameObject;
            BoardManager.canvas = GameObject.Find("Canvas");
            templines[cityofdestination].transform.SetParent(BoardManager.canvas.GetComponent<Transform>(), false);
            templines[cityofdestination].GetComponent<LineRenderer>().startWidth = linewidth;
            templines[cityofdestination].GetComponent<LineRenderer>().endWidth = linewidth;
            templines[cityofdestination].GetComponent<LineRenderer>().material.color = Color.cyan;
            templines[cityofdestination].GetComponent<LineRenderer>().sortingOrder = 2;
            templines[cityofdestination].GetComponent<LineRenderer>().SetPositions(coordinates);

            if (GameManager.problemName == 't'.ToString() || GameManager.problemName == 'm'.ToString())
            {
                // TSP instance
                int dt = BoardManager.distances[cityofdeparture, cityofdestination];
                tempDistances[cityofdestination] = Instantiate(BoardManager.TextPrefab, new Vector2(0, 0), Quaternion.identity) as GameObject;
                tempDistances[cityofdestination].transform.SetParent(BoardManager.canvas.GetComponent<Transform>(), false);
                tempDistances[cityofdestination].transform.position = ((coordestination + coordeparture) / 2);
                
                if (GameManager.problemName == 't'.ToString())
                {
                    tempDistances[cityofdestination].GetComponent<Text>().text = dt.ToString();
                }
                else
                {
                    tempDistances[cityofdestination].GetComponent<Text>().text = dt.ToString();
                }

                tempDistances[cityofdestination].GetComponent<Text>().color = textcol;
                tempDistances[cityofdestination].GetComponent<Light>().enabled = true;
            }
            else if (GameManager.problemName == 'w'.ToString())
            {
                // WCSPP Instance
                int wt = BoardManager.weights[cityofdeparture, cityofdestination];
                tempWeights[cityofdestination] = Instantiate(BoardManager.TextPrefab, new Vector2(0, 0), Quaternion.identity) as GameObject;
                tempWeights[cityofdestination].transform.SetParent(BoardManager.canvas.GetComponent<Transform>(), false);
                tempWeights[cityofdestination].transform.position = ((coordestination + coordeparture) / 2) - new Vector2(0.23f, 0.0f);
                tempWeights[cityofdestination].GetComponent<Text>().text = "$" + wt.ToString();
                tempWeights[cityofdestination].GetComponent<Text>().color = textcol;
                tempWeights[cityofdestination].GetComponent<Light>().enabled = true;

                int dt = BoardManager.distances[cityofdeparture, cityofdestination];
                tempDistances[cityofdestination] = Instantiate(BoardManager.TextPrefab, new Vector2(0, 0), Quaternion.identity) as GameObject;
                tempDistances[cityofdestination].transform.SetParent(BoardManager.canvas.GetComponent<Transform>(), false);
                tempDistances[cityofdestination].transform.position = ((coordestination + coordeparture) / 2) + new Vector2(0.23f, 0.0f);
                tempDistances[cityofdestination].GetComponent<Text>().text = "T:" + dt.ToString();
                tempDistances[cityofdestination].GetComponent<Text>().color = textcol;
                tempDistances[cityofdestination].GetComponent<Light>().enabled = true;
            }

            if (BoardManager.previouscities.Count() != 0 && cityofdestination == BoardManager.previouscities.Last())
            {
                templines[cityofdestination].GetComponent<LineRenderer>().material.color = Color.magenta;

                templines[cityofdestination].GetComponent<LineRenderer>().startWidth = linewidth * 2;
                templines[cityofdestination].GetComponent<LineRenderer>().endWidth = linewidth * 2;

                tempDistances[cityofdestination].GetComponent<Text>().color = Color.magenta;
                if (GameManager.problemName == 'w'.ToString())
                {
                    tempWeights[cityofdestination].GetComponent<Text>().color = Color.magenta;
                }

            }
        }

    }
}
