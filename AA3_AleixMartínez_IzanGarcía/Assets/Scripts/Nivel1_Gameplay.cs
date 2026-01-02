using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Nivel1_Gameplay : MonoBehaviour
{
    [Header("Referencias")]
    public CCD brazoCCD;       // Tu script CCD
    public Transform target;   // La bola verde que mueves
    public Transform inicio;   // Un objeto vacío donde empieza la bola

    [Header("UI")]
    public Text textoPantalla; // Arrastra el Texto_CCD del Canvas

    private int nucleosRobados = 0;
    private int totalNucleos = 0;

    void Start()
    {
        // Contamos cuantos objetivos hay en la escena al empezar
        totalNucleos = GameObject.FindGameObjectsWithTag("Boton").Length;
        ActualizarTexto();
    }

    public void RecogerNucleo()
    {
        nucleosRobados++;
        Debug.Log("Núcleo robado: " + nucleosRobados);
        ActualizarTexto();

        if (nucleosRobados >= totalNucleos)
        {
            textoPantalla.text = "<color=green>¡DATOS ROBADOS! (NIVEL 1 COMPLETADO)</color>";
            // Aquí podrías cargar el siguiente nivel automáticamente
        }
    }

    public void Perder()
    {
        // Si quieres poner obstáculos en el nivel 1 también
        Debug.Log("¡Detectado! Reiniciando...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void ActualizarTexto()
    {
        if (textoPantalla)
            textoPantalla.text = $"NIVEL 1 (CCD)\nNúcleos: {nucleosRobados} / {totalNucleos}";
    }
}