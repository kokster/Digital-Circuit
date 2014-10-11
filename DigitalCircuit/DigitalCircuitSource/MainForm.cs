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

namespace DigitalCircuitSource
{
    public partial class MainForm : Form
    {
        Timer timer = new Timer();
        private bool leftMouseDown;
        private bool middleMouseDown;
        private TreeNode nodeBeingDragged;
        TreeNode hoveredNode;
        CircuitManager circuitManager;
        Circuit circuit;
        Canvas canvas;

        public MainForm()
        {
            InitializeComponent();
            this.panelCanvas.MouseWheel += new MouseEventHandler(panelCanvas_MouseWheel);
            circuit = new Circuit();
            initializeTimer();
            initializeTreeView();
            initializeCircuitManager();
            initializeCanvas();
            refresh();
        }

        private void initializeTreeView() {
            treeViewItems.ExpandAll();
        }

        private void initializeCanvas()
        {
            canvas = new Canvas(panelCanvas, circuit);
        }

        private void initializeCircuitManager()
        {
            circuitManager = new CircuitManager(circuit);
        }

        private void panelCanvas_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                if (canvas.zoomOutPossible())
                {
                    canvas.zoomOut(new Point(e.X, e.Y));
                    refresh();
                }
            }
            else
            {
                if (canvas.zoomInPossible())
                {
                    canvas.zoomIn(new Point(e.X, e.Y));
                    refresh();
                }
            }
        }

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

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            MessageBox.Show("form: mousedown");
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            MessageBox.Show("form: mouseup");
        }

        void initializeTimer()
        {
            timer.Tick += new EventHandler(timer_Tick); // Everytime timer ticks, timer_Tick will be called
            timer.Interval = 30;                        // Timer will tick evert second
            timer.Enabled = true;                       // Enable the timer
            timer.Start();                              // Start the timer
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (leftMouseDown != (MouseButtons == MouseButtons.Left))
            {
                leftMouseDown = (MouseButtons == MouseButtons.Left);
                if (leftMouseDown)
                {
                    leftMousePressed();
                }
                else
                {
                    leftMouseReleased();
                }
            }

            if (middleMouseDown != (MouseButtons == MouseButtons.Middle))
            {
                middleMouseDown = (MouseButtons == MouseButtons.Middle);
                if (middleMouseDown)
                {
                    middleMousePressed();
                }
                else
                {
                    middleMouseReleased();
                }
            }
        }

        void leftMousePressed()
        {

        }

        void leftMouseReleased()
        {
            nodeBeingDragged = null;
        }

        void switchCursorTool()
        {
            toolPan.Checked = !toolPan.Checked;
            toolPointer.Checked = !toolPointer.Checked;
        }

        void middleMousePressed()
        {
            switchCursorTool();
        }

        void middleMouseReleased()
        {
            switchCursorTool();
        }

        private void treeViewItems_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void panelViewport_MouseUp(object sender, MouseEventArgs e)
        {
            if (nodeBeingDragged == null)
            {
                return;
            }
            else
            {
                Item item = null;
                switch (nodeBeingDragged.Name)
                {
                    case "nodeAndGate":
                        item = new AndGate();
                        break;

                    case "nodeOrGate":
                        item = new OrGate();
                        break;

                    case "nodeNotGate":
                        item = new NotGate();
                        break;

                    case "nodeSwitch":
                        item = new Switch();
                        break;

                    case "nodeLamp":
                        item = new Lamp();
                        break;
                }
                if (item != null)
                {
                    Point absoluteCoordinates = canvas.getAbsoluteCoordinates(new Point(e.X, e.Y));
                    item.Location = absoluteCoordinates;
                    if (circuitManager.addItem(item))
                    {
                        refresh();
                    }
                }
            }
        }

        private void treeViewItems_MouseMove(object sender, MouseEventArgs e)
        {
            TreeNode tn = treeViewItems.GetNodeAt(e.Location);
            if (tn != null && tn != hoveredNode && nodeBeingDragged == null)
            {
                if (hoveredNode != null)
                {
                    hoveredNode.BackColor = Color.Transparent;
                }
                hoveredNode = tn;
                if (tn.Nodes.Count == 0)
                {
                    tn.BackColor = Color.LightSteelBlue;
                }
            }
        }

        private void refresh()
        {
            toolUndo.Enabled = circuitManager.undoPossible();
            toolRedo.Enabled = circuitManager.redoPossible();
            toolZoomIn.Enabled = canvas.zoomInPossible();
            toolZoomOut.Enabled = canvas.zoomOutPossible();
            canvas.refresh();
        }

        private void panelViewport_Resize(object sender, EventArgs e)
        {
            canvas.changeSize(panelCanvas.Width, panelCanvas.Height);
        }

        private void toolUndo_Click(object sender, EventArgs e)
        {
            circuitManager.undo();
            refresh();
        }

        private void toolRedo_Click(object sender, EventArgs e)
        {
            circuitManager.redo();
            refresh();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                canvas.pan(Canvas.PanDirection.up);
            }

            if (e.KeyCode == Keys.Down)
            {
                canvas.pan(Canvas.PanDirection.down);
            }

            if (e.KeyCode == Keys.Left)
            {
                canvas.pan(Canvas.PanDirection.left);
            }

            if (e.KeyCode == Keys.Right)
            {
                canvas.pan(Canvas.PanDirection.right);
            }

            if (e.KeyCode == Keys.Z && e.Control && e.Shift)
            {
                if (circuitManager.redoPossible())
                {
                    circuitManager.redo();
                    refresh();
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyCode == Keys.Z && e.Control)
            {
                if (circuitManager.undoPossible())
                {
                    circuitManager.undo();
                    refresh();
                    e.SuppressKeyPress = true;
                }
            }
        }
        private void toolZoomOut_Click(object sender, EventArgs e)
        {
            canvas.zoomOut();
            refresh();
        }

        private void toolZoomIn_Click(object sender, EventArgs e)
        {
            canvas.zoomIn();
            refresh();
        }

        private void panelCanvas_MouseEnter(object sender, EventArgs e)
        {
            panelCanvas.Focus();
        }

        private void panelCanvas_MouseLeave(object sender, EventArgs e)
        {
            ActiveControl = null;
        }

        private Point lastMousePoint = Point.Empty;
        private void panelCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (middleMouseDown && toolPan.Checked)
            {  
                if (lastMousePoint != Point.Empty)
                {
                    canvas.pan(new Point(lastMousePoint.X - e.X, lastMousePoint.Y - e.Y));
                }
                lastMousePoint = new Point(e.X, e.Y);
            } else {
                lastMousePoint = Point.Empty;
            }
        }

        private void panelCanvas_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
