using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;

namespace BIMv2Buttons
{
    public class App : IExternalApplication
    {
        // define a method that will create our tab and button
        static void AddRibbonPanel(UIControlledApplication application)
        {

            String tabName = "BIMv2";    // создание вкладки
            application.CreateRibbonTab(tabName);

            // Add a new ribbon panel
            RibbonPanel ribbonPanel = application.CreateRibbonPanel(tabName, "Надстройки MRGT v1.0"); //подпись под кнопками

            // Get dll assembly path
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            // create push button for CurveTotalLength  //namespace и класс
            PushButtonData b1Data = new PushButtonData("cmdWriteNameRoomInPlumbingFixtures", "Записать" + Environment.NewLine + "в элементы", thisAssemblyPath, "BIMv2.WriteNameRoomInPlumbingFixtures");

            PushButton pb1 = ribbonPanel.AddItem(b1Data) as PushButton;
            pb1.ToolTip = "Transfer number and name to elements";

            BitmapImage pb1Image = new BitmapImage(new Uri("pack://application:,,,/BIMv2Buttons;component/Resources/transferNameRoom.png"));
            pb1.LargeImage = pb1Image;
            
            //попробуем создать еще одну кнопку
            ribbonPanel.AddSeparator(); //разделитель между кнопками
            
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // do nothing
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            // call our method that will load up our toolbar
            AddRibbonPanel(application);
            return Result.Succeeded;
        }
    }
}
