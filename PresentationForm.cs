using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public class PresentationForm : Form
{
    private PictureBox pictureBoxOnScreen;

    public PresentationForm(string imagePath, Screen targetScreen)
    {
        InitializeComponent(imagePath, targetScreen);

        // Add event handlers for closing the form
        this.KeyDown += PresentationForm_KeyDown;

        // Optional: Close on click (can be on PictureBox or Form itself)
        // If PictureBox covers the whole form, its click might be more intuitive.
        // Ensure the PictureBox can receive focus/events, or attach to the form's click.
        if (this.pictureBoxOnScreen != null) // Ensure pictureBoxOnScreen is initialized
        {
            this.pictureBoxOnScreen.Click += PresentationForm_Click;
        }
        // Alternatively, to close on any click on the form (even if PictureBox doesn't cover everything or doesn't handle clicks):
        // this.Click += PresentationForm_Click;
    }

    private void InitializeComponent(string imagePath, Screen targetScreen)
    {
        this.pictureBoxOnScreen = new PictureBox();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOnScreen)).BeginInit();
        this.SuspendLayout();

        // Configure PictureBox
        this.pictureBoxOnScreen.Dock = DockStyle.Fill;
        this.pictureBoxOnScreen.SizeMode = PictureBoxSizeMode.Zoom;
        this.pictureBoxOnScreen.BackColor = System.Drawing.Color.Black;
        try
        {
            if (File.Exists(imagePath))
            {
                this.pictureBoxOnScreen.Image = Image.FromFile(imagePath);
            }
            else
            {
                MessageBox.Show("Image file not found: " + imagePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }
        }
        catch (OutOfMemoryException oomEx)
        {
            MessageBox.Show("Error loading image: Out of memory. The image might be too large or corrupted.\n" + oomEx.Message, "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error loading image for presentation: " + ex.Message, "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
            return;
        }

        this.pictureBoxOnScreen.Location = new Point(0, 0);
        this.pictureBoxOnScreen.Name = "pictureBoxOnScreen";
        this.pictureBoxOnScreen.Size = new Size(targetScreen.Bounds.Width, targetScreen.Bounds.Height);
        this.pictureBoxOnScreen.TabIndex = 0;
        this.pictureBoxOnScreen.TabStop = false; // Usually false for a display-only PictureBox

        // Configure Form
        this.BackColor = System.Drawing.Color.Black;
        this.AutoScaleDimensions = new SizeF(6F, 13F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(targetScreen.Bounds.Width, targetScreen.Bounds.Height);
        this.Controls.Add(this.pictureBoxOnScreen);
        this.FormBorderStyle = FormBorderStyle.None;
        this.Name = "PresentationForm";
        this.Text = "Presentation"; // Not visible, but good practice
        this.StartPosition = FormStartPosition.Manual;
        this.Bounds = targetScreen.Bounds;
        this.TopMost = true;
        this.KeyPreview = true; // Important: Allows the form to receive key events before controls on it.

        ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOnScreen)).EndInit();
        this.ResumeLayout(false);

        this.Load += (s, e) => {
            this.WindowState = FormWindowState.Normal; // Important before setting bounds and maximizing
            this.Bounds = targetScreen.Bounds;
            this.WindowState = FormWindowState.Maximized;

            // Ensure the form can receive keyboard input immediately after loading
            this.Activate(); // Brings the form to the foreground and activates it.
            this.Focus();    // Sets input focus to the form.
        };
    }

    // Event handler for KeyDown event on the Form
    private void PresentationForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            this.Close(); // Closes the PresentationForm
        }
    }

    // Optional: Event handler for Click event (on PictureBox) to also close the form
    private void PresentationForm_Click(object sender, EventArgs e)
    {
        this.Close(); // Closes the PresentationForm
    }

    public void UpdateImage(string newImagePath)
    {
        // Dispose of the current image if it's not null
        if (this.pictureBoxOnScreen.Image != null)
        {
            this.pictureBoxOnScreen.Image.Dispose();
            this.pictureBoxOnScreen.Image = null; // Explicitly set to null after disposing
        }

        try
        {
            if (File.Exists(newImagePath))
            {
                this.pictureBoxOnScreen.Image = Image.FromFile(newImagePath);
            }
            else
            {
                MessageBox.Show("Image file not found: " + newImagePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.pictureBoxOnScreen.Image = null; // Clear image on error
            }
        }
        catch (OutOfMemoryException oomEx)
        {
            MessageBox.Show("Error loading image: Out of memory. The image might be too large or corrupted.\n" + oomEx.Message, "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.pictureBoxOnScreen.Image = null; // Clear image on error
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error loading image for presentation: " + ex.Message, "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.pictureBoxOnScreen.Image = null; // Clear image on error
        }

        // Ensure the form is brought to the front and activated
        this.Activate();
    }
}
