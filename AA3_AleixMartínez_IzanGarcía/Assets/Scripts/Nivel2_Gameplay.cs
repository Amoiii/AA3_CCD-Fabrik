using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Nivel2_Gameplay : MonoBehaviour
{
    public Text textoPantalla;
    private int botonesPulsados = 0;

    // ESTO SOLO LO LLAMA EL LÁSER
    public void TocarLaser()
    {
        Debug.Log("Reiniciando nivel...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ESTO SOLO LO LLAMA EL BOTÓN
    public void TocarBoton(GameObject boton)
    {
        botonesPulsados++;
        boton.SetActive(false); // Apagamos el botón para que no se pulse dos veces

        if (textoPantalla) textoPantalla.text = "Botones: " + botonesPulsados + "/2";

        if (botonesPulsados >= 2)
        {
            if (textoPantalla) textoPantalla.text = "<color=green>¡NIVEL SUPERADO!</color>";
            Debug.Log("¡Has ganado!");
        }
    }
}