using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace UOAIO;

public class GenericExceptionDialog : Form
{
	private IContainer components = null;

	private Button cmdToggleDetails;

	private Button cmdOkay;

	private Label lblMessage;

	private TextBox txtDetails;

	public GenericExceptionDialog()
	{
		this.InitializeComponent();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		e.Graphics.DrawIcon(SystemIcons.Error, 18, 16);
	}

	public void SetExceptionInfo(Exception ex, string fmt, params object[] args)
	{
		string text = string.Format(fmt, args);
		this.lblMessage.Text = text;
		this.txtDetails.Text = ex.ToString();
	}

	private void cmdToggleDetails_Click(object sender, EventArgs e)
	{
		base.SuspendLayout();
		if (this.txtDetails.Visible)
		{
			this.txtDetails.Visible = false;
			base.Height -= 192;
			this.cmdOkay.Top -= 192;
			this.cmdToggleDetails.Top -= 192;
			this.cmdToggleDetails.Text = "Show &Details";
		}
		else
		{
			this.txtDetails.Visible = true;
			base.Height += 192;
			this.cmdOkay.Top += 192;
			this.cmdToggleDetails.Top += 192;
			this.cmdToggleDetails.Text = "Hide &Details";
		}
		base.ResumeLayout();
	}

	private void txtDetails_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.A && e.Control)
		{
			this.txtDetails.SelectAll();
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && this.components != null)
		{
			this.components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.cmdToggleDetails = new System.Windows.Forms.Button();
		this.cmdOkay = new System.Windows.Forms.Button();
		this.lblMessage = new System.Windows.Forms.Label();
		this.txtDetails = new System.Windows.Forms.TextBox();
		base.SuspendLayout();
		this.cmdToggleDetails.Location = new System.Drawing.Point(292, 58);
		this.cmdToggleDetails.Name = "cmdToggleDetails";
		this.cmdToggleDetails.Size = new System.Drawing.Size(88, 23);
		this.cmdToggleDetails.TabIndex = 16;
		this.cmdToggleDetails.Text = "Show &Details";
		this.cmdToggleDetails.Click += new System.EventHandler(cmdToggleDetails_Click);
		this.cmdOkay.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.cmdOkay.Location = new System.Drawing.Point(206, 58);
		this.cmdOkay.Name = "cmdOkay";
		this.cmdOkay.Size = new System.Drawing.Size(80, 23);
		this.cmdOkay.TabIndex = 13;
		this.cmdOkay.Text = "&Okay";
		this.lblMessage.Location = new System.Drawing.Point(50, 9);
		this.lblMessage.Name = "lblMessage";
		this.lblMessage.Size = new System.Drawing.Size(330, 46);
		this.lblMessage.TabIndex = 12;
		this.txtDetails.Font = new System.Drawing.Font("Courier New", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.txtDetails.Location = new System.Drawing.Point(4, 60);
		this.txtDetails.Multiline = true;
		this.txtDetails.Name = "txtDetails";
		this.txtDetails.ReadOnly = true;
		this.txtDetails.ScrollBars = System.Windows.Forms.ScrollBars.Both;
		this.txtDetails.Size = new System.Drawing.Size(376, 184);
		this.txtDetails.TabIndex = 15;
		this.txtDetails.Visible = false;
		this.txtDetails.WordWrap = false;
		this.txtDetails.KeyDown += new System.Windows.Forms.KeyEventHandler(txtDetails_KeyDown);
		base.AcceptButton = this.cmdOkay;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(384, 85);
		base.Controls.Add(this.cmdToggleDetails);
		base.Controls.Add(this.cmdOkay);
		base.Controls.Add(this.lblMessage);
		base.Controls.Add(this.txtDetails);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "GenericExceptionDialog";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Ultima Online - Veritas";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
