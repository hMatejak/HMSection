using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMSection.Utils
{
    public static class VerticesTrans
    {
        public static Point3d[] Vertices(Curve curve)
        {
            Polyline polyline = null;
            curve.TryGetPolyline(out polyline);
            Point3d[] vertices = polyline.ToArray();

            return vertices;
        }

        public static Point2d[] ConvertPoint2D(Point3d[] vertices3d)
        {
            Point2d[] points2d = new Point2d[vertices3d.Length - 1];

            for (int i = 0; i < vertices3d.Length - 1; i++)
            {
                points2d[i] = new Point2d(vertices3d[i]);
            }

            return points2d;
        }
    }
}
