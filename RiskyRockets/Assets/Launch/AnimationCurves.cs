using UnityEngine;
using UnityEngine.Splines;

public class AnimationCurves
{
    public static float CalculateBezierCurve(float p)
    {
        return p * p * (3.0f - 2.0f * p);
    }

    public static float CalculateParametric(float p)
    {
        float sqr = p * p;
        return sqr / (2.0f * (sqr - p) + 1.0f);
    }
}
