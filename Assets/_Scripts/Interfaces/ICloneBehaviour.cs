using UnityEngine;


/// <summary>
/// Define el comportamiento básico que debe tener cualquier tipo de clon en el juego.
/// Siguiendo el principio de segregación de interfaces (ISP) de SOLID,
/// mantenemos la interfaz pequeña y específica.
/// </summary>
public interface ICloneBehavior
{
    /// <summary>
    /// Se llama cuando se crea un nuevo clon.
    /// Útil para inicializar efectos visuales o comportamientos específicos.
    /// </summary>
    void OnCloneCreated();

    /// <summary>
    /// Se llama cuando se destruye un clon.
    /// Permite limpiar recursos y activar efectos de destrucción.
    /// </summary>
    void OnCloneDestroyed();

    /// <summary>
    /// Actualiza el estado del clon cada frame.
    /// Maneja la duración, efectos visuales y otros comportamientos continuos.
    /// </summary>
    void UpdateClone();
}
