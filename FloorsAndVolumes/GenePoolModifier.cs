using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace FloorsAndVolumes
{
    public class GenePoolModifier : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GenePoolModifier class.
        /// </summary>
        public GenePoolModifier()
          : base("GenePoolModifier", "GeneModifier",
              "Modifies GenePools through their name",
              "Params", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddTextParameter( "geneList", "gnLst", "Name of the geneList that you want to modify", GH_ParamAccess.item );
            pManager.AddIntegerParameter( "count", "cnt", "Enter the number of sliders you want your genePool to have", GH_ParamAccess.item );
            pManager.AddIntervalParameter( "domain", "dmn", "Enter the domain that you want your sliders to be in", GH_ParamAccess.item );

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddTextParameter( "print", "pnt", "Prints if the genePool exists or not", GH_ParamAccess.item );

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            string genelist = "";
            DA.GetData( 0, ref genelist );

            int count = 0;
            DA.GetData( 1, ref count );

            Interval domain = new Interval();
            DA.GetData( 2, ref domain );

            var prints ="";

            GalapagosComponents.GalapagosGeneListObject GComp = null;

            GH_Document ghDocument = OnPingDocument();

            if (ghDocument != null)
            {

                foreach (IGH_DocumentObject obj in ghDocument.Objects)
                {

                    if (obj.ToString() == "GalapagosComponents.GalapagosGeneListObject")
                    {

                        if (obj.NickName == genelist)
                        {
                            GComp = (GalapagosComponents.GalapagosGeneListObject)obj;
                        }

                    }
                    
                }
                if (GComp == null)
                {
                    prints = "GenePool has not been found!";
                    return;
                }

                else
                {

                    prints = "GenePool found!";

                }

                GComp.Count = count;

                GComp.RemapToNewLimits(Convert.ToDecimal(domain.T0), Convert.ToDecimal(domain.T1));

                GComp.ExpireSolution(true);
                

            }

            else
            {

                prints = "No GH_Document";

            }

            DA.SetData( 0, prints );

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("eb8b1371-eef6-4e38-9710-42e30c1d2db1"); }
        }
    }
}