using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace CafeLibrary
{
	public class TriangleHelper
	{
		//Based on this algorithm: http://jgt.akpeters.com/papers/AkenineMoller01/tribox.html

		// ---- METHODS (PUBLIC) --------------------------------------------------------------------------------------

		/// <summary>
		/// Returns a value indicating whether the given <paramref name="triangle"/> overlaps a cube positioned at the
		/// <paramref name="cubeCenter"/> expanding with <paramref name="cubeHalfSize"/>.
		/// </summary>
		/// <param name="triangle">The <see cref="Triangle"/> to check for overlaps.</param>
		/// <param name="cubeCenter">The positional <see cref="Vector3F "/> at which the cube originates.</param>
		/// <param name="cubeHalfSize">The half length of one edge of the cube.</param>
		/// <returns><c>true</c> when the triangle intersects with the cube, otherwise <c>false</c>.</returns>
		public static bool TriangleCubeOverlap(Triangle t, Vector3 Position, float BoxSize)
		{
			float half = BoxSize / 2f;
			//Position is the min pos, so add half the box size
			Position += new Vector3(half, half, half);
			Vector3 v0 = t.Vertices[0] - Position;
			Vector3 v1 = t.Vertices[1] - Position;
			Vector3 v2 = t.Vertices[2] - Position;

			if (Math.Min(Math.Min(v0.X, v1.X), v2.X) > half || Math.Max(Math.Max(v0.X, v1.X), v2.X) < -half) return false;
			if (Math.Min(Math.Min(v0.Y, v1.Y), v2.Y) > half || Math.Max(Math.Max(v0.Y, v1.Y), v2.Y) < -half) return false;
			if (Math.Min(Math.Min(v0.Z, v1.Z), v2.Z) > half || Math.Max(Math.Max(v0.Z, v1.Z), v2.Z) < -half) return false;

			float d = Vector3.Dot(t.Normal, v0);
			double r = half * (Math.Abs(t.Normal.X) + Math.Abs(t.Normal.Y) + Math.Abs(t.Normal.Z));
			if (d > r || d < -r) return false;

			Vector3 e = v1 - v0;
			if (AxisTest(e.Z, -e.Y, v0.Y, v0.Z, v2.Y, v2.Z, half)) return false;
			if (AxisTest(-e.Z, e.X, v0.X, v0.Z, v2.X, v2.Z, half)) return false;
			if (AxisTest(e.Y, -e.X, v1.X, v1.Y, v2.X, v2.Y, half)) return false;

			e = v2 - v1;
			if (AxisTest(e.Z, -e.Y, v0.Y, v0.Z, v2.Y, v2.Z, half)) return false;
			if (AxisTest(-e.Z, e.X, v0.X, v0.Z, v2.X, v2.Z, half)) return false;
			if (AxisTest(e.Y, -e.X, v0.X, v0.Y, v1.X, v1.Y, half)) return false;

			e = v0 - v2;
			if (AxisTest(e.Z, -e.Y, v0.Y, v0.Z, v1.Y, v1.Z, half)) return false;
			if (AxisTest(-e.Z, e.X, v0.X, v0.Z, v1.X, v1.Z, half)) return false;
			if (AxisTest(e.Y, -e.X, v1.X, v1.Y, v2.X, v2.Y, half)) return false;
			return true;
		}

		// ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

		private static bool AxisTest(double a1, double a2, double b1, double b2, double c1, double c2, double half)
		{
			var p = a1 * b1 + a2 * b2;
			var q = a1 * c1 + a2 * c2;
			var r = half * (Math.Abs(a1) + Math.Abs(a2));
			return Math.Min(p, q) > r || Math.Max(p, q) < -r;
		}
	}
}