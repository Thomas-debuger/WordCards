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
        public WordItem Word { get; set; } = null;
        public frmEditWord(WordItem word)
        {
            InitializeComponent();

            this.Word = word;
            txtWord.Text = word.Word;
            txtPhonogram.Text = word.Phonogram;
            txtSoundPath.Text = word.SoundPath;
            txtExplain.Text = word.Explain;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // 儲存單字
            Word.Word = txtWord.Text;
            Word.Phonogram = txtPhonogram.Text;
            Word.SoundPath = txtSoundPath.Text;
            Word.Explain = txtExplain.Text;
        }
    }
}
