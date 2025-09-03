using FemDesign;
using FemDesign.Loads;
using FemDesign.Geometry;
using System;
using System.Collections.Specialized;
using System.Net.Mail;
using FemDesign.Results;
using FemDesign.Bars;


var strHeights = new List<double>(){3,6};
var Ploadvalues = new List<double>(){5,10,15};

foreach(var height in strHeights)
{
    foreach(var Ploadvalue in Ploadvalues)
    {
var colLeftD = new FemDesign.Geometry.Edge(new Point3d(0,0,0), new Point3d(0,0,height));
var colLeftU = new FemDesign.Geometry.Edge(new Point3d(0,6,0), new Point3d(0,6,height));
var colRightD = new FemDesign.Geometry.Edge(new Point3d(6,0,0), new Point3d(6,0,height));
var colRightU = new FemDesign.Geometry.Edge(new Point3d(6,6,0), new Point3d(6,6,height));
var beamD = new FemDesign.Geometry.Edge(new Point3d(0,0,height), new Point3d(6,0,height));
var beamU = new FemDesign.Geometry.Edge(new Point3d(0,6,height), new Point3d(6,6,height));
var beamL = new FemDesign.Geometry.Edge(new Point3d(0,0,height), new Point3d(0,6,height));
var beamR = new FemDesign.Geometry.Edge(new Point3d(6,0,height), new Point3d(6,6,height));

var motions = new FemDesign.Releases.Motions(
    FemDesign.Releases.Motions.ValueRigidPoint, 
    FemDesign.Releases.Motions.ValueRigidPoint, 
    FemDesign.Releases.Motions.ValueRigidPoint, 
    FemDesign.Releases.Motions.ValueRigidPoint, 
    FemDesign.Releases.Motions.ValueRigidPoint, 
    FemDesign.Releases.Motions.ValueRigidPoint);

var rotations = new FemDesign.Releases.Rotations(0,0,0,0,0,0);

var supportLeftD = new FemDesign.Supports.PointSupport(new Point3d(0,0,0), true, true, true, true, true, true);
var supportLeftU = new FemDesign.Supports.PointSupport(new Point3d(0,6,0), true, true, true, true, true, true);
var supportRightD = new FemDesign.Supports.PointSupport(new Point3d(6,0,0), true, true, true, true, true, true);
var supportRightU = new FemDesign.Supports.PointSupport(new Point3d(6,6,0), true, true, true, true, true, true);


// var rigidSupport = FemDesign.Supports.PointSupport.Rigid(point1, motions, rotations);

// load cases
var ldDL = new FemDesign.Loads.LoadCase("DL",FemDesign.Loads.LoadCaseType.DeadLoad, FemDesign.Loads.LoadCaseDuration.Permanent);
var ldLL = new FemDesign.Loads.LoadCase("LL",FemDesign.Loads.LoadCaseType.Static, FemDesign.Loads.LoadCaseDuration.Permanent);
var ldWind = new FemDesign.Loads.LoadCase("WIND",FemDesign.Loads.LoadCaseType.Static, FemDesign.Loads.LoadCaseDuration.Permanent);


// load combination
var loadComb1 = new FemDesign.Loads.LoadCombination(
    "ULS", 
    FemDesign.Loads.LoadCombType.UltimateOrdinary,
    (ldDL, 1.3),
    (ldLL, 1.5),
    (ldWind, 1.5));

// create loads
var Qloadvalue = 10;
var QLoad1 = new FemDesign.Loads.LineLoad(
    edge: beamD,
    constantForce: new FemDesign.Geometry.Vector3d(0, 0, -Qloadvalue), // Uniform load in -Z
    loadCase: ldLL,
    loadType: FemDesign.Loads.ForceLoadType.Force,
    $"Uniform Load -{Qloadvalue} kN/m");

var QLoad2 = new FemDesign.Loads.LineLoad(
    edge: beamU,
    constantForce: new FemDesign.Geometry.Vector3d(0, 0, -Qloadvalue), // Uniform load in -Z
    loadCase: ldLL,
    loadType: FemDesign.Loads.ForceLoadType.Force,
    $"Uniform Load -{Qloadvalue} kN/m");

var QLoad3 = new FemDesign.Loads.LineLoad(
    edge: beamL,
    constantForce: new FemDesign.Geometry.Vector3d(0, 0, -Qloadvalue), // Uniform load in -Z
    loadCase: ldLL,
    loadType: FemDesign.Loads.ForceLoadType.Force,
    $"Uniform Load -{Qloadvalue} kN/m");

var QLoad4 = new FemDesign.Loads.LineLoad(
    edge: beamR,
    constantForce: new FemDesign.Geometry.Vector3d(0, 0, -Qloadvalue), // Uniform load in -Z
    loadCase: ldLL,
    loadType: FemDesign.Loads.ForceLoadType.Force,
    $"Uniform Load -{Qloadvalue} kN/m");

var PLoad1 = new FemDesign.Loads.PointLoad(
    new Point3d(6,0,height/2), // Applied at different of the right column, midpoint and topmost point
    new FemDesign.Geometry.Vector3d(-Ploadvalue,0,0), // Force in negative X-direction
    ldWind,
    $"Horizontal Point Load -{Ploadvalue} kN",
    FemDesign.Loads.ForceLoadType.Force);

var PLoad2 = new FemDesign.Loads.PointLoad(
    new Point3d(6,6,height/2), // Applied at different of the right column, midpoint and topmost point
    new FemDesign.Geometry.Vector3d(-Ploadvalue,0,0), // Force in negative X-direction
    ldWind,
    $"Horizontal Force -{Ploadvalue} kN",
    FemDesign.Loads.ForceLoadType.Force);

var loads = new List<FemDesign.GenericClasses.ILoadElement>(){PLoad1, PLoad2, QLoad1, QLoad2, QLoad3, QLoad4};

var materialDatabase = FemDesign.Materials.MaterialDatabase.GetDefault();
var material = materialDatabase.MaterialByName("S 355"); 

var sectionDatabase = FemDesign.Sections.SectionDatabase.GetDefault();
var section = sectionDatabase.SectionByName("HEA300");

var bmD= new FemDesign.Bars.Beam(beamD, material, section);
var bmL= new FemDesign.Bars.Beam(beamL, material, section);
var bmU= new FemDesign.Bars.Beam(beamU, material, section);
var bmR= new FemDesign.Bars.Beam(beamR, material, section);
var colLD= new FemDesign.Bars.Beam(colLeftD, material, section);
var colLU= new FemDesign.Bars.Beam(colLeftU, material, section);
var colRD= new FemDesign.Bars.Beam(colRightD, material, section);
var colRU= new FemDesign.Bars.Beam(colRightU, material, section);
var model = new FemDesign.Model(Country.H);

model.AddSupports(supportLeftD, supportLeftU, supportRightD, supportRightU);
model.AddElements(bmD, bmL, bmU, bmR, colLD, colLU, colRD, colRU);
model.AddLoadCases(ldDL, ldLL, ldWind);
model.AddLoadCombinations(loadComb1);
model.AddLoads(loads);

var units = FemDesign.Results.UnitResults.Default();
units.Displacement = FemDesign.Results.Displacement.mm;
units.Force = FemDesign.Results.Force.kN;

using (var connection = new FemDesign.FemDesignConnection( keepOpen: true))
    {
    connection.Open(model);

    var analysis = FemDesign.Calculate.Analysis.StaticAnalysis();
    connection.RunAnalysis(analysis);

    var nodalDisplacement = connection.GetLoadCombinationResults<FemDesign.Results.NodalDisplacement>(units: units);

    var beamForces_Combo = connection.GetLoadCombinationResults<FemDesign.Results.BarInternalForce>(units: units);

    var maxDisplacement_z = nodalDisplacement.Select(x => Math.Abs(x.Ez)).Max();
    var maxDisplacement_x = nodalDisplacement.Select(x => Math.Abs(x.Ex)).Max();
    var maxDisplacement_y = nodalDisplacement.Select(x => Math.Abs(x.Ey)).Max();    

    var maxBending_y = beamForces_Combo.Select(x => x.My).Max();
    var minBending_y = beamForces_Combo.Select(x => x.My).Min();
    var maxBending_z = beamForces_Combo.Select(x => x.Mz).Max();
    var minBending_z = beamForces_Combo.Select(x => x.Mz).Min();

    Console.WriteLine("Hi, Budapest!");
    Console.WriteLine("Salve, Firenze!");
    Console.WriteLine("We are the Steel Dragons!");
    Console.WriteLine("-------------------------");     
    Console.WriteLine("Looping over the structure height and point load values.");
    Console.WriteLine($"Structure Height: {height} m");
    Console.WriteLine($"Horizontal Point load: -{Ploadvalue} kN");
    Console.WriteLine($"Max displacement z: {maxDisplacement_z} mm");
    Console.WriteLine($"Max displacement x: {maxDisplacement_x} mm");
    Console.WriteLine($"Max displacement y: {maxDisplacement_y} mm");
    Console.WriteLine($"Max Bending moment is: {maxBending_y} kNm");
    Console.WriteLine($"Min Bending moment is: {minBending_y} kNm");
    Console.WriteLine($"Max Bending moment is: {maxBending_z} kNm");
    Console.WriteLine($"Min Bending moment is: {minBending_z} kNm");
    Console.WriteLine($"========================");
    Console.WriteLine();
    }
}
}
