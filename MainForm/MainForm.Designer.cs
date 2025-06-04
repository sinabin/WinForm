namespace Icheon
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.pnDomino = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.btTransparentDisconnect = new System.Windows.Forms.Button();
            this.btTransparentConnect = new System.Windows.Forms.Button();
            this.btBlackDisconnect = new System.Windows.Forms.Button();
            this.btBlackConnect = new System.Windows.Forms.Button();
            this.lbTransparentDomino = new System.Windows.Forms.Label();
            this.lbBlackDomino = new System.Windows.Forms.Label();
            this.lvWorkList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader16 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader15 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader17 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader18 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lbWorkList = new System.Windows.Forms.Label();
            this.lvOutput = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btEnd = new System.Windows.Forms.Button();
            this.lbOutput = new System.Windows.Forms.Label();
            this.nud = new System.Windows.Forms.NumericUpDown();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.LB_TotalStatus = new System.Windows.Forms.Label();
            this.LB_RestOrder = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.pnDomino.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud)).BeginInit();
            this.SuspendLayout();
            // 
            // pnDomino
            // 
            this.pnDomino.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnDomino.Controls.Add(this.label5);
            this.pnDomino.Controls.Add(this.btTransparentDisconnect);
            this.pnDomino.Controls.Add(this.btTransparentConnect);
            this.pnDomino.Controls.Add(this.btBlackDisconnect);
            this.pnDomino.Controls.Add(this.btBlackConnect);
            this.pnDomino.Controls.Add(this.lbTransparentDomino);
            this.pnDomino.Controls.Add(this.lbBlackDomino);
            this.pnDomino.Location = new System.Drawing.Point(15, 4);
            this.pnDomino.Name = "pnDomino";
            this.pnDomino.Size = new System.Drawing.Size(1067, 66);
            this.pnDomino.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("굴림체", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label5.Location = new System.Drawing.Point(78, 15);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(516, 35);
            this.label5.TabIndex = 6;
            this.label5.Text = "종량제 봉투 생산관리 시스템";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // btTransparentDisconnect
            // 
            this.btTransparentDisconnect.Location = new System.Drawing.Point(972, 38);
            this.btTransparentDisconnect.Name = "btTransparentDisconnect";
            this.btTransparentDisconnect.Size = new System.Drawing.Size(75, 23);
            this.btTransparentDisconnect.TabIndex = 5;
            this.btTransparentDisconnect.Text = "투명 종료";
            this.btTransparentDisconnect.UseVisualStyleBackColor = true;
            // 
            // btTransparentConnect
            // 
            this.btTransparentConnect.Location = new System.Drawing.Point(891, 38);
            this.btTransparentConnect.Name = "btTransparentConnect";
            this.btTransparentConnect.Size = new System.Drawing.Size(75, 23);
            this.btTransparentConnect.TabIndex = 4;
            this.btTransparentConnect.Text = "투명 연결";
            this.btTransparentConnect.UseVisualStyleBackColor = true;
            // 
            // btBlackDisconnect
            // 
            this.btBlackDisconnect.Location = new System.Drawing.Point(972, 9);
            this.btBlackDisconnect.Name = "btBlackDisconnect";
            this.btBlackDisconnect.Size = new System.Drawing.Size(75, 23);
            this.btBlackDisconnect.TabIndex = 3;
            this.btBlackDisconnect.Text = "흑백 종료";
            this.btBlackDisconnect.UseVisualStyleBackColor = true;
            // 
            // btBlackConnect
            // 
            this.btBlackConnect.Location = new System.Drawing.Point(891, 9);
            this.btBlackConnect.Name = "btBlackConnect";
            this.btBlackConnect.Size = new System.Drawing.Size(75, 23);
            this.btBlackConnect.TabIndex = 2;
            this.btBlackConnect.Text = "흑백 연결";
            this.btBlackConnect.UseVisualStyleBackColor = true;
            // 
            // lbTransparentDomino
            // 
            this.lbTransparentDomino.AutoSize = true;
            this.lbTransparentDomino.Location = new System.Drawing.Point(664, 38);
            this.lbTransparentDomino.Name = "lbTransparentDomino";
            this.lbTransparentDomino.Size = new System.Drawing.Size(109, 12);
            this.lbTransparentDomino.TabIndex = 1;
            this.lbTransparentDomino.Text = "투명 도미노 프린터";
            // 
            // lbBlackDomino
            // 
            this.lbBlackDomino.AutoSize = true;
            this.lbBlackDomino.Location = new System.Drawing.Point(664, 15);
            this.lbBlackDomino.Name = "lbBlackDomino";
            this.lbBlackDomino.Size = new System.Drawing.Size(109, 12);
            this.lbBlackDomino.TabIndex = 0;
            this.lbBlackDomino.Text = "흑백 도미노 프린터";
            // 
            // lvWorkList
            // 
            this.lvWorkList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader16,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader11,
            this.columnHeader12,
            this.columnHeader15,
            this.columnHeader17,
            this.columnHeader18});
            this.lvWorkList.FullRowSelect = true;
            this.lvWorkList.HideSelection = false;
            this.lvWorkList.Location = new System.Drawing.Point(10, 101);
            this.lvWorkList.Name = "lvWorkList";
            this.lvWorkList.Size = new System.Drawing.Size(1070, 200);
            this.lvWorkList.TabIndex = 6;
            this.lvWorkList.UseCompatibleStateImageBehavior = false;
            this.lvWorkList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "선택";
            this.columnHeader1.Width = 40;
            // 
            // columnHeader16
            // 
            this.columnHeader16.Text = "작업번호";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "지자체";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "용도";
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "리터";
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "현재라인의 생산량(항목별)";
            this.columnHeader12.Width = 180;
            // 
            // columnHeader15
            // 
            this.columnHeader15.Text = "발주량";
            this.columnHeader15.Width = 80;
            // 
            // columnHeader17
            // 
            this.columnHeader17.Text = "발주번호";
            this.columnHeader17.Width = 80;
            // 
            // columnHeader18
            // 
            this.columnHeader18.Text = "발주일시";
            this.columnHeader18.Width = 120;
            // 
            // lbWorkList
            // 
            this.lbWorkList.AutoSize = true;
            this.lbWorkList.Font = new System.Drawing.Font("굴림", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbWorkList.Location = new System.Drawing.Point(12, 83);
            this.lbWorkList.Name = "lbWorkList";
            this.lbWorkList.Size = new System.Drawing.Size(71, 15);
            this.lbWorkList.TabIndex = 7;
            this.lbWorkList.Text = "작업목록";
            // 
            // lvOutput
            // 
            this.lvOutput.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10});
            this.lvOutput.HideSelection = false;
            this.lvOutput.Location = new System.Drawing.Point(12, 413);
            this.lvOutput.Name = "lvOutput";
            this.lvOutput.Size = new System.Drawing.Size(1068, 531);
            this.lvOutput.TabIndex = 8;
            this.lvOutput.UseCompatibleStateImageBehavior = false;
            this.lvOutput.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "번호";
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "생산일";
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "프린터";
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "지자체";
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "용도";
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "리터";
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "Serial";
            // 
            // btEnd
            // 
            this.btEnd.Location = new System.Drawing.Point(882, 307);
            this.btEnd.Name = "btEnd";
            this.btEnd.Size = new System.Drawing.Size(200, 100);
            this.btEnd.TabIndex = 11;
            this.btEnd.Text = "작업완료";
            this.btEnd.UseVisualStyleBackColor = true;
            // 
            // lbOutput
            // 
            this.lbOutput.AutoSize = true;
            this.lbOutput.Location = new System.Drawing.Point(734, 76);
            this.lbOutput.Name = "lbOutput";
            this.lbOutput.Size = new System.Drawing.Size(107, 12);
            this.lbOutput.TabIndex = 13;
            this.lbOutput.Text = "생산속도: 초당 0장";
            // 
            // nud
            // 
            this.nud.Location = new System.Drawing.Point(962, 74);
            this.nud.Name = "nud";
            this.nud.ReadOnly = true;
            this.nud.Size = new System.Drawing.Size(120, 21);
            this.nud.TabIndex = 14;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(867, 76);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 15;
            this.label1.Text = "다음시리얼번호";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("굴림", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.Location = new System.Drawing.Point(15, 348);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(233, 19);
            this.label2.TabIndex = 6;
            this.label2.Text = "통합생산량(전라인)        : ";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("굴림", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label3.Location = new System.Drawing.Point(15, 385);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(235, 19);
            this.label3.TabIndex = 16;
            this.label3.Text = "통합생산 잔여량(전라인) : ";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // LB_TotalStatus
            // 
            this.LB_TotalStatus.AutoSize = true;
            this.LB_TotalStatus.Font = new System.Drawing.Font("굴림", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_TotalStatus.ForeColor = System.Drawing.Color.Blue;
            this.LB_TotalStatus.Location = new System.Drawing.Point(241, 307);
            this.LB_TotalStatus.Name = "LB_TotalStatus";
            this.LB_TotalStatus.Size = new System.Drawing.Size(28, 27);
            this.LB_TotalStatus.TabIndex = 17;
            this.LB_TotalStatus.Text = "0";
            this.LB_TotalStatus.Click += new System.EventHandler(this.LB_TotalStatus_Click);
            // 
            // LB_RestOrder
            // 
            this.LB_RestOrder.AutoSize = true;
            this.LB_RestOrder.Font = new System.Drawing.Font("굴림", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LB_RestOrder.ForeColor = System.Drawing.Color.Red;
            this.LB_RestOrder.Location = new System.Drawing.Point(240, 377);
            this.LB_RestOrder.Name = "LB_RestOrder";
            this.LB_RestOrder.Size = new System.Drawing.Size(28, 27);
            this.LB_RestOrder.TabIndex = 18;
            this.LB_RestOrder.Text = "0";
            this.LB_RestOrder.Click += new System.EventHandler(this.LB_RestOrder_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(611, 76);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 19;
            this.label4.Text = "생산라인 번호: ";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("굴림", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label6.Location = new System.Drawing.Point(15, 311);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(233, 19);
            this.label6.TabIndex = 20;
            this.label6.Text = "현재라인 총생산량         : ";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("굴림", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label7.ForeColor = System.Drawing.Color.Blue;
            this.label7.Location = new System.Drawing.Point(240, 341);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(28, 27);
            this.label7.TabIndex = 21;
            this.label7.Text = "0";
            this.label7.Click += new System.EventHandler(this.label7_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1084, 861);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.LB_RestOrder);
            this.Controls.Add(this.LB_TotalStatus);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.nud);
            this.Controls.Add(this.lbOutput);
            this.Controls.Add(this.btEnd);
            this.Controls.Add(this.lvOutput);
            this.Controls.Add(this.lbWorkList);
            this.Controls.Add(this.lvWorkList);
            this.Controls.Add(this.pnDomino);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.pnDomino.ResumeLayout(false);
            this.pnDomino.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel pnDomino;
        private System.Windows.Forms.Label lbTransparentDomino;
        private System.Windows.Forms.Label lbBlackDomino;
        private System.Windows.Forms.ListView lvWorkList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Label lbWorkList;
        private System.Windows.Forms.ListView lvOutput;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.Button btEnd;
        private System.Windows.Forms.Label lbOutput;
        private System.Windows.Forms.NumericUpDown nud;
        private System.Windows.Forms.Button btTransparentDisconnect;
        private System.Windows.Forms.Button btTransparentConnect;
        private System.Windows.Forms.Button btBlackDisconnect;
        private System.Windows.Forms.Button btBlackConnect;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label LB_TotalStatus;
        private System.Windows.Forms.Label LB_RestOrder;
        private System.Windows.Forms.ColumnHeader columnHeader15;
        private System.Windows.Forms.ColumnHeader columnHeader16;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ColumnHeader columnHeader17;
        private System.Windows.Forms.ColumnHeader columnHeader18;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
    }
}

