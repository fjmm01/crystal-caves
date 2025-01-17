using UnityEngine;

public class CrystalClone : MonoBehaviour,ICloneBehaviour
{
    private float remainingDuration;
    private CloneManager cloneManager;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D cloneCollider;

    public bool IsActive { get; private set; }
    public Vector2 Position => transform.position;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cloneCollider = GetComponent<BoxCollider2D>();
    }

    public void Initialize(CloneManager manager, float duration)
    {
        cloneManager = manager;
        remainingDuration = duration;
        IsActive = true;
        OnCreateCLone();
    }

    

    public void OnCloneDestroyed()
    {
        IsActive = false;
        cloneCollider.enabled = false;
        cloneManager.RemoveClone(this);
        //Activar efectos visuales de destrucción
    }

    public void UpdateClone()
    {
        if (!IsActive) return;

        remainingDuration -= Time.deltaTime;
        if(remainingDuration <= 0)
        {
            OnCloneDestroyed();
        }

        //Actualizar efectos visuales(transparencia,etc)
        float alpha = Mathf.Lerp(0.3f,0.8f,remainingDuration/cloneManager.CloneData.duration);
        Color currentColor = spriteRenderer.color;
        spriteRenderer.color  = new Color(currentColor.r,currentColor.g, currentColor.b, alpha);
    }

    public void OnCreateCLone()
    {
        cloneCollider.enabled = true;
        //Activar efectos visuales de creación
    }
}
