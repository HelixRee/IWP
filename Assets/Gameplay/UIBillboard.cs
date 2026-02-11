using UnityEngine;

public class UIBillboard : MonoBehaviour
{
    private void LateUpdate()
    {
        Vector2 camPos = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
        Vector2 selfPos = new Vector2(transform.position.x, transform.position.z);

        Vector2 dir = camPos - selfPos;
        float angle = Vector2.SignedAngle(-dir, Vector2.up);

        Vector3 eulerAngles = transform.eulerAngles;
        eulerAngles.y = angle;
        transform.eulerAngles = eulerAngles;
    }
}
