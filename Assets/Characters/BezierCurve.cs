using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class BezierCurve : MonoBehaviour
{
    [SerializeField] private List<Transform> controlPoints;
    [SerializeField] private List<Transform> curvePoints;
    [SerializeField] private List<Vector3> rotationOffsets = new List<Vector3>();
    [SerializeField] private bool showControlPoints = false;

    [SerializeField] private Vector3 rotationOffset = Vector3.zero;
    [SerializeField] public bool applyTransformDirectly = false;

    private List<Vector3> positions = new List<Vector3>();
    private List<Vector3> rotationDirections = new List<Vector3>();
    // Start is called before the first frame update
    void Start()
    {

    }
    private void Update()
    {
        if (!applyTransformDirectly) return;

        RefreshCurve();
    }

    public void RefreshCurve()
    {
        if (controlPoints == null || curvePoints == null) return;
        if (controlPoints.Count < 2 || curvePoints.Count < 1) return;

        positions.Clear();
        rotationDirections.Clear();

        CalculateCurvePoints();
        RotateCurvePoints();
        ApplyTransforms();
    }

    private void CalculateCurvePoints()
    {
        for (int i = 0; i < curvePoints.Count; i++)
        {
            float t = (float)i / (curvePoints.Count - 1);

            // Depends on amount of control points
            Vector3 pointOnCurve = Vector3.zero;

            int recCount = controlPoints.Count - 1;
            for (int iS = 0; iS < controlPoints.Count; iS++)
            {
                int iE = (recCount - iS);
                pointOnCurve +=
                    Choose(recCount, iS) *
                    Mathf.Pow((1 - t), iE) *
                    Mathf.Pow(t, iS) *
                    controlPoints[iS].position;
            }
            positions.Add(pointOnCurve);
        }
    }

    private void RotateCurvePoints()
    {
        Vector3 direction = Vector3.zero;
        for (int i = 0; i < curvePoints.Count - 1; i++)
        {
            direction = positions[i + 1] - positions[i];
            rotationDirections.Add(direction);
        }
        rotationDirections.Add(direction);
    }

    private void ApplyTransforms()
    {
        for (int i = 0; i < curvePoints.Count; i++)
        {
            curvePoints[i].rotation = Quaternion.identity;

            curvePoints[i].rotation = Quaternion.LookRotation(rotationDirections[i], Vector3.up);
            //curvePoints[i].right = rotationDirections[i];
            //curvePoints[i].rotation = Quaternion.identity;
            curvePoints[i].Rotate(rotationOffsets[i]);

            curvePoints[i].position = positions[i];

        }
    }

    int Factorial(int integer)
    {
        if (integer <= 0)
            return 1;
        int newInt = 1;
        for (int i = 2; i <= integer; i++)
            newInt *= i;
        return newInt;
    }

    int Choose(int objects, int sample)
    {
        if ((Factorial(sample) * Factorial(objects - sample) == 0))
        { Debug.Log("divide by zero error"); return 0; }

        int output = Factorial(objects) / (Factorial(sample) * Factorial(objects - sample));

        return Factorial(objects) / (Factorial(sample) * Factorial(objects - sample));
    }

    private void OnDrawGizmos()
    {
        if (!showControlPoints) return;
        Gizmos.color = Color.red;
        foreach (Transform t in controlPoints)
        {
            Gizmos.DrawSphere(t.position, 0.025f);
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            Gizmos.DrawLine(controlPoints[i].position, controlPoints[i + 1].position);
        }

        Gizmos.color = Color.green;
        for (int i = 0; i < curvePoints.Count - 1; i++)
        {
            Gizmos.DrawLine(curvePoints[i].position, curvePoints[i + 1].position);
        }
    }
}
