using UnityEngine;

public class DetectorColision : MonoBehaviour
{
    public enum Tipo { Obstaculo, Objetivo } // Obstaculo = Laser/Pared, Objetivo = Boton/Nucleo

    [Header("CONFIGURACIÓN")]
    public Tipo tipoObjeto;

    [Header("Arrastra el Manager de TU nivel (Uno de los dos)")]
    public Nivel1_Gameplay managerNivel1; // Arrastra aquí si es el Nivel CCD
    public Nivel2_Gameplay managerNivel2; // Arrastra aquí si es el Nivel FABRIK

    void OnTriggerEnter(Collider other)
    {
        // Detectar si nos toca el Jugador (Brazo o Bola)
        // IMPORTANTE: La punta del brazo CCD también debe tener el Tag "Player"
        if (other.CompareTag("Player") || other.name == "Target" || other.name == "EndEffector")
        {
            // --- CASO 1: OBSTÁCULO (Reiniciar) ---
            if (tipoObjeto == Tipo.Obstaculo)
            {
                if (managerNivel1 != null) managerNivel1.Perder();
                if (managerNivel2 != null) managerNivel2.TocarLaser();
            }
            // --- CASO 2: OBJETIVO (Sumar Punto) ---
            else if (tipoObjeto == Tipo.Objetivo)
            {
                // Desactivamos el objeto para que parezca "recogido"
                this.gameObject.SetActive(false);

                if (managerNivel1 != null) managerNivel1.RecogerNucleo();
                if (managerNivel2 != null) managerNivel2.TocarBoton(this.gameObject);
            }
        }
    }
}