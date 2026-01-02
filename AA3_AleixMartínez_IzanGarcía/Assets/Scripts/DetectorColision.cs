using UnityEngine;

public class DetectorColision : MonoBehaviour
{
    public enum Tipo { Laser, Boton }

    [Header("CONFIGURACIÓN")]
    // ¡¡¡IMPORTANTE: CAMBIA ESTO A 'BOTON' EN LAS ESFERAS!!!
    public Tipo tipoObjeto;

    public Nivel2_Gameplay manager;

    void OnTriggerEnter(Collider other)
    {
        // Solo reaccionamos si nos toca el jugador (Brazo o Bola)
        if (other.CompareTag("Player") || other.name == "Target")
        {
            // CASO A: SOY UN LÁSER -> MATAR
            if (tipoObjeto == Tipo.Laser)
            {
                Debug.Log("Láser tocado. Muerte.");
                manager.TocarLaser();
            }
            // CASO B: SOY UN BOTÓN -> SUMAR PUNTO
            else if (tipoObjeto == Tipo.Boton)
            {
                Debug.Log("Botón tocado. Punto.");
                manager.TocarBoton(this.gameObject);
            }
        }
    }
}