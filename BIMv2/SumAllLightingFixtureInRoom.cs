#region Namespace
using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
#endregion
namespace BIMv2
{
    [Transaction(TransactionMode.Manual)]
    public class SumAllLightingFixtureInRoom : IExternalCommand
    {
        private Application _app;
        private Document _doc;
        public ElementId linkDocId = new ElementId(BuiltInCategory.OST_RvtLinks);
        public List<ElementId> roomids = new List<ElementId>();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var uiDoc = uiApp.ActiveUIDocument;
            _app = uiApp.Application;
            _doc = uiDoc.Document;
            using (var t = new Transaction(_doc))
            {
                t.Start("Select all lighting fixtures");
                WriteNameRoomInElementMethod(uiApp);
                TaskDialog.Show("У тебя получилось", "Complite!!!☺☺☺");
                t.Commit();
                t.Start();
                var res = t.Commit();
            }
            return Result.Succeeded;
        }
        public void WriteNameRoomInElementMethod(UIApplication uiApp)
        {
            var allElements = GetAllElements();

            foreach (var e in allElements)
            {
                var loc = e.Location;
                var locPoint = (LocationPoint)loc;
                var pointPF = locPoint.Point;
                var myRoom = FindRoom(_doc, pointPF);
                var nameRoom = "";

                if (myRoom != null)
                {
                    nameRoom = myRoom.get_Parameter(BuiltInParameter.ROOM_NUMBER).AsString() + " " +
                                   myRoom.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();

                    e.get_Parameter(new Guid("c78f0a7d-b68b-4d21-a247-1c8c6ced8bc5"))
                        .Set(nameRoom);

                }
                else
                {
                    var linkedInstances = FindLinkedInstances();
                    foreach (var linkInstance in linkedInstances)
                    {
                        var transform = linkInstance.GetTotalTransform().Inverse;
                        var targetPoint = transform.OfPoint(pointPF);
                        var linkDocument = linkInstance.GetLinkDocument();

                        var room = FindRoom(linkDocument, targetPoint);
                        if (room != null)
                        {
                            string[] subs = room.Name.Split(' ');

                            e.get_Parameter(new Guid("c78f0a7d-b68b-4d21-a247-1c8c6ced8bc5"))
                                .Set(room.Number + " " + subs[0]);
                        }
                        else
                        {
                            nameRoom = "It's not in Room";
                            e.get_Parameter(new Guid("c78f0a7d-b68b-4d21-a247-1c8c6ced8bc5"))
                                .Set(nameRoom);
                        }

                    }
                }
            }
        }
        public IList<Element> GetAllElements()
        {
            // List all lighting fixtures
            var lightingFixCollector = new FilteredElementCollector(_doc).OfClass(typeof(FamilyInstance));
            lightingFixCollector.OfCategory(BuiltInCategory.OST_LightingFixtures);
            var plFixList = lightingFixCollector.ToElements();
            
            return plFixList;
        }
        private IEnumerable<RevitLinkInstance> FindLinkedInstances()
        {
            var collector = new FilteredElementCollector(_doc);
            return collector
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .ToList();
        }
        private static Room FindRoom(Document document, XYZ point)
        {
            return document
                .Phases
                .Cast<Phase>()
                .Select(x => document.GetRoomAtPoint(point, x))
                .FirstOrDefault(x => x != null);
        }


    }
}