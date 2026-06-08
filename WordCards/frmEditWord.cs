using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WordCards
{
    public partial class frmEditWord : Form
    {
        // ═══════════════════════════════════════════════
        //  原有欄位（保留不動）
        // ═══════════════════════════════════════════════
        public WordItem Word { get; set; } = null;

        public frmEditWord(WordItem word)
        {
            InitializeComponent();

            this.Word = word;
            txtWord.Text = word.Word;
            txtPhonogram.Text = word.Phonogram;
            txtSoundPath.Text = word.SoundPath;
            txtExplain.Text = word.Explain;

            InitCreativeFeatures();
        }

        // ═══════════════════════════════════════════════
        //  原有方法（保留不動）
        // ═══════════════════════════════════════════════
        private void btnSave_Click(object sender, EventArgs e)
        {
            string realWord = (txtWord.ForeColor == Color.Gray) ? "" : txtWord.Text.Trim();
            string realPhono = (txtPhonogram.ForeColor == Color.Gray) ? "" : txtPhonogram.Text.Trim();
            string realSound = (txtSoundPath.ForeColor == Color.Gray) ? "" : txtSoundPath.Text.Trim();
            string realExplain = (txtExplain.ForeColor == Color.Gray) ? "" : txtExplain.Text.Trim();

            if (string.IsNullOrWhiteSpace(realWord))
            {
                lblHint.Text = "⚠ 請輸入英文單字！";
                lblHint.ForeColor = Color.OrangeRed;
                txtWord.Focus();
                return;
            }

            Word.Word = realWord;
            Word.Phonogram = realPhono;
            Word.SoundPath = realSound;
            Word.Explain = realExplain;

            lblHint.Text = "✔ 儲存成功！";
            lblHint.ForeColor = Color.SeaGreen;

            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        // ═══════════════════════════════════════════════
        //  【創意加分】
        // ═══════════════════════════════════════════════
        private Label lblHint;
        private Button btnBrowse;

        private void InitCreativeFeatures()
        {
            // ── 介面美化 ──
            this.Text = "✏️ 編輯單字";
            this.BackColor = Color.FromArgb(245, 248, 255);
            this.Font = new Font("Segoe UI", 10f);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;

            // 先把表單稍微放大，避免高 DPI 或字體變大後擠在一起
            this.ClientSize = new Size(370, 650);

            // ── 重新排版：統一左邊界、寬度、間距 ──
            int left = 24;
            int groupW = this.ClientSize.Width - left * 2;
            int groupH = 76;
            int gapY = 14;

            grpWord.Location = new Point(left, 18);
            grpWord.Size = new Size(groupW, groupH);

            grpPhonogram.Location = new Point(left, grpWord.Bottom + gapY);
            grpPhonogram.Size = new Size(groupW, groupH);

            grpSoundPath.Location = new Point(left, grpPhonogram.Bottom + gapY);
            grpSoundPath.Size = new Size(groupW, groupH);

            grpExplain.Location = new Point(left, grpSoundPath.Bottom + 20);
            grpExplain.Size = new Size(groupW, 260);

            // ── TextBox 位置與大小 ──
            int innerLeft = 16;
            int innerTop = 29;
            int innerRight = 16;

            txtWord.Location = new Point(innerLeft, innerTop);
            txtWord.Size = new Size(grpWord.Width - innerLeft - innerRight, 25);

            txtPhonogram.Location = new Point(innerLeft, innerTop);
            txtPhonogram.Size = new Size(grpPhonogram.Width - innerLeft - innerRight, 25);

            // 音檔路徑右邊留空間給瀏覽按鈕
            int browseW = 58;
            int browseGap = 8;
            txtSoundPath.Location = new Point(innerLeft, innerTop);
            txtSoundPath.Size = new Size(grpSoundPath.Width - innerLeft - innerRight - browseW - browseGap, 25);

            txtExplain.Location = new Point(innerLeft, innerTop);
            txtExplain.Size = new Size(grpExplain.Width - innerLeft - innerRight, grpExplain.Height - innerTop - 16);
            txtExplain.ScrollBars = ScrollBars.Vertical;

            // ── 字元計數 ──
            txtWord.TextChanged += (s, e) => UpdateCharCount();
            txtExplain.TextChanged += (s, e) => UpdateCharCount();

            // ── 模擬提示文字（.NET Framework 無 PlaceholderText）──
            if (string.IsNullOrEmpty(txtPhonogram.Text)) SetHintText(txtPhonogram, "例如：/ ˈæ p l /");
            if (string.IsNullOrEmpty(txtExplain.Text)) SetHintText(txtExplain, "請輸入中文解釋");
            if (string.IsNullOrEmpty(txtSoundPath.Text)) SetHintText(txtSoundPath, "例如：sounds\\apple.mp3");

            // ── 瀏覽按鈕：加到 grpSoundPath 內部 ──
            btnBrowse = new Button
            {
                Text = "📂",
                Width = browseW,
                Height = txtSoundPath.Height,
                Location = new Point(txtSoundPath.Right + browseGap, txtSoundPath.Top),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 90, 200),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f)
            };
            btnBrowse.FlatAppearance.BorderSize = 0;
            btnBrowse.Click += BtnBrowse_Click;
            grpSoundPath.Controls.Add(btnBrowse);

            // ── 美化按鈕 ──
            btnSave.Size = new Size(100, 44);
            btnSave.Location = new Point(this.ClientSize.Width - btnSave.Width - 24,
                                         this.ClientSize.Height - btnSave.Height - 22);
            btnSave.BackColor = Color.FromArgb(0, 120, 60);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnSave.Text = "💾 儲存";

            // ── 提示標籤：固定在儲存按鈕左邊，不再超出或被擋住 ──
            lblHint = new Label
            {
                AutoSize = false,
                Width = btnSave.Left - left - 12,
                Height = btnSave.Height,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.Gray,
                Text = "提示：Tab 切換欄位，Enter 儲存",
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                Location = new Point(left, btnSave.Top)
            };
            this.Controls.Add(lblHint);

            // ── Tab 順序 ──
            txtWord.TabIndex = 0;
            txtPhonogram.TabIndex = 1;
            txtExplain.TabIndex = 2;
            txtSoundPath.TabIndex = 3;
            btnBrowse.TabIndex = 4;
            btnSave.TabIndex = 5;

            // ── Enter 儲存、Esc 取消 ──
            this.KeyPreview = true;
            this.KeyDown += FrmEditWord_KeyDown;

            // ── ToolTip ──
            ToolTip tt = new ToolTip();
            tt.SetToolTip(txtWord, "英文單字（唯讀，不可修改）");
            tt.SetToolTip(txtPhonogram, "輸入音標，可留空");
            tt.SetToolTip(txtExplain, "輸入中文解釋");
            tt.SetToolTip(txtSoundPath, "音效檔路徑");
            tt.SetToolTip(btnBrowse, "點選瀏覽音效檔案");
            tt.SetToolTip(btnSave, "儲存（也可直接按 Enter）");

            // ── GroupBox 美化 ──
            foreach (Control c in this.Controls)
            {
                if (c is GroupBox grp)
                {
                    grp.ForeColor = Color.FromArgb(0, 80, 160);
                    grp.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                }
            }
        }

        // ── 模擬提示文字 ──
        private void SetHintText(TextBox tb, string hint)
        {
            if (!string.IsNullOrEmpty(tb.Text)) return;
            tb.Text = hint;
            tb.ForeColor = Color.Gray;
            tb.GotFocus += (s, e) =>
            {
                if (tb.ForeColor == Color.Gray) { tb.Text = ""; tb.ForeColor = Color.Black; }
            };
            tb.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tb.Text)) { tb.Text = hint; tb.ForeColor = Color.Gray; }
            };
        }

        // ── 字元計數 ──
        private void UpdateCharCount()
        {
            if (lblHint == null) return;
            int wLen = (txtWord.ForeColor == Color.Gray) ? 0 : txtWord.Text.Length;
            int eLen = (txtExplain.ForeColor == Color.Gray) ? 0 : txtExplain.Text.Length;
            if (wLen > 0 || eLen > 0)
                lblHint.Text = $"單字 {wLen} 字元  ／  解釋 {eLen} 字元";
        }

        // ── 瀏覽音檔 ──
        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "選擇音效檔案";
                dlg.Filter = "音效檔|*.mp3;*.wav;*.wma|所有檔案|*.*";
                dlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtSoundPath.Text = dlg.FileName;
                    txtSoundPath.ForeColor = Color.Black;
                    lblHint.Text = "✔ 已選取音效檔";
                    lblHint.ForeColor = Color.SeaGreen;
                }
            }
        }

        // ── Enter 儲存 / Esc 取消 ──
        private void FrmEditWord_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                btnSave.PerformClick();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }
    }
}