using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/* 
The current map is stored here.
All scene changes are coordinated through here;
*/

public class GameManager
{
    static Stack<Map> maps = new Stack<Map>();
    static JsonMapLoader jsonMapLoader = new JsonMapLoader();

    public static void PushPuzzle(string uuid)
    {
        maps.Push(jsonMapLoader.LoadMap(uuid));
        ShowPuzzleScreen();
    }

    public static void PopPuzzle()
    {
        maps.Pop();
        ShowPuzzleScreen();
    }

    public static Map GetCurrentPuzzle()
    {
        if (maps.Count == 0)
        {
            maps.Push(jsonMapLoader.LoadMapByName("Level 1: Enter the Henhouse"));
        }
        return maps.Peek();
    }

    public static void ShowPuzzleScreen()
    {
        SceneManager.LoadScene("Puzzle");
    }

    public static void ShowPauseMenu()
    {
        Debug.Log("You should implement the pause menu");
    }

    public static void ShowTitleScreen()
    {
        Debug.Log("You should implement the title screen");
    }
}
