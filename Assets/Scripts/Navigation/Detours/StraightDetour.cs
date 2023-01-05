using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightDetour : DetourBase
{
    Vector2 _direction;    

    public StraightDetour(Vector2 startPos, Vector2 endPos, float Vdesc, float Vdrill, float Vcurr, float Vidle, float accel)
        :base(startPos, endPos, Vdesc, Vdrill, Vcurr, Vidle, accel)
    {}

    // input: distance over Detour's trajectory
    // return: vector2 position in Detour's coordinates 
    public override Vector2 GetPoint(float distance)
    {
        if(distance > GetLength()) 
        {
            return _endPos;
        }
        Vector2 point = _startPos + _direction*distance;
        return point;
    }

    // return: Detour trajectory length 
    public override float GetLength()
    {
        float length = (_endPos - _startPos).magnitude;
        return length;
    }

    // input: distance over Detour's trajectory
    // return: quaternion rotation in Detour's coordinates 
    public override Quaternion GetRotation(float distance=0)
    {
        Quaternion rotation = Quaternion.FromToRotation(Vector2.down, _direction);
        return rotation;
    }

    // endPos correction and calculations for path
    // correct and calculate whatever you need
    public override void DefinePath()
    {
        // that condition is for the drill not to travel upwards
        if (_endPos.y > _startPos.y)
        {
            _endPos.y = _startPos.y;
        }
        _direction = _endPos - _startPos;
        _direction.Normalize();        
    }

    // returns time difference between travelling along Detour with endPos.y corrected by -deltaY
    // and time for camera to descend by deltaY 
    // just change "Straight Detour" to your Detour class
    public override float Function(float deltaY)
    {
        StraightDetour cPathPlusDeltaY = new StraightDetour(_startPos, new Vector2(_endPos.x, _endPos.y - deltaY), _Vdesc, _Vdrill, _Vstart, _Vfinal, _accel);
        cPathPlusDeltaY.DefinePath();
        cPathPlusDeltaY.CalcAccel();
        return deltaY / _Vdesc - cPathPlusDeltaY.GetTime(20);
    }
}
