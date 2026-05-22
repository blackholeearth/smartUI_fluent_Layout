# 🚀 SmartUI for WinForms

**A Declarative, Responsive, and Fluent Layout Engine that makes WinForms (.NET 4.7.2+) feel like Flutter, SwiftUI, or React.**

SmartUI eliminates the 20-year-old struggles of `TableLayoutPanel`, broken anchors, ghost margins, and flickering screens. It empowers you to build modern, responsive, DPI-aware, and pixel-perfect interfaces using a clean **Fluent API** entirely in C#.

> **"What you code is exactly what you see on the screen. No Designer needed."**

![Windows 11 Settings Built with SmartUI](ss1.png)

---

## 🤯 The Magic: Building Windows 11 Settings in C#

With SmartUI's **Component-Driven Architecture**, you can encapsulate complex layouts into reusable methods (Widgets). Look how clean and readable the main UI code becomes:

```csharp
private void BuildUI__W11ModernSettings()
{
    var ui = new SmartUI(this);

    // 1. LEFT SIDEBAR (Responsive & Collapsible)
    ui.SetupResponsiveSidebar(850); // Automatically handles the hamburger flyout!

    ui.SidePanel(Side.Left, 280,
        ui.Group( 
            imgProfile, 
            ui.Col(lblUser, lblEmail).VAlignMiddle()
        ).VAlignMiddle().Padding(20),

        txtSearch.GrowW(),
        ui.Space(10),
        
        ui.SidebarItem_v1(SegoeMDL2Icons.Home, "Home"),
        ui.SidebarItem_v1(SegoeMDL2Icons.System, "System", isSelected: true).BackColor(winCard),
        ui.SidebarItem_v1(SegoeMDL2Icons.Bluetooth, "Bluetooth & devices"),
        ui.SidebarItem_v1(SegoeMDL2Icons.Globe, "Network & internet")
    ).BackColor(winSidebar).Padding(16, 0, 0, 0);

    // 2. RIGHT CONTENT
    ui.Row(lblPageTitle).Margin(20, 0, 0, 10);

    // SECTION 1: Brightness & Color
    ui.SectionHeader_v1("Display & Brightness");
    ui.CardView_v1(icoBrightness, lblBrightnessTitle, lblBrightnessDesc, trackBrightness);
    ui.CardView_v1(icoNight, lblNightTitle, lblNightDesc, btnToggleNight);
    ui.CardView_v1(icoHDR, lblHDRTitle, lblHDRDesc, icoArrowHDR);

    // SECTION 2: Scale & Layout
    ui.SectionHeader_v1("Scale & layout");
    ui.CardView_v1(SegoeMDL2Icons.ResizeMouseMedium, "Scale", "Change the size of text, apps, and other items", cmbScale);

    ui.Divider_v1(); 
    ui.AlertBox_v1(SegoeMDL2Icons.Info, "Windows Update is paused.", Color.LightBlue, Color.DarkBlue);
}
```
*⏱️ **Blazing Fast:** The entire UI tree above builds, measures, and renders in just a few milliseconds!*

---

## 🔥 Key Features

* **Zero-Base Architecture:** No hidden default margins or paddings. If you don't explicitly set a gap using `.Spacing()`, `.Margin()`, or `.Padding()`, the distance is precisely `0px`. You have 100% pixel control.
* **CSS Flexbox in C#:** Use `.GrowW()`, `.GrowH()`, and `.Spring()` to distribute space dynamically and push elements exactly where they belong.
* **Double-Pass Reflow (True Word Wrap):** When the window resizes, text automatically wraps (`.WrapText()`), containers expand, and elements below are pushed down perfectly without overflowing.
* **Zero-Flicker Rendering:** Uses the Win32 `WM_SETREDRAW` API to completely freeze the UI during calculations. Result? Buttery-smooth resizing with zero inter-frame jumping.
* **Responsive Sidebars:** Built-in support for auto-collapsing side panels and a hamburger flyout menu for mobile-like UX `< 850px` width.
* **Rounded Corners & Flat Design:** Apply anti-aliased border radius and custom borders to any control instantly using `.Rounded()`.
* **Component-Driven (Reusable Widgets):** Extend the engine using `partial class SmartUI` to create your own reusable widgets (like CardViews, AlertBoxes, and SidebarItems).

---

## 🎨 Reusable Components (Widgets)

By extending the `SmartUI` class, you can create massive time-savers like `CardView_v1`. Inside these components, you leverage the layout engine:

```csharp
public RowResult CardView_v1(Label lbl_icon, Label lbl_title, Label lbl_desc, Control Control_atRightSide)
{
    return this.Row(
        this.Group(
            lbl_icon.Padding(0, 0, 10, 0).VAlignMiddle().BackColor(Color.Transparent),
            this.Col(
                lbl_title,
                lbl_desc.WrapText()
            ).GrowW().Padding(0)
        ).VAlignMiddle().Padding(0).GrowW(),
        
        this.Space(12),
        Control_atRightSide.VAlignMiddle()
    )
    .BackColor(Color.White)
    .Padding(18)
    .Margin(30, 0, 30, 4)
    .Rounded(8, Color.FromArgb(229, 229, 229));
}
```

---

## 🔤 Using Icons (Segoe MDL2 Assets)
For a native Windows 11/10 feel, the engine pairs perfectly with the `Segoe MDL2 Assets` or `Segoe Fluent Icons` fonts. Using a helper class (`SegoeMDL2Icons`) removes magic strings from your code:

```csharp
// Example using predefined PUA icon codes:
ui.CardView_v1(SegoeMDL2Icons.Home, "Title", "Description", myControl);
```

---

## 📚 API Reference Cheat Sheet

### 📦 Containers & Spacers
* `ui.Row(...)` : Creates a full-width horizontal row.
* `ui.Group(...)` : Creates a horizontal container that wraps tightly around its children.
* `ui.Col(...)` : Creates a vertical container (column) for stacking items.
* `ui.SidePanel(...)` : Reserves a region (Left, Right, Bottom). Main content dynamically flows into the remaining space.
* `ui.Space(int)` : Adds a transparent, scalable empty block inside a Group/Col/SidePanel.

### 📐 Flexbox Rules
* `.GrowW()` : Expands the control horizontally to share available space.
* `.GrowH()` : Expands the control vertically to fill the remaining bottom space of the form.
* `.Spring()` : An invisible spring that consumes all empty space, pushing elements apart.
* `.MatchWidth(target)` : Forces the control to be exactly as wide as the target control.

### 📏 Alignment & Layout
* `.AlignRight(target)` : Snaps the control to the right edge of the target.
* `.VAlignMiddle()` : Vertically centers the control inside its container.
* `.VAlignBottom()` : Aligns the control to the bottom of its container.
* `.WrapText()` : (For Labels) Allows text to break into multiple lines on resize.

### 🖌️ Styling & Spacing (L, T, R, B)
* `.Spacing(int)` : Sets the gap between children *inside* a `Group` or `Col`.
* `.Margin(int)` or `.Margin(L, T, R, B)` : External margins. 
* `.Padding(int)` or `.Padding(L, T, R, B)` : Internal padding.
* `.BackColor(Color)` : Sets the background color cleanly.
* `.Rounded(radius, color)` : Applies anti-aliased rounded corners.

---

*Architected with logic, performance, and 🍌 by developers who refused to let WinForms die.*