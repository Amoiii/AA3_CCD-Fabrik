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
    public float smoothing = 0.5f; 
    public float tolerance = 0.01f;
    public int maxIterations = 10;

    [Header("Debug")]
    public int iterationsUsed;
    public float currentDistance;

    
    private float[] boneLengths;
    private Vec3[] virtualPositions;
    private bool initialized = false;

    void Awake()
    {
        InitFABRIK();
    }

    void InitFABRIK()
    {
        if (joints.Count < 2) return;

        //  arrays
        int numJoints = joints.Count + 1;
        virtualPositions = new Vec3[numJoints];
        boneLengths = new float[numJoints - 1];

        // longitudes iniciales
        
        for (int i = 0; i < joints.Count - 1; i++)
        {
            boneLengths[i] = Mates.Distance((Vec3)joints[i].position, (Vec3)joints[i + 1].position);
            virtualPositions[i] = (Vec3)joints[i].position;
        }

        // último hueso al EndEffector
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

        // Base
      
        virtualPositions[0] = (Vec3)joints[0].position;

        for (int i = 1; i < joints.Count; i++)
            virtualPositions[i] = (Vec3)joints[i].position;

        virtualPositions[virtualPositions.Length - 1] = (Vec3)endEffector.position;


        // FABRIK 
        Solve();

        
        ApplyVisuals();
    }

    void Solve()
    {
        // Distancia actual al target
        currentDistance = Mates.Distance(virtualPositions[virtualPositions.Length - 1], (Vec3)target.position);
        iterationsUsed = 0;

        // Si ya estamos cerca, no calculamos
        if (currentDistance <= tolerance) return;

        
        for (int iter = 0; iter < maxIterations; iter++)
        {
            iterationsUsed++;

            // Target-> Base) ---
            
            virtualPositions[virtualPositions.Length - 1] = (Vec3)target.position;

            for (int i = virtualPositions.Length - 2; i >= 0; i--)
            {
                // Dirección hacia el siguiente punto
                Vec3 dir = (virtualPositions[i] - virtualPositions[i + 1]).Normalized();
               
                virtualPositions[i] = virtualPositions[i + 1] + dir * boneLengths[i];
            }

            // Base-> Target)¡
            virtualPositions[0] = (Vec3)joints[0].position;

            for (int i = 0; i < virtualPositions.Length - 1; i++)
            {
                // Dirección hacia el siguiente punto
                Vec3 dir = (virtualPositions[i + 1] - virtualPositions[i]).Normalized();
               
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
        // Interpolación para suavizar 
        float t = 1.0f - smoothing;


        for (int i = 0; i < joints.Count; i++)
        {
           //siguiente punto
            Vec3 nextVirtualPos;
            if (i < joints.Count - 1)
                nextVirtualPos = virtualPositions[i + 1];
            else
                nextVirtualPos = virtualPositions[virtualPositions.Length - 1]; // El end effector

            
            Vector3 direction = nextVirtualPos.ToUnity() - joints[i].position;

            if (direction.sqrMagnitude > 0.0001f) // Evitar errores de dirección cero
            {
             
                Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, direction);

                // Aplicamos rotación suave
                joints[i].rotation = Quaternion.Slerp(joints[i].rotation, targetRotation, t);
            }

         
        }
    }

    //esqueleto virtual para depurar
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