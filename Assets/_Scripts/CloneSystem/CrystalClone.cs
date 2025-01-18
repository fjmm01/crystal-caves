using UnityEngine;

/// <summary>
/// Implementación concreta de un clon de cristal.
/// Maneja el comportamiento individual de cada clon, incluyendo su ciclo de vida y efectos visuales.
/// </summary>
public class CrystalClone : MonoBehaviour, ICloneBehavior
{
    private float remainingDuration;
    private CloneManager cloneManager;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D cloneCollider;

    // Propiedades públicas para acceso externo
    public bool IsActive { get; private set; }
    public Vector2 Position => transform.position;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cloneCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (IsActive)
        {
            UpdateClone();
        }
    }

    /// <summary>
    /// Inicializa el clon con sus valores iniciales
    /// </summary>
    public void Initialize(CloneManager manager, float duration)
    {
        cloneManager = manager;
        remainingDuration = duration;
        IsActive = true;
        OnCloneCreated();
    }

    public void OnCloneCreated()
    {
        if (cloneCollider != null) cloneCollider.enabled = true;
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            spriteRenderer.color = new Color(color.r, color.g, color.b, 0.8f);
        }
    }

    public void OnCloneDestroyed()
    {
        IsActive = false;
        if (cloneCollider != null) cloneCollider.enabled = false;
        if (cloneManager != null)
        {
            cloneManager.RemoveClone(this);
        }
    }

    public void UpdateClone()
    {
        remainingDuration -= Time.deltaTime;

        // Actualizar transparencia basada en el tiempo restante
        if (spriteRenderer != null)
        {
            float alpha = Mathf.Lerp(0.2f, 0.8f, remainingDuration / cloneManager.CloneData.duration);
            Color currentColor = spriteRenderer.color;
            spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
        }

        if (remainingDuration <= 0)
        {
            OnCloneDestroyed();
        }
    }
}
