using DevExpress.XtraCharts.Native;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer
{
    internal class ItemIO
    {
        /// <summary>
        /// Item 데이터 조회
        /// </summary>
        /// <returns></returns>
        public static DataTable Select(int day, string ItemName, string Type, string ChaName, DateTime dteFrom, DateTime dteTo)
        {
            string sql = string.Empty;
            sql += "SELECT ItemKey, ";
            sql += "       cha_level, ";
            sql += "       cha_name, ";
            sql += "       use_name, ";
            sql += "       clan_name, ";
            sql += "       type, ";
            sql += "       count, ";
            sql += "       item_count, ";
            sql += "       item_name, ";
            sql += "       item_new_objid, ";
            sql += "       item_objid, ";
            sql += "       DATETIME(time_string) AS time_string ";
            sql += " FROM Item ";
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
            if (!string.IsNullOrWhiteSpace(ItemName))
            {
                sql += $"AND item_name LIKE '%{ItemName}%' ";
            }
            if (!string.IsNullOrWhiteSpace(Type))
            {
                sql += $"AND type LIKE '%{Type}%' ";
            }
            if (!string.IsNullOrWhiteSpace(ChaName))
            {
                sql += $"AND cha_name LIKE '%{ChaName}%' ";
            }
            sql += "ORDER BY time_string DESC ";

            return DBManager.Instance.GetDataTable(sql);
        }

        /// <summary>
        /// 데이터 로드용 빈 테이블
        /// </summary>
        /// <returns></returns>
        public static DataTable tempItem()
        {
            string sql = string.Empty;
            sql += "SELECT * FROM Item WHERE 1 = 2 ";

            return DBManager.Instance.GetDataTable(sql);
        }

        /// <summary>
        /// 30일이 지난 아이템 로그 삭제
        /// </summary>
        /// <returns></returns>
        public static void deleteItem()
        {
            string sql = string.Empty;
            sql += "DELETE FROM Item ";
            sql += $"WHERE DATETIME(time_string) <= datetime(CURRENT_TIMESTAMP, '-{Program.Option.saveDays} days') ";

            DBManager.Instance.ExcuteDataUpdate(sql);
        }

        /// <summary>
        /// 폴더 날짜 확인
        /// </summary>
        /// <returns></returns>
        public static DataTable GetRecentDate()
        {
            string sql = string.Empty;
            sql += "SELECT ItemFileName, ";
            sql += "       ItemFolderName ";
            sql += "  FROM RecentItemFile ";

            return DBManager.Instance.GetDataTable(sql);
        }
    }
}
