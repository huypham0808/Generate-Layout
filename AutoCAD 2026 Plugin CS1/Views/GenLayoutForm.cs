using AutoCAD_2026_Plugin_CS1.Methods;
using AutoCAD_2026_Plugin_CS1.Model;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Application = System.Windows.Forms.Application;


namespace AutoCAD_2026_Plugin_CS1.Views
{
    public partial class GenLayoutForm : Form
    {
        private ListView listView1;
        private ComboBox cmbSampleLayouts; // ComboBox chọn Layout mẫu
        private Label lblSample;
        private Button btnPickData;
        private Button btnExecute;
        private Button btnClearList;
        private List<ViewportItem> capturedItems = new List<ViewportItem>();


        public GenLayoutForm()
        {
            InitializeComponentCustom();
            LoadSampleLayouts();
        }
        private void InitializeComponentCustom()
        {
            this.Text = "Auto Place Viewport to Layout";
            this.Width = 600;
            this.Height = 500; // Tăng chiều cao form để bố cục thoáng
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // 1. Label chọn Layout mẫu
            lblSample = new Label();
            lblSample.Text = "Select layout Template:";
            lblSample.Bounds = new System.Drawing.Rectangle(15, 15, 250, 20);

            // 2. ComboBox chọn Layout mẫu
            cmbSampleLayouts = new ComboBox();
            cmbSampleLayouts.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSampleLayouts.Bounds = new System.Drawing.Rectangle(15, 40, 555, 25);

            // 3. ListView hiển thị danh sách (Đẩy xuống Y = 80 để không bị che)
            listView1 = new ListView();
            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.Bounds = new System.Drawing.Rectangle(15, 80, 555, 270);
            listView1.Columns.Add("Boundary Info", 185, HorizontalAlignment.Left);
            listView1.Columns.Add("Layout Name", 185, HorizontalAlignment.Left);
            listView1.Columns.Add("Scale (DSF)", 165, HorizontalAlignment.Left);


            // 4. Button 1: Pick Data
            btnPickData = new Button();
            btnPickData.Text = "1. Pick Viewport Data";
            btnPickData.Bounds = new System.Drawing.Rectangle(15, 365, 270, 38);
            btnPickData.Click += BtnPickData_Click;

            // 5. Button 2: Execute Placement
            btnExecute = new Button();
            btnExecute.Text = "2. Place to Layouts";
            btnExecute.Bounds = new System.Drawing.Rectangle(300, 365, 270, 38);
            btnExecute.Click += BtnExecute_Click;

            // 6. Button Clear List
            btnClearList = new Button();
            btnClearList.Text = "Clear Selection List";
            btnClearList.Bounds = new System.Drawing.Rectangle(15, 415, 180, 32);
            btnClearList.Click += BtnClearList_Click;

            this.Controls.Add(lblSample);
            this.Controls.Add(cmbSampleLayouts);
            this.Controls.Add(listView1);
            this.Controls.Add(btnPickData);
            this.Controls.Add(btnExecute);
            this.Controls.Add(btnClearList);
        }
        private void BtnClearList_Click(object sender, EventArgs e)
        {
            MethodUtil.UnhighlightEntites(capturedItems); // Unhighlight trước khi xóa list
            capturedItems.Clear();
            listView1.Items.Clear();
        }
        private void LoadSampleLayouts()
        {
            cmbSampleLayouts.Items.Clear();
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                DBDictionary layoutDict = tr.GetObject(doc.Database.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;
                foreach (DBDictionaryEntry entry in layoutDict)
                {
                    // Bỏ qua Model space
                    if (entry.Key.ToUpper() != "MODEL")
                    {
                        cmbSampleLayouts.Items.Add(entry.Key);
                    }
                }
                tr.Commit();
            }

            if (cmbSampleLayouts.Items.Count > 0)
            {
                cmbSampleLayouts.SelectedIndex = 0;
            }
        }
        private void BtnPickData_Click(object sender, EventArgs e)
        {

            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            if (doc == null) return;
            try
            {
                using (DocumentLock loc = doc.LockDocument())
                {
                    if (!db.TileMode)
                    {
                        db.TileMode = true; // Chuyển về Model Space
                        ed.WriteMessage("\nUser is on Layout, the system has automatically switched to Model Space to select objects.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Can not switch to Model Space: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // Ẩn form để thao tác trên AutoCAD
            this.Hide();

            try
            {
                
                LayoutManager layoutMgr = LayoutManager.Current;
                if(layoutMgr.CurrentLayout != "Model") ed.SwitchToModelSpace();
         
                while (true)
                {
                    // Prompt 1: Chọn Yellow Rectangle Boundary
                    PromptEntityOptions optRect = new PromptEntityOptions("Select boundary");
                    optRect.SetRejectMessage("Must select Polyline!");
                    optRect.AddAllowedClass(typeof(Polyline), exactMatch: false);
                    

                    PromptEntityResult resRect = ed.GetEntity(optRect);
                    //MethodUtil.HighlightEntites(resRect); // Highlight đối tượng được chọn
                    if (resRect.Status != PromptStatus.OK) break;
                    MethodUtil.HighlightEntites(resRect);

                    // Prompt 2: Chọn Red MText (Layout Name)
                    PromptEntityOptions optText1 = new PromptEntityOptions("Select sheet number (Red MText)");
                    optText1.SetRejectMessage("Must select MText!");
                    optText1.AddAllowedClass(typeof(MText), exactMatch: false);
                    PromptEntityResult resText1 = ed.GetEntity(optText1);
                    if (resText1.Status != PromptStatus.OK) 
                    {
                        MethodUtil.UnhighlightSpecificEntity(resRect.ObjectId);                
                        break;
                    } 
                    MethodUtil.HighlightEntites(resText1);

                    // Prompt 3: Chọn Pink MText (Scale DSF)
                    PromptEntityOptions optText2 = new PromptEntityOptions("Select scale viewport (Pink MText)");
                    optText2.SetRejectMessage("Must select MText!");
                    optText2.AddAllowedClass(typeof(MText), exactMatch: false);
                    PromptEntityResult resText2 = ed.GetEntity(optText2);
                    if (resText2.Status != PromptStatus.OK)
                    {
                        MethodUtil.UnhighlightSpecificEntity(resRect.ObjectId);
                        MethodUtil.UnhighlightSpecificEntity(resText1.ObjectId);
                        break;
                    }
                    MethodUtil.HighlightEntites(resText2);

                    // Đọc dữ liệu qua Transaction
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        Polyline rect = tr.GetObject(resRect.ObjectId, OpenMode.ForRead) as Polyline;
                        MText txtLayout = tr.GetObject(resText1.ObjectId, OpenMode.ForRead) as MText;
                        MText txtScale = tr.GetObject(resText2.ObjectId, OpenMode.ForRead) as MText;

                        if (rect != null && txtLayout != null && txtScale != null)
                        {
                            // Lấy tọa độ boundary
                            Point3d minPt = rect.GeometricExtents.MinPoint;
                            Point3d maxPt = rect.GeometricExtents.MaxPoint;
                            Point3d centerModel = new Point3d((minPt.X + maxPt.X) / 2.0, (minPt.Y + maxPt.Y) / 2.0, 0);
                            double w = maxPt.X - minPt.X;
                            double h = maxPt.Y - minPt.Y;

                            string layoutName = txtLayout.Contents.Trim();
                            string rawScaleContent = txtScale.Contents.Trim();
                            string cleanedScaleText = Regex.Replace(rawScaleContent, @"\\[A-Za-z0-9]+(\[[^\]]*\])?(\{[^\}]*\})?;", string.Empty);
                            cleanedScaleText = Regex.Replace(cleanedScaleText, @"\\[A-Za-z0-9]+;", string.Empty);
                            cleanedScaleText = cleanedScaleText.Replace("{", "").Replace("}", "").Trim();
                            if (double.TryParse(cleanedScaleText, out double dsf))
                            {
                                ViewportItem newItem = new ViewportItem
                                {
                                    MinPoint = minPt,
                                    MaxPoint = maxPt,
                                    CenterModel = centerModel,
                                    WidthModel = w,
                                    HeightModel = h,
                                    LayoutName = layoutName,
                                    DsfScale = dsf
                                };

                                capturedItems.Add(newItem);
                            }
                            else
                            {
                                MethodUtil.UnhighlightSpecificEntity(resRect.ObjectId);
                                MethodUtil.UnhighlightSpecificEntity(resText1.ObjectId);
                                MethodUtil.UnhighlightSpecificEntity(resText2.ObjectId);
                            }
                        }
                        tr.Commit();
                    }

                    ed.WriteMessage("\n--> Detected 1 viewport successfully. Continue selecting or press ESC to finish.");
                }

                // Refresh ListView on UI thread
                UpdateListView();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error selecting object: " + ex.Message);
            }
            finally
            {
                // Hiển thị lại form
                this.Show();
                this.Activate();
            }
        }
        private void UpdateListView()
        {
            listView1.Items.Clear();
            foreach (var item in capturedItems)
            {
                ListViewItem lvi = new ListViewItem(item.DisplayInfo);
                lvi.SubItems.Add(item.LayoutName);
                lvi.SubItems.Add(item.DsfScale.ToString());
                listView1.Items.Add(lvi);
            }
        }
        private void BtnExecute_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            MethodUtil.UnhighlightEntites(capturedItems); // Unhighlight trước khi thực thi
            if (capturedItems.Count == 0)
            {
                MessageBox.Show("No viewport data has been selected!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Show();
                return;
            }
            string sampleLayoutName = cmbSampleLayouts.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(sampleLayoutName))
            {
                MessageBox.Show("Please select a sample layout (Template) from the list above!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Show();
                return;
            }
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            // Ẩn form khi thực thi tính toán
            this.Hide();

            try
            {
                Database db = doc.Database;
                Editor ed = doc.Editor;
                LayoutManager layMan = LayoutManager.Current;
               
                using (DocumentLock docLock = doc.LockDocument())
                {
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        DBDictionary layoutDict = tr.GetObject(db.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;
                        
                        foreach (var item in capturedItems)
                        {                     
                            if (layoutDict.Contains(item.LayoutName))
                            {
                                ed.WriteMessage($"\nLayout with name: {item.LayoutName} already exists");
                                continue;
                            }
                            layMan.CopyLayout(sampleLayoutName, item.LayoutName);

                            ObjectId newLayoutId = layoutDict.GetAt(item.LayoutName);
                            Layout newLayout = tr.GetObject(newLayoutId, OpenMode.ForWrite) as Layout;

                            int totalLayouts = layoutDict.Count;
                            newLayout.TabOrder = totalLayouts - 1; // Đặt tab order cuối cùng để hiển thị Layout mới ở cuối danh sách

                            BlockTableRecord paperBtr = tr.GetObject(newLayout.BlockTableRecordId, OpenMode.ForWrite) as BlockTableRecord;

                            // 3. DÒ TÌM KHUNG BAO (Green Boundary / Guide Rectangle) TRONG PAPER SPACE
                            Point3d vpCenter = new Point3d(0, 0, 0);
                            double vpWidth = item.WidthModel / item.DsfScale;   // Tính chính xác kích thước khung giấy từ Model / DSF
                            double vpHeight = item.HeightModel / item.DsfScale; // Tính chính xác chiều cao khung giấy từ Model / DSF
                            bool foundGuide = false;
                            ObjectId guideLineId = ObjectId.Null;

                            foreach (ObjectId entId in paperBtr)
                            {
                                Entity? ent = tr.GetObject(entId, OpenMode.ForRead) as Entity;
                                if (ent is Polyline pline)
                                {
                                    // Lấy tâm và kích thước của khung bao chuẩn trong layout mẫu
                                    Point3d minPt = pline.GeometricExtents.MinPoint;
                                    Point3d maxPt = pline.GeometricExtents.MaxPoint;

                                    vpCenter = new Point3d((minPt.X + maxPt.X) / 2.0, (minPt.Y + maxPt.Y) / 2.0, 0);

                                    // Ưu tiên sử dụng kích thước thực tế của khung bao mẫu trên giấy
                                    vpWidth = maxPt.X - minPt.X;
                                    vpHeight = maxPt.Y - minPt.Y;

                                    foundGuide = true;                                    
                                    guideLineId = entId;
                                    //ent.Erase();
                                    break;
                                }
                            }

                            if (guideLineId != ObjectId.Null)
                            {
                                Polyline? guidePline = tr.GetObject(guideLineId, OpenMode.ForWrite) as Polyline;
                                guidePline.Erase();
                            }


                            //---------------- 4. TẠO VÀ CẤU HÌNH VIEWPORT CHUẨN XÁC
                            using (Viewport vp = new Viewport())
                            {
                                vp.SetDatabaseDefaults();

                                // Gán vị trí tâm theo khung bao mẫu và kích thước tính toán
                                vp.CenterPoint = vpCenter;
                                vp.Width = vpWidth;
                                vp.Height = vpHeight;

                                // Append vào Paper Space của layout mới
                                paperBtr.AppendEntity(vp);
                                tr.AddNewlyCreatedDBObject(vp, true);

                                // Cấu hình tâm nhìn Model và tỷ lệ
                                //vp.On = true;
                                vp.ViewCenter = new Point2d(item.CenterModel.X, item.CenterModel.Y);
                                vp.CustomScale = 1.0 / item.DsfScale; // Tỷ lệ quy đổi chuẩn theo DSF (ví dụ 1/128, 1/24)
                                vp.Locked = true; // Khóa viewport chống lệch zoom
                            }
                        }
                        tr.Commit();
                        ed.WriteMessage($"\nSuccessfully placed {capturedItems.Count} Viewport(s) into Layout!");                        
                        LoadSampleLayouts();
                        Cursor.Current = Cursors.Default;
                        MessageBox.Show($"Completed creating {capturedItems.Count} Viewports in Layouts!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        listView1.Items.Clear();
                        ed.Regen();
                    }               
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Runtime error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Show();
                this.Activate();
                
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            MethodUtil.UnhighlightEntites(capturedItems);
            base.OnFormClosing(e);
        }
    }
}
