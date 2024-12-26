using iTextSharp.text.pdf;
using NagaMaster.Models.Service;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace NagaMaster.Controllers
{
    public class ServiceController : Controller
    {
        LoginController objMas1 = new LoginController();
        public ActionResult JsonRspMsg(int IsSuccess, string Mthd, int ErrNo, string Msg, Object RspData)
        {
            try
            {
                if (IsSuccess == 1)
                    return Json(new ResponseHelper { Success = 1, Message = Msg, Data = RspData });

                objMas.PrintLog(PAGEDESC, "A[2," + ErrNo + "]: " + Msg + ": " + objMas.DBErrBuf);
                return Json(new ResponseHelper { Success = 0, Message = "A[2," + ErrNo + "]: " + Msg, Data = null }); ;
            }
            catch (Exception ex)
            {
                objMas.PrintLog(PAGEDESC, "C[2,1]: " + Mthd + ": " + ex.Message);
                return Json(new ResponseHelper { Success = 0, Message = "C[1,1]: Exception in " + Mthd, Data = null }); ;
            }
        }

        const string PAGEDESC = "ReportController";
        MasterLogic objMas = new MasterLogic();
        // GET: Service

        public ActionResult Shift()
        {
            string MthdName = "Shift";
            var model = new ShiftModel();

            var UnitDet = objMas.GetDataTable("Select UnitId,UnitName from Unit_master where Status in (1,2) Order by UnitName");
            if (UnitDet == null)
            {
                objMas.PrintLog("A[2,1]", objMas.DBErrBuf);
                return JsonRspMsg(0, MthdName, 1, "DB Return Null", null);
            }

            model.UnitDetails = (from DataRow a in UnitDet.Rows
                                 select new SelectListItem { Text = a["UnitName"].ToString(), Value = a["UnitId"].ToString() }).ToList();

            var SystemDet = objMas.GetDataTable("Select SystemId,SystemName from System_master where Status in (1,2) Order by SystemName");
            if (SystemDet == null)
            {
                objMas.PrintLog("A[2,2]", objMas.DBErrBuf);
                return JsonRspMsg(0, MthdName, 2, "DB Return Null", null);
            }

            model.SystemDetails = (from DataRow a in SystemDet.Rows
                                 select new SelectListItem { Text = a["SystemName"].ToString(), Value = a["SystemId"].ToString() }).ToList();

            model.ShiftDate = System.DateTime.Now.ToString("yyyy-MM-dd");

            ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.Shift_Details, Request.Cookies["UserId"].Value.ToString());
            model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());

            return View(model);
        }

        [HttpPost]
        [Route("action")]
        [Route("Service/OpenClose")]
        public ActionResult OpenClose(ShiftModel shtmodel)
        {
            try
            {
                Int64 Ret = 0;
                int i = 0;

                string MthdName = "OpenClose";
                string Qry = "";
                string[] sQry = new string[2];
                string plnt = ConfigurationManager.AppSettings["Plant"].ToString();
                string userid = Request.Cookies["UserId"].Value.ToString();
                string shiftname = shtmodel.ShiftId == 1 ? "SHIFT 1" : "SHIFT 2";

                if (shtmodel.Type == 0)
                {
                    Ret = objMas.GetTableCount(" dayclose_details Where System='" + shtmodel.SystemId + "' AND UnitId='" + shtmodel.UnitId
                                                  + "' AND PlantId='" + plnt + "' AND DaycloseFlag = 0");
                    if (Ret == 0)
                        sQry[i++] = " INSERT into dayclose_details(PlantId,UnitId,System,TranDate,StartShiftNo,DayCloseFlag,Status,CreatedBy,CreatedOn)Values('" +
                                    plnt + "','" + shtmodel.UnitId + "','" + shtmodel.SystemId + "',Now(),1,0,1,'" + userid + "',Now())";

                    sQry[i++] = " Insert into Shift_master(PlantCode,UnitId,SystemId,ShiftId,ShiftName,ShiftStatus,OpenDate,Status,CreatedBy,CreatedOn,ModifiedBy," +
                                " ModifiedOn)Values('" + plnt + "','" + shtmodel.UnitId + "','" + shtmodel.SystemId
                                + "'," + shtmodel.ShiftId + ",'" + shiftname + "',1,'" + shtmodel.ShiftDate + "',1,'" + userid + "',Now(),'" + userid + "',Now())";

                    Ret = objMas.ExecMultipleQry(i, sQry);
                    if (Ret >= 1)
                    {
                        objMas.PrintLog("A[2,3]", objMas.DBErrBuf);
                        return JsonRspMsg(1, MthdName, 3, "Shift Opened Successfully", null);
                    }
                    else
                    {
                        objMas.PrintLog("A[2,4]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 4, "Shift Open Failed", null);
                    }
                }
                else
                {
                    Qry = " Update Shift_master set CloseDate='" + shtmodel.ShiftDate + "',ShiftStatus=0,ModifiedBy='" + userid + "',ModifiedOn=Now() Where SystemId='" +
                            shtmodel.SystemId + "' AND ShiftId = " + shtmodel.ShiftId + " AND UnitId='" + shtmodel.UnitId + "' AND PlantCode='" + plnt + "'";

                    Ret = objMas.ExecQry(Qry);
                    if (Ret >= 1)
                    {
                        objMas.PrintLog("A[2,5]", objMas.DBErrBuf);
                        return JsonRspMsg(1, MthdName, 5, "Shift Closed Successfully", null);
                    }
                    else
                    {
                        objMas.PrintLog("A[2,6]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 6, "Shift Close Failed", null);
                    }
                }
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,1]", Ex.Message);
                return Json(new { Status = -1, Message = "Catche Error " + Ex.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("Service/LoadShiftDetails")]
        public ActionResult LoadShiftDetails(ShiftModel shtmodel)
        {
            bool Open1, Open2, Close1, Close2;
            Open1 = Open2 = Close1 = Close2 = false;

            try
            {
                Int64 Ret = 0;

                string buf = "";
                string plnt = ConfigurationManager.AppSettings["Plant"].ToString();

                Ret = objMas.GetTableCount(" dayclose_details Where DaycloseFlag = 0 And System='" + shtmodel.SystemId
                                              + "' AND UnitId='" + shtmodel.UnitId + "' AND PlantId='" + plnt + "'");
                if (Ret == 0)
                    return Json(new { Status = 1, Message = "Open Enable", Open1 = true, Open2 = true, Close1, Close2 }, JsonRequestBehavior.AllowGet);

                else
                {
                    Ret = objMas.GetTableCount(" shift_master Where ShiftStatus = 0 And SystemId='" + shtmodel.SystemId + "' AND UnitId='" + shtmodel.UnitId + "' AND PlantCode='" +
                        plnt + "'");
                    if (Ret >= 2)
                        return Json(new { Status = 2, Message = "Disable Shift", Open1, Open2, Close1, Close2 }, JsonRequestBehavior.AllowGet);
                    else if (Ret == 1)
                    {
                        objMas.ExecScalar("Select ShiftId from shift_master Where ShiftStatus = 0 And SystemId='" + shtmodel.SystemId
                            + "' AND UnitId='" + shtmodel.UnitId + "' AND PlantCode='" + plnt + "'", ref buf);
                        if (buf == "1")
                        {
                            Ret = objMas.GetTableCount("shift_master Where ShiftStatus = 1 And SystemId='" + shtmodel.SystemId + "' AND UnitId='" + shtmodel.UnitId + "' AND PlantCode='" +
                                    plnt + "'");
                            if (Ret >= 1)
                            {
                                Ret = objMas.ExecScalar("Select ShiftId from shift_master Where ShiftStatus = 1 And SystemId='" + shtmodel.SystemId
                                + "' AND UnitId='" + shtmodel.UnitId + "' AND PlantCode='" + plnt + "'", ref buf);
                                if (Ret >= 1)
                                {
                                    if (buf == "1")
                                        Close1 = true;
                                    else if (buf == "2")
                                        Close2 = true;
                                }
                            }
                            else
                                Open2 = true;
                        }
                        else
                        {
                            Ret = objMas.GetTableCount("shift_master Where ShiftStatus = 1 And SystemId='" + shtmodel.SystemId + "' AND UnitId='" + shtmodel.UnitId + "' AND PlantCode='" +
                                       plnt + "'");
                            if (Ret >= 1)
                            {
                                Ret = objMas.ExecScalar("Select ShiftId from shift_master Where ShiftStatus = 1 And SystemId='" + shtmodel.SystemId
                                + "' AND UnitId='" + shtmodel.UnitId + "' AND PlantCode='" + plnt + "'", ref buf);
                                if (Ret >= 1)
                                {
                                    if (buf == "1")
                                        Close1 = true;
                                    else if (buf == "2")
                                        Close2 = true;
                                }
                            }
                            else
                                Open1 = true;
                        }
                    }
                    else 
                    {
                        Ret = objMas.GetTableCount("shift_master Where ShiftStatus = 1 And SystemId='" + shtmodel.SystemId + "' AND UnitId='" + shtmodel.UnitId + "' AND PlantCode='" +
                                    plnt + "'");
                        if (Ret >= 1)
                        {
                            Ret = objMas.ExecScalar("Select ShiftId from shift_master Where ShiftStatus = 1 And SystemId='" + shtmodel.SystemId
                            + "' AND UnitId='" + shtmodel.UnitId + "' AND PlantCode='" + plnt + "'", ref buf);
                            if (Ret >= 1)
                            {
                                if (buf == "1")
                                    Close1 = true;
                                else if (buf == "2")
                                    Close2 = true;
                            }
                        }
                    }
                    return Json(new { Status = 3, Message = "Enable Open Close", Open1, Open2, Close1, Close2 }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,2]", Ex.Message);
                return Json(new { Status = 4, Message = "Catche Error", Open1, Open2, Close1, Close2 }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("Service/Dayclose")]
        public ActionResult Dayclose(ShiftModel model)
        {
            try
            {
                Int64 Ret = 0;
                string MthdName = "Dayclose";
                string plnt = ConfigurationManager.AppSettings["Plant"].ToString();

                Ret = objMas.GetTableCount(" shift_master Where ShiftStatus = 1 AND SystemId='" + model.SystemId
                    + "' AND UnitId='" + model.UnitId + "' AND PlantCode='" + plnt + "'");
                if (Ret >= 1)
                {
                    objMas.PrintLog("A[2,7]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 7, "Do ShiftClose First !", null);
                }
                
                Ret = objMas.GetTableCount("shift_master Where SystemId='" + model.SystemId + "' AND UnitId='" + model.UnitId
                    + "' AND PlantCode='" + plnt + "'");
                if (Ret == 0)
                {
                    objMas.PrintLog("A[2,8]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 8, "Do Shift Open & Close First", null);
                }
                
                string[] sQryHead = new string[3];
                string[] sQryTail = new string[3];

                sQryHead[0] = " Insert into shift_details_bkup(CycleId,PlantId,UnitId,System,ShiftId,OpenDate,CloseDate,Status,CreatedBy,CreatedOn)Select '";
                sQryTail[0] = "',PlantCode,UnitId,'" + model.SystemId + "',ShiftId,OpenDate,CloseDate,1,CreatedBy,Now() from Shift_master where PlantCode='" +
                              plnt + "' AND UnitId='" + model.UnitId + "'";

                sQryHead[1] = " Update Dayclose_details set EndShiftNo =2,DaycloseFlag = 1,CycleId ='";
                sQryTail[1] = "' Where System='" + model.SystemId + "' AND DaycloseFlag = 0 AND UnitId='" + model.UnitId + "' AND PlantId='" + plnt + "'";

                sQryHead[2] = " Delete from Shift_master Where SystemId='" + model.SystemId + "' AND UnitId='" + model.UnitId
                              + "' AND PlantCode='" + plnt + "'";
                sQryTail[2] = "-";

                Ret = objMas.InsertMaster(" SELECT CONCAT(DATE_FORMAT(NOW(),'%y%m%d'),LPAD(COUNT(CycleId)+ 1,4,'0')) FROM " +
                                          " shift_details_bkup WHERE DATE(CreatedOn)=DATE(NOW())", sQryHead, sQryTail, 3);
                if (Ret >= 1)
                {
                    objMas.PrintLog("A[2,9]", objMas.DBErrBuf);
                    return JsonRspMsg(1, MthdName, 9, "Dayclose Success", null);
                }
                else
                {
                    objMas.PrintLog("A[2,10]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 10, "Dayclose Failed" + objMas.DBErrBuf, null);
                }

            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,3]", Ex.Message);
                return Json(new { Status = 0, Message = "Dayclose Failed [C]" + Ex.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult Assignment()
        {
            string MthdName = "Assignment";
            var model = new AssignmentModel();

            var ItemDet = objMas.GetDataTable("Select MaterialCode,MaterialDesc from material_master where Status in (1,2)");
            if (ItemDet == null)
            {
                objMas.PrintLog("A[2,11]", objMas.DBErrBuf);
                return JsonRspMsg(0, MthdName, 11, "DB Return Null", null);
            }

            model.ItemDetails = (from DataRow a in ItemDet.Rows
                                 select new SelectListItem { Text = a["MaterialDesc"].ToString(), Value = a["MaterialCode"].ToString() }).ToList();

            var UnitDet = objMas.GetDataTable("Select UnitId,UnitName from Unit_master where Status in (1,2) Order by UnitName");
            if (UnitDet == null)
            {
                objMas.PrintLog("A[2,12]", objMas.DBErrBuf);
                return JsonRspMsg(0, MthdName, 12, "DB Return Null", null);
            }

            model.UnitDetails = (from DataRow a in UnitDet.Rows
                                 select new SelectListItem { Text = a["UnitName"].ToString(), Value = a["UnitId"].ToString() }).ToList();

            var SystemDet = objMas.GetDataTable("Select SystemId,SystemName from System_master where Status in (1,2) Order by SystemName");
            if (SystemDet == null)
            {
                objMas.PrintLog("A[2,13]", objMas.DBErrBuf);
                return JsonRspMsg(0, MthdName, 13, "DB Return Null", null);
            }

            model.SystemDetails = (from DataRow a in SystemDet.Rows
                                   select new SelectListItem { Text = a["SystemName"].ToString(), Value = a["SystemId"].ToString() }).ToList();

            ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.AssignMent, Request.Cookies["UserId"].Value.ToString());
            model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());

            return View(model);
        }

        [HttpPost]
        [Route("action")]
        [Route("/Service/GetTargetDetails")]
        public ActionResult GetTargetDetails(AssignmentModel assmodel)
        {
            try
            {
                string MthdName = "GetTargetDetails";
                DataTable dt = new DataTable();
                dt = objMas.GetDataTable("select Denominator from material_master where MaterialCode = '" + assmodel.Material + "'");
                if(dt == null)
                {
                    objMas.PrintLog("A[2,13]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 14, "Failed To Fetch Order Details", null);
                }
                else if (dt.Rows.Count == 0)
                {
                    objMas.PrintLog("A[2,14]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 15, "No Data Found", null);
                }
                else
                {
                    return Json(new
                    {
                        Status = 1,
                        Message = "Success",
                        Data = new AssignmentModel
                        {
                            Target = Convert.ToInt32(dt.Rows[0]["Denominator"])
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch(Exception Ex)
            {
                objMas.PrintLog("C[2,4]", Ex.Message);
                return Json(new { Status = -1, Message = Ex.Message.ToString(), Data = assmodel }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("Service/GetBatchDetails")]
        public ActionResult GetBatchDetails(AssignmentModel assmodel)
        {
            try
            {
                string MthdName = "GetBatchDetails";
                string ShiftId = "1";
                string plnt = ConfigurationManager.AppSettings["Plant"].ToString();
                var mnthchr = new char[] { ' ', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
                DataTable dt = new DataTable();

                dt = objMas.GetDataTable("Select Concat(Date_format(CurDate(),'%d%y'),ProdGroupCode,SizeInInchCode) as BatchData from material_master where MaterialCode='" + assmodel.Material + "'");
                if (dt == null)
                {
                    objMas.PrintLog("A[2,15]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 16, "Failed To Fetch Order Details :"+ objMas.DBErrBuf, null);
                }
                else if (dt.Rows.Count == 0)
                {
                    objMas.PrintLog("A[2,16]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 17, "No Data Found", null);
                }
                else
                {
                    return Json(new
                    {
                        Status = 1,
                        Message = "Success",
                        Data = new AssignmentModel
                        {
                            BatchNo = "I" + Convert.ToChar(mnthchr[Convert.ToInt16(System.DateTime.Now.ToString("MM"))]) + dt.Rows[0]["BatchData"].ToString() + ShiftId,
                        }
                    }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,5]", Ex.Message);
                return Json(new { Status = -1, Message = "Catche Error" + Ex.Message.ToString(), Data = assmodel }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("Service/SaveAssignment")]
        public ActionResult SaveAssignment(AssignmentModel assmodel)
        {
            try
            {
                string MthdName = "SaveAssignment";
                Int64 Ret = 0;
                string ShelflifeMax = "", ExpDays = "", PackDate = "";
                string[] Systembuf;
                string userid = Request.Cookies["UserId"].Value.ToString();

                string plnt = ConfigurationManager.AppSettings["Plant"].ToString();
                PackDate = DateTime.Now.ToString("yyyy-MM-dd");
                Ret = objMas.ExecScalar("Select ShelflifeMax From material_master Where materialCode='" + assmodel.Material + "'", ref ShelflifeMax);
                if (Ret == -1)
                {
                    objMas.PrintLog("A[2,17]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 18, "Failed To Fetch Details...!", null);
                }

                Ret = objMas.ExecScalar("Select Date_format(Date_Sub(Date_Add(Date('" + PackDate + "'),interval " + ShelflifeMax + " day),interval 1 day),'%Y-%m-%d')", ref ExpDays);

                Systembuf = assmodel.SystemId.Split(',');

                int i, j, k;
                i = j = k = 0;
                string[] Shift = new string[Systembuf.Length];
                string[] sQryHead = new string[Systembuf.Length * 2];
                string[] sQryTail = new string[Systembuf.Length * 2];
                i = j = k = 0;

                for (i = 0; i < Systembuf.Length; i++)
                {

                    Ret = objMas.GetTableCount(" production_header Where Date(PdnDate)=Curdate() AND MaterialCode='" + assmodel.Material + "'AND `System`='" + Systembuf[i].ToString() + "' AND JobFlag=0");      // 
                    if (Ret >= 1)
                        return Json(new { Status = 0, Message = "Already Job Assigned For this Item.After Job Complete Do Assignment [ItemCode :" + assmodel.Material
                            + "\n For This System : '" + Systembuf[i].ToString() + "']"              }, JsonRequestBehavior.AllowGet);
                  
                    
                    sQryHead[k] = " Insert into Production_header(PDNId,UnitId,PlantCode,PrimaryQty,TargetQty,`System`,UserId,ShiftId,PDNDate,MaterialCode," +
                                  " BatchNo,MonthYear,StartEndFlag,PackDate,ExpDate,Status,CreatedBy,CreatedOn,ModifiedBy," +
                                  " ModifiedOn)Values(";

                    sQryTail[k++] = ",'" + assmodel.UnitId + "','" + plnt + "','" + assmodel.Primary + "','" + assmodel.Target + "','" + Systembuf[i].ToString() + "','" +
                                  userid + "',1,Date_format(CurDate(),'%Y-%m-%d'),'" + assmodel.Material + "','" + assmodel.BatchNo + "','" + DateTime.Now.ToString("MMM") + ',' + 
                                  DateTime.Now.ToString("yyyy") + "',1,'" + PackDate + "','" + ExpDays + "',1,'" + userid + "',NOW(),'" + userid
                                  + "',NOW()) ON Duplicate Key Update BatchNo='" + assmodel.BatchNo + "',MonthYear = '" + DateTime.Now.ToString("MMM") + ',' +
                                 DateTime.Now.ToString("yyyy") + "', ModifiedBy ='" + userid + "',ModifiedOn = Now(),PackDate='" + PackDate + "',ExpDate='" +
                                  ExpDays + "'";

                }
                Ret = objMas.InsertMaster("SELECT ifnull(max(PDnid), concat(date_format(curdate(), '%y%m%d'),'0000')) + 1 FROM Production_header " +
                                          " where DATE(CreatedOn)=curdate()", sQryHead, sQryTail, k);
                if (Ret >= 1)
                    return Json(new
                    {
                        Status = 1,
                        Message = "Saved Successfully...!"
                    }, JsonRequestBehavior.AllowGet);
                else
                {
                    objMas.PrintLog("A[2,18]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 19, "Save Failed"+objMas.DBErrBuf, null);
                }
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,6]", Ex.Message);
                return Json(new
                {
                    Status = -1,
                    Message = "Save Failed...!" + Ex.Message.ToString()
                }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Service/LoadGrid")]
        public ActionResult LoadGrid(AssignmentModel model)
        {
            List<AssignMentGrid> lstdtls = new List<AssignMentGrid>();
            try
            {
                string MthdName = "LoadGrid";
                string plnt = ConfigurationManager.AppSettings["Plant"].ToString();

                DataTable dt = objMas.GetDataTable("Select b.SystemName,a.`System`,Date_format(PDNDate,'%d-%m-%Y') AS Date,c.MaterialDesc as ItemName, a.MaterialCode as ItemCode," +
                               " a.BatchNo,a.TargetQty,a.PrimaryQty,a.MonthYear,Concat(a.`System`,',',a.MaterialCode,',',a.PDNId) AS Id," +
                               " a.ProdDocNo as OrderNo,d.UnitName FROM production_header a,system_master b,material_master c, unit_master d Where " +
                               " a.MaterialCode = c.MaterialCode  AND a.UnitId=d.UnitId AND a.JobFlag=0 AND a.`System`=b.SystemId AND " +
                               " Date(PDNDate)=CURDATE() AND a.PlantCode='" + plnt
                               + "' Order By PDNDate,OrderNo,`System`");

                
                if (dt == null)
                {
                    objMas.PrintLog("A[2,19]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 20, "Data Fetch Failed" + objMas.DBErrBuf, null);
                }

                lstdtls = (from DataRow a in dt.Rows
                           select new AssignMentGrid
                           {
                               System = a["SystemName"].ToString(),
                               UnitName = a["UnitName"].ToString(),
                               Date = a["Date"].ToString(),
                               Material = a["ItemName"].ToString(),
                               BatchNo = a["BatchNo"].ToString(),
                               MonthYear = a["MonthYear"].ToString(),
                               TargetQty = a["TargetQty"].ToString(),
                               PrimaryQty = a["PrimaryQty"].ToString(),
                           }).ToList();


                return Json(new { Status = 1, Message = "", Data = lstdtls }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,7]", Ex.Message);
                return Json(new { Status = -1, Message = Ex.Message.ToString(), Data = lstdtls }, JsonRequestBehavior.AllowGet);
            }
        }

        
        public ActionResult PickList(PickListModel picmodel)
        {
            string MthdName = "PickList";
            var model = new PickListModel();

            var Locations = objMas.GetDataTable("select SHIPMENT_OR_PO from sapdispatchorderheader where TYPES_OF_TRANSACTION='STO' AND PickFlag='0' group by SHIPMENT_OR_PO");
            if (Locations == null)
            {
                objMas.PrintLog("A[2,20]", objMas.DBErrBuf);
                return JsonRspMsg(0, MthdName, 21, "Data Fetch Failed", null);
            }

            model.SipmentDetails = (from DataRow a in Locations.Rows
                                    select new SelectListItem { Text = a["SHIPMENT_OR_PO"].ToString(), Value = a["SHIPMENT_OR_PO"].ToString() }).ToList();


            ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.STOPicklist, Request.Cookies["UserId"].Value.ToString());
            model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());

            return View(model);
        }

        [HttpPost]
        [Route("action")]
        [Route("Service/GetQuantityDetails")]
        public ActionResult GetQuantityDetails(PickListModel picmodel)
        {
            try
            {
                string MthdName = "GetQuantityDetails";
                DataTable dt = new DataTable();
                dt = objMas.GetDataTable("select sum(a.DELIVERY_QTY_IN_BOX) as BoxQty,b.DELIVERY_NO,b.SENDING_PLANT from sapdispatchorderlines a, sapdispatchorderheader b " +
                            " where b.SHIPMENT_OR_PO =" + picmodel.Sipment + " and a.DELIVERY_NO=b.DELIVERY_NO and b.TYPES_OF_TRANSACTION = 'STO'");
                if (dt == null)
                {
                    objMas.PrintLog("A[2,21]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 22, "Failed To Fetch Order Details", null);
                }
                else if (dt.Rows.Count == 0)
                {
                    objMas.PrintLog("A[2,21]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 23, "No Data Found", null);
                }
                else
                {
                    return Json(new
                    {
                        Status = 1,
                        Message = "Success",
                        Data = new PickListModel
                        {
                            BoxQty = dt.Rows[0]["BoxQty"].ToString(),
                            DlNumber = dt.Rows[0]["DELIVERY_NO"].ToString(),
                            FromPlant = dt.Rows[0]["SENDING_PLANT"].ToString(),
                        }
                    }, JsonRequestBehavior.AllowGet);
                }


            }
            catch(Exception Ex)
            {
                objMas.PrintLog("C[2,8]", Ex.Message);
                return Json(new { Status = -1, Message = "Catche Error" + Ex.Message.ToString(), Data = picmodel }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("/Service/GetSapOrder")]
        public ActionResult GetSapOrder(PickListModel picmodel)
        {   
            try
            {
                Int64 Ret = 0;
                string MthdName = "GetSapOrder";

                DataTable dt = new DataTable();
                Ret = objMas.GetTableCount("plant_master where PlantTypeFlag='0' and PlantId ='" + picmodel.FromPlant + "'");
                if(Ret >= 1)
                    dt = objMas.GetDataTable("call STOPicklist('" + picmodel.Sipment + "')");        //c.SHIPMENT_OR_PO=a.SHIPMENT_OR_PO and 
                else
                    dt = objMas.GetDataTable("call CFAToCFAPickList('" + picmodel.Sipment + "','"+picmodel.FromPlant+"')");
                
                if (dt == null)
                {
                    objMas.PrintLog("A[2,22]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 24, "Data Fetch Failed", null);
                }
                
                else if (dt.Rows.Count == 0)
                {
                    objMas.PrintLog("A[2,23]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 25, "No Data Found For This Selection ...", null);
                }
                var sap = (from DataRow row in dt.Rows
                           select new
                           {
                               ShipmentNo = row.Field<string>("ShipmentNo").ToString(),
                               DeliveryNo = row.Field<string>("DeliveryNo").ToString(),
                               BatchNo = row.Field<string>("Batch").ToString(),
                               MaterialCode = row.Field<string>("Material Code").ToString(),
                               MaterialDesc = row.Field<string>("Material Description").ToString(),
                               BoxQty = row.Field<Int32>("Qty in Box").ToString(),
                           }).ToList();
                return Json(sap, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,9]", Ex.Message);
                return Json(new { Status = -1, Message = "Catche Error" + Ex.Message.ToString(), Data = picmodel }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("Service/SAPorderDownload")]
        public ActionResult SAPorderDownload(PickListModel picmodel)
        {

            try
            {
                Int64 Ret = 0;
                string MthdName = "SAPorderDownload";
                int ret = 0;

                DataTable dtHeader = new DataTable();
                DataTable dtlines = new DataTable();
                string[] AddlnData = new string[6];
                int[] AddlnHdrCols = new int[6];
                float[] ColWidth = new float[6] { 2f, 2f, 6f, 3f, 2f, 2f };
                int[] TotalCols = new int[6] { 0,0,0,0,1,1 };


                dtHeader = objMas.GetDataTable("Select SHIPMENT_OR_PO,DELIVERY_NO,SENDING_PLANT,RECEIVING_PLANT,TRUCK_NUMBER,b.PlantName as SendingPlant," +
                    "c.PlantName as ReceivingPlant from sapdispatchorderheader a,Plant_master b,Plant_master c where a.SENDING_PLANT = b.PlantID and " +
                    "a.RECEIVING_PLANT = c.PlantId and SHIPMENT_OR_PO='"+picmodel.Sipment+"' and a.Types_OF_Transaction='STO'");
                if (dtHeader == null)
                {
                    objMas.PrintLog("A[2,24]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 26, "Data Fetch Failed", null);
                }
                AddlnData[0] = "Sending Plant : " + dtHeader.Rows[0]["SENDING_PLANT"].ToString() + " - " + dtHeader.Rows[0]["SendingPlant"].ToString();
                AddlnHdrCols[0] = 3;

                AddlnData[1] = "Truck No : " + dtHeader.Rows[0]["TRUCK_NUMBER"].ToString();
                AddlnHdrCols[1] = 3;

                AddlnData[2] = "Receiving Plant : " + dtHeader.Rows[0]["RECEIVING_PLANT"].ToString() + " - "  + dtHeader.Rows[0]["ReceivingPlant"].ToString();
                AddlnHdrCols[2] = 3;

                AddlnData[3] = "Purchase Order No : " + dtHeader.Rows[0]["SHIPMENT_OR_PO"].ToString();
                AddlnHdrCols[3] = 3;

                AddlnData[4] = " ";
                AddlnHdrCols[4] = 3;

                AddlnData[5] = "Delivery No : " + dtHeader.Rows[0]["DELIVERY_NO"].ToString();
                AddlnHdrCols[5] = 3;

                Ret = objMas.GetTableCount("plant_master where PlantTypeFlag='0' and PlantId ='" + picmodel.FromPlant + "'");
                if (Ret >= 1)
                    dtlines = objMas.GetDataTable("call STOPicklist('" + picmodel.Sipment + "')"); 
                else
                    dtlines = objMas.GetDataTable("call CFAToCFAPickList('" + picmodel.Sipment + "','" + picmodel.FromPlant + "')");

                if (dtlines == null)
                {
                    objMas.PrintLog("A[2,24]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 26, "Data Fetch Failed", null);
                }
                else if (dtlines.Rows.Count == 0)
                {
                    objMas.PrintLog("A[2,25]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 27, "No Data Found For This Selection", null);
                }
                PdfPTable pdfTblHdl = new PdfPTable(dtlines.Columns.Count);
                dtlines.Columns.Remove("ShipmentNo");
                dtlines.Columns.Remove("DeliveryNo");

                int[] DType = Enumerable.Range(0, dtlines.Columns.Count).Select(n => MasterLogic.RPTSTYLE_STRING_SHORT).ToArray();
                DType[0] = MasterLogic.RPTSTYLE_INT;

                ret = objMas.GenerateReport(Convert.ToInt16(Globals.DWNLDFMT_PDF), 0, 0, 0, "Piclist_", "NAGA - STO Pickslip", DType, null, TotalCols, "", "", AddlnData, AddlnHdrCols, null, null, dtlines, ColWidth);
                if (ret >= 1)
                {
                    objMas.PrintLog("A[2,26]", objMas.DBErrBuf);
                    return JsonRspMsg(1, MthdName, 28, "Success", objMas.DBErrBuf);
                }
                else
                {
                    objMas.PrintLog("A[2,27]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 29, "Failed", null);
                }
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,10]", Ex.Message);
                return Json(new { Status = -1, Message = "Catche Error" + Ex.Message.ToString(), Data = picmodel }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult RejectSection(RejectModel rjtmodel)
        {
            try
            {
                
                var model = new RejectModel();
                

                if (rjtmodel.FromDate == null)           //System.DateTime.Now.ToString("yyyy-MM-dd")
                {
                    var Batches = objMas.GetDataTable("select ph.BatchNo,ph.MaterialCode,mm.MaterialDesc from production_header ph, material_master mm where ExpDate <= curdate() " +
                            " AND ph.MaterialCode=mm.MaterialCode group by BatchNo");

                    model.BatchDetails = (from DataRow a in Batches.Rows
                                          select new SelectListItem { Text = a["BatchNo"].ToString(), Value = a["BatchNo"].ToString() }).ToList();

                    model.MaterialDetails = (from DataRow a in Batches.Rows
                                             select new SelectListItem { Text = a["MaterialDesc"].ToString(), Value = a["MaterialCode"].ToString() }).ToList();

                    model.FromDate = model.ToDate = System.DateTime.Now.ToString("yyyy-MM-dd");


                    model.RejectTypeDetails = new List<SelectListItem>();
                    model.RejectTypeDetails.Add(new SelectListItem { Text = Globals.EXPIRED_REJECT_DESC, Value = Convert.ToInt32(Globals.RJTYPE_EXPIRED_REJECT).ToString() });
                    model.RejectTypeDetails.Add(new SelectListItem { Text = Globals.QUALITY_REJECT_DESC, Value = Convert.ToInt32(Globals.RJTYPE_QUALITY_REJECT).ToString() });


                }
                else
                {
                    var Batches = objMas.GetDataTable("select ph.BatchNo,ph.MaterialCode,mm.MaterialDesc from production_header ph, material_master mm where PDNDate = '"+rjtmodel.FromDate+ "' AND " +
                                " ph.MaterialCode=mm.MaterialCode group by ph.BatchNo,mm.MaterialDesc");
                    //model.BatchDetails.Clear();
                    model.BatchDetails = (from DataRow a in Batches.Rows
                                          select new SelectListItem { Text = a["BatchNo"].ToString(), Value = a["BatchNo"].ToString() }).ToList();

                    //model.MaterialDetails.Clear();
                    model.MaterialDetails = (from DataRow a in Batches.Rows
                                             select new SelectListItem { Text = a["MaterialDesc"].ToString(), Value = a["MaterialCode"].ToString() }).ToList();
                    //return RedirectToAction("GetBatchAndMaterial", "Service", model);
                    //return Json(new { Success = 1, Message = "Sucess", Data = ViewBag.BatchDetails, ViewBag.MaterialDetails });
                }

                ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.STORejection, Request.Cookies["UserId"].Value.ToString());
                model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());
                return View(model);


            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,11]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("Service/GetBatch")]
        public ActionResult GetBatch(RejectModel rjtmodel)
        {
            try
            {
                string MthdName = "GetBatch";
                if (rjtmodel.RejectType == Globals.RJTYPE_EXPIRED_REJECT.ToString())
                {
                    var Batches = objMas.GetDataTable("select date_format(ph.PDNDate,'%d-%m-%Y') as 'Date',ph.BatchNo,ph.MaterialCode,mm.MaterialDesc,pl.ShipmentNo " +
                            " from production_header ph, material_master mm,production_lines pl where ExpDate <= curdate() " +
                            " AND ph.MaterialCode=mm.MaterialCode AND pl.ShipmentNo='"+""+"' AND Date(ph.PDNDate)>='" + rjtmodel.FromDate + "' and Date(ph.PDNDate)<='" + rjtmodel.ToDate + "' group by BatchNo");
                    
                    if (Batches.Rows.Count == 0)
                    {
                        objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 30, "Expired Batch Number Not Found Within The Specified Date Range", null);
                    }

                    else if(Batches == null)
                    {
                        objMas.PrintLog("A[2,29]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 31, objMas.DBErrBuf, null);
                    }

                    var BatchDetails = (from DataRow a in Batches.Rows
                                        select new SelectListItem { Text = a["BatchNo"].ToString(), Value = a["MaterialDesc"].ToString() }).ToList();

                    return Json(new { Success = 1, Message = "Sucess", Data = BatchDetails });
                }


                else if(rjtmodel.RejectType == Globals.RJTYPE_QUALITY_REJECT.ToString())
                {
                    var Batches = objMas.GetDataTable("select date_format(ph.PDNDate,'%d-%m-%Y') as 'Date',ph.BatchNo,ph.MaterialCode,mm.MaterialDesc,pl.ShipmentNo " +
                            " from production_header ph, material_master mm,production_lines pl where ph.MaterialCode=mm.MaterialCode AND pl.ShipmentNo='" + "" + "' AND " +
                            " Date(ph.PDNDate)>='" + rjtmodel.FromDate + "' and Date(ph.PDNDate)<='" + rjtmodel.ToDate + "' group by BatchNo");
                    
                        if (Batches.Rows.Count == 0)
                    {
                        objMas.PrintLog("A[2,30]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 32, "Batch Number Not Found Within The Specified Date Range", null);
                    }

                    else if (Batches == null)
                    {
                        objMas.PrintLog("A[2,31]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 33, objMas.DBErrBuf, null);
                    }

                    var BatchDetails = (from DataRow a in Batches.Rows
                                        select new SelectListItem { Text = a["BatchNo"].ToString(), Value = a["MaterialDesc"].ToString() }).ToList();
                    return JsonRspMsg(1, MthdName, 34, "Sucess", BatchDetails);
                }
                else
                {
                    objMas.PrintLog("A[2,32]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 35, "Select Reject type", null);
                }

            }
            catch(Exception Ex)
            {
                objMas.PrintLog("C[2,12]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Service/GetBarcode")]
        public ActionResult GetBarcode(RejectModel rjtmodel)
        {
            try
            {
                string MthdName = "GetBarcode";
                var model = new RejectModel();
                DataTable dt = new DataTable();
                dt = objMas.GetDataTable("select pl.Barcode,pl.BatchNo from production_lines pl, production_header ph where pl.BatchNo=ph.BatchNo " +
                            " AND pl.BatchNo= '"+rjtmodel.BatchNo+ "' AND pl.Reject = '0' group by pl.Barcode");
                if (dt == null)
                {
                    objMas.PrintLog("A[2,33]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 36, "No Material Found", null);
                }
                var MaterialList = (from DataRow a in dt.Rows
                                                           select new SelectListItem { Text = a["Barcode"].ToString(), Value = a["Barcode"].ToString() }).ToList();

                return Json(new
                {
                    Success = 1,
                    Message = "Success",
                    Data = MaterialList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,13]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Service/GetMaterial")]
        public ActionResult GetMaterial(RejectModel rjtmodel)
        {
            try
            {
                string MthdName = "GetMaterial";
                DataTable dt = new DataTable();
                dt = objMas.GetDataTable("select mm.MaterialDesc from production_lines pl, material_master mm where pl.MaterialCode=mm.MaterialCode AND pl.BatchNo= '" + rjtmodel.BatchNo + "' group by MaterialDesc");
                if (dt == null)
                {
                    objMas.PrintLog("A[2,34]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 37, "No Material Found", null);
                }

                return Json(new
                {
                    Success = 1,
                    Message = "Success",
                    Data = new RejectModel
                    {
                        Material = dt.Rows[0]["MaterialDesc"].ToString()
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception Ex)
            {
                objMas.PrintLog("C[2,14]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }



        [HttpPost]
        [Route("action")]
        [Route("Service/RejectBatchItems")]
        public ActionResult RejectBatchItems(string rjtmodel)
        {
            try
            {
                string MthdName = "RejectBatchItems";
                string[] values = rjtmodel.Split(',');
                string userid = Request.Cookies["UserId"].Value.ToString();
                Int64 Ret = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i] == "on")
                        continue;

                    Ret = objMas.ExecQry("update production_lines set Reject = '1',RejectedBy ='"+ userid + "',RejectedOn=now() where Barcode = '" + values[i] + "'");
                    objMas.PrintLog("RejectBatchItems", objMas.DBErrBuf);
                    
                }
                if (Ret == 1)
                {
                    objMas.PrintLog("A[2,35]", objMas.DBErrBuf);
                    return JsonRspMsg(1, MthdName, 38, "Rejected", null);
                }
                objMas.PrintLog("A[2,36]", objMas.DBErrBuf);
                return JsonRspMsg(0, MthdName, 39, "No Material Found", null);
            }
            catch(Exception Ex)
            {
                objMas.PrintLog("C[2,15]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }


        public ActionResult SalesPicklist()
        {
            var model = new SalesPicklistModel();

            var Locations = objMas.GetDataTable("select SHIPMENT_OR_PO,SENDING_LOCATION from sapdispatchorderheader where TYPES_OF_TRANSACTION='SALES' AND PickFlag='0'  group by SHIPMENT_OR_PO");
            if (Locations == null)
                return View(model);

            model.SipmentDetails = (from DataRow a in Locations.Rows
                                    select new SelectListItem { Text = a["SHIPMENT_OR_PO"].ToString(), Value = a["SHIPMENT_OR_PO"].ToString() }).ToList();

            ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.SALESPicklist, Request.Cookies["UserId"].Value.ToString());
            model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());

            return View(model);
        }


        [HttpPost]
        [Route("action")]
        [Route("Service/GetCFAQuantityDetails")]
        public ActionResult GetCFAQuantityDetails(SalesPicklistModel cfamodel)
        {
            try
            {
                string MthdName = "GetCFAQuantityDetails";
                DataTable dt = new DataTable();
                dt = objMas.GetDataTable("select sum(DELIVERY_QTY_IN_BOX) as BoxQty, b.DELIVERY_NO,b.SENDING_PLANT from sapdispatchorderlines a, sapdispatchorderheader b " +
                            " where b.SHIPMENT_OR_PO =" + cfamodel.Sipment + " and a.DELIVERY_NO=b.DELIVERY_NO and b.TYPES_OF_TRANSACTION = 'SALES'");
                if (dt == null)
                {
                    objMas.PrintLog("A[2,37]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 40, "Failed To Fetch Order Details", null);
                }
                else if (dt.Rows.Count == 0)
                {
                    objMas.PrintLog("A[2,38]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 41, "No Data Found", null);
                }
                else
                {
                    return Json(new
                    {
                        Status = 1,
                        Message = "Success",
                        Data = new PickListModel
                        {
                            BoxQty = dt.Rows[0]["BoxQty"].ToString(),
                            DlNumber = dt.Rows[0]["DELIVERY_NO"].ToString(),
                            FromPlant = dt.Rows[0]["SENDING_PLANT"].ToString(),
                        }
                    }, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,16]", Ex.Message);
                return Json(new { Status = -1, Message = "Catche Error" + Ex.Message.ToString(), Data = cfamodel }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("Service/GetSalesPicklist")]
        public ActionResult GetSalesPicklist(SalesPicklistModel salesmodel)
        {
            try
            {
                Int64 Ret = 0;
                string MthdName = "GetSalesPicklist";
                DataTable dt = new DataTable();


                Ret = objMas.GetTableCount("plant_master where PlantTypeFlag='0' and PlantId ='" + salesmodel.FromPlant + "'");
                if (Ret == -1)
                {
                    objMas.PrintLog("A[2,39]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 42, "Data Fetch Failed", null);
                }
                dt = objMas.GetDataTable("call "+ (Ret >= 1 ? "SalesPicklist" : "SalesCFAPicklist") + "('" + salesmodel.Sipment + "')");        //c.SHIPMENT_OR_PO=a.SHIPMENT_OR_PO and 
                if (dt == null)
                {
                    objMas.PrintLog("A[2,39]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 42, "Data Fetch Failed", null);
                }
                
                else if (dt.Rows.Count == 0)
                {
                    objMas.PrintLog("A[2,40]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 43, "No Data Found For This Selection...", null);
                }
                var sap = (from DataRow row in dt.Rows
                           select new
                           {
                               ShipmentNo = row.Field<string>("ShipmentNo").ToString(),
                               DeliveryNo = row.Field<string>("DeliveryNo").ToString(),
                               BatchNo = row.Field<string>("Batch").ToString(),
                               MaterialCode = row.Field<string>("Material Code").ToString(),
                               MaterialDesc = row.Field<string>("Material Description").ToString(),
                               BoxQty = row.Field<Int32>("Qty in Box").ToString(),
                           }).ToList();
                return Json(sap, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,17]", Ex.Message);
                return Json(new { Status = -1, Message = "Catche Error" + Ex.Message.ToString(), Data = salesmodel }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Service/SalesPicklistGen")]
        public ActionResult SalesPicklistGen(SalesPicklistModel salesmodel)
        {
            try
            {
                string MthdName = "SalesPicklistGen";
                int ret = 0;
                Int64 Ret = 0;
                DataTable dt = new DataTable();
                DataTable dtHeader = new DataTable();
                DataTable dtShipmentWise = new DataTable();
                string[] AddlnData = new string[4];
                int[] AddlnHdrCols = new int[4];
                float[] ShipWiseColWdth = new float[6] { 1f, 2f, 6f, 2f, 2f, 1f };
                float[] OutletWiseColWdth = new float[5] { 1f, 2f, 4f, 2f, 2f };
                string[] OutletAddlnData = new string[10];
                int[] OutletAddlnHdrCols = new int[10];

                dtHeader = objMas.GetDataTable("Select SHIPMENT_OR_PO,DELIVERY_NO,SENDING_PLANT,RECEIVING_PLANT,TRUCK_NUMBER,b.PlantName as SendingPlant," +
                   "a.OUTLET_NAME,a.INDENT_NUMBER from sapdispatchorderheader a,Plant_master b,Plant_master c where a.SENDING_PLANT = b.PlantID and " +
                   "SHIPMENT_OR_PO='"+ salesmodel.Sipment+"' and a.Types_OF_Transaction='SALES'");
                if (dtHeader == null)
                {
                    objMas.PrintLog("A[2,24]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 26, "Data Fetch Failed", null);
                }

                //Shipment Header
                AddlnData[0] = "Plant : " + dtHeader.Rows[0]["SENDING_PLANT"].ToString() + " - " + dtHeader.Rows[0]["SendingPlant"].ToString();
                AddlnHdrCols[0] = 3;

                AddlnData[1] = "Truck No : " + dtHeader.Rows[0]["TRUCK_NUMBER"].ToString();
                AddlnHdrCols[1] = 3;

                AddlnData[2] = " ";
                AddlnHdrCols[2] = 3;

                AddlnData[3] = "Shipment No : " + dtHeader.Rows[0]["SHIPMENT_OR_PO"].ToString();
                AddlnHdrCols[3] = 3;



                Ret = objMas.GetTableCount("plant_master where PlantTypeFlag='0' and PlantId ='" + salesmodel.FromPlant + "'");
                if (Ret == -1)
                {
                    objMas.PrintLog("A[2,39]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 42, "Data Fetch Failed", null);
                }

                dtShipmentWise = dt = objMas.GetDataTable("call "+ (Ret >= 1 ? "SalesPicklist" : "SalesCFAPicklist") + "('" + salesmodel.Sipment + "')");        //c.SHIPMENT_OR_PO=a.SHIPMENT_OR_PO and 

                if (dt == null)
                {
                    objMas.PrintLog("A[2,41]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 44, "Data Fetch Failed", null);
                }
                else if (dt.Rows.Count == 0)
                {
                    objMas.PrintLog("A[2,42]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 45, "No Data Found For This Selection", null);
                }

                var data = (from row in dtShipmentWise.AsEnumerable()
                            group row by new
                            {
                                MaterialCode = row.Field<string>("Material Code"),
                                MaterialDescription = row.Field<string>("Material Description"),
                                Batch = row.Field<string>("Batch"),
                            } into grp
                            //orderby grp.Key
                            select new ShipmentDetails
                            {
                                SlNo  = 0,
                                MaterialCode = grp.Key.MaterialCode,
                                MaterialDescription = grp.Key.MaterialDescription,
                                Batch = grp.Key.Batch,
                                QtyinBox = grp.Sum(r => Convert.ToInt32(r.Field<Int32>("Qty in Box"))),
                                QtyinEA = grp.Sum(r => Convert.ToInt64(r.Field<Int64>("Qty in EA")))
                            }).OrderBy(x => x.MaterialCode).Select((e, index) => { e.SlNo = (index + 1); return e; }).ToList();

                dtShipmentWise = Utils.ToDataTable(data);
                dtShipmentWise.Columns["MaterialCode"].ColumnName="Material Code";
                dtShipmentWise.Columns["MaterialDescription"].ColumnName = "Material Description";
                dtShipmentWise.Columns["QtyinBox"].ColumnName = "Qty in Box";
                dtShipmentWise.Columns["QtyinEA"].ColumnName = "Qty in EA";

                dt.Columns.Remove("ShipmentNo");

                int[] DType = Enumerable.Range(0, dtShipmentWise.Columns.Count).Select(n => MasterLogic.RPTSTYLE_STRING_SHORT).ToArray();
                DType[0] = MasterLogic.RPTSTYLE_INT;
                int[] Totalcolmns = new int[6] { 0, 0, 0, 0, 1, 1 };

                ret = objMas.GenerateReport(Convert.ToInt16(Globals.DWNLDFMT_PDF), 0, 0, 0, "SalesShipmentPiclist_" + dtHeader.Rows[0]["SHIPMENT_OR_PO"].ToString()
                    , "NAGA - SALES Pickslip - Shipmentwise", DType, null, Totalcolmns, "", "", AddlnData, AddlnHdrCols, null, null, dtShipmentWise, ShipWiseColWdth);

                if (ret <= 0)
                {
                    objMas.PrintLog("A[2,43]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 46, "Failed", null);
                }
                for (int i = 0; i< dtHeader.Rows.Count; i++)
                {
                    var delivery = (from row in dt.AsEnumerable().Where(row => row.Field<String>("DeliveryNo") == dtHeader.Rows[i]["DELIVERY_NO"].ToString())
                                    group row by new
                                    {
                                        MaterialCode = row.Field<string>("Material Code"),
                                        MaterialDescription = row.Field<string>("Material Description"),
                                    } into grp
                                    select new ShipmentDetails
                                    {
                                        SlNo  = 0,
                                        MaterialCode = grp.Key.MaterialCode,
                                        MaterialDescription = grp.Key.MaterialDescription,
                                        QtyinBox = grp.Sum(r => Convert.ToInt32(r.Field<int>("Qty in Box"))),
                                        QtyinEA = grp.Sum(r => Convert.ToInt64(r.Field<Int64>("Qty in EA")))
                                    }).OrderBy(x => x.MaterialCode).Select((e, index) => { e.SlNo = (index + 1); return e; }).ToList();

                    dtShipmentWise = Utils.ToDataTable(delivery);
                    dtShipmentWise.Columns["MaterialCode"].ColumnName="Material Code";
                    dtShipmentWise.Columns["MaterialDescription"].ColumnName = "Material Description";
                    dtShipmentWise.Columns["QtyinBox"].ColumnName = "Qty in Box";
                    dtShipmentWise.Columns["QtyinEA"].ColumnName = "Qty in EA";

                    dtShipmentWise.Columns.Remove("Batch");

                    Array.Resize(ref DType, dtShipmentWise.Columns.Count);
                    DType = Enumerable.Range(0, dtShipmentWise.Columns.Count).Select(n => MasterLogic.RPTSTYLE_STRING_SHORT).ToArray();
                    Totalcolmns = new int[5] { 0, 0, 0, 1, 1 };
                    //Outlet Header
                    OutletAddlnData[0] = "Plant : " + dtHeader.Rows[i]["SENDING_PLANT"].ToString() + " - " + dtHeader.Rows[i]["SendingPlant"].ToString();
                    OutletAddlnHdrCols[0] = 3;

                    OutletAddlnData[1] = "Outlet Name : " + dtHeader.Rows[i]["OUTLET_NAME"].ToString();
                    OutletAddlnHdrCols[1] = 2;

                    OutletAddlnData[2] = " ";
                    OutletAddlnHdrCols[2] = 3;

                    OutletAddlnData[3] = "Indent No : " + dtHeader.Rows[i]["INDENT_NUMBER"].ToString();
                    OutletAddlnHdrCols[3] = 2;

                    OutletAddlnData[4] = " ";
                    OutletAddlnHdrCols[4] = 3;

                    OutletAddlnData[5] = "Truck No : " + dtHeader.Rows[i]["TRUCK_NUMBER"].ToString();
                    OutletAddlnHdrCols[5] = 2;

                    OutletAddlnData[6] = " ";
                    OutletAddlnHdrCols[6] = 3;

                    OutletAddlnData[7] = "Shipment No : " + dtHeader.Rows[i]["SHIPMENT_OR_PO"].ToString();
                    OutletAddlnHdrCols[7] = 2;

                    OutletAddlnData[8] = " ";
                    OutletAddlnHdrCols[8] = 3;

                    OutletAddlnData[9] = "Delivery No : " + dtHeader.Rows[i]["DELIVERY_NO"].ToString();
                    OutletAddlnHdrCols[9] = 2;

                    ret = objMas.GenerateReport(Convert.ToInt16(Globals.DWNLDFMT_PDF), 0, 0, 0, "SalesShipmentPiclist_"+
                        dtHeader.Rows[i]["SHIPMENT_OR_PO"].ToString() + "_" + dtHeader.Rows[i]["DELIVERY_NO"].ToString(),
                        "NAGA - SALES Pickslip - Outletwise", DType, null, Totalcolmns, "", "", OutletAddlnData, OutletAddlnHdrCols, null, null, dtShipmentWise, OutletWiseColWdth);
                    if (ret <= 0)
                    {
                        objMas.PrintLog("A[2,43]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 46, "Failed", null);
                    }
                    ret = objMas.MergePdf("SalesShipmentPiclist_"+ dtHeader.Rows[i]["SHIPMENT_OR_PO"].ToString());
                    if (ret <= 0)
                    {
                        objMas.PrintLog("A[2,43]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 46, "Failed To Merge", null);
                    }
                }
                return Json(new { Success = 1, Message = "Success", Data = objMas.DBErrBuf });
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,18]", Ex.Message);
                return Json(new { Status = -1, Message = "Catche Error" + Ex.Message.ToString(), Data = salesmodel }, JsonRequestBehavior.AllowGet);
            }
        }

        public class ShipmentDetails 
        {
            public Int64 SlNo { get; set; }

            public string MaterialCode { get; set; }

            public string MaterialDescription { get; set; }

            public string Batch { get; set; }

            public Int64 QtyinBox { get; set; }

            public Int64 QtyinEA { get; set; }

        }

        public ActionResult CFARejectSection(CFARejectModel rjtmodel)
        {
            try
            {

                var model = new CFARejectModel();


                if (rjtmodel.FromDate == null)           //System.DateTime.Now.ToString("yyyy-MM-dd")
                {
                    var Batches = objMas.GetDataTable("select ph.BatchNo,ph.MaterialCode,mm.MaterialDesc from production_header ph, material_master mm where ExpDate <= curdate() " +
                            " AND ph.MaterialCode=mm.MaterialCode group by BatchNo");

                    model.BatchDetails = (from DataRow a in Batches.Rows
                                          select new SelectListItem { Text = a["BatchNo"].ToString(), Value = a["BatchNo"].ToString() }).ToList();

                    model.MaterialDetails = (from DataRow a in Batches.Rows
                                             select new SelectListItem { Text = a["MaterialDesc"].ToString(), Value = a["MaterialCode"].ToString() }).ToList();

                    model.FromDate = model.ToDate = System.DateTime.Now.ToString("yyyy-MM-dd");


                    model.RejectTypeDetails = new List<SelectListItem>();
                    model.RejectTypeDetails.Add(new SelectListItem { Text = Globals.EXPIRED_REJECT_DESC, Value = Convert.ToInt32(Globals.RJTYPE_EXPIRED_REJECT).ToString() });
                    model.RejectTypeDetails.Add(new SelectListItem { Text = Globals.QUALITY_REJECT_DESC, Value = Convert.ToInt32(Globals.RJTYPE_QUALITY_REJECT).ToString() });


                }
                else
                {
                    var Batches = objMas.GetDataTable("select ph.BatchNo,ph.MaterialCode,mm.MaterialDesc from production_header ph, material_master mm where PDNDate = '" + rjtmodel.FromDate + "' AND " +
                                " ph.MaterialCode=mm.MaterialCode group by ph.BatchNo,mm.MaterialDesc");
                    //model.BatchDetails.Clear();
                    model.BatchDetails = (from DataRow a in Batches.Rows
                                          select new SelectListItem { Text = a["BatchNo"].ToString(), Value = a["BatchNo"].ToString() }).ToList();

                    //model.MaterialDetails.Clear();
                    model.MaterialDetails = (from DataRow a in Batches.Rows
                                             select new SelectListItem { Text = a["MaterialDesc"].ToString(), Value = a["MaterialCode"].ToString() }).ToList();
                    //return RedirectToAction("GetBatchAndMaterial", "Service", model);
                    //return Json(new { Success = 1, Message = "Sucess", Data = ViewBag.BatchDetails, ViewBag.MaterialDetails });
                }

                ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.CFARejection, Request.Cookies["UserId"].Value.ToString());
                model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());
                return View(model);


            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,11]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Service/CFAGetBatch")]
        public ActionResult CFAGetBatch(CFARejectModel rjtmodel)
        {
            try
            {
                string MthdName = "GetBatch";
                if (rjtmodel.RejectType == Globals.RJTYPE_EXPIRED_REJECT.ToString())
                {
                    var Batches = objMas.GetDataTable("select date_format(gd.PDNDate, '%d-%m-%y') as 'Date',gd.BatchNo,gd.MaterialCode,mm.MaterialDesc " +
                                                      " from cfagrn_details gd, material_master mm, production_lines pl, production_header ph " +
                                                      " where gd.Barcode=pl.Barcode and gd.MaterialCode=mm.MaterialCode and pl.PDNId=ph.PDNId " +
                                                      " and gd.BatchNo=pl.BatchNo and ToShipmentNo= '' and ph.ExpDate <= curdate() and " +
                                                      " Date(ph.PDNDate)>='" + rjtmodel.FromDate + "' and Date(ph.PDNDate)<='" + rjtmodel.ToDate + "' group by BatchNo");

                    if (Batches.Rows.Count == 0)
                    {
                        objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 47, "Expired Batch Number Not Found Within The Specified Date Range", null);
                    }

                    else if (Batches == null)
                    {
                        objMas.PrintLog("A[2,29]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 48, objMas.DBErrBuf, null);
                    }

                    var BatchDetails = (from DataRow a in Batches.Rows
                                        select new SelectListItem { Text = a["BatchNo"].ToString(), Value = a["MaterialDesc"].ToString() }).ToList();

                    return Json(new { Success = 1, Message = "Sucess", Data = BatchDetails });
                }


                else if (rjtmodel.RejectType == Globals.RJTYPE_QUALITY_REJECT.ToString())
                {
                    var Batches = objMas.GetDataTable("select date_format(ph.PDNDate,'%d-%m-%Y') as 'Date',ph.BatchNo,ph.MaterialCode,mm.MaterialDesc,pl.ShipmentNo " +
                                                     " from cfagrn_details ph, material_master mm,production_lines pl where ph.MaterialCode=mm.MaterialCode " +
                                                     " AND ph.ToShipmentNo='' AND ph.ToDeliveryNo = '' AND  Date(ph.PDNDate)>='" + rjtmodel.FromDate + "' " +
                                                     " and Date(ph.PDNDate)<='" + rjtmodel.ToDate + "' group by BatchNo");

                    if (Batches.Rows.Count == 0)
                    {
                        objMas.PrintLog("A[2,30]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 49, "Batch Number Not Found Within The Specified Date Range", null);
                    }

                    else if (Batches == null)
                    {
                        objMas.PrintLog("A[2,31]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 50, objMas.DBErrBuf, null);
                    }

                    var BatchDetails = (from DataRow a in Batches.Rows
                                        select new SelectListItem { Text = a["BatchNo"].ToString(), Value = a["MaterialDesc"].ToString() }).ToList();
                    return JsonRspMsg(1, MthdName, 51, "Sucess", BatchDetails);
                }
                else
                {
                    objMas.PrintLog("A[2,32]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 52, "Select Reject type", null);
                }

            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,12]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Service/CFAGetBarcode")]
        public ActionResult CFAGetBarcode(CFARejectModel rjtmodel)
        {
            try
            {
                string MthdName = "GetBarcode";
                var model = new CFARejectModel();
                DataTable dt = new DataTable();
                dt = objMas.GetDataTable("select gd.Barcode,gd.BatchNo from cfagrn_details gd, production_lines pl where gd.BatchNo=pl.BatchNo " +
                                        " and gd.BatchNo= '" + rjtmodel.BatchNo + "' AND gd.Reject = '0' group by gd.Barcode");
                if (dt == null)
                {
                    objMas.PrintLog("A[2,33]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 53, "No Material Found", null);
                }
                var MaterialList = (from DataRow a in dt.Rows
                                    select new SelectListItem { Text = a["Barcode"].ToString(), Value = a["Barcode"].ToString() }).ToList();

                return Json(new
                {
                    Success = 1,
                    Message = "Success",
                    Data = MaterialList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,13]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Service/CFAGetMaterial")]
        public ActionResult CFAGetMaterial(CFARejectModel rjtmodel)
        {
            try
            {
                string MthdName = "GetMaterial";
                DataTable dt = new DataTable();
                dt = objMas.GetDataTable("select mm.MaterialDesc from cfagrn_details gd, material_master mm where gd.MaterialCode=mm.MaterialCode AND gd.BatchNo= '" + rjtmodel.BatchNo + "' group by MaterialDesc");
                if (dt == null)
                {
                    objMas.PrintLog("A[2,34]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 54, "No Material Found", null);
                }

                return Json(new
                {
                    Success = 1,
                    Message = "Success",
                    Data = new CFARejectModel
                    {
                        Material = dt.Rows[0]["MaterialDesc"].ToString()
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,14]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }



        [HttpPost]
        [Route("action")]
        [Route("Service/CFARejectBatchItems")]
        public ActionResult CFARejectBatchItems(string rjtmodel)
        {
            try
            {
                string MthdName = "RejectBatchItems";
                string[] values = rjtmodel.Split(',');
                Int64 Ret = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i] == "on")
                        continue;

                    Ret = objMas.ExecQry("update cfagrn_details set Reject = '1' where Barcode = '" + values[i] + "'");
                    objMas.PrintLog("RejectBatchItems", objMas.DBErrBuf);

                }
                if (Ret == 1)
                {
                    objMas.PrintLog("A[2,35]", objMas.DBErrBuf);
                    return JsonRspMsg(1, MthdName, 55, "Rejected", null);
                }
                objMas.PrintLog("A[2,36]", objMas.DBErrBuf);
                return JsonRspMsg(0, MthdName, 56, "No Material Found", null);
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,15]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }

        [HttpGet]
        [Route("action")]
        [Route("Service/frmBatchHold")]
        public ActionResult frmBatchHold()
        {
            try
            {

                var model = new BatchHoldModel();
                ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.CFARejection, Request.Cookies["UserId"].Value.ToString());
                model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());

                model.HoldTypeDetails = new List<SelectListItem>();
                model.HoldTypeDetails.Add(new SelectListItem { Text = Globals.QUALITY_HOLD_DESC, Value = Convert.ToInt32(Globals.QUALITY_HOLD).ToString() });
                model.HoldTypeDetails.Add(new SelectListItem { Text = Globals.DAMAGE_HOLD_DESC, Value = Convert.ToInt32(Globals.DAMAGE_HOLD).ToString() });
                model.HoldTypeDetails.Add(new SelectListItem { Text = Globals.FG_DISCARD_DESC, Value = Convert.ToInt32(Globals.FG_DISCARD).ToString() });

                DataTable dt = objMas.GetDataTable("Select PlantId as Value,PlantName as Text from plant_master where status=1");
                if (dt == null)
                {
                    objMas.PrintLog("Error ", objMas.DBErrBuf);
                    return View(model);
                }
                else
                    model.PlantDetails = objMas.ConvertToList<SelectListItem>(dt);

                return View(model);
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,11]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Service/LoadPlantBatch")]
        public ActionResult LoadPlantBatch(BatchHoldModel batchmodel)
        {
            try
            {
                string MthdName = "LoadPlantBatch";
                DataTable dt = new DataTable();
                Int64 Ret = 0;

                Ret = objMas.GetTableCount("Plant_master where PlantId='"+batchmodel.PlantId+"' and PlantTypeFlag=0");
                if (Ret == -1)
                {
                    objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Plant Details", null);

                }
                else if (Ret >= 1)
                {
                    dt = objMas.GetDataTable("Select Distinct a.MaterialCode as Value,b.MaterialDesc as Text from production_lines a, Material_master b where " +
                        "a.pickflag=0 and a.Reject=0 and a.Hold=0 and a.MaterialCode=b.MaterialCode");
                    if (dt == null)
                    {
                        objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Batch Details", null);
                    }
                }
                else 
                {
                    dt = objMas.GetDataTable("Select Distinct a.MaterialCode as Value,b.MaterialDesc as Text from cfagrn_details a, Material_master b where a.pickflag=0 and " +
                        "a.Reject=0 and a.ReceivingPlant='" + batchmodel.PlantId + "' and a.Hold=0 and a.MaterialCode=b.MaterialCode");
                    if (dt == null)
                    {
                        objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Batch Details", null);
                    }
                }
                var BatchDetails = objMas.ConvertToList<SelectListItem>(dt);
                return Json(new { Success = 1, Message = "Sucess", Data = BatchDetails });
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,12]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("Service/BatchMaterial")]
        public ActionResult BatchMaterial(BatchHoldModel btchmodel)
        {
            try
            {
                string MthdName = "BatchMaterial";
                DataTable dt = new DataTable();

                Int64 Ret = 0;

                Ret = objMas.GetTableCount("Plant_master where PlantId='" + btchmodel.PlantId + "' and PlantTypeFlag=0");
                if (Ret == -1)
                {
                    objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Plant Details", null);

                }
                else if (Ret >= 1)
                {
                    dt = objMas.GetDataTable("Select Distinct BatchNo as Value,BatchNo as Text from production_lines  where pickflag=0 and Reject=0 " +
                                "and Hold=0 and MaterialCode ='" + btchmodel.MaterialCode + "' ");
                    if (dt == null)
                    {
                        objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Batch Details", null);
                    }
                }
                else
                {
                    dt = objMas.GetDataTable("Select Distinct BatchNo as Value,BatchNo as Text from cfagrndetails  where pickflag=0 and Reject=0 " +
                                "and ReceivingPlant='" + btchmodel.PlantId + "' and Hold=0 and MaterialCode ='" + btchmodel.MaterialCode+"' ");
                    if (dt == null)
                    {
                        objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Batch Details", null);
                    }
                }
                var BatchDetails = objMas.ConvertToList<SelectListItem>(dt);
                return Json(new { Success = 1, Message = "Sucess", Data = BatchDetails });
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,14]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Service/BatchesGetBarcode")]
        public ActionResult BatchesGetBarcode(BatchHoldModel batchmodel)
        {
            try
            {
                string MthdName = "BatchesGetBarcode";
                var model = new BatchHoldModel();
                DataTable dt = new DataTable();
                Int64 Ret = 0;

                Ret = objMas.GetTableCount("Plant_master where PlantId='" + batchmodel.PlantId + "' and PlantTypeFlag = 0");
                if (Ret == -1)
                {
                    objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Plant Details", null);

                }
                else if (Ret >= 1)
                {
                    dt = objMas.GetDataTable("Select Barcode as Text,Barcode as Value from production_lines where BatchNo='" + batchmodel.BatchNo
                        + "' and pickflag=0 and Reject=0 and Hold=0");
                    if (dt == null)
                    {
                        objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Batch Details", null);
                    }
                }
                else
                {
                    dt = objMas.GetDataTable("Select Barcode as Text,Barcode as Value from cfagrn_details where BatchNo='" + batchmodel.BatchNo
                        + "' and pickflag=0 and Reject=0 and Hold=0 and ReceivingPlant='"+ batchmodel.PlantId +"'");
                    if (dt == null)
                    {
                        objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Batch Details", null);
                    }
                }
                var MaterialList = objMas.ConvertToList<SelectListItem>(dt);
                return Json(new
                {
                    Success = 1,
                    Message = "Success",
                    Data = MaterialList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,13]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Service/HoldBatchItems")]
        public ActionResult HoldBatchItems(string btchmodel,BatchHoldModel batchHoldModel)
        {
            try
            {
                string MthdName = "HoldBatchItems";
                string[] values = btchmodel.Split(',');
                string[] Qry = new string[values.Count() * 2];
                string tblname = "";
                Int64 Ret = 0;

                Ret = objMas.GetTableCount("Plant_master where PlantId='"+ batchHoldModel.PlantId +"' and PlantTypeFlag=0");
                if (Ret == -1)
                {
                    objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Plant Details", null);

                }
                else if (Ret >= 1)
                    tblname = "production_lines";
                else
                    tblname = "cfagrn_details";

                Ret = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i] == "on")
                        continue;

                    Qry[Ret++] = "update "+tblname+" set Hold = 1 where Barcode = '" + values[i] + "'";
                    Qry[Ret++] = "Insert into batchholddetails(PlantId,MaterialCode,MaterialDesc,BatchNo,Barcode," +
                                 "HoldDate,ReleaseDate,FgDiscardDate,HoldType,CreatedBy,CreatedOn)values('" + batchHoldModel.PlantId
                                 +"','" + batchHoldModel.MaterialCode +"','" + batchHoldModel.MaterialDesc + "','" +
                                 batchHoldModel.BatchNo +"','" + values[i] + "',now(),'0000-00-00 00:00:00','0000-00-00 00:00:00','" +
                                 batchHoldModel.HoldType +"','" + Request.Cookies["UserId"].Value.ToString() + "',now())";
                }
                Ret = objMas.ExecMultipleQry(Ret, Qry);
                if (Ret >= 1)
                {
                    objMas.PrintLog("A[2,35]", objMas.DBErrBuf);
                    return JsonRspMsg(1, MthdName, 55, "Holded", null);
                }
                objMas.PrintLog("A[2,36]", objMas.DBErrBuf);
                return JsonRspMsg(0, MthdName, 56, "No Material Found", null);
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,15]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }

        /* Batch Reject Release */

        [HttpGet]
        [Route("action")]
        [Route("Service/frmBatchRejectRelease")]
        public ActionResult frmBatchRejectRelease()
        {
            try
            {

                var model = new BatchHoldModel();
                ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.CFARejection, Request.Cookies["UserId"].Value.ToString());
                model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());

                model.HoldTypeDetails = new List<SelectListItem>();
                model.HoldTypeDetails.Add(new SelectListItem { Text = "FG Discard", Value = Globals.RJTYPE_QUALITY_REJECT.ToString() });
                model.HoldTypeDetails.Add(new SelectListItem { Text = "Release", Value = Globals.HOLD_RELEASE.ToString() });

                DataTable dt = objMas.GetDataTable("Select PlantId as Value,PlantName as Text from plant_master where status=1");
                if (dt == null)
                {
                    objMas.PrintLog("Error ", objMas.DBErrBuf);
                    return View(model);
                }
                else
                    model.PlantDetails = objMas.ConvertToList<SelectListItem>(dt);

                return View(model);
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,11]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Service/LoadPlantReleaseBatch")]
        public ActionResult LoadPlantReleaseBatch(BatchHoldModel batchmodel)
        {
            try
            {
                string MthdName = "LoadPlantReleaseBatch";
                DataTable dt = new DataTable();
                Int64 Ret = 0;

                Ret = objMas.GetTableCount("Plant_master where PlantId='"+ batchmodel.PlantId +"' and PlantTypeFlag=0");
                if (Ret == -1)
                {
                    objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Plant Details", null);

                }
                else if (Ret >= 1)
                {
                    dt = objMas.GetDataTable("Select Distinct a.MaterialCode as Value,b.MaterialDesc as Text from production_lines a, material_master b where " +
                        "a.MaterialCode=b.MaterialCode and a.pickflag=0 and a.Reject=0 and a.Hold=1");
                    if (dt == null)
                    {
                        objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Batch Details", null);
                    }
                }
                else
                {
                    dt = objMas.GetDataTable("Select Distinct a.MaterialCode as Value,b.MaterialDesc as Text from cfagrn_details a, material_master b where " +
                        "a.MaterialCode=b.MaterialCode and a.pickflag=0 and a.Reject=0 and a.ReceivingPlant='" + batchmodel.PlantId + "' and a.Hold=1");
                    if (dt == null)
                    {
                        objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Batch Details", null);
                    }
                }
                var BatchDetails = objMas.ConvertToList<SelectListItem>(dt);
                return Json(new { Success = 1, Message = "Success", Data = BatchDetails });
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,12]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("Service/ReleaseBatchMaterial")]
        public ActionResult ReleaseBatchMaterial(BatchHoldModel batchmodel)
        {
            try
            {
                string MthdName = "ReleaseBatchMaterial";
                DataTable dt = new DataTable();
                Int64 Ret = 0;

                Ret = objMas.GetTableCount("Plant_master where PlantId='" + batchmodel.PlantId + "' and PlantTypeFlag=0");
                if (Ret == -1)
                {
                    objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Plant Details", null);

                }
                else if (Ret >= 1)
                {
                    dt = objMas.GetDataTable("Select Distinct BatchNo as Value,BatchNo as Text from production_lines  where pickflag=0 and Reject=0 and " +
                        "Hold=1 and MaterialCode ='" + batchmodel.MaterialCode + "' ");
                    if (dt == null)
                    {
                        objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Batch Details", null);
                    }
                }
                else
                {
                    dt = objMas.GetDataTable("Select Distinct BatchNo as Value,BatchNo as Text from cfagrndetails  where pickflag=0 and Reject=0 " +
                                "and ReceivingPlant='" + batchmodel.PlantId + "' and Hold=1 and MaterialCode ='" + batchmodel.MaterialCode+"' ");
                    if (dt == null)
                    {
                        objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Batch Details", null);
                    }
                }

                var BatchDetails = objMas.ConvertToList<SelectListItem>(dt);
                return Json(new { Success = 1, Message = "Success", Data = BatchDetails });

            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,14]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("Service/ReleaseBatchesGetBarcode")]
        public ActionResult ReleaseBatchesGetBarcode(BatchHoldModel batchmodel)
        {
            try
            {
                string MthdName = "ReleaseBatchesGetBarcode";
                var model = new BatchHoldModel();
                DataTable dt = new DataTable();
                Int64 Ret = 0;

                Ret = objMas.GetTableCount("Plant_master where PlantId='"+ batchmodel.PlantId +"' and PlantTypeFlag=0");
                if (Ret == -1)
                {
                    objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Plant Details", null);

                }
                else if (Ret >= 1)
                {
                    dt = objMas.GetDataTable("Select Barcode as Text,Barcode as Value from production_lines where BatchNo='" + batchmodel.BatchNo
                        + "' and pickflag=0 and Reject=0 and Hold=1");
                    if (dt == null)
                    {
                        objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Batch Details", null);
                    }
                }
                else
                {
                    dt = objMas.GetDataTable("Select Barcode as Text,Barcode as Value from cfagrn_details where BatchNo='" + batchmodel.BatchNo
                        + "' and pickflag=0 and Reject=0 and Hold=1 and ReceivingPlant='"+ batchmodel.PlantId +"'");
                    if (dt == null)
                    {
                        objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Batch Details", null);
                    }
                }
                var MaterialList = objMas.ConvertToList<SelectListItem>(dt);
                return Json(new
                {
                    Success = 1,
                    Message = "Success",
                    Data = MaterialList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,13]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Service/ReleaseBatchItems")]
        public ActionResult ReleaseBatchItems(string btchmodel, BatchHoldModel batchHoldModel)
        {
            try
            {
                string MthdName = "ReleaseBatchItems";
                string[] values = btchmodel.Split(',');
                string[] Qry = new string[values.Count() * 2];
                string tblname = "";
                Int64 Ret = 0;

                Ret = objMas.GetTableCount("Plant_master where PlantId='"+ batchHoldModel.PlantId +"' and PlantTypeFlag=0");
                if (Ret == -1)
                {
                    objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Plant Details", null);

                }
                else if (Ret >= 1)
                    tblname = "production_lines";
                else
                    tblname = "cfagrn_details";

                Ret = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i] == "on")
                        continue;

                    Qry[Ret++] = "update "+ tblname +" set "+ (batchHoldModel.HoldType == 1 ? "Reject = 1" : "Hold = 0") + " where Barcode = '" + values[i] + "' and Hold=1";
                    Qry[Ret++] = "Update batchholddetails set "+ (batchHoldModel.HoldType == 1 ? "FgDiscardDate = now()" :
                                 "ReleaseDate = now()") + " where PlantId='" + batchHoldModel.PlantId + "' and Barcode = '"
                                 + values[i] + "' and FgDiscardDate='0000-00-00 00:00:00' and ReleaseDate='0000-00-00 00:00:00'";
                }
                Ret = objMas.ExecMultipleQry(Ret, Qry);
                if (Ret >= 1)
                {
                    objMas.PrintLog("A[2,35]", objMas.DBErrBuf);
                    return JsonRspMsg(1, MthdName, 55, (batchHoldModel.HoldType == 1 ? "Rejected Successfully" : "Released Successfully"), null);
                }
                objMas.PrintLog("A[2,36]", objMas.DBErrBuf);
                return JsonRspMsg(0, MthdName, 56, "No Material Found", null);
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,15]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }



        /* Approve */

        [HttpGet]
        [Route("action")]
        [Route("Service/frmApproval")]
        public ActionResult frmApproval()
        {
            try
            {

                var model = new ApprovalModel();
                ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.ShipmentPoApproval, Request.Cookies["UserId"].Value.ToString());
                model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());

                DataTable dt = objMas.GetDataTable("Select PlantId as Value,PlantName as Text from plant_master where status=1");
                if (dt == null)
                {
                    objMas.PrintLog("Error ", objMas.DBErrBuf);
                    return View(model);
                }
                else
                    model.PlantDetails = objMas.ConvertToList<SelectListItem>(dt);

                model.SipmentDetails = new List<SelectListItem>();

                return View(model);
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,11]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Service/GetPurchaseShipmentDetails")]
        public ActionResult GetPurchaseShipmentDetails(ApprovalModel approvalmodel)
        {
            try
            {
                string MthdName = "GetPurchaseShipmentDetails";
                DataTable dt = new DataTable();

                dt = objMas.GetDataTable("Select Distinct ShipmentNo as Value,ShipmentNo as Text from shipment_approval_details where " +
                    "Plant='" + approvalmodel.PlantCode + "' and ApprovalStatus=0");
                if (dt == null)
                {
                    objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Batch Details", null);
                }

                var ShipmentpurchaseDetails = objMas.ConvertToList<SelectListItem>(dt);
                return Json(new { Success = 1, Message = "Success", Data = ShipmentpurchaseDetails });
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,12]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("Service/GetShipmentApproval")]
        public ActionResult GetShipmentApproval(ApprovalModel approvalmodel)
        {
            try
            {
                string MthdName = "GetShipmentApproval";
                DataTable dt = new DataTable();

                string sQry = "select a.DeliveryNo,a.MaterialCode,a.MaterialDesc,a.BatchNo,Count(c.BatchNo) as AvailableQty,a.ProposedQty,a.PickedQty," +
                              "a.DifferenceQty,b.OUTLET_NAME as Outlet from shipment_approval_details a,SapdispatchOrderheader b," +
                              (approvalmodel.PlantCode == "9300" ? "Production_lines c" : "cfagrn_details c") + " where a.ShipmentNo='" +
                              approvalmodel.Shipment + "' and a.ShipmentNo=b.SHIPMENT_OR_PO and a.ApprovalStatus=0 and a.DeliveryNo=b.DELIVERY_NO " +
                              "and a.Plant='" + approvalmodel.PlantCode + "' and a.BatchNo = c.BatchNo and c.PickFlag=0 and c.CrateBarcode <> '' " +
                              "and c.PutAwayFlag = 1 and c.Reject = 0 group by a.BatchNo, a.DeliveryNo";
                if (approvalmodel.PlantCode != "9300")
                    sQry += " UNION select a.DeliveryNo,a.MaterialCode,a.MaterialDesc,a.BatchNo,Floor(c.BalanceQty) as AvailableQty,a.ProposedQty,a.PickedQty," +
                    "a.DifferenceQty,b.OUTLET_NAME as Outlet from shipment_approval_details a,SapdispatchOrderheader b,Stock_nobarcode c" +
                    " where a.ShipmentNo='" + approvalmodel.Shipment + "' and a.ShipmentNo=b.SHIPMENT_OR_PO and a.ApprovalStatus=0 and " +
                    "a.DeliveryNo=b.DELIVERY_NO and a.Plant='" + approvalmodel.PlantCode + "' and a.BatchNo = c.BatchNo and c.BalanceQty <> 0 " +
                    "and c.CrateBarcode <> '' Group by BatchNo,b.DELIVERY_NO";

                sQry += " Order by DeliveryNo,MaterialCode,BatchNo";


                dt = objMas.GetDataTable(sQry);
                if (dt == null)
                {
                    objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 47, "Failed To Fetch Batch Details", null);
                }

                var ShipmentpurchaseDetails = objMas.ConvertToList<ApprovalDetails>(dt);
                return Json(new { Success = 1, Message = "Success", Data = ShipmentpurchaseDetails });
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,12]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Service/Approve")]
        public ActionResult Approve(ApprovalModel approvalmodel)
        {
            try
            {
                Int64 Ret = 0;

                Ret = objMas.ExecQry("Update shipment_approval_details set ApprovalStatus=1,ApprovedBy='"+ Request.Cookies["UserId"].Value.ToString()+
                                "',ApprovedOn=now() where ShipmentNo='" + approvalmodel.Shipment
                                + "' and Plant = '" + approvalmodel.PlantCode + "'");
                if (Ret >= 1)
                    return Json(new { Success = 1, Message = "Approved Success" });
                else
                    return Json(new { Success = 0, Message = "Failed To Approve " + objMas.DBErrBuf });
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,12]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }


        public ActionResult MissingCrates()
        {
            try
            {
                var model = new MissingCrateModel();
                ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.CrateMissing, Request.Cookies["UserId"].Value.ToString());
                model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());

                //model.FromDate = model.ToDate = System.DateTime.Now.ToString("yyyy-MM-dd");

                return View(model);
            }
            catch(Exception Ex)
            {
                objMas.PrintLog("C[2,13]", Ex.Message);
                return Content("C[13]: " + Ex.Message);
            }
        }



        [HttpPost]
        [Route("action")]
        [Route("Service/GetShipmentOrTrack")]
        public ActionResult GetShipmentOrTrack(MissingCrateModel data)
        {
            string MthdName = "GetShipmentOrTrack";
            try
            {
                DataTable dt = new DataTable();

                if (data.ProccessType == 0)
                    return JsonRspMsg(0, MthdName, 17, "Please Select Valid Process Type", null);
                else if (data.ProccessType == 1)
                {
                    dt = objMas.GetDataTable("select a.TrackingId as Value,a.TrackingId as Text from crate_transfer_header a, crate_transfer_lines b where a.TrackingId = b.TrackingId and " +
                                        " b.ReceivedFlag = 0 group by a.TrackingId ");
                }
                else
                {
                    dt = objMas.GetDataTable("select a.ShipmentNo as Value,a.ShipmentNo as Text from crate_return_header a, crate_return_lines b where a.ShipmentNo = b.ShipmentNo " +
                                        "and b.ReceivedFlag = 0 group by a.ShipmentNo");
                }

                if (dt == null)
                    return JsonRspMsg(0, MthdName, 16, "Failed To Fetch Order Details", null);
                else if (dt.Rows.Count == 0)
                    return JsonRspMsg(0, MthdName, 17, "No Data Found", null);
                
                var BatchDetails = objMas.ConvertToList<SelectListItem>(dt);
                
                if(BatchDetails == null)
                    return JsonRspMsg(0, MthdName, 17, "No Data Found", null);

                return Json(new {Status = 1,Message = "Success",Data = BatchDetails }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,13]", Ex.Message);
                return Content("C[13]: " + Ex.Message);
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Service/GetMissingCrates")]
        public ActionResult GetMissingCrates(MissingCrateModel data)
        {
            string MthdName = "GetMissingCrates";
            try
            {
                DataTable dt = new DataTable();
                //List<MissingCrate_Grid> Crates = new List<MissingCrate_Grid>();

                if (data.ProccessType == 1)
                    dt = objMas.GetDataTable("select CrateCode from crate_master cm, crate_transfer_header cth, crate_transfer_lines ctl where cm.CrateCode = ctl.CrateBarcode and " +
                        "ctl.ReceivedFlag = 0 and cth.TrackingId = ctl.TrackingId and ctl.TrackingId = '" + data.Shipment + "'  group by CrateCode");
                else if (data.ProccessType == 2)
                    dt = objMas.GetDataTable("select CrateCode from crate_master cm, crate_return_header crh, crate_return_lines crl where cm.CrateCode = crl.CrateBarcode and " +
                        "crl.ReceivedFlag = 0 and crh.ShipmentNo = crl.ShipmentNo and crl.ShipmentNo = '" + data.Shipment + "' group by CrateCode; ");

                if (dt == null)
                    return JsonRspMsg(0, MthdName, 17, "Data Fetch Failed", null);
                
                var Crates = objMas.ConvertToList<MissingCrate_Grid>(dt);
                
                if (Crates.Count == 0)
                    return JsonRspMsg(0, MthdName, 17, "No Data Found", null);
                
                return Json(Crates, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,13]", Ex.Message);
                return Content("C[13]: " + Ex.Message);
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Service/UpdateMissingCrates")]
        public ActionResult UpdateMissingCrates(string ShipId, string ProcType, List<MissingCrate_Save> data)
        {
            string MthdName = "UpdateMissingCrates";
            try
            {
                //ProcType --> 1 => Crate STO, 2 => Crate Return.
                //UpdateVAl --> "3" => Missing, "4" => Revoke, "5" => Received.

                Int64 Ret = 0;
                if(ProcType == "" || ProcType.Length == 0)
                    return JsonRspMsg(0, MthdName, 17, "Please select Valid Process Type", null);

                else if (ShipId == "" || ShipId.Length == 0)
                    return JsonRspMsg(0, MthdName, 17, ProcType == "2" ? "Invalid Shipment Number" : ProcType == "1" ? "Invalid Tracking id" : "Invalid Inputs", null);
                
                foreach(var item in data)
                {
                    if(ProcType == "1")
                    {
                        Ret = objMas.ExecQry("update crate_transfer_lines set ReceivedFlag = '" + item.UpdateVal + "',ReceivedBy='" +
                                    Request.Cookies["UserId"].Value.ToString() + "',ReceivedOn = now() where CrateBarcode='" + item.CrateCode + "' and TrackingId='" + ShipId + "'");
                        if(Ret == -1)
                            return JsonRspMsg(0, MthdName, 17, "DB Error. Contact System Admin", null);
                        else if(Ret == 0 || Ret > 1)
                            return JsonRspMsg(0, MthdName, 17, "Failed. Contact System Admin", null);
                        
                        if(item.UpdateVal == "3")
                        {
                            Ret = objMas.ExecQry("update crate_master set Status = 0, Occupied = 0, CurrentStatus = 'Crate Missing', ModifiedBy = '"+ Request.Cookies["UserId"].Value.ToString() +
                                 "', ModifiedOn = now() where CrateCode = '" + item.CrateCode + "'");
                            
                            if (Ret == -1)
                                return JsonRspMsg(0, MthdName, 17, "DB Error. Contact System Admin", null);
                            else if (Ret == 0 || Ret > 1)
                                return JsonRspMsg(0, MthdName, 17, "Failed. Contact System Admin", null);
                        }
                        else if(item.UpdateVal == "4")
                        {
                            Ret = objMas.ExecQry("update crate_transfer_lines a, crate_transfer_header b set a.Receiving_Plant = b.Sending_Plant where a.TrackingId = b.TrackingId and " +
                                "b.TrackingId = '" + ShipId + "' and a.CrateBarcode = '" + item.CrateCode + "'");
                            
                            if (Ret == -1)
                                return JsonRspMsg(0, MthdName, 17, "DB Error. Contact System Admin", null);
                            else if (Ret == 0 || Ret > 1)
                                return JsonRspMsg(0, MthdName, 17, "Failed. Contact System Admin", null);
                        }
                        else if (item.UpdateVal == "5")
                        {
                            Ret = objMas.ExecQry("update crate_transfer_lines a, crate_transfer_header b set a.Receiving_Plant = b.Receiving_Plant where a.TrackingId = b.TrackingId and " +
                                "b.TrackingId = '" + ShipId + "' and a.CrateBarcode = '" + item.CrateCode + "'");

                            if (Ret == -1)
                                return JsonRspMsg(0, MthdName, 17, "DB Error. Contact System Admin", null);
                            else if (Ret == 0 || Ret > 1)
                                return JsonRspMsg(0, MthdName, 17, "Failed. Contact System Admin", null);
                        }
                    }
                    else if(ProcType == "2")
                    {
                        Ret = objMas.ExecQry("update crate_return_lines set ReceivedFlag='" + item.UpdateVal + "',ReceivedBy='" + Request.Cookies["UserId"].Value.ToString() +
                                "',ReceivedOn = now() where CrateBarcode = '" + item.CrateCode + "' and ShipmentNo='" + ShipId + "' ");
                        if (Ret == -1)
                            return JsonRspMsg(0, MthdName, 17, "DB Error. Contact System Admin", null);
                        else if (Ret == 0 || Ret > 1)
                            return JsonRspMsg(0, MthdName, 17, "Failed. Contact System Admin", null);

                        if (item.UpdateVal == "3")
                        {
                            Ret = objMas.ExecQry("update crate_master set Status = 0, Occupied = 0, CurrentStatus = 'Crate Missing', ModifiedBy = '" + Request.Cookies["UserId"].Value.ToString() +
                                 "', ModifiedOn = now() where CrateCode = '" + item.CrateCode + "'");

                            if (Ret == -1)
                                return JsonRspMsg(0, MthdName, 17, "DB Error. Contact System Admin", null);
                            else if (Ret == 0 || Ret > 1)
                                return JsonRspMsg(0, MthdName, 17, "Failed. Contact System Admin", null);
                        }
                        else if(item.UpdateVal == "4")
                        {
                            Ret = objMas.ExecQry("update crate_return_lines a, crate_return_header b set a.Receiving_Plant = b.Sales_Plant where a.ShipmentNo = b.ShipmentNo and " +
                                "b.ShipmentNo = '" + ShipId + "' and a.CrateBarcode = '" + item.CrateCode + "'");

                            if (Ret == -1)
                                return JsonRspMsg(0, MthdName, 17, "DB Error. Contact System Admin", null);
                            else if (Ret == 0 || Ret > 1)
                                return JsonRspMsg(0, MthdName, 17, "Failed. Contact System Admin", null);
                        }
                        else if (item.UpdateVal == "5")
                        {
                            Ret = objMas.ExecQry("update crate_return_lines a, crate_return_header b set a.Receiving_Plant = b.Receiving_Plant where a.ShipmentNo = b.ShipmentNo and " +
                                "b.ShipmentNo = '" + ShipId + "' and a.CrateBarcode = '" + item.CrateCode + "'");

                            if (Ret == -1)
                                return JsonRspMsg(0, MthdName, 17, "DB Error. Contact System Admin", null);
                            else if (Ret == 0 || Ret > 1)
                                return JsonRspMsg(0, MthdName, 17, "Failed. Contact System Admin", null);
                        }
                    }
                }
                if (Ret <= 0)
                    return JsonRspMsg(0, MthdName, 17, "Failed. Contact System Admin", null);
                else
                    return JsonRspMsg(1, MthdName, 17, "Sucess", null);

            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,13]", Ex.Message);
                return Content("C[13]: " + Ex.Message);
            }
        }
    }
}