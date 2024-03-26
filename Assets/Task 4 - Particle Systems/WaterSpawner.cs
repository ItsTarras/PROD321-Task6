using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class encapsulates a "water" particle spawner/generator
 * for a particle system.
 *
 * PROD321 - Interactive Computer Graphics and Animation
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */
 
public class WaterSpawner : MonoBehaviour
{
    // A reference to the water particle prefab we will spawn
    public GameObject waterParticle;

    // The number of particles we should spawn per second
    public float particlesSpawnedPerSecond;

    // The minimum and maximum variables for the particle direction
    public Vector3 particleDirectionVarianceMin = new Vector3(-.2f, 1, -.1f);
    public Vector3 particleDirectionVarianceMax = new Vector3(.2f, 1.1f, .1f);

    // The minimum and maximum variables for the particle acceleration
    public Vector3 particleAccelerationDirectionVarianceMin = new Vector3(0f, -0.001f, 0f);
    public Vector3 particleAccelerationDirectionVarianceMax = new Vector3(0f, -0.1f, 0f);

    // The time we last spawned a particle
    float timeLastSpawned = 0;

    // The amount of time between each spawn
    float timeBetweenSpawns = 0;

    public Vector3 acceleration = Vector3.zero;

    public enum gravity
    {
        on,
        off,
    }


    public gravity gravityEnabled = gravity.on;

    // Start is called before the first frame update
    void Start()
    {
        // The time between spawns will be 1/particlesSpawnedPerSecond
        timeBetweenSpawns = 1f / particlesSpawnedPerSecond;

        // Spawn our first particle
        Spawn();
    }

    void Spawn()
    {
        // Spawn a new copy of our particle
        GameObject newParticle = GameObject.Instantiate(waterParticle);

        // Set this new particle to be a child of this gameobject
        newParticle.transform.SetParent(transform, false);

        // Calculate a random direction for the particle between
        // particleDirectionVarianceMin and particleDirectionVarianceMax
        Vector3 direction = new Vector3(Random.Range(particleDirectionVarianceMin.x, particleDirectionVarianceMax.x),
            Random.Range(particleDirectionVarianceMin.y, particleDirectionVarianceMax.y),
            Random.Range(particleDirectionVarianceMin.z, particleDirectionVarianceMax.z));

        if (newParticle.GetComponent<WaterParticle>() == null)
            newParticle.AddComponent<WaterParticle>();

        // Update the new particles WaterParticle script's direction vector
        newParticle.GetComponent<WaterParticle>().Direction = direction;

        //TODO: Calculate acceleration and add to our particle
        if (gravityEnabled == gravity.on)
        {
            acceleration = new Vector3(Random.Range(particleAccelerationDirectionVarianceMin.x, particleAccelerationDirectionVarianceMax.x),
            Random.Range(particleAccelerationDirectionVarianceMin.y, particleAccelerationDirectionVarianceMax.y),
            Random.Range(particleAccelerationDirectionVarianceMin.z, particleAccelerationDirectionVarianceMax.z));
        }
        else
        {
            acceleration = Vector3.zero;
        }


        newParticle.GetComponent<WaterParticle>().Acceleration = acceleration;

        // Update the time we last spawned a particle to now
        timeLastSpawned = Time.realtimeSinceStartup;
    }

    // Update is called once per frame
    void Update()
    {
        // If the time since we last spawned a particle is longer the the time
        // we are waiting between particle spawns
        if (Time.realtimeSinceStartup - timeLastSpawned > timeBetweenSpawns)
            // Spawn a particle
            Spawn();
    }
}
