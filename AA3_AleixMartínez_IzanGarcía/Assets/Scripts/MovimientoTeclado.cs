using UnityEngine;
using UnityEngine.InputSystem; 

public class MovimientoTeclado : MonoBehaviour
{
    [Header("Ajustes")]
    public float velocidad = 5f;

    void Update()
    {
        
        float moveX = 0f;
        float moveZ = 0f; 
        float moveY = 0f; 

       
        if (Keyboard.current != null)
        {
            //X a d
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
                moveX = -1f;
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
                moveX = 1f;

            //Z w s
            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
                moveZ = 1f;
            if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed)
                moveZ = -1f;

            //Y spacio c
            if (Keyboard.current.spaceKey.isPressed)
                moveY = 1f; // Subir

            if (Keyboard.current.cKey.isPressed)
                moveY = -1f; // Bajar
        }

        // 3. Aplicar movimiento
       
        Vector3 movimiento = new Vector3(moveX, moveY, moveZ);

        // Normalizamos para  diagonal
        if (movimiento.magnitude > 1f) movimiento.Normalize();

      
        transform.Translate(movimiento * velocidad * Time.deltaTime, Space.World);
    }
}