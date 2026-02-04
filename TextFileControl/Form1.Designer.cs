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
      ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
      SuspendLayout();
      // 
      // dataGridView1
      // 
      dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      dataGridView1.Location = new Point(0, 34);
      dataGridView1.Name = "dataGridView1";
      dataGridView1.RowHeadersWidth = 51;
      dataGridView1.Size = new Size(713, 412);
      dataGridView1.TabIndex = 0;
      // 
      // btnLoadFile
      // 
      btnLoadFile.Location = new Point(0, -1);
      btnLoadFile.Name = "btnLoadFile";
      btnLoadFile.Size = new Size(713, 29);
      btnLoadFile.TabIndex = 1;
      btnLoadFile.Text = "Load File";
      btnLoadFile.UseVisualStyleBackColor = true;
      btnLoadFile.Click += btnLoadFile_Click;
      // 
      // Form1
      // 
      AutoScaleDimensions = new SizeF(8F, 20F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(1133, 450);
      Controls.Add(btnLoadFile);
      Controls.Add(dataGridView1);
      Name = "Form1";
      Text = "Form1";
      ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
      ResumeLayout(false);
    }

    #endregion

    private DataGridView dataGridView1;
    private Button btnLoadFile;
  }
}
