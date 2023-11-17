using UnityEngine;
using System.Collections;

/// <summary>
/// This class provides convenient methods for common easing types.
/// Usage: use one of the easing methods to calculate t value for
/// iterpolating methods such as Lerp.
/// Example: Vector3 vector = Vector3.Lerp(v1, v2, EaseType.EaseInQuad(t));
/// Easing equations by Robert Penner at http://gizma.com/easing/#quad1.
/// </summary>
public class EaseType
{
    public static float LinearTween(float t)
    {
        return t;
    }

    public static float EaseInQuad(float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        return t*t;
    }

    public static float EaseOutQuad(float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        return -t*(t-2);
    }

    public static float EaseInOutQuad(float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        t *= 2;

        if (t < 1)
        {
            return 0.5f*t*t;
        }
        else
        {
            t--;
            return -0.5f*(t*(t-2) - 1);
        }
    }
        
    public static float EaseInCubic(float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        return t*t*t;
    }

    public static float EaseOutCubic(float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        t--;
        return (t*t*t + 1);
    }

    public static float EaseInOutCubic(float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        t *= 2;

        if (t < 1)
        {
            return 0.5f*t*t*t;
        }
        else
        {
            t -= 2;
            return 0.5f*(t*t*t + 2);
        }
    }

    public static float EaseInQuart(float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        return t*t*t*t;
    }

    public static float EaseOutQuart(float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        t--;
        return -(t*t*t*t - 1);
    }

    public static float EaseInOutQuart(float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        t *= 2;

        if (t < 1)
        {
            return 0.5f*t*t*t*t;
        }
        else
        {
            t -= 2;
            return -0.5f*(t*t*t*t - 2);
        }
    }

    public static float EaseInQuint(float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        return t*t*t*t*t;
    }

    public static float EaseOutQuint(float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        t--;
        return (t*t*t*t*t + 1);
    }

    public static float EaseInOutQuint(float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        t *= 2;

        if (t < 1)
        {
            return 0.5f*t*t*t*t*t;
        }
        else
        {
            t -= 2;
            return 0.5f*(t*t*t*t*t + 2);
        }
    }

    public static float EaseInSine(float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        return (1 - Mathf.Cos(t * Mathf.PI/2));
    }

    public static float EaseOutSine(float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        return Mathf.Sin(t * Mathf.PI/2);
    }

    public static float EaseInOutSine(float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        return -0.5f*(Mathf.Cos(Mathf.PI * t) - 1);
    }

    public static float EaseInExpo(float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        return Mathf.Pow(2, 10 * (t - 1));
    }

    public static float EaseOutExpo(float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        return (1 - Mathf.Pow(2, -10 * t));
    }

    public static float EaseInOutExpo(float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        t *= 2;

        if (t < 1)
        {
            return 0.5f * Mathf.Pow(2, 10 * (t-1));
        }
        else
        {   
            t--;
            return 0.5f * (2 - Mathf.Pow(2, -10 * t));
        }
    }

    public static float EaseInCirc(float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        return (1 - Mathf.Sqrt(1 - t*t));
    }

    public static float EaseOutCirc(float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        t--;
        return Mathf.Sqrt(1 - t*t);
    }

    public static float EaseInOutCirc(float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        t *= 2;

        if (t < 1)
        {
            return -0.5f * (Mathf.Sqrt(1 - t*t) - 1);
        }
        else
        {   
            t -= 2;
            return 0.5f * (Mathf.Sqrt(1 - t*t) + 1);
        }
    }
}
