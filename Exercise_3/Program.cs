using FemDesign;
using FemDesign.Loads;
using FemDesign.Geometry;
using System;
using System.Collections.Specialized;
using System.Net.Mail;
using FemDesign.Results;


var beamSections = new List<string>(){"HEA200", "HEA300", "IPE200", "VKR 120x80x6.3", "CHS127-6.3"};

foreach(var beamsection in beamSections)
{
var colLeft = new FemDesign.Geometry.Edge(new Point3d(0,0,0), new Point3d(2,0,6));
var colRight = new FemDesign.Geometry.Edge(new Point3d(10,0,0), new Point3d(8,0,6));
var beam = new FemDesign.Geometry.Edge(new Point3d(2,0,6), new Point3d(8,0,6));

var motions = new FemDesign.Releases.Motions(
    FemDesign.Releases.Motions.ValueRigidPoint, 
    FemDesign.Releases.Motions.ValueRigidPoint, 
    FemDesign.Releases.Motions.ValueRigidPoint, 
    FemDesign.Releases.Motions.ValueRigidPoint, 
    FemDesign.Releases.Motions.ValueRigidPoint, 
    FemDesign.Releases.Motions.ValueRigidPoint);

var rotations = new FemDesign.Releases.Rotations(0,0,0,0,0,0);

var supportLeft = new FemDesign.Supports.PointSupport(new Point3d(0,0,0), true, true, true, true, true, true);
var supportRight = new FemDesign.Supports.PointSupport(new Point3d(10,0,0), true, true, true, true, true, true);


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

var Qloadvalue = 10;
// create loads
var QLoad = new FemDesign.Loads.LineLoad(
    edge: beam,
    constantForce: new FemDesign.Geometry.Vector3d(0, 0, -Qloadvalue), // Uniform load in negative Z
    loadCase: ldLL,
    loadType: FemDesign.Loads.ForceLoadType.Force,
    $"Uniform Load -{Qloadvalue} kN/m");

var Ploadvalue = 5;

var PLoad = new FemDesign.Loads.PointLoad(
    new Point3d(8,0,6), // Applied at top of the right column
    new FemDesign.Geometry.Vector3d(-Ploadvalue,0,0), // Force in negative X-direction
    ldWind,
    $"Horizontal Point Load -{Ploadvalue} kN",
    FemDesign.Loads.ForceLoadType.Force);

var loads = new List<FemDesign.GenericClasses.ILoadElement>(){PLoad, QLoad};

var materialDatabase = FemDesign.Materials.MaterialDatabase.GetDefault();
var material = materialDatabase.MaterialByName("S 355"); 

var sectionDatabase = FemDesign.Sections.SectionDatabase.GetDefault();
var section = sectionDatabase.SectionByName(beamsection);

var beammain= new FemDesign.Bars.Beam(beam, material, section);
var colL= new FemDesign.Bars.Beam(colLeft, material, section);
var colR= new FemDesign.Bars.Beam(colRight, material, section);
var model = new FemDesign.Model(Country.H);

model.AddSupports(supportLeft, supportRight);
model.AddElements(beammain, colL, colR);
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

    var maxBending = beamForces_Combo.Select(x => x.My).Max();
    var minBending = beamForces_Combo.Select(x => x.My).Min();

    Console.WriteLine("Hi, Budapest!");
    Console.WriteLine("Salve, Firenze!");
    Console.WriteLine("We are the Steel Dragons!");
    Console.WriteLine("-------------------------");
     Console.WriteLine("Looping over the steel section profiles.");
    Console.WriteLine($"Steel Section: {beamsection}");
    Console.WriteLine($"Max displacement z: {maxDisplacement_z} mm");
    Console.WriteLine($"Max displacement x: {maxDisplacement_x} mm");
    Console.WriteLine($"Max Bending moment is: {maxBending} kNm");
    Console.WriteLine($"Min Bending moment is: {minBending} kNm");
    Console.WriteLine($"========================");
    Console.WriteLine();
    }
}
