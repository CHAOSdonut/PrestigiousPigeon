using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Tilemaps;
using ReDesign;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private TileBase ruleTile;
    [SerializeField] private Tilemap walkingLayer;

    // Start is called before the first frame update
    void Start()
    {

    }

    /// <summary>
    /// Moves player from current location to new location
    /// </summary>
    /// <param name="targetLocation">Destination vector location in world</param>
    /// <param name="gridLayout">Layout of the grid the player should move within so the player can be centered</param>
    /// <param name="pathNodesMap">List of PathNodes that form a grid together, PathNodes have both simplified and in world coordinates</param>
    public void MovePlayer(Vector3 targetLocation, GridLayout gridLayout, List<DefaultTile> pathNodesMap)
    {
        //Debug.Log("MOVE PLAYER ");

        //Currently only works for square grids not rectangular grids
        int width = (int) Math.Sqrt(pathNodesMap.Count); //temp
        int height = (int) Math.Sqrt(pathNodesMap.Count); //temp
        Debug.Log("pnodesmap count"+pathNodesMap.Count);
        Debug.Log("pnoesmap 100 xpos"+pathNodesMap[99].XPos);

        PlayerPathfinding playerPathfinding = new PlayerPathfinding(width, height, pathNodesMap);
        DefaultTile targetPathNode = FindNearestXYPathNode(targetLocation, pathNodesMap);
        DefaultTile playerPathNode = FindNearestXYPathNode(transform.position, pathNodesMap);

        Debug.Log("PLAYER: "+playerPathNode.XPos + ", "+playerPathNode.YPos);
        Debug.Log("TARGET: " + targetPathNode.XPos + ", " + targetPathNode.YPos);
        Debug.Log("WORLDPOS: "+ targetPathNode.GameObject.transform.position.x +", "+ targetPathNode.GameObject.transform.position.y+ ", "+ targetPathNode.GameObject.transform.position.z);
        List<DefaultTile> path = playerPathfinding.FindPath(playerPathNode.XPos, playerPathNode.YPos, targetPathNode.XPos, targetPathNode.YPos);
        Debug.Log(path.Count);
        //foreach(DefaultTile tile in pathNodesMap)
        {
            //Debug.Log("x: "+tile.XPos+", Y: "+tile.YPos);
        }
        if (path != null)
        {
            DrawPath(path);
            StartCoroutine(MoveSquares(path, gridLayout));
            playerPathNode.Walkable = true;
            targetPathNode.Walkable = false;
        }
    }

    // Finds the nearest PathNode based on world location coordinates; basically translates in world coordinates to the simplified ones.
    public DefaultTile FindNearestXYPathNode(Vector3 targetLocation, List<DefaultTile> pathNodesMap)
    {
        DefaultTile resultNode = pathNodesMap.OrderBy(item => Math.Abs(targetLocation.x - item.GameObject.transform.position.x)).ThenBy(item => Math.Abs(targetLocation.z - item.GameObject.transform.position.z)).ToList().FirstOrDefault();
        return resultNode;
    }

    IEnumerator MoveSquares(List<DefaultTile> path, GridLayout gridLayout)
    {
        foreach (DefaultTile pathNode in path)
        {
            transform.position = SnapCoordinateToGrid(new Vector3(pathNode.GameObject.transform.position.x, 0, pathNode.GameObject.transform.position.z), gridLayout); //fix!!!!
            yield return new WaitForSeconds(.2f);
            Vector3Int cell = walkingLayer.WorldToCell(new Vector3(pathNode.GameObject.transform.position.x, 0, pathNode.GameObject.transform.position.z));
            walkingLayer.SetTile(cell, null);
        }
    }

    private Vector3 SnapCoordinateToGrid(Vector3 position, GridLayout gridLayout)
    {
        Vector3Int cellPos = gridLayout.WorldToCell(position);
        Grid grid = gridLayout.gameObject.GetComponent<Grid>();
        position = grid.GetCellCenterWorld(cellPos);
        position.y = -0.5f; // Fix
        // Change Y position of player to match grid here
        return position;
    }

    void DrawPath(List<DefaultTile> pathNodes)
    {
        foreach (var node in pathNodes)
        {
            Vector3Int cell = walkingLayer.WorldToCell(new Vector3(node.GameObject.transform.position.x, 0, node.GameObject.transform.position.z));
            
            walkingLayer.SetTile(cell, ruleTile);
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}
