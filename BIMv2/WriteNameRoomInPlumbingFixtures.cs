#region Namespace

using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

#endregion

namespace BIMv2
{
    [Transaction(TransactionMode.Manual)]
    internal class WriteNameRoomInPlumbingFixtures : IExternalCommand
    {
        private Application _app;
        private Document _doc;
        private Autodesk.Revit.Creation.Application creApp;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var uiDoc = uiApp.ActiveUIDocument;
            _app = uiApp.Application;
            _doc = uiDoc.Document;
            creApp = commandData.Application.Application.Create;


            using (var t = new Transaction(_doc))
            {
                t.Start("Select all plumbing fixtures");

                WriteNameRoomInElementMethod();

                TaskDialog.Show("У тебя получилось", "Ты красавчик!☺☺☺");
                t.Commit();

                t.Start();

                var res = t.Commit();
            }

            return Result.Succeeded;
        }

        private void WriteNameRoomInElementMethod()
        {
            string nameRoom = "";

            var allElements = GetAllElements();

            for (var i = 0; i < allElements.Count; i++)
            {
                var loc = allElements[i].Location;

                var locPoint = (LocationPoint)loc;
                var pointPF = locPoint.Point;

                var myRoom = _doc.GetRoomAtPoint(pointPF);
                
                if (myRoom == null)
                {
                    var linkedFilesMayBe = AllLinkedFiles();
                    for (int j = 0; j < linkedFilesMayBe.Count; j++)
                    {
                        
                        Document doc2 = linkedFilesMayBe[j].Document;
                        var myRoom2 = doc2.GetRoomAtPoint(pointPF);
                        if (myRoom2 != null)
                        {
                            nameRoom = myRoom2.get_Parameter(BuiltInParameter.ROOM_NUMBER).AsString() + " " +
                                        myRoom2.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
                        }
                    }
                    nameRoom = "It's not in Room";
                    allElements[i].get_Parameter(new Guid("c78f0a7d-b68b-4d21-a247-1c8c6ced8bc5"))
                        .Set(nameRoom).ToString();
                }
                else
                {
                    nameRoom = myRoom.get_Parameter(BuiltInParameter.ROOM_NUMBER).AsString() + " " +
                               myRoom.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();

                    allElements[i].get_Parameter(new Guid("c78f0a7d-b68b-4d21-a247-1c8c6ced8bc5"))
                        .Set(nameRoom).ToString();
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
            
                // start by the root links (no parent node)
                FilteredElementCollector coll = new FilteredElementCollector(_doc);
                coll.OfClass(typeof(RevitLinkInstance));
                var rvtLinked = coll.ToElements(); ;
                return rvtLinked;
        }
    }
}