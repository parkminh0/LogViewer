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
    internal class LoadChatting
    {
        /// <summary>
        /// 폴더 읽기(자동 로드)
        /// </summary>
        public static void ReadFolderAndFile()
        {
            DirectoryInfo directoryinfo = new DirectoryInfo(Program.Option.filepath + "\\chatting");

            try
            {
                // 폴더 날짜 확인
                DataTable latestChat = ChattingIO.GetRecentDate();
                List<string> FolderArray = new List<string>();

                // DB 상 제일 최신 기록보다 크거나 같은 날짜 폴더만 가져옴
                foreach (DirectoryInfo folder in directoryinfo.GetDirectories())
                {
                    string dir = folder.Name.ToString();

                    if (DateTime.Parse(dir) >= DateTime.Parse(latestChat.Rows.Count == 0 ? DateTime.MinValue.ToString() : latestChat.Rows[0]["ChatFolderName"].ToString()))
                        FolderArray.Add(dir);
                }

                foreach (string folder in FolderArray)
                {
                    // 폴더 내 모든 파일 배열화
                    string[] fileArray = Directory.GetFiles(Program.Option.filepath + $"\\chatting\\{folder}", "*.*", SearchOption.AllDirectories);

                    // DB 상 가장 최신 기록보다 큰 날짜 파일만 
                    foreach (string file in fileArray)
                    {
                        string[] filename = file.Substring(0, file.Length - 4).Split('\\');
                        if (DateTime.Parse(filename[7]) <= DateTime.MinValue)
                        {
                            XtraMessageBox.Show($"[자동로드] 중 문제가 발생하였습니다.\r\n{filename[7]} 파일을 확인해주세요.", "채팅 자동 로드", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        if (DateTime.Parse(filename[7]) > DateTime.Parse(latestChat.Rows.Count == 0 ? DateTime.MinValue.ToString() : latestChat.Rows[0]["ChatFileName"].ToString()))
                        {
                            try
                            {
                                DataTable tempChatting = ReadFile(file, filename[7]);
                                if (tempChatting == null || tempChatting.Rows.Count == 0)
                                {
                                    XtraMessageBox.Show($"[자동로드] 중 문제가 발생하였습니다.\r\n{filename[7]} 파일을 확인해주세요.", "채팅 자동 로드", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    return;
                                }

                                DataTable dt = DBManager.Instance.GetDataTable("SELECT * FROM RecentChatFile WHERE 1 = 2 ");
                                DataRow row = dt.NewRow();
                                row[0] = filename[7];
                                row[1] = filename[7].Substring(0, 10);
                                dt.Rows.Add(row);
                                DBManager.Instance.DoBulkCopyTI(tempChatting, "Chatting", filename[7], "chatFileName");
                                DBManager.Instance.DoBulkCopyTI(dt, "RecentChatFile");
                                Program.Option.RecentChatLoad = filename[7];
                                Program.SaveConfig();
                            }
                            catch (Exception ex)
                            {
                                XtraMessageBox.Show($"[자동로드] 중 문제가 발생하였습니다.\r\n{filename[7]} 파일을 확인해주세요.", "채팅 자동 로드", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            DataTable dtChatting = ChattingIO.tempChatting();

            // 컬럼 설정
            List<string> ChattingColumns = new List<string>();
            foreach (DataColumn dataColumn in dtChatting.Columns)
                ChattingColumns.Add(dataColumn.ToString());

            // 한 문장 씩 배열에 넣음
            string[] jsonStringArray = Regex.Split(json, Environment.NewLine);

            foreach (string strJSONarr in jsonStringArray)
            {
                string[] RowData = strJSONarr.Split('\t');

                // 빈 문장일 경우 pass
                if (RowData.Length == 1)
                    continue;

                DataRow nr = dtChatting.NewRow();
                for (int i = 0; i < RowData.Length; i++)
                {
                    try
                    {
                        if (i == 0) //time_string의 맨 처음은 [ 으로 시작하기 때문에 제거
                            nr[i + 2] = RowData[i].Substring(1, RowData[i].Length - 1);
                        else
                            nr[i + 2] = RowData[i];
                    }
                    catch (Exception e)
                    {
                        XtraMessageBox.Show($"[채팅 파일 로드 오류]\r\r\n[파일명: {fileName}]\r\r\n" + e.Message, "자동 로드 오류", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                        return null;
                    }
                }

                nr[1] = fileName;
                dtChatting.Rows.Add(nr);

            }

            return dtChatting;
        }
    }
}
