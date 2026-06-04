using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageMagick;

namespace ImagesForWebsites
{
    public partial class Form1 : Form
    {
        private Button btnSelectSourceFolder;
        private Button btnSelectOutputFolder;
        private Button btnProcess;
        private TextBox txtLog;

        private Label lblHeightInput;
        private NumericUpDown numHeightInput;
        private Button btnAddDimension;
        private Button btnRemoveDimension;
        private ListBox listDimensions;
        private Label lblDimensionsTitle;

        private Label lblPreviewTitle;
        private Button btnSelectAll;
        private Button btnDeselectAll;
        private FlowLayoutPanel flowLayoutPanelThumbnails;

        private string sourceDirectoryPath = string.Empty;
        private string outputDirectoryPath = string.Empty;
        private List<string> imageFilePaths = new List<string>();

        // Ukládání přiřazení: Výška -> Seznam souborů
        private Dictionary<uint, HashSet<string>> dimensionAssignments = new Dictionary<uint, HashSet<string>>();

        // Ukládání barev: Výška -> Barva
        private Dictionary<uint, Color> dimensionColors = new Dictionary<uint, Color>();

        // Předdefinovaná paleta jemných pastelových barev
        private readonly Color[] PresetColors = new Color[]
        {
            Color.FromArgb(200, 245, 200), // Zelená
            Color.FromArgb(200, 225, 255), // Modrá
            Color.FromArgb(255, 255, 200), // Žlutá
            Color.FromArgb(255, 205, 225), // Růžová
            Color.FromArgb(255, 220, 190), // Oranžová
            Color.FromArgb(230, 205, 255), // Fialová
            Color.FromArgb(200, 250, 245)  // Tyrkysová
        };
        private int colorIndex = 0;

        // Mapování prvků na soubor
        private Dictionary<string, CheckBox> fileToCheckValueMap = new Dictionary<string, CheckBox>();
        private Dictionary<string, Panel> fileToCardMap = new Dictionary<string, Panel>();
        private Dictionary<string, FlowLayoutPanel> fileToIndicatorsMap = new Dictionary<string, FlowLayoutPanel>();

        private bool isUpdatingUiState = false;

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Optimalizace obrázků pro web (Barevné rozměry)";
            this.Size = new Size(1000, 680);
            this.MinimumSize = new Size(950, 550);
            this.StartPosition = FormStartPosition.CenterScreen;

            // --- LEVÝ OVLÁDACÍ PANEL ---

            btnSelectSourceFolder = new Button
            {
                Text = "1. Vybrat zdrojovou složku",
                Location = new Point(20, 15),
                Size = new Size(280, 30)
            };
            btnSelectSourceFolder.Click += BtnSelectSourceFolder_Click;

            lblSourceFolderStatus = new Label
            {
                Text = "Zdrojová složka nevybrána",
                Location = new Point(20, 48),
                Size = new Size(280, 15),
                ForeColor = Color.DarkRed
            };

            lblSourceSize = new Label
            {
                Text = "",
                Location = new Point(20, 63),
                Size = new Size(280, 15),
                ForeColor = Color.Gray,
                Font = new Font(this.Font.FontFamily, 7.5f)
            };

            btnSelectOutputFolder = new Button
            {
                Text = "2. Vybrat výstupní složku",
                Location = new Point(20, 85),
                Size = new Size(280, 30)
            };
            btnSelectOutputFolder.Click += BtnSelectOutputFolder_Click;

            lblOutputFolderStatus = new Label
            {
                Text = "Výstupní složka nevybrána",
                Location = new Point(20, 118),
                Size = new Size(280, 15),
                ForeColor = Color.DarkRed
            };

            lblDimensionsTitle = new Label
            {
                Text = "3. Správa cílových výšek (px):",
                Location = new Point(20, 145),
                Size = new Size(280, 20),
                Font = new Font(this.Font, FontStyle.Bold)
            };

            numHeightInput = new NumericUpDown
            {
                Location = new Point(20, 170),
                Size = new Size(80, 20),
                Minimum = 10,
                Maximum = 10000,
                Value = 600
            };

            btnAddDimension = new Button
            {
                Text = "Přidat",
                Location = new Point(110, 167),
                Size = new Size(80, 25)
            };
            btnAddDimension.Click += BtnAddDimension_Click;

            btnRemoveDimension = new Button
            {
                Text = "Odebrat",
                Location = new Point(200, 167),
                Size = new Size(100, 25)
            };
            btnRemoveDimension.Click += BtnRemoveDimension_Click;

            listDimensions = new ListBox
            {
                Location = new Point(20, 200),
                Size = new Size(280, 95),
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 18
            };
            listDimensions.SelectedIndexChanged += ListDimensions_SelectedIndexChanged;
            listDimensions.DrawItem += ListDimensions_DrawItem;

            btnProcess = new Button
            {
                Text = "Spustit převod vybraných",
                Location = new Point(20, 310),
                Size = new Size(280, 35),
                BackColor = Color.LightGreen
            };
            btnProcess.Click += BtnProcess_Click;

            txtLog = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(20, 360),
                Size = new Size(280, 260),
                Font = new Font("Consolas", 8)
            };

            // --- PRAVÝ PANEL PRO MINIATURY ---

            lblPreviewTitle = new Label
            {
                Text = "Přiřazení obrázků k vybranému rozměru:",
                Location = new Point(320, 18),
                Size = new Size(380, 20),
                Font = new Font(this.Font, FontStyle.Bold)
            };

            btnSelectAll = new Button
            {
                Text = "Vybrat všechny",
                Location = new Point(710, 13),
                Size = new Size(120, 25),
                Enabled = false
            };
            btnSelectAll.Click += BtnSelectAll_Click;

            btnDeselectAll = new Button
            {
                Text = "Odebrat všechny",
                Location = new Point(835, 13),
                Size = new Size(120, 25),
                Enabled = false
            };
            btnDeselectAll.Click += BtnDeselectAll_Click;

            flowLayoutPanelThumbnails = new FlowLayoutPanel
            {
                Location = new Point(320, 45),
                Size = new Size(640, 575),
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            this.Controls.Add(btnSelectSourceFolder);
            this.Controls.Add(lblSourceFolderStatus);
            this.Controls.Add(lblSourceSize);
            this.Controls.Add(btnSelectOutputFolder);
            this.Controls.Add(lblOutputFolderStatus);
            this.Controls.Add(lblDimensionsTitle);
            this.Controls.Add(numHeightInput);
            this.Controls.Add(btnAddDimension);
            this.Controls.Add(btnRemoveDimension);
            this.Controls.Add(listDimensions);
            this.Controls.Add(btnProcess);
            this.Controls.Add(txtLog);
            this.Controls.Add(lblPreviewTitle);
            this.Controls.Add(btnSelectAll);
            this.Controls.Add(btnDeselectAll);
            this.Controls.Add(flowLayoutPanelThumbnails);
        }

        private Label lblSourceFolderStatus;
        private Label lblSourceSize;
        private Label lblOutputFolderStatus;

        private void ListDimensions_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            e.DrawBackground();

            string text = listDimensions.Items[e.Index].ToString();
            uint height = GetHeightFromText(text);

            if (dimensionColors.TryGetValue(height, out Color color))
            {
                using (SolidBrush brush = new SolidBrush(color))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds.X + 5, e.Bounds.Y + 3, 12, 12);
                }
                e.Graphics.DrawRectangle(Pens.Gray, e.Bounds.X + 5, e.Bounds.Y + 3, 12, 12);
            }

            using (Brush textBrush = new SolidBrush(e.ForeColor))
            {
                e.Graphics.DrawString(text, e.Font, textBrush, e.Bounds.X + 25, e.Bounds.Y + 1);
            }

            e.DrawFocusRectangle();
        }

        private async void BtnSelectSourceFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    sourceDirectoryPath = fbd.SelectedPath;
                    lblSourceFolderStatus.Text = "Zdroj: " + TruncatePath(sourceDirectoryPath, 35);
                    lblSourceFolderStatus.ForeColor = Color.DarkGreen;

                    LogMessage($"Zvolen zdrojový adresář: {sourceDirectoryPath}");

                    SetUiEnabled(false);
                    await LoadThumbnailsAsync(sourceDirectoryPath);
                    SetUiEnabled(true);
                }
            }
        }

        private void BtnSelectOutputFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    outputDirectoryPath = fbd.SelectedPath;
                    lblOutputFolderStatus.Text = "Výstup: " + TruncatePath(outputDirectoryPath, 35);
                    lblOutputFolderStatus.ForeColor = Color.DarkGreen;

                    LogMessage($"Zvolen výstupní adresář: {outputDirectoryPath}");
                }
            }
        }

        private void BtnAddDimension_Click(object sender, EventArgs e)
        {
            uint height = (uint)numHeightInput.Value;
            if (dimensionAssignments.ContainsKey(height))
            {
                MessageBox.Show("Tento rozměr již v seznamu existuje.", "Upozornění", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Color assignedColor = PresetColors[colorIndex % PresetColors.Length];
            colorIndex++;

            dimensionAssignments.Add(height, new HashSet<string>());
            dimensionColors.Add(height, assignedColor);

            listDimensions.Items.Add($"{height} px");
            listDimensions.SelectedIndex = listDimensions.Items.Count - 1;

            LogMessage($"Přidán rozměr {height} px.");
        }

        private void BtnRemoveDimension_Click(object sender, EventArgs e)
        {
            if (listDimensions.SelectedItem == null) return;

            uint selectedHeight = GetSelectedHeight();
            dimensionAssignments.Remove(selectedHeight);
            dimensionColors.Remove(selectedHeight);

            int index = listDimensions.SelectedIndex;
            listDimensions.Items.RemoveAt(index);

            if (listDimensions.Items.Count > 0)
                listDimensions.SelectedIndex = Math.Max(0, index - 1);
            else
                UpdateThumbnailsCheckboxesState();

            LogMessage($"Odebrán rozměr: {selectedHeight} px");
        }

        private void ListDimensions_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateThumbnailsCheckboxesState();
        }

        // Hromadný výběr všech obrázků pro aktuální výšku
        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            uint selectedHeight = GetSelectedHeight();
            if (selectedHeight == 0) return;

            foreach (var filePath in imageFilePaths)
            {
                dimensionAssignments[selectedHeight].Add(filePath);
            }

            LogMessage($"Vybrány všechny obrázky ({imageFilePaths.Count}) pro rozměr {selectedHeight} px.");
            UpdateThumbnailsCheckboxesState();
        }

        // Hromadné odebrání všech obrázků pro aktuální výšku
        private void BtnDeselectAll_Click(object sender, EventArgs e)
        {
            uint selectedHeight = GetSelectedHeight();
            if (selectedHeight == 0) return;

            dimensionAssignments[selectedHeight].Clear();

            LogMessage($"Zrušen výběr všech obrázků pro rozměr {selectedHeight} px.");
            UpdateThumbnailsCheckboxesState();
        }

        private async Task LoadThumbnailsAsync(string folderPath)
        {
            ClearExistingThumbnails();
            LogMessage("Načítání obrázků...");

            string[] extensions = { "*.png", "*.jpg", "*.jpeg" };
            imageFilePaths.Clear();

            long totalBytes = 0;

            foreach (var ext in extensions)
            {
                try
                {
                    var files = Directory.GetFiles(folderPath, ext);
                    imageFilePaths.AddRange(files);

                    foreach (var file in files)
                    {
                        totalBytes += new FileInfo(file).Length;
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"Chyba při čtení složky: {ex.Message}");
                }
            }

            lblSourceSize.Text = $"Celkem: {imageFilePaths.Count} souborů, {FormatBytes(totalBytes)}";

            foreach (var key in dimensionAssignments.Keys.ToList())
            {
                dimensionAssignments[key] = new HashSet<string>();
            }

            if (imageFilePaths.Count == 0)
            {
                LogMessage("Nebyly nalezeny žádné podporované obrázky.");
                return;
            }

            foreach (var file in imageFilePaths)
            {
                long fileBytes = new FileInfo(file).Length;

                // Velikost karty upravena na 135x190 px, aby se vešel popisek velikosti
                Panel card = new Panel
                {
                    Size = new Size(135, 190),
                    Margin = new Padding(5),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.GhostWhite
                };

                PictureBox pb = new PictureBox
                {
                    Size = new Size(125, 90),
                    Location = new Point(4, 4),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.LightGray
                };

                CheckBox cb = new CheckBox
                {
                    Location = new Point(4, 98),
                    Size = new Size(125, 20),
                    Text = "Vybrat pro rozměr",
                    Font = new Font(this.Font.FontFamily, 7, FontStyle.Bold),
                    Tag = file,
                    Enabled = false
                };
                cb.CheckedChanged += Thumbnail_CheckedChanged;

                Label lblName = new Label
                {
                    Size = new Size(125, 16),
                    Location = new Point(4, 118),
                    Text = Path.GetFileName(file),
                    TextAlign = ContentAlignment.TopCenter,
                    Font = new Font(this.Font.FontFamily, 7)
                };

                Label lblSize = new Label
                {
                    Size = new Size(125, 14),
                    Location = new Point(4, 134),
                    Text = FormatBytes(fileBytes),
                    TextAlign = ContentAlignment.TopCenter,
                    ForeColor = Color.Gray,
                    Font = new Font(this.Font.FontFamily, 7)
                };

                FlowLayoutPanel flowIndicators = new FlowLayoutPanel
                {
                    Size = new Size(125, 16),
                    Location = new Point(4, 152),
                    WrapContents = false,
                    FlowDirection = FlowDirection.LeftToRight,
                    BackColor = Color.Transparent
                };

                card.Controls.Add(pb);
                card.Controls.Add(cb);
                card.Controls.Add(lblName);
                card.Controls.Add(lblSize);
                card.Controls.Add(flowIndicators);
                flowLayoutPanelThumbnails.Controls.Add(card);

                fileToCheckValueMap[file] = cb;
                fileToCardMap[file] = card;
                fileToIndicatorsMap[file] = flowIndicators;

                try
                {
                    Image thumb = await Task.Run(() => CreateSafeThumbnail(file, 125, 90));
                    if (thumb != null)
                    {
                        pb.Image = thumb;
                        pb.BackColor = Color.White;
                    }
                }
                catch
                {
                    pb.BackColor = Color.LightCoral;
                }
            }

            UpdateThumbnailsCheckboxesState();
            LogMessage($"Nalezeno {imageFilePaths.Count} obrázků.");
        }

        private void Thumbnail_CheckedChanged(object sender, EventArgs e)
        {
            if (isUpdatingUiState) return;

            CheckBox cb = sender as CheckBox;
            if (cb == null) return;

            string filePath = cb.Tag as string;
            if (string.IsNullOrEmpty(filePath)) return;

            uint selectedHeight = GetSelectedHeight();
            if (selectedHeight == 0) return;

            if (cb.Checked)
                dimensionAssignments[selectedHeight].Add(filePath);
            else
                dimensionAssignments[selectedHeight].Remove(filePath);

            UpdateCardAppearance(filePath);
        }

        private void UpdateThumbnailsCheckboxesState()
        {
            isUpdatingUiState = true;

            uint selectedHeight = GetSelectedHeight();
            bool hasSelectedHeight = selectedHeight > 0;

            btnSelectAll.Enabled = hasSelectedHeight && imageFilePaths.Count > 0;
            btnDeselectAll.Enabled = hasSelectedHeight && imageFilePaths.Count > 0;

            foreach (var filePath in imageFilePaths)
            {
                if (fileToCheckValueMap.ContainsKey(filePath))
                {
                    CheckBox cb = fileToCheckValueMap[filePath];
                    cb.Enabled = hasSelectedHeight;

                    if (hasSelectedHeight)
                        cb.Checked = dimensionAssignments[selectedHeight].Contains(filePath);
                    else
                        cb.Checked = false;

                    UpdateCardAppearance(filePath);
                }
            }

            isUpdatingUiState = false;
        }

        private void UpdateCardAppearance(string filePath)
        {
            if (!fileToCardMap.ContainsKey(filePath) || !fileToCheckValueMap.ContainsKey(filePath) || !fileToIndicatorsMap.ContainsKey(filePath)) return;

            Panel card = fileToCardMap[filePath];
            CheckBox cb = fileToCheckValueMap[filePath];
            FlowLayoutPanel indicatorsPanel = fileToIndicatorsMap[filePath];

            uint selectedHeight = GetSelectedHeight();

            if (cb.Checked && dimensionColors.TryGetValue(selectedHeight, out Color activeColor))
                card.BackColor = activeColor;
            else
                card.BackColor = Color.GhostWhite;

            indicatorsPanel.Controls.Clear();
            foreach (var kvp in dimensionAssignments)
            {
                uint height = kvp.Key;
                HashSet<string> files = kvp.Value;

                if (files.Contains(filePath) && dimensionColors.TryGetValue(height, out Color color))
                {
                    Panel dot = new Panel
                    {
                        Size = new Size(12, 12),
                        Margin = new Padding(2, 0, 2, 0),
                        BackColor = color,
                        BorderStyle = BorderStyle.FixedSingle
                    };

                    ToolTip tt = new ToolTip();
                    tt.SetToolTip(dot, $"{height} px");

                    indicatorsPanel.Controls.Add(dot);
                }
            }
        }

        private uint GetSelectedHeight()
        {
            if (listDimensions.SelectedItem == null) return 0;
            return GetHeightFromText(listDimensions.SelectedItem.ToString());
        }

        private uint GetHeightFromText(string text)
        {
            string numericPart = text.Replace(" px", "").Trim();
            if (uint.TryParse(numericPart, out uint result))
            {
                return result;
            }
            return 0;
        }

        private Image CreateSafeThumbnail(string path, int width, int height)
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (var original = Image.FromStream(stream, false, false))
                {
                    return original.GetThumbnailImage(width, height, () => false, IntPtr.Zero);
                }
            }
            catch
            {
                return null;
            }
        }

        private void ClearExistingThumbnails()
        {
            foreach (Control control in flowLayoutPanelThumbnails.Controls)
            {
                if (control is Panel card)
                {
                    foreach (Control subControl in card.Controls)
                    {
                        if (subControl is PictureBox pb && pb.Image != null)
                        {
                            pb.Image.Dispose();
                        }
                    }
                }
            }
            flowLayoutPanelThumbnails.Controls.Clear();
            fileToCheckValueMap.Clear();
            fileToCardMap.Clear();
            fileToIndicatorsMap.Clear();
        }

        private async void BtnProcess_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(outputDirectoryPath) || !Directory.Exists(outputDirectoryPath))
            {
                MessageBox.Show("Vyberte platný výstupní adresář.", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int totalToProcess = dimensionAssignments.Values.Sum(set => set.Count);
            if (totalToProcess == 0)
            {
                MessageBox.Show("Přiřaďte alespoň jeden obrázek k nějakému rozměru.", "Upozornění", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool willOverwrite = DetectOverwrites();
            if (willOverwrite)
            {
                var dialogResult = MessageBox.Show(
                    "Některé obrázky jsou přiřazeny k více rozměrům najednou. " +
                    "Protože požadujete zachování stejného názvu, různé rozměry stejného souboru se navzájem přepíší. " +
                    "Chcete přesto pokračovat?",
                    "Upozornění na přepsání souborů",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (dialogResult == DialogResult.No) return;
            }

            SetUiEnabled(false);
            LogMessage($"Spuštěn převod {totalToProcess} úloh...");

            await Task.Run(() => ProcessAssignedImages());

            SetUiEnabled(true);
        }

        private bool DetectOverwrites()
        {
            HashSet<string> processedFileNames = new HashSet<string>();
            foreach (var kvp in dimensionAssignments)
            {
                foreach (var filePath in kvp.Value)
                {
                    string name = Path.GetFileNameWithoutExtension(filePath).ToLower();
                    if (processedFileNames.Contains(name))
                    {
                        return true;
                    }
                    processedFileNames.Add(name);
                }
            }
            return false;
        }

        private void ProcessAssignedImages()
        {
            uint quality = 80;

            long totalInputBytes = 0;
            long totalOutputBytes = 0;

            foreach (var assignment in dimensionAssignments)
            {
                uint targetHeight = assignment.Key;
                HashSet<string> files = assignment.Value;

                if (files.Count == 0) continue;

                this.Invoke(new Action(() => LogMessage($"Zpracování rozměru {targetHeight} px...")));

                foreach (var filePath in files)
                {
                    try
                    {
                        string fileName = Path.GetFileNameWithoutExtension(filePath);
                        string outputFileName = $"{fileName}.webp";
                        string outputPath = Path.Combine(outputDirectoryPath, outputFileName);

                        // Připočtení velikosti vstupního souboru
                        FileInfo inputInfo = new FileInfo(filePath);
                        totalInputBytes += inputInfo.Length;

                        using (var image = new MagickImage(filePath))
                        {
                            var size = new MagickGeometry(0, targetHeight);
                            image.Resize(size);

                            image.Format = MagickFormat.WebP;
                            image.Quality = quality;

                            image.Write(outputPath);
                        }

                        // Připočtení velikosti výstupního souboru
                        FileInfo outputInfo = new FileInfo(outputPath);
                        totalOutputBytes += outputInfo.Length;

                        this.Invoke(new Action(() => LogMessage($"Uloženo: {outputFileName} (výška {targetHeight}px)")));
                    }
                    catch (Exception ex)
                    {
                        this.Invoke(new Action(() => LogMessage($"Chyba u {Path.GetFileName(filePath)} ({targetHeight}px): {ex.Message}")));
                    }
                }
            }

            // Výpočet úspory
            this.Invoke(new Action(() => {
                LogMessage("---------------------------------------------");
                LogMessage($"Původní velikost: {FormatBytes(totalInputBytes)}");
                LogMessage($"Nová velikost (WebP): {FormatBytes(totalOutputBytes)}");

                if (totalOutputBytes > 0)
                {
                    double ratio = (double)totalInputBytes / totalOutputBytes;
                    LogMessage($"Výsledné soubory jsou {ratio:0.##}x menší.");
                }
                LogMessage("---------------------------------------------");
            }));
        }

        // Pomocná metoda pro formátování bajtů na čitelné jednotky
        private string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            double doubleBytes = bytes;
            int index = 0;
            while (doubleBytes >= 1024 && index < suffixes.Length - 1)
            {
                doubleBytes /= 1024;
                index++;
            }
            return $"{doubleBytes:0.##} {suffixes[index]}";
        }

        private string TruncatePath(string path, int maxLength)
        {
            if (string.IsNullOrEmpty(path) || path.Length <= maxLength)
                return path;

            return "..." + path.Substring(path.Length - maxLength);
        }

        private void LogMessage(string message)
        {
            txtLog.AppendText($"[{DateTime.Now.ToString("HH:mm:ss")}] {message}{Environment.NewLine}");
        }

        private void SetUiEnabled(bool enabled)
        {
            btnSelectSourceFolder.Enabled = enabled;
            btnSelectOutputFolder.Enabled = enabled;
            btnProcess.Enabled = enabled;
            btnAddDimension.Enabled = enabled;
            btnRemoveDimension.Enabled = enabled;
            listDimensions.Enabled = enabled;
            numHeightInput.Enabled = enabled;

            // Aktivovat hromadná tlačítka pouze pokud máme aktivní rozměr
            uint selectedHeight = GetSelectedHeight();
            btnSelectAll.Enabled = enabled && selectedHeight > 0 && imageFilePaths.Count > 0;
            btnDeselectAll.Enabled = enabled && selectedHeight > 0 && imageFilePaths.Count > 0;
        }
    }
}