using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BoardFunctions : MonoBehaviour
{
    // Resoultion width and Height
    // CAUTION! Modifying this does not modify the Screen resolution. This is related to the unit grid on Unity.
    // CAUTION x2! Listen to the above warning. It's really not a good idea to mess with the following parameters. Just leave them as is.
    public static int resolutionWidth = 1024 - 110;
    public static int resolutionHeight = 768 - 130;

    // Function which takes coordinates from input files and converts them to coordinates on the unity grid
    // unity origin x=-476, y=-344, which means opposite corner is x=476 and y=344
    // minimum input coordinate is (0,0), maximum input coordinate is (952,688)
    public static List<Vector2> CoordinateConvertor(float[] cox, float[] coy)
    {
        List<Vector2> unitycoordinates = new List<Vector2>();
        for (int i = 0; i < cox.Length; i++)
        {
            //x=0 in coord leads to x=-384 in unity, x=1024 leads to x=599.04 - so a 1 unit increase in coord leads to a 0.96 unit increase in unity
            //y=0 in coord leads to y=-288 in unity, y=768 leads to y=449.28 - so a 1 unit increase in coord leads to a 0.96 unit increase in unity
            unitycoordinates.Add(new Vector2((float)((cox[i] / cox.Max()) * resolutionWidth) / 100, (float)((coy[i] / coy.Max()) * resolutionHeight) / 100));
        }
        return unitycoordinates;
    }

    // Places the Item on the input position
    public static void PlaceItem(BoardManager.Item ItemToLocate, Vector2 position)
    {
        //Setting the position in a separate line is importatant in order to set it according to global coordinates.
        ItemToLocate.gameItem.transform.position = position;
        ItemToLocate.CityButton.onClick.AddListener(delegate { GameManager.gameManager.boardScript.ClickOnItem(ItemToLocate); });
    }

    // Updates the timer rectangle size accoriding to the remaining time.
    public static void UpdateTimer()
    {
        Image timer = GameObject.Find("Timer").GetComponent<Image>();
        timer.fillAmount = GameManager.tiempo / GameManager.totalTime;
    }

}
