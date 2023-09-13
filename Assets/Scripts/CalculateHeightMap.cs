using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;


public class CalculateHeightMap : MonoBehaviour
{
    [SerializeField] Tilemap groundTilemap;
    [SerializeField] Tilemap wallTilemap;

    public static Dictionary<Vector3Int, int> groundHeightMap = new Dictionary<Vector3Int, int>();
    public static Dictionary<Vector3Int, float> wallHeightMap = new Dictionary<Vector3Int, float>();

    const string stairsPrefix = "TX Struct_";

    // Timeout duration in milliseconds
    private const long TimeoutMilliseconds = 1000; // Adjust as needed

    /** Ground = Neighbour ground level, else, neighbour is stair = stair-1
     * Stair = Neighbour ground level+1, else, neighbout is stair = stair+1
     */
    void Start()
    {
        PropagateGround(new Vector3Int(0, 0, 0));
        PropagateWalls();
    }

    // Data structure to store position and level
    private struct TileInfo {
        public Vector3Int position;
        public int level;
    }

    private enum StairType{
        Bottom,
        Middle,
        Top,
        None
    }

    Queue<TileInfo> _frontier = new Queue<TileInfo>();
    HashSet<Vector3Int> _visited = new HashSet<Vector3Int>();

    // Function to propagate the level to neighboring ground tiles
    private void PropagateGround(Vector3Int position) {

        // Initialize the frontier with the starting position
        _frontier.Enqueue(new TileInfo { position = position, level = 0 });

        // Initialize stopwatch for timeout
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        while (_frontier.Count > 0) {
            // Check the elapsed time and exit if it exceeds the timeout
            if (stopwatch.ElapsedMilliseconds > TimeoutMilliseconds) {
                return;
            }

            // Set current to visited
            TileInfo currentTileInfo = _frontier.Dequeue();
            Vector3Int currentPos = currentTileInfo.position;

            _visited.Add(currentPos);

            // Set the level for the current position
            groundHeightMap[currentPos] = currentTileInfo.level;

            StairType currentStairType = TileNameToType(groundTilemap.GetTile(currentPos).name);

            TraverseNeighbour(currentStairType, currentTileInfo.level, currentPos, 1, 0);
            TraverseNeighbour(currentStairType, currentTileInfo.level, currentPos, -1, 0);
            TraverseNeighbour(currentStairType, currentTileInfo.level, currentPos, 0, 1);
            TraverseNeighbour(currentStairType, currentTileInfo.level, currentPos, 0, -1);
        }

        stopwatch.Stop();
    }

    void TraverseNeighbour(StairType currentStairType, int currentLevel, Vector3Int currentPos, int x, int y) {
        Vector3Int neighborPos = new Vector3Int(currentPos.x + x, currentPos.y + y, currentPos.z);

        // Skip the current position and visited positions
        if (neighborPos == currentPos || _visited.Contains(neighborPos)) return;

        TileBase neighborTile = groundTilemap.GetTile(neighborPos);
        if (neighborTile == null) return;

        // Propagate rule
        StairType neighbourStairType = TileNameToType(neighborTile.name);
        if (currentStairType == StairType.None) { //Ground
            if (neighbourStairType == StairType.None) { //Ground -> Ground (Same)
                if (wallTilemap.GetTile(neighborPos) != null) return; //Ground behind wall
                _frontier.Enqueue(new TileInfo { position = neighborPos, level = currentLevel });
            } else {
                if (neighbourStairType == StairType.Top) //Ground -> Top (Same)
                    _frontier.Enqueue(new TileInfo { position = neighborPos, level = currentLevel });
                else //Ground -> Bottom (Increase)
                    _frontier.Enqueue(new TileInfo { position = neighborPos, level = currentLevel});
            }
        } else { //Stair
            if (neighbourStairType == StairType.None) { //Stair -> Ground
                if (wallTilemap.GetTile(neighborPos) != null) return; //Ground behind wall
                if (currentStairType == StairType.Top) //Top -> Ground (Same)
                    _frontier.Enqueue(new TileInfo { position = neighborPos, level = currentLevel });
                else //Bottom -> Ground (Decrease)
                    _frontier.Enqueue(new TileInfo { position = neighborPos, level = currentLevel - 1 });
            } else { //Stair -> Stair
                if(currentStairType == neighbourStairType)
                    _frontier.Enqueue(new TileInfo { position = neighborPos, level = currentLevel});
                else if (neighbourStairType == StairType.Top) //Stair -> Top (Increase)
                    _frontier.Enqueue(new TileInfo { position = neighborPos, level = currentLevel + 1 });
                else if (neighbourStairType == StairType.Bottom) //Stair -> Bottom (Decrease)
                    _frontier.Enqueue(new TileInfo { position = neighborPos, level = currentLevel - 1 });
                else if (currentStairType == StairType.Top) //Top -> Stair (Decrease)
                    _frontier.Enqueue(new TileInfo { position = neighborPos, level = currentLevel - 1 });
                else if (currentStairType == StairType.Bottom) //Bottom -> Stair (Increase)
                    _frontier.Enqueue(new TileInfo { position = neighborPos, level = currentLevel + 1 });
                else {
                    Debug.Log(neighborTile.name + "," + groundTilemap.GetTile(currentPos).name);
                    Debug.Log("Too many stairs");
                    return;
                }
            }
        }
    }

    StairType TileNameToType(string tileName) {
        if (!tileName.StartsWith(stairsPrefix)) return StairType.None;

        if (int.TryParse(tileName.Substring(stairsPrefix.Length), out int number)) {
            if (number == 24 || number == 25) return StairType.Bottom;
            if (number == 14 || number == 15) return StairType.Middle;
            if (number == 4 || number == 5) return StairType.Top;
        }
        return StairType.None;
    }


    void PropagateWalls() {
        BoundsInt bounds = wallTilemap.cellBounds;

        foreach (Vector3Int cellPosition in bounds.allPositionsWithin) {
            TileBase wallTile = wallTilemap.GetTile(cellPosition);
            if (wallTile == null) continue;
            wallHeightMap[cellPosition] = SetAboveWall(new Vector3Int(cellPosition.x, cellPosition.y + 1, cellPosition.z));
        }
    }

    // Returns height of above wall
    float SetAboveWall(Vector3Int currentPosition) {
        TileBase aboveWallTile = wallTilemap.GetTile(currentPosition);
        bool IsTopOfWall = aboveWallTile == null;
        if (IsTopOfWall) {
            if (!groundHeightMap.ContainsKey(currentPosition)) {
                Vector3Int belowPosition = new Vector3Int(currentPosition.x, currentPosition.y - 1, currentPosition.z);
                if (!groundHeightMap.ContainsKey(belowPosition)) {
                    //TODO: Fix, this shouldn't be called
                    //Debug.Log("Some wall has not ground above it");
                    return 2;
                }
                    
                return groundHeightMap[belowPosition] + 0.5f;
            }

            return groundHeightMap[currentPosition]+0.5f;
        } else {
            Vector3Int abovePosition = new Vector3Int(currentPosition.x, currentPosition.y + 1, currentPosition.z);
            // If above is already calculated - Dynamic Programming
            if (wallHeightMap.ContainsKey(abovePosition)) {
                wallHeightMap[currentPosition] = wallHeightMap[abovePosition];
            }// Else recurse to wall above
            float WallHeight = SetAboveWall(abovePosition);
            wallHeightMap[abovePosition] = WallHeight;
            return WallHeight;
        }
    }
}
