using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Web;
using System.Text.RegularExpressions;

namespace Naga
{
    class WebService
    {
        public string WSErrBuf = "";
        public string Url { get; set; }
        public string MethodName { get; set; }
        public string ResultString;

        public WebService()
        {
        }

        public WebService(string url, string methodName)
        {
            try
            {
                Url = url;
                MethodName = methodName;
            }

            catch (Exception ex)
            {
                WSErrBuf = "C[98,1] " + ex.Message.ToString();
            }
        }

        public void Invoke(int ParamCnt, string[] Params, string[] Datas)
        {
            try
            {
                string soapStr =
                    @"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
               xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
               xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
              <soap:Body>
                <{0} xmlns=""http://tempuri.org/"">
                  {1}
                </{0}>
              </soap:Body>
            </soap:Envelope>";

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
                req.Headers.Add("SOAPAction", "\"http://tempuri.org/" + MethodName + "\"");
                req.ContentType = "text/xml;charset=\"utf-8\"";
                req.Accept = "text/xml";
                req.Method = "POST";
                req.AllowWriteStreamBuffering = true;

                using (Stream stm = req.GetRequestStream())
                {
                    string postValues = "";
                    for (int i = 0; i < ParamCnt; i++)
                        postValues += string.Format("<{0}>{1}</{0}>", Params[i], Datas[i]);

                    soapStr = string.Format(soapStr, MethodName, postValues);
                    using (StreamWriter stmw = new StreamWriter(stm))
                        stmw.Write(soapStr);
                }

                using (StreamReader responseReader = new StreamReader(req.GetResponse().GetResponseStream()))
                {
                    string result = responseReader.ReadToEnd();
                    ResultString = splitResult(result);
                }
            }

            catch (Exception ex)
            {
                ResultString = "";
                WSErrBuf = "C[98,2] " + ex.Message.ToString();
            }
        }

        public string splitResult(string Data)
        {
            try
            {
                /* Sample output format
                 <?xml version="1.0" encoding="utf-8"?>
                 <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" 
                       xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                    <soap:Body>
                        <DeviceInputResponse xmlns="http://tempuri.org/">
                            <DeviceInputResult>00888888888888888</DeviceInputResult>
                        </DeviceInputResponse>
                    </soap:Body>
                 </soap:Envelope>
                */

                int len = 0;
                string buf = "";

                buf = MethodName + "Response";
                // main response chk
                if (Data.Contains(buf) == false)
                    return "";

                len = Data.IndexOf(buf);
                Data = Data.Substring(len + buf.Length);
                len = Data.LastIndexOf(buf);
                Data = Data.Substring(0, len);

                // chking whether function return is available
                if (Data.Contains(MethodName + "Result") == false)
                    return "";

                len = Data.IndexOf(MethodName + "Result");
                Data = Data.Substring(len + (MethodName + "Result").Length);
                len = Data.LastIndexOf(MethodName + "Result");
                Data = Data.Substring(0, len);
                len = Data.IndexOf(">");
                Data = Data.Substring(len + 1);
                len = Data.LastIndexOf("<");
                Data = Data.Substring(0, len);

                return Data;
            }

            catch (Exception ex)
            {
                WSErrBuf = "C[98,3] " + ex.Message.ToString();
                return "";
            }
        }

    }
}