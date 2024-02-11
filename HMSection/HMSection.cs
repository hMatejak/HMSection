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

namespace HMSection
{
    public class HMSection : GH_Component
    {

        public HMSection()
          : base("HMSection", "HMS",
            "Description",
            "HMSection", "02-Analysis")
        {
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("d6bfe73e-d072-44bb-a0be-9476be3ba629");

        public override GH_Exposure Exposure => GH_Exposure.hidden;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Section Curve", "SC", "Curve representing a section", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Closed", "C", "closed", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Polyline", "P", "poylyline", GH_ParamAccess.item);
            pManager.AddTextParameter("Vertices", "V", "verticesTEST", GH_ParamAccess.item);
            pManager.AddBrepParameter("brep", "B", "BREP_test", GH_ParamAccess.item);
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

            if (!closed) {
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



            Point3d[] vertices3d = Vertices(curve);
            Point2d[] vertices2d = ConvertPoint2D(vertices3d);

            SectionDefinition sec = SecFromPolygon(vertices2d);


            DA.SetData(0, closed);
            DA.SetData(1, polyline);
            //DA.SetData(2, string.Join(", ", vertices2d));
            DA.SetData(2, sec.Output.SectionProperties.Area);

            sec.Output.SectionProperties.ToString();

            GH_Brep gH_Brep = new GH_Brep();
            GH_Convert.ToGHBrep(Rhino.Geometry.Brep.CreatePlanarBreps(curve, 0.001)[0], 0 , ref gH_Brep);
            DA.SetData(3, gH_Brep);
        }

        private SectionDefinition SecFromPolygon(Point2d[] vertices2d)
        {
            SectionDefinition sec = new SectionDefinition();
            sec.SolutionSettings = new SolutionSettings(0.002);

            SectionMaterial defaultMat = new SectionMaterial("dummy", id: 1, elastic_modulus: 1.0, poissons_ratio: 0.0, yield_strength: 1.0);

            List<Point2D> points = new List<Point2D>();
            //create list of 2d points
            foreach (Point2d point in vertices2d) {
                Point2D point2D = new Point2D(point.X, point.Y);
                points.Add(point2D); }


            var polygon = new SectionContour(points, false, defaultMat);
            sec.Contours.Add(polygon);


            Solver _solver = new Solver();
            _solver.Solve(sec);
            
            return sec;
        }


        private Point3d[] Vertices(Curve curve)
        {
            Polyline polyline = null;
            curve.TryGetPolyline(out polyline);
            Point3d[] vertices = polyline.ToArray();

            return vertices;
        }

        private Point2d[] ConvertPoint2D(Point3d[] vertices3d) {
            Point2d[] points2d = new Point2d[vertices3d.Length - 1];

            for (int i = 0; i < vertices3d.Length -1; i++)
            {
                points2d[i] = new Point2d(vertices3d[i]);
            }

            return points2d;
        }


        private string TestMethod()
        {

            Solver _solver = new Solver();
            ShapeGeneratorHelper helper = new ShapeGeneratorHelper();

            SectionMaterial defaultMat = new SectionMaterial("dummy",id: 1, elastic_modulus: 1.0, poissons_ratio: 0.0, yield_strength: 1.0 );

            SectionDefinition sec = new SectionDefinition();
            sec.SolutionSettings = new SolutionSettings(0.002);

            //sec.Contours.Add(new SectionContour(helper.CreateRectangle(d: 0.300, 0.180), false, defaultMat));

            List<Point2D> points = new List<Point2D>();
            points.Add(new Point2D(0,   0));
            points.Add(new Point2D(0.5, 0));
            points.Add(new Point2D(0.6,  1));
            points.Add(new Point2D(-0.1, 1));

            var polygon = new SectionContour(points, false, defaultMat);
            sec.Contours.Add(polygon);

            _solver.Solve(sec);

            return sec.Output.SectionProperties.Area.ToString();
        }

    }
}