using System;
using System.Xml.Linq;
using UnityEngine;

public class IKHelper
{
    /******************************************************************************
      Copyright (c) 2008-2009 Ryan Juckett
      http://www.ryanjuckett.com/
     
      This software is provided 'as-is', without any express or implied
      warranty. In no event will the authors be held liable for any damages
      arising from the use of this software.
     
      Permission is granted to anyone to use this software for any purpose,
      including commercial applications, and to alter it and redistribute it
      freely, subject to the following restrictions:
     
      1. The origin of this software must not be misrepresented; you must not
         claim that you wrote the original software. If you use this software
         in a product, an acknowledgment in the product documentation would be
         appreciated but is not required.
     
      2. Altered source versions must be plainly marked as such, and must not be
         misrepresented as being the original software.
     
      3. This notice may not be removed or altered from any source
         distribution.
    ******************************************************************************/

    ///***************************************************************************************
    /// CalcIK_2D_TwoBoneAnalytic
    /// Given a two bone chain located at the origin (bone1 is the parent of bone2), this
    /// function will compute the bone angles needed for the end of the chain to line up
    /// with a handTarget position. If there is no valid solution, the angles will be set to
    /// get as close to the handTarget as possible.
    ///  
    /// returns: True when a valid solution was found.
    ///***************************************************************************************
    public static bool CalcIK_2D_TwoBoneAnalytic
    (
        out float angle1,   // Angle of bone 1
        out float angle2,   // Angle of bone 2
        bool solvePosAngle2, // Solve for positive angle 2 instead of negative angle 2
        float length1,      // Length of bone 1. Assumed to be >= zero
        float length2,      // Length of bone 2. Assumed to be >= zero
        float targetX,      // Target x position for the bones to reach
        float targetY       // Target y position for the bones to reach
    )
    {
        Debug.Assert(length1 >= 0);
        Debug.Assert(length2 >= 0);

        const float epsilon = 0.0001f; // used to prevent division by small numbers

        bool foundValidSolution = true;

        float targetDistSqr = (targetX * targetX + targetY * targetY);

        //===
        // Compute a new value for angle2 along with its cosine
        float sinAngle2;
        float cosAngle2;

        float cosAngle2_denom = 2 * length1 * length2;
        if (cosAngle2_denom > epsilon)
        {
            cosAngle2 = (targetDistSqr - length1 * length1 - length2 * length2)
                        / (cosAngle2_denom);

            // if our result is not in the legal cosine range, we can not find a
            // legal solution for the handTarget
            if ((cosAngle2 < -1.0) || (cosAngle2 > 1.0))
                foundValidSolution = false;

            // clamp our value into range so we can calculate the best
            // solution when there are no valid ones
            cosAngle2 = Mathf.Max(-1, Mathf.Min(1, cosAngle2));

            // compute a new value for angle2
            angle2 = Mathf.Acos(cosAngle2);

            // adjust for the desired bend direction
            if (!solvePosAngle2)
                angle2 = -angle2;

            // compute the sine of our angle
            sinAngle2 = Mathf.Sin(angle2);
        }
        else
        {
            // At least one of the bones had a zero length. This means our
            // solvable domain is a circle around the origin with a radius
            // equal to the sum of our bone lengths.
            float totalLenSqr = (length1 + length2) * (length1 + length2);
            if (targetDistSqr < (totalLenSqr - epsilon)
                || targetDistSqr > (totalLenSqr + epsilon))
            {
                foundValidSolution = false;
            }

            // Only the value of angle1 matters at this point. We can just
            // set angle2 to zero. 
            angle2 = 0.0f;
            cosAngle2 = 1.0f;
            sinAngle2 = 0.0f;
        }

        //===
        // Compute the value of angle1 based on the sine and cosine of angle2
        float triAdjacent = length1 + length2 * cosAngle2;
        float triOpposite = length2 * sinAngle2;

        float tanY = targetY * triAdjacent - targetX * triOpposite;
        float tanX = targetX * triAdjacent + targetY * triOpposite;

        // Note that it is safe to call Atan2(0,0) which will happen if targetX and
        // targetY are zero
        angle1 = Mathf.Atan2(tanY, tanX);

        return foundValidSolution;
    }


}
public class SODVec3
{
    private Vector3 xp; // previous input
    public Vector3 y, yd; // state variables
    private float k1, k2, k3;

    public Vector3 Value
    {
        get { return y; }
        set { y = value;}
    }
    public Vector3 AbsValue
    {
        set { y = value; xp = value; yd = Vector3.zero; }
    }
    public SODVec3(Vector3 x0, float f = 1, float z = 0.5f, float r = 2)
    {
        // compute constants
        k1 = z / (Mathf.PI * f);
        k2 = 1 / ((2 * Mathf.PI * f) * (2 * Mathf.PI * f));
        k3 = r * z / (2 * Mathf.PI * f);

        // initialize variables
        xp = x0;
        y = x0;
        yd = Vector3.zero;
    }

    public Vector3 Update(float dt, Vector3 x)
    {
        // estimate velocity
        Vector3 xd = (x - xp) / dt;
        xp = x;

        float k2_stable = Mathf.Max(k2, dt * dt / 2 + dt * k1 / 2, dt * k1); // clamp k2 to guarantee stability without jitter
        y = y + dt * yd; // integrate position by velocity
        yd = yd + dt * (x + k3 * xd - y - k1 * yd) / k2_stable; // integrate velocity by acceleration
        return y;
    }
}
public class SODFloat
{
    private float xp; // previous input
    private float y, yd; // state variables
    private float k1, k2, k3;
    public float Value
    {
        get { return y; }
        set { y = value; }
    }
    public SODFloat(float x0, float f = 1, float z = 0.5f, float r = 2)
    {
        // compute constants
        k1 = z / (Mathf.PI * f);
        k2 = 1 / ((2 * Mathf.PI * f) * (2 * Mathf.PI * f));
        k3 = r * z / (2 * Mathf.PI * f);

        // initialize variables
        xp = x0;
        y = x0;
        yd = 0;
    }

    public float Update(float dt, float x)
    {
        // estimate velocity
        float xd = (x - xp) / dt;
        xp = x;

        float k2_stable = Mathf.Max(k2, dt * dt / 2 + dt * k1 / 2, dt * k1); // clamp k2 to guarantee stability without jitter
        y = y + dt * yd; // integrate position by velocity
        yd = yd + dt * (x + k3 * xd - y - k1 * yd) / k2_stable; // integrate velocity by acceleration
        return y;
    }
}

public class SODAngle
{
    private float xp; // previous input
    private float y, yd; // state variables
    private float k1, k2, k3;
    public float Value
    {
        get { return y; }
        set { y = value; }
    }
    public SODAngle(float x0, float f = 1, float z = 0.5f, float r = 2)
    {
        // compute constants
        k1 = z / (Mathf.PI * f);
        k2 = 1 / ((2 * Mathf.PI * f) * (2 * Mathf.PI * f));
        k3 = r * z / (2 * Mathf.PI * f);

        // initialize variables
        xp = x0;
        y = x0;
        yd = 0;
    }

    public float Update(float dt, float x)
    {
        if (x - xp > 180f)
            x = x - 360f;
        if (xp - x > 180f)
            x = x + 360f;
        if (y - xp > 180f)
            y = y - 360f;
        if (xp - y > 180f)
            y = y + 360f;

        // estimate velocity
        float xd = (x - xp) / dt;
        xp = x;

        float k2_stable = Mathf.Max(k2, dt * dt / 2 + dt * k1 / 2, dt * k1); // clamp k2 to guarantee stability without jitter
        y = y + dt * yd; // integrate position by velocity
        yd = yd + dt * (x + k3 * xd - y - k1 * yd) / k2_stable; // integrate velocity by acceleration

        xp = xp % 360;
        y = y % 360;

        return y;
    }
}
