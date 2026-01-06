using UnityEngine;
using System.Collections.Generic;

public class CCD : MonoBehaviour
{
    [Header("Configuración")]
    public List<Transform> bones;
    public Transform endEffector;
    public Transform target;

    [Header("Parámetros")]
    public float tolerance = 0.01f;
    public int maxIterations = 10;
    [Range(0.01f, 5f)] public float smoothness = 0.5f;
    [Range(0, 180)] public float angleLimit = 90f;

    [Header("Debug")]
    public int iterationsUsed;
    public float currentDistance;

    private List<Quaternion> initialRotations = new List<Quaternion>();
    private bool initialized = false;

    void Awake()
    {
        foreach (Transform bone in bones) initialRotations.Add(bone.rotation);
        initialized = true;
    }

    void OnEnable()
    {
        if (initialized)
            for (int i = 0; i < bones.Count; i++) bones[i].rotation = initialRotations[i];
    }

    void LateUpdate()
    {
        if (target == null || endEffector == null) return;

        
        currentDistance = Mates.Distance((Vec3)endEffector.position, (Vec3)target.position);
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

                // Convertimos a nuestros vectores Vec3
                Vec3 bonePos = currentBone.position;
                Vec3 endPos = endEffector.position;
                Vec3 targetPos = target.position;

                // Cálculo de vectores dirección propios
                Vec3 toEnd = (endPos - bonePos).Normalized();
                Vec3 toTarget = (targetPos - bonePos).Normalized();

                // Usamos Mates.Cross y Mates.Dot
                Vec3 rotationAxis = Mates.Cross(toEnd, toTarget).Normalized();
                if (rotationAxis.Magnitude() < 0.001f) continue;

                float dotProduct = Mates.Clamp(Mates.Dot(toEnd, toTarget), -1f, 1f);
                float angleDegrees = Mates.Acos(dotProduct) * Mates.Rad2Deg;

                if (Mates.Abs(angleDegrees) > smoothness)
                    angleDegrees = Mates.Sign(angleDegrees) * smoothness;

                // Aplicamos la rotación. 
                
                currentBone.Rotate(rotationAxis.ToUnity(), angleDegrees, Space.World);

                // Constraints
                if (j > 0)
                {
                    Transform parentBone = bones[j - 1];
                    Quaternion localRot = Quaternion.Inverse(parentBone.rotation) * currentBone.rotation;
                    if (Quaternion.Angle(Quaternion.identity, localRot) > angleLimit)
                        currentBone.rotation = parentBone.rotation * Quaternion.RotateTowards(Quaternion.identity, localRot, angleLimit);
                }
            }
            if (Mates.Distance((Vec3)endEffector.position, (Vec3)target.position) <= tolerance) break;
        }
    }
}