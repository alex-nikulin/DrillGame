using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class TileMapBehaviour : MonoBehaviour
{
    class TilemapManager 
    {
        Tilemap mainTilemap;
        Tilemap  secTilemap;
        Transform[] tmapsPos;
        Tilemap[] tilemapsFG;
        Tilemap[] tilemapsBG;
        SpriteMask[,] masks;
        Texture2D[,] textures;
        Sprite[,] sprites;
        Color[] colors;
        Color[] playerColors;
        Texture2D playerTex;
        int amountOfChunks;
        int[,] masksToChange;
        Vector3 deltaPos;
        Tilemap tmapPrefab;
        SpriteMask maskPrefab;
        Grid grid;
        Tile dirtTile, dirtBGTile;
        Tile rockTile, rockBGTile;
        Color defaultColor;

        List<SpriteMask> smallMasks;

        int width, height;
        int threshold, numIter;

        public TilemapManager(int threshold_, int numIter_, Vector3Int tmapSize, Tile dirt, Tile dirtBG, Tile rock, Tile rockBG, Tilemap prefab, SpriteMask maskfab, Grid parent) {
            width  = tmapSize.x;
            height = tmapSize.y;
            tmapPrefab = prefab;
            maskPrefab = maskfab;
            dirtTile = dirt;
            dirtBGTile = dirtBG;
            rockTile = rock;
            rockBGTile = rockBG;
            grid = parent;
            threshold = threshold_;
            numIter = numIter_;
            deltaPos = new Vector2(0.0f, 0.0f);
            amountOfChunks = 6;
            tilemapsBG = new    Tilemap[amountOfChunks];
            tilemapsFG = new    Tilemap[amountOfChunks];
            masks      = new SpriteMask[amountOfChunks, 4];
            textures   = new  Texture2D[amountOfChunks, 4];
            sprites    = new     Sprite[amountOfChunks, 4];
            defaultColor = new Color(0, 0, 0, 0);
            smallMasks = new List<SpriteMask>();

            for(int i = 0; i < 2; i++)
            {
                for(int j = -1; j < 2; j++) 
                {
                    Vector2 newPos = Vector3.down * grid.cellSize.y * height * i + Vector3.right * grid.cellSize.x * width * j;
                    tilemapsBG[i*3+j+1] = Instantiate(tmapPrefab, newPos, Quaternion.identity, grid.GetComponent<Transform>());
                    tilemapsFG[i*3+j+1] = Instantiate(tmapPrefab, newPos, Quaternion.identity, grid.GetComponent<Transform>());
                    UpdateMapFGBG(tilemapsFG[i*3+j+1], tilemapsBG[i*3+j+1]);
                }
            }
        }
        //=============================================
        //Terrain Generation

        // beautiful mistake;
        int[,] MazeAlgorithm()
        {
            int[,] map = new int[width+2, height+2];
            // tilemap.tag = "fg_tile";
            for (int x = 0; x < width+2; x++)
            {
                for (int y = 0; y < height+2; y++)
                {   
                    if (x==0 ^ y==0 ^ x==width+1 ^ y==height+1)
                    {
                        map[x,y] = 1;
                        continue;
                    }
                    map[x,y] = (Random.Range(1, 11) > 5) ? 1 : 0;
                }
            }
            for (int i = 0; i < numIter; i++)
            {
                for (int x = 1; x < width+1; x++)
                {
                    for (int y = 1; y < height+1; y++)
                    {
                        int sum = 0;
                        sum += map[x-1,y  ]; sum += map[x+1,y  ];
                        sum += map[x,  y-1]; sum += map[x,  y+1];
                        sum += map[x-1,y-1]; sum += map[x+1,y+1];
                        sum += map[x-1,y+1]; sum += map[x+1,y-1];
                        if (sum >= threshold) {map[x,y] = 0;}
                    }
                }
                for (int x = 1; x < width; x++)
                {
                    for (int y = 1; y < height; y++)
                    {
                        int sum = 0;
                        sum += map[x-1,y  ]; sum += map[x+1,y  ];
                        sum += map[x,  y-1]; sum += map[x,  y+1];
                        sum += map[x-1,y-1]; sum += map[x+1,y+1];
                        sum += map[x-1,y+1]; sum += map[x+1,y-1];
                        if (sum < threshold) {map[x,y] = 1;}
                    }
                }
            }
            return map;
        }
        void Swap<T>(ref T a, ref T b)
        {
            T c = a;
            a = b;
            b = c;
        }
        // terrain generation algorhitm
        int[,] GenerateMapWithBlend()
        {
            int[,] map    = new int[width+2, height+2];
            int[,] newMap = new int[width+2, height+2];
            // tilemap.tag = "fg_tile";
            for (int x = 0; x < width+2; x++)
            {
                for (int y = 0; y < height+2; y++)
                {   
                    if (x==0 ^ y==0 ^ x==width+1 ^ y==height+1)
                    {
                        map[x,y] = 1;
                        continue;
                    }
                    map[x,y] = (Random.Range(1, 11) > 5) ? 1 : 0;
                }
            }
            for (int i = 0; i < numIter; i++)
            {
                for (int x = 1; x < width+1; x++)
                {
                    for (int y = 1; y < height+1; y++)
                    {
                        int sum = 0;
                        sum += map[x-1,y  ]; sum += map[x+1,y  ];
                        sum += map[x,  y-1]; sum += map[x,  y+1];
                        sum += map[x-1,y-1]; sum += map[x+1,y+1];
                        sum += map[x-1,y+1]; sum += map[x+1,y-1];
                        if (sum >= threshold) {newMap[x,y] = 1;}
                    }
                }
                for (int x = 1; x < width+1; x++)
                {
                    for (int y = 1; y < height+1; y++)
                    {
                        int sum = 0;
                        sum += map[x-1,y  ]; sum += map[x+1,y  ];
                        sum += map[x,  y-1]; sum += map[x,  y+1];
                        sum += map[x-1,y-1]; sum += map[x+1,y+1];
                        sum += map[x-1,y+1]; sum += map[x+1,y-1];
                        if (sum < threshold) {newMap[x,y] = 0;}
                    }
                }
                (map, newMap) = (newMap, map);
                //Swap(ref map, ref newMap);
            }
            return map;
        }
        // generate map and turn into tilemap
        void UpdateMapFGBG(Tilemap tilemapFG, Tilemap tilemapBG)
        {
            tilemapFG.tag = "fg_tile"; tilemapBG.tag = "bg_tile";
            TilemapRenderer rend = tilemapFG.GetComponent<TilemapRenderer>();
            rend.sortingOrder = 0;
            rend = tilemapBG.GetComponent<TilemapRenderer>();
            rend.sortingOrder = 1;
            rend.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            int[,] map = GenerateMapWithBlend();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {   
                    if (map[x+1,y+1] == 1) 
                    {
                        tilemapFG.SetTile(new Vector3Int(x - width / 2, y - height / 2, 0),  dirtTile);
                        tilemapBG.SetTile(new Vector3Int(x - width / 2, y - height / 2, 0),  dirtBGTile);
                    }
                    else
                    {
                        tilemapFG.SetTile(new Vector3Int(x - width / 2, y - height / 2, 0),  rockTile);
                        tilemapBG.SetTile(new Vector3Int(x - width / 2, y - height / 2, 0),  rockBGTile);
                    }
                }
            }
        }
        // for updating parameters from unity editor
        public void SetGenerationParams(int thrshld, int nmTr)
        {
            threshold = thrshld;
            numIter = nmTr;
        }

        //=============================================
        //Leaving Trace behind Drill

        // create circle mask
        public void MaskPath(Vector2 pos) {
            if (deltaPos.magnitude > 0.25f)
            {
                deltaPos = new Vector2(0.0f, 0.0f);
                smallMasks.Add(Instantiate(maskPrefab, pos, Quaternion.identity));
            }
        }
        // destroy a mask out of screen
        public void DestroyOneMask(float upperBorder)
        {
            for (int i = 0; i < smallMasks.Count; i++) {
                if (smallMasks[i] != null & smallMasks[i].transform.position.y > upperBorder) {
                    Destroy(smallMasks[i].gameObject);
                    smallMasks.RemoveAt(i);
                }
            }
        }

        //=============================================
        //Tilemaps Managment
        
        // moves foreground and background tilemaps to a new position newPos and generates new terrain
        public void ReplaceMap(int i, Vector2 newPos) {
            //newPos = Vector3.down * grid.cellSize.y * System.Math.Abs(tilemapsBG[i].transform.position.y - 2 * height);
            tilemapsBG[i].transform.position = newPos;
            tilemapsFG[i].transform.position = newPos;
            UpdateMapFGBG(tilemapsFG[i], tilemapsBG[i]);
        }
        // chooses when and which maps to move to new position
        public void UpdateMaps() {
            for(int i = 0; i < amountOfChunks; i++)
            {
                if (tilemapsBG[i].transform.position.x >   2 * width * grid.cellSize.x)
                {
                    ReplaceMap(i, tilemapsBG[i].transform.position - new Vector3(3 * width, 0.0f, 0.0f)); 
                }
                else if (tilemapsBG[i].transform.position.x < - 2 * width * grid.cellSize.x)
                {
                    ReplaceMap(i, tilemapsBG[i].transform.position + new Vector3(3 * width, 0.0f, 0.0f)); 
                }
                else if (tilemapsBG[i].transform.position.y > height * grid.cellSize.y)
                {
                    ReplaceMap(i, tilemapsBG[i].transform.position - new Vector3(0.0f, 2 * height, 0.0f));
                }
            }
        }
        // moves all maps according to Vdesc
        public void MoveAllMaps(float deltaTime, Vector2 Vdesc, Vector2 Vdrill) {
            for(int i = 0; i < amountOfChunks; i++)
            {
                tilemapsBG[i].transform.position += (Vector3) Vdesc * deltaTime;
                tilemapsFG[i].transform.position += (Vector3) Vdesc * deltaTime;
                // for (int k = 0; k < 4; k++) {
                //     masks[i,k].transform.position += (Vector3) Vdesc * deltaTime;
                // }
            }
            foreach (SpriteMask mask in smallMasks) 
            {
                mask.transform.position += (Vector3) Vdesc * deltaTime;
            }

            deltaPos += (Vector3) Vdrill * deltaTime;
        }
        // change x component of velDir, which defines movement of tiles
        public Vector2 ManageSideMotion(Vector3 playerPos, Camera cam, Vector2 velDir, float deltaT, float softBound, float hardBound) {
            float x  = playerPos.x - cam.transform.position.x;
            float Bs = cam.orthographicSize * cam.aspect * (1-softBound) * Mathf.Sign(x);
            float Bh = cam.orthographicSize * cam.aspect * (1-hardBound) * Mathf.Sign(x);
            if (Mathf.Abs(x) > Mathf.Abs(Bs)) {
                velDir.x = ((x-Bs+1.0f*Mathf.Sign(x))*(x-Bs)/(Bh-Bs)/(Bh-Bs) + 0.1f) * Mathf.Sign(-x);
            } else {velDir.x = 0;}
            return velDir;
        }
        // return index of closest map to playerPos
        public int ClosestMap(Vector3 playerPos) {
            float dist = 50;
            int output = 0;
            for (int i = 0; i < amountOfChunks; i++) {
                if ((playerPos - tilemapsFG[i].transform.position).magnitude < dist) {dist = (playerPos - tilemapsFG[i].transform.position).magnitude; output = i;}
            }
            return output;
        }
        // returns 1 if tile under playerPos is CompareTo, otherwise 0
        public int GetCurrentTile(Vector3 playerPos, Tile compareTo){
            int i = ClosestMap(playerPos);
            Vector3Int tileCoords = tilemapsFG[i].WorldToCell(playerPos);
            if(tilemapsFG[i].GetTile(tileCoords) == compareTo){
                return 1;
            }
            return 0;
        }
    }

    public Grid grid;
    public Tilemap tmapPrefab;
    public SpriteMask maskPrefab;
    public DrillBehaviour playerDrill;
    public GameObject circlePrefab;

    public Tile dirtTile;
    public Tile rockTile;
    public Tile rockBGTile;
    public Tile dirtBGTile;

    public Vector3Int tmapSize;
    int amountOfChunks;
    public float descendingSpeed;
    public Vector3 velDir = Vector3.up;

    public float softBound;
    public float hardBound;
    public Camera cam;

    public int threshold;
    public int numberOfIterations;
    TilemapManager tmapMgr;

    ParticleSystem psOld, psNew;

    public int CheckCurrentTile(Vector3 pos) {
        return tmapMgr.GetCurrentTile(pos, rockTile);
    }
    public float GetMaxSpeed(Vector2 pos)
    {
        return 1.0f;
    }
    void Awake() {
        tmapMgr = new TilemapManager(threshold, numberOfIterations, tmapSize, dirtTile, dirtBGTile, rockTile, rockBGTile, tmapPrefab, maskPrefab, grid);
    }
    void Update() {
        tmapMgr.MoveAllMaps(Time.deltaTime, new Vector3(velDir.x, velDir.y * descendingSpeed, 0.0f), new Vector2(2,0));
        tmapMgr.UpdateMaps();
        playerDrill.dot.transform.position += new Vector3(velDir.x * Time.deltaTime, 0.0f, 0.0f);
        tmapMgr.SetGenerationParams(threshold, numberOfIterations);
    }
    void LateUpdate() {
        tmapMgr.MaskPath(playerDrill.transform.position);
        tmapMgr.DestroyOneMask(cam.orthographicSize);
        // velDir = tmapMgr.ManageSideMotion(playerDrill.transform.position, cam, velDir, Time.deltaTime, softBound, hardBound);
    }
}
