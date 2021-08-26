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
                WriteNameRoomInPlumbingFixturesMethod();
                TaskDialog.Show("У тебя получилось", "Ты красавчик!☺☺☺");
                t.Commit();

                t.Start();

                var res = t.Commit();

            }

            return Result.Succeeded;
        }

        private void WriteNameRoomInPlumbingFixturesMethod()
        {
            string nameRoom = "";

            IList<Element> plFixList = GetAllPlumbingFixtures();

            for (int i = 0; i < plFixList.Count; i++)
            {
               Location loc = plFixList[i].Location;
               
               LocationPoint locPoint = (LocationPoint)loc;
               XYZ pointPF = locPoint.Point;

               Room myRoom = _doc.GetRoomAtPoint(pointPF);
               nameRoom = myRoom.get_Parameter(BuiltInParameter.ROOM_NUMBER).AsString()+" "+
                          myRoom.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();

               //Parameter p = plFixList[i].get_Parameter(new Guid("c78f0a7d-b68b-4d21-a247-1c8c6ced8bc5"));

               plFixList[i].get_Parameter(new Guid("c78f0a7d-b68b-4d21-a247-1c8c6ced8bc5")).Set(nameRoom).ToString();

            }
            
        }

        public IList<Element> GetAllPlumbingFixtures()
        {
            // List all plumbing fixtures
            var plumbingFixCollector = new FilteredElementCollector(_doc).OfClass(typeof(FamilyInstance));
            plumbingFixCollector.OfCategory(BuiltInCategory.OST_PlumbingFixtures);
            IList<Element> plFixList = plumbingFixCollector.ToElements();

            return plFixList;
        }
    }
}
