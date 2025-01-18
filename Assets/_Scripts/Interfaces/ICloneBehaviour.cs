using UnityEngine;


/// <summary>
/// Define el comportamiento b�sico que debe tener cualquier tipo de clon en el juego.
/// Siguiendo el principio de segregaci�n de interfaces (ISP) de SOLID,
/// mantenemos la interfaz peque�a y espec�fica.
/// </summary>
public interface ICloneBehavior
{
    /// <summary>
    /// Se llama cuando se crea un nuevo clon.
    /// �til para inicializar efectos visuales o comportamientos espec�ficos.
    /// </summary>
    void OnCloneCreated();

    /// <summary>
    /// Se llama cuando se destruye un clon.
    /// Permite limpiar recursos y activar efectos de destrucci�n.
    /// </summary>
    void OnCloneDestroyed();

    /// <summary>
    /// Actualiza el estado del clon cada frame.
    /// Maneja la duraci�n, efectos visuales y otros comportamientos continuos.
    /// </summary>
    void UpdateClone();
}
