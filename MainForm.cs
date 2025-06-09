using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace DreamsLive_Solutions_PresenterApp1
{
    public partial class MainForm : Form
    {
        private string selectedImagePath = null;
        private PresentationForm activePresentationForm = null;
        private Rectangle selectionRectangle = Rectangle.Empty;
        private Point selectionStartPoint = Point.Empty;
        private bool isSelecting = false;
        private ComboBox cmbDisplayMode;
        public MainForm()
        {
            InitializeComponent();
            // Subscribe to picPreview mouse events
            if (this.picPreview != null) // Ensure picPreview is not null
            {
                this.picPreview.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picPreview_MouseDown);
                this.picPreview.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picPreview_MouseMove);
                this.picPreview.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picPreview_MouseUp);
                this.picPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.picPreview_Paint);
            }

            // Initialize DisplayMode ComboBox
            this.cmbDisplayMode = new ComboBox();
            this.cmbDisplayMode.Name = "cmbDisplayMode";
            this.cmbDisplayMode.DropDownStyle = ComboBoxStyle.DropDownList; // User can't type new values

            // Add items (string representations of the enum values)
            this.cmbDisplayMode.Items.AddRange(new object[] {
                "Fit",
                "Fill",
                "Stretch",
                "Tile",
                "Center"
            });
            this.cmbDisplayMode.SelectedItem = "Fit"; // Default selection

            // Positioning (example: place it below cmbDisplays)
            // This requires cmbDisplays to be accessible via this.cmbDisplays.
            // Ensure cmbDisplays is made accessible if it's designer-generated (e.g. change private to internal or public, or provide a getter).
            // For this example, we'll assume it becomes accessible or use fallback.
            // Note: Accessing other controls like 'this.cmbDisplays' directly might fail if they are private
            // and defined only in Designer.cs without being exposed.
            // A more robust way would be to find the control by name if necessary, or ensure its accessibility.
            Control cmbDisplaysControl = this.Controls.Find("cmbDisplays", true).FirstOrDefault(); // Attempt to find by name

            if (cmbDisplaysControl != null)
            {
                this.cmbDisplayMode.Location = new Point(cmbDisplaysControl.Location.X,
                                                         cmbDisplaysControl.Location.Y + cmbDisplaysControl.Height + 6); // 6px spacing
                this.cmbDisplayMode.Size = cmbDisplaysControl.Size; // Same size as cmbDisplays
            }
            else // Fallback positioning if cmbDisplays isn't found as expected
            {
                // These fallback coordinates might need adjustment based on your form layout.
                // Consider where lblSelectedDisplay (label for cmbDisplays) is to align.
                // Let's try to find lblSelectedDisplay for better relative positioning.
                Control lblSelectedDisplayControl = this.Controls.Find("lblSelectedDisplay", true).FirstOrDefault();
                if (lblSelectedDisplayControl != null)
                {
                     this.cmbDisplayMode.Location = new Point(lblSelectedDisplayControl.Location.X,
                                                             lblSelectedDisplayControl.Location.Y + lblSelectedDisplayControl.Height + 26); // More space after label
                }
                else // Absolute fallback
                {
                    this.cmbDisplayMode.Location = new Point(15, 80); // Adjust as needed if no other refs
                }
                this.cmbDisplayMode.Size = new Size(121, 21);     // Example fallback size, should match cmbDisplays if possible
            }

            // Set TabIndex based on other controls. This is a rough guess.
            // It's better to set this in the designer or more carefully in code.
            if (this.btnStartPresentation != null)
            {
                 this.cmbDisplayMode.TabIndex = this.btnStartPresentation.TabIndex + 1;
            }
            else
            {
                this.cmbDisplayMode.TabIndex = 5; // Arbitrary fallback
            }

            // Add to form's controls
            this.Controls.Add(this.cmbDisplayMode);

            // Subscribe to SelectedIndexChanged event
            this.cmbDisplayMode.SelectedIndexChanged += new System.EventHandler(this.cmbDisplayMode_SelectedIndexChanged);
        }

        private void cmbDisplayMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.activePresentationForm != null && !this.activePresentationForm.IsDisposed)
            {
                string selectedModeString = this.cmbDisplayMode.SelectedItem as string;
                if (selectedModeString != null)
                {
                    try
                    {
                        ImageDisplayMode selectedMode = (ImageDisplayMode)Enum.Parse(typeof(ImageDisplayMode), selectedModeString);
                        this.activePresentationForm.SetDisplayMode(selectedMode);
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine("Error parsing display mode: " + ex.Message);
                    }
                }
            }
        }

        public class DisplayItem
        {
            public string Name { get; set; }
            public Screen DisplayScreen { get; set; }
            public override string ToString() => Name; // Concise way to write the ToString
        }

        private void tnBrowse_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select an Image";
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp|All Files|*.*";
                openFileDialog.FilterIndex = 1; // Default to "Image Files"
                openFileDialog.RestoreDirectory = true; // Remembers the last directory opened

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    this.selectionRectangle = Rectangle.Empty;
                    this.isSelecting = false;
                    if (this.picPreview != null) // Ensure picPreview exists
                    {
                        this.picPreview.Invalidate(); // Clear old selection rectangle from display
                    }

                    try
                    {
                        selectedImagePath = openFileDialog.FileName;
                        lblImagePath.Text = "Selected Image: " + selectedImagePath;

                        // Optional: Display a preview of the image
                        if (picPreview != null) // Check if picPreview exists on the form
                        {
                            // Dispose previous image if any, to free resources
                            if (picPreview.Image != null)
                            {
                                picPreview.Image.Dispose();
                            }
                            picPreview.Image = Image.FromFile(selectedImagePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading image: " + ex.Message, "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        selectedImagePath = null;
                        lblImagePath.Text = "Selected Image: None";
                        if (picPreview != null && picPreview.Image != null)
                        {
                            picPreview.Image.Dispose();
                            picPreview.Image = null; // This will trigger a repaint, clearing everything
                        }
                        // Ensure selection is also cleared if image load fails
                        this.selectionRectangle = Rectangle.Empty;
                        // Invalidate again if picPreview still exists, to be certain selection visual is gone
                        if (this.picPreview != null) this.picPreview.Invalidate();
                    }
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

            PopulateDisplayComboBox();
        }
        private void PopulateDisplayComboBox()
        {
            cmbDisplays.Items.Clear();
            Screen[] allScreens = Screen.AllScreens;

            if (allScreens.Length == 0)
            {
                cmbDisplays.Items.Add("No displays found");
                cmbDisplays.Enabled = false;
                btnStartPresentation.Enabled = false; // Disable start if no displays
                return;
            }

            for (int i = 0; i < allScreens.Length; i++)
            {
                Screen screen = allScreens[i];
                string displayName = $"Display {i + 1}: {screen.Bounds.Width}x{screen.Bounds.Height}";
                if (screen.Primary)
                {
                    displayName += " (Primary)";
                }

                // Add a custom object to the ComboBox
                cmbDisplays.Items.Add(new DisplayItem { Name = displayName, DisplayScreen = screen });
            }

            // Select a default display
            if (cmbDisplays.Items.Count > 0)
            {
                // Try to select the first non-primary screen if more than one screen exists
                int defaultIndex = 0;
                if (allScreens.Length > 1)
                {
                    for (int i = 0; i < allScreens.Length; i++)
                    {
                        if (!allScreens[i].Primary)
                        {
                            defaultIndex = i;
                            break;
                        }
                    }
                }
                cmbDisplays.SelectedIndex = defaultIndex;
            }
            else // Should be caught by allScreens.Length == 0, but as a safeguard
            {
                cmbDisplays.Items.Add("No displays configured");
                cmbDisplays.Enabled = false;
                btnStartPresentation.Enabled = false;
            }
        }

        private void btnStartPresentation_Click(object sender, EventArgs e)
        {

            // selectedImagePath is assumed to be a form-level field in MainForm.
            // Example: private string selectedImagePath;

            // cmbDisplays is assumed to be a ComboBox on your MainForm.
            // DisplayItem class (from previous step) is assumed to be available.
            // Example: public class DisplayItem { public string Name { get; set; } public Screen DisplayScreen { get; set; } ... }


            if (string.IsNullOrEmpty(selectedImagePath))
            {
                MessageBox.Show("Please select an image first.", "No Image Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbDisplays.SelectedItem == null)
            {
                MessageBox.Show("Please select a display.", "No Display Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DisplayItem selectedDisplayItem = cmbDisplays.SelectedItem as DisplayItem;
            if (selectedDisplayItem == null)
            {
                MessageBox.Show("Invalid display selection. Ensure DisplayItem is correctly populated in ComboBox.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Screen targetScreen = selectedDisplayItem.DisplayScreen;

            RectangleF? selectedRegionInImageCoords = GetSelectedRegionInImageCoordinates();

            // Check if an active presentation form already exists and is not disposed
            if (activePresentationForm != null && !activePresentationForm.IsDisposed)
            {
                // If it exists and is not disposed, update its image
                activePresentationForm.UpdateImage(selectedImagePath, selectedRegionInImageCoords);

                // Ensure the latest display mode from ComboBox is applied
                string currentModeString = this.cmbDisplayMode.SelectedItem as string;
                if (currentModeString != null)
                {
                    try
                    {
                        ImageDisplayMode currentMode = (ImageDisplayMode)Enum.Parse(typeof(ImageDisplayMode), currentModeString);
                        activePresentationForm.SetDisplayMode(currentMode);
                    }
                    catch (ArgumentException ex) { Console.WriteLine("Error parsing current display mode: " + ex.Message); }
                }

                if (activePresentationForm.WindowState == FormWindowState.Minimized)
                {
                    activePresentationForm.WindowState = FormWindowState.Normal;
                }
                activePresentationForm.Activate(); // Bring to front
            }
            else
            {
                // Otherwise, create a new presentation form
                activePresentationForm = new PresentationForm(selectedImagePath, targetScreen, selectedRegionInImageCoords);
                activePresentationForm.FormClosed += (s, args) => {
                    if (s == activePresentationForm)
                    {
                        activePresentationForm = null;
                    }
                };

                // Set initial display mode before showing
                string initialModeString = this.cmbDisplayMode.SelectedItem as string;
                if (initialModeString != null)
                {
                    try
                    {
                        ImageDisplayMode initialMode = (ImageDisplayMode)Enum.Parse(typeof(ImageDisplayMode), initialModeString);
                        activePresentationForm.SetDisplayMode(initialMode);
                    }
                    catch (ArgumentException ex) { Console.WriteLine("Error parsing initial display mode: " + ex.Message); }
                }
                activePresentationForm.Show();
            }
        }

        // --- Create a new class file for PresentationForm (e.g., PresentationForm.cs) ---
        // Or add this class within the same file as MainForm, outside the MainForm class.

        // --- Create a new class file for PresentationForm (e.g., PresentationForm.cs) ---
        // Or add this class within the same file as MainForm, outside the MainForm class.

        // Required using statements for PresentationForm:
        // using System.Windows.Forms;
        // using System.Drawing;
        // using System.IO;
    }
}
