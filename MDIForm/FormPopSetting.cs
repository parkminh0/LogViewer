using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace LogViewer
{
    public partial class FormPopSetting : DevExpress.XtraEditors.XtraForm
    {
        /// <summary>
        /// 
        /// </summary>
        public FormPopSetting()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 폼 로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormPopFilePath_Load(object sender, EventArgs e)
        {
            btePath.Text = Program.Option.filepath;
            txtsaveDays.Text = Program.Option.saveDays.ToString();
        }

        /// <summary>
        /// 파일 경로 설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btePath_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                btePath.Text = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// 적용
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnApply_Click(object sender, EventArgs e)
        {
            // 자동 로드 여부 확인 후 
            if (Program.isBusy)
            {
                XtraMessageBox.Show("현재 자동 데이터 로딩중입니다.\r\n잠시 후 다시 시도해주세요.", "검색", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string path = btePath.Text.Trim();

            if (string.IsNullOrEmpty(path))
            {
                XtraMessageBox.Show("폴더 경로를 지정해주세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (int.Parse(txtsaveDays.Text.Trim()) < 0 || int.Parse(txtsaveDays.Text.Trim()) > 90)
            {
                XtraMessageBox.Show("보관 기간은 1일 ~ 90일 사이로 지정하셔야 합니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // 폴더 내부에 item, chatting 없을 시 
            DirectoryInfo directoryinfo = new DirectoryInfo(path);
            int chk = 0;
            foreach (DirectoryInfo folder in directoryinfo.GetDirectories())
            {
                if (folder.Name.ToString() == "chatting" || folder.Name.ToString() == "item")
                    chk++;
            }

            if (chk != 2)
            {
                if (XtraMessageBox.Show("현재 폴더 내부에 item 또는 chatting 폴더가 존재하지 않습니다.\r\r\n" + "계속하시겠습니까?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
                    return;
            }

            // 적용
            string msg = "";
            if (path != Program.Option.filepath && int.Parse(txtsaveDays.Text.Trim()) != Program.Option.saveDays)
            {
                msg += $"1) 폴더경로: {path}\r\r\n";
                msg += $"2) 기록 보관 기간: {txtsaveDays.Text}\r\r\n";
            }
            else if (path != Program.Option.filepath)
            {
                msg += $"폴더경로: {path}\r\r\n";

            }
            else if (int.Parse(txtsaveDays.Text.Trim()) != Program.Option.saveDays)
            {
                msg += $"기록 보관 기간: {txtsaveDays.Text.Trim()}\r\r\n";
            }
            else
                return;

            msg += "변경 사항을 적용하시겠습니까?";

            if (XtraMessageBox.Show(msg, "수정", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Program.Option.filepath = btePath.Text.Trim();
                Program.Option.saveDays = int.Parse(txtsaveDays.Text.Trim());
                Program.SaveConfig();

                ChattingIO.deleteChat();
                ItemIO.deleteItem();

                XtraMessageBox.Show("수정되었습니다.", "수정 완료", MessageBoxButtons.OK);

                this.DialogResult = DialogResult.OK;
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// 팝업 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}