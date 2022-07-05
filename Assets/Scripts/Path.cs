using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DetourBase
{
    protected Vector2 _startPos;
    protected Vector2 _startDir;
    protected Vector2 _endPos;
    protected bool _hasArrived;
    protected float _Vdrill;
    protected float _baseSpeed;
    public float _brakeVal;
    public abstract Vector2 GetPoint(float distance);
    public abstract Quaternion GetRotation(float distance);
    public abstract float GetLength();
    public abstract float Function(float deltaY); //remove
    public float Vdrill 
    { 
        get
        {
            return _Vdrill;
        } 
    }

    public DetourBase(Vector2 startPos, Vector2 startDir, Vector2 endPos)
    {
        _startPos = startPos; 
        _startDir = startDir;
        _startDir.Normalize();
        _endPos = endPos;
    }
}

public struct Point
{
    public Vector2 position;
    public Quaternion rotation;
    public float speed;
    public float deltaS;
    public float brakeVal;

    public Point(Vector2 pos, Quaternion rot, float sp, float dS)
    {
        position = pos;
        rotation = rot;
        speed = sp;
        deltaS = dS;
        brakeVal = 1;
    }
}

public class Path
{
    DetourBase _detour;
    bool _onDetour;
    float _currDetourDistance;
    float _distanceAhead;
    public Point[] _points;
    int _lookAhead;
    int _currPoint;
    TileMapBehaviour _worldInfo;
    float _timeStep;
    
    public DetourBase Detour
    {
        get { return _detour; }
        set 
        { 
            _detour = value;
            _onDetour = true; 
            _currDetourDistance = 0; 
            InitPoints(_points[_currPoint].position, _points[_currPoint].rotation);
            Debug.Log("going onDetour!");
        }
    }

    // void Brake(int strength);

    public Path(Vector2 initPos, Quaternion initRot, int lookAhead, TileMapBehaviour worldInfo, float timeStep)
    {
        _lookAhead = lookAhead;
        _currPoint = 0;
        _points = new Point[lookAhead];
        _worldInfo = worldInfo;
        _timeStep = timeStep;
        _onDetour = false;
        InitPoints(initPos, initRot);
    }


    void InitPoints(Vector2 initPos, Quaternion initRot)
    {
        Debug.Log("Init");
        float initMaxSpeed = _worldInfo.GetMaxSpeed(initPos);
        float initdS = initMaxSpeed * _timeStep;
        _points[_currPoint] = new Point(initPos, initRot, initMaxSpeed, initdS);
        _distanceAhead = initdS;
        for (int i = 0; i < _lookAhead-1; i++)
        {
            int current = (_currPoint + i    ) % _lookAhead;
            int next    = (_currPoint + i + 1) % _lookAhead;
            _points[next] = NextPoint(current, false);
        }
    }

    public Point Move()
    {
        for (int i = 0; i < _lookAhead; i++)
        {
            _points[i].position += Vector2.up * _worldInfo.descendingSpeed * _timeStep;
        }
        Point pointToReturn = _points[_currPoint];
        _points[_currPoint] = NextPoint();
        _distanceAhead -= pointToReturn.deltaS;
        _currPoint = (_currPoint + 1) % _lookAhead;
        // Debug.Log(_distanceAhead + " " + _currDetourDistance + " " + _detour.GetLength());
        if (_onDetour)
        {
            if(_currDetourDistance > _detour.GetLength())
            {
                _onDetour = false;
                _currDetourDistance = 0;
                Debug.Log("back to idle(");
            }
        }
        return pointToReturn;
    }

    Point NextPoint(int aheadOf = -1, bool changeCurrDetourDist = true)
    {
        if (aheadOf == -1) { aheadOf = (_lookAhead + _currPoint - 1) % _lookAhead; }
        if (_onDetour)
        {   
            if(_currDetourDistance + _distanceAhead < Detour.GetLength())
            {
                return NextDetourPoint(changeCurrDetourDist);
            }
        }
        return NextIdlePoint(aheadOf);
    }

    Point NextDetourPoint(bool changeCurrDetourDist)
    {
        Vector2    nextPos = Detour.GetPoint   (_currDetourDistance + _distanceAhead);
        Quaternion nextRot = Detour.GetRotation(_currDetourDistance + _distanceAhead);
        float nextSpeed = Detour.Vdrill * _worldInfo.GetMaxSpeed(nextPos);// * _detour._brakeVal;
        float nextdS = nextSpeed * _timeStep;
        _distanceAhead += nextdS;
        if (changeCurrDetourDist)
        {
            _currDetourDistance += _points[_currPoint].deltaS; 
        }
        return new Point(nextPos, nextRot, nextSpeed, nextdS);
    }

    Point NextIdlePoint(int aheadOf)
    {
        Vector2    nextPos = _points[aheadOf].position + Vector2.down * _points[aheadOf].speed * _timeStep;
        Quaternion nextRot = _points[aheadOf].rotation;
        float nextSpeed = _worldInfo.GetMaxSpeed(nextPos);
        float nextdS = nextSpeed * _timeStep;
        _distanceAhead += nextdS;
        return new Point(nextPos, nextRot, nextSpeed, nextdS);
    }
}
