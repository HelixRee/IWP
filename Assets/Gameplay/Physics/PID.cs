using UnityEngine;

[System.Serializable]
public class PID
{
    public float pFactor, iFactor, dFactor;

    float integral;
    float lastError;


    public PID(float pFactor, float iFactor, float dFactor)
    {
        this.pFactor = pFactor;
        this.iFactor = iFactor;
        this.dFactor = dFactor;
    }


    public float Update(float setpoint, float actual, float timeFrame)
    {
        float present = setpoint - actual;
        integral += present * timeFrame;
        float deriv = (present - lastError) / timeFrame;
        lastError = present;
        return present * pFactor + integral * iFactor + deriv * dFactor;
    }
}

[System.Serializable]
public class PIDVec3
{
    public float pFactor, iFactor, dFactor;

    Vector3 integral;
    Vector3 lastError;


    public PIDVec3(float pFactor, float iFactor, float dFactor)
    {
        this.pFactor = pFactor;
        this.iFactor = iFactor;
        this.dFactor = dFactor;
    }


    public Vector3 Update(Vector3 setpoint, Vector3 actual, float timeFrame)
    {
        Vector3 present = setpoint - actual;
        integral += present * timeFrame;
        Vector3 deriv = (present - lastError) / timeFrame;
        lastError = present;
        return present * pFactor + integral * iFactor + deriv * dFactor;
    }
}