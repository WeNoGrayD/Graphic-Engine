namespace lab1
{
    partial class DrawLines
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnAffines = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnAffines
            // 
            this.btnAffines.Location = new System.Drawing.Point(126, 150);
            this.btnAffines.Name = "btnAffines";
            this.btnAffines.Size = new System.Drawing.Size(235, 45);
            this.btnAffines.TabIndex = 0;
            this.btnAffines.Text = "Произвести аффинные преобразования";
            this.btnAffines.UseVisualStyleBackColor = true;
            this.btnAffines.Click += new System.EventHandler(this.btnAffines_Click);
            // 
            // DrawLines
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(487, 217);
            this.Controls.Add(this.btnAffines);
            this.Name = "DrawLines";
            this.Text = "Процедура генерации линий";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnAffines;
    }
}

