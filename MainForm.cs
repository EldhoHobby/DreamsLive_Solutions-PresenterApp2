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
                if (activePresentationForm.WindowState == FormWindowState.Minimized)
                {
                    activePresentationForm.WindowState = FormWindowState.Normal;
                }
                activePresentationForm.Activate(); // Bring to front
            }
            else
            {
                // Otherwise, create a new presentation form
                // Consider disabling the start button or main form here to prevent multiple presentations
                // For example: btnStartPresentation.Enabled = false;
                // You might want to add logic to prevent multiple PresentationForms from opening
                // or to manage the state of the btnStartPresentation button.

                activePresentationForm = new PresentationForm(selectedImagePath, targetScreen, selectedRegionInImageCoords);
                activePresentationForm.FormClosed += (s, args) => {
                    // Re-enable start button or main form when presentation is closed
                    // For example: btnStartPresentation.Enabled = true;
                    // Consider re-enabling based on application logic
                    if (s == activePresentationForm) // Check if the closed form is the active one
                    {
                        activePresentationForm = null;
                    }
                };
                activePresentationForm.Show(); // Show non-modally
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

        private void picPreview_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && this.picPreview.Image != null)
            {
                this.isSelecting = true;
                this.selectionStartPoint = e.Location; // e.Location is relative to picPreview
                this.selectionRectangle = Rectangle.Empty; // Clear previous selection
                this.picPreview.Invalidate(); // Request repaint to clear old rectangle visual
            }
        }

        private void picPreview_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isSelecting && this.picPreview.Image != null)
            {
                Point currentMousePosition = e.Location;
                int x = Math.Min(this.selectionStartPoint.X, currentMousePosition.X);
                int y = Math.Min(this.selectionStartPoint.Y, currentMousePosition.Y);
                int width = Math.Abs(this.selectionStartPoint.X - currentMousePosition.X);
                int height = Math.Abs(this.selectionStartPoint.Y - currentMousePosition.Y);
                this.selectionRectangle = new Rectangle(x, y, width, height);
                this.picPreview.Invalidate(); // Request repaint to draw updated rectangle
            }
        }

        private void picPreview_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.isSelecting && this.picPreview.Image != null)
            {
                this.isSelecting = false;
                // Optional: Finalize selectionRectangle based on e.Location (already done by MouseMove)
                // Optional: Check for minimal size
                if (this.selectionRectangle.Width < 5 || this.selectionRectangle.Height < 5)
                {
                    this.selectionRectangle = Rectangle.Empty; // Discard very small selections
                }
                this.picPreview.Invalidate(); // Request repaint for final state
            }
        }

        private void picPreview_Paint(object sender, PaintEventArgs e)
        {
            // The PictureBox already handles drawing its Image if assigned.
            // We just need to draw our selection rectangle on top of it.
            // However, if picPreview.Image is null, we shouldn't attempt to draw a selection.

            if (this.picPreview.Image == null)
            {
                // Optional: Clear selection if image is gone
                // this.selectionRectangle = Rectangle.Empty;
                return;
            }

            // Draw the selection rectangle if it's not empty.
            if (this.selectionRectangle != Rectangle.Empty)
            {
                // Use a semi-transparent brush for better visibility (optional)
                // using (Brush selectionBrush = new SolidBrush(Color.FromArgb(128, 72, 145, 220))) // Example: semi-transparent blue
                // {
                // e.Graphics.FillRectangle(selectionBrush, this.selectionRectangle);
                // }

                // Draw a border for the rectangle
                // Ensure Pen is disposed if created like this, or use a static Pen.
                using (Pen selectionPen = new Pen(Color.Red, 2)) // Red border, 2px thick
                {
                    e.Graphics.DrawRectangle(selectionPen, this.selectionRectangle);
                }
            }
        }

        private RectangleF? GetSelectedRegionInImageCoordinates()
        {
            if (this.picPreview.Image == null || this.selectionRectangle == Rectangle.Empty || this.picPreview.ClientSize.Width == 0 || this.picPreview.ClientSize.Height == 0)
            {
                return null;
            }

            Image img = this.picPreview.Image;
            Rectangle clientRect = this.picPreview.ClientRectangle; // Using ClientRectangle

            float imgAspectRatio = (float)img.Width / img.Height;
            float picBoxAspectRatio = (float)clientRect.Width / clientRect.Height;

            float scaleFactor;
            if (imgAspectRatio > picBoxAspectRatio)
            {
                scaleFactor = (float)clientRect.Width / img.Width;
            }
            else
            {
                scaleFactor = (float)clientRect.Height / img.Height;
            }

            if (scaleFactor <= 0) return null;

            float displayedImageWidth = img.Width * scaleFactor;
            float displayedImageHeight = img.Height * scaleFactor;

            float offsetX = (clientRect.Width - displayedImageWidth) / 2.0f;
            float offsetY = (clientRect.Height - displayedImageHeight) / 2.0f;

            float selRelX = this.selectionRectangle.X - offsetX;
            float selRelY = this.selectionRectangle.Y - offsetY;

            float originalX = selRelX / scaleFactor;
            float originalY = selRelY / scaleFactor;
            float originalWidth = this.selectionRectangle.Width / scaleFactor;
            float originalHeight = this.selectionRectangle.Height / scaleFactor;

            // Boundary checks
            if (originalX < 0) { originalWidth += originalX; originalX = 0; }
            if (originalX >= img.Width) { originalX = img.Width - 1; originalWidth = 0; }

            if (originalY < 0) { originalHeight += originalY; originalY = 0; }
            if (originalY >= img.Height) { originalY = img.Height - 1; originalHeight = 0; }

            if (originalX + originalWidth > img.Width) { originalWidth = img.Width - originalX; }
            if (originalY + originalHeight > img.Height) { originalHeight = img.Height - originalY; }

            if (originalWidth <= 0 || originalHeight <= 0)
            {
                return null;
            }

            return new RectangleF(originalX, originalY, originalWidth, originalHeight);
        }
    }
}
