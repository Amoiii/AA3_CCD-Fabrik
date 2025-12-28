using UnityEngine;
using System.Collections.Generic;

public class CCD : MonoBehaviour
{
    [Header("Configuración del Brazo")]
    public List<Transform> bones;
    public Transform endEffector;
    public Transform target;

    [Header("Parámetros (Teoría)")]
    public float tolerance = 0.1f;
    public int maxIterations = 10;

    [Header("Restricciones (Constraints)")]
    [Range(0, 180)]
    public float angleLimit = 90f; // NUEVO: Límite de giro (ej. un codo no gira 360º)

    [Header("Debug")]
    public int iterationsUsed;
    public float currentDistance;

    void LateUpdate()
    {
        if (target == null || endEffector == null) return;

        currentDistance = Vector3.Distance(endEffector.position, target.position);
        iterationsUsed = 0;

        if (currentDistance <= tolerance) return;

        SolveCCD();
    }

    void SolveCCD()
    {
        for (int i = 0; i < maxIterations; i++)
        {
            iterationsUsed++;

            // Recorremos desde el último hueso hasta la base
            for (int j = bones.Count - 1; j >= 0; j--)
            {
                Transform currentBone = bones[j];

                // Vectores hacia el efector y hacia el objetivo
                Vector3 toEnd = (endEffector.position - currentBone.position).normalized;
                Vector3 toTarget = (target.position - currentBone.position).normalized;

                if (toEnd == Vector3.zero || toTarget == Vector3.zero) continue;

                // 1. Calcular rotación necesaria
                Vector3 rotationAxis = Vector3.Cross(toEnd, toTarget).normalized;
                if (rotationAxis.sqrMagnitude < 0.001f) continue;

                float dotProduct = Mathf.Clamp(Vector3.Dot(toEnd, toTarget), -1f, 1f);
                float angleDegrees = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

                // Suavizado (Damping)
                angleDegrees = Mathf.Clamp(angleDegrees, -80f, 80f);

                if (Mathf.Abs(angleDegrees) < 0.001f) continue;

                // 2. Aplicar rotación
                currentBone.Rotate(rotationAxis, angleDegrees, Space.World);

                // --- NUEVO: APLICAR CONSTRAINTS (Límites Angulares) ---
                // Esto impide giros antinaturales que causan colisiones raras
                if (j > 0) // No limitamos la base, solo codos/muñecas
                {
                    Transform parentBone = bones[j - 1]; // O el padre en la jerarquía
                    // Calculamos el ángulo local respecto al padre
                    Quaternion localRot = Quaternion.Inverse(parentBone.rotation) * currentBone.rotation;

                    // Si el ángulo supera el límite, lo corregimos
                    if (Quaternion.Angle(Quaternion.identity, localRot) > angleLimit)
                    {
                        // Buscamos la rotación válida más cercana
                        currentBone.rotation = parentBone.rotation * Quaternion.RotateTowards(Quaternion.identity, localRot, angleLimit);
                    }
                }
            }

            currentDistance = Vector3.Distance(endEffector.position, target.position);
            if (currentDistance <= tolerance) break;
        }
    }
}