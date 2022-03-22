using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;


namespace WindowsFormsApp1
{

    [StructLayout(LayoutKind.Sequential)]
    internal struct MOUSEINPUT
    {
        public int X;
        public int Y;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KEYBDINPUT
    {
        public ushort Vk;
        public ushort Scan;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HARDWAREINPUT
    {
        public uint Msg;
        public ushort ParamL;
        public ushort ParamH;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct MOUSEKEYBDHARDWAREINPUT
    {
        [FieldOffset(0)]
        public HARDWAREINPUT Hardware;
        [FieldOffset(0)]
        public KEYBDINPUT Keyboard;
        [FieldOffset(0)]
        public MOUSEINPUT Mouse;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct INPUT
    {
        public uint Type;
        public MOUSEKEYBDHARDWAREINPUT Data;
    }

    public partial class Form1 : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        //private static extern uint SendInput(uint numberOfInputs, INPUT inputs, int sizeOfInputStructure);
        private static extern uint SendInput(uint numberOfInputs, INPUT[] inputs, int sizeOfInputStructure);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out Point lpPoint);



        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public SendInputEventType type;
            public MouseKeybdhardwareInputUnion mkhi;
        }
        [StructLayout(LayoutKind.Explicit)]
        struct MouseKeybdhardwareInputUnion
        {
            [FieldOffset(0)]
            public MouseInputData mi;

            [FieldOffset(0)]
            public KEYBDINPUT ki;

            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }
        struct MouseInputData
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public MouseEventFlags dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        [Flags]
        enum MouseEventFlags : uint
        {
            MOUSEEVENTF_MOVE = 0x0001,
            MOUSEEVENTF_LEFTDOWN = 0x0002,
            MOUSEEVENTF_LEFTUP = 0x0004,
            MOUSEEVENTF_RIGHTDOWN = 0x0008,
            MOUSEEVENTF_RIGHTUP = 0x0010,
            MOUSEEVENTF_MIDDLEDOWN = 0x0020,
            MOUSEEVENTF_MIDDLEUP = 0x0040,
            MOUSEEVENTF_XDOWN = 0x0080,
            MOUSEEVENTF_XUP = 0x0100,
            MOUSEEVENTF_WHEEL = 0x0800,
            MOUSEEVENTF_VIRTUALDESK = 0x4000,
            MOUSEEVENTF_ABSOLUTE = 0x8000
        }
        //Mouse actions
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        private System.Windows.Forms.ListBox ListDragSource;
        //private System.Windows.Forms.ListBox ListDragTarget;
        private System.Windows.Forms.CheckBox UseCustomCursorsCheck;
        private System.Windows.Forms.Label DropLocationLabel;

        private int indexOfItemUnderMouseToDrag;
        private int indexOfItemUnderMouseToDrop;

        private Rectangle dragBoxFromMouseDown;
        private Point screenOffset;

        private Cursor MyNoDropCursor;
        private Cursor MyNormalCursor;

        public void DoMouseClick()
        {
            //Call the imported function with the cursor's current position
            uint X = (uint)Cursor.Position.X;
            uint Y = (uint)Cursor.Position.Y;
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
        }

        enum SendInputEventType : int
        {
            InputMouse,
            InputKeyboard,
            InputHardware
        }

        public Form1()
        {
            //InitializeComponent();
            //doMove();
            this.ListDragSource = new System.Windows.Forms.ListBox();
            //this.ListDragTarget = new System.Windows.Forms.ListBox();
            this.UseCustomCursorsCheck = new System.Windows.Forms.CheckBox();
            this.DropLocationLabel = new System.Windows.Forms.Label();

            this.SuspendLayout();

            // ListDragSource
            this.ListDragSource.Items.AddRange(new object[] {"aa.json", "bb.json" });
 //           this.ListDragSource.Items.AddRange(new object[] {"one", "two", "three", "four",
 //"five", "six", "seven", "eight",
 //"nine", "ten"});
            this.ListDragSource.Location = new System.Drawing.Point(10, 17);
            this.ListDragSource.Size = new System.Drawing.Size(120, 225);
            this.ListDragSource.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ListDragSource_MouseDown);
            //this.ListDragSource.QueryContinueDrag += new System.Windows.Forms.QueryContinueDragEventHandler(this.ListDragSource_QueryContinueDrag);
            this.ListDragSource.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ListDragSource_MouseUp);
            this.ListDragSource.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ListDragSource_MouseMove);
            this.ListDragSource.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.ListDragSource_GiveFeedback);

            // ListDragTarget
            //this.ListDragTarget.AllowDrop = false;
            //this.ListDragTarget.Location = new System.Drawing.Point(154, 17);
            //this.ListDragTarget.Size = new System.Drawing.Size(120, 225);
            //this.ListDragTarget.DragOver += new System.Windows.Forms.DragEventHandler(this.ListDragTarget_DragOver);
            //this.ListDragTarget.DragDrop += new System.Windows.Forms.DragEventHandler(this.ListDragTarget_DragDrop);
            //this.ListDragTarget.DragEnter += new System.Windows.Forms.DragEventHandler(this.ListDragTarget_DragEnter);
            //this.ListDragTarget.DragLeave += new System.EventHandler(this.ListDragTarget_DragLeave);

            // UseCustomCursorsCheck
            this.UseCustomCursorsCheck.Location = new System.Drawing.Point(10, 243);
            this.UseCustomCursorsCheck.Size = new System.Drawing.Size(137, 24);
            this.UseCustomCursorsCheck.Text = "Use Custom Cursors";

            // DropLocationLabel
            this.DropLocationLabel.Location = new System.Drawing.Point(154, 245);
            this.DropLocationLabel.Size = new System.Drawing.Size(137, 24);
            this.DropLocationLabel.Text = "None";

            // Form1
            this.ClientSize = new System.Drawing.Size(292, 270);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {this.ListDragSource,
  this.UseCustomCursorsCheck,
 this.DropLocationLabel});
 //           this.Controls.AddRange(new System.Windows.Forms.Control[] {this.ListDragSource,
 //this.ListDragTarget, this.UseCustomCursorsCheck,
 //this.DropLocationLabel});
            this.Text = "drag-and-drop Example";

            this.ResumeLayout(false);
        }
        private void ListDragSource_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Get the index of the item the mouse is below.
            indexOfItemUnderMouseToDrag = ListDragSource.IndexFromPoint(e.X, e.Y);

            if (indexOfItemUnderMouseToDrag != ListBox.NoMatches)
            {

                // Remember the point where the mouse down occurred. The DragSize indicates
                // the size that the mouse can move before a drag event should be started. 
                Size dragSize = SystemInformation.DragSize;

                // Create a rectangle using the DragSize, with the mouse position being
                // at the center of the rectangle.
                dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2),
                e.Y - (dragSize.Height / 2)), dragSize);
            }
            else
                // Reset the rectangle if the mouse is not over an item in the ListBox.
                dragBoxFromMouseDown = Rectangle.Empty;

        }

        private void ListDragSource_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Reset the drag rectangle when the mouse button is raised.
            dragBoxFromMouseDown = Rectangle.Empty;
        }

        private void ListDragSource_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                string[] filesToDrag =
               {
                 "E:\\temp\\aa.json"
                };

                // Proceed with the drag-and-drop, passing in the list item. 
                DragDropEffects dropEffect = ListDragSource.DoDragDrop(new DataObject(DataFormats.FileDrop, filesToDrag), DragDropEffects.All | DragDropEffects.Link);
                Debug.WriteLine("doDragDrop");
                // If the drag operation was a move then remove the item.
                if (dropEffect == DragDropEffects.Move)
                {
                    ListDragSource.Items.RemoveAt(indexOfItemUnderMouseToDrag);

                    // Selects the previous item in the list as long as the list has an item.
                    if (indexOfItemUnderMouseToDrag > 0)
                        ListDragSource.SelectedIndex = indexOfItemUnderMouseToDrag - 1;

                    else if (ListDragSource.Items.Count > 0)
                        // Selects the first item.
                        ListDragSource.SelectedIndex = 0;
                }
            }
        }
        private void ListDragSource_GiveFeedback(object sender, System.Windows.Forms.GiveFeedbackEventArgs e)
        {
            // Use custom cursors if the check box is checked.
            if (UseCustomCursorsCheck.Checked)
            {

                // Sets the custom cursor based upon the effect.
                e.UseDefaultCursors = false;
                if ((e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
                    Cursor.Current = MyNormalCursor;
                else
                    Cursor.Current = MyNoDropCursor;
            }

        }
        private void ListDragTarget_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
        {

            // Determine whether string data exists in the drop data. If not, then
            // the drop effect reflects that the drop cannot occur.
            if (!e.Data.GetDataPresent(typeof(System.String)))
            {

                e.Effect = DragDropEffects.None;
                DropLocationLabel.Text = "None - no string data.";
                return;
            }

            // Set the effect based upon the KeyState.
            if ((e.KeyState & (8 + 32)) == (8 + 32) &&
            (e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link)
            {
                // KeyState 8 + 32 = CTL + ALT

                // Link drag-and-drop effect.
                e.Effect = DragDropEffects.Link;

            }
            else if ((e.KeyState & 32) == 32 &&
           (e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link)
            {

                // ALT KeyState for link.
                e.Effect = DragDropEffects.Link;

            }
            else if ((e.KeyState & 4) == 4 &&
           (e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
            {

                // SHIFT KeyState for move.
                e.Effect = DragDropEffects.Move;

            }
            else if ((e.KeyState & 8) == 8 &&
           (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {

                // CTL KeyState for copy.
                e.Effect = DragDropEffects.Copy;

            }
            else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
            {

                // By default, the drop action should be move, if allowed.
                e.Effect = DragDropEffects.Move;

            }
            else
                e.Effect = DragDropEffects.None;

            // Get the index of the item the mouse is below.

            // The mouse locations are relative to the screen, so they must be 
            // converted to client coordinates.

            //indexOfItemUnderMouseToDrop =
             //ListDragTarget.IndexFromPoint(ListDragTarget.PointToClient(new Point(e.X, e.Y)));

            // Updates the label text.
            if (indexOfItemUnderMouseToDrop != ListBox.NoMatches)
            {

                DropLocationLabel.Text = "Drops before item #" + (indexOfItemUnderMouseToDrop + 1);
            }
            else
                DropLocationLabel.Text = "Drops at the end.";

        }
        private void ListDragTarget_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            // Ensure that the list item index is contained in the data.
            if (e.Data.GetDataPresent(typeof(System.String)))
            {

                Object item = (object)e.Data.GetData(typeof(System.String));

                // Perform drag-and-drop, depending upon the effect.
                if (e.Effect == DragDropEffects.Copy ||
                e.Effect == DragDropEffects.Move)
                {

                    // Insert the item.
                    //if (indexOfItemUnderMouseToDrop != ListBox.NoMatches)
                    //    ListDragTarget.Items.Insert(indexOfItemUnderMouseToDrop, item);
                    //else
                    //    ListDragTarget.Items.Add(item);

                }
            }
            // Reset the label text.
            DropLocationLabel.Text = "None";
        }
        private void ListDragSource_QueryContinueDrag(object sender, System.Windows.Forms.QueryContinueDragEventArgs e)
        {
            // Cancel the drag if the mouse moves off the form.
            ListBox lb = sender as ListBox;

            if (lb != null)
            {

                Form f = lb.FindForm();

                // Cancel the drag if the mouse moves off the form. The screenOffset
                // takes into account any desktop bands that may be at the top or left
                // side of the screen.
                if (((Control.MousePosition.X - screenOffset.X) < f.DesktopBounds.Left) ||
                ((Control.MousePosition.X - screenOffset.X) > f.DesktopBounds.Right) ||
                ((Control.MousePosition.Y - screenOffset.Y) < f.DesktopBounds.Top) ||
                ((Control.MousePosition.Y - screenOffset.Y) > f.DesktopBounds.Bottom))
                {

                    e.Action = DragAction.Cancel;
                }
            }
        }
        private void ListDragTarget_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            // Reset the label text.
            DropLocationLabel.Text = "None";
        }
        private void ListDragTarget_DragLeave(object sender, System.EventArgs e)
        {
            // Reset the label text.
            DropLocationLabel.Text = "None";
        }
        void SendMove(int x, int  y)
        {
            INPUT mouseInput = new INPUT();

            int W = Screen.PrimaryScreen.Bounds.Width;
            int H = Screen.PrimaryScreen.Bounds.Height;

            //// MOUSEINPUT struct uses a truely bizarre coordinate system
            ////   the screen is mapped to a scale from 0 to 65535 in both axis.
            ////Rectangle screen = Screen.PrimaryScreen.Bounds;


            mouseInput.type = SendInputEventType.InputMouse;
            //mouseInput.mkhi.mi.dx = 2; 
            //mouseInput.mkhi.mi.dy = 2;
            mouseInput.mkhi.mi.dx = (65535 * x)/ W; 
            mouseInput.mkhi.mi.dy = (65535 *y) /H;
            mouseInput.mkhi.mi.mouseData = 0;


            //mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_MOVE ;
            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_MOVE | MouseEventFlags.MOUSEEVENTF_ABSOLUTE;
            INPUT[] inputs = { mouseInput };
            SendInput(1, inputs, Marshal.SizeOf(new INPUT()));

        }
        private int doMove()
        {
            int nCnt = 0;

            int W = Screen.PrimaryScreen.Bounds.Width;
            int H = Screen.PrimaryScreen.Bounds.Height;
            while (nCnt < 5)
            {
                SendMove(W/2+ nCnt*100, H/2);

                Thread.Sleep(2000);
                nCnt++;
            }
                DoMouseClick();
            return 0;

        }

        private void Test_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keys)e.KeyValue == Keys.D1) {
                this.Close();
            }
            else
            {

            }

        }
    }
}
