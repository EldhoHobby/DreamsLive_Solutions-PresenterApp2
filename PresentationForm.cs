using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public class DoubleBufferedPanel : System.Windows.Forms.Panel
{
    public DoubleBufferedPanel()
    {
        this.DoubleBuffered = true;
        this.SetStyle(System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer |
                      System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
                      System.Windows.Forms.ControlStyles.UserPaint, true);
    }
}

public class PresentationForm : Form
{
    private DoubleBufferedPanel displayPanel;
    private Image currentImage = null;
    private float currentZoom = 1.0f;
    private PointF currentPan = PointF.Empty;
    private TrackBar zoomSlider;
    private bool isPanning = false;
    private Point lastMousePosition = Point.Empty;
    private RectangleF? initialSourceRegion = null;

    public PresentationForm(string imagePath, Screen targetScreen, RectangleF? initialRegion = null)
    {
        this.initialSourceRegion = initialRegion;
        InitializeComponent(imagePath, targetScreen);

        // Add event handlers for closing the form
        this.KeyDown += PresentationForm_KeyDown;

        // Optional: Close on click (can be on PictureBox or Form itself)
        // If PictureBox covers the whole form, its click might be more intuitive.
        // Ensure the displayPanel can receive focus/events, or attach to the form's click.
        if (this.displayPanel != null) // Ensure displayPanel is initialized
        {
            this.displayPanel.Click += PresentationForm_Click;
        }
        // Alternatively, to close on any click on the form (even if displayPanel doesn't cover everything or doesn't handle clicks):
        // this.Click += PresentationForm_Click;
    }

    private void InitializeComponent(string imagePath, Screen targetScreen)
    {
        this.displayPanel = new DoubleBufferedPanel();
        this.zoomSlider = new TrackBar();
        this.SuspendLayout();

        // Configure Panel
        this.displayPanel.Dock = DockStyle.Fill;
        this.displayPanel.BackColor = Color.Black;
        this.displayPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.displayPanel_Paint);
        this.displayPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.displayPanel_MouseDown);
        this.displayPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.displayPanel_MouseMove);
        this.displayPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.displayPanel_MouseUp);

        // Load initial image
        try
        {
            if (File.Exists(imagePath))
            {
                this.currentImage = Image.FromFile(imagePath);
            }
            else
            {
                MessageBox.Show("Image file not found: " + imagePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.currentImage = null; // Explicitly set to null on error before Close() might be called
                this.Close();
                return;
            }
        }
        catch (OutOfMemoryException oomEx)
        {
            MessageBox.Show("Error loading image: Out of memory. The image might be too large or corrupted.\n" + oomEx.Message, "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.currentImage = null;
            this.Close();
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error loading image for presentation: " + ex.Message, "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.currentImage = null;
            this.Close();
            return;
        }

        // Configure Zoom Slider
        this.zoomSlider.Dock = DockStyle.Bottom;
        this.zoomSlider.Minimum = 10; // 10%
        this.zoomSlider.Maximum = 500; // 500%
        this.zoomSlider.Value = 100; // 100%
        this.zoomSlider.TickFrequency = 10;
        this.zoomSlider.Scroll += new System.EventHandler(this.zoomSlider_Scroll);

        // Configure Form
        this.BackColor = System.Drawing.Color.Black;
        // this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true); // Removed, panel handles its own
        this.AutoScaleDimensions = new SizeF(6F, 13F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(targetScreen.Bounds.Width, targetScreen.Bounds.Height);
        this.Controls.Add(this.displayPanel); // Add panel first so it's behind slider
        this.Controls.Add(this.zoomSlider);
        this.FormBorderStyle = FormBorderStyle.None;
        this.Name = "PresentationForm";
        this.Text = "Presentation"; // Not visible, but good practice
        this.StartPosition = FormStartPosition.Manual;
        this.Bounds = targetScreen.Bounds;
        this.TopMost = true;
        this.KeyPreview = true; // Important: Allows the form to receive key events before controls on it.

        this.ResumeLayout(false);

        // Constructor no longer calls SetupInitialView directly.
        // It will be called by the Load event.

        this.Load += (s, e) => {
            this.WindowState = FormWindowState.Normal; // Important before setting bounds and maximizing
            this.Bounds = targetScreen.Bounds;
            this.WindowState = FormWindowState.Maximized;

            // Ensure the form can receive keyboard input immediately after loading
            this.Activate(); // Brings the form to the foreground and activates it.
            this.Focus();    // Sets input focus to the form.
            SetupInitialView(); // Call here
        };
    }

    private void SetupInitialView()
    {
        if (this.currentImage == null || this.displayPanel == null || this.displayPanel.ClientSize.Width == 0 || this.displayPanel.ClientSize.Height == 0)
        {
            this.currentZoom = 1.0f;
            if (this.zoomSlider != null) this.zoomSlider.Value = 100;
            this.currentPan = PointF.Empty;
            if (this.displayPanel != null) this.displayPanel.Invalidate();
            return;
        }

        if (this.initialSourceRegion.HasValue && this.initialSourceRegion.Value.Width > 0 && this.initialSourceRegion.Value.Height > 0)
        {
            RectangleF region = this.initialSourceRegion.Value;

            // Calculate zoom to fit the selected region within the panel
            float zoomX = (float)this.displayPanel.ClientSize.Width / region.Width;
            float zoomY = (float)this.displayPanel.ClientSize.Height / region.Height;
            this.currentZoom = Math.Min(zoomX, zoomY);

            // Set pan to the top-left of the selected region
            this.currentPan = new PointF(region.X, region.Y);
        }
        else
        {
            // Fallback: Fit entire image if no valid region is provided
            float zoomX = (float)this.displayPanel.ClientSize.Width / this.currentImage.Width;
            float zoomY = (float)this.displayPanel.ClientSize.Height / this.currentImage.Height;
            this.currentZoom = Math.Min(zoomX, zoomY);

            // Center the full image
            this.currentPan.X = (this.currentImage.Width / 2.0f) - (this.displayPanel.ClientSize.Width / this.currentZoom / 2.0f);
            this.currentPan.Y = (this.currentImage.Height / 2.0f) - (this.displayPanel.ClientSize.Height / this.currentZoom / 2.0f);
        }

        // Update slider and re-clamp currentZoom based on slider limits
        if (this.zoomSlider != null) // Ensure slider is initialized
        {
            // Temporarily unsubscribe from the Scroll event
            this.zoomSlider.Scroll -= new System.EventHandler(this.zoomSlider_Scroll);

            int newSliderValue = (int)(this.currentZoom * 100);
            if (newSliderValue < this.zoomSlider.Minimum) newSliderValue = this.zoomSlider.Minimum;
            if (newSliderValue > this.zoomSlider.Maximum) newSliderValue = this.zoomSlider.Maximum;

            this.zoomSlider.Value = newSliderValue;

            // Re-confirm currentZoom from the actual slider value (potentially clamped)
            this.currentZoom = this.zoomSlider.Value / 100.0f;
            if(this.currentZoom <= 0) this.currentZoom = 0.01f; // Prevent zero or negative zoom

            // Re-subscribe to the Scroll event
            this.zoomSlider.Scroll += new System.EventHandler(this.zoomSlider_Scroll);
        }
        else
        {
            // Ensure zoom is valid even if slider isn't ready (e.g. during very early init)
            if(this.currentZoom <= 0) this.currentZoom = 0.01f;
        }

        ApplyPanBoundaries();

        if (this.displayPanel != null) this.displayPanel.Invalidate();
    }

    private void zoomSlider_Scroll(object sender, EventArgs e)
    {
        if (this.currentImage == null || this.displayPanel == null || this.displayPanel.ClientSize.Width == 0 || this.displayPanel.ClientSize.Height == 0)
        {
            this.currentZoom = this.zoomSlider.Value / 100.0f;
            if (this.displayPanel != null) this.displayPanel.Invalidate();
            return;
        }

        float oldZoom = this.currentZoom;
        float newZoom = this.zoomSlider.Value / 100.0f;
        if (newZoom <= 0) newZoom = 0.01f; // Prevent zoom from being zero or negative

        // Calculate the image point that is currently at the center of the panel
        float imagePointAtPanelCenterX = this.currentPan.X + (this.displayPanel.ClientSize.Width / 2.0f) / oldZoom;
        float imagePointAtPanelCenterY = this.currentPan.Y + (this.displayPanel.ClientSize.Height / 2.0f) / oldZoom;

        this.currentZoom = newZoom;

        // Update pan to keep the same image point at the center of the panel
        this.currentPan.X = imagePointAtPanelCenterX - (this.displayPanel.ClientSize.Width / 2.0f) / this.currentZoom;
        this.currentPan.Y = imagePointAtPanelCenterY - (this.displayPanel.ClientSize.Height / 2.0f) / this.currentZoom;

        ApplyPanBoundaries();

        this.displayPanel.Invalidate();
    }

    // Event handler for KeyDown event on the Form
    private void PresentationForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            this.Close(); // Closes the PresentationForm
        }
    }

    // Optional: Event handler for Click event (on Panel) to also close the form
    private void PresentationForm_Click(object sender, EventArgs e)
    {
        this.Close(); // Closes the PresentationForm
    }

    private void displayPanel_Paint(object sender, PaintEventArgs e)
    {
        e.Graphics.Clear(Color.Black);
        if (this.currentImage == null || this.currentZoom <= 0 || this.displayPanel.ClientSize.Width == 0 || this.displayPanel.ClientSize.Height == 0)
        {
            return;
        }

        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

        float srcRectX = this.currentPan.X;
        float srcRectY = this.currentPan.Y;
        float srcRectWidth = this.displayPanel.ClientSize.Width / this.currentZoom;
        float srcRectHeight = this.displayPanel.ClientSize.Height / this.currentZoom;

        RectangleF srcRect = new RectangleF(srcRectX, srcRectY, srcRectWidth, srcRectHeight);

        // Boundary checks for srcRect relative to the image dimensions
        srcRect.X = Math.Max(0f, srcRect.X);
        srcRect.Y = Math.Max(0f, srcRect.Y);

        if (srcRect.X + srcRect.Width > this.currentImage.Width)
        {
            srcRect.Width = this.currentImage.Width - srcRect.X;
        }
        if (srcRect.Y + srcRect.Height > this.currentImage.Height)
        {
            srcRect.Height = this.currentImage.Height - srcRect.Y;
        }

        // Ensure width and height are not negative after adjustments
        if (srcRect.Width < 0) srcRect.Width = 0;
        if (srcRect.Height < 0) srcRect.Height = 0;


        if (srcRect.Width > 0 && srcRect.Height > 0)
        {
            Rectangle destRect = new Rectangle(0, 0, this.displayPanel.ClientSize.Width, this.displayPanel.ClientSize.Height);
            e.Graphics.DrawImage(this.currentImage, destRect, srcRect, GraphicsUnit.Pixel);
        }
    }

    private void displayPanel_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            this.isPanning = true;
            this.lastMousePosition = e.Location;
            this.displayPanel.Cursor = Cursors.Hand; // Optional: change cursor
        }
    }

    private void displayPanel_MouseMove(object sender, MouseEventArgs e)
    {
        if (this.isPanning)
        {
            int deltaX = e.Location.X - this.lastMousePosition.X;
            int deltaY = e.Location.Y - this.lastMousePosition.Y;

            // Panning needs to be scaled inversely to zoom for intuitive movement.
            this.currentPan.X += (deltaX / this.currentZoom);
            this.currentPan.Y += (deltaY / this.currentZoom);

            ApplyPanBoundaries();

            this.lastMousePosition = e.Location;
            this.displayPanel.Invalidate();
        }
    }

    private void displayPanel_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            this.isPanning = false;
            this.displayPanel.Cursor = Cursors.Default; // Optional: reset cursor
        }
    }

    public void UpdateImage(string newImagePath, RectangleF? initialRegion = null)
    {
        // Dispose of the current image if it's not null
        if (this.currentImage != null)
        {
            this.currentImage.Dispose();
            this.currentImage = null; // Explicitly set to null after disposing
        }

        try
        {
            if (File.Exists(newImagePath))
            {
                this.currentImage = Image.FromFile(newImagePath);
            }
            else
            {
                MessageBox.Show("Image file not found: " + newImagePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.currentImage = null; // Clear image on error
            }
        }
        catch (OutOfMemoryException oomEx)
        {
            MessageBox.Show("Error loading image: Out of memory. The image might be too large or corrupted.\n" + oomEx.Message, "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.currentImage = null; // Clear image on error
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error loading image for presentation: " + ex.Message, "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.currentImage = null; // Clear image on error
        }

        // if (this.currentImage != null && this.displayPanel != null && this.displayPanel.ClientSize.Width > 0 && this.displayPanel.ClientSize.Height > 0)
        // {
        //     float zoomX = (float)this.displayPanel.ClientSize.Width / this.currentImage.Width;
        //     float zoomY = (float)this.displayPanel.ClientSize.Height / this.currentImage.Height;
        //     this.currentZoom = Math.Min(zoomX, zoomY);

        //     this.currentPan.X = (this.currentImage.Width / 2.0f) - (this.displayPanel.ClientSize.Width / this.currentZoom / 2.0f);
        //     this.currentPan.Y = (this.currentImage.Height / 2.0f) - (this.displayPanel.ClientSize.Height / this.currentZoom / 2.0f);

        //     ApplyPanBoundaries();

        //     int sliderValue = (int)(this.currentZoom * 100);
        //     if (sliderValue < this.zoomSlider.Minimum) sliderValue = this.zoomSlider.Minimum;
        //     if (sliderValue > this.zoomSlider.Maximum) sliderValue = this.zoomSlider.Maximum;
        //     this.zoomSlider.Value = sliderValue;
        //     // Ensure currentZoom reflects the actual slider value
        //     this.currentZoom = this.zoomSlider.Value / 100.0f;
        //     this.currentPan.X = (this.currentImage.Width / 2.0f) - (this.displayPanel.ClientSize.Width / this.currentZoom / 2.0f);
        //     this.currentPan.Y = (this.currentImage.Height / 2.0f) - (this.displayPanel.ClientSize.Height / this.currentZoom / 2.0f);
        //     ApplyPanBoundaries();
        // }
        // else if (this.zoomSlider != null)
        // {
        //     this.currentZoom = 1.0f;
        //     this.zoomSlider.Value = 100;
        //     this.currentPan = PointF.Empty;
        // }

        // if (this.displayPanel != null)
        // {
        //     this.displayPanel.Invalidate(); // Trigger repaint
        // }
        this.initialSourceRegion = initialRegion; // Store the new region before calling SetupInitialView
        SetupInitialView(); // Call this to set zoom/pan for the new image

        this.Activate();
    }

    private void ApplyPanBoundaries()
    {
        if (this.currentImage == null || this.currentZoom <= 0 || this.displayPanel == null || this.displayPanel.ClientSize.Width == 0 || this.displayPanel.ClientSize.Height == 0) return;

        float panelWidth = this.displayPanel.ClientSize.Width;
        float panelHeight = this.displayPanel.ClientSize.Height;

        // Calculate the effective width and height of the image if it were fully displayed at currentPan.X = 0, currentPan.Y = 0
        // This is not zoomedImageWidth/Height, but rather the maximum extent of the image in its own coordinate system.
        float maxImageX = this.currentImage.Width;
        float maxImageY = this.currentImage.Height;

        // Calculate the width/height of the viewport in terms of image pixels
        float viewportWidthInImagePixels = panelWidth / this.currentZoom;
        float viewportHeightInImagePixels = panelHeight / this.currentZoom;

        // If the zoomed image is smaller than the panel, center it.
        // currentPan.X here represents the top-left X of the source rectangle.
        if (maxImageX < viewportWidthInImagePixels) // Image width is smaller than viewport width in image pixels
        {
             // Center X: (image_width / 2) - (viewport_width_in_image_pixels / 2)
            this.currentPan.X = (this.currentImage.Width / 2.0f) - (viewportWidthInImagePixels / 2.0f);
        }
        else
        {
            // Max value for currentPan.X is when the right edge of the image aligns with the right edge of the viewport
            // maxPanX = image_width - viewport_width_in_image_pixels
            float maxPanX = this.currentImage.Width - viewportWidthInImagePixels;
            this.currentPan.X = Math.Max(0, Math.Min(this.currentPan.X, maxPanX));
        }

        if (maxImageY < viewportHeightInImagePixels) // Image height is smaller than viewport height in image pixels
        {
            // Center Y: (image_height / 2) - (viewport_height_in_image_pixels / 2)
            this.currentPan.Y = (this.currentImage.Height / 2.0f) - (viewportHeightInImagePixels / 2.0f);
        }
        else
        {
            // Max value for currentPan.Y is when the bottom edge of the image aligns with the bottom edge of the viewport
            // maxPanY = image_height - viewport_height_in_image_pixels
            float maxPanY = this.currentImage.Height - viewportHeightInImagePixels;
            this.currentPan.Y = Math.Max(0, Math.Min(this.currentPan.Y, maxPanY));
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // if (components != null) components.Dispose(); // Only if 'components' field exists
            if (this.currentImage != null)
            {
                this.currentImage.Dispose();
                this.currentImage = null;
            }
        }
        base.Dispose(disposing);
    }
}
