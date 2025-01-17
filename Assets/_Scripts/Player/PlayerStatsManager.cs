using UnityEngine;

[RequireComponent(typeof(PlayerManager))]
public class PlayerStatsManager : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Energy Settings")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float energyRegenerationRate = 10f;
    [SerializeField] private float energyRegenerationDelay = 1f;
    [SerializeField] private float cloneEnergyCost = 20f;

    private HealthSystem healthSystem;
    private EnergySystem energySystem;

    public HealthSystem Health => healthSystem;
    public EnergySystem Energy => energySystem;

    private void Awake()
    {
        healthSystem = new HealthSystem(maxHealth);
        energySystem = new EnergySystem(maxEnergy,energyRegenerationRate,energyRegenerationDelay);

        //Suscribirse a eventos relevantes
        healthSystem.OnDeath += HandlePlayerDeath;
    }

    public void Update()
    {
        energySystem.UpdateRegeneration(Time.deltaTime);
        healthSystem.UpdateInvulnerability(Time.deltaTime);
    }

    public bool CanCreateClone()
    {
        return energySystem.CurrentValue >= cloneEnergyCost;
    }

    public void ConsumeCloneEnergy()
    {
        if(CanCreateClone())
        {
            energySystem.Modify(-cloneEnergyCost);
        }
    }

    private void HandlePlayerDeath()
    {
        //Implementar lógica de muerte
        Debug.Log("Player died!");
    }

    private void OnDestroy()
    {
        // Limpieza de eventos
        healthSystem.OnDeath -= HandlePlayerDeath;
    }
}
