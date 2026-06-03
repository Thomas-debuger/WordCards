using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordCards
{
    public class WordItem
    {
        public string Word { get; set; }
        public string Phonogram { get; set; }
        public string SoundPath { get; set; }
        public string Explain { get; set; }
        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name=“str”>單行的單字資料</param>
        public WordItem(string str)
        { // 用Tab分隔字串
            string[] strLists = str.Split('\t');
            if (strLists.Length >= 3)
            {
                Word = strLists[0];
                Phonogram = strLists[1];
                SoundPath = strLists[2];
                Explain = string.Join(Environment.NewLine, strLists.Skip(3));
            }
        }
        /// <summary>
        /// 覆寫 ToString() 可將物件自動轉換為字串
        /// </summary>
        /// <returns>單字</returns>
        public override string ToString()
        {
            return Word;
        }

        ///<summary>
        ///將WordItem物件轉換為字串/// 格式為"Word\tPhonogram\tSoundPath\tExplain"
        ///</summary>
        /// <returns>字串，格式為"Word\tPhonogram\tSoundPath\tExplain"</returns>
        public string ToLineString()
        {
            // 將Explain 屬性中的換行符號替換為\t，以便在字串中顯示。
            string strExplain = Explain.Replace(Environment.NewLine, "\t");
            // 將WordItem物件轉換為字串
            return $"{Word}\t{Phonogram}\t{SoundPath}\t{strExplain}";
        }
    }
}
