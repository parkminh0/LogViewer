using DevExpress.LookAndFeel;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using System.Net;
using DevExpress.CodeParser;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraBars.Ribbon;
using DevExpress.Pdf.Native;

namespace LogViewer
{
    public partial class MainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        #region 폼 로드
        /// <summary>
        /// 
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 폼 로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.UserSkin != "")
                defaultLookAndFeel1.LookAndFeel.SkinName = Properties.Settings.Default.UserSkin;
        }

        /// <summary>
        /// 폼 쇼운
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Shown(object sender, EventArgs e)
        {
            Application.DoEvents();
            SplashScreenManager.CloseForm(false);
            ChattingIO.deleteChat();
            ItemIO.deleteItem();
        }

        /// <summary>
        /// 시작준비
        ///  1. DB Connection
        ///  2. Ready Screen
        /// </summary>
        public string Splash()
        {
            string InitLoadComplete = string.Empty;
            SplashScreenManager.Default.SendCommand(SplashScreen1.SplashScreenCommand.SetProgress, 10);

            if (Program.constance.DBConnectInSplash) // 스플래쉬에서 DB 커넥션
            {
                string networkMsg = string.Empty;
                if (string.IsNullOrEmpty(networkMsg))
                {
                    SplashScreenManager.Default.SendCommand(SplashScreen1.SplashScreenCommand.SetStatus, "Database Connection...");
                    DataTable dt = DBManager.Instance.GetDataTable(Program.constance.DBTestQuery);  //DB Connection
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        SplashScreenManager.Default.SendCommand(SplashScreen1.SplashScreenCommand.SetProgress, 30);
                        SplashScreenManager.Default.SendCommand(SplashScreen1.SplashScreenCommand.SetStatus, "Ready to database and user screen...");
                        FormItem frm = new FormItem();
                        frm.Show();
                        frm.Close();
                        SplashScreenManager.Default.SendCommand(SplashScreen1.SplashScreenCommand.SetProgress, 40);
                    }
                    else
                    {
                        InitLoadComplete = "데이터베이스 오류가 발생하였습니다." + "\r\n" + DBManager.DBMErrString;
                        return InitLoadComplete;
                    }
                }
                else
                {
                    InitLoadComplete = networkMsg;
                    return InitLoadComplete;
                }

                SplashScreenManager.Default.SendCommand(SplashScreen1.SplashScreenCommand.SetStatus, "Loading Common data...");
                SplashScreenManager.Default.SendCommand(SplashScreen1.SplashScreenCommand.SetProgress, 100);
            }

            return InitLoadComplete;
        }

        /// <summary>
        /// 창이 열려있는지 검사
        /// </summary>
        /// <param name="formName"></param>
        /// <returns></returns>
        public bool isNewForm(string formName)
        {
            //foreach (Form theForm in this.MdiChildren)    // 현재 MainForm의 MdiChildren에 해당하는 폼만 체크
            foreach (Form theForm in Application.OpenForms) // 현재 프로그램에 있는 모든 열려있는 폼 체크
            {
                if (formName == theForm.Name)
                {
                    theForm.BringToFront();
                    theForm.Focus();
                    return false;
                }
            }
            return true;
        }
        #endregion

        /// <summary>
        /// 폼 열기 버튼 클릭 시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenForm_ItemClick(object sender, ItemClickEventArgs e)
        {
            BarButtonItem bbtnitem = e.Item as BarButtonItem;

            if (isNewForm(bbtnitem.Description))
            {
                switch (bbtnitem.Description)
                {
                    case "FormItem":
                        new FormItem() { MdiParent = this }.Show();
                        break;
                    case "FormChatting":
                        new FormChatting() { MdiParent = this }.Show();
                        break;
                    default:
                        break;
                }
            }
            SplashScreenManager.CloseForm(false);
        }

        #region 종료
        /// <summary>
        /// 프로그램 종료시
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            // 자동 로드 여부 확인 후 
            if (Program.isBusy)
            {
                XtraMessageBox.Show("현재 자동 데이터 로딩중입니다.\r\n잠시 후 다시 시도해주세요.", "수동 로드", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (XtraMessageBox.Show("프로그램을 종료하시겠습니까?", "프로그램 종료", MessageBoxButtons.YesNo, MessageBoxIcon.Information) != System.Windows.Forms.DialogResult.Yes)
                e.Cancel = true;
            else
                Program.SaveConfig();
        }

        /// <summary>
        /// 종료
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bbtnExit_ItemClick(object sender, ItemClickEventArgs e)
        {
            Close();
        }
        #endregion

        /// <summary>
        /// 자동로드 여부 및 Timer 실행
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        System.Timers.Timer timer = new System.Timers.Timer();
        private bool tryCancel = false;
        private void btglLoadType_CheckedChanged(object sender, ItemClickEventArgs e)
        {
            if (btglLoadType.Checked == true)
            {
                if (tryCancel)
                    return;

                tryCancel = false;
                start_background();
                timer.Interval = 30 * 60 * 1000; // 30분 간격
                timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                timer.Start();
            }
            else
            {
                // 자동 로드 여부 확인 후 
                if (Program.isBusy)
                {
                    XtraMessageBox.Show("현재 자동 데이터 로딩중입니다.\r\n잠시 후 다시 시도해주세요.", "수동 로드", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    tryCancel = true;
                    btglLoadType.Checked = true;
                    return;
                }

                tryCancel = false;
                timer.Stop();
            }
        }

        /// <summary>
        /// Timer 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            start_background();
        }

        /// <summary>
        /// 백그라운드 실행
        /// </summary>
        private void start_background()
        {
            Program.isBusy = true;
            
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true; //진척 보고?
            worker.WorkerSupportsCancellation = true; //취소 가능?

            // 이벤트 핸들러 지정
            worker.DoWork += new DoWorkEventHandler(worker_Dowork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            // 작업쓰레드 시작
            worker.RunWorkerAsync();
        }

        /// <summary>
        /// 로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void worker_Dowork(object sender, DoWorkEventArgs e)
        {
            bProgress.Visibility = BarItemVisibility.Always;

            LoadItem.ReadFolderAndFile();
            LoadChatting.ReadFolderAndFile();
        }

        /// <summary>
        /// busy 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ChattingIO.deleteChat();
            ItemIO.deleteItem();

            Program.isBusy = false;

            bProgress.Visibility = BarItemVisibility.Never;
        }

        /// <summary>
        /// 경로 설정/기록 보관 기간 설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bbtnSetting_ItemClick(object sender, ItemClickEventArgs e)
        {
            FormPopSetting frm = new FormPopSetting();
            frm.ShowDialog();
        }
    }
}