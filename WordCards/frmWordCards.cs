using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;

namespace WordCards
{
    public partial class frmWordCards : Form
    {
        // ═══════════════════════════════════════════════
        //  原有欄位（保留不動）
        // ═══════════════════════════════════════════════
        WordCollection _WordList = new WordCollection();
        WindowsMediaPlayer wmp = new WindowsMediaPlayer();
        string strWordFile = "WordCards.txt";
        bool isPlay = false;

        // ═══════════════════════════════════════════════
        //  【創意加分】新增欄位
        // ═══════════════════════════════════════════════

        // ── 學習統計 ──
        int _correctCount = 0;   // 答對次數
        int _wrongCount = 0;    // 答錯次數
        int _studySeconds = 0;  // 學習秒數（計時器每秒 +1）
        Timer _studyTimer = new Timer();

        // ── 暗夜模式 ──
        bool _darkMode = false;

        // ── 字體縮放 ──
        float _wordFontSize = 36f;

        // ── 隨機出題（克漏字）模式 ──
        bool _quizMode = false;
        string _currentAnswer = "";

        // ── 搜尋關鍵字高亮 ──
        string _searchKeyword = "";

        // ── 播放速度（AutoPlay 間隔）──
        int _playIntervalMs = 3000; // 預設 3 秒

        public frmWordCards()
        {
            InitializeComponent();
            InitCreativeFeatures(); // 初始化創意功能
        }

        // ═══════════════════════════════════════════════
        //  【創意加分】初始化
        // ═══════════════════════════════════════════════
        private void InitCreativeFeatures()
        {
            // ── 1. 介面美化：設定漸層背景 ──
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);

            // ── 2. 學習計時器（每秒更新）──
            _studyTimer.Interval = 1000;
            _studyTimer.Tick += StudyTimer_Tick;
            _studyTimer.Start();

            // ── 3. 工具提示（貼心輔助）──
            ToolTip tt = new ToolTip();
            tt.SetToolTip(btnAutoPlay, "自動循環播放單字（也可按 Enter 手動下一個）");
            tt.SetToolTip(txtWord, "目前顯示的英文單字");
            tt.SetToolTip(txtPhonogram, "音標");
            tt.SetToolTip(txtExplain, "中文解釋");
            tt.SetToolTip(lstWordList, "雙擊可編輯單字；單擊可播放發音");

            // ── 4. 為 lstWordList 加上繪製事件（斑馬紋）──
            lstWordList.DrawMode = DrawMode.OwnerDrawFixed;
            lstWordList.DrawItem += LstWordList_DrawItem;

            // ── 5. 建立搜尋框（貼心輔助：即時搜尋）──
            BuildSearchBox();

            // ── 6. 建立功能按鈕列（暗夜模式、字體縮放、測驗模式、播速）──
            BuildExtraToolBar();

            // ── 7. Tab 順序（鍵盤操作）──
            txtWord.TabIndex = 0;
            txtPhonogram.TabIndex = 1;
            txtExplain.TabIndex = 2;
            lstWordList.TabIndex = 3;
            btnAutoPlay.TabIndex = 4;

            // ── 8. 狀態列更多資訊欄位 ──
            BuildStatusBarExtras();

            // ── 9. 版面修正：避免按鈕列、說明文字與解釋框互相重疊 ──
            SetupResponsiveLayout();
        }

        // ═══════════════════════════════════════════════
        //  原有方法（完全保留）
        // ═══════════════════════════════════════════════
        private void ShowWord(WordItem word)
        {
            if (_quizMode)
            {
                // 【測驗模式】：隱藏中文解釋，讓使用者猜
                _currentAnswer = word.Explain;
                txtWord.Text = word.Word;
                txtPhonogram.Text = word.Phonogram;
                txtExplain.Text = "？？？  （按 Q 顯示答案）";
                txtExplain.ForeColor = Color.Gray;
            }
            else
            {
                txtWord.Text = word.Word;
                txtPhonogram.Text = word.Phonogram;
                txtExplain.Text = word.Explain;
                txtExplain.ForeColor = _darkMode ? Color.LightYellow : Color.Black;
            }

            // 【動畫效果】：單字文字淡入（透過 Timer 模擬）
            AnimateWordAppear();
        }

        private void UpdateWordList()
        {
            lstWordList.BeginUpdate();
            lstWordList.Items.Clear();
            foreach (WordItem item in this._WordList)
            {
                lstWordList.Items.Add(item);
            }
            lstWordList.EndUpdate();
        }

        private void PlayWord(WordItem word)
        {
            if (File.Exists(word.SoundPath))
            {
                wmp.URL = word.SoundPath;
                wmp.settings.autoStart = false;
                wmp.settings.mute = false;
                wmp.controls.play();
            }
            else
                UpdateStatusMessage($"找無 {word.SoundPath} 音效檔");
        }

        private void frmWordCards_Load(object sender, EventArgs e)
        {
            string[] lines;
            if (File.Exists(strWordFile))
            {
                lines = File.ReadAllLines(strWordFile, Encoding.UTF8);
            }
            else
            {
                MessageBox.Show($"找不到單字檔\n{strWordFile}", "錯誤",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            this._WordList.LoadFromStringArray(lines);
            if (this._WordList.Count > 0)
            {
                UpdateWordList();
                this.ShowWord(_WordList[0]);
                UpdateStatusMessage($"單字數量：{_WordList.Count}");
            }

            // 【創意加分】：載入後套用主題
            ApplyTheme();
        }

        private void PlaySelectedWord()
        {
            if (lstWordList.SelectedItem != null)
            {
                int idx = lstWordList.SelectedIndex;
                ShowWord(_WordList[idx]);
                PlayWord(_WordList[idx]);
            }
        }

        private void NextWordList()
        {
            lstWordList.Focus();
            if (lstWordList.SelectedIndex + 1 >= lstWordList.Items.Count)
                lstWordList.SelectedIndex = 0;
            else
                lstWordList.SelectedIndex++;
            int lstRows = lstWordList.Height / lstWordList.GetItemHeight(0);
            if (lstWordList.SelectedIndex >= lstRows / 2)
                lstWordList.TopIndex = lstWordList.SelectedIndex - lstRows / 2;
        }

        private void lstWordList_Click(object sender, EventArgs e)
        {
            if (isPlay == true)
                btnAutoPlay.PerformClick();
            if (lstWordList.SelectedItem != null)
                if (lstWordList.SelectedItem.ToString().Length != 0)
                    PlaySelectedWord();
        }

        private void timPlayer_Tick(object sender, EventArgs e)
        {
            NextWordList();
            PlaySelectedWord();
        }

        private void btnAutoPlay_Click(object sender, EventArgs e)
        {
            lstWordList.Focus();
            if (isPlay == false)
            {
                btnAutoPlay.Text = "⏹ Stop";
                isPlay = true;
                PlaySelectedWord();
                timPlayer.Interval = _playIntervalMs; // 【創意】套用播速
                timPlayer.Start();
                UpdateStatusMessage("自動播放中… 點擊清單可暫停");
            }
            else
            {
                btnAutoPlay.Text = "▶ Play";
                isPlay = false;
                timPlayer.Stop();
                UpdateStatusMessage($"已暫停｜單字數：{_WordList.Count}");
            }
        }

        private void frmWordCards_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (isPlay == true) return;

            switch (e.KeyChar)
            {
                case (char)Keys.Return:
                    NextWordList();
                    PlaySelectedWord();
                    e.Handled = true;
                    break;

                case (char)Keys.Space:
                    if (lstWordList.SelectedIndex >= 0)
                        PlaySelectedWord();
                    e.Handled = true;
                    break;

                // 【創意加分】Q 鍵顯示測驗答案
                case 'q':
                case 'Q':
                    if (_quizMode && lstWordList.SelectedIndex >= 0)
                    {
                        txtExplain.Text = _currentAnswer;
                        txtExplain.ForeColor = Color.LimeGreen;
                        UpdateStatusMessage("答案揭曉！按 ↑/↓ 自行評分");
                    }
                    e.Handled = true;
                    break;

                // 【創意加分】+ / - 調整字體大小
                case '+':
                case '=':
                    AdjustWordFontSize(2f);
                    e.Handled = true;
                    break;
                case '-':
                    AdjustWordFontSize(-2f);
                    e.Handled = true;
                    break;
            }
        }

        // 【創意加分】方向鍵答對 / 答錯記錄
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_quizMode && !isPlay)
            {
                if (keyData == Keys.Up)
                {
                    _correctCount++;
                    UpdateStatusMessage($"✔ 答對！累計答對：{_correctCount}  答錯：{_wrongCount}");
                    NextWordList();
                    PlaySelectedWord();
                    return true;
                }
                if (keyData == Keys.Down)
                {
                    _wrongCount++;
                    UpdateStatusMessage($"✘ 答錯！累計答對：{_correctCount}  答錯：{_wrongCount}");
                    NextWordList();
                    PlaySelectedWord();
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void lstWordList_DoubleClick(object sender, EventArgs e)
        {
            lstWordList.Focus();
            int idx = lstWordList.SelectedIndex;
            frmEditWord edit = new frmEditWord(_WordList[idx]);
            DialogResult result = edit.ShowDialog(this);
            if (result == DialogResult.Yes)
            {
                PlaySelectedWord();
                _WordList.SaveToFile(strWordFile);
                UpdateStatusMessage("✔ 單字已儲存");
            }
        }

        // ═══════════════════════════════════════════════
        //  【創意加分】美化：斑馬紋列表
        // ═══════════════════════════════════════════════
        private void LstWordList_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            bool selected = (e.State & DrawItemState.Selected) != 0;

            Color bg, fg;
            if (selected)
            {
                bg = _darkMode ? Color.FromArgb(0, 120, 215) : Color.FromArgb(0, 102, 204);
                fg = Color.White;
            }
            else if (e.Index % 2 == 0)
            {
                bg = _darkMode ? Color.FromArgb(40, 40, 50) : Color.FromArgb(240, 245, 255);
                fg = _darkMode ? Color.LightGray : Color.FromArgb(30, 30, 60);
            }
            else
            {
                bg = _darkMode ? Color.FromArgb(30, 30, 40) : Color.White;
                fg = _darkMode ? Color.LightGray : Color.FromArgb(30, 30, 60);
            }

            e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);

            // 搜尋關鍵字高亮
            string text = lstWordList.Items[e.Index].ToString();
            if (!string.IsNullOrEmpty(_searchKeyword) &&
                text.IndexOf(_searchKeyword, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                e.Graphics.DrawString("★ " + text, e.Font, new SolidBrush(Color.Orange),
                    e.Bounds.X + 4, e.Bounds.Y + 2);
            }
            else
            {
                e.Graphics.DrawString(text, e.Font, new SolidBrush(fg),
                    e.Bounds.X + 4, e.Bounds.Y + 2);
            }

            if (selected)
                ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds);
        }

        // ═══════════════════════════════════════════════
        //  【創意加分】暗夜模式切換
        // ═══════════════════════════════════════════════
        private void ApplyTheme()
        {
            Color bg = _darkMode ? Color.FromArgb(25, 25, 35) : Color.FromArgb(245, 248, 255);
            Color fg = _darkMode ? Color.FromArgb(220, 220, 235) : Color.FromArgb(30, 30, 60);
            Color txtBg = _darkMode ? Color.FromArgb(35, 35, 48) : Color.White;
            Color accent = _darkMode ? Color.FromArgb(0, 160, 255) : Color.FromArgb(0, 90, 200);

            this.BackColor = bg;
            this.ForeColor = fg;

            foreach (Control c in this.Controls)
                ApplyThemeToControl(c, bg, fg, txtBg, accent);

            lstWordList.Invalidate(); // 重繪斑馬紋
        }

        private void ApplyThemeToControl(Control c, Color bg, Color fg, Color txtBg, Color accent)
        {
            if (c is TextBox tb)
            {
                tb.BackColor = txtBg;
                tb.ForeColor = fg;
                tb.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (c is Button btn)
            {
                btn.BackColor = accent;
                btn.ForeColor = Color.White;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
            }
            else if (c is ListBox lb)
            {
                lb.BackColor = _darkMode ? Color.FromArgb(30, 30, 42) : Color.White;
                lb.ForeColor = fg;
            }
            else if (c is Panel pnl)
            {
                pnl.BackColor = bg;
                foreach (Control child in pnl.Controls)
                    ApplyThemeToControl(child, bg, fg, txtBg, accent);
            }
            else if (c is Label lbl)
            {
                lbl.ForeColor = _darkMode ? Color.FromArgb(180, 180, 200) : Color.FromArgb(80, 80, 120);
            }
        }

        // ═══════════════════════════════════════════════
        //  【創意加分】建立搜尋框（即時篩選）
        // ═══════════════════════════════════════════════
        private TextBox _txtSearch;
        private Panel _pnlExtra; // 下方功能列，改放在 palMain 內，避免蓋到狀態列與內容

        private void BuildSearchBox()
        {
            // 搜尋框定位在 lstWordList 正上方，緊貼清單左側
            // 注意：InitCreativeFeatures 在 InitializeComponent 之後呼叫，lstWordList 位置已確定

            Label lblSearch = new Label
            {
                Text = "🔍",
                AutoSize = false,
                Width = 22,
                Height = 22,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(lstWordList.Left, lstWordList.Top - 26),
                Font = new Font("Segoe UI", 10f)
            };

            _txtSearch = new TextBox
            {
                Text = "搜尋單字...",
                ForeColor = Color.Gray,
                Location = new Point(lstWordList.Left + 24, lstWordList.Top - 28),
                Width = lstWordList.Width - 24,
                Font = new Font("Segoe UI", 9.5f),
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };

            // .NET Framework 沒有 PlaceholderText，用 GotFocus/LostFocus 模擬
            _txtSearch.GotFocus += (s, ev) =>
            {
                if (_txtSearch.Text == "搜尋單字..." && _txtSearch.ForeColor == Color.Gray)
                {
                    _txtSearch.Text = "";
                    _txtSearch.ForeColor = Color.Black;
                }
            };
            _txtSearch.LostFocus += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(_txtSearch.Text))
                {
                    _txtSearch.Text = "搜尋單字...";
                    _txtSearch.ForeColor = Color.Gray;
                    _searchKeyword = "";
                    lstWordList.Invalidate();
                }
            };
            _txtSearch.TextChanged += TxtSearch_TextChanged;

            // 貼心輔助：ToolTip
            ToolTip tt2 = new ToolTip();
            tt2.SetToolTip(_txtSearch, "即時搜尋單字（橘色★標示符合結果）");

            // 直接加到主表單，不使用 Dock Panel，避免佔用版面
            this.Controls.Add(lblSearch);
            this.Controls.Add(_txtSearch);
            _txtSearch.BringToFront();
            lblSearch.BringToFront();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            // 如果目前是佔位提示文字（灰色），不作搜尋
            if (_txtSearch.ForeColor == Color.Gray) return;
            _searchKeyword = _txtSearch.Text.Trim();
            lstWordList.Invalidate(); // 重繪高亮

            if (!string.IsNullOrEmpty(_searchKeyword))
            {
                // 自動捲動到第一個符合的項目
                for (int i = 0; i < _WordList.Count; i++)
                {
                    if (_WordList[i].Word.IndexOf(_searchKeyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        _WordList[i].Explain.IndexOf(_searchKeyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        lstWordList.SelectedIndex = i;
                        lstWordList.TopIndex = Math.Max(0, i - 3);
                        ShowWord(_WordList[i]);
                        break;
                    }
                }
                UpdateStatusMessage($"搜尋「{_searchKeyword}」中…");
            }
            else
            {
                UpdateStatusMessage($"單字數量：{_WordList.Count}");
            }
        }

        // ═══════════════════════════════════════════════
        //  【創意加分】建立額外工具列
        // ═══════════════════════════════════════════════
        private void BuildExtraToolBar()
        {
            // 原本是 Dock 在整個 Form 底部，容易和 StatusStrip / txtExplain / txtHelp 打架。
            // 改成放進 palMain，並由 LayoutMainControls() 統一計算位置。
            _pnlExtra = new Panel
            {
                Height = 42,
                BackColor = Color.FromArgb(230, 235, 250),
                Padding = new Padding(6, 5, 6, 5),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            // 暗夜模式按鈕
            Button btnDark = new Button
            {
                Text = "🌙 夜間",
                Width = 72,
                Height = 30,
                Location = new Point(8, 6),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Tag = "dark"
            };
            btnDark.Click += BtnDark_Click;

            // 測驗模式按鈕
            Button btnQuiz = new Button
            {
                Text = "📝 測驗",
                Width = 72,
                Height = 30,
                Location = new Point(86, 6),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Tag = "quiz"
            };
            btnQuiz.Click += BtnQuiz_Click;

            // 統計按鈕
            Button btnStats = new Button
            {
                Text = "📊 統計",
                Width = 72,
                Height = 30,
                Location = new Point(164, 6),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Tag = "stats"
            };
            btnStats.Click += BtnStats_Click;

            // 播速滑桿標籤
            Label lblSpeed = new Label
            {
                Text = "播速",
                AutoSize = true,
                Location = new Point(252, 12),
                Font = new Font("Segoe UI", 9f),
                Tag = "speedLabel"
            };

            // 播速滑桿
            TrackBar trkSpeed = new TrackBar
            {
                Minimum = 1,
                Maximum = 10,
                Value = 3,
                TickFrequency = 1,
                Width = 150,
                Height = 34,
                Location = new Point(292, 4),
                Tag = "speed"
            };
            trkSpeed.ValueChanged += TrkSpeed_ValueChanged;

            ToolTip tt3 = new ToolTip();
            tt3.SetToolTip(btnDark, "切換白天 / 夜間模式");
            tt3.SetToolTip(btnQuiz, "測驗模式：隱藏中文解釋，按↑答對 / ↓答錯，按Q顯示答案");
            tt3.SetToolTip(btnStats, "查看學習統計");
            tt3.SetToolTip(trkSpeed, "調整自動播放間隔（1=快 10=慢）");

            _pnlExtra.Controls.AddRange(new Control[] {
                btnDark, btnQuiz, btnStats, lblSpeed, trkSpeed
            });

            palMain.Controls.Add(_pnlExtra);
            _pnlExtra.BringToFront();
        }

        // ═══════════════════════════════════════════════
        //  【版面修正】集中處理右側畫面排版
        // ═══════════════════════════════════════════════
        private void SetupResponsiveLayout()
        {
            // 稍微加大預設視窗，避免中文、Emoji、TrackBar 在不同 DPI 下被切掉。
            if (this.ClientSize.Width < 880 || this.ClientSize.Height < 540)
                this.ClientSize = new Size(Math.Max(this.ClientSize.Width, 880), Math.Max(this.ClientSize.Height, 540));

            this.MinimumSize = new Size(860, 540);

            // 文字區改由程式統一計算高度，避免 Bottom Anchor 跟下方工具列重疊。
            txtWord.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            txtPhonogram.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            txtExplain.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            txtExplain.ScrollBars = ScrollBars.Vertical;

            picLogo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAutoPlay.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtHelp.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            palMain.Resize += (s, e) => LayoutMainControls();
            this.Shown += (s, e) => LayoutMainControls();

            LayoutMainControls();
        }

        private void LayoutMainControls()
        {
            if (palMain.ClientSize.Width <= 0 || palMain.ClientSize.Height <= 0) return;

            int margin = 18;
            int topWord = 18;
            int topPhonogram = 92;
            int topExplain = 146;
            int rightColumnWidth = 126;
            int gap = 18;
            int toolbarHeight = 42;
            int toolbarTop = palMain.ClientSize.Height - toolbarHeight;

            if (_pnlExtra != null)
            {
                _pnlExtra.Location = new Point(0, toolbarTop);
                _pnlExtra.Size = new Size(palMain.ClientSize.Width, toolbarHeight);
                LayoutExtraToolBarControls();
            }

            int contentWidth = Math.Max(220, palMain.ClientSize.Width - rightColumnWidth - gap - margin * 2);
            int rightX = margin + contentWidth + gap;

            txtWord.Location = new Point(margin, topWord);
            txtWord.Size = new Size(contentWidth, 54);

            txtPhonogram.Location = new Point(margin, topPhonogram);
            txtPhonogram.Size = new Size(contentWidth, 36);

            int explainBottom = toolbarTop - 12;
            int explainHeight = Math.Max(120, explainBottom - topExplain);
            txtExplain.Location = new Point(margin, topExplain);
            txtExplain.Size = new Size(contentWidth, explainHeight);

            picLogo.Location = new Point(rightX, 18);
            picLogo.Size = new Size(86, 104);

            btnAutoPlay.Location = new Point(rightX, 140);
            btnAutoPlay.Size = new Size(86, 36);

            txtHelp.Size = new Size(126, 74);
            txtHelp.Location = new Point(rightX, Math.Max(190, toolbarTop - txtHelp.Height - 14));
        }

        private void LayoutExtraToolBarControls()
        {
            if (_pnlExtra == null) return;

            // 按鈕固定在左側，滑桿吃剩餘寬度，避免右邊被切掉。
            Control lblSpeed = null;
            Control trkSpeed = null;
            foreach (Control c in _pnlExtra.Controls)
            {
                if ((string)c.Tag == "speedLabel") lblSpeed = c;
                if ((string)c.Tag == "speed") trkSpeed = c;
            }

            int x = 8;
            foreach (Control c in _pnlExtra.Controls)
            {
                if ((string)c.Tag == "dark" || (string)c.Tag == "quiz" || (string)c.Tag == "stats")
                {
                    c.Location = new Point(x, 6);
                    c.Size = new Size(72, 30);
                    x += 78;
                }
            }

            if (lblSpeed != null)
            {
                lblSpeed.Location = new Point(x + 8, 13);
                x += 46;
            }

            if (trkSpeed != null)
            {
                int w = Math.Max(80, _pnlExtra.ClientSize.Width - x - 12);
                trkSpeed.Location = new Point(x, 4);
                trkSpeed.Size = new Size(w, 34);
            }
        }

        // ═══════════════════════════════════════════════
        //  【創意加分】額外工具列事件
        // ═══════════════════════════════════════════════
        private void BtnDark_Click(object sender, EventArgs e)
        {
            _darkMode = !_darkMode;
            Button btn = sender as Button;
            btn.Text = _darkMode ? "☀️ 白天" : "🌙 夜間";
            ApplyTheme();
            UpdateStatusMessage(_darkMode ? "已切換為夜間模式" : "已切換為白天模式");
        }

        private void BtnQuiz_Click(object sender, EventArgs e)
        {
            _quizMode = !_quizMode;
            Button btn = sender as Button;
            btn.Text = _quizMode ? "📖 背誦" : "📝 測驗";

            if (_quizMode)
            {
                UpdateStatusMessage("測驗模式：↑=答對  ↓=答錯  Q=看答案");
                if (lstWordList.SelectedIndex >= 0)
                    ShowWord(_WordList[lstWordList.SelectedIndex]);
            }
            else
            {
                UpdateStatusMessage($"已切回背誦模式｜答對 {_correctCount} / 答錯 {_wrongCount}");
                if (lstWordList.SelectedIndex >= 0)
                    ShowWord(_WordList[lstWordList.SelectedIndex]);
            }
        }

        private void BtnStats_Click(object sender, EventArgs e)
        {
            int total = _correctCount + _wrongCount;
            double rate = total > 0 ? (double)_correctCount / total * 100 : 0;
            string time = $"{_studySeconds / 3600:D2}:{(_studySeconds % 3600) / 60:D2}:{_studySeconds % 60:D2}";

            string msg =
                $"╔══════════ 學習統計 ══════════╗\n" +
                $"  📚 單字總數：{_WordList.Count} 個\n" +
                $"  ✔  答對次數：{_correctCount} 次\n" +
                $"  ✘  答錯次數：{_wrongCount} 次\n" +
                $"  📈 答對率　：{rate:F1} %\n" +
                $"  ⏱  學習時間：{time}\n" +
                $"╚══════════════════════════════╝";

            MessageBox.Show(msg, "學習統計", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void TrkSpeed_ValueChanged(object sender, EventArgs e)
        {
            TrackBar t = sender as TrackBar;
            // value 1(快)→500ms  value 10(慢)→5000ms
            _playIntervalMs = t.Value * 500;
            if (isPlay) timPlayer.Interval = _playIntervalMs;
            UpdateStatusMessage($"播放間隔設為 {_playIntervalMs / 1000.0:F1} 秒");
        }

        // ═══════════════════════════════════════════════
        //  【創意加分】學習計時器（每秒）
        // ═══════════════════════════════════════════════
        private void StudyTimer_Tick(object sender, EventArgs e)
        {
            _studySeconds++;
            // 每 10 秒更新狀態列顯示學習時間
            if (_studySeconds % 10 == 0)
            {
                string t = $"{_studySeconds / 3600:D2}:{(_studySeconds % 3600) / 60:D2}:{_studySeconds % 60:D2}";
                UpdateStatusMessage($"單字數：{_WordList.Count}  ⏱ {t}  ✔{_correctCount} ✘{_wrongCount}");
            }
        }

        // ═══════════════════════════════════════════════
        //  【創意加分】字體動態縮放
        // ═══════════════════════════════════════════════
        private void AdjustWordFontSize(float delta)
        {
            _wordFontSize = Math.Max(12f, Math.Min(72f, _wordFontSize + delta));
            txtWord.Font = new Font(txtWord.Font.FontFamily, _wordFontSize, FontStyle.Bold);
            UpdateStatusMessage($"字體大小：{_wordFontSize:F0}pt（+/- 調整）");
        }

        // ═══════════════════════════════════════════════
        //  【創意加分】單字出現動畫（淡入效果）
        // ═══════════════════════════════════════════════
        private Timer _animTimer = new Timer();
        private int _animStep = 0;
        private void AnimateWordAppear()
        {
            _animStep = 0;
            _animTimer.Interval = 30;
            _animTimer.Tick -= AnimTimer_Tick; // 防止重複訂閱
            _animTimer.Tick += AnimTimer_Tick;
            _animTimer.Start();
        }
        private void AnimTimer_Tick(object sender, EventArgs e)
        {
            _animStep++;
            // 模擬淡入：逐漸加深文字顏色
            int alpha = Math.Min(255, _animStep * 30);
            Color c = _darkMode
                ? Color.FromArgb(alpha, 200, 220, 255)
                : Color.FromArgb(alpha, 0, 60, 180);
            txtWord.ForeColor = c;

            if (_animStep >= 9)
            {
                txtWord.ForeColor = _darkMode ? Color.FromArgb(200, 220, 255) : Color.FromArgb(0, 60, 180);
                _animTimer.Stop();
            }
        }

        // ═══════════════════════════════════════════════
        //  【創意加分】建立狀態列額外欄位
        // ═══════════════════════════════════════════════
        private void BuildStatusBarExtras()
        {
            // 已在 Designer 中有 tsslMessage；這裡設定字型讓它更好看
            // （如果 Designer 中沒有，可忽略此處）
            try
            {
                tsslMessage.Font = new Font("Segoe UI", 9f);
            }
            catch { }
        }

        // ═══════════════════════════════════════════════
        //  【創意加分】更新狀態訊息（集中管理）
        // ═══════════════════════════════════════════════
        private void UpdateStatusMessage(string msg)
        {
            tsslMessage.Text = msg;
        }
    }
}