using System;
using System.Collections.Generic;
using System.Text;

namespace AGraphicsLibrary
{
    public class CurveHelper
    {
        public static float Interpolate(AampLibraryCSharp.Curve curve, float t)
        {
            switch (curve.CurveType)
            {
                case AampLibraryCSharp.CurveType.Hermit: return InterpolateHermite(t, curve.NumUses, curve.valueFloats);
                case AampLibraryCSharp.CurveType.Hermit2D: return InterpolateHermite2D(t, curve.NumUses, curve.valueFloats);
                case AampLibraryCSharp.CurveType.Linear:   return InterpolateLinear(t, curve.NumUses, curve.valueFloats);
                case AampLibraryCSharp.CurveType.Linear2D: return InterpolateLinear2D(t, curve.NumUses, curve.valueFloats);
                case AampLibraryCSharp.CurveType.Step: return InterpolateStep(t, curve.NumUses, curve.valueFloats);
                case AampLibraryCSharp.CurveType.Sin: return InterpolateSin(t, curve.NumUses, curve.valueFloats);
                case AampLibraryCSharp.CurveType.Cos: return InterpolateCos(t, curve.NumUses, curve.valueFloats);
                case AampLibraryCSharp.CurveType.SinPow2: return InterpolateSinPow2(t, curve.NumUses, curve.valueFloats);
                default:
                    return 0.0f;
                    //     throw new Exception($"Unsupported color type! {curve.CurveType}");
            }
        }

        //https://github.com/open-ead/sead/blob/16d150caade87410309acbc04069ec9067c78fd6/modules/src/hostio/seadHostIOCurve.cpp

        static float InterpolateLinear(float t, uint numUses, float[] f)
        {
            if (t < 0)
                return f[0];

            int n = (int)numUses / 3;
            int i = (int)(n * t);
            if (i >= n)
                return f[n];
            return f[i] + (fracPart(n * t) * (f[i + 1] - f[i]));
        }

        static float InterpolateHermite(float t, uint numUses, float[] f)
        {
            if (t < 0)
                return f[0];

            int n = (int)(numUses / 2) - 1;
            int i = n * (int)t;
            int j = 2 * i;
            if (i >= n)
                return f[j];

            var x = fracPart(n * t);

            return ((2 * x * x * x) - (3 * x * x) + 1) * f[j + 0]  // (2t^3 - 3t^2 + 1)p0
                   + ((-2 * x * x * x) + (3 * x * x)) * f[j + 2]   // (-2t^3 + 2t^2)p1
                   + ((x * x * x) - (x * x)) * f[j + 3]            // (t^3 - t^2)m1
                   + ((x * x * x) - (2 * x * x) + x) * f[j | 1]    // (t^3 - 2t^2 + t)m0
                ;
        }

        static float InterpolateStep(float t, uint numUses, float[] f)
        {
            float x = (float)Math.Clamp(t, 0.0, 1.0);
            return f[(int)(x * ((int)numUses - 1))];
        }

        static float InterpolateSin(float t, uint numUses, float[] f)
        {
            return MathF.Sin(f[0] * t * (2 * MathF.PI)) * f[1];
        }

        static float InterpolateCos(float t, uint numUses, float[] f)
        {
            return MathF.Cos(f[0] * t * (2 * MathF.PI)) * f[1];
        }

        static float InterpolateSinPow2(float t, uint numUses, float[] f)
        {
            var y = MathF.Sin(f[0] * t * (2 * MathF.PI));
            return y * y * f[1];
        }

        static float InterpolateLinear2D(float t, uint numUses, float[] f)
        {
            if (t < 0)
                return f[0];

            int n = (int)numUses / 3;
            if (f[2 * (n - 1)] <= t)
                return f[2 * (n - 1) + 1];

            for (int i = 0; i < n; ++i)
            {
                var j = 2 * i;
                if (f[j + 2] > t)
                    return f[j + 1] + ((t - f[j]) / (f[j + 2] - f[j])) * (f[j + 3] - f[j + 1]);
            }
            return 0;
        }

        static float InterpolateHermite2D(float t, uint numUses, float[] f)
        {
            int n = (int)numUses / 3;
            if (f[0] >= t)
                return f[1];

            if (f[3 * (n - 1)] <= t)
                return f[3 * (n - 1) + 1];

            for (int i = 0; i < n; ++i)
            {
                var j = 3 * i;
                if (f[j + 3] > t)
                {
                    var x = (t - f[j]) / (f[j + 3] - f[j]);
                    return ((2 * x * x * x) - (3 * x * x) + 1) * f[j + 1]  // (2t^3 - 3t^2 + 1)p0
                           + ((-2 * x * x * x) + (3 * x * x)) * f[j + 4]   // (-2t^3 + 2t^2)p1
                           + ((x * x * x) - (x * x)) * f[j + 5]            // (t^3 - t^2)m1
                           + ((x * x * x) - (2 * x * x) + x) * f[j + 2]    // (t^3 - 2t^2 + t)m0
                        ;
                }
            }

            return 0;
        }

        static float fracPart(float x)
        {
            return x - (int)x;
        }
    }
}
