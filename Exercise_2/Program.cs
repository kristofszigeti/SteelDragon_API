using FemDesign;
using FemDesign.Bars;
using FemDesign.Calculate;
using FemDesign.Geometry;
using FemDesign.Loads;
using FemDesign.Releases;
using FemDesign.Results;
using FemDesign.Supports;

// INPUT

string Dir = @"C:\Program Files\StruSoft\FEM-Design 24 Educational"; //fd3dstruct directory
double length = 12; // m
var height = 4; //m
var placement1 = height/2; //m
var placement2 = height/2; //m
var P = - 100; // kN
var q = - 20; // kN/m

//endpoints of beams
var Point1 = new Point3d(placement1,0,height);
var Point2 = new Point3d(length-placement2, 0, height);

//points of supports
var Point3 = new Point3d(0,0,0);
var Point4 = new Point3d(length, 0, 0);

// define material
var materialDatabase = FemDesign.Materials.MaterialDatabase.GetDefault();
var material = materialDatabase.MaterialByName("S 355");

// get material names if you want to see what is available
var materialNames = materialDatabase.MaterialNames();

// define section
var sectionDatabase = FemDesign.Sections.SectionDatabase.GetDefault();
var section = sectionDatabase.SectionByName("IPE 360");
var sectionBar = sectionDatabase.SectionByName("VKR 120x80x6.3");

// get section names if you want to see what is available
var sectionNames = sectionDatabase.SectionNames();

// define beams
var beam1 = new Bar(Point1, Point2, material, section, BarType.Beam);
var beam2 = new Bar(Point3, Point4, material, section, BarType.Beam);

//define rods
var Point5 = new Point3d(length/2, 0, height);
var Point6 = new Point3d(length/3, 0, 0);
var Point7 = new Point3d(2*length/3, 0, 0);

var edge5 = new Edge(Point1, Point3);
var edge6 = new Edge(Point2, Point4);

var edge7 = new Edge(Point1, Point6);
var edge8 = new Edge(Point6, Point5);
var edge9 = new Edge(Point5, Point7);
var edge10 = new Edge(Point7, Point2);

var connectivity = new FemDesign.Bars.Connectivity
{Tx = true, TxRelease = 0, Ty = false, TyRelease = 0.4, // valid only if Ty = false. Same for the others.
    Tz = true, TzRelease = 0, Rx = true, RxRelease = 0, Ry = true, RyRelease = 0, Rz = true, RzRelease = 0};

var connectivityRigid = FemDesign.Bars.Connectivity.Rigid;
var connectivityHinged = FemDesign.Bars.Connectivity.Hinged;

var rod1 = new Beam(edge: edge5, material, section, startConnectivity: connectivityHinged, endConnectivity: connectivityHinged);
var rod2 = new Beam(edge: edge6, material, section, startConnectivity: connectivityHinged, endConnectivity: connectivityHinged);
var rod3 = new Beam(edge: edge7, material, sectionBar, startConnectivity: connectivityHinged, endConnectivity: connectivityHinged);
var rod4 = new Beam(edge: edge8, material, sectionBar, startConnectivity: connectivityHinged, endConnectivity: connectivityHinged);
var rod5 = new Beam(edge: edge9, material, sectionBar, startConnectivity: connectivityHinged, endConnectivity: connectivityHinged);
var rod6 = new Beam(edge: edge10, material, sectionBar, startConnectivity: connectivityHinged, endConnectivity: connectivityHinged);

// define 2 supports
var plane1 = new Plane(Point3, new Vector3d(0,1,0), new Vector3d(0,0,1));
var plane2 = new Plane(Point4, new Vector3d(0,1,0), new Vector3d(0,0,1));

//hinge support properties
var support1 = new PointSupport(plane1, true, true, true, false, true, false);
var support2 = new PointSupport(plane2, true, true, true, false, true, false);

// define load case
var lcDL = new LoadCase("DeadLoad", LoadCaseType.DeadLoad, LoadCaseDuration.Permanent);
var lcIL = new LoadCase("IL", LoadCaseType.Static, LoadCaseDuration.Permanent);

// define line load
var constantForce = new Vector3d(0,0, P);
var edge1 = beam1.Edge;
var lineLoad1 = new LineLoad(edge1, constantForce, lcIL, ForceLoadType.Force);

// define point loads
var ptLoadLocation1 = new Point3d(length, 0, height);
var force = new Vector3d(q,0, 0);
var pointLoad1 = new PointLoad(ptLoadLocation1, force, lcIL, "", ForceLoadType.Force);

// define load combination
var loadCombination = new LoadCombination("ULS", LoadCombType.UltimateOrdinary, (lcDL, 1.0), (lcIL, 1.5) );

// define a model
var model = new Model(Country.H);

model.AddElements(beam1, beam2, rod1, rod2, rod3, rod4, rod5, rod6);
model.AddElements(support1, support2);

model.AddLoadCases(lcDL, lcIL);
model.AddLoadCombinations(loadCombination);

model.AddLoads(pointLoad1);
model.AddLoads(lineLoad1);
var connection = new FemDesignConnection(Dir, keepOpen: true);

connection.Open(model);

var analysis = Analysis.StaticAnalysis();
connection.RunAnalysis(analysis);

var design = new Design(autoDesign: true,check: true, loadCombination: true, applyChanges: false);
connection.RunDesign(CmdUserModule.STEELDESIGN, design);

var utilisation = connection.GetResults<FemDesign.Results.BarSteelUtilization>();
var displacement = connection.GetResults<FemDesign.Results.BarDisplacement>();

var units = FemDesign.Results.UnitResults.Default();
units.Displacement = FemDesign.Results.Displacement.mm;
units.Force = FemDesign.Results.Force.kN;
connection.Dispose();

Console.WriteLine("Max utilisation: " + utilisation[0].Max + " %");
Console.WriteLine("Max displacement: " + displacement.Select(x => x.Ez).Max()*1000 + " mm");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
