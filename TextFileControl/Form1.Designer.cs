namespace TextFileControl
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dataGridView1 = new DataGridView();
            btnLoadFile = new Button();
            pnlExport = new Panel();
            lblExport = new Label();
            btnExportExcel = new Button();
            btnExportWord = new Button();
            btnExportPowerPoint = new Button();
            btnExportCsv = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            pnlExport.SuspendLayout();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(0, 34);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(1133, 360);
            dataGridView1.TabIndex = 0;
            // 
            // btnLoadFile
            // 
            btnLoadFile.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnLoadFile.Location = new Point(0, 0);
            btnLoadFile.Name = "btnLoadFile";
            btnLoadFile.Size = new Size(1133, 29);
            btnLoadFile.TabIndex = 1;
            btnLoadFile.Text = "Cargar Archivo";
            btnLoadFile.UseVisualStyleBackColor = true;
            btnLoadFile.Click += btnLoadFile_Click;
            // 
            // pnlExport
            // 
            pnlExport.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlExport.BackColor = Color.FromArgb(240, 240, 240);
            pnlExport.Controls.Add(lblExport);
            pnlExport.Controls.Add(btnExportExcel);
            pnlExport.Controls.Add(btnExportWord);
            pnlExport.Controls.Add(btnExportPowerPoint);
            pnlExport.Controls.Add(btnExportCsv);
            pnlExport.Location = new Point(0, 400);
            pnlExport.Name = "pnlExport";
            pnlExport.Size = new Size(1133, 50);
            pnlExport.TabIndex = 2;
            // 
            // lblExport
            // 
            lblExport.AutoSize = true;
            lblExport.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblExport.ForeColor = Color.FromArgb(80, 80, 80);
            lblExport.Location = new Point(12, 16);
            lblExport.Name = "lblExport";
            lblExport.Size = new Size(74, 20);
            lblExport.TabIndex = 0;
            lblExport.Text = "Exportar:";
            // 
            // btnExportExcel
            // 
            btnExportExcel.BackColor = Color.FromArgb(33, 115, 70);
            btnExportExcel.FlatAppearance.BorderSize = 0;
            btnExportExcel.FlatStyle = FlatStyle.Flat;
            btnExportExcel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnExportExcel.ForeColor = Color.White;
            btnExportExcel.Location = new Point(106, 10);
            btnExportExcel.Name = "btnExportExcel";
            btnExportExcel.Size = new Size(150, 32);
            btnExportExcel.TabIndex = 3;
            btnExportExcel.Text = "Excel (.xlsx)";
            btnExportExcel.UseVisualStyleBackColor = false;
            btnExportExcel.Click += btnExportExcel_Click;
            // 
            // btnExportWord
            // 
            btnExportWord.BackColor = Color.FromArgb(43, 87, 154);
            btnExportWord.FlatAppearance.BorderSize = 0;
            btnExportWord.FlatStyle = FlatStyle.Flat;
            btnExportWord.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnExportWord.ForeColor = Color.White;
            btnExportWord.Location = new Point(271, 10);
            btnExportWord.Name = "btnExportWord";
            btnExportWord.Size = new Size(150, 32);
            btnExportWord.TabIndex = 4;
            btnExportWord.Text = "Word (.docx)";
            btnExportWord.UseVisualStyleBackColor = false;
            btnExportWord.Click += btnExportWord_Click;
            // 
            // btnExportPowerPoint
            // 
            btnExportPowerPoint.BackColor = Color.FromArgb(209, 68, 36);
            btnExportPowerPoint.FlatAppearance.BorderSize = 0;
            btnExportPowerPoint.FlatStyle = FlatStyle.Flat;
            btnExportPowerPoint.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnExportPowerPoint.ForeColor = Color.White;
            btnExportPowerPoint.Location = new Point(436, 10);
            btnExportPowerPoint.Name = "btnExportPowerPoint";
            btnExportPowerPoint.Size = new Size(170, 32);
            btnExportPowerPoint.TabIndex = 5;
            btnExportPowerPoint.Text = "PowerPoint (.pptx)";
            btnExportPowerPoint.UseVisualStyleBackColor = false;
            btnExportPowerPoint.Click += btnExportPowerPoint_Click;
            // 
            // btnExportCsv
            // 
            btnExportCsv.BackColor = Color.FromArgb(120, 85, 160);
            btnExportCsv.FlatAppearance.BorderSize = 0;
            btnExportCsv.FlatStyle = FlatStyle.Flat;
            btnExportCsv.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnExportCsv.ForeColor = Color.White;
            btnExportCsv.Location = new Point(621, 10);
            btnExportCsv.Name = "btnExportCsv";
            btnExportCsv.Size = new Size(150, 32);
            btnExportCsv.TabIndex = 6;
            btnExportCsv.Text = "CSV (.csv)";
            btnExportCsv.UseVisualStyleBackColor = false;
            btnExportCsv.Click += btnExportCsv_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1133, 450);
            Controls.Add(pnlExport);
            Controls.Add(btnLoadFile);
            Controls.Add(dataGridView1);
            Name = "Form1";
            Text = "TextFileControl - Visor y Exportador";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            pnlExport.ResumeLayout(false);
            pnlExport.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dataGridView1;
        private Button btnLoadFile;
        private Panel pnlExport;
        private Button btnExportExcel;
        private Button btnExportWord;
        private Button btnExportPowerPoint;
        private Button btnExportCsv;
        private Label lblExport;
    }
}