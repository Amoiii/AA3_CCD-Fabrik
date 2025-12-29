using UnityEngine;

public class DetectorColision : MonoBehaviour
{
    public enum TipoObjeto { Laser, Boton, Brazo }
    public TipoObjeto tipo;

    // Arrastra aquí el objeto GameManager que tiene el script Nivel2_Gameplay
    public Nivel2_Gameplay manager;

    void OnTriggerEnter(Collider other)
    {
        // Si soy un LÁSER y me toca el BRAZO o el TARGET -> MUERTE
        if (tipo == TipoObjeto.Laser)
        {
            if (other.CompareTag("Player")) // Asegúrate de poner el tag "Player" al Brazo y Target
            {
                manager.TocarLaser();
            }
        }
        // Si soy un BOTÓN y me toca el TARGET (La bola azul) -> WIN
        else if (tipo == TipoObjeto.Boton)
        {
            if (other.name == "TargetFabrik" || other.CompareTag("Player")) // O el nombre de tu bola azul
            {
                manager.TocarBoton(this.gameObject);
            }
        }
    }
}