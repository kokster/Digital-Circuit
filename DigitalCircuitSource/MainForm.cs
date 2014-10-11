using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DigitalCircuit.Library;
using System.Resources;
using System.Reflection;
using System.IO;

namespace DigitalCircuitSource
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// Timer used for tracking the state of the mouse
        /// </summary>
        Timer timer = new Timer();
        public bool leftMouseDown { get; private set; }
        public bool middleMouseDown { get; private set; }

        /// <summary>
        /// Checks if the panning tool is selected
        /// </summary>
        public bool toolPanSelected
        {
            get
            {
                return toolPan.Checked;
            }
        }

        /// <summary>
        /// The node of the treeview that is being dragged to the circuit
        /// </summary>
        public TreeNode nodeBeingDragged { get; set; }

        /// <summary>
        /// The port that is being dragged for making a connection
        /// </summary>
        public PortDrawing portDrawingBeingDragged { get; private set; }

        /// <summary>
        /// The circuit manager that manages the current circuit
        /// </summary>
        public CircuitManager circuitManager { get; private set; }

        /// <summary>
        /// The canvas that displays the circuit and manages related input such as drawing a connection
        /// </summary>
        public Canvas canvas { get; private set; }

        /// <summary>
        /// The filepath of the last saved file that is still opened
        /// </summary>
        private string lastFilePath;

        /// <summary>
        /// Whether there are any changes made since the last save or since a new documented has been opened
        /// </summary>
        public bool unsavedChangesMade = false;

        public MainForm()
        {
            InitializeComponent();     
            initializeTimer();
            treeViewItems.ExpandAll();
            circuitManager = new CircuitManager();
            canvas = new Canvas(panelCanvas, this);
            toolPan.CheckedChanged += toolPan_CheckedChanged;
            refresh();
        }

        /// <summary>
        /// Gets fired when the checked state of the panning tool gets changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void toolPan_CheckedChanged(object sender, EventArgs e)
        {
            setCursor();
        }

        /// <summary>
        /// Sets the cursor according to the tool that is currently selected
        /// </summary>
        void setCursor()
        {
            if (toolPan.Checked)
            {
                treeViewItems.Enabled = false;
                Cursor = Cursors.SizeAll;
            }
            else
            {
                treeViewItems.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        //
        /// <summary>
        /// Makes it only possible to select leafs of the tree view and makes dragging
        /// of those leafs possible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeViewItems_MouseDown(object sender, MouseEventArgs e)
        {
            TreeNode tn = treeViewItems.GetNodeAt(e.Location);
            if (tn != null && tn.Nodes.Count == 0)
            {
                treeViewItems.SelectedNode = tn;
                nodeBeingDragged = tn;
            }
        }

        /// <summary>
        /// Initializes the timer
        /// </summary>
        void initializeTimer()
        {
            timer.Tick += new EventHandler(timer_Tick); // Everytime timer ticks, timer_Tick will be called
            timer.Interval = 2;                        // Timer will tick evert second
            timer.Enabled = true;                       // Enable the timer
            timer.Start();                              // Start the timer
        }

        /// <summary>
        /// Timer tick that will register the current mouse state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timer_Tick(object sender, EventArgs e)
        {
            if (leftMouseDown != (MouseButtons == MouseButtons.Left))
            {
                leftMouseDown = (MouseButtons == MouseButtons.Left);
                if (!(MouseButtons == MouseButtons.Left))
                {
                    leftMouseReleased();
                }
            }

            if (middleMouseDown != (MouseButtons == MouseButtons.Middle))
            {
                middleMouseDown = (MouseButtons == MouseButtons.Middle);
                if (middleMouseDown)
                {
                    switchCursorTool();
                }
                else
                {
                    switchCursorTool();
                }
            }
        }

        /// <summary>
        /// Will be executed when the left mouse is released, with a delay of a few milliseconds
        /// </summary>
        void leftMouseReleased()
        {
            nodeBeingDragged = null;
            portDrawingBeingDragged = null;
        }

        /// <summary>
        /// Switches the currently selected tool (from panning to pointer or pointer to panning tool
        /// </summary>
        void switchCursorTool()
        {
            toolPan.Checked = !toolPan.Checked;
            toolPointer.Checked = !toolPointer.Checked;
        }

        /// <summary>
        /// Disables selecting treeview item, since items can only be dragged and dropped and selection has no
        /// meaning
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeViewItems_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
        }

        /// <summary>
        /// Refresh the whole application
        /// </summary>
        public void refresh()
        {
            toolUndo.Enabled = circuitManager.undoPossible();
            toolRedo.Enabled = circuitManager.redoPossible();
            toolZoomIn.Enabled = canvas.zoomInPossible();
            toolZoomOut.Enabled = canvas.zoomOutPossible();
            if (unsavedChangesMade)
            {
                toolSave.Enabled = true;
            }
            else
            {
                toolSave.Enabled = false;
            }
            canvas.refresh();
        }

        /// <summary>
        /// Event handler for clicking the undo button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolUndo_Click(object sender, EventArgs e)
        {
            circuitManager.undo();
            unsavedChangesMade = true;
            refresh();
        }

        /// <summary>
        /// Event handler for clicking the redo button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolRedo_Click(object sender, EventArgs e)
        {
            circuitManager.redo();
            unsavedChangesMade = true;
            refresh();
        }

        /// <summary>
        /// Event handler for pressing any button and is resposible for registering shortcuts (ctrl + z, ctrl + shift + z)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Z && e.Control && e.Shift)
            {
                if (circuitManager.redoPossible())
                {
                    circuitManager.redo();
                    refresh();
                    e.SuppressKeyPress = true;
                    unsavedChangesMade = true;
                }
            }
            else if (e.KeyCode == Keys.Z && e.Control)
            {
                if (circuitManager.undoPossible())
                {
                    circuitManager.undo();
                    unsavedChangesMade = true;
                    e.SuppressKeyPress = true;
                    refresh();
                }
            }
        }

        /// <summary>
        /// Event handler for clicking the zoom out button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolZoomOut_Click(object sender, EventArgs e)
        {
            canvas.zoomOut();
            refresh();
        }

        /// <summary>
        /// Event handler for clicking the zoom in button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolZoomIn_Click(object sender, EventArgs e)
        {
            canvas.zoomIn();
            refresh();
        }

        /// <summary>
        /// Event handler for entering the canvas and focusses on the panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panelCanvas_MouseEnter(object sender, EventArgs e)
        {
            canvas.mouseOnCanvas = true;
            panelCanvas.Focus();
        }

        /// <summary>
        /// Event handler for leaving the canvas and unfocusses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panelCanvas_MouseLeave(object sender, EventArgs e)
        {
            canvas.mouseOnCanvas = false;
            canvas.refresh();
            ActiveControl = null;
        }


        public void portUp(PortDrawing port)
        {
            if(connectionPossible(port.port)) {
                Connection connection = new Connection(portDrawingBeingDragged.port, port.port);
                circuitManager.addConnection(connection);
                refresh();
                unsavedChangesMade = true;
            }
        }

        /// <summary>
        /// Gets executed when a port is being dragged
        /// </summary>
        /// <param name="port"></param>
        public void portDown(PortDrawing port)
        {
            portDrawingBeingDragged = port;
        }

        /// <summary>
        /// Checks if a connection is possible
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool connectionPossible(Port port)
        {
            if (portDrawingBeingDragged != null && circuitManager.circuit.connectionPossible(portDrawingBeingDragged.port, port))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Event handler for clicking the panning tool
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolPan_Click(object sender, EventArgs e)
        {
            toolPan.Checked = true;
            toolPointer.Checked = false;
        }

        /// <summary>
        /// Event handler for clicking the pointer tool
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolPointer_Click(object sender, EventArgs e)
        {
            toolPan.Checked = false;
            toolPointer.Checked = true;
        }

        /// <summary>
        /// Event handler for entering the toolstrip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStrip_MouseEnter(object sender, EventArgs e)
        {
            // Override cursor
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// Event handler for clicking the save as button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolSaveAs_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Title = "Save a circuit";
                saveDialog.Filter = "Circuit File|*.circuit";
                DialogResult result = saveDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    FileStream stream = File.Open(saveDialog.FileName, FileMode.Create);
                    circuitManager.circuit.SaveToFile(stream);
                    stream.Close();
                    lastFilePath = saveDialog.FileName;

                    unsavedChangesMade = false;
                    refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Event handler for leaving the toolstrip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStrip_MouseLeave(object sender, EventArgs e)
        {
            setCursor();
        }

        /// <summary>
        /// Event handler for clicking the save button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (lastFilePath == null)
                {
                    // Circuit wasn't saved yet
                    toolSaveAs_Click(sender, e);
                }
                else
                {
                    // Circuit was saved
                    FileStream stream = File.Open(lastFilePath, FileMode.Create);
                    circuitManager.circuit.SaveToFile(stream);
                    stream.Close();
                    unsavedChangesMade = false;
                    refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Event handler for clicking the new document button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolNewDocument_Click(object sender, EventArgs e)
        {
            if (unsavedChangesMade)
            {
                if (!promptSave())
                {
                    return;
                }
            }

            circuitManager = new CircuitManager();
            unsavedChangesMade = false;
            lastFilePath = null;
            newCanvas();
            refresh();
        }

        /// <summary>
        /// Asks if the user wants to save current circuit
        /// </summary>
        private bool promptSave()
        {
            DialogResult result = MessageBox.Show("Would you like to save the circuit?", "Save changes?",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                toolSaveAs_Click(this, new EventArgs());
            }
            else if (result == DialogResult.Cancel)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Resets the canvas for a new file
        /// </summary>
        private void newCanvas()
        {
            canvas.Destruct();
            panelCanvas.Controls.Clear();
            canvas = new Canvas(panelCanvas, this);
        }

        /// <summary>
        /// Event handler for clicking the open button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolOpen_Click(object sender, EventArgs e)
        {
            if (unsavedChangesMade)
            {
                if (!promptSave())
                {
                    return;
                }
            }

            OpenFileDialog loadDialog = new OpenFileDialog();
            loadDialog.Title = "Choose a circuit";
            loadDialog.Filter = "Circuit File|*.circuit";
            DialogResult result = loadDialog.ShowDialog();

            if(result == DialogResult.OK)
            {
                try
                {
                    FileStream stream = File.Open(loadDialog.FileName, FileMode.Open);
                    circuitManager.LoadFromFile(stream);
                    stream.Close();
                    newCanvas();
                    
                    unsavedChangesMade = false;
                    refresh();
                }
                catch
                {
                    MessageBox.Show("Error: could not open file!");
                }
            }
        }
    }
}
