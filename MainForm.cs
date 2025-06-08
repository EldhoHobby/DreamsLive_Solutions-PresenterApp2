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
        public MainForm()
        {
            InitializeComponent();
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
                            picPreview.Image = null;
                        }
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

            // Check if an active presentation form already exists and is not disposed
            if (activePresentationForm != null && !activePresentationForm.IsDisposed)
            {
                // If it exists and is not disposed, update its image
                activePresentationForm.UpdateImage(selectedImagePath);
            }
            else
            {
                // Otherwise, create a new presentation form
                // Consider disabling the start button or main form here to prevent multiple presentations
                // For example: btnStartPresentation.Enabled = false;
                // You might want to add logic to prevent multiple PresentationForms from opening
                // or to manage the state of the btnStartPresentation button.

                activePresentationForm = new PresentationForm(selectedImagePath, targetScreen);
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
    }
}
