using UnityEngine;
using System.Collections.Generic;

public class FABRIK : MonoBehaviour
{
    [Header("Configuración")]
    public List<Transform> joints;
    public Transform endEffector;
    public Transform target;

    [Range(0, 1)]
    public float smoothing = 0.5f;
    public float tolerance = 0.01f;
    public int maxIterations = 10;

    public int iterationsUsed;
    public float currentDistance;

    private float[] boneLengths;
    private Vector3[] positions;
    private Vector3 startPosition;
    private float totalArmLength;

   
    private List<Vector3> initialJointPositions = new List<Vector3>();
    private List<Quaternion> initialJointRotations = new List<Quaternion>();
    private bool initialized = false;

    void Awake()
    {
       
        foreach (Transform j in joints)
        {
            initialJointPositions.Add(j.position);
            initialJointRotations.Add(j.rotation);
        }
        initialized = true;
    }

    void OnEnable()
    {
        // RESETEAR
        if (initialized && joints.Count > 0)
        {
            for (int i = 0; i < joints.Count; i++)
            {
                joints[i].position = initialJointPositions[i];
                joints[i].rotation = initialJointRotations[i];
                // Resetear también el array matemático
                if (positions != null && positions.Length > i) positions[i] = initialJointPositions[i];
            }
            if (endEffector != null && positions != null)
                positions[positions.Length - 1] = endEffector.position;
        }
    }

    void Start()
    {
        if (joints.Count == 0 || endEffector == null || target == null) return;

        int totalNodes = joints.Count + 1;
        positions = new Vector3[totalNodes];
        boneLengths = new float[totalNodes - 1];
        startPosition = joints[0].position;
        totalArmLength = 0;

        for (int i = 0; i < joints.Count; i++)
        {
            Vector3 startNode = joints[i].position;
            Vector3 endNode = (i == joints.Count - 1) ? endEffector.position : joints[i + 1].position;
            boneLengths[i] = Vector3.Distance(startNode, endNode);
            totalArmLength += boneLengths[i];
            positions[i] = startNode;
        }
        positions[joints.Count] = endEffector.position;
    }

    void LateUpdate()
    {
        if (target == null || endEffector == null) return;

        SolveFABRIK();
        ApplyVisualsSmoothed();
    }

    void SolveFABRIK()
    {
        startPosition = joints[0].position;
        float distanceToTarget = Vector3.Distance(startPosition, target.position);
        currentDistance = Vector3.Distance(positions[positions.Length - 1], target.position);

        if (distanceToTarget > totalArmLength)
        {
            Vector3 direction = (target.position - startPosition).normalized;
            positions[0] = startPosition;
            for (int i = 0; i < boneLengths.Length; i++)
                positions[i + 1] = positions[i] + direction * boneLengths[i];
            iterationsUsed = 0;
        }
        else
        {
            positions[0] = startPosition;
            if (currentDistance > tolerance)
            {
                for (int iter = 0; iter < maxIterations; iter++)
                {
                    iterationsUsed++;
                    // Forward
                    positions[positions.Length - 1] = target.position;
                    for (int i = positions.Length - 2; i >= 0; i--)
                    {
                        Vector3 dir = (positions[i] - positions[i + 1]).normalized;
                        positions[i] = positions[i + 1] + dir * boneLengths[i];
                    }
                    // Backward
                    positions[0] = startPosition;
                    for (int i = 0; i < positions.Length - 1; i++)
                    {
                        Vector3 dir = (positions[i + 1] - positions[i]).normalized;
                        positions[i + 1] = positions[i] + dir * boneLengths[i];
                    }
                    if (Vector3.Distance(positions[positions.Length - 1], target.position) < tolerance) break;
                }
            }
        }
    }

    void ApplyVisualsSmoothed()
    {
        float t = 1.0f - smoothing;
        for (int i = 0; i < joints.Count; i++)
            joints[i].position = Vector3.Lerp(joints[i].position, positions[i], t);

        endEffector.position = Vector3.Lerp(endEffector.position, positions[positions.Length - 1], t);

        for (int i = 0; i < joints.Count; i++)
        {
            Vector3 targetPos = (i < joints.Count - 1) ? joints[i + 1].position : endEffector.position;
            Vector3 direction = targetPos - joints[i].position;
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, direction);
                joints[i].rotation = Quaternion.Slerp(joints[i].rotation, targetRotation, t);
            }
        }
    }
}