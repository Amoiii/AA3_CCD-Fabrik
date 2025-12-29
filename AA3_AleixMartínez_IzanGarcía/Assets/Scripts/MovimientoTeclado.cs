using UnityEngine;
using UnityEngine.InputSystem; // IMPORTANTE: Necesario para que no de error

public class MovimientoTeclado : MonoBehaviour
{
    [Header("Ajustes")]
    public float velocidad = 5f;

    void Update()
    {
        // 1. Definimos las variables de movimiento a 0
        float moveX = 0f; // Izquierda/Derecha
        float moveZ = 0f; // Delante/Atrás
        float moveY = 0f; // Arriba/Abajo (Altura)

        // 2. Comprobamos el Teclado (Nuevo Sistema)
        if (Keyboard.current != null)
        {
            // --- EJE HORIZONTAL (X) ---
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
                moveX = -1f;
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
                moveX = 1f;

            // --- EJE PROFUNDIDAD (Z) ---
            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
                moveZ = 1f;
            if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed)
                moveZ = -1f;

            // --- EJE VERTICAL (Y) - LO QUE HAS PEDIDO ---
            if (Keyboard.current.spaceKey.isPressed)
                moveY = 1f; // Subir

            if (Keyboard.current.cKey.isPressed)
                moveY = -1f; // Bajar
        }

        // 3. Aplicar movimiento
        // Creamos el vector combinando los 3 ejes
        Vector3 movimiento = new Vector3(moveX, moveY, moveZ);

        // Normalizamos para que no corra más al ir en diagonal
        if (movimiento.magnitude > 1f) movimiento.Normalize();

        // Movemos el objeto
        transform.Translate(movimiento * velocidad * Time.deltaTime, Space.World);
    }
}