using UnityEngine;
using System.Collections.Generic;

public class FABRIK : MonoBehaviour
{
    [Header("Configuración")]
    public List<Transform> joints;
    public Transform endEffector;
    public Transform target;

    [Header("Ajustes")]
    [Range(0, 1)]
    public float smoothing = 0.5f; // Suavizado visual
    public float tolerance = 0.01f;
    public int maxIterations = 10;

    [Header("Debug")]
    public int iterationsUsed;
    public float currentDistance;

    // Datos internos matemáticos
    private float[] boneLengths;
    private Vec3[] virtualPositions; // Usamos tu estructura Vec3
    private bool initialized = false;

    void Awake()
    {
        InitFABRIK();
    }

    void InitFABRIK()
    {
        if (joints.Count < 2) return;

        // 1. Inicializamos arrays
        int numJoints = joints.Count + 1; // +1 porque incluimos el EndEffector como un "nodo" más
        virtualPositions = new Vec3[numJoints];
        boneLengths = new float[numJoints - 1];

        // 2. Guardamos longitudes iniciales (Rígidas)
        // La distancia entre hueso 0 y 1, entre 1 y 2... y entre el último hueso y el endEffector
        for (int i = 0; i < joints.Count - 1; i++)
        {
            boneLengths[i] = Mates.Distance((Vec3)joints[i].position, (Vec3)joints[i + 1].position);
            virtualPositions[i] = (Vec3)joints[i].position;
        }

        // Último tramo: Del último hueso al EndEffector
        int lastBoneIndex = joints.Count - 1;
        boneLengths[lastBoneIndex] = Mates.Distance((Vec3)joints[lastBoneIndex].position, (Vec3)endEffector.position);

        // Posiciones iniciales
        virtualPositions[lastBoneIndex] = (Vec3)joints[lastBoneIndex].position;
        virtualPositions[numJoints - 1] = (Vec3)endEffector.position;

        initialized = true;
    }

    void LateUpdate()
    {
        if (!initialized || target == null) return;

        // --- PASO 1: ACTUALIZAR LA BASE (CRÍTICO PARA EL DRON) ---
        // Antes de calcular nada, decimos que la posición virtual 0 es la posición REAL del dron ahora mismo.
        virtualPositions[0] = (Vec3)joints[0].position;

        // Inicializamos el resto de la cadena virtual con donde están los gráficos ahora 
        // (para que el movimiento sea continuo y no salte)
        for (int i = 1; i < joints.Count; i++)
            virtualPositions[i] = (Vec3)joints[i].position;

        virtualPositions[virtualPositions.Length - 1] = (Vec3)endEffector.position;


        // --- PASO 2: RESOLVER FABRIK (MATEMÁTICAS PURAS) ---
        Solve();

        // --- PASO 3: APLICAR A UNITY (VISUALES) ---
        ApplyVisuals();
    }

    void Solve()
    {
        // Distancia actual al target
        currentDistance = Mates.Distance(virtualPositions[virtualPositions.Length - 1], (Vec3)target.position);
        iterationsUsed = 0;

        // Si ya estamos cerca, no calculamos
        if (currentDistance <= tolerance) return;

        // BUCLE DE ITERACIONES
        for (int iter = 0; iter < maxIterations; iter++)
        {
            iterationsUsed++;

            // --- A. FORWARD REACHING (Desde el Target hacia la Base) ---
            // Ponemos la punta en el target
            virtualPositions[virtualPositions.Length - 1] = (Vec3)target.position;

            for (int i = virtualPositions.Length - 2; i >= 0; i--)
            {
                // Dirección hacia el siguiente punto
                Vec3 dir = (virtualPositions[i] - virtualPositions[i + 1]).Normalized();
                // Nos colocamos a la distancia correcta del hueso
                virtualPositions[i] = virtualPositions[i + 1] + dir * boneLengths[i];
            }

            // --- B. BACKWARD REACHING (Desde la Base hacia el Target) ---
            // ¡¡AQUÍ ESTÁ EL TRUCO DEL DRON!!
            // Volvemos a clavar la base en la posición del dron (que se ha movido)
            virtualPositions[0] = (Vec3)joints[0].position;

            for (int i = 0; i < virtualPositions.Length - 1; i++)
            {
                // Dirección hacia el siguiente punto
                Vec3 dir = (virtualPositions[i + 1] - virtualPositions[i]).Normalized();
                // Proyectamos hacia adelante
                virtualPositions[i + 1] = virtualPositions[i] + dir * boneLengths[i];
            }

            // Comprobar si hemos llegado para salir antes del bucle
            float distToTarget = Mates.Distance(virtualPositions[virtualPositions.Length - 1], (Vec3)target.position);
            if (distToTarget <= tolerance) break;
        }

        currentDistance = Mates.Distance(virtualPositions[virtualPositions.Length - 1], (Vec3)target.position);
    }

    void ApplyVisuals()
    {
        // Interpolación para suavizar (evita vibraciones fuertes)
        float t = 1.0f - smoothing;


        for (int i = 0; i < joints.Count; i++)
        {
            // ¿Cuál es mi siguiente punto objetivo en el esqueleto virtual?
            Vec3 nextVirtualPos;
            if (i < joints.Count - 1)
                nextVirtualPos = virtualPositions[i + 1];
            else
                nextVirtualPos = virtualPositions[virtualPositions.Length - 1]; // El end effector

            // Convertimos a Vector3 de Unity para usar funciones de rotación
            Vector3 direction = nextVirtualPos.ToUnity() - joints[i].position;

            if (direction.sqrMagnitude > 0.0001f) // Evitar errores de dirección cero
            {
                // Rotamos el hueso para que mire al siguiente punto calculado por FABRIK
                // Asumimos que el hueso crece en el eje Y o Z (ajusta Vector3.up o Vector3.forward según tu modelo)
                // Si tus cilindros crecen hacia arriba, usa Vector3.up.
                Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, direction);

                // Aplicamos rotación suave
                joints[i].rotation = Quaternion.Slerp(joints[i].rotation, targetRotation, t);
            }

            // OPCIONAL: Forzar posición (solo si se ve que se separa mucho)
            // if (i > 0) joints[i].position = Vector3.Lerp(joints[i].position, virtualPositions[i].ToUnity(), t);
        }
    }

    // Dibujar el esqueleto virtual para depurar
    void OnDrawGizmos()
    {
        if (!initialized || virtualPositions == null) return;
        Gizmos.color = Color.red;
        for (int i = 0; i < virtualPositions.Length - 1; i++)
        {
            Gizmos.DrawLine(virtualPositions[i].ToUnity(), virtualPositions[i + 1].ToUnity());
            Gizmos.DrawSphere(virtualPositions[i].ToUnity(), 0.05f);
        }
        Gizmos.DrawSphere(virtualPositions[virtualPositions.Length - 1].ToUnity(), 0.05f);
    }
}