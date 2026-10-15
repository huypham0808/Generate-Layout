# AutoCAD Auto Viewport & Layout Manager (.NET C# Plugin)

A professional AutoCAD .NET plugin designed to automate and standardize engineering and R&D drafting workflows. This tool accelerates drawing production by automatically creating layouts from templates, parsing text metadata from Model Space, and precisely positioning/scaling viewports.

---

## 🚀 Key Features

* **Interactive Data Selection:** Seamlessly pick Model Space boundaries (`Polyline`), layout name MText, and scale MText with real-time visual highlighting and cleanup.
* **Template-Based Layout Cloning:** Clone existing layout templates (`LayoutManager.CopyLayout`) to preserve company standards, title blocks, plot styles, and page setups.
* **Smart Viewport Positioning & Scaling:** Automatically computes viewport dimensions and center points using paper space guide boundaries and applies precise DSF scales.
* **Automatic Tab Ordering:** Ensures newly created layout tabs maintain strict sequential order (e.g., S100, S200, S300) instead of reversing.
* **Workspace Safety Checks:** Automatically detects if you are in Paper Space and safely switches to Model Space before picking objects.
* **Single-Instance UI Protection:** Modeless Windows Forms UI with built-in instance checks to prevent duplicate windows from opening.

---

## 🛠️ Tech Stack

* **Language:** C#
* **Framework:** .NET / AutoCAD .NET API
* **UI:** Windows Forms (Modeless Dialog)
* **Compatibility:** AutoCAD 2025 / 2026

---

## 📦 Installation & Usage

1. **Build the Plugin:**
   * Open the solution in Visual Studio.
   * Ensure your project references `AcDbMgd.dll`, `AcCoreMgd.dll`, and `AcMgd.dll` from your AutoCAD installation directory.
   * Build the project to generate the `.dll` file.

2. **Load into AutoCAD:**
   * Open AutoCAD.
   * Run the **`NETLOAD`** command.
   * Browse and select your compiled plugin DLL file.

3. **Run the Command:**
   * Type **`GENLAYOUT`** in the AutoCAD command line.
   * The control panel will pop up. Follow the intuitive steps:
     1. Select your template layout from the dropdown.
     2. Click **"1. Pick Viewport Data"** to select your Model Space objects.
     3. Click **"2. Place to Layouts"** to automatically generate layouts and place viewports.

---

## 📝 Workflow Overview

1. **Model Space Selection:** The user picks a boundary rectangle, layout name text (red MText), and scale text (pink MText).
2. **Data Parsing:** Regex cleans any AutoCAD formatting codes from MText to extract the exact scale factor and layout name.
3. **Automated Generation:** The plugin clones the selected template, calculates the correct paper-space viewport dimensions, links the model space coordinates, and locks the viewport scale.

---
