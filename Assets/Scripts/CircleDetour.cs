using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleDetour : DetourBase
{
    Vector2 _center1;
    Vector2 _rad1;
    Vector2 _center2;
    Vector2 _rad2;
    Vector2 _L;
    Vector2 _descendVelocity;
    float _radius;
    bool _clockDir;
    bool _leftwing;
    float alpha1;
    float alpha2;
    float _currentDistance;
    float _timer;
    public float _Vdesc, _Vside;
    Vector2 _Shift;
    Vector2 _originalEndPos;
    TileMapBehaviour _worldInfo;

    //tmp fields
    float _allegedTime, _allegedTime2;

    // Point[] _pointsToCalcTime;
    float _lengthStep;
    int _amountOfSteps;

    public CircleDetour(Vector2 startPos, Vector2 startDir, Vector2 endPos, float radius, float speed, float descendSpeed, float sideSpeed, TileMapBehaviour worldInfo, bool defTurns = false)
        :base(startPos, startDir, endPos)
    {
        // all turns have the same radius
        _radius = radius;
        // since tilemaps move up, path needs to accommodate for movement
        _Shift = new Vector2(0.0f, 0.0f);
        // _Vdrill - maximum magnitude of speed along the path, _Vdesc - speed of descending (tilemaps moving up) is always constant
        // _Vside - current speed of moving side ways, updated every frame
        _Vdrill = speed;
        Delta = new Vector2(0.0f, 0.0f);
        _Vdesc  = descendSpeed;
        _Vside = sideSpeed;
        // distance travelled along the path
        _currentDistance = 0;
        // not used
        _timer = 0;
        _originalEndPos = _endPos;
        _worldInfo = worldInfo;
        _lengthStep = 0.1f;
        if (defTurns) 
        {
            // DefineTurns();
            _amountOfSteps = (int)Mathf.Ceil(GetLength()/_lengthStep);
            // _pointsToCalcTime = new Point[_amountOfSteps];
            CorrectEndPos();
        }
    }
    public Vector2 Rotate(Vector2 v, float rads) 
    {
        float sin = Mathf.Sin(rads);
        float cos = Mathf.Cos(rads);
        
        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }
    public float GetVRatio() 
    {
        if (_hasArrived) {return 1;}
        return _Vdrill / _Vdesc;
    }
    public float GetVdrill() 
    {
        if (_hasArrived) {return _Vdesc;}
        return _Vdrill;
    }
    public float GetRadius() 
    {
        return _radius;
    }
    // update vertical component of endPos
    public void SetEndPos(float deltaY) 
    {
        _endPos += new Vector2(0.0f, deltaY);
        DefineTurns();
    }
    public void SetSideSpeed(float sideSpeed) 
    {
        _Vside = sideSpeed;
    }

    // changes currentDistance and returns new point according to currentDistance
    public Vector2 Move(float deltaTime)
    {
        _Shift += new Vector2(_Vside, _Vdesc) * deltaTime;
        float l = _Vdrill;
        _timer += deltaTime;
        if (GetLength() <= 0.05) 
        {
            if(Mathf.Abs(_originalEndPos.y - _endPos.y) / _Vdesc > _timer) 
            {
                _hasArrived = true;
            }
            return  _endPos;
        }
        else if (_currentDistance + l * deltaTime < GetLength())
        {
            _currentDistance += l * deltaTime;
            return GetPoint(_currentDistance) + _Shift;
        }
        else
        {
            _hasArrived = true;
            return _endPos + _Shift;
        }
    }

    public bool Arrived()
    {
        return _hasArrived;
    }

    // since tilemaps move actual endPos up (endPos in tilemap coordinates) must by way lower than endPos we first recieved (endPos in global coordinates)
    // to calculate this endPos Newton's method is used
    // Function calculates difference between time for drill to travel along path (with endPos moved down by deltaY) and time for tilemaps to move up by deltaY
    // looking for such deltaY: Function(deltaY) == 0
    public override float Function(float deltaY)

    {
        CircleDetour cPathPlusDeltaY = new CircleDetour(_startPos, _startDir, new Vector2(_endPos.x, _endPos.y - deltaY), _radius, _Vdrill, _Vdesc, _Vside, _worldInfo);
        cPathPlusDeltaY.DefineTurns();
        return (deltaY) / _Vdesc - cPathPlusDeltaY.GetLength() / _Vdrill;//cPathPlusDeltaY.GetTime(20);
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
        }
        _endPos = new Vector2(_endPos.x, _endPos.y - deltaY);
        DefineTurns();
        float time = GetTime(20);
        float time_old = GetLength()/_Vdrill; 
        float differY = Mathf.Max(0.0f, _originalEndPos.y - _endPos.y);
        _brakeVal = time/(time + differY/_Vdesc);
        _Vdrill = Mathf.Min(_Vdrill, GetLength() * _Vdesc / Mathf.Abs(_originalEndPos.y - _endPos.y));
        
    }

    public void CorrectEndPos2()
    {
        float leftY = -20;
        float rightY = 100;
        float centralY = 0;
        for (int i = 0; i < 10; i++) 
        {
            centralY = (leftY + rightY)/2;
            float fl = Function(leftY);
            float fr = Function(rightY);
            float fc = Function(centralY);
            if (fc*fl>0) {leftY = centralY;}
            else {rightY = centralY;}
        }
        _endPos = new Vector2(_endPos.x, _endPos.y - centralY);
    }

    // essential function, which returns point, corresponding to arbitrary distance
    public override Vector2 GetPoint(float distance)
    {

        float OG = distance;
        if (_clockDir != _leftwing)
        {
            if (distance <= (alpha1 - alpha2) * _radius)
            {
                return _center1 + Rotate(_rad1, (_clockDir ? 1 : -1) * distance / _radius);
            }
            distance -= (alpha1 - alpha2) * _radius;
            if (distance <= StraightAway().magnitude)
            {
                return _center1 + Rotate(_rad1, (_clockDir ? 1 : -1) * (alpha1 - alpha2)) + _L * distance / _L.magnitude;
            }
            distance -= _L.magnitude;
            if (distance <= alpha2 * _radius)
            {
                return _center2 + Rotate(_rad2, (_leftwing ? 1 : -1) * (alpha2 - distance / _radius));
            }
            return _endPos;
        }
        else
        {
            if (distance <= (alpha1 + alpha2) * _radius)
            {
                return _center1 + Rotate(_rad1, (_clockDir ? 1 : -1) * distance / _radius);
            }
            distance -= (alpha1 + alpha2) * _radius;
            if (distance <= StraightAway().magnitude)
            {
                return _center1 + Rotate(_rad1, (_clockDir ? 1 : -1) * (alpha1 + alpha2)) + StraightAway() * distance / StraightAway().magnitude;
            }
            distance -= StraightAway().magnitude;
            if (distance <= alpha2 * _radius)
            {
                return _center2 + Rotate(_rad2, (_leftwing ? 1 : -1) * (alpha2 - distance / _radius));
            }
            return _endPos;
        }
    }
    public override Quaternion GetRotation(float distance) 
    {
        //float distance = _currentDistance;
        if (_clockDir != _leftwing)
        {
            if (distance <= (alpha1 - alpha2) * _radius)
            {
                if (_clockDir) {return Quaternion.FromToRotation(new Vector2(-1, 0), Rotate(_rad1, (_clockDir ? 1 : -1) * distance / _radius));}
                else           {return Quaternion.FromToRotation(new Vector2( 1, 0), Rotate(_rad1, (_clockDir ? 1 : -1) * distance / _radius));}
            }
            distance -= (alpha1 - alpha2) * _radius;
            if (distance <= StraightAway().magnitude)
            {
                return Quaternion.FromToRotation(new Vector2(0, -1), StraightAway());
            }
            distance -= _L.magnitude;
            if (distance <= alpha2 * _radius)
            {
                if (_leftwing) {return Quaternion.FromToRotation(new Vector2( 1, 0), Rotate(_rad2, (_leftwing ? 1 : -1) * (alpha2 - distance / _radius)));}
                else           {return Quaternion.FromToRotation(new Vector2(-1, 0), Rotate(_rad2, (_leftwing ? 1 : -1) * (alpha2 - distance / _radius)));}
            }
            return Quaternion.FromToRotation(new Vector2(0, -1), new Vector2(0, -1));
        }
        else
        {
            if (distance <= (alpha1 + alpha2) * _radius)
            {
                if (_clockDir) {return Quaternion.FromToRotation(new Vector2(-1, 0), Rotate(_rad1, (_clockDir ? 1 : -1) * distance / _radius));}
                else           {return Quaternion.FromToRotation(new Vector2( 1, 0), Rotate(_rad1, (_clockDir ? 1 : -1) * distance / _radius));}
            }
            distance -= (alpha1 + alpha2) * _radius;
            if (distance <= StraightAway().magnitude)
            {
                return Quaternion.FromToRotation(new Vector2(0, -1), StraightAway());
            }
            distance -= StraightAway().magnitude;
            if (distance <= alpha2 * _radius)
            {
                if (_leftwing) {return Quaternion.FromToRotation(new Vector2( 1, 0), Rotate(_rad2, (_leftwing ? 1 : -1) * (alpha2 - distance / _radius)));}
                else           {return Quaternion.FromToRotation(new Vector2(-1, 0), Rotate(_rad2, (_leftwing ? 1 : -1) * (alpha2 - distance / _radius)));}
            }
            return Quaternion.FromToRotation(new Vector2(0, -1), new Vector2(0, -1));
        }
    }
    // defines centers of circles and angles across these circles, the drill will move
    public void DefineTurns()
    {
        Vector2 radClock  = -_radius * Rotate(_startDir,  Mathf.PI / 2);
        Vector2 radCClock = -_radius * Rotate(_startDir, -Mathf.PI / 2);
        Vector2 center1Clock  = _startPos - radClock;
        Vector2 center1CClock = _startPos - radCClock;
        Vector2 centerCBounds  = center1Clock  + _radius * Vector2.right;
        Vector2 endPosLocal = _endPos - centerCBounds;
        Vector2 centerCCBounds = center1CClock + _radius * Vector2.left;

        if ((endPosLocal.magnitude < _radius * 2 & endPosLocal.x < 0 & endPosLocal.y < 0) ^ (endPosLocal.x < 0 & endPosLocal.x >= -_radius * 2 & endPosLocal.y >= 0))
        {
            _endPos = new Vector2(_endPos.x, centerCBounds.y - Mathf.Sqrt(4 * _radius * _radius - endPosLocal.x * endPosLocal.x));
        }
        else if (endPosLocal.x >= 0 & endPosLocal.y > -2 * _radius)
        {
            _endPos = new Vector2(_endPos.x, centerCBounds.y - 2 * _radius);
        }
        endPosLocal = _endPos - centerCCBounds;
        if ((endPosLocal.magnitude < _radius * 2 & endPosLocal.x > 0 & endPosLocal.y < 0) ^ (endPosLocal.x > 0 & endPosLocal.x <= _radius * 2 & endPosLocal.y >= 0))
        {
            _endPos = new Vector2(_endPos.x, centerCCBounds.y - Mathf.Sqrt(4 * _radius * _radius - endPosLocal.x * endPosLocal.x));
        }
        else if (endPosLocal.x <= 0 & endPosLocal.y > -2 * _radius)
        {
            _endPos = new Vector2(_endPos.x, centerCCBounds.y - 2 * _radius);
        }

        if (Mathf.Abs(center1CClock.x - _endPos.x) <= _radius & Mathf.Abs(center1Clock.x - _endPos.x) <= _radius)
        {
            if (Mathf.Abs(center1CClock.y - _endPos.y) > Mathf.Abs(center1Clock.y - _endPos.y))
            {
                _center1 = center1Clock;
                _rad1 = radClock;
                _clockDir = true;
                _rad2 = _radius * Vector2.right;
                _center2 = _endPos - _rad2;
                _leftwing = true;
            }
            else
            {
                _center1 = center1CClock;
                _rad1 = radCClock;
                _clockDir = false;
                _rad2 = _radius * Vector2.left;
                _center2 = _endPos - _rad2;
                _leftwing = false;
            }
        }

        else if (center1Clock.x - _endPos.x > _radius & center1CClock.x - _endPos.x > -_radius)
        {
            _rad2 = _radius * Vector2.left;
            _center2 = _endPos - _rad2;
            _leftwing = false;
            float c = (_center2     - center1CClock ).magnitude;
            float a = (_center2     - center1Clock).magnitude;
            float b = (center1Clock - center1CClock).magnitude;

            if (c*c < a*a + b*b)
            {
                _center1 = center1CClock;
                _rad1 = radCClock;
                _clockDir = false;
            }
            else
            {
                _center1 = center1Clock;
                _rad1 = radClock;
                _clockDir = true;
            }
        }
        else if (center1Clock.x - _endPos.x < _radius & center1CClock.x - _endPos.x < -_radius)
        {
            _rad2 = _radius * Vector2.right;
            _center2 = _endPos - _rad2;
            _leftwing = true;
            float c = (_center2     - center1Clock ).magnitude;
            float a = (_center2     - center1CClock).magnitude;
            float b = (center1Clock - center1CClock).magnitude;

            if (c*c < a*a + b*b)
            {
                _center1 = center1Clock;
                _rad1 = radClock;
                _clockDir = true;
            }
            else
            {
                _center1 = center1CClock;
                _rad1 = radCClock;
                _clockDir = false;
            }
        }
        _L = _center2 - _center1;
        alpha1 = Mathf.Acos( -_startDir.y / _startDir.magnitude) * ((!_clockDir & _startDir.x < 0 ^ _clockDir & _startDir.x > 0) ? -1 : 1);
        if (_clockDir != _leftwing)
        {
            alpha2 = Mathf.Acos( Mathf.Abs(_L.y) / _L.magnitude);
        }
        else
        {
            alpha2 = Mathf.Acos(Mathf.Max(-1, Mathf.Min(Vector2.Dot(-1 * _L, _rad2) / (_L.magnitude * _rad2.magnitude), 1))) - Mathf.Acos( Mathf.Min(2 * _radius / _L.magnitude, 1));
        }
    }
    public override float GetLength()
    {
        if (_clockDir != _leftwing)
        {
            return alpha1 * _radius + _L.magnitude;
        }
        else
        {
            return Mathf.Abs(alpha1 * _radius + 2 * alpha2 * _radius) + 2 * Mathf.Sqrt(Mathf.Max(0, _L.magnitude * _L.magnitude / 4 - _radius * _radius));
        }
    }
    // returns vector tangent (касательный) to both circles 
    Vector2 StraightAway()
    {
        if (_clockDir != _leftwing)
        {
            return _L;
        }
        else
        {
            return _L + 2 * Rotate(_rad2, alpha2 * (_leftwing ? 1 : -1));
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
