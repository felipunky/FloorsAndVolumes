using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace FloorsAndVolumes
{
    public class FloorsAndVolumesInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "FloorsAndVolumes";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "Finds the region intersection of 1, 2 or 3 shapes. It also modifies Galapagos GenePools through sliders.";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("1d1368be-1f55-4748-b17d-b99834c553d0");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Felipe Gutierrez Duque";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "Email: felipegutierrezduque10@gmail.com";
            }
        }
    }
}
