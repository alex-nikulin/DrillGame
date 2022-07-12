using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleDetour : DetourBase
{
    Vector2 _center1, _center2;
    Vector2 _startDir;
    Vector2 _rad1, _rad2;
    Vector2 _L;
    float _radius, alpha1, alpha2;
    bool _clockDir, _leftwing;
    public float _Vside;
    TileMapBehaviour _worldInfo;

    public CircleDetour(Vector2 startPos, Vector2 startDir, Vector2 endPos, float Vcurr, float Vidle, float accel, float radius, float speed, float descendSpeed, float sideSpeed, TileMapBehaviour worldInfo)
        :base(startPos, endPos, descendSpeed, speed, Vcurr, Vidle, accel)
    {
        _radius = radius;
        _startDir = startDir;
        Delta = new Vector2(0.0f, 0.0f);
        _Vside = sideSpeed;
        _worldInfo = worldInfo;
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
    // update vertical component of endPos
    public void SetEndPos(float deltaY) 
    {
        _endPos += new Vector2(0.0f, deltaY);
        DefinePath();
    }
    public void SetSideSpeed(float sideSpeed) 
    {
        _Vside = sideSpeed;
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
        CircleDetour cPathPlusDeltaY = new CircleDetour(_startPos, _startDir, new Vector2(_endPos.x, _endPos.y - deltaY), _Vstart, _Vfinal, _accel, _radius, _Vdrill, _Vdesc, _Vside, _worldInfo);
        cPathPlusDeltaY.DefinePath();
        return (deltaY) / _Vdesc - cPathPlusDeltaY.GetTime(40);
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
    public override void DefinePath()
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
}
