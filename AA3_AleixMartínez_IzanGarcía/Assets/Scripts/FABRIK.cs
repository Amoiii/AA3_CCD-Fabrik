using UnityEngine;
using System.Collections.Generic;

public class FABRIK : MonoBehaviour
{
    [Header("Configuración")]
    public List<Transform> joints; // Los huesos que vemos
    public Transform endEffector;  // La punta final (hija del último hueso)
    public Transform target;       // La bola azul

    [Header("Suavizado (Smooth)")]
    [Tooltip("Velocidad de movimiento. 5 = Lento y pesado. 20 = Rápido.")]
    [Range(1f, 50f)]
    public float speed = 10f;

    [Header("Parámetros del Algoritmo")]
    public float tolerance = 0.01f;
    public int maxIterations = 10;

    [Header("Datos UI (No tocar)")]
    public int iterationsUsed;
    public float currentDistance;

    // --- VARIABLES INTERNAS ---
    private float[] boneLengths;
    private Vector3[] virtualPositions; // Aquí vive el "Esqueleto Fantasma"
    private float totalArmLength;

    // Para reiniciar
    private List<Vector3> initialLocalPositions = new List<Vector3>();
    private List<Quaternion> initialLocalRotations = new List<Quaternion>();
    private bool initialized = false;

    void Awake()
    {
        // 1. Guardar postura inicial (Local) para el reset
        foreach (Transform j in joints)
        {
            initialLocalPositions.Add(j.localPosition);
            initialLocalRotations.Add(j.localRotation);
        }
        initialized = true;

        // 2. Inicializar el sistema virtual
        int totalNodes = joints.Count + 1; // Joints + EndEffector
        virtualPositions = new Vector3[totalNodes];
        boneLengths = new float[totalNodes - 1];

        // 3. Calcular longitudes y llenar posiciones iniciales
        for (int i = 0; i < joints.Count; i++)
        {
            Vector3 startNode = joints[i].position;
            Vector3 endNode = (i == joints.Count - 1) ? endEffector.position : joints[i + 1].position;

            boneLengths[i] = Vector3.Distance(startNode, endNode);
            totalArmLength += boneLengths[i];

            virtualPositions[i] = startNode;
        }
        virtualPositions[joints.Count] = endEffector.position;
    }

    void OnEnable()
    {
        // RESETEAR al activar (Tecla 2)
        if (initialized)
        {
            for (int i = 0; i < joints.Count; i++)
            {
                joints[i].localPosition = initialLocalPositions[i];
                joints[i].localRotation = initialLocalRotations[i];
                // Importante: El fantasma también se resetea al sitio real
                virtualPositions[i] = joints[i].position;
            }
            // Resetear endEffector
            endEffector.localPosition = new Vector3(0, endEffector.localPosition.y, 0);
            virtualPositions[joints.Count] = endEffector.position;
        }
    }

    void LateUpdate()
    {
        if (target == null || endEffector == null) return;

        // PASO 1: Calcular FABRIK en el "Esqueleto Fantasma"
        // (Esto es pura matemática instantánea, no se ve)
        SolveVirtualFABRIK();

        // PASO 2: Mover los objetos reales hacia el Fantasma
        // (Esto es lo que da el efecto suave)
        ApplyVisuals();
    }

    void SolveVirtualFABRIK()
    {
        // La base fantasma SIEMPRE está pegada a la base real (por si el dron se mueve)
        virtualPositions[0] = joints[0].position;

        // Distancias para la UI
        float distToBase = Vector3.Distance(virtualPositions[0], target.position);
        currentDistance = Vector3.Distance(virtualPositions[virtualPositions.Length - 1], target.position);

        // --- A. INALCANZABLE (Estirar recto) ---
        if (distToBase > totalArmLength)
        {
            Vector3 direction = (target.position - virtualPositions[0]).normalized;
            for (int i = 0; i < boneLengths.Length; i++)
            {
                virtualPositions[i + 1] = virtualPositions[i] + direction * boneLengths[i];
            }
            iterationsUsed = 0;
        }
        // --- B. ALCANZABLE (Iterar) ---
        else
        {
            // Solo calculamos si el objetivo se ha movido (ahorra vibraciones)
            if (currentDistance > tolerance)
            {
                iterationsUsed = 0;
                for (int iter = 0; iter < maxIterations; iter++)
                {
                    iterationsUsed++;

                    // Forward (Hacia Target)
                    virtualPositions[virtualPositions.Length - 1] = target.position;
                    for (int i = virtualPositions.Length - 2; i >= 0; i--)
                    {
                        Vector3 dir = (virtualPositions[i] - virtualPositions[i + 1]).normalized;
                        virtualPositions[i] = virtualPositions[i + 1] + dir * boneLengths[i];
                    }

                    // Backward (Hacia Base)
                    virtualPositions[0] = joints[0].position; // Fijar base
                    for (int i = 0; i < virtualPositions.Length - 1; i++)
                    {
                        Vector3 dir = (virtualPositions[i + 1] - virtualPositions[i]).normalized;
                        virtualPositions[i + 1] = virtualPositions[i] + dir * boneLengths[i];
                    }

                    // Comprobar salida
                    if (Vector3.Distance(virtualPositions[virtualPositions.Length - 1], target.position) < tolerance)
                        break;
                }
            }
        }
        // Actualizar distancia final UI
        currentDistance = Vector3.Distance(virtualPositions[virtualPositions.Length - 1], target.position);
    }

    void ApplyVisuals()
    {
        float step = speed * Time.deltaTime;

        // 1. MOVER POSICIONES (Suavemente hacia el fantasma)
        for (int i = 0; i < joints.Count; i++)
        {
            joints[i].position = Vector3.MoveTowards(joints[i].position, virtualPositions[i], step);
        }
        // Mover también el endEffector visual
        endEffector.position = Vector3.MoveTowards(endEffector.position, virtualPositions[virtualPositions.Length - 1], step);

        // 2. CORREGIR ROTACIONES (El truco anti-desmontaje)
        // Cada hueso mira obligatoriamente al siguiente hueso VISUAL
        for (int i = 0; i < joints.Count; i++)
        {
            // El objetivo es el siguiente hueso, o el endEffector si soy el último
            Transform targetTransform = (i < joints.Count - 1) ? joints[i + 1] : endEffector;

            Vector3 direction = targetTransform.position - joints[i].position;

            if (direction.sqrMagnitude > 0.001f)
            {
                // NOTA: Si tus cilindros se giran mal, cambia Vector3.up por Vector3.right o Vector3.forward
                // Quaternion.FromToRotation(EJE_DE_TU_CILINDRO, DIRECCION)
                joints[i].rotation = Quaternion.FromToRotation(Vector3.up, direction);
            }
        }
    }
}