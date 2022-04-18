using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class TileMapBehaviour : MonoBehaviour
{
    class TilemapManager {
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

        // beautiful mistake;
        void MazeAlgorithm(int[,] map)
        {
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
                Swap(ref map, ref newMap);
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
        // deprecated
        int[,] MasksToChange(float playerDiag, Vector3 brushPos)
        {
            int[,] output = new int[amountOfChunks, 4];
            for (int i = 0; i < amountOfChunks; i++)
            {
                for (int k = 0; k < 4; k++) {
                    if (System.Math.Abs(masks[i,k].transform.position.y - brushPos.y) < (playerDiag + height/2) / 2 & System.Math.Abs(masks[i,k].transform.position.x - brushPos.x) < (playerDiag + width/2)/2)
                    {
                        output[i,k] = 1;
                    }
                    else {output[i,k] = 0;}
                }
            }
            return output;
        }
        // for updating parameters from unity editor
        public void SetGenerationParams(int thrshld, int nmTr)
        {
            threshold = thrshld;
            numIter = nmTr;
        }
        // deprecated
        public void DrawShape(PlayerDrillBehaviour playerDrill) {
            if (deltaPos.magnitude > 0.125f)
            {
                deltaPos = new Vector2(0.0f, 0.0f);
                Sprite playerSprite = playerDrill.GetComponent<SpriteRenderer>().sprite;
                Rect borders = playerDrill.GetComponent<SpriteRenderer>().sprite.rect;
                Vector2 localPos;
                Vector2Int pixelCoords;
                playerTex = playerDrill.GetComponent<SpriteRenderer>().sprite.texture;
                float playerDiag = (float) System.Math.Sqrt(borders.width * borders.width + borders.height * borders.height) / 32;
                masksToChange = MasksToChange(playerDiag, playerDrill.transform.position);
                int c = 0;
                foreach(int poop in masksToChange) {
                    if (poop==1) {c++;}
                }
                for (int i = 0; i < amountOfChunks; i++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        if (masksToChange[i,k] == 1)
                        {
                            playerColors = playerTex.GetPixels();
                            colors = masks[i,k].sprite.texture.GetPixels();
                            for (int y = 0; y < borders.height; y++)
                            {
                                for (int x = 0; x < borders.width; x++)
                                {
                                    int index = (int) ((borders.y + y) * playerTex.width + borders.x + x);
                                    if (playerColors[index].a != 0)
                                    {
                                        localPos = masks[i,k].transform.InverseTransformPoint(playerDrill.transform.TransformPoint(new Vector2((x + 0.5f - borders.width / 2) / 32.0f, (y + 0.5f - borders.height*0.7f) / 32.0f)));
                                        pixelCoords = new Vector2Int((int)System.Math.Round(masks[i,k].sprite.texture.width / 2 - localPos.x * 32), (int)System.Math.Round(masks[i,k].sprite.texture.height / 2 - localPos.y * 32));
                                        if (pixelCoords.x < masks[i,k].sprite.texture.width & pixelCoords.y < masks[i,k].sprite.texture.height & pixelCoords.x > 0 & pixelCoords.y > 0)
                                        {
                                            colors[(masks[i,k].sprite.texture.height - pixelCoords.y) * masks[i,k].sprite.texture.width + (masks[i,k].sprite.texture.width - pixelCoords.x)] = new Color(1, 1, 1, 1);
                                        }
                                    }
                                }
                            }
                            masks[i,k].sprite.texture.SetPixels(colors);
                            masks[i,k].sprite.texture.Apply(false);
                        }
                    }
                }
            }
        }
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
        // moves foreground and background tilemaps to a new position newPos and generates new terrain
        public void ReplaceMap(int i, Vector2 newPos) {
            //newPos = Vector3.down * grid.cellSize.y * System.Math.Abs(tilemapsBG[i].transform.position.y - 2 * height);
            tilemapsBG[i].transform.position = newPos;
            tilemapsFG[i].transform.position = newPos;
            UpdateMapFGBG(tilemapsFG[i], tilemapsBG[i]);
            // UpdateMap(tilemapsFG[i]);
            // for (int k = 0; k < 4; k++) {
            //     Vector2 deltaK = new Vector3(-width/4.0f + width/2.0f * (k%2), -height/4.0f + height/2.0f * (k/2), 0.0f);
            //     masks[i,k].transform.position = newPos - new Vector2(1/64.0f, 1/64.0f) + deltaK;
            //     colors = textures[i,k].GetPixels();
            //     for (int x = 0; x < colors.Length; x++)
            //     {
            //         colors[x] = defaultColor;
            //     }
            //     textures[i,k].SetPixels(colors);
            //     textures[i,k].Apply(false);
            //     // sprites[i]  = Sprite.Create(textures[i], new Rect(0, 0, width*32, height*32), new Vector2(0.5f, 0.5f), 32.0f);
            //     // masks[i].GetComponent<SpriteMask>().sprite = sprites[i];
            // }
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
        // moves all maps according to 
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
    public PlayerDrillBehaviour playerDrill;
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

    void Start() {
        tmapMgr = new TilemapManager(threshold, numberOfIterations, tmapSize, dirtTile, dirtBGTile, rockTile, rockBGTile, tmapPrefab, maskPrefab, grid);
    }
    void Update() {
        tmapMgr.MoveAllMaps(Time.deltaTime, new Vector3(velDir.x, velDir.y * descendingSpeed, 0.0f), playerDrill.GetVdrill());
        tmapMgr.UpdateMaps();
        playerDrill.dot.transform.position += new Vector3(velDir.x * Time.deltaTime, 0.0f, 0.0f);
        tmapMgr.SetGenerationParams(threshold, numberOfIterations);
    }
    void LateUpdate() {
        tmapMgr.MaskPath(playerDrill.transform.position);
        tmapMgr.DestroyOneMask(cam.orthographicSize);
        velDir = tmapMgr.ManageSideMotion(playerDrill.transform.position, cam, velDir, Time.deltaTime, softBound, hardBound);
    }
}
