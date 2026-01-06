using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Nivel2_Gameplay : MonoBehaviour
{
    public Text textoPantalla;
    private int botonesPulsados = 0;

   
    public void TocarLaser()
    {
       
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    
    public void TocarBoton(GameObject boton)
    {
        botonesPulsados++;
        boton.SetActive(false); 

 
    }
}