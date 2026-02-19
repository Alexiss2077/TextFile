using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TextFileControl
{
  public partial class Form1 : Form
  {
    private List<string[]> csvData;

    public Form1()
    {
      InitializeComponent();
      ConfigurarDataGridView();
    }

    private void ConfigurarDataGridView()
    {
      // Habilitar modo virtual para mejor rendimiento
      dataGridView1.VirtualMode = true;
      dataGridView1.ReadOnly = true;
      dataGridView1.AllowUserToAddRows = false;
      dataGridView1.AllowUserToDeleteRows = false;

      // Optimizaciones de rendimiento
      dataGridView1.RowHeadersVisible = false;
      dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

      // Eventos del modo virtual
      dataGridView1.CellValueNeeded += DataGridView1_CellValueNeeded;
    }

    private void CargarCSV(string rutaArchivo)////
    {
      try
      {
        Cursor = Cursors.WaitCursor;
        //Leer la primera línea 
        string? firstLine = File.ReadLines(rutaArchivo).FirstOrDefault();

        // Validar si el archivo está vacío
        if (string.IsNullOrEmpty(firstLine))
        {
          MessageBox.Show("El archivo está vacío.", "Error",
              MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
        string[] firstLineColumns = firstLine.Split(',');

        // Leer todas las líneas del CSV
        csvData = File.ReadAllLines(rutaArchivo)
                      .Skip(1) // Saltar encabezado si existe
                      .Select(line => line.Split(','))
                      .ToList();

        // Configurar columnas (ajustar según tu CSV)
        dataGridView1.Columns.Clear();

        for (int i = 0; i < firstLineColumns.Length; i++)
        {
          dataGridView1.Columns.Add($"Column{i}", firstLineColumns[i]);
        }

        // Establecer número de filas
        dataGridView1.RowCount = csvData.Count;

        MessageBox.Show($"Se cargaron {csvData.Count:N0} registros exitosamente.");
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Error al cargar el archivo: {ex.Message}",
            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      finally
      {
        Cursor = Cursors.Default;
      }
    }

    private void DataGridView1_CellValueNeeded(object sender,
        DataGridViewCellValueEventArgs e)
    {
      // Solo proporcionar datos cuando se necesiten (virtualización)
      if (csvData != null && e.RowIndex < csvData.Count)
      {
        var row = csvData[e.RowIndex];
        if (e.ColumnIndex < row.Length)
        {
          e.Value = row[e.ColumnIndex];
        }
      }
    }

    private void LoadFile()
    {
      using (OpenFileDialog ofd = new OpenFileDialog())
      {
        ofd.Filter = "Archivos CSV|*.csv|Todos los archivos|*.*";
        if (ofd.ShowDialog() == DialogResult.OK)
        {
          CargarCSV(ofd.FileName);
        }
      }
    }

    private void btnLoadFile_Click(object sender, EventArgs e)
    {
      LoadFile();
    }
  }
}
