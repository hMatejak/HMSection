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

namespace HMSection.Output
{
    public class Plastic : GH_Component
    {

        public Plastic()
          : base("HMsec - Plastic", "PL", "Plastic section properties", "HMSection", "03-Section Properties")
        {
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.platic;

        public override Guid ComponentGuid => new Guid("50a74e78-55a4-482c-9d8e-cac5e5652759");


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Section Properties", "SP", "Calculated characteristics", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Torsion constant", "j", "Torsion constant", GH_ParamAccess.item);
            pManager.AddNumberParameter("Shear factor", "Delta_s", "Shear factor", GH_ParamAccess.item);
            pManager.AddPointParameter("Shear center - elastic", "sc_e", "Shear centre(Elasticity approach)", GH_ParamAccess.item);
            pManager.AddPointParameter("Shear center - Trefftz", "sc_t", "Shear centre(Trefftz’s approach)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Warping constant", "gamma", "Warping constant", GH_ParamAccess.item);
            pManager.AddNumberParameter("Shear area A_sx", "A_sx", "Shear area about the x-axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("Shear area A_sy", "A_sy", "Shear area about the y-axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("Shear area A_sxy", "A_sxy", "Shear area about the xy-axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("Shear area A_s11", "A_s11", "Shear area about the 11-axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("Shear area A_s22", "A_s22", "Shear area about the 22-axis", GH_ParamAccess.item);
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
            if(!DA.GetData("Section Properties", ref obj)) return;

            SectionProperties sectionProperties = obj.Value as SectionProperties;

            if (sectionProperties.j == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Plastic properties were not calculated!");
                return;
            }

            DA.SetData("Torsion constant", sectionProperties.j);
            DA.SetData("Shear factor", sectionProperties.Delta_s);

            //shear center output
            Point3d sc_e = new Point3d(sectionProperties.cx - sectionProperties.x_se, sectionProperties.cy - sectionProperties.y_se, 0.0);
            DA.SetData("Shear center - elastic", sc_e);
            //shear center output
            Point3d sc_t = new Point3d(sectionProperties.cx - sectionProperties.x_st, sectionProperties.cy - sectionProperties.y_st, 0.0);
            DA.SetData("Shear center - Trefftz", sc_t);

            DA.SetData("Warping constant", sectionProperties.gamma);
            DA.SetData("Shear area A_sx", sectionProperties.A_sx);
            DA.SetData("Shear area A_sy", sectionProperties.A_sy);
            DA.SetData("Shear area A_sxy", sectionProperties.A_sxy);
            DA.SetData("Shear area A_s11", sectionProperties.A_s11);
            DA.SetData("Shear area A_s22", sectionProperties.A_s22);
        }

    }
}