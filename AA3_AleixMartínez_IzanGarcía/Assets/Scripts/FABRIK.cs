using UnityEngine;
using System.Collections.Generic;

public class FABRIK : MonoBehaviour
{
    [Header("Configuración del Brazo")]
    // Arrastra aquí TODOS los joints (desde la Base hasta el último antes de la punta)
    public List<Transform> joints;
    public Transform endEffector; // La bolita roja (hija del último joint)
    public Transform target;      // La esfera azul (objetivo)

    [Header("Parámetros")]
    public float tolerance = 0.1f;
    public int maxIterations = 10;

    [Header("Debug UI")]
    public int iterationsUsed;
    public float currentDistance;

    // Datos internos (Longitudes de los huesos)
    private float[] boneLengths;
    private Vector3[] positions; // Posiciones virtuales para calcular
    private Vector3 startPosition; // Donde empieza la base (para que no se mueva del sitio)

    void Start()
    {
        // 1. Inicializar arrays
        // Necesitamos guardar la posición de TODOS los joints + el EndEffector
        int totalNodes = joints.Count + 1;
        positions = new Vector3[totalNodes];
        boneLengths = new float[totalNodes - 1];

        // 2. Calcular longitudes iniciales (Hueso 0 mide X, Hueso 1 mide Y...)
        // Guardamos las longitudes para que el brazo no se deforme luego
        for (int i = 0; i < joints.Count; i++)
        {
            if (i < joints.Count - 1)
                boneLengths[i] = Vector3.Distance(joints[i].position, joints[i + 1].position);
            else
                // Último hueso: distancia del último joint al endEffector
                boneLengths[i] = Vector3.Distance(joints[i].position, endEffector.position);
        }

        startPosition = joints[0].position;
    }

    void LateUpdate()
    {
        if (target == null || endEffector == null) return;

        // Comprobamos distancia
        currentDistance = Vector3.Distance(endEffector.position, target.position);
        iterationsUsed = 0;

        if (currentDistance <= tolerance) return;

        SolveFABRIK();

        // IMPORTANTE: Aplicar la magia visual (Rotar los cilindros)
        ApplyVisuals();
    }

    void SolveFABRIK()
    {
        // 1. Copiar posiciones reales a nuestro array virtual
        for (int i = 0; i < joints.Count; i++) positions[i] = joints[i].position;
        positions[joints.Count] = endEffector.position;

        // Bucle de intentos
        for (int iter = 0; iter < maxIterations; iter++)
        {
            iterationsUsed++;

            // --- STAGE 1: FORWARD (Hacia adelante) --- 
            // Ponemos la punta en el target y arrastramos el resto hacia ella
            positions[positions.Length - 1] = target.position;

            for (int i = positions.Length - 2; i >= 0; i--)
            {
                // Dirección desde el target hacia el padre
                Vector3 direction = (positions[i] - positions[i + 1]).normalized;
                // Recolocamos el padre a la distancia correcta del hijo
                positions[i] = positions[i + 1] + direction * boneLengths[i];
            }

            // --- STAGE 2: BACKWARD (Hacia atrás) ---
            // Ponemos la base en su sitio original y empujamos el resto hacia la punta
            positions[0] = startPosition; // La base NO se mueve

            for (int i = 0; i < positions.Length - 1; i++)
            {
                // Dirección desde la base hacia el hijo
                Vector3 direction = (positions[i + 1] - positions[i]).normalized;
                // Recolocamos el hijo a la distancia correcta del padre
                positions[i + 1] = positions[i] + direction * boneLengths[i];
            }

            // Comprobar si hemos llegado
            if (Vector3.Distance(positions[positions.Length - 1], target.position) < tolerance)
                break;
        }
    }

    void ApplyVisuals()
    {
        // 1. Mover los objetos a las posiciones calculadas
        for (int i = 0; i < joints.Count; i++)
        {
            joints[i].position = positions[i];
        }
        endEffector.position = positions[positions.Length - 1];

        // 2. ROTAR LOS HUESOS (Esto hace que se vea bien)
        // Hacemos que cada Joint mire al siguiente Joint
        for (int i = 0; i < joints.Count; i++)
        {
            if (i < joints.Count - 1)
                RotateBone(joints[i], joints[i + 1].position);
            else
                RotateBone(joints[i], endEffector.position);
        }
    }

    void RotateBone(Transform current, Vector3 targetPos)
    {
        Vector3 direction = (targetPos - current.position).normalized;
        if (direction != Vector3.zero)
            current.up = direction; // Asumiendo que tu cilindro crece en Y
    }
}