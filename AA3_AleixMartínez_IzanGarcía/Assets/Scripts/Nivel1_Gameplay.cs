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
            
        }
    }

    public void Perder()
    {
       
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void ActualizarTexto()
    {
        if (textoPantalla)
            textoPantalla.text = $"NIVEL 1 (CCD)\nNúcleos: {nucleosRobados} / {totalNucleos}";
    }
}