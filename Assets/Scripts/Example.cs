using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


/**
 * 1.create a unity 2D project
 * 2.copy scripts to you project folder
 * 3.create a gameobject,and attach example.cs to it
 * 4.play
 */
public class Example : MonoBehaviour
{
    enum TileType
    {
        none,
        wall,
    }

    public int width = 15;      //tile map width
    public int height = 12;     //tile map height
    public int obstacleFillPercent = 30;    //tile map obstacle fill percent
    public float scale = 32f;

    Sprite tilePrefab;
    string message = "";

    List<int> passableValues;

    GameObject allMapTiles;     //the map and tiles
    GameObject player;          //the player
    GameObject goal;            //the goal



    void Start()
    {
        Camera.main.orthographicSize = scale * 10;
        Camera.main.gameObject.transform.position = new Vector3(width * scale / 2, height * scale / 2, -10);
        tilePrefab = Sprite.Create(new Texture2D((int)scale, (int)scale), new Rect(0, 0, scale, scale), new Vector2(0.5f, 0.5f), 1f);

        goal = new GameObject("goal");
        goal.AddComponent<SpriteRenderer>();
        goal.GetComponent<SpriteRenderer>().sprite = tilePrefab;
        goal.GetComponent<SpriteRenderer>().color = Color.yellow;
        goal.GetComponent<SpriteRenderer>().sortingOrder = 1;

        player = new GameObject("player");
        player.AddComponent<SpriteRenderer>();
        player.GetComponent<SpriteRenderer>().sprite = tilePrefab;
        player.GetComponent<SpriteRenderer>().color = Color.red;
        player.GetComponent<SpriteRenderer>().sortingOrder = 2;


        passableValues = new List<int>();
        passableValues.Add((int)TileType.none);
    }

    void OnGUI()
    {


        GUI.Label(new Rect(180, 20, 300, 30), message);
    }

    /**
     * simulate path finding in grid tilemaps
     */

    void setTransformPosition(Transform trans, Vector2Int pos, float xScale, float yScale)
    {
        trans.position = new Vector3(pos.x * xScale, pos.y * yScale, 0);
    }

    void renderMap(Dictionary<Vector2Int, int> map, float xScale, float yScale)
    {
        Destroy(allMapTiles);
        allMapTiles = new GameObject("allMapTiles");
        foreach (var item in map)
        {
            GameObject temp = new GameObject();
            temp.transform.position = new Vector3(item.Key.x * xScale, item.Key.y * yScale, 0);
            SpriteRenderer spr = temp.AddComponent<SpriteRenderer>();
            spr.sprite = tilePrefab;
            switch (item.Value)
            {
                case (int)TileType.none:
                    spr.color = Color.white;
                    break;
                case (int)TileType.wall:
                    spr.color = Color.black;
                    break;
            }
            temp.transform.parent = allMapTiles.transform;
        }
    }

    IEnumerator movePlayer(List<Vector2Int> path, float xScale, float yScale, float interval = 0.1f)
    {
        foreach(var item in path) {
            setTransformPosition(player.transform, item, xScale, yScale);
            yield return new WaitForSeconds(interval);
        }
   
        message = "reach goal !";
    }

    int[,] generateMapArray(int pwidth, int pheight)
    {
        var mapArray = new int[pwidth, pheight];
        for (int x = 0; x < pwidth; x++)
        {
            for (int y = 0; y < pheight; y++)
            {
                mapArray[x, y] = Random.Range(0, 100) < obstacleFillPercent ? (int)TileType.wall : (int)TileType.none;
            }
        }
        return mapArray;
    }

    //Dictionary<Vector2Int, int> mapToDict4(int[,] mapArray)
    //{
    //    Dictionary<Vector2Int, int> mapDict = new Dictionary<Vector2Int, int>();
    //    for (int x = 0; x < mapArray.GetLength(0); x++)
    //    {
    //        for (int y = 0; y < mapArray.GetLength(1); y++)
    //        {
    //            mapDict.Add(new Vector2Int(x, y), mapArray[x, y]);
    //        }
    //    }
    //    return mapDict;
    //}

}