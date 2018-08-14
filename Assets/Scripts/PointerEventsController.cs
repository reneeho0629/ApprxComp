using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class PointerEventsController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /* This section is responsible for the highlighting function when the pointer is hovering over a city
     * No user data is recorded.
     */
    public static int fromcity;
    public static int tocity;

    public static GameObject tempLine;
    public static GameObject tempDistance;
    public static GameObject tempWeight;

    public void OnPointerEnter(PointerEventData eventData)
    {
        tocity = int.Parse(eventData.pointerCurrentRaycast.gameObject.GetComponent<Text>().text);
        //Debug.Log(eventData.pointerCurrentRaycast.gameObject.GetComponent<Text>().text);
        if (BoardManager.previouscities.Count() != 0)
        {
            fromcity = BoardManager.previouscities.Last();
            HighlightLine(fromcity, tocity, 0.04f);
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("pointer out");
        Destroy(tempLine);
        Destroy(tempDistance);
        Destroy(tempWeight);
    }

    // Function to draw slim lines in WCSPP instances (to represent the valid connections) and to display distance & weight information
    public static void HighlightLine(int cityofdeparture, int cityofdestination, float linewidth)
    {
        Vector2 coordestination = BoardManager.unitycoord[cityofdestination];
        Vector2 coordeparture = BoardManager.unitycoord[cityofdeparture];

        Vector3[] coordinates = new Vector3[2];
        coordinates[0] = coordestination;
        coordinates[1] = coordeparture;

        if (BoardManager.distances[BoardManager.previouscities.Last(), tocity] != 0)
        {
            tempLine = Instantiate(BoardManager.LineItemPrefab, new Vector2(0, 0), Quaternion.identity) as GameObject;
            BoardManager.canvas = GameObject.Find("Canvas");
            tempLine.transform.SetParent(BoardManager.canvas.GetComponent<Transform>(), false);
            tempLine.GetComponent<LineRenderer>().startWidth = linewidth;
            tempLine.GetComponent<LineRenderer>().endWidth = linewidth;
            tempLine.GetComponent<LineRenderer>().material.color = Color.green;
            tempLine.GetComponent<LineRenderer>().sortingOrder = 0;
            tempLine.GetComponent<LineRenderer>().SetPositions(coordinates);
        
            if (GameManager.problemName == 't'.ToString() || GameManager.problemName == 'm'.ToString())
            {
                // TSP instance
                int dt = BoardManager.distances[cityofdeparture, cityofdestination];
                tempDistance = Instantiate(BoardManager.TextPrefab, new Vector2(0, 0), Quaternion.identity) as GameObject;
                tempDistance.transform.SetParent(BoardManager.canvas.GetComponent<Transform>(), false);
                tempDistance.transform.position = ((coordestination + coordeparture) / 2);
                
                if (GameManager.problemName == 't'.ToString())
                {
                    tempDistance.GetComponent<Text>().text = "T:" + dt.ToString();
                }
                else
                {
                    tempDistance.GetComponent<Text>().text = "T:" + dt.ToString();
                }

                tempDistance.GetComponent<Text>().color = Color.blue;
                tempDistance.GetComponent<Light>().enabled = true;
            }
            else if (GameManager.problemName == 'w'.ToString())
            {
                // WCSPP Instance
                int wt = BoardManager.weights[cityofdeparture, cityofdestination];
                tempWeight = Instantiate(BoardManager.TextPrefab, new Vector2(0, 0), Quaternion.identity) as GameObject;
                tempWeight.transform.SetParent(BoardManager.canvas.GetComponent<Transform>(), false);
                tempWeight.transform.position = ((coordestination + coordeparture) / 2) - new Vector2(0.23f, 0.0f);
                tempWeight.GetComponent<Text>().text = "$" + wt.ToString();
                tempWeight.GetComponent<Text>().color = Color.blue;
                tempWeight.GetComponent<Light>().enabled = true;

                int dt = BoardManager.distances[cityofdeparture, cityofdestination];
                tempDistance = Instantiate(BoardManager.TextPrefab, new Vector2(0, 0), Quaternion.identity) as GameObject;
                tempDistance.transform.SetParent(BoardManager.canvas.GetComponent<Transform>(), false);
                tempDistance.transform.position = ((coordestination + coordeparture) / 2) + new Vector2(0.23f, 0.0f);
                tempDistance.GetComponent<Text>().text = "D:" + dt.ToString();
                tempDistance.GetComponent<Text>().color = Color.blue;
                tempDistance.GetComponent<Light>().enabled = true;
            }
        }
    }
}
