/// <copyright>
/// https://github.com/hMatejak/HMSection
/// MIT License
///
/// Copyright(c) 2024 Jan Matěják
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

        public override string AssemblyVersion => "0.6.0";
    }
}