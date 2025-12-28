using UnityEngine;
using System.Collections.Generic;

public class CCD : MonoBehaviour
{
    [Header("Configuración del Brazo")]
    public List<Transform> bones;
    public Transform endEffector;
    public Transform target;

    [Header("Parámetros (Teoría)")]
    public float tolerance = 0.01f; // Tolerancia baja para precisión
    public int maxIterations = 10;

    [Header("Movimiento Natural")]
    [Range(0.01f, 5f)]
    public float smoothness = 0.5f; // NUEVO: Velocidad de giro (Bajo = Lento/Natural, Alto = Rápido)
    // Recomendado: entre 0.1 y 1.0 para que se vea el movimiento.

    [Header("Restricciones (Constraints)")]
    [Range(0, 180)]
    public float angleLimit = 90f;

    [Header("Debug")]
    public int iterationsUsed;
    public float currentDistance;

    void LateUpdate()
    {
        if (target == null || endEffector == null) return;

        currentDistance = Vector3.Distance(endEffector.position, target.position);
        iterationsUsed = 0;

        // Aunque estemos cerca, si queremos movimiento suave continuo, 
        // a veces conviene seguir ejecutando con límite de velocidad,
        // pero por rendimiento mantenemos el freno de tolerancia.
        if (currentDistance <= tolerance) return;

        SolveCCD();
    }

    void SolveCCD()
    {
        for (int i = 0; i < maxIterations; i++)
        {
            iterationsUsed++;

            for (int j = bones.Count - 1; j >= 0; j--)
            {
                Transform currentBone = bones[j];

                Vector3 toEnd = (endEffector.position - currentBone.position).normalized;
                Vector3 toTarget = (target.position - currentBone.position).normalized;

                if (toEnd == Vector3.zero || toTarget == Vector3.zero) continue;

                // 1. Eje de rotación
                Vector3 rotationAxis = Vector3.Cross(toEnd, toTarget).normalized;
                if (rotationAxis.sqrMagnitude < 0.001f) continue;

                // 2. Ángulo necesario
                float dotProduct = Mathf.Clamp(Vector3.Dot(toEnd, toTarget), -1f, 1f);
                float angleDegrees = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

                // --- CLAVE DEL MOVIMIENTO NATURAL ---
                // Aquí está el truco: Limitamos el ángulo MÁXIMO por paso.
                // En lugar de girar todo lo necesario (angleDegrees), giramos solo un poquito ('smoothness').
                // Al repetirse esto frame a frame, se crea la animación de acercamiento.

                if (Mathf.Abs(angleDegrees) > smoothness)
                {
                    // Si el ángulo es positivo o negativo, aplicamos solo el paso máximo en esa dirección
                    angleDegrees = Mathf.Sign(angleDegrees) * smoothness;
                }

                // Aplicamos la rotación limitada
                currentBone.Rotate(rotationAxis, angleDegrees, Space.World);

                // --- CONSTRAINTS ---
                if (j > 0)
                {
                    Transform parentBone = bones[j - 1];
                    Quaternion localRot = Quaternion.Inverse(parentBone.rotation) * currentBone.rotation;

                    if (Quaternion.Angle(Quaternion.identity, localRot) > angleLimit)
                    {
                        currentBone.rotation = parentBone.rotation * Quaternion.RotateTowards(Quaternion.identity, localRot, angleLimit);
                    }
                }
            }

            // Comprobación de salida
            currentDistance = Vector3.Distance(endEffector.position, target.position);
            if (currentDistance <= tolerance) break;
        }
    }
}