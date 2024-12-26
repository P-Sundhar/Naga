using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace StockAudit
{
    public class MasterLogic
    {
        public string DbErrBuf = "";
        public int iDbErrRet = 0;
    
        public MasterLogic()
        {

        }
        public void FetchUniqueId(ref string Result)
        {
            Xamarin.Forms.DependencyService.Get<GetDeviceID>().GetUniqueId(ref Result);
        }
        public void ShowAlert(string title, string Msg, Xamarin.Forms.ContentPage forms)
        {
            DependencyService.Get<ShowAlertWithFunction>().ShowAlert(title, Msg, forms);
        }
        public string FmtDBDate(string Date)
        {
            try
            {
                string[] sSplit = Date.Split('-');
                if (sSplit.Length == 3)
                    return sSplit[2] + "-" + sSplit[1] + "-" + sSplit[0];
                else
                    return "";
            }

            catch (Exception Ex)
            {
                DbErrBuf = "DC[14]: " + Ex.Message.ToString();
                return "";
            }
        }
        public int IsAllInteger(string srcData)
        {
            try
            {
                int i = 0, len = 0;

                len = srcData.Length;
                if (len == 0)
                    return 0;

                for (i = 0; i < len; i++)
                {
                    if (!(srcData[i] >= '0' && srcData[i] <= '9'))
                        return 0;
                }
                return 1;
            }
            catch (Exception Ex)
            {
                DbErrBuf = "C[35,2] " + Ex.Message.ToString();
                return -1;
            }
        }
        private void DeleteFile(string Filename)
        {
            FileInfo f1 = new FileInfo(Filename);
            if (f1.Exists)
                f1.Delete();
        }
        public string FmtDBString(string SrcStr)
        {
            try
            {
                string DstBuf = "";
                DstBuf = SrcStr.Replace("'", "");
                DstBuf = DstBuf.Replace("\"", "");
                DstBuf = DstBuf.Replace("&", " ");
                return DstBuf;
            }

            catch (Exception ex)
            {
                DbErrBuf = "DC[3] : " + ex.Message.ToString();
                return "";
            }
        }
        public string EncodeData(string Data)
        {
            DbErrBuf = "";

            try
            {
                string buf = "";

                buf = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(Data));
                return buf;
            }

            catch (Exception Ex)
            {
                DbErrBuf = "UC[1]: " + Ex.Message.ToString();
                return "";
            }
        }
        public void HideKeyBoard()
        {
            DependencyService.Get<IKeyboardHelper>().HideKeyboard();
        }

    }
}

