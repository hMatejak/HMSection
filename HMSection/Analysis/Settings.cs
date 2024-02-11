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
using Rhino.DocObjects;
using Eto.Forms;

namespace HMSection.Analysis
{
    public class Settings : GH_Component
    {

        public Settings()
          : base("HMsec - Settings", "S", "Settings of an analysis", "HMSection", "02-Analysis")
        {
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.settings;

        public override Guid ComponentGuid => new Guid("ae2838e4-8244-493c-a207-6617201747a5");


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Roughness", "R", "Roughness settings", GH_ParamAccess.item, 0.01);
            pManager.AddNumberParameter("Max Area", "MAr", "Maximum area", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Min Angle", "Min", "Minimum angle", GH_ParamAccess.item, 30);
            pManager.AddNumberParameter("Max Angle", "Man", "Maximum angle", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("Conforming Delaunay", "CD", "Conforming Delaunay", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Plastic axis accuracy", "PA", "Plastic axis accuracy", GH_ParamAccess.item, 1E-5);
            pManager.AddIntegerParameter("Plastic axis max iterations", "PI", "Plastic axis max iterations", GH_ParamAccess.item, 500);
            pManager.AddBooleanParameter("Plastic analysis", " PA?", "Determines if plastic analysis s run", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Warping analysis", " WA?", "Determines if warping analysis s run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Settings", "S", "Settings of an analysis", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double roughness = new double();
            DA.GetData(0, ref roughness);

            double maximumArea = new double();
            DA.GetData(1, ref maximumArea);

            double minimumAngle = new double();
            DA.GetData(2, ref minimumAngle);

            double maximumAngle = new double();
            DA.GetData(3, ref maximumAngle);

            bool conformingDelaunay = new bool();
            DA.GetData(4, ref conformingDelaunay);

            double plasticAxisAccuracy = new double();
            DA.GetData(5, ref plasticAxisAccuracy);

            int plasticAxisMaxIterations = new int();
            DA.GetData(6, ref plasticAxisMaxIterations);

            SolutionSettings solutionSettings = new SolutionSettings(roughness: roughness,
                                                                    maximumArea: maximumArea,
                                                                    minimumAngle: minimumAngle,
                                                                    maximumAngle: maximumAngle,
                                                                    conformingDelaunay: conformingDelaunay,
                                                                    plasticAxisAccuracy: plasticAxisAccuracy,
                                                                    plasticAxisMaxIterations: plasticAxisMaxIterations);

            bool warp = new bool();
            DA.GetData(7, ref warp);
            bool plast = new bool();
            DA.GetData(8, ref plast);



            solutionSettings.RunWarpingAnalysis = warp;
            solutionSettings.RunPlasticAnalysis = plast;

            DA.SetData(0, solutionSettings);
        }

    }
}