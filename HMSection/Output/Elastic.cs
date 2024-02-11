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
    public class Elastic : GH_Component
    {

        public Elastic()
          : base("HMsec - Elastic", "EL", "Elastic section properties", "HMSection", "03-Section Properties")
        {
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.elastic;

        public override Guid ComponentGuid => new Guid("c929ccf9-475d-4e30-9b9a-b7ce978b3dad");


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
            pManager.AddPointParameter("Centroid", "C", "Centroid", GH_ParamAccess.item);
            pManager.AddNumberParameter("Area", "A", "Area", GH_ParamAccess.item);
            pManager.AddNumberParameter("Eff. Poisson’s ratio", "nu_eff", "Effective Poisson’s ratio", GH_ParamAccess.item);
            //moments
            pManager.AddNumberParameter("First moment about x", "qx", "First moment of area about the x-axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("First moment about y", "qy", "First moment of area about the y-axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("Second moment about global x", "ixx_g", "Second moment of area about the global x-axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("Second moment about global y", "iyy_g", "Second moment of area about the global y-axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("Second moment about global xy", "ixy_g", "Second moment of area about the global xy-axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("Second moment about centroidal x", "ixx_c", "Second moment of area about the centroidal x-axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("Second moment about centroidal y", "iyy_c", "Second moment of area about the centroidal y-axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("Second moment about centroidal xy", "ixy_c", "Second moment of area about the centroidal xy-axis", GH_ParamAccess.item);
            //section modulus
            pManager.AddNumberParameter("Section modulus zxx_plus", "zxx_plus", "Section modulus about the centroidal x-axis for stresses at the positive extreme value of y", GH_ParamAccess.item);
            pManager.AddNumberParameter("Section modulus zxx_minus", "zxx_minus", "Section modulus about the centroidal x-axis for stresses at the negative extreme value of y", GH_ParamAccess.item);
            pManager.AddNumberParameter("Section modulus zyy_plus", "zyy_plus", "Section modulus about the centroidal y-axis for stresses at the positive extreme value of x", GH_ParamAccess.item);
            pManager.AddNumberParameter("Section modulus zyy_minus", "zyy_minus", "Section modulus about the centroidal y-axis for stresses at the negative extreme value of x", GH_ParamAccess.item);
            //gyration radius
            pManager.AddNumberParameter("Radius of gyration rx", "rx_c", "Radius of gyration about the centroidal x-axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("Radius of gyration ry", "ry_c", "Radius of gyration about the centroidal y-axis", GH_ParamAccess.item);
            //second moments about centroidial roatted axis
            pManager.AddNumberParameter("Second moment about 11-axis", "i11_c", "Second moment of area about the centroidal 11-axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("Second moment about 22-axis", "i22_c", "Second moment of area about the centroidal 22-axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("Principal axis angle", "phi", "Principal axis angle", GH_ParamAccess.item);
            //section modulus about principal axes
            pManager.AddNumberParameter("Section modulus about 11-axis plus", "z11_plus", "Section modulus about the principal 11-axis for stresses at the positive extreme value of the 22-axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("Section modulus about 11-axis minus", "z11_minus", "Section modulus about the principal 11-axis for stresses at the negative extreme value of the 22-axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("Section modulus about 22-axis plus", "z22_plus", "Section modulus about the principal 22-axis for stresses at the positive extreme value of the 11-axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("Section modulus about 22-axis minus", "z22_minus", "Section modulus about the principal 22-axis for stresses at the negative extreme value of the 11-axis", GH_ParamAccess.item);
            pManager.AddNumberParameter("Radius of gyration about 11-axis", "r11_c", "Radius of gyration about the principal 11-axis.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Radius of gyration about 22-axis", "r22_c", "Radius of gyration about the principal 22-axis.", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //unpack section proeprties
            GH_ObjectWrapper obj = new GH_ObjectWrapper();
            if (!DA.GetData("Section Properties", ref obj)) return;
      
            SectionProperties sectionProperties = obj.Value as SectionProperties;

            if (sectionProperties.Area == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Section properties were not calculated!");
                return;
            }

            //centroid output
            Point3d centroid = new Point3d(sectionProperties.cx, sectionProperties.cy, 0.0);
            DA.SetData("Centroid", centroid);


            DA.SetData("Area", sectionProperties.Area);
            DA.SetData("Eff. Poisson’s ratio", sectionProperties.nu_eff);
            DA.SetData("First moment about x", sectionProperties.qx);
            DA.SetData("First moment about y", sectionProperties.qy);
            DA.SetData("Second moment about global x", sectionProperties.ixx_g);
            DA.SetData("Second moment about global y", sectionProperties.iyy_g);
            DA.SetData("Second moment about global xy", sectionProperties.ixy_g);
            DA.SetData("Second moment about centroidal x", sectionProperties.ixx_c);
            DA.SetData("Second moment about centroidal y", sectionProperties.iyy_c);
            DA.SetData("Second moment about centroidal xy", sectionProperties.ixy_c);
            DA.SetData("Section modulus zxx_plus", sectionProperties.zxx_plus);
            DA.SetData("Section modulus zxx_minus", sectionProperties.zxx_minus);
            DA.SetData("Section modulus zyy_plus", sectionProperties.zyy_plus);
            DA.SetData("Section modulus zyy_minus", sectionProperties.zyy_minus);
            DA.SetData("Radius of gyration rx", sectionProperties.rx_c);
            DA.SetData("Radius of gyration ry", sectionProperties.ry_c);
            DA.SetData("Second moment about 11-axis", sectionProperties.i11_c);
            DA.SetData("Second moment about 11-axis", sectionProperties.i11_c);
            DA.SetData("Principal axis angle", sectionProperties.phi);
            DA.SetData("Section modulus about 11-axis plus", sectionProperties.z11_plus);
            DA.SetData("Section modulus about 11-axis minus", sectionProperties.z11_minus);
            DA.SetData("Section modulus about 22-axis plus", sectionProperties.z22_plus);
            DA.SetData("Section modulus about 22-axis minus", sectionProperties.z22_minus);
            DA.SetData("Radius of gyration about 11-axis", sectionProperties.r11_c);
            DA.SetData("Radius of gyration about 22-axis", sectionProperties.r22_c);
        }

    }
}