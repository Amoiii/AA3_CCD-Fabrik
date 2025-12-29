using UnityEngine;
using System.Collections.Generic;

public class FABRIK : MonoBehaviour
{
    [Header("Configuración")]
    public List<Transform> joints;
    public Transform endEffector;
    public Transform target;

    [Header("Suavizado")]
    [Range(1f, 50f)] public float speed = 20f;

    [Header("Parámetros")]
    public float tolerance = 0.01f;
    public int maxIterations = 20;

    [Header("Datos UI")]
    public int iterationsUsed;
    public float currentDistance;

    // USAMOS NUESTRO PROPIO VECTOR "Vec3"
    private float[] boneLengths;
    private Vec3[] virtualPositions; // <-- Aquí está el cambio clave
    private float totalArmLength;

    // Variables para reset
    private List<Vector3> initialLocalPositions = new List<Vector3>();
    private List<Quaternion> initialLocalRotations = new List<Quaternion>();
    private bool initialized = false;

    void Awake()
    {
        foreach (Transform j in joints)
        {
            initialLocalPositions.Add(j.localPosition);
            initialLocalRotations.Add(j.localRotation);
        }
        initialized = true;

        int totalNodes = joints.Count + 1;
        virtualPositions = new Vec3[totalNodes]; // Array de MIs vectores
        boneLengths = new float[totalNodes - 1];

        for (int i = 0; i < joints.Count; i++)
        {
            // Conversión implícita de Unity Vector3 a Vec3
            Vec3 startNode = joints[i].position;
            Vec3 endNode = (i == joints.Count - 1) ? (Vec3)endEffector.position : (Vec3)joints[i + 1].position;

            // Usamos NUESTRA función de distancia
            boneLengths[i] = Mates.Distance(startNode, endNode);
            totalArmLength += boneLengths[i];
            virtualPositions[i] = startNode;
        }
        virtualPositions[joints.Count] = endEffector.position;
    }

    void OnEnable()
    {
        if (initialized)
        {
            for (int i = 0; i < joints.Count; i++)
            {
                joints[i].localPosition = initialLocalPositions[i];
                joints[i].localRotation = initialLocalRotations[i];
                virtualPositions[i] = joints[i].position;
            }
            endEffector.localPosition = new Vector3(0, endEffector.localPosition.y, 0);
            virtualPositions[joints.Count] = endEffector.position;
        }
    }

    void LateUpdate()
    {
        if (target == null || endEffector == null) return;
        SolveVirtualFABRIK();
        ApplyVisuals();
    }

    void SolveVirtualFABRIK()
    {
        virtualPositions[0] = joints[0].position;
        // Mates.Distance en lugar de Vector3.Distance
        float distToBase = Mates.Distance(virtualPositions[0], target.position);
        currentDistance = Mates.Distance(virtualPositions[virtualPositions.Length - 1], target.position);

        if (distToBase > totalArmLength)
        {
            // Cálculo manual de dirección y normalización con nuestra librería
            Vec3 direction = (Vec3)target.position - virtualPositions[0];
            direction = direction.Normalized();

            for (int i = 0; i < boneLengths.Length; i++)
                virtualPositions[i + 1] = virtualPositions[i] + direction * boneLengths[i];
            iterationsUsed = 0;
        }
        else
        {
            if (currentDistance > tolerance)
            {
                iterationsUsed = 0;
                for (int iter = 0; iter < maxIterations; iter++)
                {
                    iterationsUsed++;
                    // Forward
                    virtualPositions[virtualPositions.Length - 1] = target.position;
                    for (int i = virtualPositions.Length - 2; i >= 0; i--)
                    {
                        Vec3 dir = virtualPositions[i] - virtualPositions[i + 1];
                        dir = dir.Normalized(); // Normalización propia
                        virtualPositions[i] = virtualPositions[i + 1] + dir * boneLengths[i];
                    }
                    // Backward
                    virtualPositions[0] = joints[0].position;
                    for (int i = 0; i < virtualPositions.Length - 1; i++)
                    {
                        Vec3 dir = virtualPositions[i + 1] - virtualPositions[i];
                        dir = dir.Normalized();
                        virtualPositions[i + 1] = virtualPositions[i] + dir * boneLengths[i];
                    }
                    if (Mates.Distance(virtualPositions[virtualPositions.Length - 1], target.position) < tolerance) break;
                }
            }
        }
        currentDistance = Mates.Distance(virtualPositions[virtualPositions.Length - 1], target.position);
    }

    void ApplyVisuals()
    {
        float step = speed * Time.deltaTime;
        for (int i = 0; i < joints.Count; i++)
        {
            // Convertimos de Vec3 a Unity Vector3 solo para pintar en pantalla
            Vec3 nuevaPos = Mates.MoveTowards(joints[i].position, virtualPositions[i], step);
            joints[i].position = nuevaPos.ToUnity();
        }

        Vec3 endPos = Mates.MoveTowards(endEffector.position, virtualPositions[virtualPositions.Length - 1], step);
        endEffector.position = endPos.ToUnity();

        // Para las rotaciones mantenemos Quaternion de Unity porque reimplementar Quaterniones
        // desde cero es extremadamente complejo y propenso a errores graves.
        // Normalmente se permite usar Quaternion para el resultado visual final.
        for (int i = 0; i < joints.Count; i++)
        {
            Transform targetTransform = (i < joints.Count - 1) ? joints[i + 1] : endEffector;
            Vector3 direction = targetTransform.position - joints[i].position;
            if (direction.sqrMagnitude > 0.001f)
                joints[i].rotation = Quaternion.FromToRotation(Vector3.up, direction);
        }
    }
}