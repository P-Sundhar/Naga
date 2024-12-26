using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StockAudit
{
    public interface DBPath
    {
        string dbpath();
    }
    public interface Toast
    {
        void Show(string message);
    }
    public interface GetDeviceID
    {
        void GetUniqueId(ref string Result);
        void GetDevId(ref string Result);
    }
    public interface ICloseApplication
    {
        void closeApplication();
    }
    public interface IKeyboardHelper
    {
        void HideKeyboard();
    }
    public interface DeleteFile
    {
        void Delete(string FileName);
    }
    public interface ShowAlertWithFunction
    {
        void ShowAlert(string title, string Msg, Xamarin.Forms.ContentPage forms);
        void ShowAlertWithFunction(string title, string Msg,  Func<Task<int>> function, Xamarin.Forms.ContentPage contentPage);
    }
}
