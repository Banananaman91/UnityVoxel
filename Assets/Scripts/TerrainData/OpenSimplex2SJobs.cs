/**
 * K.jpg's OpenSimplex 2, smooth variant ("SuperSimplex")
 *
 * - 2D is standard simplex, modified to support larger kernels.
 *   Implemented using a lookup table.
 * - 3D is "Re-oriented 8-point BCC noise" which constructs a
 *   congruent BCC lattice in a much different way than usual.
 * - 4D uses a naïve pregenerated lookup table, and averages out
 *   to the expected performance.
 *
 * Multiple versions of each function are provided. See the
 * documentation above each, for more info.
 */

using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace SimplexNoise
{
    public class OpenSimplex2SJobs
    {
        private const int PSIZE = 2048;
        private const int PMASK = 2047;

        /*
         * Noise Evaluators
         */

        /**
         * 2D SuperSimplex noise, with Y pointing down the main diagonal.
         * Might be better for a 2D sandbox style game, where Y is vertical.
         * Probably slightly less optimal for heightmaps or continent maps.
         */
        public static double Noise2_XBeforeY(double x, double y, long seed)
        {

            // Skew transform and rotation baked into one.
            double xx = x * 0.7071067811865476;
            double yy = y * 1.224744871380249;

            return noise2_Base(yy + xx, yy - xx, seed);
        }

        /**
         * 2D SuperSimplex noise base.
         * Lookup table implementation inspired by DigitalShadow.
         */
        private static double noise2_Base(double xs, double ys, long seed)
        {
            double value = 0;

            // Get base points and offsets
            int xsb = fastFloor(xs), ysb = fastFloor(ys);
            double xsi = xs - xsb, ysi = ys - ysb;

            // Index to point list
            int a = (int)(xsi + ysi);
            int index =
                (a << 2) |
                (int)(xsi - ysi / 2 + 1 - a / 2.0) << 3 |
                (int)(ysi - xsi / 2 + 1 - a / 2.0) << 4;

            double ssi = (xsi + ysi) * -0.211324865405187;
            double xi = xsi + ssi, yi = ysi + ssi;
            
            //Experimenting
            var perm = new short[PSIZE];
            var permGrad2 = new double2[PSIZE];
            var source = new short[PSIZE];

            var GRADIENTS_2D = new double2[PSIZE];
            for (int i = 0; i < PSIZE; i++)
            {
                GRADIENTS_2D[i] = grad2[i % grad2.Length];
            }
            
            for (short i = 0; i < PSIZE; i++)
                source[i] = i;
            for (int i = PSIZE - 1; i >= 0; i--)
            {
                seed = seed * 6364136223846793005L + 1442695040888963407L;
                int r = (int)((seed + 31) % (i + 1));
                if (r < 0)
                    r += (i + 1);
                perm[i] = source[r];
                permGrad2[i] = GRADIENTS_2D[perm[i]];
                source[r] = source[i];
            }

            var LOOKUP_2D = new double2x2[8 * 4];

            for (int i = 0; i < 8; i++)
            {
                int i1, j1, i2, j2;
                if ((i & 1) == 0)
                {
                    if ((i & 2) == 0) { i1 = -1; j1 = 0; } else { i1 = 1; j1 = 0; }
                    if ((i & 4) == 0) { i2 = 0; j2 = -1; } else { i2 = 0; j2 = 1; }
                }
                else
                {
                    if ((i & 2) != 0) { i1 = 2; j1 = 1; } else { i1 = 0; j1 = 1; }
                    if ((i & 4) != 0) { i2 = 1; j2 = 2; } else { i2 = 1; j2 = 0; }
                }
                LOOKUP_2D[i * 4 + 0] = LatticePoint2DConvert(0, 0);
                LOOKUP_2D[i * 4 + 1] = LatticePoint2DConvert(1, 1);
                LOOKUP_2D[i * 4 + 2] = LatticePoint2DConvert(i1, j1);
                LOOKUP_2D[i * 4 + 3] = LatticePoint2DConvert(i2, j2);
            }

            // Point contributions
            for (int i = 0; i < 4; i++)
            {
                double2x2 c = LOOKUP_2D[index + i];

                double dx = xi + c.c1.x, dy = yi + c.c1.y;
                double attn = 2.0 / 3.0 - dx * dx - dy * dy;
                if (attn <= 0) continue;

                int pxm = (xsb + (int)c.c0.x) & PMASK, pym = (ysb + (int)c.c0.y) & PMASK;
                double2 grad = permGrad2[perm[pxm] ^ pym];
                double extrapolation = grad.x * dx + grad.y * dy;

                attn *= attn;
                value += attn * attn * extrapolation;
            }

            return value;
        }

        /*
         * Utility
         */

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int fastFloor(double x)
        {
            int xi = (int)x;
            return x < xi ? xi - 1 : xi;
        }

        /*
         * Lookup Tables & Gradients
         */

        private static double N2 = 0.05481866495625118;
        private static double2[] grad2 = {
            new double2( 0.130526192220052 / N2,  0.99144486137381 / N2),
            new double2( 0.38268343236509 / N2,   0.923879532511287 / N2),
            new double2( 0.608761429008721 / N2,  0.793353340291235 / N2),
            new double2( 0.793353340291235 / N2,  0.608761429008721 / N2),
            new double2( 0.923879532511287 / N2,  0.38268343236509 / N2),
            new double2( 0.99144486137381 / N2,   0.130526192220051 / N2),
            new double2( 0.99144486137381 / N2,  -0.130526192220051 / N2),
            new double2( 0.923879532511287 / N2, -0.38268343236509 / N2),
            new double2( 0.793353340291235 / N2, -0.60876142900872 / N2),
            new double2( 0.608761429008721 / N2, -0.793353340291235 / N2),
            new double2( 0.38268343236509 / N2,  -0.923879532511287 / N2),
            new double2( 0.130526192220052 / N2, -0.99144486137381 / N2),
            new double2(-0.130526192220052 / N2, -0.99144486137381 / N2),
            new double2(-0.38268343236509 / N2,  -0.923879532511287 / N2),
            new double2(-0.608761429008721 / N2, -0.793353340291235 / N2),
            new double2(-0.793353340291235 / N2, -0.608761429008721 / N2),
            new double2(-0.923879532511287 / N2, -0.38268343236509 / N2),
            new double2(-0.99144486137381 / N2,  -0.130526192220052 / N2),
            new double2(-0.99144486137381 / N2,   0.130526192220051 / N2),
            new double2(-0.923879532511287 / N2,  0.38268343236509 / N2),
            new double2(-0.793353340291235 / N2,  0.608761429008721 / N2),
            new double2(-0.608761429008721 / N2,  0.793353340291235 / N2),
            new double2(-0.38268343236509 / N2,   0.923879532511287 / N2),
            new double2(-0.130526192220052 / N2,  0.99144486137381 / N2)
        };

        private static double2x2 LatticePoint2DConvert(int xsv, int ysv)
        {
            double ssv = (xsv + ysv) * -0.211324865405187;
            double2x2 returnDouble = new double2x2 {c0 = {x = xsv, y = ysv}, c1 = {x = -xsv - ssv, y = -ysv - ssv}};
            return returnDouble;
        }
    }
}
