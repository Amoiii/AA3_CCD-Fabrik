using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("--- NIVEL 1: CCD ---")]
    public GameObject objetoBrazoCCD; // El objeto padre del brazo (Joint0)
    public GameObject grupoUICCD;     // La carpeta "GRUPO_UI_CCD" del Canvas
    public CCD scriptCCD;      // El script del brazo

    [Header("Controles CCD")]
    public Text textoInfoCCD;
    public Slider sliderIteraciones;
    public Slider sliderTolerancia;

    [Header("--- NIVEL 2: FABRIK ---")]
    public GameObject objetoBrazoFABRIK; // El objeto padre del brazo (Joint0)
    public GameObject grupoUIFABRIK;     // La carpeta "GRUPO_UI_FABRIK" del Canvas
    public FABRIK scriptFABRIK;    // El script del brazo

    [Header("Controles FABRIK")]
    public Text textoInfoFABRIK;

    void Start()
    {
        // Configuración inicial de Sliders
        if (sliderIteraciones != null && scriptCCD != null)
        {
            sliderIteraciones.minValue = 1;
            sliderIteraciones.maxValue = 50;
            sliderIteraciones.value = scriptCCD.maxIterations;
        }
        if (sliderTolerancia != null && scriptCCD != null)
        {
            sliderTolerancia.minValue = 0.001f;
            sliderTolerancia.maxValue = 1.0f;
            sliderTolerancia.value = scriptCCD.tolerance;
        }

        // --- ESTADO POR DEFECTO: CCD ACTIVADO ---
        ActivarModoCCD();
    }

    void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                ActivarModoCCD();
            }
            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                ActivarModoFABRIK();
            }
        }
        // --- ACTUALIZAR DATOS EN PANTALLA ---

        // Solo actualizamos la UI del que esté activo para ahorrar recursos
        if (objetoBrazoCCD.activeSelf && scriptCCD != null)
        {
            // Lógica CCD
            scriptCCD.maxIterations = (int)sliderIteraciones.value;
            scriptCCD.tolerance = sliderTolerancia.value;

            string estado = (scriptCCD.currentDistance <= scriptCCD.tolerance) ? "<color=green>LLEGADO</color>" : "MOVIENDO";
            textoInfoCCD.text = $"<b>ALGORITMO: CCD (Nivel 1)</b>\n" +
                                $"--------------------------\n" +
                                $"Estado: {estado}\n" +
                                $"Iteraciones: {scriptCCD.iterationsUsed}\n" +
                                $"Distancia: {scriptCCD.currentDistance:F3}\n" +
                                $"Tolerancia: {scriptCCD.tolerance:F3}";
        }

        if (objetoBrazoFABRIK.activeSelf && scriptFABRIK != null)
        {
            // Lógica FABRIK
            string estado = (scriptFABRIK.currentDistance <= scriptFABRIK.tolerance) ? "<color=green>LLEGADO</color>" : "MOVIENDO";
            textoInfoFABRIK.text = $"<b>ALGORITMO: FABRIK (Nivel 2)</b>\n" +
                                   $"--------------------------\n" +
                                   $"Estado: {estado}\n" +
                                   $"Iteraciones: {scriptFABRIK.iterationsUsed}\n" +
                                   $"Distancia: {scriptFABRIK.currentDistance:F3}";
        }
    }

    // --- FUNCIONES PARA CAMBIAR DE MODO ---

    void ActivarModoCCD()
    {
        // Encender cosas de CCD
        if (objetoBrazoCCD) objetoBrazoCCD.SetActive(true);
        if (grupoUICCD) grupoUICCD.SetActive(true);

        // Apagar cosas de FABRIK
        if (objetoBrazoFABRIK) objetoBrazoFABRIK.SetActive(false);
        if (grupoUIFABRIK) grupoUIFABRIK.SetActive(false);
    }

    void ActivarModoFABRIK()
    {
        // Apagar cosas de CCD
        if (objetoBrazoCCD) objetoBrazoCCD.SetActive(false);
        if (grupoUICCD) grupoUICCD.SetActive(false);

        // Encender cosas de FABRIK
        if (objetoBrazoFABRIK) objetoBrazoFABRIK.SetActive(true);
        if (grupoUIFABRIK) grupoUIFABRIK.SetActive(true);
    }
}