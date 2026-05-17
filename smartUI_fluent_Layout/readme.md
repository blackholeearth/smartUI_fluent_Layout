Here is the professional English version of the **SmartUI** README. It’s written for developers who appreciate efficiency and clever design over manual labor.

---

# 🚀 SmartUI (The Wise Developer's Layout Engine)

**A Fluent API-based layout engine for WinForms that eliminates the 20-year-old struggle of manual positioning. "What you code is what you get."**

While others waste hours fighting with XAML `Grid.Row` and `ColumnSpan`, you can build pixel-perfect, jilet-sharp interfaces with a single line of C# code.

---

### 🎨 Visual Mapping
In SmartUI, the structure of your code directly reflects the physical layout of your UI. Each code line represents a visual row.

```csharp
// Row 1: Header (Navy background, padded)
SmartUI.Row(lblTitle).Background(Color.Navy).Padding(10);

// Row 2: Label and a horizontally expanding TextBox
SmartUI.Row(lblUsername, txtUsername.GrowW()).Margin(0, 5, 0, 15);

// Row 3: A DataGrid that consumes all remaining vertical space
SmartUI.Row(dgvData.GrowW().GrowH());

// Row 4: Buttons aligned to the right of the TextBox with matched widths
SmartUI.Row(btnSave.AlignRight(txtUsername), btnCancel.MatchWidth(btnSave)).Padding(10);
```

### 🧐 Visual Output: How it looks
Think of your code as a blueprint. Here is how the engine interprets it:

| UI Layout | Technical Logic |
| :---      | :---            |
| `[ Title Label ]`                                 | Styled Header Row via `.Background()` |
| `[ Label ] [ ------ Input Box (Expanding) ---- ]` | Space-filling row via `.GrowW()` |
| `[ =========================================== ]` |
| `[                 DataGrid (TABLE)            ]` | Consumes vertical space via `.GrowH()` |
| `[              Fills Entire Bottom Area       ]` |
| `[ =========================================== ]` |
| `                      [ Cancel ] [ Save ]`       | Snapped to Input Box via `.AlignRight()` |

---

### 🔥 Why SmartUI?

1.  **DPI-Aware:** No more hardcoded coordinates like `120, 45`. Relationships are calculated at runtime. Your UI won't break on 4K monitors.
2.  **Legacy Friendly:** Pure C# and .NET 4.7.2+ architecture. Works on Windows 7 & 10. No external DLLs required.
3.  **No More "Amele" Work:** The `Row()` function automatically handles vertical offsets. You focus on *what* goes where, not *where* it goes.
4.  **Rapid Prototyping:** Use the WinForms Designer as a "Bucket." Drop your controls anywhere, name them, and let SmartUI organize them perfectly at runtime.

---

### 🛠️ Method Reference

#### Control Extensions (Alignment Rules)
*   `.GrowW()`: Expands the control to fill the remaining horizontal width of the row.
*   `.GrowH()`: Expands the control vertically to fill the remaining bottom space of the form.
*   `.AlignRight(target)`: Snaps the control's right edge to the target control's right edge.
*   `.AlignLeft(target)`: Snaps the control's left edge to the target control's left edge.
*   `.MatchWidth(target)`: Sets the control's width to match the target's width exactly.

#### RowResult Methods (Row Styling)
*   `.Background(Color)`: Sets the background color for the entire row.
*   `.Border()`: Adds a visual border to the row container.
*   `.Padding(L, T, R, B)`: Internal spacing (distance from row edges to controls).
*   `.Margin(L, T, R, B)`: External spacing (distance between rows).
*   `.Visible(bool)`: Toggles row visibility; hidden rows collapse, and lower rows automatically shift up.

---

### 💡 Pro-Tip Workflow
Treat the Visual Studio Designer as a **"Control Bin."** Don't waste a single second dragging controls to the "right spot." Just toss them onto the form, give them names (e.g., `btnSave`), and define the layout in your constructor.

**"The best code is the code you don't have to write; the best alignment is the one that happens automatically."**

---

This README is now ready for your GitHub repo or project folder. It perfectly captures the "SmartUI" philosophy! Enjoy your well-deserved rest, usta! 🚀😴