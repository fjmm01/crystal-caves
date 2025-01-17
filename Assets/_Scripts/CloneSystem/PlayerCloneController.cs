using UnityEngine;

public class PlayerCloneController : MonoBehaviour
{
    [SerializeField] private CloneManager cloneManager;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerMovement playerMovement;

    private bool isCreatingClone;
    private CrystalClone selectedClone;

    private void Update()
    {
        HandleCloneCreation();
        HandleCloneTeleport();
    }

    private void HandleCloneCreation()
    {
        if((inputManager.CloneInput && !inputManager.NeedNewCloneInput)&& cloneManager.CanCreateClone())
        {
            cloneManager.CreateClone(transform.position);
            inputManager.NeedNewCloneInput = true;
        }
    }

    private void HandleCloneTeleport()
    {
        if(inputManager.TeleportInput && !inputManager.NeedNewTeleportInput)
        {
            //Implementar lógica de seleccion de clon y teleport
            if(selectedClone != null)
            {
                TeleportToClone(selectedClone);
                inputManager.NeedNewTeleportInput = true;
            }
        }
    }

    private void TeleportToClone(CrystalClone targetClone)
    {
        Vector2 previousPosition = transform.position;
        transform.position = targetClone.Position;

        //Efectos visuales del teleport
        CreateTeleportEffect(previousPosition,targetClone.Position);
    }

    private void CreateTeleportEffect(Vector2 startPos, Vector2 endPos)
    {
        // Implementar efectos visuales del teleport
    }
}
