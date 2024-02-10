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
    public class BasicOut : GH_Component
    {

        public BasicOut()
          : base("HMsec - BasicOUT", "BO", "...", "HMSection", "03-Section Properties")
        {
        }

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("ba48f676-895f-4885-b829-68ad4d08f4c8");

        public override GH_Exposure Exposure => GH_Exposure.hidden;


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

            //centroid output
            Point3d centroid = new Point3d(sectionProperties.cx, sectionProperties.cy, 0.0);
            DA.SetData("Centroid", centroid);

        }

    }
}