using UnityEngine;
using System.Collections.Generic;

public class FABRIK : MonoBehaviour
{
    [Header("Configuración del Brazo")]
    public List<Transform> joints;
    public Transform endEffector;
    public Transform target;

    [Header("Ajustes de Suavidad")]
    [Range(0, 1)]
    public float smoothing = 0.5f; // 0 = Rápido/Robótico, 1 = Muy Lento/Fluido
    public float tolerance = 0.05f;
    public int maxIterations = 10;

    [Header("Debug Info")]
    public int iterationsUsed;
    public float currentDistance;

    // Datos internos
    private float[] boneLengths;
    private Vector3[] positions; // Posiciones virtuales (donde DEBERÍA estar)
    private Vector3 startPosition;
    private float totalArmLength;

    void Start()
    {
        // Validación
        if (joints.Count == 0 || endEffector == null || target == null)
        {
            Debug.LogError("¡Faltan asignar cosas en el inspector!");
            return;
        }

        // Inicializar arrays
        int totalNodes = joints.Count + 1;
        positions = new Vector3[totalNodes];
        boneLengths = new float[totalNodes - 1];

        startPosition = joints[0].position;
        totalArmLength = 0;

        // Calcular longitudes y longitud total del brazo
        for (int i = 0; i < joints.Count; i++)
        {
            // Usamos las posiciones iniciales para leer el largo de los huesos
            Vector3 startNode = joints[i].position;
            Vector3 endNode = (i == joints.Count - 1) ? endEffector.position : joints[i + 1].position;

            boneLengths[i] = Vector3.Distance(startNode, endNode);
            totalArmLength += boneLengths[i];

            // Inicializar las posiciones virtuales con las reales
            positions[i] = startNode;
        }
        positions[joints.Count] = endEffector.position;
    }

    void LateUpdate()
    {
        if (target == null || endEffector == null) return;

        // 1. Decidir la posición ideal (Matemáticas)
        SolveFABRIK();

        // 2. Mover los objetos visuales hacia esa posición suavemente
        ApplyVisualsSmoothed();
    }

    void SolveFABRIK()
    {
        // Actualizar la base virtual por si el Dron se ha movido
        // (Pero NO reseteamos todo el array para mantener coherencia temporal)
        startPosition = joints[0].position;

        // Distancia al objetivo
        float distanceToTarget = Vector3.Distance(startPosition, target.position);
        currentDistance = Vector3.Distance(positions[positions.Length - 1], target.position);

        // CASO 1: EL OBJETIVO ESTÁ FUERA DE ALCANCE (Aquí se quedaba pillado antes)
        if (distanceToTarget > totalArmLength)
        {
            // Simplemente estiramos el brazo en línea recta hacia el objetivo
            // No hace falta iterar (esto ahorra CPU y evita vibraciones)
            Vector3 direction = (target.position - startPosition).normalized;

            positions[0] = startPosition;
            for (int i = 0; i < boneLengths.Length; i++)
            {
                positions[i + 1] = positions[i] + direction * boneLengths[i];
            }
            iterationsUsed = 0; // No hemos iterado
        }
        // CASO 2: EL OBJETIVO ESTÁ CERCA (ALCANCE)
        else
        {
            // Copiamos la base actual para empezar a calcular
            positions[0] = startPosition;

            // Bucle iterativo (Solo si no hemos llegado ya)
            // Si la distancia visual ya es buena, no calculamos para evitar jitter
            if (currentDistance > tolerance)
            {
                for (int iter = 0; iter < maxIterations; iter++)
                {
                    iterationsUsed++;

                    // --- FORWARD (Hacia el target) ---
                    positions[positions.Length - 1] = target.position;
                    for (int i = positions.Length - 2; i >= 0; i--)
                    {
                        Vector3 dir = (positions[i] - positions[i + 1]).normalized;
                        positions[i] = positions[i + 1] + dir * boneLengths[i];
                    }

                    // --- BACKWARD (Hacia la base) ---
                    positions[0] = startPosition;
                    for (int i = 0; i < positions.Length - 1; i++)
                    {
                        Vector3 dir = (positions[i + 1] - positions[i]).normalized;
                        positions[i + 1] = positions[i] + dir * boneLengths[i];
                    }

                    // Salir si ya estamos muy cerca
                    if (Vector3.Distance(positions[positions.Length - 1], target.position) < tolerance)
                        break;
                }
            }
        }
    }

    void ApplyVisualsSmoothed()
    {
        // Factor de suavizado (Lerp)
        // Si smoothing es 0, t=1 (Instantáneo). Si smoothing es 0.9, t=0.1 (Lento).
        float t = 1.0f - smoothing;

        // 1. Mover articulaciones
        for (int i = 0; i < joints.Count; i++)
        {
            // Interpolamos la posición actual hacia la calculada por FABRIK
            joints[i].position = Vector3.Lerp(joints[i].position, positions[i], t);
        }
        // Mover el efector final
        endEffector.position = Vector3.Lerp(endEffector.position, positions[positions.Length - 1], t);

        // 2. Rotar huesos (LookAt suave)
        for (int i = 0; i < joints.Count; i++)
        {
            Vector3 targetPos = (i < joints.Count - 1) ? joints[i + 1].position : endEffector.position;
            Vector3 direction = targetPos - joints[i].position;

            if (direction.sqrMagnitude > 0.001f) // Evitar rotar si están pegados
            {
                // Calculamos la rotación ideal
                Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, direction); // Asumiendo hueso en eje Y

                // Interpolamos la rotación (Slerp)
                joints[i].rotation = Quaternion.Slerp(joints[i].rotation, targetRotation, t);
            }
        }
    }
}