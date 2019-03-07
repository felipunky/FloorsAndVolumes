using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace FloorsAndVolumes
{
    public class FloorsAndVolumesComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public FloorsAndVolumesComponent()
          : base("FloorsAndVolumes", "FloorsAndVolumes",
              "Finds the region intersection of 1, 2 or 3 shapes.",
              "Intersect", "Shape")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddPointParameter("points", "pts", "We will use this points to do the culling for our region union", GH_ParamAccess.tree);
            pManager.AddPlaneParameter("frames", "fms", "We need this planes to run the region union algorithm", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("collisions", "coll", "We need this to know which algo to run", GH_ParamAccess.item);
            pManager.AddCurveParameter("curves", "cvs", "We need this to perform the region union algo", GH_ParamAccess.tree);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddCurveParameter("floors", "floo", "Outputs the floor plans", GH_ParamAccess.list);
            pManager.AddBrepParameter("volumes", "v3", "Outputs the volume of the buildings", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("numberOfBranches", "branches", "Outputs the number of branches that the original trees contain", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            var curves = new DataTree<Polyline>();
            var points = new DataTree<Point3d>();
            var frames = new DataTree<Plane>();
            int collisions = 0;

            var gCurves = new GH_Structure<GH_Curve>();
            var gPoints = new GH_Structure<GH_Point>();
            var gFrames = new GH_Structure<GH_Plane>();

            if( !DA.GetDataTree(0, out gPoints) ) return;
            if( !DA.GetDataTree(1, out gFrames) ) return;
            DA.GetData(2, ref collisions);
            if( !DA.GetDataTree(3, out gCurves) ) return;
            
            var tempPoint = new Point3d();
            foreach( var path in gPoints.Paths )
            {

                var branch = gPoints.get_Branch( path );

                foreach( var item in branch )
                {

                    GH_Convert.ToPoint3d( item, ref tempPoint, GH_Conversion.Both );

                    points.Add( tempPoint, path );

                }

            }
            
            Curve tempCurve = null;
            var tempPoly = new Polyline();
            var curvesList = new List<Curve>();
            foreach (var path in gCurves.Paths)
            {

                var branch = gCurves.get_Branch(path);

                foreach (var item in branch)
                {

                    GH_Convert.ToCurve(item, ref tempCurve, GH_Conversion.Both);
                    tempCurve.TryGetPolyline( out tempPoly );

                    curves.Add(tempPoly, path);

                }

            }
            
            var tempPlane = new Plane();
            foreach( var path in gFrames.Paths )
            {

                var branch = gFrames.get_Branch( path );

                foreach( var item in branch )
                {

                    GH_Convert.ToPlane(item, ref tempPlane, GH_Conversion.Both);
                    frames.Add( tempPlane, path );

                }

            }
            
            int branches = curves.BranchCount;
            List<Polyline> listOut = new List<Polyline>();

            if (branches == 1)
            {

                listOut.AddRange(curves.Branch(0));

            }

            if (branches == 2)
            {

                if (collisions != 0)
                {

                    int branchOneCount = curves.Branch(0).Count;

                    var pointComparer = points.Branch(0)[curves.Branch(0).Count - 1].Z;

                    var comparer = new List<double>();

                    var listOfCurves = new List<Polyline>();
                    var listOfCurvesOne = new List<Polyline>();
                    var listOfCurvesTwo = new List<Polyline>();

                    listOfCurves.AddRange(curves.Branch(0));

                    var listOfPlanes = new List<Plane>();
                    var listOfPlanesOne = new List<Plane>();

                    listOfPlanes.AddRange(frames.Branch(0));

                    for (int i = 0; i < curves.Branch(1).Count; ++i)
                    {

                        comparer.Add(points.Branch(1)[i].Z);

                        if (pointComparer >= comparer[i])
                        {

                            listOfCurves.Add(curves.Branch(1)[i]);

                        }

                        else
                        {

                            listOfCurvesOne.Add(curves.Branch(1)[i]);
                            listOfPlanesOne.Add(frames.Branch(1)[i]);

                        }

                    }

                    var listOfUnions = RegionUnion(listOfCurves, listOfPlanes);

                    listOut.AddRange(listOfUnions);
                    listOut.AddRange(listOfCurvesOne);

                }

                else
                {

                    for (int i = 0; i < branches; ++i)
                    {

                        listOut.AddRange(curves.Branch(i));

                    }

                }

            }

            if (branches == 3)
            {

                if (collisions != 0)
                {

                    int branchOneCount = curves.Branch(0).Count;

                    var pointComparer = points.Branch(0)[curves.Branch(0).Count - 1].Z;
                    var pointComparerOne = points.Branch(1)[curves.Branch(1).Count - 1].Z;

                    var comparer = new List<double>();

                    var listOfCurves = new List<Polyline>();
                    var listOfCurvesOne = new List<Polyline>();
                    var listOfCurvesTwo = new List<Polyline>();

                    listOfCurves.AddRange(curves.Branch(0));

                    var listOfPlanes = new List<Plane>();
                    var listOfPlanesOne = new List<Plane>();

                    listOfPlanes.AddRange(frames.Branch(0));

                    for (int i = 0; i < curves.Branch(1).Count; ++i)
                    {

                        comparer.Add(points.Branch(1)[i].Z);

                        if (pointComparer >= comparer[i])
                        {

                            listOfCurves.Add(curves.Branch(1)[i]);

                        }

                        else
                        {

                            listOfCurvesOne.Add(curves.Branch(1)[i]);
                            listOfCurvesOne.Add(curves.Branch(2)[i]);
                            listOfPlanesOne.Add(frames.Branch(1)[i]);

                        }

                    }

                    var comparerOne = new List<double>();

                    for (int i = 0; i < curves.Branch(2).Count; ++i)
                    {

                        comparerOne.Add(points.Branch(2)[i].Z);

                        if (pointComparer >= comparerOne[i])
                        {

                            listOfCurves.Add(curves.Branch(2)[i]);

                        }

                        if (pointComparerOne >= comparerOne[i])
                        {

                        }

                        else
                        {

                            listOfCurvesTwo.Add(curves.Branch(2)[i]);

                        }

                    }

                    List<Polyline> listOfUnionObjs = RegionUnion(listOfCurves, listOfPlanes);
                    List<Polyline> listOfUnionObjsOne = RegionUnion(listOfCurvesOne, listOfPlanesOne);

                    listOut.AddRange(listOfUnionObjs);
                    listOut.AddRange(listOfUnionObjsOne);
                    listOut.AddRange(listOfCurvesTwo);

                }

                else
                {

                    for (int i = 0; i < branches; ++i)
                    {

                        listOut.AddRange(curves.Branch(i));

                    }

                }

            }

            DA.SetDataList(0, listOut);

            var volume = new DataTree<Brep>();
            var polyToCurves = new DataTree<Curve>();

            for (int i = 0; i < branches; ++i)
            {

                var path = new GH_Path(i);

                for (int j = 0; j < curves.Branch(i).Count; ++j)
                {

                    polyToCurves.Add(curves.Branch(i)[j].ToNurbsCurve(), path);

                }

            }

            for (int i = 0; i < branches; ++i)
            {

                volume.Add(Brep.CreateFromLoft(polyToCurves.Branch(i), Point3d.Unset, Point3d.Unset, LoftType.Tight, false)[0].CapPlanarHoles(0.0001), new GH_Path(i));

            }

            DA.SetDataTree(1, volume);

            DA.SetData(2, branches);

        }

        List<Polyline> RegionUnion(List<Polyline> listOfCurves, List<Plane> frames)
        {

            var regionUnion = new SurfaceComponents.SolidComponents.Component_CurveBooleanUnion();
            var regionUnionInputCurves = regionUnion.Params.Input[0] as Grasshopper.Kernel.GH_PersistentParam<Grasshopper.Kernel.Types.GH_Curve>;
            regionUnionInputCurves.PersistentData.ClearData();

            int curveCount = listOfCurves.Count;
            int branchOneCount = frames.Count;

            var ghCurve = new GH_Curve[curveCount];

            var regionUnionInputPlanes = regionUnion.Params.Input[1] as Grasshopper.Kernel.GH_PersistentParam<Grasshopper.Kernel.Types.GH_Plane>;
            regionUnionInputPlanes.PersistentData.ClearData();

            for (int i = 0; i < curveCount; ++i)
            {

                regionUnionInputCurves.PersistentData.Append(new GH_Curve(listOfCurves[i].ToNurbsCurve()));

            }

            var ghPlane = new GH_Plane[branchOneCount];

            for (int i = 0; i < branchOneCount; ++i)
            {

                GH_Convert.ToGHPlane(frames[i], GH_Conversion.Both, ref ghPlane[i]);
                regionUnionInputPlanes.PersistentData.Append(new GH_Plane(ghPlane[i]));

            }


            regionUnion.ExpireSolution(true);

            var doc = new Grasshopper.Kernel.GH_Document();
            doc.AddObject(regionUnion, false);

            regionUnion.Params.Output[0].CollectData();

            int countOfUnions = regionUnion.Params.Output[0].VolatileDataCount;

            var listOfUnionObjs = new object();
            Curve temp = null;
            var polyOut = new Polyline[countOfUnions];

            for (int i = 0; i < countOfUnions; ++i)
            {

                listOfUnionObjs = regionUnion.Params.Output[0].VolatileData.get_Branch(i)[0];
                GH_Convert.ToCurve(listOfUnionObjs, ref temp, GH_Conversion.Both);
                temp.TryGetPolyline(out polyOut[i]);

            }

            doc.RemoveObject(regionUnion.Attributes, false);

            var polysOut = new List<Polyline>();
            polysOut.AddRange(polyOut);

            return polysOut;


        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e61118af-96cd-487f-ae26-fa9e1d2d69c9"); }
        }
    }
}
