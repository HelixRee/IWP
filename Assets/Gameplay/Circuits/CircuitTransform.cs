using UnityEngine;

public class CircuitTransform : CircuitComponent
{
    [SerializeField] private Transform _onPosition;
    [SerializeField] private Transform _offPosition;

    override protected void Update()
    {
        base.Update();
        if (isPowered)
        {
            transform.position = Vector3.Lerp(transform.position, _onPosition.position, 12f * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, _onPosition.rotation, 12f * Time.deltaTime);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, _offPosition.position, 12f * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, _offPosition.rotation, 12f * Time.deltaTime);
        }
    }
}
