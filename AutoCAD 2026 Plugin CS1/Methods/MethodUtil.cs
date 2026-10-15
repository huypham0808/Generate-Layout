using AutoCAD_2026_Plugin_CS1.Model;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AutoCAD_2026_Plugin_CS1.Methods
{
    public class MethodUtil
    {
        public static void HighlightEntites(PromptEntityResult per)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            using (Transaction trH = db.TransactionManager.StartTransaction())
            {
                Entity ent = trH.GetObject(per.ObjectId, OpenMode.ForRead) as Entity;
                ent?.Highlight();
                trH.Commit();
            }
        }
        public static void UnhighlightEntites(List<ViewportItem> capturedItems)
        {
            if (capturedItems.Count == 0) return;

            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            using (DocumentLock loc = doc.LockDocument())
            {
                using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
                {
                    foreach (var item in capturedItems)
                    {
                        if (!item.BoundaryId.IsNull && !item.BoundaryId.IsErased)
                        {
                            Entity ent = tr.GetObject(item.BoundaryId, OpenMode.ForRead) as Entity;
                            ent?.Unhighlight();
                        }
                        if (!item.LayoutTextId.IsNull && !item.LayoutTextId.IsErased)
                        {
                            Entity ent = tr.GetObject(item.LayoutTextId, OpenMode.ForRead) as Entity;
                            ent?.Unhighlight();
                        }
                        if (!item.ScaleTextId.IsNull && !item.ScaleTextId.IsErased)
                        {
                            Entity ent = tr.GetObject(item.ScaleTextId, OpenMode.ForRead) as Entity;
                            ent?.Unhighlight();
                        }
                    }
                    tr.Commit();
                }
            }
        }
        public static void UnhighlightSpecificEntity(ObjectId id)
        {
            if (id.IsNull || id.IsErased) return;
            using (Transaction trH = id.Database.TransactionManager.StartTransaction())
            {
                (trH.GetObject(id, OpenMode.ForRead) as Entity)?.Unhighlight();
                trH.Commit();
            }
        }
    }
}
