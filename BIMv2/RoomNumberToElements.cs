using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace BIMv2
{
    [Transaction(TransactionMode.Manual)]
    public class RoomNumberToElements :IExternalCommand
    {
        // Member variables
        Application _app;
        Document _doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get the access to the top most objects.
            // Notice that we have UI and DB versions for application and Document.
            // (We list them both here to show two versions.)

            var uiApp = commandData.Application;
            var uiDoc = uiApp.ActiveUIDocument;
            
            _app = uiApp.Application;
            _doc = uiDoc.Document;

            // (1) select an object on a screen. (We'll come back to the selection in the UI Lab later.)
            Reference r = uiDoc.Selection.PickObject(ObjectType.Element, "Pick an element");

            // We have picked something.
            Element elem = uiDoc.Document.GetElement(r);

            // (2) let's see what kind of element we got.
            // Key properties that we need to check are: Class, Category and if an element is ElementType or not.

            //установка значения для стены

            // SetElement(elem);


            //ShowBasicElementInfo(elem);

            // (3) now, we are going to identify each major types of element.
            //IdentifyElement(elem);

            // Now look at other properties - important ones are parameters, locations and geometry.

            // (4) first parameters.

            //ShowParameters(elem, "Element Parameters: ");

            // Check to see its type parameter as well

            ElementId elemTypeId = elem.GetTypeId();

            ElementType elemType = (ElementType)_doc.GetElement(elemTypeId); // since 2013
            //ShowParameters(elemType, "Type Parameters: ");

            // Okay. we saw a set or parameters for a given element or element type.
            // How can we access to each parameters. For example, how can we get the value of "length" information?
            // Here is how:

            // RetrieveParameter(elem, "Element Parameter (by Name and BuiltInParameter): ");
            // The same logic applies to the type parameter.
            //RetrieveParameter(elemType, "Type Parameter (by Name and BuiltInParameter): ");

            // (5) location
            ShowLocation(elem);

            // (6) geometry - the last piece. (Optional)
            //ShowGeometry(elem);

            // These are the common proerties.
            // There may be more properties specific to the given element class,
            // such as Wall.Width, .Flipped and Orientation. Expore using RevitLookup and RevitAPI.chm.



            return Result.Succeeded;
        }

        /// <summary>
        /// Show basic information about the given element.
        /// Note: we are intentionally including both element and element type
        /// here to compare the output on the same dialog.
        /// Compare, for example, the categories of element and element type.
        /// </summary>
        public void ShowBasicElementInfo(Element e)
        {
            // Let's see what kind of element we got.

            string s = "You picked:"
              + "\r\nClass name = " + e.GetType().Name
              + "\r\nCategory = " + e.Category.Name
              + "\r\nElement id = " + e.Id.ToString();

            // And check its type info.


            ElementId elemTypeId = e.GetTypeId(); // since 2011


            ElementType elemType = (ElementType)_doc.GetElement(elemTypeId); // since 2013

            s += "\r\nIts ElementType:"
              + " Class name = " + elemType.GetType().Name
              + " Category = " + elemType.Category.Name
              + " Element type id = " + elemType.Id.ToString();

            // Show what we got.
            TaskDialog.Show("Basic Element Info", s);
        }

        /// <summary>
        /// Identify the type of the element known to the UI.
        /// </summary>
        public void IdentifyElement(Element e)
        {
            // An instance of a system family has a designated class.
            // You can use it identify the type of element.
            // e.g., walls, floors, roofs.

            string s = "";

            if (e is Wall)
            {
                s = "Wall";
            }
            else if (e is Floor)
            {
                s = "Floor";
            }
            else if (e is RoofBase)
            {
                s = "Roof";
            }
            else if (e is FamilyInstance)
            {
                // An instance of a component family is all FamilyInstance.
                // We'll need to further check its category.
                // e.g., Doors, Windows, Furnitures.
                if (e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors)
                {
                    s = "Дверь";
                }
                else if (e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows)
                {
                    s = "Окно";
                }
                else if (e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Furniture)
                {
                    s = "Furniture";
                }
                else
                {
                    // e.g. Plant
                    s = "Component family instance";
                }
            }
            // Check the base class. e.g., CeilingAndFloor.
            else if (e is HostObject)
            {
                s = "System family instance";
            }
            else
            {
                s = "Other";
            }

            s = "Вы выбрали: " + s;

            TaskDialog.Show("Identify Element", s);
        }

        /// <summary>
        /// Show the parameter values of an element.
        /// </summary>
        public void ShowParameters(Element e, string header)
        {
            string s = string.Empty;

            foreach (Parameter param in e.GetOrderedParameters())
            {
                string name = param.Definition.Name;
                // To get the value, we need to parse the param depending on the storage type
                // see the helper function below
                string val = ParameterToString(param);
                s += "\r\n" + name + " = " + val;
            }

            TaskDialog.Show(header, s);
        }

        /// <summary>
        /// Helper function: return a string form of a given parameter.
        /// </summary>
        public static string ParameterToString(Parameter param)
        {
            string val = "none";

            if (param == null)
            {
                return val;
            }

            // To get to the parameter value, we need to parse it depending on its storage type

            switch (param.StorageType)
            {
                case StorageType.Double:
                    double dVal = param.AsDouble();
                    val = dVal.ToString();
                    break;
                case StorageType.Integer:
                    int iVal = param.AsInteger();
                    val = iVal.ToString();
                    break;
                case StorageType.String:
                    string sVal = param.AsString();
                    val = sVal;
                    break;
                case StorageType.ElementId:
                    ElementId idVal = param.AsElementId();
                    val = idVal.IntegerValue.ToString();
                    break;
                case StorageType.None:
                    break;
            }
            return val;
        }

        /// <summary>
        /// Examples of retrieving a specific parameter indivisually
        /// (hard coded for simplicity; This function works best
        /// with walls and doors).
        /// </summary>
        public void RetrieveParameter(Element e, string header)
        {
            string s = string.Empty;

            // As an experiment, let's pick up some arbitrary parameters.
            // Comments - most of instance has this parameter

            // (1) by BuiltInParameter.
            Parameter param = e.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
            if (param != null)
            {
                s += "Comments (by BuiltInParameter) = " + ParameterToString(param) + "\n";
            }

            // (2) by name. (Mark - most of instance has this parameter.) if you use this method, it will language specific.
            // param = e.get_Parameter("Mark");

            // 'Autodesk.Revit.DB.Element.get_Parameter(string)' is obsolete:
            // 'This property is obsolete in Revit 2015, as more than one parameter can have the same name on a given element.
            // Use Element.Parameters to obtain a complete list of parameters on this Element,
            // or Element.GetParameters(String) to get a list of all parameters by name,
            // or Element.LookupParameter(String) to return the first available parameter with the given name.'
            //
            //

            param = e.LookupParameter("Mark");
            if (param != null)
            {
                s += "Mark (by Name) = " + ParameterToString(param) + "\n";
            }




            // Though the first one is the most commonly used, other possible methods are:
            // (3) by definition
            // param = e.Parameter(Definition)
            // (4) and for shared parameters, you can also use GUID.
            // parameter = Parameter(GUID)

            // The following should be in most of type parameter

            param = e.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS);
            if (param != null)
            {
                s += "Type Comments (by BuiltInParameter) = " + ParameterToString(param) + "\n";
            }

            //param = e.get_Parameter("Fire Rating"); // Autodesk.Revit.DB.Element.get_Parameter(string)' is obsolete in 2015


            param = e.LookupParameter("Fire Rating");

            if (param != null)
            {
                s += "Fire Rating (by Name) = " + ParameterToString(param) + "\n";
            }

            // Using the BuiltInParameter, you can sometimes access one that is not in the parameters set.
            // Note: this works only for element type.

            param = e.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM);
            if (param != null)
            {
                s += "SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM (only by BuiltInParameter) = "
                    + ParameterToString(param) + "\n";
            }

            param = e.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM);
            if (param != null)
            {
                s += "SYMBOL_FAMILY_NAME_PARAM (only by BuiltInParameter) = "
                    + ParameterToString(param) + "\n";
            }

            // Show it.

            TaskDialog.Show(header, s);
        }

        /// <summary>
        /// Show the location information of the given element.
        /// The location can be a LocationPoint (e.g., furniture)
        /// or LocationCurve (e.g., wall).
        /// </summary>
        public void ShowLocation(Element e)
        {
            string s = "Location Information: " + "\n" + "\n";
            Location loc = e.Location;
            string sg="";

            if (loc is LocationPoint)
            {
                // (1) we have a location point

                LocationPoint locPoint = (LocationPoint)loc;
                XYZ pt = locPoint.Point;

                Room myRoom = _doc.GetRoomAtPoint(pt);
                if (myRoom == null)
                {
                    sg = "нету";
                }
                else
                {
                    sg = myRoom.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
                }

                double r = locPoint.Rotation;

                //s += "LocationPoint" + "\n";
                //s += "Point = " + PointToString(pt) + "\n";
                //s += "Rotation = " + r.ToString() + "\n";
                s += "Room = " + sg + "\n";
            }
            else if (loc is LocationCurve)
            {
                // (2) we have a location curve

                LocationCurve locCurve = (LocationCurve)loc;
                Curve crv = locCurve.Curve;

                s += "LocationCurve" + "\n";
                s += "EndPoint(0)/Start Point = " + PointToString(crv.GetEndPoint(0)) + "\n";
                s += "EndPoint(1)/End point = " + PointToString(crv.GetEndPoint(1)) + "\n";
                s += "Length = " + crv.Length.ToString() + "\n";

                // Location Curve also has property JoinType at the end

                s += "JoinType(0) = " + locCurve.get_JoinType(0).ToString() + "\n";
                s += "JoinType(1) = " + locCurve.get_JoinType(1).ToString() + "\n";
            }
            TaskDialog.Show("Show Location", s);
        }

        // Helper function: returns XYZ in a string form.

        public static string PointToString(XYZ p)
        {
            if (p == null)
            {
                return "";
            }

            return string.Format("({0},{1},{2})",
              p.X.ToString("F2"), p.Y.ToString("F2"),
              p.Z.ToString("F2"));
        }

        /// <summary>
        /// This is lengthy, so optional:
        /// show the geometry information of the given element.
        /// Here is how to access it.
        /// You can also go through tis using RevitLookup instead.
        /// </summary>
        public void ShowGeometry(Element e)
        {
            // Set a geometry option

            Options opt = _app.Create.NewGeometryOptions();
            //opt.DetailLevel = DetailLevels.Fine; // 2012
            opt.DetailLevel = ViewDetailLevel.Fine; // since 2013

            // Get the geometry from the element
            GeometryElement geomElem = e.get_Geometry(opt);

            // If there is a geometry data, retrieve it as a string to show it.
            string s = (geomElem == null) ?
              "no data" :
              GeometryElementToString(geomElem);

            TaskDialog.Show("Show Geometry", s);
        }

        /// <summary>
        /// Helper Function: parse the geometry element by geometry type.
        /// Geometry informaion can easily go into depth. Here we look at the top level.
        /// See RevitCommands in the SDK sample for complete implementation.
        /// </summary>
        public static string GeometryElementToString(GeometryElement geomElem)
        {
            string str = string.Empty;

            foreach (GeometryObject geomObj in geomElem)
            {

                if (geomObj is Solid)
                {
                    // ex. wall

                    Solid solid = (Solid)geomObj;
                    //str += GeometrySolidToString(solid);

                    str += "Solid" + "\n";
                }
                else if (geomObj is GeometryInstance)
                {
                    // ex. door/window

                    str += " -- Geometry.Instance -- " + "\n";
                    GeometryInstance geomInstance = (GeometryInstance)geomObj;
                    GeometryElement geoElem = geomInstance.SymbolGeometry;

                    str += GeometryElementToString(geoElem);
                }
                else if (geomObj is Curve)
                {
                    Curve curv = (Curve)geomObj;
                    //str += GeometryCurveToString(curv);

                    str += "Curve" + "\n";
                }
                else if (geomObj is Mesh)
                {
                    Mesh mesh = (Mesh)geomObj;
                    //str += GeometryMeshToString(mesh);

                    str += "Mesh" + "\n";
                }
                else
                {
                    str += " *** unkown geometry type" + geomObj.GetType().ToString();
                }
            }
            return str;
        }

    }
}

