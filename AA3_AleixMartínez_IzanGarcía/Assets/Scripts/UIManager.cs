using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Referencias")]
    public CCD scriptCCD;      // Tu script de Nivel 1
    public FABRIK scriptFabrik; // TU NUEVO SCRIPT DE NIVEL 2

    [Header("Textos UI")]
    public Text textoCCD;
    public Text textoFabrik;

    void Update()
    {
        // Panel CCD
        if (scriptCCD != null)
        {
            textoCCD.text = "<b>CCD (Robatori)</b>\n" +
                            "Iteraciones: " + scriptCCD.iterationsUsed + "\n" +
                            "Distancia: " + scriptCCD.currentDistance.ToString("F2");
        }

        // Panel FABRIK
        if (scriptFabrik != null)
        {
            textoFabrik.text = "<b>FABRIK (Làser)</b>\n" +
                               "Iteraciones: " + scriptFabrik.iterationsUsed + "\n" +
                               "Distancia: " + scriptFabrik.currentDistance.ToString("F2");
        }
    }
}