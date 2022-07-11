using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DetourBase
{
    protected Vector2 _startPos;
    protected Vector2 _endPos, _originalEndPos;
    protected bool _hasArrived;
    protected float _Vdrill, _Vdesc;
    public abstract Vector2 GetPoint(float distance);
    public abstract Quaternion GetRotation(float distance);
    public abstract float GetLength();
    public abstract float Function(float deltaY);
    public abstract void DefinePath();
    public float Vdrill 
    { 
        get
        {
            return _Vdrill;
        } 
    }
    public float Vdesc 
    { 
        get
        {
            return _Vdesc;
        } 
    }
    public Vector2 Delta { get; set; }
    public DetourBase(Vector2 startPos, Vector2 endPos, float Vdesc, float Vdrill)
    {
        _startPos = startPos; 
        _endPos = endPos;
        _originalEndPos = endPos;
        _Vdesc  = Vdesc;
        _Vdrill = Vdrill;
    }
    public void CorrectEndPos()
    {
        float deltaY = 0;
        float diffStep = 0.01f;
        float cPathL1, cPathL2;
        for (int i = 0; i < 4; i++) 
        {
            cPathL2 = Function(deltaY-diffStep);
            cPathL1 = Function(deltaY);
            if((cPathL2 - cPathL1) == 0) {break;}
            deltaY += cPathL1 * diffStep / (cPathL2 - cPathL1);
            Debug.Log(i+" - answer: " + deltaY + ", accuracy: " + Function(deltaY));
        }
        _endPos = new Vector2(_endPos.x, _endPos.y - deltaY);
        DefinePath();
        float time = GetTime(20);
        float time_old = GetLength()/_Vdrill; 
        float differY = Mathf.Max(0.0f, _originalEndPos.y - _endPos.y);
        _Vdrill = Mathf.Min(_Vdrill, GetLength() * _Vdesc / Mathf.Abs(_originalEndPos.y - _endPos.y));
        // Debug.Log("final answer: " + deltaY + ", accuracy: " + Function(deltaY));
        
    }
    public float GetTime(int acc) 
    {
        float length = GetLength();
        float time = 0;
        float step = 0;
        float deltaS = 0;
        Vector2 pos1, pos2;
        pos1 = GetPoint(0);
        pos2 = GetPoint(0);
        for (int i = 1; i < acc; i++)
        {
            pos2 = GetPoint(length*i/(acc-1.0f));
            step = (pos2-pos1).magnitude;
            deltaS += step;
            time += step/(_Vdrill);
            pos1 = pos2;
        }
        return time;
    }

}
