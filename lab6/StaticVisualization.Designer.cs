namespace lab6
{
    partial class StaticVisualization
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
            this.pbScreen = new System.Windows.Forms.PictureBox();
            this.btnStartAnimation = new System.Windows.Forms.Button();
            this.lstObject = new System.Windows.Forms.ListBox();
            this.lstvObject = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lstbObserver = new System.Windows.Forms.ListBox();
            this.lstvObserver = new System.Windows.Forms.ListView();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnStopAnimation = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbScreen)).BeginInit();
            this.SuspendLayout();
            // 
            // pbScreen
            // 
            this.pbScreen.BackColor = System.Drawing.Color.White;
            this.pbScreen.Location = new System.Drawing.Point(0, 0);
            this.pbScreen.Name = "pbScreen";
            this.pbScreen.Size = new System.Drawing.Size(600, 600);
            this.pbScreen.TabIndex = 0;
            this.pbScreen.TabStop = false;
            // 
            // btnStartAnimation
            // 
            this.btnStartAnimation.BackColor = System.Drawing.Color.GhostWhite;
            this.btnStartAnimation.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnStartAnimation.Location = new System.Drawing.Point(612, 461);
            this.btnStartAnimation.Name = "btnStartAnimation";
            this.btnStartAnimation.Size = new System.Drawing.Size(160, 40);
            this.btnStartAnimation.TabIndex = 1;
            this.btnStartAnimation.Text = "Запустить анимацию";
            this.btnStartAnimation.UseVisualStyleBackColor = false;
            this.btnStartAnimation.Click += new System.EventHandler(this.btnStartAnimation_Click);
            // 
            // lstObject
            // 
            this.lstObject.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lstObject.FormattingEnabled = true;
            this.lstObject.ItemHeight = 16;
            this.lstObject.Items.AddRange(new object[] {
            "3D-объект: ",
            "усечённая пирамида",
            "(ВСК)"});
            this.lstObject.Location = new System.Drawing.Point(606, 12);
            this.lstObject.Name = "lstObject";
            this.lstObject.Size = new System.Drawing.Size(170, 84);
            this.lstObject.TabIndex = 2;
            // 
            // lstvObject
            // 
            this.lstvObject.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lstvObject.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lstvObject.Location = new System.Drawing.Point(606, 102);
            this.lstvObject.Name = "lstvObject";
            this.lstvObject.Size = new System.Drawing.Size(170, 87);
            this.lstvObject.TabIndex = 3;
            this.lstvObject.UseCompatibleStateImageBehavior = false;
            this.lstvObject.View = System.Windows.Forms.View.List;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 170;
            // 
            // lstbObserver
            // 
            this.lstbObserver.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lstbObserver.FormattingEnabled = true;
            this.lstbObserver.ItemHeight = 16;
            this.lstbObserver.Items.AddRange(new object[] {
            "Наблюдатель (МСК):"});
            this.lstbObserver.Location = new System.Drawing.Point(606, 195);
            this.lstbObserver.Name = "lstbObserver";
            this.lstbObserver.Size = new System.Drawing.Size(170, 20);
            this.lstbObserver.TabIndex = 4;
            // 
            // lstvObserver
            // 
            this.lstvObserver.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
            this.lstvObserver.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lstvObserver.Location = new System.Drawing.Point(607, 221);
            this.lstvObserver.Name = "lstvObserver";
            this.lstvObserver.Size = new System.Drawing.Size(170, 87);
            this.lstvObserver.TabIndex = 5;
            this.lstvObserver.UseCompatibleStateImageBehavior = false;
            this.lstvObserver.View = System.Windows.Forms.View.List;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Width = 170;
            // 
            // btnStopAnimation
            // 
            this.btnStopAnimation.BackColor = System.Drawing.Color.GhostWhite;
            this.btnStopAnimation.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnStopAnimation.Location = new System.Drawing.Point(612, 510);
            this.btnStopAnimation.Name = "btnStopAnimation";
            this.btnStopAnimation.Size = new System.Drawing.Size(160, 40);
            this.btnStopAnimation.TabIndex = 6;
            this.btnStopAnimation.Text = "Остановить анимацию";
            this.btnStopAnimation.UseVisualStyleBackColor = false;
            this.btnStopAnimation.Click += new System.EventHandler(this.btnStopAnimation_Click);
            // 
            // StaticVisualization
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.HotTrack;
            this.ClientSize = new System.Drawing.Size(784, 604);
            this.Controls.Add(this.btnStopAnimation);
            this.Controls.Add(this.lstvObserver);
            this.Controls.Add(this.lstbObserver);
            this.Controls.Add(this.lstvObject);
            this.Controls.Add(this.lstObject);
            this.Controls.Add(this.btnStartAnimation);
            this.Controls.Add(this.pbScreen);
            this.Name = "StaticVisualization";
            this.Text = "Закраска Фонга";
            ((System.ComponentModel.ISupportInitialize)(this.pbScreen)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbScreen;
        private System.Windows.Forms.Button btnStartAnimation;
        private System.Windows.Forms.ListBox lstObject;
        private System.Windows.Forms.ListView lstvObject;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ListBox lstbObserver;
        private System.Windows.Forms.ListView lstvObserver;
        private System.Windows.Forms.Button btnStopAnimation;
        private System.Windows.Forms.ColumnHeader columnHeader2;
    }
}

