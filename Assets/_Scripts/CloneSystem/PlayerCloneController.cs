using UnityEngine;

/// <summary>
/// Controla las interacciones del jugador con el sistema de clones.
/// Este componente actúa como intermediario entre el sistema de input y el sistema de clones.
/// </summary>
public class PlayerCloneController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CloneManager cloneManager;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Teleport Settings")]
    [SerializeField] private float teleportCooldown = 0.2f;
    [SerializeField] private float maxTeleportDistance = 10f;
    [SerializeField] private ParticleSystem teleportEffect;

    private bool canTeleport = true;
    private float teleportCooldownTimer;
    private CrystalClone nearestClone;

    private void Update()
    {
        HandleCloneCreation();
        UpdateNearestClone();
        HandleCloneTeleport();
        UpdateCooldown();
    }

    /// <summary>
    /// Actualiza el temporizador del cooldown de teleportación
    /// </summary>
    private void UpdateCooldown()
    {
        if (!canTeleport)
        {
            teleportCooldownTimer -= Time.deltaTime;
            if (teleportCooldownTimer <= 0)
            {
                canTeleport = true;
            }
        }
    }

    /// <summary>
    /// Maneja la creación de nuevos clones
    /// </summary>
    private void HandleCloneCreation()
    {
        if (inputManager == null)
        {
            Debug.LogError("InputManager reference is missing in PlayerCloneController!");
            return;
        }

        if (cloneManager == null)
        {
            Debug.LogError("CloneManager reference is missing in PlayerCloneController!");
            return;
        }

        if (inputManager.CloneInput && cloneManager.CanCreateClone())
        {
            var newClone = cloneManager.CreateClone(transform.position);
            if (newClone != null)
            {
                // Opcional: Efectos de sonido o visuales al crear el clon
                PlayCloneCreationEffects();
            }
        }
    }

    /// <summary>
    /// Actualiza cuál es el clon más cercano al jugador
    /// </summary>
    private void UpdateNearestClone()
    {
        float nearestDistance = float.MaxValue;
        nearestClone = null;

        foreach (var clone in cloneManager.ActiveClones)
        {
            float distance = Vector2.Distance(transform.position, clone.Position);
            if (distance < nearestDistance && distance <= maxTeleportDistance)
            {
                nearestDistance = distance;
                nearestClone = clone;
            }
        }

        // Opcional: Resaltar visualmente el clon más cercano
        HighlightNearestClone();
    }

    /// <summary>
    /// Maneja la teleportación hacia los clones
    /// </summary>
    private void HandleCloneTeleport()
    {
        if (!canTeleport) return;

        if (inputManager.TeleportInput && nearestClone != null)
        {
            TeleportToClone(nearestClone);
            canTeleport = false;
            teleportCooldownTimer = teleportCooldown;
        }
    }

    /// <summary>
    /// Realiza la teleportación hacia un clon específico
    /// </summary>
    private void TeleportToClone(CrystalClone targetClone)
    {
        if (targetClone == null || !targetClone.IsActive) return;

        Vector2 previousPosition = transform.position;
        Vector2 targetPosition = targetClone.Position;

        // Guardar la velocidad actual si es necesario conservar el momentum
        Vector2 currentVelocity = Vector2.zero;
        if (playerMovement != null && playerMovement.rb != null)
        {
            currentVelocity = playerMovement.rb.linearVelocity;
        }

        // Realizar la teleportación
        transform.position = targetPosition;

        // Aplicar el momentum conservado si está configurado así
        if (playerMovement != null && playerMovement.rb != null && cloneManager.CloneData.preserveMomentum)
        {
            playerMovement.rb.linearVelocity = currentVelocity;
        }

        // Efectos visuales y sonoros
        CreateTeleportEffect(previousPosition, targetPosition);
    }

    /// <summary>
    /// Crea efectos visuales para la teleportación
    /// </summary>
    private void CreateTeleportEffect(Vector2 startPos, Vector2 endPos)
    {
        if (teleportEffect != null)
        {
            // Efecto en la posición inicial
            var startEffect = Instantiate(teleportEffect, startPos, Quaternion.identity);
            startEffect.Play();

            // Efecto en la posición final
            var endEffect = Instantiate(teleportEffect, endPos, Quaternion.identity);
            endEffect.Play();

            // Destruir los efectos después de que terminen
            float duration = teleportEffect.main.duration;
            Destroy(startEffect.gameObject, duration);
            Destroy(endEffect.gameObject, duration);
        }
    }

    /// <summary>
    /// Reproduce efectos visuales al crear un clon
    /// </summary>
    private void PlayCloneCreationEffects()
    {
        // Implementar efectos de creación de clones
    }

    /// <summary>
    /// Resalta visualmente el clon más cercano
    /// </summary>
    private void HighlightNearestClone()
    {
        // Implementar highlight visual del clon más cercano
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Dibujar el rango de teleportación
        Gizmos.color = new Color(1, 1, 0, 0.3f); // Amarillo transparente
        Gizmos.DrawWireSphere(transform.position, maxTeleportDistance);

        // Resaltar el clon más cercano
        if (nearestClone != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, nearestClone.Position);
        }
    }
#endif
}
