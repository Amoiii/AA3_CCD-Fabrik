using System; // Usamos System para matemáticas básicas (Raíz cuadrada), esto SÍ está permitido.

// -------------------------------------------------------------------------
// MI PROPIA LIBRERÍA DE VECTORES (Para no usar Unity.Vector3)
// -------------------------------------------------------------------------
[System.Serializable]
public struct Vec3
{
    public float x, y, z;

    public Vec3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    // Convertir de Unity a Mi Vector
    public static implicit operator Vec3(UnityEngine.Vector3 v)
    {
        return new Vec3(v.x, v.y, v.z);
    }

    // Convertir de Mi Vector a Unity (Para poder mover los objetos al final)
    public UnityEngine.Vector3 ToUnity()
    {
        return new UnityEngine.Vector3(x, y, z);
    }

    // --- OPERACIONES IMPLEMENTADAS A MANO ---

    public static Vec3 operator +(Vec3 a, Vec3 b) => new Vec3(a.x + b.x, a.y + b.y, a.z + b.z);
    public static Vec3 operator -(Vec3 a, Vec3 b) => new Vec3(a.x - b.x, a.y - b.y, a.z - b.z);
    public static Vec3 operator *(Vec3 a, float d) => new Vec3(a.x * d, a.y * d, a.z * d);
    public static Vec3 operator /(Vec3 a, float d) => new Vec3(a.x / d, a.y / d, a.z / d);

    // Magnitud (Pitágoras)
    public float Magnitude()
    {
        return (float)Math.Sqrt(x * x + y * y + z * z);
    }

    // Normalizar (Hacer que mida 1)
    public Vec3 Normalized()
    {
        float mag = Magnitude();
        if (mag > 0.00001f)
            return this / mag;
        return new Vec3(0, 0, 0);
    }
}

// -------------------------------------------------------------------------
// MI PROPIA LIBRERÍA DE MATEMÁTICAS (Para no usar Unity.Mathf)
// -------------------------------------------------------------------------
public static class Mates
{
    public const float PI = 3.14159265359f;
    public const float Rad2Deg = 180f / PI;

    // Distancia entre dos puntos
    public static float Distance(Vec3 a, Vec3 b)
    {
        float diffX = a.x - b.x;
        float diffY = a.y - b.y;
        float diffZ = a.z - b.z;
        return (float)Math.Sqrt(diffX * diffX + diffY * diffY + diffZ * diffZ);
    }

    // Producto Escalar (Dot Product)
    public static float Dot(Vec3 a, Vec3 b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }

    // Producto Vectorial (Cross Product)
    public static Vec3 Cross(Vec3 a, Vec3 b)
    {
        return new Vec3(
            a.y * b.z - a.z * b.y,
            a.z * b.x - a.x * b.z,
            a.x * b.y - a.y * b.x
        );
    }

    // Clamp (Limitar valor entre min y max)
    public static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    // Acos (Arco Coseno)
    public static float Acos(float val)
    {
        return (float)Math.Acos(val);
    }

    // MoveTowards (Moverse hacia un punto con velocidad constante)
    public static Vec3 MoveTowards(Vec3 current, Vec3 target, float maxDistDelta)
    {
        Vec3 a = target - current;
        float magnitude = a.Magnitude();
        if (magnitude <= maxDistDelta || magnitude == 0f)
        {
            return target;
        }
        return current + a / magnitude * maxDistDelta;
    }

    public static float Abs(float val) => Math.Abs(val);
    public static float Sign(float val) => Math.Sign(val);
}