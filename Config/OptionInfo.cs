using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogViewer
{
    public class OptionInfo
    {
        /// <summary>
        /// 
        /// </summary>
        string loginID;
        public string LoginID
        {
            get { return loginID; }
            set
            {
                loginID = value;
            }
        }

        /// <summary>
        /// 자동로드로 최근 로드한 item file name
        /// </summary>
        private string _RecentItemLoad;

        public string RecentItemLoad
        {
            get { return _RecentItemLoad; }
            set { _RecentItemLoad = value; }
        }

        /// <summary>
        /// 자동로드로 최근 로드한 chatting file name
        /// </summary>
        private string _RecentChatLoad;

        public string RecentChatLoad
        {
            get { return _RecentChatLoad; }
            set { _RecentChatLoad = value; }
        }

        /// <summary>
        /// 파일 경로
        /// </summary>
        private string _filepath = "C:\\Users\\Administrator\\Desktop\\log";

        public string filepath
        {
            get { return _filepath; }
            set { _filepath = value; }
        }

        /// <summary>
        /// 기록 보관 기간
        /// </summary>
        private int _saveDays = 90;

        public int saveDays
        {
            get { return _saveDays; }
            set { _saveDays = value; }
        }
    }
}
