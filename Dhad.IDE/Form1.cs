using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Dhad.Core;

namespace Dhad.IDE
{
    /// <summary>
    /// The main window of the Dhad IDE where children write and run their programs
    /// It's divided into areas for code editing, terminal output, and graphics display
    /// </summary>
    public partial class MainForm : Form
    {
        // UI Components
        private TextBox codeEditor;           // Where children write their code
        private TextBox terminal;             // Shows program output (like أظهر statements)
        private PictureBox graphicsDisplay;   // Shows drawings
        private Button runButton;             // Run the program
        private Button clearButton;           // Clear everything
        private Button newButton;             // New program
        private Button openButton;            // Open saved program
        private Button saveButton;            // Save program
        private StatusStrip statusBar;        // Shows helpful messages
        private ToolStripStatusLabel statusLabel;
        
        // Language processing components
        private Lexer? lexer;
        private Parser? parser;
        private Interpreter? interpreter;
        
        // Graphics handling
        private Bitmap? drawingSurface;
        private Graphics? drawingGraphics;
        private GraphicsAdapter? graphicsAdapter;
        private TerminalAdapter? terminalAdapter;
        
        // Colors mapping (Arabic to System.Drawing.Color)
        private readonly Dictionary<string, Color> colorMap = new Dictionary<string, Color>
        {
            { "أحمر", Color.Red },
            { "أخضر", Color.Green },
            { "أزرق", Color.Blue },
            { "أصفر", Color.Yellow },
            { "أسود", Color.Black },
            { "أبيض", Color.White },
            { "برتقالي", Color.Orange },
            { "بنفسجي", Color.Purple },
            { "وردي", Color.Pink },
            { "رمادي", Color.Gray },
            { "بني", Color.Brown }
        };

        public MainForm()
        {
            InitializeComponent();
            SetupDefaultProgram();
        }

        private void InitializeComponent()
        {
            // Form settings
            this.Text = "ضاد - بيئة البرمجة للأطفال";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            // Create main container
            var mainContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 400,
                RightToLeft = RightToLeft.Yes
            };

            // Create toolbar
            var toolbar = new ToolStrip
            {
                RightToLeft = RightToLeft.Yes,
                Dock = DockStyle.Top,
                ImageScalingSize = new Size(32, 32),
                Height = 40
            };

            // Create buttons with Arabic labels
            newButton = CreateToolButton("جديد", "إنشاء برنامج جديد");
            openButton = CreateToolButton("فتح", "فتح برنامج محفوظ");
            saveButton = CreateToolButton("حفظ", "حفظ البرنامج");
            runButton = CreateToolButton("▶ تشغيل", "تشغيل البرنامج");
            clearButton = CreateToolButton("مسح", "مسح الشاشة");

            // Style the run button specially
            runButton.Font = new Font(runButton.Font.FontFamily, 12, FontStyle.Bold);
            runButton.ForeColor = Color.Green;

            toolbar.Items.AddRange(new ToolStripItem[] {
                newButton,
                new ToolStripSeparator(),
                openButton,
                saveButton,
                new ToolStripSeparator(),
                runButton,
                clearButton
            });

            // Left panel - Code Editor
            var leftPanel = new Panel { Dock = DockStyle.Fill };
            
            var codeLabel = new Label
            {
                Text = "اكتب البرنامج هنا:",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Arial", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(5)
            };

            codeEditor = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 14),
                ScrollBars = ScrollBars.Both,
                AcceptsTab = true,
                RightToLeft = RightToLeft.Yes
            };

            leftPanel.Controls.Add(codeEditor);
            leftPanel.Controls.Add(codeLabel);

            // Right panel - Output areas
            var rightContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 300,
                RightToLeft = RightToLeft.Yes
            };

            // Graphics display area
            var graphicsPanel = new Panel { Dock = DockStyle.Fill };
            
            var graphicsLabel = new Label
            {
                Text = "شاشة الرسم:",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Arial", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(5)
            };

            graphicsDisplay = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Normal
            };

            graphicsPanel.Controls.Add(graphicsDisplay);
            graphicsPanel.Controls.Add(graphicsLabel);

            // Terminal area
            var terminalPanel = new Panel { Dock = DockStyle.Fill };
            
            var terminalLabel = new Label
            {
                Text = "النتائج:",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Arial", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(5)
            };

            terminal = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 12),
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.LightGreen,
                RightToLeft = RightToLeft.Yes
            };

            terminalPanel.Controls.Add(terminal);
            terminalPanel.Controls.Add(terminalLabel);

            // Status bar
            statusBar = new StatusStrip { RightToLeft = RightToLeft.Yes };
            statusLabel = new ToolStripStatusLabel("جاهز") { Spring = true, TextAlign = ContentAlignment.MiddleRight };
            statusBar.Items.Add(statusLabel);

            // Assemble the UI
            rightContainer.Panel1.Controls.Add(graphicsPanel);
            rightContainer.Panel2.Controls.Add(terminalPanel);
            
            mainContainer.Panel1.Controls.Add(leftPanel);
            mainContainer.Panel2.Controls.Add(rightContainer);

            this.Controls.Add(mainContainer);
            this.Controls.Add(toolbar);
            this.Controls.Add(statusBar);

            // Wire up events
            runButton.Click += RunButton_Click;
            clearButton.Click += ClearButton_Click;
            newButton.Click += NewButton_Click;
            openButton.Click += OpenButton_Click;
            saveButton.Click += SaveButton_Click;
            
            // Initialize graphics when form loads
            this.Load += MainForm_Load;
            graphicsDisplay.SizeChanged += GraphicsDisplay_SizeChanged;
        }

        private ToolStripButton CreateToolButton(string text, string tooltip)
        {
            return new ToolStripButton
            {
                Text = text,
                ToolTipText = tooltip,
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Font = new Font("Arial", 11),
                Padding = new Padding(10, 0, 10, 0)
            };
        }

        private void MainForm_Load(object? sender, EventArgs e)
        {
            InitializeGraphics();
        }

        private void InitializeGraphics()
        {
            // Create drawing surface matching the PictureBox size
            if (graphicsDisplay.Width > 0 && graphicsDisplay.Height > 0)
            {
                drawingSurface = new Bitmap(graphicsDisplay.Width, graphicsDisplay.Height);
                drawingGraphics = Graphics.FromImage(drawingSurface);
                drawingGraphics.Clear(Color.White);
                drawingGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphicsDisplay.Image = drawingSurface;
            }
        }

        private void GraphicsDisplay_SizeChanged(object? sender, EventArgs e)
        {
            // Recreate graphics surface when size changes
            if (graphicsDisplay.Width > 0 && graphicsDisplay.Height > 0)
            {
                var oldSurface = drawingSurface;
                InitializeGraphics();
                
                // Copy old content if exists
                if (oldSurface != null && drawingGraphics != null)
                {
                    drawingGraphics.DrawImage(oldSurface, 0, 0);
                    oldSurface.Dispose();
                }
                
                graphicsDisplay.Refresh();
            }
        }

        private void RunButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // Clear previous output
                terminal.Clear();
                statusLabel.Text = "جاري تشغيل البرنامج...";
                Application.DoEvents(); // Update UI
                
                // Get the code
                string sourceCode = codeEditor.Text;
                
                if (string.IsNullOrWhiteSpace(sourceCode))
                {
                    terminal.AppendText("لا يوجد برنامج للتشغيل!");
                    statusLabel.Text = "جاهز";
                    return;
                }
                
                // Create adapters for terminal and graphics
                terminalAdapter = new TerminalAdapter(terminal);
                graphicsAdapter = new GraphicsAdapter(this, drawingGraphics!, graphicsDisplay, colorMap);
                
                // Lexical analysis
                lexer = new Lexer(sourceCode);
                var tokens = lexer.ScanTokens();
                
                // Parsing
                parser = new Parser(tokens);
                var statements = parser.Parse();
                
                // Check for parse errors
                var parseErrors = parser.GetErrors();
                if (parseErrors.Count > 0)
                {
                    foreach (var error in parseErrors)
                    {
                        terminal.AppendText($"خطأ: {error.Message}\r\n");
                    }
                    statusLabel.Text = "يوجد أخطاء في البرنامج";
                    return;
                }
                
                // Interpretation
                interpreter = new Interpreter(terminalAdapter, graphicsAdapter);
                interpreter.Interpret(statements);
                
                statusLabel.Text = "تم تشغيل البرنامج بنجاح";
            }
            catch (Exception ex)
            {
                terminal.AppendText($"\r\nخطأ: {ex.Message}\r\n");
                statusLabel.Text = "حدث خطأ أثناء التشغيل";
            }
        }

        private void ClearButton_Click(object? sender, EventArgs e)
        {
            terminal.Clear();
            if (drawingGraphics != null)
            {
                drawingGraphics.Clear(Color.White);
                graphicsDisplay.Refresh();
            }
            statusLabel.Text = "تم مسح الشاشة";
        }

        private void NewButton_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(codeEditor.Text))
            {
                var result = MessageBox.Show(
                    "هل تريد حفظ البرنامج الحالي قبل إنشاء برنامج جديد؟",
                    "برنامج جديد",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                
                if (result == DialogResult.Yes)
                {
                    SaveButton_Click(sender, e);
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }
            
            codeEditor.Clear();
            terminal.Clear();
            if (drawingGraphics != null)
            {
                drawingGraphics.Clear(Color.White);
                graphicsDisplay.Refresh();
            }
            SetupDefaultProgram();
            statusLabel.Text = "برنامج جديد";
        }

        private void OpenButton_Click(object? sender, EventArgs e)
        {
            using (var openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "ملفات ضاد (*.dhad)|*.dhad|جميع الملفات (*.*)|*.*";
                openDialog.Title = "فتح برنامج ضاد";
                
                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        codeEditor.Text = System.IO.File.ReadAllText(openDialog.FileName);
                        statusLabel.Text = $"تم فتح: {System.IO.Path.GetFileName(openDialog.FileName)}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"خطأ في فتح الملف: {ex.Message}", "خطأ",
                            MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1,
                            MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                    }
                }
            }
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "ملفات ضاد (*.dhad)|*.dhad|جميع الملفات (*.*)|*.*";
                saveDialog.Title = "حفظ برنامج ضاد";
                saveDialog.DefaultExt = "dhad";
                
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        System.IO.File.WriteAllText(saveDialog.FileName, codeEditor.Text);
                        statusLabel.Text = $"تم الحفظ: {System.IO.Path.GetFileName(saveDialog.FileName)}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"خطأ في حفظ الملف: {ex.Message}", "خطأ",
                            MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1,
                            MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                    }
                }
            }
        }

        private void SetupDefaultProgram()
        {
            // Add a welcome program that demonstrates the language
            codeEditor.Text = @"؟؟ مرحباً بك في لغة ضاد!
؟؟ هذا برنامج بسيط يوضح إمكانيات اللغة

أظهر ""مرحباً بك في لغة البرمجة ضاد!"" سطر
أظهر ""دعنا نتعلم البرمجة معاً!"" سطر

؟؟ لنجرب بعض العمليات الحسابية
متغير س = 10
متغير ص = 20
أظهر ""س = "", س سطر
أظهر ""ص = "", ص سطر
أظهر ""س + ص = "", س + ص سطر

؟؟ لنرسم شيئاً جميلاً
القلم لونه = ""أزرق""
القلم عرضه = 3
ارسم دائرة 200، 200، 50

؟؟ جرب تغيير البرنامج وشاهد النتائج!";
        }
    }

    /// <summary>
    /// Adapter to connect the interpreter's output to the terminal TextBox
    /// </summary>
    public class TerminalAdapter : IOutput
    {
        private readonly TextBox terminal;
        private bool waitingForInput = false;
        private string? inputBuffer = null;

        public TerminalAdapter(TextBox terminal)
        {
            this.terminal = terminal;
        }

        public void Write(string text)
        {
            terminal.Invoke((MethodInvoker)(() => terminal.AppendText(text)));
        }

        public void WriteLine(string text)
        {
            terminal.Invoke((MethodInvoker)(() => terminal.AppendText(text + "\r\n")));
        }

        public string? ReadLine()
        {
            // For simplicity, we'll use an input dialog
            // In a more advanced version, you could implement inline input in the terminal
            string? result = null;
            
            terminal.Invoke((MethodInvoker)(() =>
            {
                using (var inputForm = new InputDialog())
                {
                    if (inputForm.ShowDialog() == DialogResult.OK)
                    {
                        result = inputForm.InputText;
                        terminal.AppendText(result + "\r\n");
                    }
                }
            }));
            
            return result ?? "";
        }
    }

    /// <summary>
    /// Adapter to connect the interpreter's graphics commands to Windows Forms drawing
    /// </summary>
    public class GraphicsAdapter : IGraphics
    {
        private readonly Form parentForm;
        private readonly Graphics graphics;
        private readonly PictureBox pictureBox;
        private readonly Dictionary<string, Color> colorMap;
        private Pen currentPen = new Pen(Color.Black, 1);
        private Brush currentBrush = Brushes.Black;
        private Font currentFont = new Font("Arial", 12);

        public GraphicsAdapter(Form parent, Graphics graphics, PictureBox pictureBox, Dictionary<string, Color> colorMap)
        {
            this.parentForm = parent;
            this.graphics = graphics;
            this.pictureBox = pictureBox;
            this.colorMap = colorMap;
        }

        public void ClearSurface()
        {
            parentForm.Invoke((MethodInvoker)(() =>
            {
                graphics.Clear(Color.White);
                pictureBox.Refresh();
            }));
        }

        public void SetPenColor(string colorName)
        {
            if (colorMap.TryGetValue(colorName, out Color color))
            {
                currentPen = new Pen(color, currentPen.Width);
                currentBrush = new SolidBrush(color);
            }
        }

        public void SetPenWidth(int width)
        {
            currentPen = new Pen(currentPen.Color, width);
        }

        public void DrawPoint(int x, int y)
        {
            parentForm.Invoke((MethodInvoker)(() =>
            {
                graphics.FillEllipse(currentBrush, x - 2, y - 2, 4, 4);
                pictureBox.Refresh();
            }));
        }

        public void DrawLine(int x1, int y1, int x2, int y2)
        {
            parentForm.Invoke((MethodInvoker)(() =>
            {
                graphics.DrawLine(currentPen, x1, y1, x2, y2);
                pictureBox.Refresh();
            }));
        }

        public void DrawCircle(int x, int y, int radius, string? fillColorName = null)
        {
            parentForm.Invoke((MethodInvoker)(() =>
            {
                int diameter = radius * 2;
                int topLeftX = x - radius;
                int topLeftY = y - radius;
                
                if (fillColorName != null && colorMap.TryGetValue(fillColorName, out Color fillColor))
                {
                    using (var fillBrush = new SolidBrush(fillColor))
                    {
                        graphics.FillEllipse(fillBrush, topLeftX, topLeftY, diameter, diameter);
                    }
                }
                
                graphics.DrawEllipse(currentPen, topLeftX, topLeftY, diameter, diameter);
                pictureBox.Refresh();
            }));
        }

        public void DrawRectangle(int x, int y, int width, int height, string? fillColorName = null)
        {
            parentForm.Invoke((MethodInvoker)(() =>
            {
                if (fillColorName != null && colorMap.TryGetValue(fillColorName, out Color fillColor))
                {
                    using (var fillBrush = new SolidBrush(fillColor))
                    {
                        graphics.FillRectangle(fillBrush, x, y, width, height);
                    }
                }
                
                graphics.DrawRectangle(currentPen, x, y, width, height);
                pictureBox.Refresh();
            }));
        }

        public void DrawText(int x, int y, string text)
        {
            parentForm.Invoke((MethodInvoker)(() =>
            {
                graphics.DrawString(text, currentFont, currentBrush, x, y);
                pictureBox.Refresh();
            }));
        }

        public void EnsureVisible()
        {
            // Graphics area is always visible in our design
        }
    }

    /// <summary>
    /// Simple input dialog for the أدخل (input) statement
    /// </summary>
    public class InputDialog : Form
    {
        private TextBox inputBox;
        private Button okButton;
        private Button cancelButton;

        public string InputText => inputBox.Text;

        public InputDialog()
        {
            this.Text = "إدخال";
            this.Size = new Size(400, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;

            var label = new Label
            {
                Text = "أدخل القيمة:",
                Location = new Point(12, 20),
                Size = new Size(360, 20),
                TextAlign = ContentAlignment.MiddleRight
            };

            inputBox = new TextBox
            {
                Location = new Point(12, 45),
                Size = new Size(360, 25),
                Font = new Font("Arial", 12),
                RightToLeft = RightToLeft.Yes
            };

            okButton = new Button
            {
                Text = "موافق",
                Location = new Point(297, 80),
                Size = new Size(75, 25),
                DialogResult = DialogResult.OK
            };

            cancelButton = new Button
            {
                Text = "إلغاء",
                Location = new Point(216, 80),
                Size = new Size(75, 25),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.AddRange(new Control[] { label, inputBox, okButton, cancelButton });
            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }
    }
}
