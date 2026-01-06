using UnityEngine;

public class DetectorColision : MonoBehaviour
{
    public enum Tipo { Obstaculo, Objetivo } 

    [Header("CONFIGURACIÓN")]
    public Tipo tipoObjeto;

    [Header("Arrastra el Manager de TU nivel (Uno de los dos)")]
    public Nivel1_Gameplay managerNivel1; 
    public Nivel2_Gameplay managerNivel2;

    void OnTriggerEnter(Collider other)
    {
        // Detectar si nos toca el Jugador (Brazo o Bola)
      
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
               
                this.gameObject.SetActive(false);

                if (managerNivel1 != null) managerNivel1.RecogerNucleo();
                if (managerNivel2 != null) managerNivel2.TocarBoton(this.gameObject);
            }
        }
    }
}