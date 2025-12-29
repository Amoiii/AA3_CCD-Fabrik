using UnityEngine;
using UnityEngine.UI;

public class Nivel2_Gameplay : MonoBehaviour
{
    [Header("Referencias")]
    public Transform targetJugador;    // La bola azul que mueves
    public Transform posicionInicio;   // Un objeto vacío donde empieza el nivel
    public FABRIK brazoScript;         // El script del brazo para resetearlo

    [Header("Feedback")]
    public Text mensajePantalla;       // Arrastra un texto del Canvas aquí

    // Sonidos (Opcional)
    // public AudioSource audioError;
    // public AudioSource audioWin;

    private int botonesPulsados = 0;

    void Start()
    {
        ResetearNivel();
    }

    public void TocarLaser()
    {
        Debug.Log("¡Te quemaste");
        if (mensajePantalla) mensajePantalla.text = "<color=red>¡ERROR! LÁSER TOCADO</color>";

        // Reiniciar posición
        ResetearNivel();
    }

    public void TocarBoton(GameObject boton)
    {
        botonesPulsados++;
        boton.SetActive(false); // Apagar el botón tocado

        Debug.Log("Botón pulsado: " + botonesPulsados);
        if (mensajePantalla) mensajePantalla.text = "Botones: " + botonesPulsados + "/2";

        if (botonesPulsados >= 2)
        {
            if (mensajePantalla) mensajePantalla.text = "<color=green>¡NIVEL COMPLETADO!</color>";
            // Aquí podrías lanzar fuegos artificiales o acabar el juego
        }
    }

    void ResetearNivel()
    {
        // Volver la bola azul al inicio
        targetJugador.position = posicionInicio.position;
        botonesPulsados = 0;

        // Reactivar todos los botones (Buscamos por etiqueta)
        GameObject[] botones = GameObject.FindGameObjectsWithTag("Boton");
        foreach (GameObject b in botones) b.SetActive(true);

        // Resetear texto
        if (mensajePantalla) mensajePantalla.text = "Evita los láseres...";

        // Forzar al brazo a recolocarse instantáneamente
        brazoScript.enabled = false;
        brazoScript.enabled = true;
    }
}