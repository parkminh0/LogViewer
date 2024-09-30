using DevExpress.Entity.ProjectModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer
{
    internal class ChattingIO
    {
        /// <summary>
        /// Chatting 데이터 조회
        /// </summary>
        /// <returns></returns>
        public static DataTable Select(int day, string ChaName, DateTime dteFrom, DateTime dteTo)
        {
            string sql = string.Empty;
            sql += "SELECT chatKey, ";
            sql += "       account_ip, ";
            sql += "       account_id, ";
            sql += "       cha_name, ";
            sql += "       clan_name, ";
            sql += "       type, ";
            sql += "       chat_content, ";
            sql += "       DATETIME(time_string) AS time_string ";
            sql += "  FROM Chatting ";
            sql += " WHERE 1 = 1 ";
            switch (day)
            {
                case 0:
                    sql += $"AND DATE(time_string) = '{DateTime.Now.ToString("yyyy-MM-dd")}' ";
                    break;
                case 1:
                    sql += $"AND DATE(time_string) = '{DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd")}' ";
                    break;
                case 2:
                    sql += $"AND DATE(time_string) BETWEEN '{dteFrom.ToString("yyyy-MM-dd")}' AND '{dteTo.ToString("yyyy-MM-dd")}' ";
                    break;
            }
            if (!string.IsNullOrWhiteSpace(ChaName))
            {
                sql += $"AND cha_name LIKE '%{ChaName}%' ";
            }
            sql += " ORDER BY time_string DESC ";

            return DBManager.Instance.GetDataTable(sql);
        }

        /// <summary>
        /// 데이터 로드용 빈 테이블
        /// </summary>
        /// <returns></returns>
        public static DataTable tempChatting()
        {
            string sql = string.Empty;
            sql += "SELECT * ";
            sql += "  FROM Chatting ";
            sql += " WHERE 1 = 2 ";

            return DBManager.Instance.GetDataTable(sql);
        }

        /// <summary>
        /// 폴더 날짜 확인
        /// </summary>
        /// <returns></returns>
        public static DataTable GetRecentDate()
        {
            string sql = string.Empty;
            sql += "SELECT ChatFileName, ";
            sql += "       ChatFolderName ";
            sql += "  FROM RecentChatFile ";

            return DBManager.Instance.GetDataTable(sql);
        }

        /// <summary>
        /// 채팅 기록 삭제
        /// </summary>
        public static void deleteChat()
        {
            string sql = string.Empty;
            sql += "DELETE FROM Chatting ";
            sql += $"WHERE DATETIME(time_string) <= DATETIME(CURRENT_TIMESTAMP, '-{Program.Option.saveDays} days') ";

            DBManager.Instance.ExcuteDataUpdate(sql);
        }
    }
}
