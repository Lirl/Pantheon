using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour {

    public GameObject TilePrefab;
    public Tile[,] tiles = new Tile[8, 8];
    private Tile selectedTile;
    private Tile hoveredTile;

    Vector3 boardOffset = new Vector3(10, -10, 0);

    Vector2 mouseOver;
    Grid grid;

    void Start() {
        GenerateTiles();
        grid = GetComponent<Grid>();
    }

    void Update() {
        UpdateMouseOver();

        if (Input.GetMouseButtonDown(0)) {
            // TODO: use mousePosition coordinates
            // to pick up the tile selected as set it to selectedTile
        }
    }

    private void UpdateMouseOver() {
        // TODO:
        // use raycast to get grid position
        // and update hoveredTile according to mouse position
        
        if (!Camera.main) {
            Debug.LogError("Somehow main camera is not set");
            return;
        }

        // get mouse click's position in 2d plane
        Vector3 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pz.y = 0;

        // convert mouse click's position to Grid position
        Vector3 cellPosition = grid.WorldToCell(pz) + boardOffset;

        

    }

    private void GenerateTiles() {

    }
}
