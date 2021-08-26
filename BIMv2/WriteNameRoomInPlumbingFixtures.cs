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
            IList<Element> plFixList=GetAllPlumbingFixtures();

            TaskDialog.Show("My Dialog Title", "Hello World!");

        }

        public IList<Element> GetAllPlumbingFixtures()
        {
            // List all plumbing fixtures
            var plumbingFixCollector = new FilteredElementCollector(_doc).OfClass(typeof(FamilyInstance));
            plumbingFixCollector.OfCategory(BuiltInCategory.OST_PlumbingFixtures);
            IList<Element> plFixList = plumbingFixCollector.ToElements();

            return plFixList;

            //ShowElementList(plFixList, "Plumbing Fixtured: ");
        }

       /* public void ShowElementList(IList<Element> elems, string header)
        {
            string s = " - Class - Category - Name (or Family: Type Name) - Id - \r\n";
            foreach (Element e in elems)
            {
                s += ElementToString(e);
            }
            TaskDialog.Show(header + "(" + elems.Count.ToString() + "):", s);
        }
        public string ElementToString(Element e)
        {
            if (e == null)
            {
                return "none";
            }

            string name = "";

            if (e is ElementType)
            {
                Parameter param = e.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM);
                if (param != null)
                {
                    name = param.AsString();
                }
            }
            else
            {
                name = e.Name;
            }
            return e.GetType().Name + "; "
                                    + e.Category.Name + "; "
                                    + name + "; "
                                    + e.Id.IntegerValue.ToString() + "\r\n";
        }
   */ }
}
