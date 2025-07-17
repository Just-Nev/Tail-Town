using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class Dog : MonoBehaviour
{
    [Header("Wandering")]
    public float wanderRadius = 5f;
    public float waitTime = 2f;

    [Header("Energy")]
    public float maxEnergy = 100f;
    public float energyDrainRate = 5f;       // Energy per second while wandering
    public float energyRecoveryRate = 10f;   // Energy per second while sleeping
    public float lowEnergyThreshold = 20f;

    [Header("Sleep Settings")]
    public GameObject sleepSpot;             // Assign in Inspector

    [Header("Visual Effects")]
    public ParticleSystem sleepEffectPrefab; // Assign particle system prefab

    private NavMeshAgent agent;
    private bool isWandering = false;
    private bool isSleeping = false;
    private float currentEnergy;

    private ParticleSystem activeSleepEffect; // Reference to the instantiated effect

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false; // Important for 2D
        agent.updateUpAxis = false;

        currentEnergy = maxEnergy;

        StartCoroutine(DogRoutine());
    }

    private IEnumerator DogRoutine()
    {
        while (true)
        {
            if (currentEnergy <= lowEnergyThreshold)
            {
                yield return StartCoroutine(GoSleepRoutine());
            }
            else
            {
                yield return StartCoroutine(WanderRoutine());
            }
        }
    }

    private IEnumerator WanderRoutine()
    {
        Vector3 wanderPoint = GetRandomNavmeshLocation(wanderRadius);
        agent.SetDestination(wanderPoint);

        isWandering = true;

        while (agent.pathPending || agent.remainingDistance > 0.1f)
        {
            DrainEnergy();
            yield return null;
        }

        isWandering = false;

        yield return new WaitForSeconds(waitTime);
    }

    private IEnumerator GoSleepRoutine()
    {
        isSleeping = true;
        agent.SetDestination(sleepSpot.transform.position);

        while (agent.pathPending || agent.remainingDistance > 0.1f)
        {
            yield return null;
        }

        // Instantiate sleep effect and store reference
        if (sleepEffectPrefab != null && activeSleepEffect == null)
        {
            activeSleepEffect = Instantiate(sleepEffectPrefab, sleepSpot.transform.position, Quaternion.identity);
        }

        // Sleep and restore energy
        while (currentEnergy < maxEnergy)
        {
            currentEnergy += energyRecoveryRate * Time.deltaTime;
            yield return null;
        }

        currentEnergy = maxEnergy;
        isSleeping = false;

        yield return new WaitForSeconds(1f);

        // Destroy particle effect after sleeping
        if (activeSleepEffect != null)
        {
            activeSleepEffect.Stop();
            Destroy(activeSleepEffect.gameObject, 1f); // Wait a second before destroying (in case there's fade-out)
            activeSleepEffect = null;
        }
    }

    private void DrainEnergy()
    {
        currentEnergy -= energyDrainRate * Time.deltaTime;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
    }

    private Vector3 GetRandomNavmeshLocation(float radius)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector2 randomDirection = Random.insideUnitCircle * radius;
            Vector3 samplePosition = transform.position + new Vector3(randomDirection.x, randomDirection.y, 0);

            if (NavMesh.SamplePosition(samplePosition, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return transform.position; // fallback
    }
}




