using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AutoCAD_2026_Plugin_CS1.Model
{
    public class ViewportItem
    {
        public ObjectId BoundaryId { get; set; }
        public ObjectId LayoutTextId { get; set; }
        public ObjectId ScaleTextId { get; set; }

        public Point3d MinPoint { get; set; }
        public Point3d MaxPoint { get; set; }
        public Point3d CenterModel { get; set; }
        public double WidthModel { get; set; }
        public double HeightModel { get; set; }
        public string? LayoutName { get; set; }
        public double DsfScale { get; set; } // Ví dụ: 128, 24...
        public string DisplayInfo => $"Min:({MinPoint.X:F1}, {MinPoint.Y:F1})";
    }
}
