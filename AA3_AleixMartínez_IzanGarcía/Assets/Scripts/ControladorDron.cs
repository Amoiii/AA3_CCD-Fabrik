using UnityEngine;

public class ControladorDron : MonoBehaviour
{
    [Header("ARRASTRA AQUÍ TU BOLA AZUL/VERDE")]
    public Transform ikTarget;      
    [Header("ARRASTRA AQUÍ UN OBJETO VACÍO (Dijo el codo)")]
    public Transform posicionReposo;

    [Header("Configuración")]
    public float radioDeteccion = 10.0f; 
    public float velocidadAtaque = 20f;  

   
    private Transform objetivoActual = null;

    void Update()
    {
        // objetivo vivo?
        if (objetivoActual == null || !objetivoActual.gameObject.activeSelf)
        {
            // Si no tengo, busco 
            BuscarSiguienteObjetivo();
        }

        //  Mover Guía ikTarget
        if (objetivoActual != null)
        {
            // si hay objetivo
            ikTarget.position = Vector3.MoveTowards(ikTarget.position, objetivoActual.position, velocidadAtaque * Time.deltaTime);
        }
        else
        {
            // Si no hay
            ikTarget.position = Vector3.MoveTowards(ikTarget.position, posicionReposo.position, velocidadAtaque * Time.deltaTime);
        }
    }

    void BuscarSiguienteObjetivo()
    {
       
        Collider[] cosasCerca = Physics.OverlapSphere(transform.position, radioDeteccion);

        float distanciaMasCorta = Mathf.Infinity;
        Transform mejorCandidato = null;

        foreach (Collider cosa in cosasCerca)
        {
            // Es un botón Y está activo?
            if (cosa.CompareTag("Boton") && cosa.gameObject.activeSelf)
            {
                float dist = Vector3.Distance(transform.position, cosa.transform.position);

                // Me quedo con el más cercano de todos
                if (dist < distanciaMasCorta)
                {
                    distanciaMasCorta = dist;
                    mejorCandidato = cosa.transform;
                }
            }
        }

        // Si hemos encontrado uno nuevo, lo fijamos
        objetivoActual = mejorCandidato;
    }

   
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}