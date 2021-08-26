using System;
using System.Collections.Generic;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace BIMv2
{
    [Transaction(TransactionMode.Manual)]
    internal class Test : IExternalCommand
    {
        // Member variables
        private Application _app;
        private Document _doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var uiDoc = uiApp.ActiveUIDocument;

            _app = uiApp.Application;
            _doc = uiDoc.Document;

            ProccessStart(uiApp);

            return Result.Succeeded;
        }

        private void ProccessStart(UIApplication m_app)

        {
            try
            {
                // Make sure no other RVTs are open in Revit

                if (MultipleRVTsOpen(m_app))
                {
                    TaskDialog.Show("Process Stopped", "Please only have one file open when running this tool");

                    return;
                }

                // Iterate through each document

                foreach (Document oDocument in m_app.Application.Documents)

                    // Only process links

                    if (oDocument.IsLinked)

                    {
                        // Create a room collection from rooms in the current link

                        var oRoomFilter = new RoomFilter();

                        var oFilteredElemCol = new FilteredElementCollector(oDocument);

                        oFilteredElemCol.WherePasses(oRoomFilter).WhereElementIsNotElementType();

                        // Iterate through each room

                        foreach (Room oRoom in oFilteredElemCol)

                        {
                            // Only process placed rooms

                            if (RoomIsPlaced(oRoom) == false)

                                continue;

                            // Get all LightingFixtures in the current room

                            var oFamilyInstances = LightingFixtsFromRoom(oRoom, m_app.ActiveUIDocument.Document);
                        }
                    }
            }

            catch (Exception ex)

            {
                TaskDialog.Show("Error", ex.Message);
            }
        }

        private bool MultipleRVTsOpen(UIApplication m_app)

        {
            var intCntr = 0;

            try

            {
                // Iterate through each document

                foreach (Document oDocument in m_app.Application.Documents)

                    // Skip linked RVTs and families

                    if (!oDocument.IsLinked && !oDocument.IsFamilyDocument)

                    {
                        intCntr++;

                        if (intCntr > 1)

                            return true;
                    }

                return false;
            }

            catch

            {
                return true;
            }
        }

        private bool RoomIsPlaced(Room oRoom)

        {
            try

            {
                // Make sure the room does not contain a location or area value

                if (null != oRoom.Location && Math.Round(oRoom.Area) != 0.0)

                    return true;

                return false;
            }

            catch

            {
                return false;
            }
        }

        private List<FamilyInstance> LightingFixtsFromRoom(Room oRoom, Document oDocument)

        {
            var oFamilyInstances = new List<FamilyInstance>();

            try

            {
                // Create a LightingFixture/FamilyIntance collection that exist in the active document

                var oFilteredElemCol = new FilteredElementCollector(oDocument)
                    .WhereElementIsNotElementType()
                    .WhereElementIsViewIndependent()
                    .OfCategory(BuiltInCategory.OST_LightingFixtures)
                    .OfClass(typeof(FamilyInstance));

                // Iterate through each instance

                foreach (FamilyInstance oFamilyInstance in oFilteredElemCol)

                    try

                    {
                        // Get the boudning box of the instance

                        var oBoundingBoxXYZ = oFamilyInstance.get_BoundingBox(null);

                        if (oBoundingBoxXYZ == null)

                            continue;

                        // Get the center point of the instance (except Z, want to make sure recessed lights are found)

                        var oCenterPoint = new XYZ((oBoundingBoxXYZ.Min.X + oBoundingBoxXYZ.Max.X) / 2,
                            (oBoundingBoxXYZ.Min.Y + oBoundingBoxXYZ.Max.Y) / 2,
                            oBoundingBoxXYZ.Min.Z);

                        // Determine if the point exists within the bounding box/room

                        if (oRoom.IsPointInRoom(oCenterPoint))

                            TaskDialog.Show("Got lights", "Light family name: " +
                                                          oFamilyInstance.Symbol.FamilyName + ", Room name: " +
                                                          oRoom.Name);
                    }

                    catch

                    {
                        // continue processing
                    }

                return oFamilyInstances;
            }

            catch

            {
                return oFamilyInstances = new List<FamilyInstance>();
            }
        }
    }
}