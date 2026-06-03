using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordCards
{
    internal class WordCollection : Collection<WordItem>
    {
        /// <summary>
        /// 從字串陣列載入資料
        /// </summary>
        /// <param name="lines">輸入的單字字串陣列</param>
        public void LoadFromStringArray(string[] lines)
        {
            this.Clear(); // 清空現有的資料

            // 將字串陣列的資料載入到 WordCollection 物件中
            foreach (string line in lines)
            {
                // 產生 WordItem 物件
                WordItem item = new WordItem(line);
                this.Add(item);
            }
        }
        ///<summary>
        ///將WordCollection物件的資料儲存到檔案中
        ///</summary>
        ///<paramname="filePath"></param>
        public void SaveToFile(string filePath)
        {
            // 將WordCollection物件的資料儲存到檔案中
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (WordItem item in this)
                {
                    // 將每個單字項目轉換為字串並寫入檔案
                    writer.WriteLine(item.ToLineString());
                }
            }
        }
    }
}
