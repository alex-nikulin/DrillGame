using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemManager : MonoBehaviour
{
    ParticleSystem.Particle[] particles;
    public float _camSpeed;
    public DrillBehaviour drillInfo;
    int prevParticle;

    public void UpdateParticlesVelocity(ParticleSystem particleSystem) 
    {
        var emiss = particleSystem.emission;
        string tag = particleSystem.tag;
        int rate = (tag == "dyn_system") ? 30 : 10;
        emiss.rateOverTime = rate * drillInfo.Path.GetVRatio();
        int particlesNumAlive = particleSystem.GetParticles(particles);
        for (int i = 0; i < particlesNumAlive; i++) 
        {
            particles[i].velocity = new Vector2(0.0f, _camSpeed);
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
        // if (tmapBehav.CheckCurrentTile(checkUnder) != prevParticle) 
        // {
        //     if (tmapBehav.CheckCurrentTile(checkUnder) == 1) 
        //     {
        //         pSystems[0].Stop();
        //         pSystems[1].Stop();
        //         pSystems[2].Stop();
        //         pSystems[3].Play();
        //         pSystems[4].Play();
        //         pSystems[5].Play();
        //     }
        //     else 
        //     {
        //         pSystems[0].Play();
        //         pSystems[1].Play();
        //         pSystems[2].Play();
        //         pSystems[3].Stop();
        //         pSystems[4].Stop();
        //         pSystems[5].Stop();
        //     }
        //     prevParticle = tmapBehav.CheckCurrentTile(checkUnder);
        // }

    }


    // Start is called before the first frame update
    void Start()
    {
        ParticleSystem[] pSystems = GetComponentsInChildren<ParticleSystem>();
        pSystems[0].Play();
        pSystems[1].Play();
        pSystems[2].Play();
        pSystems[3].Stop();
        pSystems[4].Stop();
        pSystems[5].Stop();
        prevParticle = 1;
        particles = new ParticleSystem.Particle[256];
        //ManageParticleSystems();
    }

    // Update is called once per frame
    void Update()
    {
        ManageParticleSystems();
    }
    void FixedUpdate()
    {
        UpdateAllParticleSystems();
    }
}
