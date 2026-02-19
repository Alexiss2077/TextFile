using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text;
using D = DocumentFormat.OpenXml.Drawing;
using SsFont = DocumentFormat.OpenXml.Spreadsheet.Font;
using SsColor = DocumentFormat.OpenXml.Spreadsheet.Color;
using P = DocumentFormat.OpenXml.Presentation;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace TextFileControl
{
    public partial class Form1 : Form
    {
        // DataTable almacenado como campo ? thread-safe para lectura desde hilos de fondo
        private System.Data.DataTable? _dataTable;

        // Límites para formatos que no escalan bien con tablas grandes
        private const int LimiteWord = 5_000;
        private const int LimitePpt = 1_000;

        public Form1()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint, true);

            // Double buffer en DataGridView (no tiene propiedad pública)
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.SetProperty,
                null, dataGridView1, new object[] { true });
        }

        // ?????????????????????????????????????????????
        //  CARGAR ARCHIVO
        // ?????????????????????????????????????????????
        private async void btnLoadFile_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new()
            {
                Title = "Seleccionar archivo de texto",
                Filter = "Archivos de texto (*.txt;*.csv;*.tsv)|*.txt;*.csv;*.tsv|Todos (*.*)|*.*"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            SetControlesHabilitados(false);
            btnLoadFile.Text = "[ Cargando... ]";

            try
            {
                string filePath = ofd.FileName;
                var dt = await Task.Run(() => CargarArchivo(filePath));

                _dataTable = dt;

                dataGridView1.SuspendLayout();
                dataGridView1.DataSource = null;
                dataGridView1.Columns.Clear();
                dataGridView1.DataSource = dt;

                int maxCols = Math.Min(dataGridView1.Columns.Count, 30);
                for (int c = 0; c < maxCols; c++)
                    dataGridView1.AutoResizeColumn(c, DataGridViewAutoSizeColumnMode.DisplayedCells);

                dataGridView1.ResumeLayout();

                btnLoadFile.Text = $"{Path.GetFileName(filePath)}  ({dt.Rows.Count:N0} filas · {dt.Columns.Count} columnas)";
            }
            catch (Exception ex)
            {
                btnLoadFile.Text = "Cargar Archivo";
                MessageBox.Show($"Error al cargar el archivo:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { SetControlesHabilitados(true); }
        }

        private static System.Data.DataTable CargarArchivo(string filePath)
        {
            var lines = new List<string>(capacity: 50_000);
            using (var sr = new StreamReader(filePath, System.Text.Encoding.UTF8))
            {
                string? line;
                while ((line = sr.ReadLine()) != null)
                    lines.Add(line);
            }

            if (lines.Count == 0) throw new InvalidDataException("El archivo está vacío.");

            char sep = DetectSeparator(lines[0]);
            string[] headers = lines[0].Split(sep);

            var dt = new System.Data.DataTable();
            dt.BeginLoadData();
            foreach (string h in headers) dt.Columns.Add(h.Trim());

            int colCount = dt.Columns.Count;
            for (int i = 1; i < lines.Count; i++)
            {
                string l = lines[i];
                if (string.IsNullOrWhiteSpace(l)) continue;
                string[] fields = l.Split(sep);
                var row = dt.NewRow();
                for (int j = 0; j < colCount; j++)
                    row[j] = j < fields.Length ? fields[j].Trim() : string.Empty;
                dt.Rows.Add(row);
            }

            dt.EndLoadData();
            dt.AcceptChanges();
            return dt;
        }

        private static char DetectSeparator(string line)
        {
            foreach (char c in new[] { ',', ';', '\t', '|' })
                if (line.Contains(c)) return c;
            return ',';
        }

        // ?????????????????????????????????????????????
        //  VALIDACIÓN COMÚN
        // ?????????????????????????????????????????????
        private bool ValidarDatos()
        {
            if (_dataTable == null || _dataTable.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos. Cargue un archivo primero.",
                    "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private static string? GuardarComo(string filtro, string extension)
        {
            using SaveFileDialog sfd = new()
            {
                Filter = filtro,
                DefaultExt = extension,
                FileName = $"Exportacion_{DateTime.Now:yyyyMMdd_HHmmss}"
            };
            return sfd.ShowDialog() == DialogResult.OK ? sfd.FileName : null;
        }

        private void SetControlesHabilitados(bool v)
        {
            btnLoadFile.Enabled = v;
            btnExportExcel.Enabled = v;
            btnExportWord.Enabled = v;
            btnExportPowerPoint.Enabled = v;
            btnExportCsv.Enabled = v;
        }

        private void IniciarExport(string nombre, int filas)
        {
            SetControlesHabilitados(false);
            btnLoadFile.Text = $"[ Exportando {nombre} ({filas:N0} filas)... ]";
        }

        private void FinalizarExport()
        {
            SetControlesHabilitados(true);
            btnLoadFile.Text = _dataTable != null
                ? $" ({_dataTable.Rows.Count:N0} filas · {_dataTable.Columns.Count} columnas)"
                : "Cargar Archivo";
        }

        // ?????????????????????????????????????????????
        //  EXPORTAR ? EXCEL  (SAX / streaming OpenXML)
        //  Sin ClosedXML, sin DOM en RAM, sin AdjustToContents
        // ?????????????????????????????????????????????
        private async void btnExportExcel_Click(object sender, EventArgs e)
        {
            if (!ValidarDatos()) return;
            string? path = GuardarComo("Excel (*.xlsx)|*.xlsx", "xlsx");
            if (path == null) return;

            var dt = _dataTable!;
            IniciarExport("Excel", dt.Rows.Count);

            try
            {
                await Task.Run(() => ExportarExcelSax(path, dt));
                MostrarExito(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar Excel:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { FinalizarExport(); }
        }

        /// <summary>
        /// Escribe el xlsx usando SAX (OpenXmlWriter) — streaming puro.
        /// No construye ningún DOM en memoria: cada celda se escribe directamente al ZIP.
        /// Rendimiento: ~200k filas/seg en hardware moderado.
        /// </summary>
        private static void ExportarExcelSax(string path, System.Data.DataTable dt)
        {
            // Paleta de colores encabezado (fondo verde oscuro, texto blanco)
            const string headerFill = "FF217346"; // ARGB
            const string headerFont = "FFFFFFFF";
            const string evenFill = "FFFFFFFF";
            const string oddFill = "FFE8F5E9";

            using var doc = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook);

            // ?? WorkbookPart ??
            var wbPart = doc.AddWorkbookPart();
            wbPart.Workbook = new Workbook();

            // ?? Styles (mínimo necesario para encabezados coloreados) ??
            var stylesPart = wbPart.AddNewPart<WorkbookStylesPart>();
            stylesPart.Stylesheet = CrearEstilos(headerFill, headerFont, evenFill, oddFill);
            stylesPart.Stylesheet.Save();

            // ?? SharedStrings (opcional pero acelera archivos con texto repetido) ??
            // Para máxima velocidad escribimos inline strings y lo omitimos.

            // ?? Worksheet vía SAX ??
            var wsPart = wbPart.AddNewPart<WorksheetPart>();
            using (var writer = OpenXmlWriter.Create(wsPart))
            {
                writer.WriteStartElement(new Worksheet());
                writer.WriteStartElement(new SheetData());

                int colCount = dt.Columns.Count;

                // Fila 1: encabezados  (styleIndex = 1)
                writer.WriteStartElement(new Row());
                for (int c = 0; c < colCount; c++)
                    EscribirCeldaSax(writer, 1, c, dt.Columns[c].ColumnName, styleIndex: 1);
                writer.WriteEndElement(); // Row

                // Filas de datos
                for (int r = 0; r < dt.Rows.Count; r++)
                {
                    uint styleIdx = (r % 2 == 0) ? 2u : 3u; // even / odd
                    writer.WriteStartElement(new Row());
                    for (int c = 0; c < colCount; c++)
                        EscribirCeldaSax(writer, r + 2, c, dt.Rows[r][c]?.ToString() ?? "", styleIdx);
                    writer.WriteEndElement(); // Row
                }

                writer.WriteEndElement(); // SheetData
                writer.WriteEndElement(); // Worksheet
            }

            // ?? Relacionar hoja con workbook ??
            var sheets = wbPart.Workbook.AppendChild(new Sheets());
            sheets.AppendChild(new Sheet
            {
                Id = wbPart.GetIdOfPart(wsPart),
                SheetId = 1,
                Name = "Datos"
            });
            wbPart.Workbook.Save();
        }

        private static void EscribirCeldaSax(OpenXmlWriter writer, int row, int col,
                                              string valor, uint styleIndex)
        {
            // Referencia de celda: A1, B2, etc.
            string colRef = ColLetra(col);
            string cellRef = $"{colRef}{row}";

            var attrs = new List<OpenXmlAttribute>
            {
                new OpenXmlAttribute("r", null, cellRef),
                new OpenXmlAttribute("s", null, styleIndex.ToString()),
                new OpenXmlAttribute("t", null, "inlineStr")
            };

            writer.WriteStartElement(new Cell(), attrs);
            writer.WriteStartElement(new InlineString());
            writer.WriteElement(new Text(valor));
            writer.WriteEndElement(); // InlineString
            writer.WriteEndElement(); // Cell
        }

        private static string ColLetra(int idx)
        {
            string col = "";
            idx++; // 1-based
            while (idx > 0)
            {
                int rem = (idx - 1) % 26;
                col = (char)('A' + rem) + col;
                idx = (idx - 1) / 26;
            }
            return col;
        }

        private static Stylesheet CrearEstilos(string headerFill, string headerFont,
                                               string evenFill, string oddFill)
        {
            // Índices de estilos que usamos:
            //   0 = default, 1 = header, 2 = even row, 3 = odd row

            var fills = new Fills(
                new Fill(new PatternFill { PatternType = PatternValues.None }),           // 0 req
                new Fill(new PatternFill { PatternType = PatternValues.Gray125 }),        // 1 req
                new Fill(new PatternFill(                                                 // 2 header
                    new ForegroundColor { Rgb = headerFill })
                { PatternType = PatternValues.Solid }),
                new Fill(new PatternFill(                                                 // 3 even
                    new ForegroundColor { Rgb = evenFill })
                { PatternType = PatternValues.Solid }),
                new Fill(new PatternFill(                                                 // 4 odd
                    new ForegroundColor { Rgb = oddFill })
                { PatternType = PatternValues.Solid })
            );

            var fonts = new Fonts(
                new SsFont(),                                                                      // 0 default
                new SsFont(new Bold(), new SsColor { Rgb = headerFont })                           // 1 header
            );

            var borders = new Borders(new Border());

            var cellFormats = new CellFormats(
                new CellFormat(),                                      // 0 default
                new CellFormat { FontId = 1, FillId = 2, ApplyFont = true, ApplyFill = true }, // 1 header
                new CellFormat { FillId = 3, ApplyFill = true },      // 2 even row
                new CellFormat { FillId = 4, ApplyFill = true }       // 3 odd row
            );

            return new Stylesheet(fonts, fills, borders, cellFormats);
        }

        // ?????????????????????????????????????????????
        //  EXPORTAR ? WORD
        // ?????????????????????????????????????????????
        private async void btnExportWord_Click(object sender, EventArgs e)
        {
            if (!ValidarDatos()) return;

            var dt = _dataTable!;

            // Word no escala bien con tablas enormes — avisar si supera el límite
            int filas = Math.Min(dt.Rows.Count, LimiteWord);
            if (dt.Rows.Count > LimiteWord)
            {
                var resp = MessageBox.Show(
                    $"El documento tiene {dt.Rows.Count:N0} filas.\n" +
                    $"Word puede volverse muy lento con tablas grandes.\n\n" +
                    $"Se exportarán solo las primeras {LimiteWord:N0} filas.\n\n" +
                    "¿Continuar?",
                    "Advertencia", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (resp != DialogResult.Yes) return;
            }

            string? path = GuardarComo("Word (*.docx)|*.docx", "docx");
            if (path == null) return;

            IniciarExport("Word", filas);

            try
            {
                string fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                await Task.Run(() => ExportarWord(path, dt, filas, fecha));
                MostrarExito(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar Word:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { FinalizarExport(); }
        }

        private static void ExportarWord(string path, System.Data.DataTable dt, int filas, string fecha)
        {
            using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new W.Document();
            var body = mainPart.Document.AppendChild(new W.Body());

            // Título
            var tp = body.AppendChild(new W.Paragraph());
            tp.ParagraphProperties = new W.ParagraphProperties
            { Justification = new W.Justification { Val = W.JustificationValues.Center } };
            var tr = tp.AppendChild(new W.Run());
            tr.RunProperties = new W.RunProperties { Bold = new W.Bold(), FontSize = new W.FontSize { Val = "36" } };
            tr.AppendChild(new W.Text("Exportación de Datos"));

            // Fecha
            var dp = body.AppendChild(new W.Paragraph());
            dp.ParagraphProperties = new W.ParagraphProperties
            {
                Justification = new W.Justification { Val = W.JustificationValues.Center },
                SpacingBetweenLines = new W.SpacingBetweenLines { After = "200" }
            };
            var dr = dp.AppendChild(new W.Run());
            dr.RunProperties = new W.RunProperties { Italic = new W.Italic() };
            dr.AppendChild(new W.Text($"Generado el: {fecha}  —  {filas:N0} filas"));

            // Tabla
            var table = body.AppendChild(new W.Table());
            table.AppendChild(new W.TableProperties(
                new W.TableWidth { Width = "5000", Type = W.TableWidthUnitValues.Pct },
                new W.TableBorders(
                    new W.TopBorder { Val = W.BorderValues.Single, Size = 4 },
                    new W.BottomBorder { Val = W.BorderValues.Single, Size = 4 },
                    new W.LeftBorder { Val = W.BorderValues.Single, Size = 4 },
                    new W.RightBorder { Val = W.BorderValues.Single, Size = 4 },
                    new W.InsideHorizontalBorder { Val = W.BorderValues.Single, Size = 4 },
                    new W.InsideVerticalBorder { Val = W.BorderValues.Single, Size = 4 })));

            // Encabezados
            var hRow = new W.TableRow();
            for (int c = 0; c < dt.Columns.Count; c++)
            {
                var tc = WordCelda(dt.Columns[c].ColumnName, bold: true, fill: "2B579A", color: "FFFFFF");
                hRow.AppendChild(tc);
            }
            table.AppendChild(hRow);

            // Datos (hasta el límite)
            for (int r = 0; r < filas; r++)
            {
                bool par = r % 2 == 0;
                var dRow = new W.TableRow();
                for (int c = 0; c < dt.Columns.Count; c++)
                {
                    string val = dt.Rows[r][c]?.ToString() ?? "";
                    dRow.AppendChild(WordCelda(val, bold: false,
                        fill: par ? "FFFFFF" : "E8F0FE", color: "000000"));
                }
                table.AppendChild(dRow);
            }

            mainPart.Document.Save();
        }

        private static W.TableCell WordCelda(string texto, bool bold, string fill, string color)
        {
            var tc = new W.TableCell();
            tc.AppendChild(new W.TableCellProperties(
                new W.Shading { Val = W.ShadingPatternValues.Clear, Fill = fill, Color = "auto" }));
            var p = new W.Paragraph();
            var r = new W.Run();
            r.RunProperties = new W.RunProperties();
            if (bold) r.RunProperties.AppendChild(new W.Bold());
            r.RunProperties.AppendChild(new W.Color { Val = color });
            r.RunProperties.AppendChild(new W.FontSize { Val = "18" });
            r.AppendChild(new W.Text(texto));
            p.AppendChild(r);
            tc.AppendChild(p);
            return tc;
        }

        // ?????????????????????????????????????????????
        //  EXPORTAR ? POWERPOINT
        // ?????????????????????????????????????????????
        private async void btnExportPowerPoint_Click(object sender, EventArgs e)
        {
            if (!ValidarDatos()) return;

            var dt = _dataTable!;

            int filas = Math.Min(dt.Rows.Count, LimitePpt);
            if (dt.Rows.Count > LimitePpt)
            {
                var resp = MessageBox.Show(
                    $"El documento tiene {dt.Rows.Count:N0} filas.\n" +
                    $"PowerPoint no es adecuado para datasets grandes.\n\n" +
                    $"Se exportarán solo las primeras {LimitePpt:N0} filas.\n\n" +
                    "¿Continuar?",
                    "Advertencia", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (resp != DialogResult.Yes) return;
            }

            string? path = GuardarComo("PowerPoint (*.pptx)|*.pptx", "pptx");
            if (path == null) return;

            IniciarExport("PowerPoint", filas);

            try
            {
                await Task.Run(() => ExportarPptx(path, dt, filas));
                MostrarExito(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar PowerPoint:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { FinalizarExport(); }
        }

        private static void ExportarPptx(string path, System.Data.DataTable dt, int filas)
        {
            const int rowsPerSlide = 15;
            int totalSlides = Math.Max(1, (int)Math.Ceiling((double)filas / rowsPerSlide));

            using var pptDoc = PresentationDocument.Create(path, PresentationDocumentType.Presentation);
            var presPart = pptDoc.AddPresentationPart();
            presPart.Presentation = new P.Presentation();

            var slideIdList = new P.SlideIdList();
            presPart.Presentation.AppendChild(slideIdList);
            presPart.Presentation.AppendChild(new P.SlideSize { Cx = 12192000, Cy = 6858000 });
            presPart.Presentation.AppendChild(new P.NotesSize { Cx = 6858000, Cy = 9144000 });

            uint slideId = 256;

            for (int s = 0; s < totalSlides; s++)
            {
                int startRow = s * rowsPerSlide;
                int endRow = Math.Min(startRow + rowsPerSlide, filas);

                var slidePart = presPart.AddNewPart<SlidePart>();
                var slide = new P.Slide();
                var cSld = new P.CommonSlideData();
                var spTree = new P.ShapeTree();

                spTree.AppendChild(new P.NonVisualGroupShapeProperties(
                    new P.NonVisualDrawingProperties { Id = 1, Name = "" },
                    new P.NonVisualGroupShapeDrawingProperties(),
                    new P.ApplicationNonVisualDrawingProperties()));
                spTree.AppendChild(new P.GroupShapeProperties(new D.TransformGroup()));

                string titulo = totalSlides > 1
                    ? $"Datos exportados  —  Página {s + 1} de {totalSlides}"
                    : "Datos exportados";

                spTree.AppendChild(PptTitulo(titulo));
                spTree.AppendChild(PptTabla(dt, startRow, endRow, s));

                cSld.AppendChild(spTree);
                slide.AppendChild(cSld);
                slide.AppendChild(new P.ColorMapOverride(new D.MasterColorMapping()));
                slidePart.Slide = slide;

                string relId = presPart.CreateRelationshipToPart(slidePart);
                slideIdList.AppendChild(new P.SlideId { Id = slideId++, RelationshipId = relId });
            }

            presPart.Presentation.Save();
        }

        private static P.Shape PptTitulo(string texto) =>
            new P.Shape(
                new P.NonVisualShapeProperties(
                    new P.NonVisualDrawingProperties { Id = 2, Name = "Titulo" },
                    new P.NonVisualShapeDrawingProperties(new D.ShapeLocks { NoGrouping = true }),
                    new P.ApplicationNonVisualDrawingProperties(new P.PlaceholderShape { Index = 1 })),
                new P.ShapeProperties(new D.Transform2D(
                    new D.Offset { X = 457200, Y = 274638 },
                    new D.Extents { Cx = 11277600, Cy = 457200 })),
                new P.TextBody(new D.BodyProperties(), new D.ListStyle(),
                    new D.Paragraph(new D.Run(
                        new D.RunProperties { Language = "es-MX", FontSize = 2000, Bold = true },
                        new D.Text(texto)))));

        private static P.GraphicFrame PptTabla(System.Data.DataTable dt,
                                               int startRow, int endRow, int slideIdx)
        {
            int cols = dt.Columns.Count;
            long totalAncho = 11277600L;
            long colAncho = cols > 0 ? totalAncho / cols : totalAncho;
            long filAltura = 380000L;

            var filas = new List<D.TableRow>();

            // Encabezado
            var enc = new D.TableRow { Height = filAltura };
            for (int c = 0; c < cols; c++)
                enc.AppendChild(PptCelda(dt.Columns[c].ColumnName, bold: true,
                    bgHex: "1F5C8B", fgHex: "FFFFFF"));
            filas.Add(enc);

            // Datos
            for (int r = startRow; r < endRow; r++)
            {
                string bg = ((r - startRow) % 2 == 0) ? "FFFFFF" : "E8F0FE";
                var fila = new D.TableRow { Height = filAltura };
                for (int c = 0; c < cols; c++)
                    fila.AppendChild(PptCelda(dt.Rows[r][c]?.ToString() ?? "",
                        bold: false, bgHex: bg, fgHex: "000000"));
                filas.Add(fila);
            }

            var grid = new D.TableGrid();
            for (int c = 0; c < cols; c++)
                grid.AppendChild(new D.GridColumn { Width = colAncho });

            var tabla = new D.Table();
            tabla.AppendChild(new D.TableProperties { FirstRow = true, BandRow = true });
            tabla.AppendChild(grid);
            foreach (var f in filas) tabla.AppendChild(f);

            return new P.GraphicFrame(
                new P.NonVisualGraphicFrameProperties(
                    new P.NonVisualDrawingProperties { Id = (uint)(100 + slideIdx), Name = $"Tabla{slideIdx}" },
                    new P.NonVisualGraphicFrameDrawingProperties(new D.GraphicFrameLocks { NoGrouping = true }),
                    new P.ApplicationNonVisualDrawingProperties()),
                new P.Transform(
                    new D.Offset { X = 457200, Y = 820000 },
                    new D.Extents { Cx = totalAncho, Cy = filas.Count * filAltura }),
                new D.Graphic(
                    new D.GraphicData(tabla)
                    { Uri = "http://schemas.openxmlformats.org/drawingml/2006/table" }));
        }

        private static D.TableCell PptCelda(string texto, bool bold, string bgHex, string fgHex) =>
            new D.TableCell(
                new D.TextBody(new D.BodyProperties(), new D.ListStyle(),
                    new D.Paragraph(new D.Run(
                        new D.RunProperties { Bold = bold, FontSize = 1200, Language = "es-MX", Dirty = false },
                        new D.Text(texto)))),
                new D.TableCellProperties(
                    new D.SolidFill(new D.RgbColorModelHex { Val = bgHex }),
                    new D.LeftBorderLineProperties(new D.SolidFill(new D.RgbColorModelHex { Val = "CCCCCC" })),
                    new D.RightBorderLineProperties(new D.SolidFill(new D.RgbColorModelHex { Val = "CCCCCC" })),
                    new D.TopBorderLineProperties(new D.SolidFill(new D.RgbColorModelHex { Val = "CCCCCC" })),
                    new D.BottomBorderLineProperties(new D.SolidFill(new D.RgbColorModelHex { Val = "CCCCCC" }))));

        // ?????????????????????????????????????????????
        //  EXPORTAR ? CSV  (StreamWriter, sin StringBuilder gigante)
        // ?????????????????????????????????????????????
        private async void btnExportCsv_Click(object sender, EventArgs e)
        {
            if (!ValidarDatos()) return;
            string? path = GuardarComo("CSV (*.csv)|*.csv", "csv");
            if (path == null) return;

            var dt = _dataTable!;
            IniciarExport("CSV", dt.Rows.Count);

            try
            {
                await Task.Run(() => ExportarCsv(path, dt));
                MostrarExito(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar CSV:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { FinalizarExport(); }
        }

        private static void ExportarCsv(string path, System.Data.DataTable dt)
        {
            // StreamWriter con buffer grande ? escribe directo a disco sin acumular en RAM
            using var sw = new StreamWriter(path, append: false,
                encoding: new System.Text.UTF8Encoding(true), bufferSize: 65536);

            int cols = dt.Columns.Count;

            // Encabezados
            for (int c = 0; c < cols; c++)
            {
                if (c > 0) sw.Write(',');
                sw.Write(EscaparCsv(dt.Columns[c].ColumnName));
            }
            sw.WriteLine();

            // Datos
            for (int r = 0; r < dt.Rows.Count; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (c > 0) sw.Write(',');
                    sw.Write(EscaparCsv(dt.Rows[r][c]?.ToString() ?? ""));
                }
                sw.WriteLine();
            }
        }

        private static string EscaparCsv(string v) =>
            (v.Contains(',') || v.Contains('"') || v.Contains('\n'))
                ? $"\"{v.Replace("\"", "\"\"")}\""
                : v;

        // ?????????????????????????????????????????????
        //  UTILIDAD
        // ?????????????????????????????????????????????
        private static void MostrarExito(string path)
        {
            var res = MessageBox.Show(
                $"Archivo exportado correctamente:\n{path}\n\n¿Desea abrir el archivo?",
                "Exportación exitosa", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (res == DialogResult.Yes)
            {
                try
                {
                    System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
                }
                catch { }
            }
        }
    }
}