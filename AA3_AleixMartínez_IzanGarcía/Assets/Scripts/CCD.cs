using UnityEngine;
using System.Collections.Generic;

public class CCD : MonoBehaviour
{
    [Header("Configuración del Brazo")]
    // Arrastra aquí tus articulaciones EN ORDEN (Base -> Codo -> Muñeca)
    // NO incluyas el EndEffector (la punta roja) en esta lista, solo los huesos que giran.
    public List<Transform> bones;

    public Transform endEffector; // La bolita roja (hija del último hueso)
    public Transform target;      // La esfera verde que persigue

    [Header("Parámetros (Teoría)")]
    public float tolerance = 0.1f;
    public int maxIterations = 10; // Con 10 suele sobrar si es por frame

    [Header("Debug / UI (No tocar)")]
    public int iterationsUsed;
    public float currentDistance;

    void Start()
    {
        // Validación básica para evitar errores tontos
        if (bones == null || bones.Count == 0) Debug.LogError("¡La lista de 'Bones' está vacía!");
        if (endEffector == null) Debug.LogError("¡Falta asignar el EndEffector!");
        if (target == null) Debug.LogError("¡Falta asignar el Target!");
    }

    void LateUpdate() // Usamos LateUpdate para sobreescribir cualquier animación si la hubiera
    {
        if (target == null || endEffector == null) return;

        // 1. Calculamos distancia inicial
        currentDistance = Vector3.Distance(endEffector.position, target.position);
        iterationsUsed = 0;

        // 2. Si ya estamos cerca, no gastamos procesador
        if (currentDistance <= tolerance) return;

        // 3. Ejecutamos el algoritmo CCD
        SolveCCD();
    }

    void SolveCCD()
    {
        // Bucle de iteraciones (simulando los intentos del algoritmo)
        for (int i = 0; i < maxIterations; i++)
        {
            iterationsUsed++;

            // Recorremos los huesos desde el ÚLTIMO hacia el PRIMERO (Base)
            // Esto es la definición de "Cyclic Coordinate Descent"
            for (int j = bones.Count - 1; j >= 0; j--)
            {
                Transform currentBone = bones[j];

                // --- MATEMÁTICA DE LA TEORÍA ---

                // Vector A: Desde el hueso actual hasta la punta del brazo (EndEffector)
                Vector3 toEnd = (endEffector.position - currentBone.position).normalized;

                // Vector B: Desde el hueso actual hasta el objetivo (Target)
                Vector3 toTarget = (target.position - currentBone.position).normalized;

                // Evitar errores si los vectores son cero
                if (toEnd == Vector3.zero || toTarget == Vector3.zero) continue;

                // 1. EJE DE ROTACIÓN (Producto Vectorial / Cross Product)
                // Nos da el vector perpendicular sobre el que tenemos que girar
                Vector3 rotationAxis = Vector3.Cross(toEnd, toTarget).normalized;

                // Si el eje es muy pequeño (vectores paralelos), no rotamos
                if (rotationAxis.sqrMagnitude < 0.001f) continue;

                // 2. ÁNGULO DE ROTACIÓN (Producto Escalar / Dot Product)
                // Nos da el coseno del ángulo
                float dotProduct = Mathf.Clamp(Vector3.Dot(toEnd, toTarget), -1f, 1f);
                float angleDegrees = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

                // Limitar la velocidad de giro para evitar "tembleques" (Damping)
                // Si pide girar 90º, le dejamos solo 30º por paso para que sea suave
                angleDegrees = Mathf.Clamp(angleDegrees, -80f, 80f); // Puedes cambiar este 80

                // Si el ángulo es muy pequeño, saltamos
                if (Mathf.Abs(angleDegrees) < 0.1f) continue;

                // --- APLICACIÓN EN UNITY ---
                // Rotamos el hueso REAL en el mundo. Unity moverá automáticamente a los hijos (EndEffector)
                currentBone.Rotate(rotationAxis, angleDegrees, Space.World);
            }

            // Comprobar si ya hemos llegado tras esta pasada
            currentDistance = Vector3.Distance(endEffector.position, target.position);
            if (currentDistance <= tolerance) break;
        }
    }
}