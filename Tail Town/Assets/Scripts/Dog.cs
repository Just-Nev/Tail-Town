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
    public float energyDrainRate = 5f;
    public float energyRecoveryRate = 10f; // Sleep rate
    public float lowEnergyThreshold = 20f;

    [Header("Hunger")]
    public float maxHunger = 100f;
    public float hungerDrainRate = 2f;
    public float hungerRecoveryRate = 25f; // Faster than thirst
    public float energyBoostPerSecond = 5f;
    public float lowHungerThreshold = 30f;
    public GameObject foodBowl;
    public SpriteRenderer foodBowlRenderer;
    public ParticleSystem eatingEffectPrefab;

    [Header("Thirst")]
    public float maxThirst = 100f;
    public float thirstDrainRate = 3f;
    public float thirstRecoveryRate = 15f;
    public float lowThirstThreshold = 25f;
    public GameObject waterBowl;
    public SpriteRenderer bowlRenderer;
    public ParticleSystem drinkingEffectPrefab;

    [Header("Sleep Settings")]
    public GameObject sleepSpot;

    [Header("Visual Effects")]
    public ParticleSystem sleepEffectPrefab;

    private NavMeshAgent agent;
    private float currentEnergy;
    private float currentThirst;
    private float currentHunger;

    private bool isWandering = false;
    private bool isSleeping = false;
    private bool isDrinking = false;
    private bool isEating = false;

    private ParticleSystem activeSleepEffect;
    private ParticleSystem activeDrinkEffect;
    private ParticleSystem activeEatEffect;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        currentEnergy = maxEnergy;
        currentThirst = maxThirst;
        currentHunger = maxHunger;

        StartCoroutine(DogRoutine());
    }

    private IEnumerator DogRoutine()
    {
        while (true)
        {
            if (currentHunger <= lowHungerThreshold)
            {
                yield return StartCoroutine(EatRoutine());
            }
            else if (currentThirst <= lowThirstThreshold)
            {
                yield return StartCoroutine(DrinkRoutine());
            }
            else if (currentEnergy <= lowEnergyThreshold)
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
            DrainThirst();
            DrainHunger();
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
            yield return null;

        if (sleepEffectPrefab != null && activeSleepEffect == null)
        {
            activeSleepEffect = Instantiate(sleepEffectPrefab, sleepSpot.transform.position, Quaternion.identity);
        }

        while (currentEnergy < maxEnergy)
        {
            currentEnergy += energyRecoveryRate * Time.deltaTime;
            yield return null;
        }

        currentEnergy = maxEnergy;
        isSleeping = false;
        yield return new WaitForSeconds(1f);

        if (activeSleepEffect != null)
        {
            activeSleepEffect.Stop();
            Destroy(activeSleepEffect.gameObject, 1f);
            activeSleepEffect = null;
        }
    }

    private IEnumerator DrinkRoutine()
    {
        isDrinking = true;
        agent.SetDestination(waterBowl.transform.position);

        while (agent.pathPending || agent.remainingDistance > 0.1f)
            yield return null;

        Color bowlColor = bowlRenderer.color;
        if (bowlColor.a <= 0.05f)
        {
            Debug.Log("Water bowl is empty.");
            yield return new WaitForSeconds(1f);
            isDrinking = false;
            yield break;
        }

        float duration = (maxThirst - currentThirst) / thirstRecoveryRate;
        float elapsed = 0f;
        Color originalColor = bowlColor;

        if (drinkingEffectPrefab != null)
        {
            activeDrinkEffect = Instantiate(drinkingEffectPrefab, waterBowl.transform.position, Quaternion.identity);
            activeDrinkEffect.Play();
        }

        while (currentThirst < maxThirst && bowlRenderer.color.a > 0.05f)
        {
            currentThirst += thirstRecoveryRate * Time.deltaTime;
            elapsed += Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            Color newColor = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Clamp01(alpha));
            bowlRenderer.color = newColor;

            yield return null;
        }

        currentThirst = Mathf.Min(currentThirst, maxThirst);
        isDrinking = false;

        if (activeDrinkEffect != null)
        {
            activeDrinkEffect.Stop();
            Destroy(activeDrinkEffect.gameObject, 1f);
            activeDrinkEffect = null;
        }
    }

    private IEnumerator EatRoutine()
    {
        isEating = true;
        agent.SetDestination(foodBowl.transform.position);

        while (agent.pathPending || agent.remainingDistance > 0.1f)
            yield return null;

        Color bowlColor = foodBowlRenderer.color;
        if (bowlColor.a <= 0.05f)
        {
            Debug.Log("Food bowl is empty.");
            yield return new WaitForSeconds(1f);
            isEating = false;
            yield break;
        }

        float duration = (maxHunger - currentHunger) / hungerRecoveryRate;
        float elapsed = 0f;
        Color originalColor = bowlColor;

        if (eatingEffectPrefab != null)
        {
            activeEatEffect = Instantiate(eatingEffectPrefab, foodBowl.transform.position, Quaternion.identity);
            activeEatEffect.Play();
        }

        while (currentHunger < maxHunger && foodBowlRenderer.color.a > 0.05f)
        {
            currentHunger += hungerRecoveryRate * Time.deltaTime;
            currentEnergy += energyBoostPerSecond * Time.deltaTime; // Boost energy from food
            currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);

            elapsed += Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            Color newColor = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Clamp01(alpha));
            foodBowlRenderer.color = newColor;

            yield return null;
        }

        currentHunger = Mathf.Min(currentHunger, maxHunger);
        isEating = false;

        if (activeEatEffect != null)
        {
            activeEatEffect.Stop();
            Destroy(activeEatEffect.gameObject, 1f);
            activeEatEffect = null;
        }
    }

    private void DrainEnergy()
    {
        currentEnergy -= energyDrainRate * Time.deltaTime;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
    }

    private void DrainThirst()
    {
        currentThirst -= thirstDrainRate * Time.deltaTime;
        currentThirst = Mathf.Clamp(currentThirst, 0, maxThirst);
    }

    private void DrainHunger()
    {
        currentHunger -= hungerDrainRate * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
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

        return transform.position;
    }
}







