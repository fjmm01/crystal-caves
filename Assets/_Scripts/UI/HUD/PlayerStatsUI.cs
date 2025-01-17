using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField] private PlayerStatsManager statsManager;
    [SerializeField] private Image healthBar;
    [SerializeField] private Image energyBar;

    private void Start()
    {
        // Suscribirse a eventos de cambio
        statsManager.Health.OnHealthChanged += UpdateHealthBar;
        statsManager.Energy.OnEnergyChanged += UpdateEnergyBar;
    }

    private void UpdateHealthBar(float currentHealth)
    {
        healthBar.fillAmount = currentHealth / statsManager.Health.MaxValue;
    }

    private void UpdateEnergyBar(float currentEnergy)
    {
        energyBar.fillAmount = currentEnergy / statsManager.Energy.MaxValue;
    }

    private void OnDestroy()
    {
        // Limpieza de eventos
        statsManager.Health.OnHealthChanged -= UpdateHealthBar;
        statsManager.Energy.OnEnergyChanged -= UpdateEnergyBar;
    }
}
