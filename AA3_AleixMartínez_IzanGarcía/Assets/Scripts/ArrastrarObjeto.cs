using UnityEngine;

public class ArrastrarObjeto : MonoBehaviour
{
    private Vector3 mOffset;
    private float mZCoord;

    void OnMouseDown()
    {
        // 1. Calcular la profundidad (Z) del objeto respecto a la cámara
        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;

        // 2. Calcular la diferencia entre donde hago clic y el centro del objeto
        // (Para que no pegue un salto si hago clic en el borde de la bola)
        mOffset = gameObject.transform.position - GetMouseAsWorldPoint();
    }

    void OnMouseDrag()
    {
        // 3. Mientras arrastro, actualizo la posición sumando el offset
        transform.position = GetMouseAsWorldPoint() + mOffset;
    }

    private Vector3 GetMouseAsWorldPoint()
    {
        // Pixel coordinates of mouse (x,y)
        Vector3 mousePoint = Input.mousePosition;

        // z coordinate of game object on screen
        mousePoint.z = mZCoord;

        // Convert it to world points
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}