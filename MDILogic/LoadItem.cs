using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogViewer
{
    internal class LoadItem
    {
        /// <summary>
        /// 폴더 읽기(자동 로드)
        /// </summary>
        public static void ReadFolderAndFile()
        {
            DirectoryInfo directoryinfo = new DirectoryInfo(Program.Option.filepath + "\\item");

            try
            {
                // 폴더명 확인
                DataTable latestItem = ItemIO.GetRecentDate();
                List<string> FolderArray = new List<string>();

                foreach (DirectoryInfo folder in directoryinfo.GetDirectories())
                {
                    string dir = folder.Name.ToString();

                    if (DateTime.Parse(dir) >= DateTime.Parse(latestItem.Rows.Count == 0 ? DateTime.MinValue.ToString() : latestItem.Rows[0]["ItemFolderName"].ToString()))
                        FolderArray.Add(dir);
                }

                // 폴더 안 파일에 대해
                foreach (string folder in FolderArray)
                {
                    string[] fileArray = Directory.GetFiles(Program.Option.filepath + $"\\item\\{folder}", "*.*", SearchOption.AllDirectories);

                    foreach (string file in fileArray)
                    {
                        string[] filename = file.Substring(0, file.Length - 4).Split('\\');
                        if (DateTime.Parse(filename[7]) <= DateTime.MinValue)
                        {
                            XtraMessageBox.Show($"[자동로드] 중 문제가 발생하였습니다.\r\n{filename[7]} 파일을 확인해주세요.", "아이템 자동 로드", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        if (DateTime.Parse(filename[7]) > DateTime.Parse(latestItem.Rows.Count == 0 ? DateTime.MinValue.ToString() : latestItem.Rows[0]["ItemFileName"].ToString()))
                        {
                            try
                            {
                                DataTable tempItem = ReadFile(file, filename[7]);
                                if (tempItem == null || tempItem.Rows.Count == 0)
                                {
                                    XtraMessageBox.Show($"[자동로드] 중 문제가 발생하였습니다.\r\n{filename[7]} 파일을 확인해주세요.", "아이템 자동 로드", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    return;
                                }

                                DataTable dt = DBManager.Instance.GetDataTable("SELECT * FROM RecentItemFile WHERE 1 = 2 ");
                                DataRow row = dt.NewRow();
                                row[0] = filename[7];
                                row[1] = filename[7].Substring(0, 10);
                                dt.Rows.Add(row);
                                DBManager.Instance.DoBulkCopyTI(tempItem, "Item", filename[7], "ItemFileName");
                                DBManager.Instance.DoBulkCopyTI(dt, "RecentItemFile");
                                Program.Option.RecentItemLoad = filename[7];
                                Program.SaveConfig();
                            }
                            catch (Exception ex)
                            {
                                XtraMessageBox.Show($"[자동로드] 중 문제가 발생하였습니다.\r\n{filename[7]} 파일을 확인해주세요.", "아이템 자동 로드", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 파일 읽기
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static DataTable ReadFile(string filePath, string fileName)
        {
            // JSON 읽기
            string json = File.ReadAllText(filePath);

            // 저장용 빈 테이플
            DataTable dtItem = ItemIO.tempItem();

            // 컬럼 셋팅
            List<string> ItemColumns = new List<string>();
            foreach (DataColumn dataColumn in dtItem.Columns)
                ItemColumns.Add(dataColumn.ToString());

            // 한 문장 씩 배열에 넣음
            string[] jsonStringArray = Regex.Split(json.Replace(";", ""), Environment.NewLine);

            foreach (string strJSONarr in jsonStringArray)
            {
                string[] RowData = Regex.Split(strJSONarr.Replace("{", "").Replace("}", ""), ",");

                // 빈 문장일 경우 pass
                if (RowData.Length == 1)
                    continue;

                DataRow nr = dtItem.NewRow();
                foreach (string rowData in RowData)
                {
                    try
                    {
                        int idx = rowData.IndexOf(":");
                        if (idx == -1)
                            continue;

                        string RowColumns = rowData.Substring(0, idx).Replace("\"", "").Trim();
                        string RowDataString = rowData.Substring(idx + 1).Replace("\"", "").Trim();
                        if (!ItemColumns.Contains(RowColumns))
                            continue;

                        nr[RowColumns] = RowDataString;
                    }
                    catch (Exception e)
                    {
                        XtraMessageBox.Show($"[아이템 파일 로드 오류]\r\r\n[파일명: {fileName}]\r\r\n" + e.Message, "자동 로드 오류", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return null;
                    }
                }

                nr[1] = fileName;
                dtItem.Rows.Add(nr);
            }

            return dtItem;
        }
    }
}
