using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Nivel2_Gameplay : MonoBehaviour
{
  
    [Header("Configuración Nivel")]
    public GameObject contenedorLasers; 
    public int botonesParaGanar = 3;   

    private int botonesPulsados = 0;

    void Start()
    {
       
        if (contenedorLasers != null) contenedorLasers.SetActive(true);
       
    }

   
    public void TocarLaser()
    {
       
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    
    public void TocarBoton(GameObject boton)
    {
        botonesPulsados++;
        boton.SetActive(false); 

     
        // COMPROBACIÓN DE VICTORIA
        if (botonesPulsados >= botonesParaGanar)
        {
            DesactivarSeguridad();
        }
    }

    void DesactivarSeguridad()
    {
        // 1. Apagamos los láseres visual y físicamente
        if (contenedorLasers != null)
        {
            contenedorLasers.SetActive(false);
        }

     
      
    }

  
}