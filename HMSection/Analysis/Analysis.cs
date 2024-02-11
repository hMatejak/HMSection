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
using System.Reflection;

namespace HMSection.Analysis
{
    public class Analysis : GH_Component
    {

        public Analysis()
          : base("HMSec - Analysis", "HMA",
            "Analyses inputed section",
            "HMSection", "02-Analysis")
        {
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.analysis;   

        public override Guid ComponentGuid => new Guid("629ee77e-625e-4610-b142-59fa9ba17768");


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "R", "Toggle to run", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Contours", "C", "Contours of section", GH_ParamAccess.list);
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

        Dictionary<int, List<Rhino.Geometry.Curve>> bordersDict = new Dictionary<int, List<Rhino.Geometry.Curve>>();

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

            bordersDict.Clear();

            //unpack settings corectly and cast them
            Grasshopper.Kernel.Types.GH_ObjectWrapper set = new Grasshopper.Kernel.Types.GH_ObjectWrapper();
            DA.GetData(3, ref set);
            SolutionSettings settings = set.Value as SolutionSettings;

            //unpack contour
            List<SectionContour> contours = new List<SectionContour>();
            DA.GetDataList(1, contours);
            if ((contours == null) | (contours.Count == 0))
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
            this.CollectCurvesForBrep(contours); 
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
                foreach (SectionContour contour in contours)
                {
                    sec.Contours.Add(contour);
                }

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

        /// <summary>
        /// Draws breps in Rhino view that represent the section.
        /// </summary>
        /// <param name="args"></param>
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            base.DrawViewportMeshes(args);

            Rhino.Geometry.Brep[] breps = new Brep[0];

            List<Rhino.Geometry.Brep> borderBrep = new List<Brep>();
            foreach (var border in borders)
            {
                borderBrep.AddRange(Rhino.Geometry.Brep.CreatePlanarBreps(border, 0.01).ToList());
            }

            List<Rhino.Geometry.Brep> brepsHoles = new List<Brep>();
            foreach (var hole in holeBorders)
            {
                brepsHoles.AddRange( Rhino.Geometry.Brep.CreatePlanarBreps(hole, 0.01).ToList() );
            }


            Dictionary<int, Rhino.Geometry.Brep[]> brepDict = new Dictionary<int, Brep[]>();
            foreach (var brep in bordersDict)
            {
                brepDict.Add(brep.Key, Rhino.Geometry.Brep.CreatePlanarBreps(brep.Value, 0.001));
            }

            Dictionary<int, Rhino.Geometry.Brep[]> brepDictHoles = new Dictionary<int, Brep[]>();
            foreach (var brepPair in brepDict)
            {
                if (Rhino.Geometry.Brep.CreateBooleanDifference(brepPair.Value, brepsHoles, 0.001).Length == 0)
                {
                    brepDictHoles.Add(brepPair.Key, brepPair.Value);
                }
                else
                {
                    brepDictHoles.Add(brepPair.Key, Rhino.Geometry.Brep.CreateBooleanDifference(brepPair.Value, brepsHoles, 0.001));
                }

            }


            //generate list of colors
            List<Color> colorList = Utils.Colors.ColorStructToList();
            colorList.Reverse();
            int colorID = new int();

            foreach (var brepAr in brepDictHoles)
            {
                foreach (var brep in brepAr.Value) 
                {
                    if (brepAr.Key > colorList.Count)
                    {
                        int colorId = colorList.Count -1;
                    }
                    else { colorID = brepAr.Key; }
                    args.Display.DrawBrepShaded(brep, new Rhino.Display.DisplayMaterial(colorList[colorID],0.6));
                }
            }
        }

        protected void CollectCurvesForBrep(List<SectionContour> contours)
        {
            foreach (SectionContour contour in contours)
            {
                if (contour.Points.Count > 0 )
                {
                    List<Point2D> points = contour.Points;
                    List<Rhino.Geometry.Point3d> points3d = new List<Rhino.Geometry.Point3d>();

                    foreach (var point in points)
                    {
                        points3d.Add(new Rhino.Geometry.Point3d(point.X, point.Y, 0));
                    }
                    points3d.Add(new Rhino.Geometry.Point3d(points[0].X, points[0].Y, 0));

                    Rhino.Geometry.Curve border = Curve.CreateInterpolatedCurve(points3d, 1);

                    ////test for orientation
                    CurveOrientation orientation = border.ClosedCurveOrientation();
                    if (orientation == CurveOrientation.Clockwise)
                    {
                        border.Reverse();
                    }
                    borders.Add(border);

                    int matID = contour.Material.Id;

                    if (!bordersDict.ContainsKey(matID))
                    {
                        List<Rhino.Geometry.Curve> newmat_id_list= new List<Rhino.Geometry.Curve>();
                        newmat_id_list.Add(border);
                        bordersDict.Add(matID, newmat_id_list);
                    }
                    else
                    {
                        bordersDict[matID].Add(border);
                    }
                }
            }
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

                ////test for orientation
                CurveOrientation orientation = holeBorder.ClosedCurveOrientation();
                if (orientation == CurveOrientation.Clockwise)
                {
                    holeBorder.Reverse();
                }

                holeBorders.Add(holeBorder);
            }
        }
    }
}