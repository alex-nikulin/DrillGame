using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DetourBase
{
    protected Vector2 _startPos;
    protected Vector2 _endPos, _originalEndPos;
    protected bool _hasArrived;
    protected float _Sstart, _Sfinal, _Scrit, _accel;
    protected float _Vdrill, _Vdesc, _Vstart, _Vfinal;
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
    public DetourBase(Vector2 startPos, Vector2 endPos, float Vdesc, float Vdrill, float Vcurr, float Vidle, float accel)
    {
        _startPos = startPos; 
        _endPos = endPos;
        _originalEndPos = endPos;
        _Vdesc  = Vdesc;
        _Vdrill = Vdrill;
        _Vstart = Vcurr;
        _Vfinal = Vidle;
        _accel  = accel;
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
        CalcAccel();
    // Debug.Log("final answer: " + deltaY + ", accuracy: " + Function(deltaY));
        
    }
    public void CalcAccel()
    {
        _Sstart = (_Vdrill - _Vstart) / (_accel * Mathf.Sign(_Vdrill - _Vstart));
        _Sfinal = (_Vdrill - _Vfinal) / (_accel * Mathf.Sign(_Vfinal - _Vdrill)) + GetLength();
        _Scrit  = (_Vfinal - _Vstart) / (_accel * Mathf.Sign(_Vdrill - _Vstart));
        Debug.Log("GetLength: " + GetLength() + ", S start: " + _Sstart + ", S final: " + _Sfinal + ", V init: " + _Vstart + ", V final: " + _Vfinal + ", Vdrill: " + _Vdrill);
    }
    public float GetSpeed(float distance)
    {
        if (_Sstart > _Sfinal)
        {
            if (GetLength() < _Scrit)
            {
                return _Vstart + (_Vfinal - _Vstart) * distance / GetLength();
            }
            float f_start = _Vstart + _accel * Mathf.Sign(_Vdrill - _Vstart) * distance;
            float f_final = _Vfinal + _accel * Mathf.Sign(_Vfinal - _Vdrill) * (distance - GetLength());
            if (_Vdrill > _Vstart & _Vdrill > _Vfinal)
            {
                return Mathf.Min(f_start, f_final);
            }
            else if (_Vdrill < _Vstart & _Vdrill < _Vfinal)
            {
                return Mathf.Max(f_start, f_final);
            }
        }
        if (distance < _Sstart)
        {
            return _Vstart + _accel * Mathf.Sign(_Vdrill - _Vstart) * distance;
        }
        else if (distance < _Sfinal)
        {
            return _Vdrill;
        }
        else
        {
            return _Vfinal + _accel * Mathf.Sign(_Vfinal - _Vdrill) * (distance - GetLength());
        }
    }
    public float GetTime(int acc) 
    {
        float length = GetLength();
        float time = 0;
        float step = 0;
        float deltaS = 0;
        Vector2 pos1, pos2;
        pos1 = GetPoint(0);
        pos2 = pos1;
        for (int i = 1; i < acc; i++)
        {
            float S1 = length*(i-1)/(acc-1.0f);
            float S2 = length* i   /(acc-1.0f);
            pos2 = GetPoint(S2);
            float speed = _Vdrill;//GetSpeed(S1);
            step = (pos2-pos1).magnitude;
            deltaS += step;
            time += step/speed;
            pos1 = pos2;
        }
        return time;
    }

}
