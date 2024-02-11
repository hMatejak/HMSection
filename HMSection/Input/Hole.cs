/// <copyright>
/// https://github.com/hMatejak/HMSection
/// MIT License
///
/// Copyright(c) 2024 Jan Matìják
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
///
/// The above copyright notice and this permission notice shall be included in all
/// copies or substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.
/// </copyright>
/// 
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using CrossSection;
using CrossSection.Analysis;
using CrossSection.Maths;
using CrossSection.DataModel;
using System.Linq;
using Grasshopper.Kernel.Types;
using Rhino.Geometry.Intersect;
using Rhino;
using Rhino.DocObjects;
using HMSection.Utils;

namespace HMSection.Input
{
    public class Hole : GH_Component
    {

        public Hole()
          : base("HMsec - Hole", "H", "Polygon that represents hole in section.", "HMSection", "01-Input")
        {
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.hole;

        public override Guid ComponentGuid => new Guid("ebbaf142-22db-4241-9d43-01402550a1b6");

        public override GH_Exposure Exposure => GH_Exposure.primary;


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Hole Curve", "HC", "Polyline representing hole in section", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Hole", "H", "Hole in section", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Closed", "C", "Closed curve?", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Polyline", "P", "Polyline curve?", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve curve = null;
            if (!DA.GetData(0, ref curve))
            {
                return;
            }

            //test for closed or polyline
            bool closed = curve.IsClosed;
            bool polyline = curve.IsPolyline();
            if (!closed)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Curve is not closed!");
                return;
            }
            if (!polyline)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Curve is not polyline!");
                return;
            }

            //test for self intersecting curve
            var intersections = Intersection.CurveSelf(curve, 0.0001);
            if (intersections.Count > 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Curve intersects Itself!");
                return;
            }

            //creating contour
            Point3d[] vertices3d = VerticesTrans.Vertices(curve);
            Point2d[] vertices2d = VerticesTrans.ConvertPoint2D(vertices3d);

            SectionContour contour = SecHole(vertices2d);

            DA.SetData(0, contour);
            DA.SetData(1, closed);
            DA.SetData(2, polyline);
        }

        private SectionContour SecHole(Point2d[] vertices2d)
        {
            List<Point2D> points = new List<Point2D>();
            //create list of 2d points
            foreach (Point2d point in vertices2d)
            {
                Point2D point2D = new Point2D(point.X, point.Y);
                points.Add(point2D);
            }

            var polygon = new SectionContour(points, true, null);
            return polygon;
        }

    }
}