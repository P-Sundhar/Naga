using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using System.IO;
using System.Security.Cryptography;
using System.Management;

namespace Naga
{
    class EncDecFun
    {
        // Constants
        private const int ENCY_MAC_ADDR = 1;
        private const int ENCY_LICENCE = 2;

        private const int SYS_TYPE_POS = 1;
        private const int SYS_TYPE_SYSTEM = 2;
        private const int SYS_TYPE_WINCE_DEVICE = 3;

        // Variables
        public string sErrBuf = "";
        private int iSysType = SYS_TYPE_SYSTEM;       // set as per client's system type


        public string MasterKey = "NCLIHEIOLKWNIATC";
        private string MacEncKey = "lmcoiciaenhkniwt";
        private string LicFileName = "Windows\\Microsoft.oem";

        string[] DDVals = new string[] { "0", "F", "Z", "Y", "V", "X", "A", "G", "L", "C", "~", "!", "}", ")", "(", "{", "+",
                                        "_", "@", "3", "5", "H", "1", "<", ">", ",", ".", "?","%", "^", "&", "*"};
        string[] MMVals = new string[] { "00", "AO", "MV", "3F", "HH", "N)", "X_", "+G", "]Q", "2>", "0,", "B'", "S#" };
        // App Start Date  = YYYMMd [ABCDEF] --> ADEBFC
        // App Expiry Date = YYYMMd [ABCDEF] --> FBCDEA


// ****************  Wince Device Based Machine Address
        // Device id releated settings
        private static Int32 FILE_DEVICE_HAL = 0x00000101;
        private static Int32 FILE_ANY_ACCESS = 0x0;
        private static Int32 METHOD_BUFFERED = 0x0;

        private static Int32 IOCTL_HAL_GET_DEVICEID = ((FILE_DEVICE_HAL) << 16) | ((FILE_ANY_ACCESS) << 14)
                        | ((21) << 2) | (METHOD_BUFFERED);

        [DllImport("coredll.dll")]
        private static extern bool KernelIoControl(Int32 IoControlCode, IntPtr InputBuffer, Int32 InputBufferSize,
                    byte[] OutputBuffer, Int32 OutputBufferSize, ref Int32 BytesReturned);


        public int GetDeviceID(ref string DeviceId)
        {
            sErrBuf = "";
            try
            {
                byte[] OutputBuffer = new byte[256];
                Int32 OutputBufferSize, BytesReturned;

                OutputBufferSize = OutputBuffer.Length;
                BytesReturned = 0;

                // Call KernelIoControl passing the previously defined IOCTL_HAL_GET_DEVICEID parameter
                // We don’t need to pass any input buffers to this call, so InputBuffer and InputBufferSize are set to their null values
                bool retVal = KernelIoControl(IOCTL_HAL_GET_DEVICEID, IntPtr.Zero, 0, OutputBuffer, OutputBufferSize, ref BytesReturned);
                if (retVal == false)
                    return 0;

                // Examine the OutputBuffer byte array to find the start of the Preset ID and Platform ID, as well as the size of the
                // PlatformID. 
                // PresetIDOffset – The number of bytes the preset ID is offset from the beginning of the structure
                // PlatformIDOffset - The number of bytes the platform ID is offset from the beginning of the structure
                // PlatformIDSize - The number of bytes used to store the platform ID
                // Use BitConverter.ToInt32() to convert from byte[] to int
                Int32 PresetIDOffset = BitConverter.ToInt32(OutputBuffer, 4);
                Int32 PlatformIDOffset = BitConverter.ToInt32(OutputBuffer, 0xc);
                Int32 PlatformIDSize = BitConverter.ToInt32(OutputBuffer, 0x10) - 1;

                // Convert the Preset ID segments into a string so they can be displayed easily.
                StringBuilder sb = new StringBuilder();
                // Break the Platform ID down into 2-digit hexadecimal numbers and append them to the Preset ID. This will result in a 
                // string-formatted Device ID
                for (int i = PlatformIDOffset; i < PlatformIDOffset + PlatformIDSize; i++)
                    sb.Append(OutputBuffer[i] - '0');

                DeviceId = sb.ToString();
                return 1;
            }

            catch
            {
                sErrBuf = "L[1]: Device Id Fetch Failed";
                return 0;
            }
        }



// ************  System Based Machine Address ****************************
        private int SplitPnpDeviceId(string OrgData, ref string MacAddr)
        {
            try
            {
                /*  // Sample device id
                    IDE\DISKHGST_HTS545050A7E680____________________GG2OAH10\5&2FB95BF9&0&0.0.0
                    SCSI\DISK&VEN_&PROD_ST500LT012-1DG14\4&14C44C70&0&000000                   
                    IDE\DISKTOSHIBA_MQ01ABD050______________________AX003J__\4&3593C471&0&0.0.0
                    IDE\DISKST500DM002-1BD142_______________________KC48____\5&4C8852F&0&0.0.0 
                    IDE\DISKST500DM002-1BD142_______________________KC48____\5&8F1EEEB&0&0.0.0 

                    SerialNumber :- 2020202057202d44585731343341523535594c48
                    SerialNumber :-       ET3834M3G1Y6EA
                    SerialNumber :-       MT38318MG189JD
                    SerialNumber :-       MT2831ZLN1JDJN
                 */

                int ret, i;
                string buf, tmp;

                ret = i = 0;
                buf = tmp = "";

                ret = OrgData.LastIndexOf('\\');
                buf = OrgData.Substring(ret + 1);
                tmp = buf;

                ret = i = 0;
                while (true)
                {
                    ret = buf.IndexOf('&');
                    if (ret == -1)
                        break;
                    i++;
                    buf = buf.Substring(ret + i);
                }

                buf = "";
                if (i >= 2)
                {
                    while (i >= 2)
                    {
                        ret = tmp.LastIndexOf('&');
                        if (ret == -1)
                            break;
                        tmp = tmp.Substring(0, ret);
                        i--;
                    }

                    buf = tmp.Replace("&", "");
                }
                else if (tmp.EndsWith("20") == true)
                {
                    while (true)
                    {
                        if (tmp.EndsWith("20") == false)
                            break;

                        tmp = tmp.Remove(tmp.Length - 2, 2);
                    }
                    buf = tmp;
                }
                else
                    buf = tmp;

                buf = buf.Replace(" ", "");

                MacAddr = buf;
                return 1;
            }

            catch (Exception ex)
            {
                sErrBuf = "L[2] : " + ex.Message.ToString();
                return 0;
            }
        }


        private int ChkAlphaNumeric(string Src, ref string Dst)
        {
            try
            
            {
                Src = Src.ToUpper();
                foreach (char c in Src)
                {
                    if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z')))
                        Dst += "";
                    else
                        Dst += c;
                }

                return 1;
            }

            catch (Exception ex)
            {
                sErrBuf = "L[3]: " + ex.Message.ToString();
                return 0;
            }
        }


        private int SplitSerialNo(string OrgData, ref string MacAddr)
        {
            try
            {
                /*  // Sample Serial No
                    SerialNumber :- 2020202057202d44585731343341523535594c48
                    SerialNumber :-       ET3834M3G1Y6EA
                    SerialNumber :-       MT38318MG189JD
                    SerialNumber :-       MT2831ZLN1JDJN
                 */

                int ret;
                string buf, tmp;
                char[] NonHex = { 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

                ret = 0;
                buf = tmp = "";

                tmp = OrgData.Trim();
                if (ChkAlphaNumeric(tmp, ref buf) == 0)
                    return 0;

                if (buf.Length >= 17)
                {
                    tmp = buf.Replace("20", "");
                    ret = tmp.Length - 16 <= -1 ? 0 : tmp.Length - 16;
                    buf = tmp.Substring(ret, 16);
                }

                if (buf.IndexOfAny(NonHex) != -1)
                {
                    tmp = "";
                    if (ConvertAsciiStrtoHex(buf, ref tmp) == 0)
                        return 0;
                    ret = tmp.Length - 16 <= -1 ? 0 : tmp.Length - 16;
                    buf = tmp.Substring(ret, 16);
                }

                MacAddr = buf;
                return 1;
            }

            catch (Exception ex)
            {
                sErrBuf = "L[4]: " + ex.ToString();
                return 0;
            }
        }

        public int GetSystemId(ref string MacAddress)
        {
            sErrBuf = "";
            try
            {
                int ret = 0;
                string tmp = "", final = "";

                System.Management.ManagementObjectSearcher mos = new ManagementObjectSearcher("select * from Win32_DiskDrive");
                foreach (ManagementObject mo in mos.Get())
                {
                    tmp = mo["DeviceID"].ToString();
                    if (tmp.EndsWith("0") == false)
                        continue;

                    if (iSysType == SYS_TYPE_SYSTEM)
                        final = mo["SerialNumber"].ToString();
                    else
                        final = mo["PNPDeviceID"].ToString();
                }

                ret = 0;
                if (iSysType == SYS_TYPE_SYSTEM)
                    ret = SplitSerialNo(final, ref tmp);
                else
                    ret = SplitPnpDeviceId(final, ref tmp);
                if (ret == 0)
                {
                    sErrBuf = "Mac Address Data Fetch Failed";
                    return 0;
                }
                MacAddress = tmp;
                return 1;
            }

            catch
            {
                sErrBuf = "L[5] : ID Fetch Failed";
                return 0;
            }
        }



        private int GetMacAddress(ref string MacAddr)
        {
            try
            {
                if (iSysType == SYS_TYPE_SYSTEM || iSysType == SYS_TYPE_POS)
                    GetSystemId(ref MacAddr);
                else if (iSysType == SYS_TYPE_WINCE_DEVICE)
                    GetDeviceID(ref MacAddr);
                else
                {
                    sErrBuf = "L[6] : Invalid System Type";
                    return 0;
                }

                return 1;
            }
            catch (Exception ex)
            {
                sErrBuf = "L[7] : " + ex.Message.ToString();
                return 0;
            }
        }

        private int ConvertHexToString(string src, ref byte[] Dst)
        {
            sErrBuf = "";

            try
            {
                int i = 0, j = 0;
                string tmp = "";

                for (i = j = 0; i < src.Length; i += 2, j++)
                {
                    tmp = src.Substring(i, 2);
                    Dst[j] = Convert.ToByte(Int32.Parse(tmp, NumberStyles.HexNumber));
                }
                return 1;
            }

            catch (Exception ex)
            {
                sErrBuf = "L[8]: " + ex.Message.ToString();
                return 0;
            }
        }


        private int ConvertStrtoHex(int len, byte[] Src, ref string Dst)
        {
            sErrBuf = "";
            try
            {
                int i;
                string tmp = "";

                for (i = 0; i < len; i++)
                {
                    tmp = string.Format("{0:X}", Src[i]).PadLeft(2, '0');
                    Dst += tmp;
                }

                return 1;
            }
            catch (Exception ex)
            {
                sErrBuf = "L[9]: " + ex.Message.ToString();
                return 0;
            }
        }

        private int ConvertAsciiStrtoHex(string Src, ref string Dst)
        {
            try
            {
                int ret = 0;

                foreach (char c in Src)
                {
                    ret = c;
                    Dst += String.Format("{0:X2}", (uint)System.Convert.ToUInt32(ret.ToString()));
                }

                return 1;
            }

            catch (Exception ex)
            {
                sErrBuf = "L[10]: " + ex.Message.ToString();
                return 0;
            }
        }


        private int EnDecrypt(int Type, byte[] PlainText, byte[] key, ref byte[] Dst)
        {
            sErrBuf = "";

            try
            {
                MemoryStream ms = new MemoryStream();
                DESCryptoServiceProvider mDES = new DESCryptoServiceProvider();
                mDES.Mode = CipherMode.ECB;
                mDES.Padding = PaddingMode.None;
                mDES.Key = key;

                CryptoStream encStream;

                if (Type == 'E')
                    encStream = new CryptoStream(ms, mDES.CreateEncryptor(), CryptoStreamMode.Write);
                else
                    encStream = new CryptoStream(ms, mDES.CreateDecryptor(), CryptoStreamMode.Write);

                BinaryWriter bw = new BinaryWriter(encStream);

                bw.Write(PlainText);
                bw.Close();
                encStream.Close();

                byte[] buffer = ms.ToArray();
                ms.Close();

                Array.Copy(buffer, Dst, buffer.Length);
                return 1;
            }
            catch
            {
                sErrBuf = "L[11]: Error";
                return 0;
            }
        }


        private int TripleDes(string PlainText, string Key, ref  byte[] DstBuf)
        {
            sErrBuf = "";

            try
            {
                int i, ret;
                byte[] KeyVal = new byte[16];
                byte[] Sess1 = new byte[8];
                byte[] Sess2 = new byte[8];
                byte[] Src = null;
                byte[] Dst = new byte[8];
                byte[] tmp = new byte[8];

                i = ret = 0;

                Src = new byte[PlainText.Length / 2];
                ret = ConvertHexToString(PlainText, ref Src);
                if (ret == 0)
                    return 0;

                for (i = 0; i < 16; i++)
                    KeyVal[i] = Convert.ToByte(Key[i]);

                for (i = 0; i < 8; i++)
                {
                    Sess1[i] = KeyVal[i];
                    Sess2[i] = KeyVal[i + 8];
                }

                i = EnDecrypt('E', Src, Sess1, ref Dst);
                if (i == 0) return 0;

                i = EnDecrypt('D', Dst, Sess2, ref tmp);
                if (i == 0) return 0;

                i = EnDecrypt('E', tmp, Sess1, ref Dst);
                if (i == 0) return 0;

                Array.Copy(Dst, DstBuf, Dst.Length);
                return 1;
            }
            catch
            {
                sErrBuf = "L[12]: Error";
                return 0;
            }
        }

        private int ChkFmtMacLen(ref string MacId)
        {
            try
            {
                int i = 0;
                string buf, tmp;

                buf = tmp = "";

                if (MacId.Length <= 11)
                {
                    i = 12 - MacId.Length;
                    MacId = "5478963201".Substring(0, i) + MacId;
                }

                i = 16 - MacId.Length;
                switch (i)
                {
                    case 0:
                        tmp = buf = "";
                        break;

                    case 1:
                        buf = "";
                        tmp = "1";
                        break;

                    case 2:
                        buf = "3";
                        tmp = "1";
                        break;

                    case 3:
                        buf = "3";
                        tmp = "16";
                        break;

                    default:
                        buf = "16";
                        tmp = "03";
                        break;
                }

                MacId = buf + MacId + tmp;
                return 1;
            }
            catch
            {
                sErrBuf = "L[13]: Error";
                return 0;
            }
        }

        public int LicGenerate(int CallMode, string Src, ref string Dst)
        {
            sErrBuf = "";
            try
            {
                int i = 0;
                string sVal, KeyVal;
                byte[] tmp = new byte[16];
                byte[] buf = new byte[16];

                sVal = KeyVal = "";

                if (ChkFmtMacLen(ref Src) == 0)
                    return 0;

                if (CallMode == ENCY_MAC_ADDR)      // Mac Id encryption
                    KeyVal = MacEncKey;
                else                                // Licence key encryption
                    KeyVal = MasterKey;

                sVal = Src;
                i = TripleDes(sVal, KeyVal, ref tmp);
                if (i == 0) return 0;

                Array.Clear(buf, 0, buf.Length);

                // do xor
                for (i = 0; i < 4; i++)
                    buf[i] = Convert.ToByte(tmp[i] ^ tmp[i + 4]);

                for (i = 0; i < 4; i++)
                    buf[i] = Convert.ToByte(buf[i] ^ 0xEE);

                Dst = "";
                i = ConvertStrtoHex(4, buf, ref Dst);
                if (i == 0) return 0;

                if (CallMode == ENCY_MAC_ADDR)
                    Dst = "F5" + Dst + "A3";

                return 1;
            }

            catch
            {
                sErrBuf = "L[14]: Error";
                return 0;
            }
        }

        private int FmtSaveExpDate(string ExpMode, ref string StDate, ref string ExpDate)
        {
            sErrBuf = "";
            try
            {
                int dd, mm, yy;
                string tmp, buf;

                dd = mm = yy = 0;
                tmp = buf = "";

                // App Start Date  = YYYMMd [ABCDEF] --> ADEBFC
                // App Expiry Date = YYYMMd [ABCDEF] --> FBCDEA

                tmp = (DateTime.Now.Year - 2000 + 24).ToString().PadLeft(3, '0');
                buf = tmp.Substring(0, 1) + MMVals[DateTime.Now.Month] + tmp.Substring(1, 1) + DDVals[DateTime.Now.Day] + tmp.Substring(2, 1);
                StDate = buf;

                tmp = buf = "";
                if (ExpMode == "0")     // no date chk 
                    tmp = "000000";
                else if (ExpMode == "1")    // chk for 30 days
                    tmp = DateTime.Now.AddDays(30).ToString("ddMMyy");
                else
                    return 0;

                dd = Convert.ToInt16(tmp.Substring(0, 2));
                mm = Convert.ToInt16(tmp.Substring(2, 2));
                yy = Convert.ToInt16(tmp.Substring(4, 2));

                buf = tmp = "";
                buf = yy.ToString().PadLeft(3, '0');
                tmp = DDVals[dd] + buf.Substring(1, 2) + MMVals[mm] + buf.Substring(0, 1);
                ExpDate = tmp;

                return 1;
            }
            catch
            {
                sErrBuf = "L[15] : Error";
                return 0;
            }
        }

        private int ChkExpDate(string StDate, string ExpDate)
        {
            sErrBuf = "";
            try
            {
                int ret, dd, mm, yy;
                string buf, tmp;

                ret = dd = mm = yy = 0;
                buf = tmp = "";
                //                                       012345
                // App Start Date  = YYYMMd [ABCDEF] --> ADEBFC     [Year + 24]
                // App Expiry Date = YYYMMd [ABCDEF] --> FBCDEA

                // ---------- Start Date
                // yy
                tmp = StDate.Substring(0, 1) + StDate.Substring(3, 1) + StDate.Substring(5, 1);
                yy = Convert.ToInt16(tmp) - 24 + 2000;
                // dd
                buf = StDate.Substring(4, 1);
                for (ret = dd = 0; ret < DDVals.Length; ret++)
                {
                    if (DDVals[ret].CompareTo(buf) == 0)
                    {
                        dd = ret;
                        break;
                    }
                }
                if (!(dd >= 1 && dd <= 31))
                {
                    sErrBuf = "L[16] : Invalid Date";
                    return 0;
                }

                // mm
                buf = "";
                buf = StDate.Substring(1, 2);
                for (ret = mm = 0; ret < MMVals.Length; ret++)
                {
                    if (MMVals[ret].CompareTo(buf) == 0)
                    {
                        mm = ret;
                        break;
                    }
                }
                if (!(mm >= 1 && mm <= 12))
                {
                    sErrBuf = "L[17] : Invalid Date";
                    return 0;
                }

                if (dd == 0 && mm == 0 && yy == 0)
                    return 1;


                // chk Ct date is greater than stdate
                tmp = buf = "";
                // St Date == tmp,  CtDate = buf;
                tmp = yy.ToString().PadLeft(2, '0') + mm.ToString().PadLeft(2, '0') + dd.ToString().PadLeft(2, '0');
                buf = DateTime.Now.ToString("yyyyMMdd");
                ret = buf.CompareTo(tmp);
                if (!(ret >= 0))
                {
                    sErrBuf = "L[18] : Set Valid System DateTime";
                    return 3;          // date differs
                }

                // ---------- Expiry Date
                // yy
                tmp = ExpDate.Substring(5, 1) + ExpDate.Substring(1, 2);
                yy = Convert.ToInt16(tmp);

                // dd
                buf = ExpDate.Substring(0, 1);
                for (ret = dd = 0; ret < DDVals.Length; ret++)
                {
                    if (DDVals[ret].CompareTo(buf) == 0)
                    {
                        dd = ret;
                        break;
                    }
                }

                if (!(dd >= 0 && dd <= 31))
                {
                    sErrBuf = "L[19] : Invalid Date";
                    return 0;
                }

                // mm
                buf = "";
                buf = ExpDate.Substring(3, 2);
                for (ret = mm = 0; ret < MMVals.Length; ret++)
                {
                    if (MMVals[ret].CompareTo(buf) == 0)
                    {
                        mm = ret;
                        break;
                    }
                }

                if (!(mm >= 0 && mm <= 12))
                {
                    sErrBuf = "L[20] : Invalid Date";
                    return 0;
                }

                if (dd == 0 && mm == 0 && yy == 0)
                    return 1;

                // chk licence has expired
                tmp = buf = "";
                // Expiry Date == tmp,  CtDate = buf;
                tmp = "20" + yy.ToString().PadLeft(2, '0') + mm.ToString().PadLeft(2, '0') + dd.ToString().PadLeft(2, '0');
                buf = DateTime.Now.ToString("yyyyMMdd");
                ret = tmp.CompareTo(buf);
                if (ret >= 0)
                    return 1;

                // Licence Expired
                return 2;
            }

            catch
            {
                sErrBuf = "L[21]: Error";
                return 0;
            }
        }

        public int ChkInputtedLicence(string LicInput)
        {
            try
            {
                int ret = 0;
                string data, tmp1, tmp2;
                string ExpMode, ExpDate, StDate;

                data = tmp1 = tmp2 = ExpMode = ExpDate = StDate = "";

                if (Globals.CHK_LICENCE == false)
                    return 1;

                if (LicInput.Length != 9)
                    return 100;

                tmp1 = LicInput.Substring(0, 1);
                if (!(tmp1 == "1" || tmp1 == "0"))
                    return 100;

                ret = GetMacAddress(ref data);
                if (ret == 0)
                    return -1;

                tmp1 = tmp2 = "";
                ret = LicGenerate(ENCY_MAC_ADDR, data, ref tmp1);
                if (ret != 1)
                    return -2;

                ret = LicGenerate(ENCY_LICENCE, tmp1, ref tmp2);
                if (ret != 1)
                    return -3;

                if (LicInput == "")
                    return 100;

                if (LicInput.Length != 9)
                    return 100;

                ExpMode = LicInput.Substring(0, 1);
                LicInput = LicInput.Substring(1);

                if (LicInput.CompareTo(tmp2) != 0)
                    return 100;

                ret = FmtSaveExpDate(ExpMode, ref StDate, ref ExpDate);
                if (ret != 1)
                    return 100;

                data = "";
                data = Path.GetPathRoot(Environment.SystemDirectory);
                data += LicFileName;

                FileInfo fi = new FileInfo(data);
                if (fi.Exists == true)
                {
                    fi.Attributes = FileAttributes.Normal;
                    fi.Delete();
                }

                FileStream fs = fi.Open(FileMode.CreateNew);
                if (fs == null)
                {
                    sErrBuf = "L[22]: Error";
                    return -5;
                }

                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(tmp2);
                    sw.WriteLine(StDate);
                    sw.WriteLine(ExpDate);
                    sw.Close();
                }
                fs.Close();
                fi.Attributes |= FileAttributes.Hidden;
                fi.Attributes |= FileAttributes.Compressed;
                fi.Attributes |= FileAttributes.ReadOnly;
                fi.Attributes |= FileAttributes.NotContentIndexed;
                return 1;
            }

            catch 
            {
                sErrBuf = "L[23] : Error";
                return 0;
            }
        }

        public int ChkLicence(ref string EncyMacId)
        {
            sErrBuf = "";

            try
            {
                int ret = 0;
                string data, tmp1, tmp2, ExpDate, StDate;

                data = tmp1 = tmp2 = ExpDate = StDate = "";

                if (Globals.CHK_LICENCE == false)
                    return 1;

                ret = GetMacAddress(ref data);
                if (ret != 1)
                    return -1;

                ret = LicGenerate(ENCY_MAC_ADDR, data, ref tmp1);
                if (ret != 1)
                    return -2;

                EncyMacId = tmp1;

                data = "";
                data = Path.GetPathRoot(Environment.SystemDirectory);
                data += LicFileName;

                if (!File.Exists(data))
                    return 100;

                FileInfo fi = new FileInfo(data);
                fi.Attributes = FileAttributes.Normal;
                FileStream fs = fi.Open(FileMode.Open, FileAccess.Read);
                if (fs == null)
                    return 100;

                tmp2 = "";
                using (StreamReader sr = new StreamReader(fs))
                {
                    tmp2 = sr.ReadLine();
                    StDate = sr.ReadLine();
                    ExpDate = sr.ReadLine();
                    sr.Close();
                }
                fi.Attributes |= FileAttributes.Hidden;
                fi.Attributes |= FileAttributes.Compressed;
                fi.Attributes |= FileAttributes.ReadOnly;
                fi.Attributes |= FileAttributes.NotContentIndexed;
                fs.Close();

                data = "";
                ret = LicGenerate(ENCY_LICENCE, tmp1, ref data);
                if (ret != 1)
                    return -3;

                if (tmp2 == "" || ExpDate == "" || StDate == "" || tmp2 == null || ExpDate == null || StDate == null)
                    return 100;
                
                if (tmp2.CompareTo(data) != 0)
                    return 100;

                // return --> 0 - error, 1 - success, 2 - Licence Expired, 3 - Not Ct Date, Date Changed
                ret = ChkExpDate(StDate, ExpDate);
                if (ret == 0)
                    return -3;
                else if (ret == 2)
                {
                    sErrBuf = "L[24]: Licence Expired";
                    return 200;
                }
                else if (ret == 3)
                {
                    sErrBuf = "L[25] : Date Modified. Set Current Date";
                    return 300;
                }

                return 1;
            }

            catch(Exception Ex)
            {
                sErrBuf = "L[26] : Error :" + Ex.Message.ToString();
                return 0;
            }
        }

    }
}
