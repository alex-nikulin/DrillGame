using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightDetour : DetourBase
{
    Vector2 _direction;    

    public StraightDetour(Vector2 startPos, Vector2 endPos, float Vdesc, float Vdrill)
        :base(startPos, endPos, Vdesc, Vdrill)
    {}
    public override Vector2 GetPoint(float distance)
    {
        if(distance > GetLength()) 
        {
            return _endPos;
        }
        Vector2 point = _startPos + _direction*distance;
        return point;
    }
    public override float GetLength()
    {
        float length = (_endPos - _startPos).magnitude;
        return length;
    }
    public override Quaternion GetRotation(float distance=0)
    {
        Quaternion rotation = Quaternion.FromToRotation(Vector2.down, _direction);
        return rotation;
    }
    public override void DefinePath()
    {
        if (_endPos.y > _startPos.y)
        {
            _endPos.y = _startPos.y;
        }
        _direction = _endPos - _startPos;
        _direction.Normalize();        
    }
    public override float Function(float deltaY)
    {
        StraightDetour cPathPlusDeltaY = new StraightDetour(_startPos, new Vector2(_endPos.x, _endPos.y - deltaY), _Vdesc, _Vdrill);
        cPathPlusDeltaY.DefinePath();
        return deltaY / _Vdesc - cPathPlusDeltaY.GetTime(20);
    }
}
