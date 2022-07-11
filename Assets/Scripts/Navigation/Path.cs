
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class PathT
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
    public bool markedForInit;
    
    public DetourBase Detour
    {
        get { return _detour; }
        set 
        { 
            _detour = value;
            _onDetour = true; 
            _currDetourDistance = 0; 
            markedForInit = true;
        }
    }

    // void Brake(int strength);

    public PathT(Vector2 initPos, Quaternion initRot, int lookAhead, TileMapBehaviour worldInfo, float timeStep)
    {
        _lookAhead = lookAhead;
        _currPoint = 0;
        _points = new Point[lookAhead];
        _worldInfo = worldInfo;
        _timeStep = timeStep;
        _onDetour = false;
        // Detour = new CircleDetour(Vector2.up, Vector2.up, Vector2.up, 2.0f, 4.0f, 2.0f, 0.0f, worldInfo);
        Detour = new StraightDetour(Vector2.zero, Vector2.zero, 0.0f, 0.0f);
        InitPoints(initPos, initRot);
    }

    public void MakeADetour(Vector2 target)
    {
        Point currPoint = _points[_currPoint];
        Vector2 dir = new Vector2(Mathf.Sin(currPoint.rotation.eulerAngles.z * Mathf.Deg2Rad), -Mathf.Cos(currPoint.rotation.eulerAngles.z * Mathf.Deg2Rad));
        Vector2 pos = currPoint.position;
        float speed = 4.0f;
        float radius = 2.0f;
        // Detour = new StraightDetour(pos, target, _worldInfo.descendingSpeed, speed);
        Detour = new CircleDetour(pos, dir, target, radius, speed, _worldInfo.descendingSpeed, _worldInfo.velDir.x, _worldInfo);
        Detour.CorrectEndPos();
        _onDetour = true;
        _currDetourDistance = 0;
        markedForInit = true;
    }

    void InitPoints(Vector2 initPos, Quaternion initRot)
    {
        float initMaxSpeed = (_onDetour) ? Detour.Vdrill : _worldInfo.descendingSpeed;
        float initdS = initMaxSpeed * _timeStep;
        _points[_currPoint] = new Point(initPos, initRot, initMaxSpeed, initdS);
        _distanceAhead = initdS;
        for (int i = 0; i < _lookAhead-1; i++)
        {
            int aheadOf = (_currPoint + i    ) % _lookAhead;
            int next    = (_currPoint + i + 1) % _lookAhead;
            _points[next] = NextPoint(aheadOf, false);
            _distanceAhead += _points[next].deltaS; 
        }
        Debug.Log("Init: InitPos: " + initPos + ", initdS: " + initdS + ", _distAhead: " + _distanceAhead + ", _currDetourDist: " + _currDetourDistance + ", init speed: " + initMaxSpeed);
    }

    public Point Move()
    {
        int next = (_currPoint + 1) % _lookAhead;
        for (int i = 0; i < _lookAhead; i++)
        {
            _points[i].position += Vector2.up * _worldInfo.descendingSpeed * _timeStep;
        }
        if (_onDetour)
        {
            Detour.Delta += Vector2.up * _worldInfo.descendingSpeed * _timeStep;
            if(_currDetourDistance > _detour.GetLength())
            {
                _onDetour = false;
                _currDetourDistance = 0;
            }
        }
        if (markedForInit)
        {
            InitPoints(_points[_currPoint].position, _points[_currPoint].rotation);
            markedForInit = false;
        }
        Point pointToReturn = _points[next];
        _distanceAhead -= pointToReturn.deltaS;
        _points[_currPoint] = NextPoint();
        _distanceAhead += _points[_currPoint].deltaS;
        _currPoint = next;

        Debug.Log("Moving to: " + pointToReturn.position + ", at speed: " + pointToReturn.speed + ", covering distance: " + pointToReturn.deltaS + ", currentPoint: " + _currPoint + ", detourDelta: " + Detour.Delta + ", DetourDist: " + _currDetourDistance + ", length: " + Detour.GetLength());
        return pointToReturn;
    }

    Point NextPoint(int aheadOf = -1, bool changeCurrDetourDist = true)
    {
        if (aheadOf == -1) { aheadOf = (_lookAhead + _currPoint - 1) % _lookAhead; }
        if (_onDetour)
        {
            if (changeCurrDetourDist)
            {
                _currDetourDistance += _points[_currPoint].deltaS; 
            }
            if(_currDetourDistance + _distanceAhead < Detour.GetLength())
            {
                return NextDetourPoint(changeCurrDetourDist);
            }
        }
        return NextIdlePoint(aheadOf);
    }

    Point NextDetourPoint(bool changeCurrDetourDist)
    {
        Vector2    nextPos = Detour.GetPoint   (_currDetourDistance + _distanceAhead) + Detour.Delta;
        Quaternion nextRot = Detour.GetRotation(_currDetourDistance + _distanceAhead);
        float nextSpeed = Detour.Vdrill; // * _detour._brakeVal;
        float nextdS = nextSpeed * _timeStep;
        return new Point(nextPos, nextRot, nextSpeed, nextdS);
    }

    Point NextIdlePoint(int aheadOf)
    {
        Debug.Log("in idle");
        Vector2    nextPos = _points[aheadOf].position + Vector2.down * _points[aheadOf].speed * _timeStep;
        Quaternion nextRot = Quaternion.identity;
        float nextSpeed = _worldInfo.descendingSpeed; // _worldInfo.GetMaxSpeed(nextPos);
        float nextdS = nextSpeed * _timeStep;
         return new Point(nextPos, nextRot, nextSpeed, nextdS);
    }
    public float GetVRatio()
    {
        return _points[_currPoint].speed/_worldInfo.descendingSpeed;
    }
}
