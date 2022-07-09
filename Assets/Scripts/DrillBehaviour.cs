using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillBehaviour : MonoBehaviour
{
    Vector2 poi;
    Vector2 dir;
    public GameObject dot;
    public GameObject circlePrefab;
    public float speed;
    public TileMapBehaviour tmapBehav;
    
    List<GameObject> drilledAreaFrame;
    GameObject[] _dots;
    GameObject _dot;

    public PathT Path { get; set; } 

    public void Draw()
    {
        for(int i = 0; i < 30; i++)
        {
            Destroy(_dots[i]);
            _dots[i] = Instantiate(circlePrefab, Path._points[i].position, Quaternion.identity);
            _dots[i].GetComponent<SpriteRenderer>().sortingOrder = 5;
        }
    }
    public void DrawF() 
    {
        for (int i = 0; i < 30; i++)
        {
            Destroy(_dots[i]);
            _dots[i] = Instantiate(circlePrefab, new Vector2((15-i)/4.0f, Path.Detour.Function((15-i))/1000.0f), Quaternion.identity);
            _dots[i].GetComponent<SpriteRenderer>().sortingOrder = 5;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        dot = Instantiate(circlePrefab, poi, Quaternion.identity);
        dot.GetComponent<SpriteRenderer>().sortingOrder = 5;
        Path = new PathT(transform.position, transform.rotation, 30, tmapBehav, Time.fixedDeltaTime);
        _dots = new GameObject[30];
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            poi = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dot.transform.position = poi;
            dir = new Vector2(Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad), -Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad));
            Path.Detour = new CircleDetour(transform.position, dir, poi, 2.0f, speed, tmapBehav.descendingSpeed, tmapBehav.velDir.x, tmapBehav);
            Path.Detour = new StraightDetour(transform.position, poi, tmapBehav.descendingSpeed, 4.0f);
            Path.Detour.CorrectEndPos();
        }
        Draw();
        // DrawF();
    }

    void FixedUpdate()
    {
        Point newPoint = Path.Move();
        transform.position = newPoint.position;
        transform.rotation = newPoint.rotation;
    }
}
