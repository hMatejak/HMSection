using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace HMSection
{
    public class HMSectionInfo : GH_AssemblyInfo
    {
        public override string Name => "HMSection";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => Properties.Resources.main;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "HMSection is a grasshopper toolbox" +
            "for the analysis of cross-sections. Uses CrossSection.net prackage," +
            " that is based on python package sectionproperties.";

        public override Guid Id => new Guid("a6cb1a60-393c-4e9b-967e-67c31b98d7e4");

        //Return a string identifying you or your company.
        public override string AuthorName => "Jan Matěják";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "honza.m46@gmail.com";

        public override string AssemblyVersion => "0.5.0";
    }
}