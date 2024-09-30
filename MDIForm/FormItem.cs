using DevExpress.DataProcessing;
using DevExpress.Pdf.Native.BouncyCastle.Asn1.X509.SigI;
using DevExpress.Utils.Extensions;
using DevExpress.Utils.Text;
using DevExpress.XtraEditors;
using DevExpress.XtraReports.Design;
using DevExpress.XtraSplashScreen;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogViewer
{
    public partial class FormItem : DevExpress.XtraEditors.XtraForm
    {
        #region 폼 로드
        /// <summary>
        /// 
        /// </summary>
        public FormItem()
        {
            InitializeComponent();
            rdoDate.SelectedIndex = 0;
        }
        
        /// <summary>
        /// 폼 로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            dteFrom.DateTime = DateTime.Now.AddDays(-Program.Option.saveDays);
            dteTo.DateTime = DateTime.Now;
        }

        /// <summary>
        /// 폼 쇼운
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormItem_Shown(object sender, EventArgs e)
        {
            groupControl1.Text = $"≡ 아이템 로그 [최근 자동로드 파일: {Program.Option.RecentItemLoad}]";
        }
        #endregion

        /// <summary>
        /// Item 로드
        /// </summary>
        private void GetData()
        {
            if (dteFrom.DateTime == DateTime.MinValue)
                dteFrom.DateTime = DateTime.Now;

            DataTable dt = ItemIO.Select(rdoDate.SelectedIndex,txtItemName.Text.Trim(), txtType.Text.Trim(), txtChaName.Text.Trim(), dteFrom.DateTime, dteTo.DateTime);
            grdItem.DataSource = dt;
            grdViewItem.OptionsView.BestFitMaxRowCount = 100;
            grdViewItem.BestFitColumns();
            groupControl1.Text = $"≡ 아이템 로그 [최근 자동로드 파일: {Program.Option.RecentItemLoad}]";
        }

        /// <summary>
        /// 조건 검색
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelect_Click(object sender, EventArgs e)
        {
            // 자동 로드 여부 확인 후 
            if (Program.isBusy)
            {
                XtraMessageBox.Show("현재 자동 데이터 로딩중입니다.\r\n잠시 후 다시 시도해주세요.", "조회", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            GetData();
        }

        #region 수동 로드
        /// <summary>
        /// 수동 로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpload_Click(object sender, EventArgs e)
        {
            // 자동 로드 여부 확인 후 
            if (Program.isBusy)
            {
                XtraMessageBox.Show("현재 자동 데이터 로딩중입니다.\r\n잠시 후 다시 시도해주세요.", "수동 로드", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "log(*.log)|*.log";
                dialog.InitialDirectory = Program.Option.filepath + "\\item";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filePath = dialog.FileName;
                        if (!filePath.Contains("item"))
                        {
                            XtraMessageBox.Show("Item 파일이 아닙니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return;
                        }

                        string fileName = dialog.SafeFileName.Substring(0, dialog.SafeFileName.Length - 4);
                        string folderName = dialog.SafeFileName.Substring(0, 10);

                        // 데이터 받아오기
                        DataTable tempItem = LoadItem.ReadFile(filePath, fileName);
                        if (tempItem == null || tempItem.Rows.Count == 0)
                        {
                            XtraMessageBox.Show($"파일 로드 중 문제가 발생하였습니다.\r\n파일을 확인해주세요.", "아이템 수동 로드", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        // Item BulkCopyTI
                        DBManager.Instance.DoBulkCopyTI(tempItem, "Item", fileName, "itemFileName");

                        XtraMessageBox.Show("성공적으로 로드되었습니다.", "로드 완료", MessageBoxButtons.OK);
                        GetData();
                    }
                    catch (Exception ex)
                    {
                        XtraMessageBox.Show("[아이템 수동 로드 오류]\r\n"+ex.Message, "아이템 업로드 오류", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 폼 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 날짜 기간 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rdoDate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rdoDate.SelectedIndex != 2)
                dteFrom.Enabled = dteTo.Enabled = false;
            else
                dteFrom.Enabled = dteTo.Enabled = true;
        }
    }
}