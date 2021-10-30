using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FloatingOriginAdvanced : MonoBehaviour
{
    public float distance = 100.0f;
    public bool collectObjectsOnce = true;
    public bool checkParticles = true;
    public bool checkPhysics = true;
    public float physicsDistance = 1000.0f;
    public float defaultSleepThreshold = 0.14f;
    public GameObject trailRenderer;

    private float distanceSqr;
    private float physicsDistanceSqr;
    private Object[] objects;
    private static List<Object> gameObjects;
    private static List<Object> physicsObjects;
    private static ParticleSystem trailSystem;
    private ParticleSystem.Particle[] particles;
    private ParticleSystem.EmissionModule emissionModule;

    [HideInInspector] public Vector3 absolutePosition;
    [HideInInspector] public Vector3 worldOffset;
    [HideInInspector] public bool originChanged = false;


    void Start()
    {
        distanceSqr = Mathf.Pow(distance, 2f);
        physicsDistanceSqr = Mathf.Pow(physicsDistance, 2f);

        if (collectObjectsOnce)
            CollectObjectsOnce();

        if(trailRenderer != null)
        {
            trailSystem = trailRenderer.GetComponent<ParticleSystem>();

            if (particles == null || particles.Length < trailSystem.main.maxParticles)
                particles = new ParticleSystem.Particle[trailSystem.main.maxParticles];

            emissionModule = trailSystem.emission;
        }

        absolutePosition = transform.position;
    }

    void LateUpdate()
    {
        ManageFloatingOrigin();
    }

    public void CollectObjectsOnce()
    {
        gameObjects = new List<Object>();
        gameObjects = FindObjectsOfType(typeof(Transform)).ToList();

        if (checkPhysics)
        {
            physicsObjects = new List<Object>();
            physicsObjects = FindObjectsOfType(typeof(Rigidbody)).ToList();
        }
    }

    private void ManageFloatingOrigin()
    {
        originChanged = false;
        Vector3 cameraPosition = transform.position;
        absolutePosition = transform.position + worldOffset;

        if (cameraPosition.sqrMagnitude > distanceSqr)
        {
            worldOffset += transform.position;
            originChanged = true;

            if (collectObjectsOnce)
            {
                foreach (Object o in gameObjects)
                {
                    Transform t = (Transform)o;

                    if (t.parent == null)
                        t.position -= cameraPosition;
                }
            }
            else
            {
                objects = FindObjectsOfType(typeof(Transform));

                foreach (Object o in objects)
                {
                    Transform t = (Transform)o;

                    if (t.parent == null)
                        t.position -= cameraPosition;
                }
            }

            if (checkParticles && trailRenderer != null)
            {
                //emissionModule.enabled = false;

                int liveParticles = trailSystem.GetParticles(particles);

                for (int i = 0; i < liveParticles; i++)
                    particles[i].position -= cameraPosition;

                trailSystem.SetParticles(particles, liveParticles);

                //emissionModule.enabled = true;
            }

            if (checkPhysics && physicsDistance > 0f)
            {
                if (collectObjectsOnce)
                {
                    foreach (Object o in physicsObjects)
                    {
                        Rigidbody r = (Rigidbody)o;

                        if (r.gameObject.transform.position.sqrMagnitude > physicsDistanceSqr)
                            r.sleepThreshold = float.MaxValue;
                        else
                            r.sleepThreshold = defaultSleepThreshold;
                    }
                }
                else
                {
                    objects = FindObjectsOfType(typeof(Rigidbody));

                    foreach (Object o in objects)
                    {
                        Rigidbody r = (Rigidbody)o;

                        if (r.gameObject.transform.position.sqrMagnitude > physicsDistanceSqr)
                            r.sleepThreshold = float.MaxValue;
                        else
                            r.sleepThreshold = defaultSleepThreshold;
                    }
                }
            }
        }
    }
}

