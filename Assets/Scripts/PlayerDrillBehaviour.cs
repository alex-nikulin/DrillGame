using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

public class PlayerDrillBehaviour : MonoBehaviour
{
    
    //calculates and stores information about path, drill is moving along
    class CirclePath
    {
        Vector2 _startPos;
        Vector2 _startDir;
        Vector2 _endPos;
        Vector2 _originalEndPos;
        Vector2 _center1;
        Vector2 _rad1;
        Vector2 _center2;
        Vector2 _rad2;
        Vector2 _L;
        Vector2 _descendVelocity;
        bool _hasArrived;
        float _radius;
        bool _clockDir;
        bool _leftwing;
        float _z;
        float alpha1;
        float alpha2;
        float _currentDistance;
        float _timer;
        float _Vdrill, _Vdesc, _Vside;
        Vector2 _Shift;


        //tmp fields
        float _allegedTime, _allegedTime2;

        public CirclePath(Vector2 startPos, Vector2 startDir, Vector2 endPos, float radius, float speed, float descendSpeed, float sideSpeed)
        {
            //startPos, startDir - starting position and direction of the path, endPos - final position of the path, relative to camera (final direction is always down)
            _startPos = startPos; 
            _startDir = startDir;
            _startDir.Normalize();
            _endPos = endPos;
            // all turns have the same radius
            _radius = radius;
            // since tilemaps move up, path needs to accommodate for movement
            _Shift = new Vector2(0.0f, 0.0f);
            // _Vdrill - maximum magnitude of speed along the path, _Vdesc - speed of descending (tilemaps moving up) is always constant
            // _Vside - current speed of moving side ways, updated every frame
            _Vdrill = speed;
            _Vdesc  = descendSpeed;
            _Vside = sideSpeed;
            // distance travelled along the path
            _currentDistance = 0;
            // not used
            _timer = 0;
            _originalEndPos = _endPos;
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
            if (PathLength() <= 0.05) 
            {
                if(Mathf.Abs(_originalEndPos.y - _endPos.y) / _Vdesc > _timer) 
                {
                    _hasArrived = true;
                }
                return  _endPos;
            }
            else if (_currentDistance + l * deltaTime < PathLength())
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

        // since tilemaps move up actual endPos (endPos in tilemap coordinates) must by way lower than endPos we first recieved (endPos in global coordinates)
        // to calculate this endPos Newton's method is used
        // Function calculates difference between time for drill to travel along path (with endPos moved down by deltaY) and time for tilemaps to move up by deltaY
        // looking for such deltaY: Function(deltaY) == 0
        public float Function(float deltaY)
        {
            CirclePath cPathPlusDeltaY = new CirclePath(_startPos, _startDir, new Vector2(_endPos.x, _endPos.y - deltaY), _radius, _Vdrill, _Vdesc, _Vside);
            cPathPlusDeltaY.DefineTurns();
            return (deltaY) / _Vdesc - cPathPlusDeltaY.PathLength() / _Vdrill;
        }
        // calculation
        public void CorrectEndPos()
        {
            float deltaY = 10;
            float diffStep = 0.0001f;
            float cPathL1, cPathL2;
            for (int i = 0; i < 4; i++) 
            {
                cPathL2 = Function(deltaY-diffStep);
                cPathL1 = Function(deltaY);
                deltaY += cPathL1 * diffStep / (cPathL2 - cPathL1);
            }
            _endPos = new Vector2(_endPos.x, _endPos.y - deltaY);
            DefineTurns();
            Debug.Log(Function(deltaY));
                Debug.Log(_Vdrill);
            _Vdrill = Mathf.Min(_Vdrill, PathLength() * _Vdesc / Mathf.Abs(_originalEndPos.y - _endPos.y));
            
        }
        
        // essential function, which returns point, corresponding to arbitrary distance
        public Vector2 GetPoint(float distance)
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

        public Quaternion GetCurrentRotation() 
        {
            float distance = _currentDistance;
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
                //Debug.Log("POOPS!");
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
            if (alpha1 == 0 & alpha2 == 0)
            {
                Debug.Log(_startPos);
                Debug.Log(_startDir);
                Debug.Log(_endPos);
                Debug.Log(-_startDir.y / _startDir.magnitude);
                Debug.Log(Vector2.Dot(-1 * _L, _rad2));
            }
        }

        public float PathLength()
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

    CirclePath cPath;
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
    public void UpdateParticlesVelocity(ParticleSystem particleSystem) 
    {
        var emiss = particleSystem.emission;
        string tag = particleSystem.tag;
        int rate = (tag == "dyn_system") ? 30 : 10;
        emiss.rateOverTime = rate * cPath.GetVRatio();
        int particlesNumAlive = particleSystem.GetParticles(particles);
        for (int i = 0; i < particlesNumAlive; i++) {
            particles[i].velocity = new Vector2(tmapBehav.velDir.x, tmapBehav.velDir.y * tmapBehav.descendingSpeed);
        }
        particleSystem.SetParticles(particles, particlesNumAlive);

    }
    public void UpdateAllParticleSystems() 
    {
        ParticleSystem[] pSystems = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in pSystems) 
        {
            UpdateParticlesVelocity(ps);
        }
    }
    public void ManageParticleSystems() 
    {
        ParticleSystem[] pSystems = GetComponentsInChildren<ParticleSystem>();
        Vector3 checkUnder = transform.TransformPoint(new Vector3(0.0f, -0.5f, 0.0f));
        if (tmapBehav.CheckCurrentTile(checkUnder) != prevParticle) {
            if (tmapBehav.CheckCurrentTile(checkUnder) == 1) {
                pSystems[0].Stop();
                pSystems[1].Stop();
                pSystems[2].Stop();
                pSystems[3].Play();
                pSystems[4].Play();
                pSystems[5].Play();
            }
            else {
                pSystems[0].Play();
                pSystems[1].Play();
                pSystems[2].Play();
                pSystems[3].Stop();
                pSystems[4].Stop();
                pSystems[5].Stop();
            }
            prevParticle = tmapBehav.CheckCurrentTile(checkUnder);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        prevParticle = 3;
        dot = Instantiate(circlePrefab, poi, Quaternion.identity);
        dot.GetComponent<SpriteRenderer>().sortingOrder = 5;
        cPath = new CirclePath(transform.position, Vector2.down, new Vector2(0,1), 0.5f, speed, tmapBehav.descendingSpeed, tmapBehav.velDir.x);
        ManageParticleSystems();
        deltaPos = new Vector2(0, 0);
        var PSList = GetComponentsInChildren<ParticleSystem>();
        Debug.Log(PSList.Length);
        particles = new ParticleSystem.Particle[256];
        drilledAreaFrame = new List<GameObject>();
    }
    // Update is called once per frame
    void Update()
    {
        cPath.SetSideSpeed(tmapBehav.velDir.x);
        if (Input.GetMouseButton(0))
        {
            poi = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dot.transform.position = poi;
            hasArrived = false;
            if (((Vector3)poi - transform.position).magnitude != 0) {
                dir = new Vector2(Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad), -Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad));
                cPath = new CirclePath(transform.position, dir, poi, 2.0f, speed, tmapBehav.descendingSpeed, tmapBehav.velDir.x);
                cPath.CorrectEndPos();
            }
        }
        if (!hasArrived)
        {
            float radius = cPath.GetRadius();
            cPath.SetSideSpeed(tmapBehav.velDir.x);
            Vector2 newPos = cPath.Move(Time.deltaTime);
            Quaternion newRot = cPath.GetCurrentRotation();
            transform.rotation = newRot;
            transform.position = newPos;
            if (cPath.Arrived())
            {
                transform.rotation = Quaternion.FromToRotation(new Vector2(0, -1), new Vector2(0, -1)); 
                hasArrived = true;
            }
        }
        else {
            transform.position += new Vector3(tmapBehav.velDir.x * Time.deltaTime, 0.0f, 0.0f);
        }
        dir = new Vector2(Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad), -Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad));
        ManageParticleSystems();
        UpdateAllParticleSystems();
    }
}
