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

namespace HMSection.Input
{
    public class Material : GH_Component
    {

        public Material()
          : base("HMsec - Material", "M", "Material of section", "HMSection", "01-Input")
        {
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.material;

        public override Guid ComponentGuid => new Guid("40b80d9a-2609-4dce-b169-0d626a2ba484");

        public override GH_Exposure Exposure => GH_Exposure.secondary;


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of material", GH_ParamAccess.item, "Default");
            pManager.AddIntegerParameter("Id", "I", "Id of material", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("E", "E", "Elastic modulus", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("v", "V", "Poissons ratio", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("s", "S", "Yield strength", GH_ParamAccess.item, 1.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "M", "Material of Section", GH_ParamAccess.item);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = string.Empty;
            int matid = 0;
            double E = 0;
            double v = 0;
            double s = 0;

            DA.GetData(0, ref name);
            DA.GetData(1, ref matid);
            DA.GetData(2, ref E);
            DA.GetData(3, ref v);
            DA.GetData(4, ref s);

            SectionMaterial mat = new SectionMaterial(name, id: matid, elastic_modulus: E, poissons_ratio: v, yield_strength: s);

            DA.SetData(0, mat);

        }


    }
}