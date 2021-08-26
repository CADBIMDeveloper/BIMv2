#region Namespace
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;


#endregion

namespace BIMv2
{
    [Transaction(TransactionMode.Manual)]
    class WriteNameRoomInPlumbingFixtures: IExternalCommand
    {
        Application _app;
        Document _doc;
        private Autodesk.Revit.Creation.Application creApp;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            _app = uiApp.Application;
            _doc = uiDoc.Document;
            creApp = commandData.Application.Application.Create;
            
            using (Transaction t = new Transaction(_doc))
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

            IList<Element> allElements = GetAllElements();

            for (int i = 0; i < allElements.Count; i++)
            {
               Location loc = allElements[i].Location;
               
               LocationPoint locPoint = (LocationPoint)loc;
               XYZ pointPF = locPoint.Point;

               Room myRoom = _doc.GetRoomAtPoint(pointPF);

               if (myRoom == null)
               { 
                   nameRoom = "не в помещении";
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
            IList<Element> plFixList = plumbingFixCollector.ToElements();

            // List all generic
            var genericCollector = new FilteredElementCollector(_doc).OfClass(typeof(FamilyInstance));
            genericCollector.OfCategory(BuiltInCategory.OST_GenericModel);
            IList<Element> plFixList2 = genericCollector.ToElements();

            foreach (var VARIABLE in plFixList2)
            {
                plFixList.Add(VARIABLE);
            }

            // List all SE
            var seCollector = new FilteredElementCollector(_doc).OfClass(typeof(FamilyInstance));
            seCollector.OfCategory(BuiltInCategory.OST_SpecialityEquipment);
            IList<Element> plFixList3 = seCollector.ToElements();

            foreach (var VARIABLE in plFixList3)
            {
                plFixList.Add(VARIABLE);
            }

            // List all furniture
            var fCollector = new FilteredElementCollector(_doc).OfClass(typeof(FamilyInstance));
            fCollector.OfCategory(BuiltInCategory.OST_Furniture);
            IList<Element> plFixList4 = fCollector.ToElements();

            foreach (var VARIABLE in plFixList4)
            {
                plFixList.Add(VARIABLE);
            }
            
            return plFixList;
        }
    }
}
