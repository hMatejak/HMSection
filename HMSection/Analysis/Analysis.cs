using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using CrossSection;
using CrossSection.Analysis;
using CrossSection.Maths;
using CrossSection.DataModel;

using CrossSection.Triangulation;
using TriangleNet;
using TriangleNet.Geometry;
using TriangleNet.Topology;
using System.Collections.ObjectModel;
using MathNet.Numerics.LinearAlgebra.Factorization;
using Rhino;
using Rhino.Geometry;
using System.Linq;
using System.Drawing;
using HMSection.Input;

namespace HMSection.Analysis
{
    public class Analysis : GH_Component
    {

        public Analysis()
          : base("Analysis", "HMA",
            "Analyses inputed section",
            "HMSection", "02-Analysis")
        {
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.analysis;   

        public override Guid ComponentGuid => new Guid("6456fb97-626b-40c1-b02b-82f6f732d07c");


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "R", "Toggle to run", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Contour", "C", "Contour of section", GH_ParamAccess.item);
            pManager.AddGenericParameter("Holes", "H", "Holes in section", GH_ParamAccess.list);
            pManager[2].Optional = true;
            pManager.AddGenericParameter("Settings", "S", "Analysis settings", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Section Properties", "SP", "Calculated characteristics", GH_ParamAccess.item);
            pManager.AddGenericParameter("Mesh", "M", "Generated mesh for vizualization", GH_ParamAccess.item);
        }

        // Persistent variables

        /// <summary>
        /// Holds section definition
        /// </summary>
        SectionDefinition sec = new SectionDefinition();

        /// <summary>
        /// Holds points of mesh of section
        /// </summary>
        List<Rhino.Geometry.Point3d> meshPoints = new List<Rhino.Geometry.Point3d>();

        /// <summary>
        /// Holds info about mesh lines
        /// </summary>
        List<Rhino.Geometry.Line> meshLines = new List<Rhino.Geometry.Line>();

        TriangleNet.Mesh mesh = null;

        List<Rhino.Geometry.Curve> borders = new List<Rhino.Geometry.Curve>();

        List<Rhino.Geometry.Curve> holeBorders = new List<Rhino.Geometry.Curve>();

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            borders.Clear();
            holeBorders.Clear();

            //change message under the coponent
            Message = "Not Calculated";

            //unpack settings corectly and cast them
            Grasshopper.Kernel.Types.GH_ObjectWrapper set = new Grasshopper.Kernel.Types.GH_ObjectWrapper();
            DA.GetData(3, ref set);
            SolutionSettings settings = set.Value as SolutionSettings;

            //unpack contour
            Grasshopper.Kernel.Types.GH_ObjectWrapper con = new Grasshopper.Kernel.Types.GH_ObjectWrapper();
            DA.GetData(1, ref con);
            SectionContour contour = con.Value as SectionContour;
            if (contour == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Contour is not valid!");
                return;
            }

            //unpack holes
            //Grasshopper.Kernel.Types.GH_ObjectWrapper hol = new Grasshopper.Kernel.Types.GH_ObjectWrapper();
            //List<Grasshopper.Kernel.Types.GH_ObjectWrapper> hol = new List<Grasshopper.Kernel.Types.GH_ObjectWrapper>();
            List<SectionContour> holes = new List<SectionContour>();
            DA.GetDataList(2, holes);

            

            //display section as brep
            this.CollectCurvesForBrep(contour);
            this.CollectHolesForBrep(holes);


            //main analysis funtion
            bool run = new bool();
            DA.GetData(0, ref run);
            if (run)
            {
                //reinitialization of sec definition
                if (!sec.Equals(new SectionDefinition()))
                {
                    sec = new SectionDefinition();
                    mesh = null;
                    meshPoints = new List<Rhino.Geometry.Point3d>();
                    meshLines = new List<Rhino.Geometry.Line>();
                }

                sec.SolutionSettings = settings;

                sec.Contours.Clear();

                //adding contours
                sec.Contours.Add(contour);

                //adding holes
                if (!(holes.Count == 0))
                {
                    foreach (var hole in holes)
                    {
                        sec.Contours.Add(hole);
                    }
                }

                Solver _solver = new Solver();
                _solver.Solve(sec);

                //vertices of triangulated points
                mesh = sec.Triangulate();

            }

            DA.SetData("Section Properties", sec.Output.SectionProperties);
            DA.SetData("Mesh", mesh);

            if (!(sec.Output.SectionProperties.Area == 0))
            {
                Message = "Calculated!";
            }

        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            base.DrawViewportMeshes(args);

            Rhino.Geometry.Brep[] breps = new Brep[0];

            foreach (var border in borders)
            {
                breps = Rhino.Geometry.Brep.CreatePlanarBreps(border, 0.001);
            }


            List<Rhino.Geometry.Brep> brepsHoles = new List<Brep>();
            foreach (var hole in holeBorders)
            {
                brepsHoles.AddRange( Rhino.Geometry.Brep.CreatePlanarBreps(hole, 0.001).ToList() );
            }

            if ((brepsHoles.Count > 0)) 
            {
                breps = Rhino.Geometry.Brep.CreateBooleanDifference(breps, brepsHoles, 0.001);  
            }

            foreach (Rhino.Geometry.Brep brep in breps)
            {
                args.Display.DrawBrepShaded(brep, new Rhino.Display.DisplayMaterial(System.Drawing.Color.FromKnownColor(KnownColor.Aqua), 0.7));
            }

        }

        protected void CollectCurvesForBrep(SectionContour contour)
        {
            List<Point2D> points = contour.Points;
            List<Rhino.Geometry.Point3d> points3d = new List<Rhino.Geometry.Point3d>();

            foreach (var point in points)
            {
                points3d.Add(new Rhino.Geometry.Point3d(point.X, point.Y, 0));
            }
            points3d.Add(new Rhino.Geometry.Point3d(points[0].X, points[0].Y, 0));

            Rhino.Geometry.Curve border = Curve.CreateInterpolatedCurve(points3d, 1);
 
            borders.Add(border);
        }

        protected void CollectHolesForBrep(List<SectionContour> holes)
        {
            foreach (SectionContour hole in holes)
            {
                List<Point2D> points = hole.Points;
                List<Rhino.Geometry.Point3d> points3d = new List<Rhino.Geometry.Point3d>();

                foreach (var point in points)
                {
                    points3d.Add(new Rhino.Geometry.Point3d(point.X, point.Y, 0));
                }
                points3d.Add(new Rhino.Geometry.Point3d(points[0].X, points[0].Y, 0));

                Rhino.Geometry.Curve holeBorder = Curve.CreateInterpolatedCurve(points3d, 1);

                holeBorders.Add(holeBorder);
            }
        
        }
    }
}