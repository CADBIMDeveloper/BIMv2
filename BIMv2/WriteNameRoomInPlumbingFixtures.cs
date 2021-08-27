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
    public class WriteNameRoomInPlumbingFixtures : IExternalCommand
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
                t.Start("Select all plumbing fixtures");

                WriteNameRoomInElementMethod(uiApp);

                TaskDialog.Show("У тебя получилось", "Ты красавчик!☺☺☺");
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
                var myRoom = _doc.GetRoomAtPoint(pointPF);

                var nameRoom = "";

                if (myRoom != null)
                {
                    nameRoom = myRoom.get_Parameter(BuiltInParameter.ROOM_NUMBER).AsString() + " " +
                               myRoom.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();

                    e.get_Parameter(new Guid("c78f0a7d-b68b-4d21-a247-1c8c6ced8bc5"))
                        .Set(nameRoom).ToString();
                }
                else
                {
                    var doc2 = AllLinkedFiles(); // List all linked files

                    for (var i = 0; i < doc2.Count; i++)
                    {
                        var egElement = doc2[i];

                        var linkType = egElement as RevitLinkType;
                        var linkName = string.Concat(linkType.Name.Reverse().Skip(4).Reverse());
                        //Room myRoom2;

                        foreach (Document linkedDoc in uiApp.Application.Documents)
                            if (linkedDoc.Title.Equals(linkName))
                            {
                               var myRoom2 = linkedDoc.GetRoomAtPoint(pointPF);
                               if (myRoom2 != null){
                                   nameRoom = myRoom2.get_Parameter(BuiltInParameter.ROOM_NUMBER).AsString() + " " +
                                              myRoom2.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
                               } else
                               {
                                   nameRoom = "It's not in Room";
                                   e.get_Parameter(new Guid("c78f0a7d-b68b-4d21-a247-1c8c6ced8bc5"))
                                       .Set(nameRoom).ToString();
                               }
                               linkDocId = egElement.Id;
                               var collLinked = new FilteredElementCollector(linkedDoc);
                               var linkedRooms = collLinked
                                   .OfClass(typeof(SpatialElement)).ToElements();
                               if (linkedRooms != null)
                                   foreach (var l in linkedRooms)
                                       roomids.Add(l.Id);
                               //return linkedDoc;
                            }

                        //var myRoom2 = doc2[0].Document.GetRoomAtPoint(pointPF); //return main doc
                        
                        
                    }
                }
            }
        }

        public IList<Element> GetAllElements()
        {
            // List all plumbing fixtures
            var plumbingFixCollector = new FilteredElementCollector(_doc).OfClass(typeof(FamilyInstance));
            plumbingFixCollector.OfCategory(BuiltInCategory.OST_PlumbingFixtures);
            var plFixList = plumbingFixCollector.ToElements();

            // List all generic
            var genericCollector = new FilteredElementCollector(_doc).OfClass(typeof(FamilyInstance));
            genericCollector.OfCategory(BuiltInCategory.OST_GenericModel);
            var plFixList2 = genericCollector.ToElements();

            foreach (var VARIABLE in plFixList2) plFixList.Add(VARIABLE);

            // List all SE
            var seCollector = new FilteredElementCollector(_doc).OfClass(typeof(FamilyInstance));
            seCollector.OfCategory(BuiltInCategory.OST_SpecialityEquipment);
            var plFixList3 = seCollector.ToElements();

            foreach (var VARIABLE in plFixList3) plFixList.Add(VARIABLE);

            // List all furniture
            var fCollector = new FilteredElementCollector(_doc).OfClass(typeof(FamilyInstance));
            fCollector.OfCategory(BuiltInCategory.OST_Furniture);
            var plFixList4 = fCollector.ToElements();

            foreach (var VARIABLE in plFixList4) plFixList.Add(VARIABLE);

            return plFixList;
        }

        public IList<Element> AllLinkedFiles()
        {
            var coll = new FilteredElementCollector(_doc);
            var elems = coll
                .OfCategory(BuiltInCategory.OST_RvtLinks)
                .OfClass(typeof(RevitLinkType))
                .ToElements();
            var rvtLinked = elems;

            return rvtLinked;
        }
    }
}