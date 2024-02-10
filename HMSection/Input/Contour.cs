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

namespace HMSection.Input
{
    public class Contour : GH_Component
    {

        public Contour()
          : base("HMsec - Countour", "C", "Polygon that represents countour of a section.", "HMSection", "01-Input")
        {
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.contour;

        public override Guid ComponentGuid => new Guid("bf384461-9560-4c78-8c01-d7a6cfde2174");

        public override GH_Exposure Exposure => GH_Exposure.primary;


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Section Curve", "SC", "Curve representing a section", GH_ParamAccess.item);
            pManager.AddGenericParameter("Material", "M", "Material of section", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Contour", "C", "Contour of section", GH_ParamAccess.item);
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
            //unpack material
            GH_ObjectWrapper obj = new GH_ObjectWrapper();
            DA.GetData(1, ref obj);

            SectionMaterial material = obj.Value as SectionMaterial;
            if (material == null ^ obj == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, " Material is not defined!");
                return;
            }

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

            SectionContour contour = SecContour(vertices2d, material);

            if (closed & polyline & !(intersections.Count > 0))
            {
                DA.SetData(0, contour);
            }

            DA.SetData(1, closed);
            DA.SetData(2, polyline);
        }

        private SectionContour SecContour(Point2d[] vertices2d, SectionMaterial material)
        {
            List<Point2D> points = new List<Point2D>();
            //create list of 2d points
            foreach (Point2d point in vertices2d)
            {
                Point2D point2D = new Point2D(point.X, point.Y);
                points.Add(point2D);
            }
            var polygon = new SectionContour(points, false, material);
            return polygon;
        }

    }
}