using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

public class PlayerDrillBehaviour : MonoBehaviour
{
    CircleDetour cPath;
    Vector2 poi;
    Vector2 deltaPos;
    public GameObject dot;
    public GameObject circlePrefab;
    public float speed;
    public TileMapBehaviour tmapBehav;
    bool hasArrived = true;
    ParticleSystem.Particle[] particles;
    Vector2 dir;
    int prevParticle;
    
    List<GameObject> drilledAreaFrame;

    public Vector2 GetVdrill() 
    {
        return cPath.GetVdrill() * dir;
    }

    // Start is called before the first frame update
    void Start()
    {
        prevParticle = 3;
        dot = Instantiate(circlePrefab, poi, Quaternion.identity);
        dot.GetComponent<SpriteRenderer>().sortingOrder = 5;
        // cPath = new CircleDetour(transform.position, Vector2.down, new Vector2(0,1), 0.5f, speed, tmapBehav.descendingSpeed, tmapBehav.velDir.x);
        deltaPos = new Vector2(0, 0);
        var PSList = GetComponentsInChildren<ParticleSystem>();
        particles = new ParticleSystem.Particle[256];
        drilledAreaFrame = new List<GameObject>();
    }
    // Update is called once per frame
    void Update()
    {
        // cPath.SetSideSpeed(tmapBehav.velDir.x);
        // if (Input.GetMouseButton(0))
        // {
        //     poi = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //     dot.transform.position = poi;
        //     hasArrived = false;
        //     if (((Vector3)poi - transform.position).magnitude != 0) {
        //         dir = new Vector2(Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad), -Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad));
        //         cPath = new CircleDetour(transform.position, dir, poi, 2.0f, speed, tmapBehav.descendingSpeed, tmapBehav.velDir.x);
        //         cPath.CorrectEndPos();
        //     }
        // }
        // if (!hasArrived)
        // {
        //     float radius = cPath.GetRadius();
        //     cPath.SetSideSpeed(tmapBehav.velDir.x);
        //     Vector2 newPos = cPath.Move(Time.deltaTime);
        //     Quaternion newRot = cPath.GetCurrentRotation();
        //     transform.rotation = newRot;
        //     transform.position = newPos;
        //     if (cPath.Arrived())
        //     {
        //         transform.rotation = Quaternion.FromToRotation(new Vector2(0, -1), new Vector2(0, -1)); 
        //         hasArrived = true;
        //     }
        // }
        // else {
        //     transform.position += new Vector3(tmapBehav.velDir.x * Time.deltaTime, 0.0f, 0.0f);
        // }
        // dir = new Vector2(Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad), -Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad));
    }
}
