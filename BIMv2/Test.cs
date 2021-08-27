#region Namespace

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

#endregion

namespace BIMv2
{
    [Transaction(TransactionMode.Manual)]
    public class Test : IExternalCommand

    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)

        {
            var uiApp = commandData.Application;
            var doc = uiApp.ActiveUIDocument.Document;

            var collector = new FilteredElementCollector(doc);

            var elems = collector
                .OfCategory(BuiltInCategory.OST_RvtLinks)
                .OfClass(typeof(RevitLinkType))
                .ToElements();


            foreach (var e in elems)

            {
                var linkType = e as RevitLinkType;

                var s = string.Empty;


                foreach (Document linkedDoc in uiApp.Application.Documents)
                    if (linkedDoc.Title.Equals(linkType.Name))

                    {
                        var collLinked = new FilteredElementCollector(linkedDoc);

                        var linkedWalls =
                            collLinked.OfClass(typeof(Wall)).ToElements();


                        foreach (var eleWall in linkedWalls)

                        {
                            var wall = eleWall as Wall;

                            s = s + "\n" + eleWall.Name;
                        }


                        TaskDialog.Show("Wall Level", linkedDoc.PathName + " : " + s);
                    }
            }

            return Result.Succeeded;
        }
    }
}