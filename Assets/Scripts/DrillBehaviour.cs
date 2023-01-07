using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillBehaviour : MonoBehaviour
{
    Vector2 poi;
    Vector2 dir;
    public GameObject dot;
    public GameObject circlePrefab;
    public float camSpeed;
    public SpriteMask _maskPrefab;
    
    List<GameObject> drilledAreaFrame;
    List<SpriteMask> _smallMasks;
    GameObject[] _dots;
    GameObject _dot;

    float _deltaPos;

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
    public void DrawSpeedF() 
    {
        float l = Path.Detour.GetLength();
        for (int i = 0; i < 30; i++)
        {
            Destroy(_dots[i]);
            _dots[i] = Instantiate(circlePrefab, new Vector2(i/30.0f*4, Path.Detour.GetSpeed(i/30.0f*l)/2.0f), Quaternion.identity);
            _dots[i].GetComponent<SpriteRenderer>().sortingOrder = 5;
        }
    }
    public void DrawF() 
    {
        float l = Path.Detour.GetLength();
        for (int i = 0; i < 30; i++)
        {
            Destroy(_dots[i]);
            _dots[i] = Instantiate(circlePrefab, new Vector2((15-i)/15.0f*5, Path.Detour.Function(15-i)/4.0f), Quaternion.identity);
            _dots[i].GetComponent<SpriteRenderer>().sortingOrder = 5;
        }
    }
    //=============================================
    //Leaving Trace behind Drill

    // create circle mask
    public void MaskPath() {
        if (_deltaPos > 0.20f)
        {
            _deltaPos = 0.0f;
            Vector2 pos = transform.position;
            _smallMasks.Add(Instantiate(_maskPrefab, pos, Quaternion.identity));
        }
    }
    // destroy a mask out of screen
    public void DestroyOneMask(float upperBorder)
    {
        for (int i = 0; i < _smallMasks.Count; i++) {
            if (_smallMasks[i] != null & _smallMasks[i].transform.position.y > upperBorder) {
                Destroy(_smallMasks[i].gameObject);
                _smallMasks.RemoveAt(i);
                break;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _smallMasks = new List<SpriteMask>();
        _deltaPos = 0.0f;
        dot = Instantiate(circlePrefab, poi, Quaternion.identity);
        dot.GetComponent<SpriteRenderer>().sortingOrder = 5;
        Path = new PathT(transform.position, transform.rotation, 30, camSpeed, Time.fixedDeltaTime);
        _dots = new GameObject[30];
    }

    // Update is called once per frame
    void Update()
    {
        float fstart = Time.realtimeSinceStartup;
        if (Input.GetMouseButton(0))
        {
            poi = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dot.transform.position = poi;
            float start = Time.realtimeSinceStartup;
            Path.MakeADetour(poi);
            float end = Time.realtimeSinceStartup;
            Debug.Log("MakeADetour time:" + (end - start));
            Debug.Log("overall time: " + (end - fstart));
        }
        MaskPath();
        DestroyOneMask(Camera.main.orthographicSize+0.5f);
        
        foreach (SpriteMask mask in _smallMasks) 
        {
            mask.transform.position += Vector3.up * camSpeed * Time.deltaTime;
        }
        _deltaPos += Path.GetSpeed() * Time.deltaTime;

    }

    void FixedUpdate()
    {
        Point newPoint = Path.Move();
        transform.position = newPoint.position;
        transform.rotation = newPoint.rotation;
    }
}
