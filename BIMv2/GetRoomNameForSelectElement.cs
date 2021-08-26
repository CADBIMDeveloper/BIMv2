using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace BIMv2
{
    [Transaction(TransactionMode.Manual)]
    public class GetRoomNameForSelectElement :IExternalCommand
    {
        // Member variables
        Application _app;
        Document _doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var uiDoc = uiApp.ActiveUIDocument;
            
            _app = uiApp.Application;
            _doc = uiDoc.Document;

            // (1) select an object on a screen. (We'll come back to the selection in the UI Lab later.)
            Reference r = uiDoc.Selection.PickObject(ObjectType.Element, "Pick an element");

            // We have picked something.
            Element elem = uiDoc.Document.GetElement(r);
            ElementId elemTypeId = elem.GetTypeId();

            ElementType elemType = (ElementType)_doc.GetElement(elemTypeId); // since 2013
            
            // (5) location
            ShowLocation(elem);
            
            return Result.Succeeded;
        }

        public void ShowLocation(Element e)
        {
            string s = "Room Info: " + "\n" + "\n";
            Location loc = e.Location;
            string nameRoom="";

            if (loc is LocationPoint)
            {
                // (1) we have a location point

                LocationPoint locPoint = (LocationPoint)loc;
                XYZ pt = locPoint.Point;

                //Get room for Point
                Room myRoom = _doc.GetRoomAtPoint(pt);
                if (myRoom == null)
                {
                    nameRoom = "нету";
                }
                else
                {
                    nameRoom = myRoom.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
                }

                double r = locPoint.Rotation;

                //s += "LocationPoint" + "\n";
                //s += "Point = " + PointToString(pt) + "\n";
                //s += "Rotation = " + r.ToString() + "\n";
                s += "Room = " + nameRoom + "\n";
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
            TaskDialog.Show("Room", s);
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
        

    }
}

