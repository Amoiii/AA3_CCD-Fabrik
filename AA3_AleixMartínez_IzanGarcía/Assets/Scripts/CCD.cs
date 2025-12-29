using UnityEngine;
using System.Collections.Generic;

public class CCD : MonoBehaviour
{
    [Header("Configuración del Brazo")]
    public List<Transform> bones;
    public Transform endEffector;
    public Transform target;

    [Header("Parámetros")]
    public float tolerance = 0.01f;
    public int maxIterations = 10;
    [Range(0.01f, 5f)]
    public float smoothness = 0.5f;
    [Range(0, 180)]
    public float angleLimit = 90f;

    [Header("Debug")]
    public int iterationsUsed;
    public float currentDistance;

    // --- NUEVO: Para guardar la postura inicial ---
    private List<Quaternion> initialRotations = new List<Quaternion>();
    private bool initialized = false;

    void Awake()
    {
        // Guardamos la postura "T-Pose" del brazo al arrancar
        foreach (Transform bone in bones)
        {
            initialRotations.Add(bone.rotation);
        }
        initialized = true;
    }

    void OnEnable()
    {
        // ESTO ES LO QUE PIDES:
        // Cada vez que se active este objeto (tecla 1), el brazo vuelve a su sitio.
        if (initialized)
        {
            for (int i = 0; i < bones.Count; i++)
            {
                bones[i].rotation = initialRotations[i];
            }
        }
    }

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
            for (int j = bones.Count - 1; j >= 0; j--)
            {
                Transform currentBone = bones[j];
                Vector3 toEnd = (endEffector.position - currentBone.position).normalized;
                Vector3 toTarget = (target.position - currentBone.position).normalized;

                if (toEnd == Vector3.zero || toTarget == Vector3.zero) continue;

                Vector3 rotationAxis = Vector3.Cross(toEnd, toTarget).normalized;
                if (rotationAxis.sqrMagnitude < 0.001f) continue;

                float dotProduct = Mathf.Clamp(Vector3.Dot(toEnd, toTarget), -1f, 1f);
                float angleDegrees = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

                if (Mathf.Abs(angleDegrees) > smoothness)
                    angleDegrees = Mathf.Sign(angleDegrees) * smoothness;

                currentBone.Rotate(rotationAxis, angleDegrees, Space.World);

                if (j > 0)
                {
                    Transform parentBone = bones[j - 1];
                    Quaternion localRot = Quaternion.Inverse(parentBone.rotation) * currentBone.rotation;
                    if (Quaternion.Angle(Quaternion.identity, localRot) > angleLimit)
                        currentBone.rotation = parentBone.rotation * Quaternion.RotateTowards(Quaternion.identity, localRot, angleLimit);
                }
            }
            if (Vector3.Distance(endEffector.position, target.position) <= tolerance) break;
        }
    }
}