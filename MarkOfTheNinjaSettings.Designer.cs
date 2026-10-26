namespace LiveSplit.MarkOfTheNinja
{
    partial class MarkOfTheNinjaSettings
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.chkAutoStart = new System.Windows.Forms.CheckBox();
            this.chkSplitOnLevelComplete = new System.Windows.Forms.CheckBox();
            this.chkSplitOnFinalChoice = new System.Windows.Forms.CheckBox();
            this.chkAutoReset = new System.Windows.Forms.CheckBox();
            this.chkRemoveLoads = new System.Windows.Forms.CheckBox();
            this.lblNote = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // chkAutoStart
            this.chkAutoStart.AutoSize = true;
            this.chkAutoStart.Location = new System.Drawing.Point(10, 10);
            this.chkAutoStart.Name = "chkAutoStart";
            this.chkAutoStart.Text = "Start when the player gets control in 0-1";

            // chkSplitOnLevelComplete
            this.chkSplitOnLevelComplete.AutoSize = true;
            this.chkSplitOnLevelComplete.Location = new System.Drawing.Point(10, 35);
            this.chkSplitOnLevelComplete.Name = "chkSplitOnLevelComplete";
            this.chkSplitOnLevelComplete.Text = "Split at the end of each level";

            // chkSplitOnFinalChoice
            this.chkSplitOnFinalChoice.AutoSize = true;
            this.chkSplitOnFinalChoice.Location = new System.Drawing.Point(10, 60);
            this.chkSplitOnFinalChoice.Name = "chkSplitOnFinalChoice";
            this.chkSplitOnFinalChoice.Text = "Split on the final choice input (4-2)";

            // chkAutoReset
            this.chkAutoReset.AutoSize = true;
            this.chkAutoReset.Location = new System.Drawing.Point(10, 85);
            this.chkAutoReset.Name = "chkAutoReset";
            this.chkAutoReset.Text = "Reset when 0-1 is restarted";

            // chkRemoveLoads
            this.chkRemoveLoads.AutoSize = true;
            this.chkRemoveLoads.Location = new System.Drawing.Point(10, 110);
            this.chkRemoveLoads.Name = "chkRemoveLoads";
            this.chkRemoveLoads.Text = "Remove loading screens (compare against Game Time)";

            // lblNote
            this.lblNote.AutoSize = true;
            this.lblNote.Location = new System.Drawing.Point(10, 140);
            this.lblNote.Name = "lblNote";
            this.lblNote.ForeColor = System.Drawing.Color.Gray;
            this.lblNote.Text = "Variable: MarkOfTheNinja_Version";

            // MarkOfTheNinjaSettings
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkAutoStart);
            this.Controls.Add(this.chkSplitOnLevelComplete);
            this.Controls.Add(this.chkSplitOnFinalChoice);
            this.Controls.Add(this.chkAutoReset);
            this.Controls.Add(this.chkRemoveLoads);
            this.Controls.Add(this.lblNote);
            this.Name = "MarkOfTheNinjaSettings";
            this.Size = new System.Drawing.Size(415, 165);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.CheckBox chkAutoStart;
        private System.Windows.Forms.CheckBox chkSplitOnLevelComplete;
        private System.Windows.Forms.CheckBox chkSplitOnFinalChoice;
        private System.Windows.Forms.CheckBox chkAutoReset;
        private System.Windows.Forms.CheckBox chkRemoveLoads;
        private System.Windows.Forms.Label lblNote;
    }
}
