using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Nivel1_Gameplay : MonoBehaviour
{
    [Header("Referencias")]
    public CCD brazoCCD;
    public Transform target;
    public Transform inicio;

    [Header("UI")]
    public Text textoPantalla;

    private int nucleosRobados = 0;
    private int totalNucleos = 0;

    void Start()
    {
       
        GameObject[] botones = GameObject.FindGameObjectsWithTag("Boton");
        if (botones != null)
        {
            totalNucleos = botones.Length;
        }
        ActualizarTexto();
    }

    public void RecogerNucleo()
    {
        nucleosRobados++;
       
        ActualizarTexto();

        
        if (nucleosRobados >= totalNucleos)
        {
            
            if (textoPantalla != null)
            {
                textoPantalla.text = "<color=green>¡DATOS ROBADOS! (NIVEL 1 COMPLETADO)</color>";
            }
        }
    }

    public void Perder()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void ActualizarTexto()
    {
        if (textoPantalla != null)
            textoPantalla.text = $"NIVEL 1 (CCD)\nNúcleos: {nucleosRobados} / {totalNucleos}";
    }
}