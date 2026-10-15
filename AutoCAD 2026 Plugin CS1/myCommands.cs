using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using AutoCAD_2026_Plugin_CS1.Views;

[assembly: CommandClass(typeof(AutoCAD_2026_Plugin_CS1.MyCommands))]
[assembly: ExtensionApplication(typeof(AutoCAD_2026_Plugin_CS1.PluginExtension))]

namespace AutoCAD_2026_Plugin_CS1
{
    public class MyCommands
    {
        [CommandMethod("GENLAYOUT", CommandFlags.Modal)]
        public void CallFormGenLayout() // This method can have any name
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            GenLayoutForm _myForm = new GenLayoutForm();
            if (doc != null)
            {
                AcadApp.ShowModelessDialog(_myForm);
            }
        }
    }
}
