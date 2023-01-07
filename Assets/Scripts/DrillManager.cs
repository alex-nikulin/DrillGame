using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillManager : MonoBehaviour
{
    public GameObject _drillPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI() 
    {
        if (GUI.Button(new Rect(10, 10, 150, 100), "Spawn drill"))
        {
            Instantiate(_drillPrefab, transform);
        }    
    }
}
