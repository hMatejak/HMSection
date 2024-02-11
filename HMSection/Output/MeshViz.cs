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
using Grasshopper.Kernel.Types;
using TriangleNet;
using System.Collections.Generic;
using TriangleNet.Topology;
using TriangleNet.Geometry;

namespace HMSection.Output
{
    public class MeshViz : GH_Component
    {

        public MeshViz()
          : base("HMsec - Mesh", "M", "Displays mesh of section", "HMSection", "03-Section Properties")
        {
        }

        protected override System.Drawing.Bitmap Icon => Properties.Resources.mesh;

        public override Guid ComponentGuid => new Guid("220b7449-f51c-4c06-9460-ad5eaee1e78d");

        public override GH_Exposure Exposure => GH_Exposure.secondary;


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "M", "Mesh of section", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Mesh Points", "MP", "Points of generated mesh of section", GH_ParamAccess.list);
            pManager.AddLineParameter("Mesh Lines", "ML", "Lines of generated mesh of section", GH_ParamAccess.list);
        }

        /// <summary>
        /// Holds points of mesh of section
        /// </summary>
        List<Rhino.Geometry.Point3d> meshPoints = new List<Rhino.Geometry.Point3d>();

        /// <summary>
        /// Holds info about mesh lines
        /// </summary>
        List<Rhino.Geometry.Line> meshLines = new List<Rhino.Geometry.Line>();

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //resets hold values
            if (meshPoints != new List<Rhino.Geometry.Point3d>())
            {
                meshPoints = new List<Rhino.Geometry.Point3d>();
                meshLines = new List<Rhino.Geometry.Line>();
            }

            //unpack material
            GH_ObjectWrapper obj = new GH_ObjectWrapper();
            DA.GetData("Mesh", ref obj);
            Mesh mesh = obj.Value as Mesh;

            if (mesh != null)
            {
                ICollection<Vertex> vertices = mesh.Vertices;
                foreach (var vertex in vertices)
                {
                    meshPoints.Add(new Rhino.Geometry.Point3d(vertex.X, vertex.Y, 0));
                }

                //edges of triangulated triangles - lines
                ICollection<Triangle> triangles = mesh.Triangles;
                foreach (var triangel in triangles)
                {
                    //gets point numbers
                    Vertex p0 = triangel.GetVertex(0);
                    Vertex p1 = triangel.GetVertex(1);
                    Vertex p2 = triangel.GetVertex(2); ;

                    //gets points coordinates from mesh
                    Rhino.Geometry.Point3d point_0 = new Rhino.Geometry.Point3d(x: p0.X, y: p0.Y, z: 0);
                    Rhino.Geometry.Point3d point_1 = new Rhino.Geometry.Point3d(x: p1.X, y: p1.Y, z: 0);
                    Rhino.Geometry.Point3d point_2 = new Rhino.Geometry.Point3d(x: p2.X, y: p2.Y, z: 0);


                    meshLines.Add(new Rhino.Geometry.Line(point_0, point_1));
                    meshLines.Add(new Rhino.Geometry.Line(point_1, point_2));
                    meshLines.Add(new Rhino.Geometry.Line(point_2, point_0));
                }
            }
            DA.SetDataList("Mesh Points", meshPoints);
            DA.SetDataList("Mesh Lines", meshLines);

        }

    }
}