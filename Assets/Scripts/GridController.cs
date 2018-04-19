using UnityEngine;
using System;
using System.Collections.Generic;       //Allows us to use Lists.
using Random = UnityEngine.Random;      //Tells Random to use the Unity Engine random number generator.

public class GridController : MonoBehaviour {
    public const int MAP_WIDTH = 59;
    public const int MAP_HEIGHT = 89;
    public int columns = 89;                                         
    public int rows = 70;

    private Vector3 boardOffset = new Vector3(30.0f, 1.0f, 42.0f);


    private void Start() {

        var cube = Resources.Load("Cube") as GameObject;
        var cubeWall = Resources.Load("cubeWall") as GameObject;
        for (int x = -1; x < MAP_WIDTH + 1; x++) {
            //Within each column, loop through y axis (rows).
            for (int y = -1; y < MAP_HEIGHT + 1; y++) {
                if (x == -1 || x == columns || y == -1 || y == rows) {
                    Instantiate(cubeWall, new Vector3(x, 0, y) - boardOffset, Quaternion.identity);
                    continue;
                }
                //At each index add a new Vector3 to our list with the x and y coordinates of that position.
                var ins = Instantiate(cube, new Vector3(x, 0, y) - boardOffset, Quaternion.identity);
            }
        }
    }
}
