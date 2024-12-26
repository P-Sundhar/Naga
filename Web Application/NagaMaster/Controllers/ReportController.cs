using iTextSharp.text.pdf;
using NagaMaster.Models;
using NagaMaster.Models.Reports;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NagaMaster.Controllers
{
    public class ReportController : Controller
    {
        MasterLogic objMas = new MasterLogic();
        AppDet objApp = new AppDet();
        LoginController objMas1 = new LoginController();

        // GET: Report
        public ActionResult Index()
        {
            return View();
        }



        public ActionResult JsonRspMsg(int IsSuccess, string Mthd, int ErrNo, string Msg, Object RspData)
        {
            try
            {
                if (IsSuccess == 1)
                    return Json(new ResponseHelper { Success = 1, Message = Msg, Data = RspData });
                if (IsSuccess == 2)
                    return Json(new ResponseHelper { Success = 2, Message = Msg, Data = RspData });

                objMas.PrintLog(PAGEDESC, "A[2," + ErrNo + "]: " + Msg + ": " + objMas.DBErrBuf);
                return Json(new ResponseHelper { Success = 0, Message = "A[2," + ErrNo + "]: " + Msg, Data = null });

            }
            catch (Exception ex)
            {
                objMas.PrintLog(PAGEDESC, "C[2,1]: " + Mthd + ": " + ex.Message);
                return Json(new ResponseHelper { Success = 0, Message = "C[1,1]: Exception in " + Mthd, Data = null });
            }
        }

        const string PAGEDESC = "ReportController";




        public ActionResult StockSmartReport()
        {
            DataTable dt = new DataTable();

            var model = new PeriodWiseModel();
            dt = objMas.GetDataTable("Select PlantId,PlantName from Plant_master where Status in (1,2) Order by PlantName");
            if (dt == null)
            {
                objMas.PrintLog("A[2,1]", objMas.DBErrBuf);
                return View(model);
            }
            model.PlantDetails = (from DataRow a in dt.Rows
                                  select new SelectListItem { Text = a["PlantName"].ToString(), Value = a["PlantId"].ToString() }).ToList();


            dt = objMas.GetDataTable("Select ProdGroupCode,ProdGroupCode from material_master where Status in (1,2) group by ProdGroupCode Order by ProdGroupCode");
            if (dt == null)
            {
                objMas.PrintLog("A[2,1]", objMas.DBErrBuf);
                return View(model);
            }
            model.MaterialDetails = (from DataRow a in dt.Rows
                                  select new SelectListItem { Text = a["ProdGroupCode"].ToString(), Value = a["ProdGroupCode"].ToString() }).ToList();


            model.FromDate = model.ToDate = System.DateTime.Now.ToString("yyyy-MM-dd");


            dt = objApp.LoadDwnldFormats(objMas);
            if (dt == null)
            {
                objMas.PrintLog("A[2,4]", objMas.DBErrBuf);
                return View(model);
            }

            
            model.DwnldFmtDetail = (from DataRow a in dt.Rows
                                    select new SelectListItem { Text = a["DwnldDesc"].ToString(), Value = Convert.ToInt16(a["DwnldId"]).ToString() }).ToList();
            dt = objApp.LoadStockStatus(objMas);
            if (dt == null)
            {
                objMas.PrintLog("A[2,4]", objMas.DBErrBuf);
                return View(model);
            }

            model.StockStatusDetails = (from DataRow a in dt.Rows
                                    select new SelectListItem { Text = a["TypeName"].ToString(), Value = Convert.ToInt16(a["TypedId"]).ToString() }).ToList();

            ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.Plant_Wise_Report, Request.Cookies["UserId"].Value.ToString());
            model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());
            return View(model);
        }


        [HttpPost]
        [Route("action")]
        [Route("Report/GetMaterialForStockReport")]
        public ActionResult GetMaterialForStockReport(PeriodWiseModel rjtmodel)
        {
            try
            {
                string MthdName = "GetMaterialForStockReport";
                var Batches = objMas.GetDataTable("SELECT MaterialCode, MaterialDesc FROM material_master  " + (rjtmodel.MaterialCategory != null ? " where ProdGroupCode ='" + 
                                rjtmodel.MaterialCategory + "'" : "") + "order by MaterialCode");

                if (Batches == null)
                {
                    objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                    return JsonRspMsg(0, MthdName, 31, objMas.DBErrBuf, null);
                }
                else if (Batches.Rows.Count == 0)
                {
                    objMas.PrintLog("A[2,29]", objMas.DBErrBuf);
                    return JsonRspMsg(2, MthdName, 30, "Batch Number Not Found For This Selection", null);
                }

                 

                var BatchDetails = (from DataRow a in Batches.Rows
                                    select new SelectListItem { Text = a["MaterialDesc"].ToString(), Value = a["MaterialCode"].ToString() }).ToList();

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
        [Route("Report/GetBatchForStockReport")]
        public ActionResult GetBatchForStockReport(PeriodWiseModel rjtmodel)
        {
            try
            {
                string MthdName = "GetBatch";
                var Batches = objMas.GetDataTable("SELECT a.MaterialCode, a.BatchNo,b.MaterialDesc FROM production_lines a, " +
                    "material_master b where a.MaterialCode=b.MaterialCode " + (rjtmodel.MaterialCode != null ? " AND " +
                    "a.MaterialCode='" + rjtmodel.MaterialCode + "'" : "") + " AND a.PickFlag = 0 AND a.Reject = 0  " +
                    "UNION " +
                    "SELECT c.MaterialCode, c.BatchNo,d.MaterialDesc FROM cfagrn_details c, material_master d " +
                    "where d.MaterialCode=c.MaterialCode " + (rjtmodel.MaterialCode != null ? " AND " +
                    "c.MaterialCode='" + rjtmodel.MaterialCode + "'" : "") +" AND c.PickFlag = 0 AND c.Reject = 0 " +
                    "group by BatchNo order by MaterialCode");


                    if (Batches.Rows.Count == 0)
                    {
                        objMas.PrintLog("A[2,28]", objMas.DBErrBuf);
                        return JsonRspMsg(2, MthdName, 30, "Batch Number Not Found For This Selection", null);
                    }

                    else if (Batches == null)
                    {
                        objMas.PrintLog("A[2,29]", objMas.DBErrBuf);
                        return JsonRspMsg(0, MthdName, 31, objMas.DBErrBuf, null);
                    }

                    var BatchDetails = (from DataRow a in Batches.Rows
                                        select new SelectListItem { Text = a["BatchNo"].ToString(), Value = a["MaterialDesc"].ToString() }).ToList();

                    return Json(new { Success = 1, Message = "Sucess", Data = BatchDetails });

            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,12]", Ex.Message);
                return Content("C[1]: " + Ex.Message);
            }
        }


        private List<StockSmartModel> StockSmartReport_GetQry(int IsDwnld, PeriodWiseModel data)
        {
            try
            {
                DataTable dt = new DataTable();
                string Qry = "";

                Qry = "SELECT ph.PlantCode as Plant, a.MaterialCode, b.MaterialDesc, a.BatchNo, DATE_FORMAT(a.TranDate, '%d-%m-%Y') AS 'ManufacturingDate', " +
                        " DATE_FORMAT(ph.ExpDate,'%d-%m-%Y') AS 'ExpiryDate', DATEDIFF(DATE_FORMAT(ph.ExpDate,'%Y-%m-%d'), CURDATE()) AS 'RemainingDays'," +
                        " DATEDIFF(CURDATE(), DATE_FORMAT(ph.PDNDate, '%Y-%m-%d')) AS 'AgingDays',COUNT(a.Barcode) AS 'StockQtyInBox', (COUNT(a.Barcode) * b.Denominator) AS 'StockQtyInEach', " +
                        " SUM(a.Reject='1') AS 'Restricted', (COUNT(a.Barcode) - SUM(a.Reject='1')) AS 'UnRestricted',a.Barcode as Secondary,a.CrateBarcode as Crate FROM production_lines a " +
                        " JOIN material_master b ON a.MaterialCode=b.MaterialCode JOIN production_header ph ON a.PDNId=ph.PDNId WHERE Date(a.TranDate)>='" + data.FromDate + "' AND " +
                        " Date(a.TranDate)<='" + data.ToDate + "' " + (data.StockStatus != null ? " and a.Reject='" + data.StockStatus + "'" : "") + (data.MaterialCode != null ? " and " +
                        " a.MaterialCode='" + data.MaterialCode + "'" : "") + (data.BatchNo != null ? " and a.BatchNo='" + data.BatchNo + "'" : "") + (data.PlantId != null ? " and " +
                        " ph.PlantCode='" + data.PlantId + "'" : "") + " and a.IsStock = '1' and a.PickFlag = '0' GROUP BY ph.PlantCode, a.MaterialCode, a.BatchNo,a.Barcode " +
                        " UNION " +
                        " SELECT c.ReceivingPlant AS Plant, c.MaterialCode, d.MaterialDesc, c.BatchNo, DATE_FORMAT(c.PDNDate,'%d-%m-%Y') AS 'ManufacturingDate'," +
                        " DATE_FORMAT(phs.ExpDate, '%d-%m-%Y') AS 'ExpiryDate', DATEDIFF(DATE_FORMAT(phs.ExpDate, '%Y-%m-%d'), CURDATE()) AS 'RemainingDays', DATEDIFF(CURDATE(), " +
                        " DATE_FORMAT(phs.PDNDate, '%Y-%m-%d')) AS 'AgingDays', COUNT(c.Barcode) AS 'StockQtyInBox', (COUNT(c.Barcode) * d.Denominator) AS 'StockQtyInEach', " +
                        " SUM(c.Reject ='1') AS 'Restricted', (count(c.Barcode) - SUM(c.Reject ='1')) AS 'UnRestricted', c.Barcode as Secondary,c.CrateBarcode as Crate FROM " +
                        " cfagrn_details c JOIN material_master d ON c.MaterialCode=d.MaterialCode JOIN production_header phs ON c.BatchNo=phs.BatchNo WHERE " +
                        " Date(c.PDNDate)>='" + data.FromDate + "' AND Date(c.PDNDate) <= '" + data.ToDate + "' " + (data.StockStatus != null ? " and " +
                        " c.Reject='" + data.StockStatus + "'" : "") + (data.MaterialCode != null ? " and c.MaterialCode='" + data.MaterialCode + "'" : "") + (data.BatchNo != null ? " " +
                        " and c.BatchNo='" + data.BatchNo + "'" : "") + (data.PlantId != null ? " and c.ReceivingPlant='" + data.PlantId + "'" : "") + " and c.PickFlag = '0' " +
                        " GROUP BY c.ReceivingPlant, c.MaterialCode, c.BatchNo,c.Barcode " +
                        " UNION " +
                        " SELECT cd.StockTransferPlant AS Plant, cd.MaterialCode, mm.MaterialDesc, cd.BatchNo, DATE_FORMAT(cd.PDNDate,'%d-%m-%Y') AS 'ManufacturingDate', " +
                        " DATE_FORMAT(phr.ExpDate, '%d-%m-%Y') AS 'ExpiryDate', DATEDIFF(DATE_FORMAT(phr.ExpDate, '%Y-%m-%d'), CURDATE()) AS 'RemainingDays', " +
                        " DATEDIFF(CURDATE(), DATE_FORMAT(phr.PDNDate, '%Y-%m-%d')) AS 'AgingDays', COUNT(cd.Barcode) AS 'StockQtyInBox', " +
                        " (COUNT(cd.Barcode) * mm.Denominator) AS 'StockQtyInEach',  SUM(cd.Reject = '1') AS 'Restricted', " +
                        " (count(cd.Barcode) - SUM(cd.Reject = '1')) AS 'UnRestricted', cd.Barcode as Secondary,cd.CrateBarcode as Crate FROM cfagrn_details cd " +
                        " JOIN material_master mm ON cd.MaterialCode = mm.MaterialCode JOIN production_header phr ON cd.BatchNo = phr.BatchNo WHERE " +
                        " Date(cd.PDNDate)>='" + data.FromDate + "' AND Date(cd.PDNDate) <= '" + data.ToDate + "' " + (data.StockStatus != null ? " and " +
                        " cd.Reject='" + data.StockStatus + "'" : "") + (data.MaterialCode != null ? " and cd.MaterialCode='" + data.MaterialCode + "'" : "") + (data.BatchNo != null ? " " +
                        " and cd.BatchNo='" + data.BatchNo + "'" : "") + (data.PlantId != null ? " and cd.ReceivingPlant='" + data.PlantId + "'" : "") + " and cd.PickFlag = '0' and " +
                        " cd.CfaPickFlag = '1'  GROUP BY cd.StockTransferPlant, cd.MaterialCode, cd.BatchNo,cd.Barcode ORDER BY Plant,MaterialCode, BatchNo";

                dt = objMas.GetDataTable(Qry);

                if (data.BatchType == 1)
                {
                    dt.Columns.Remove("Secondary");
                    dt.Columns.Remove("Crate");

                    var result = (from r in dt.AsEnumerable()
                                  group r by new
                                  {
                                      PlantCode = r["Plant"],
                                      MaterialCode = r["MaterialCode"],
                                      MaterialDesc = r["MaterialDesc"],
                                      BatchNo = r["BatchNo"],
                                      ManufacturingDate = r["ManufacturingDate"],
                                      ExpiryDate = r["ExpiryDate"],
                                      AgingDays = r["AgingDays"],
                                      RemainingDays = r["RemainingDays"],
                                  } into g
                                  select new
                                  {
                                      Plant = g.Key.PlantCode.ToString(),
                                      MaterialCode = g.Key.MaterialCode.ToString(),
                                      MaterialDesc = g.Key.MaterialDesc.ToString(),
                                      BatchNo = g.Key.BatchNo.ToString(),
                                      ManufacturingDate = g.Key.ManufacturingDate.ToString(),
                                      ExpiryDate = g.Key.ExpiryDate.ToString(),
                                      AgingDays = Convert.ToInt64(g.Key.AgingDays),
                                      RemainingDays = Convert.ToInt64(g.Key.RemainingDays),
                                      StockQtyInBox = g.Sum(x => Convert.ToInt64(x["StockQtyInBox"])),
                                      StockQtyInEach = g.Sum(x => Convert.ToInt64(x["StockQtyInEach"])),
                                      Restricted = g.Sum(x => Convert.ToDecimal(x["Restricted"])),
                                      UnRestricted = g.Sum(x => Convert.ToDecimal(x["UnRestricted"]))
                                  }).Select(x => new StockSmartModel
                                  {
                                      Plant = x.Plant,
                                      MaterialCode = x.MaterialCode,
                                      MaterialDesc = x.MaterialDesc,
                                      BatchNo=x.BatchNo,
                                      ManufacturingDate = x.ManufacturingDate,
                                      ExpiryDate=x.ExpiryDate,
                                      RemainingDays = x.RemainingDays,
                                      AgingDays = x.AgingDays,
                                      StockQtyInBox = x.StockQtyInBox,
                                      StockQtyInEach =x.StockQtyInEach,
                                      Restricted =x.Restricted,
                                      UnRestricted =x.UnRestricted
                                  }).ToList();

                    return result;

                }


                return objMas.ConvertToList<StockSmartModel>(dt);
            }
            catch (Exception ex)
            {
                objMas.PrintLog("C[2,1]", ex.Message);
                return null;
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Report/StockSmartReport_LoadGrid")]
        public ActionResult StockSmartReport_LoadGrid(PeriodWiseModel data)
        {
            try
            {

                var list = StockSmartReport_GetQry(0, data);
                if (list == null)
                {
                    objMas.PrintLog("A[2,8]", objMas.DBErrBuf);
                    return Content("Report Data Fetch Failed");
                }
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,3]: ", Ex.Message);
                return null;
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("Report/StockSmartReport_GenRpt")]
        public ActionResult StockSmartReport_GenRpt(PeriodWiseModel data)
        {
            try
            {
                DataColumn column = new DataColumn("SNo", typeof(int));
                column.DataType = System.Type.GetType("System.Int32");
                column.AutoIncrement = true;
                column.AutoIncrementSeed = 1;
                column.AutoIncrementStep = 1;
                int ret = 0;
                DataTable dt = new DataTable();


                DataTable dtHeader = new DataTable();
                string[] AddlnData = new string[6];
                int[] AddlnHdrCols = new int[12];
                float[] ColWidth;

                var lst = StockSmartReport_GetQry(1, data);
                dt = objMas.ToDataTable(lst);
                dt.Columns["SNo"].SetOrdinal(0);

                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["SNo"] = i + 1;
                if (dt == null)
                {
                    objMas.PrintLog("A[2,5]", objMas.DBErrBuf);
                    return Json(new { Success = 0, Message = "Report Data Fetch Failed", Data = "" });
                }
                if (data.BatchType == 1)
                {
                    dt.Columns.Remove("Secondary");
                    dt.Columns.Remove("Crate");
                    ColWidth = new float[13] { 1.5f, 2f, 4f, 8f, 4f, 4f, 4f, 2f, 2f, 2f, 2f, 2f, 2f };
                }
                else
                {
                    dt.Columns.Remove("StockQtyInBox");
                    dt.Columns.Remove("StockQtyInEach");
                    dt.Columns.Remove("Restricted");
                    dt.Columns.Remove("UnRestricted");
                    ColWidth = new float[11] { 1.5f, 2f, 4f, 8f, 4f, 3f, 6f, 4f, 4f, 2f, 2f};
                }

                PdfPTable pdfTblHdl = new PdfPTable(dt.Columns.Count);
                int[] DType = Enumerable.Range(0, dt.Columns.Count).Select(n => MasterLogic.RPTSTYLE_STRING_SHORT).ToArray();
                DType[0] = MasterLogic.RPTSTYLE_INT;
                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                ret = objMas.GenerateReport(Convert.ToInt16(data.DwnldFmt), 1, 1, 0, "RptStock_", "NAGA - Stock Report", DType, null, null, data.FromDate, data.ToDate,AddlnData, null, null,null, dt,ColWidth);

                if (ret == 1)
                {
                    objMas.PrintLog("A[2,6]", objMas.DBErrBuf);
                    return Json(new { Success = 1, Message = "Success", Data = objMas.DBErrBuf });
                }

                objMas.PrintLog("A[2,7]", objMas.DBErrBuf);
                return Json(new { Success = 0, Message = "Failed", Data = objMas.DBErrBuf });
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,2]: ", Ex.Message);
                return null;
            }
        }




        // ***************************** PeriodWise Report *********

        public ActionResult RptPeriodWise_FillScreen()
        {
            DataTable dt = new DataTable();

            var model = new PeriodWiseModel();
            dt = objMas.GetDataTable("Select PlantId,PlantName from Plant_master where Status in (1,2) Order by PlantName");
            if (dt == null)
            {
                objMas.PrintLog("A[2,1]",objMas.DBErrBuf);
                return View(model);
            }
                

            model.PlantDetails = (from DataRow a in dt.Rows
                                  select new SelectListItem { Text = a["PlantName"].ToString(), Value = a["PlantId"].ToString() }).ToList();

            dt = objMas.GetDataTable("Select UnitId,UnitName from Unit_master where Status in (1,2) Order by UnitName");
            if (dt == null)
            {
                objMas.PrintLog("A[2,2]", objMas.DBErrBuf);
                return View(model);
            }
                

            model.UnitDetails = (from DataRow a in dt.Rows
                                 select new SelectListItem { Text = a["UnitName"].ToString(), Value = a["UnitId"].ToString() }).ToList();

            dt = objMas.GetDataTable("Select SystemId,SystemName from system_master where Status in (1,2) Order by SystemName");
            if (dt == null)
            {
                objMas.PrintLog("A[2,3]", objMas.DBErrBuf);
                return View(model);
            }

            //model.SystemDetails = (from DataRow a in dt.Rows
            //                     select new SelectListItem { Text = a["SystemName"].ToString(), Value = a["SystemId"].ToString() }).ToList();

            model.FromDate = model.ToDate =  System.DateTime.Now.ToString("yyyy-MM-dd");

            dt = objApp.LoadDwnldFormats(objMas);
            if (dt == null)
            {
                objMas.PrintLog("A[2,4]", objMas.DBErrBuf);
                return View(model);
            }

            model.DwnldFmtDetail = (from DataRow a in dt.Rows
                              select new SelectListItem { Text = a["DwnldDesc"].ToString(), Value = Convert.ToInt16(a["DwnldId"]).ToString()}).ToList();

            ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.Plant_Wise_Report, Request.Cookies["UserId"].Value.ToString());
            model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());
            return View(model);

        }


        private DataTable RptPeriodWise_GetQry(int IsDwnld, PeriodWiseModel data)
        {
            try
            {
                DataTable dt = new DataTable();
                string Qry="";

                Qry = " select date_format(PDNDate,'%d-%m-%Y') as 'Date', ph.PDNId as 'JobOrder#',   mm.MaterialDesc as 'Material Name', pm.PlantName as Plant, " +
                    " um.UnitName as Unit, sm.SystemName as 'System',ph.PrimaryQty,ph.TargetQty, Count(Distinct pl.Barcode) as 'Production Quantity'," +
                    " pb.ReworkCnt as Reprint,ph.ExcessCnt as Excess,ph.QCCnt as 'QC Count', ph.DamageCnt as 'Damage Count',ph.ChefCnt as 'Chef Sample',ph.RnDCnt as 'RnD Count',ph.ProdDocNo as 'Prod Doc No' from production_header ph," +
                    " production_lines pl, plant_master pm, unit_master um,system_master sm, " +
                    " primarybarcode pb, material_master mm where ph.PDNId=pl.PDNId and ph.PDNId=pb.PDNId and ph.MaterialCode=  pl.MaterialCode and ph.System=ph.System " +
                    " and Date(ph.PDNDate)>='" + data.FromDate + "' and Date(ph.PDNDate)<= '" + data.ToDate + "' and  ph.PlantCode=pm.PlantId and ph.MaterialCode=mm.MaterialCode and ph.UnitId=um.UnitId " +
                    " and ph.System=sm.SystemId " + (data.PlantId != null ? " and ph.PlantCode='" + data.PlantId + "'" : "") + (data.UnitId != null ? " and ph.UnitId=" + data.UnitId : "") +
                      (data.SystemId != null ? " and ph.System=" + data.SystemId : "") + " group by ph.PDNDate,ph.PlantCode,mm.MaterialDesc,pm.PlantName,um.UnitName,sm.SystemName," +
                    "ph.System,ph.PrimaryQty,ph.TargetQty,ph.PDNID,pb.ReworkCnt,ph.ExcessCnt,ph.QCCnt, ph.DamageCnt,ph.ChefCnt,ph.RnDCnt,ph.ProdDocNo order by PDNDate";
                dt = objMas.GetDataTable(Qry);
                return dt;
            }
            catch (Exception ex)
            {
                objMas.PrintLog("C[2,1]", ex.Message);
                return null;
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("Report/RptPeriodWise_GenRpt")]
        public ActionResult RptPeriodWise_GenRpt(PeriodWiseModel data)
        {
            try
            {
                int ret = 0;
                DataTable dt = new DataTable();

                DataColumn column = new DataColumn("SlNo", typeof(int));
                column.DataType = System.Type.GetType("System.Int32");
                column.AutoIncrement = true;
                column.AutoIncrementSeed = 1;
                column.AutoIncrementStep = 1;


                DataTable dtHeader = new DataTable();
                string[] AddlnData = new string[6];
                int[] AddlnHdrCols = new int[12];
                float[] ColWidth;

                dt = RptPeriodWise_GetQry(1, data);

                DataColumn Col = dt.Columns.Add("SlNo", typeof(int));
                Col.SetOrdinal(0);// to put the column in position 0;

                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["SlNo"] = i + 1;
                if (dt == null)
                {
                    objMas.PrintLog("A[2,5]", objMas.DBErrBuf);
                    return Json(new { Success = 0, Message = "Report Data Fetch Failed", Data = "" });
                }
                    dt.Columns.Remove("Prod Doc No");
                ColWidth = new float[16] { 1f, 4f, 5f, 6f, 2f, 2f, 3f, 3f, 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f };

                PdfPTable pdfTblHdl = new PdfPTable(dt.Columns.Count);
                int[] DType = Enumerable.Range(0, dt.Columns.Count).Select(n => MasterLogic.RPTSTYLE_STRING_SHORT).ToArray();
                DType[0] = MasterLogic.RPTSTYLE_INT;
                ExcelPackage.LicenseContext = LicenseContext.Commercial;


                
                ret = objMas.GenerateReport(data.DwnldFmt, 1, 0, 0, "RptPeriod_", "NAGA - Production Report", DType, null, null, data.FromDate, data.ToDate, AddlnData, null,null, null, dt, ColWidth);
                if (ret == 1)
                {
                    objMas.PrintLog("A[2,6]", objMas.DBErrBuf);
                    return Json(new { Success = 1, Message = "Success", Data = objMas.DBErrBuf });
                }

                objMas.PrintLog("A[2,7]", objMas.DBErrBuf);
                return Json(new { Success = 0, Message = "Failed", Data = objMas.DBErrBuf });
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,2]: " , Ex.Message);
                return null;
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Report/RptPeriodWise_LoadGrid")]
        public ActionResult RptPeriodWise_LoadGrid(PeriodWiseModel data)
        {
            try
            {
                DataTable dt = new DataTable();
                
                dt = RptPeriodWise_GetQry(0, data);
                if (dt == null)
                {
                    objMas.PrintLog("A[2,8]", objMas.DBErrBuf);
                    return Content("Report Data Fetch Failed");
                }
                    

                var emp = (from DataRow row in dt.Rows
                           select new
                           {
                               DispDate = row.Field<string>("Date").ToString(),
                               Plant = row.Field<string>("Plant").ToString(),
                               Unit = row.Field<string>("Unit").ToString(),
                               System = row.Field<string>("System").ToString(),
                               PDNId = Convert.ToDecimal(row.Field<decimal>("JobOrder#")).ToString(),
                               MaterialDesc = row.Field<string>("Material Name").ToString(),
                               TargetQty = row.Field<string>("TargetQty").ToString(),
                               PrimaryQty = row.Field<string>("PrimaryQty").ToString(),
                               Quantity = Convert.ToInt64(row.Field<Int64>("Production Quantity")).ToString(),
                               ReworkCnt = Convert.ToInt32(row.Field<int>("Reprint")).ToString(),
                               ExcessCnt = Convert.ToInt64(row.Field<int>("Excess")).ToString(),
                               QCCnt = Convert.ToInt64(row.Field<int>("QC Count")).ToString(),
                               DamageCnt = Convert.ToInt64(row.Field<int>("Damage Count")).ToString(),
                               ChefCnt = Convert.ToInt64(row.Field<int>("Chef Sample")).ToString(),
                               RnDCount = Convert.ToInt32(row.Field<int>("RnD Count")).ToString(),
                               ProdDoc = row.Field<string>("Prod Doc No").ToString(),
                           }).ToList();
                
                return Json(emp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,3]: " , Ex.Message);
                return null;
            }
        }

        public ActionResult RptPutaway_FillScreen()
        {
            DataTable dt = new DataTable();

            var model = new PutawayModel();
            dt = objMas.GetDataTable("Select PlantId,PlantName from Plant_master where Status in (1,2) Order by PlantName");
            if (dt == null)
            {
                objMas.PrintLog("[A[2,9]", objMas.DBErrBuf);
                return View(model);
            }
                

            model.PlantDetails = (from DataRow a in dt.Rows
                                  select new SelectListItem { Text = a["PlantName"].ToString(), Value = a["PlantId"].ToString() }).ToList();

            dt = objMas.GetDataTable("Select UnitId,UnitName from Unit_master where Status in (1,2) Order by UnitName");
            if (dt == null)
            {
                objMas.PrintLog("[A[2,10]", objMas.DBErrBuf);
                return View(model);
            }

            model.UnitDetails = (from DataRow a in dt.Rows
                                 select new SelectListItem { Text = a["UnitName"].ToString(), Value = a["UnitId"].ToString() }).ToList();

            dt = objMas.GetDataTable("Select SystemId,SystemName from system_master where Status in (1,2) Order by SystemName");
            if (dt == null)
            {
                objMas.PrintLog("[A[2,11]", objMas.DBErrBuf);
                return View(model);
            }

            model.SystemDetails = (from DataRow a in dt.Rows
                                   select new SelectListItem { Text = a["SystemName"].ToString(), Value = a["SystemId"].ToString() }).ToList();

            model.FromDate = model.ToDate = System.DateTime.Now.ToString("yyyy-MM-dd");

            dt = objApp.LoadDwnldFormats(objMas);
            if (dt == null)
            {
                objMas.PrintLog("[A[2,12]", objMas.DBErrBuf);
                return View(model);
            }
            model.DwnldFmtDetails = (from DataRow a in dt.Rows
                              select new SelectListItem { Text = a["DwnldDesc"].ToString(), Value = a["DwnldId"].ToString() }).ToList();

            ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.PutAwayReport, Request.Cookies["UserId"].Value.ToString());
            model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());
            return View(model);

        }


        private DataTable RptPutaway_GetQry(int IsDwnld, PutawayModel data)
        {
            try
            {
                //int ret = 0;
                DataTable dt = new DataTable();
                string Qry = "";

                Qry = " select date_format(pl.PutawayDate,'%m-%d-%Y') as 'Putaway Date',pl.BatchNo as 'Batch Number', pl.MaterialCode as 'Material Code',mm.MaterialDesc as 'Material Name',pl.RackNo as 'Rack Number', " +//
                        " Count(distinct pl.CrateBarcode)as 'Crate Count',Count(distinct pl.Barcode) as 'Box Count' from production_lines pl, plant_master pm, rack_master rm, material_master mm,production_header ph " +
                        "where Date(pl.PutawayDate)>='" + data.FromDate + "' and Date(pl.PutawayDate)<='" + data.ToDate + "'" +     // and pl.RackNo=rm.RackNumber  "+()+"
                        " and ph.PlantCode=pm.PlantId and ph.PDNId=pl.PDNId and pl.MaterialCode=mm.MaterialCode and pl.Status=1";

                if (data.PlantId != null)
                    Qry += " and ph.PlantCode='" + data.PlantId + "'";
                if (data.UnitId != null)
                    Qry += " and ph.UnitId=" + data.UnitId;
                if (data.SystemId != null)
                    Qry += " and ph.System=" + data.SystemId;

                Qry += " group by pl.RackNo,mm.MaterialDesc order by pl.PutawayDate";     //pl.PutawayDate,ph.PlantCode,mm.MaterialDesc,rm.StorageLocation, ,pl.PutawayDate
                dt = objMas.GetDataTable(Qry);
                

                return dt;
            }
            catch(Exception Ex)
            {
                objMas.PrintLog("C[2,4]", Ex.Message);
                return null;
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Report/RptPutaway_GenRpt")]
        public ActionResult RptPutaway_GenRpt(PutawayModel data)
        {
            try
            {
                int ret = 0;
                DataTable dt = new DataTable();

                DataColumn column = new DataColumn("SlNo", typeof(int));
                column.DataType = System.Type.GetType("System.Int32");
                column.AutoIncrement = true;
                column.AutoIncrementSeed = 1;
                column.AutoIncrementStep = 1;


                DataTable dtHeader = new DataTable();
                string[] AddlnData = new string[6];
                int[] AddlnHdrCols = new int[12];
                float[] ColWidth;

                dt = RptPutaway_GetQry(1, data);

                DataColumn Col = dt.Columns.Add("SlNo", typeof(int));
                Col.SetOrdinal(0);// to put the column in position 0;

                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["SlNo"] = i + 1;
                if (dt == null)
                {
                    objMas.PrintLog("A[2,5]", objMas.DBErrBuf);
                    return Json(new { Success = 0, Message = "Report Data Fetch Failed", Data = "" });
                }
                ColWidth = new float[8] { 2f, 4f, 5f, 5f, 8f, 3f, 3f, 3f };

                PdfPTable pdfTblHdl = new PdfPTable(dt.Columns.Count);
                int[] DType = Enumerable.Range(0, dt.Columns.Count).Select(n => MasterLogic.RPTSTYLE_STRING_SHORT).ToArray();
                DType[0] = MasterLogic.RPTSTYLE_INT;
                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                ret = objMas.GenerateReport(data.DwnldFmt, 1, 0, 0, "RptPutaway_", "Putaway Report", DType, null, null, data.FromDate, data.ToDate, AddlnData, null, null, null, dt, ColWidth);
                if (ret == 1)
                {
                    objMas.PrintLog("A[2,6]", objMas.DBErrBuf);
                    return Json(new { Success = 1, Message = "Success", Data = objMas.DBErrBuf });
                }
                objMas.PrintLog("[A[2,16]", objMas.DBErrBuf);
                return Json(new { Success = 0, Message = "Failed", Data = objMas.DBErrBuf });
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,5]: " , Ex.Message);
                return null;
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Report/RptPutawayLoadGrid")]
        public ActionResult RptPutawayLoadGrid(PutawayModel data)
        {
            try
            {
                DataTable dt = new DataTable();

                dt = RptPutaway_GetQry(0, data);
                if (dt == null)
                {
                    objMas.PrintLog("[A[2,17]", objMas.DBErrBuf);
                    return Json(new
                    {
                        Success = 0,
                        Message = "Report Data Fetch Failed",
                        Data = objMas.DBErrBuf
                    });
                }
                    

                var emp = (from DataRow row in dt.Rows
                           select new
                           {
                               PutawayDate = row.Field<string>("Putaway Date").ToString(),
                               BatchNo = row.Field<string>("Batch Number").ToString(),
                               MaterialCode = row.Field<string>("Material Code").ToString(),
                               MaterialDesc = row.Field<string>("Material Name").ToString(),
                               //StorageLocation = row.Field<string>("StorageLocation").ToString(),
                               CrateBarcode = row.Field<Int64>("Crate Count").ToString(),
                               BoxCount = row.Field<Int64>("Box Count").ToString(),
                               RackNo = row.Field<string>("Rack Number").ToString(),

                           }).ToList();

                return Json(emp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,6]: " , Ex.Message);
                return null;
            }
        }

        public ActionResult RejectionReport()
        {
            var model = new RjtReportModel();
            DataTable dt = new DataTable();
            model.FromDate = model.ToDate = System.DateTime.Now.ToString("yyyy-MM-dd");

            dt = objApp.LoadDwnldFormats(objMas);
            if (dt == null)
            {
                objMas.PrintLog("[A[2,18]", objMas.DBErrBuf);
                return View(model);
            }
                
            model.DwnldFmtDetail = (from DataRow a in dt.Rows
                                     select new SelectListItem { Text = a["DwnldDesc"].ToString(), Value = a["DwnldId"].ToString() }).ToList();

            ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.STORejectionReport, Request.Cookies["UserId"].Value.ToString());
            model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());
            return View(model);
        }

        private DataTable Reject_GetQry(int IsDwnld, RjtReportModel rjtmodel)
        {
            DataTable dt = new DataTable();
            string Qry = "";
            Qry = ("select date_format(ph.PDNDate,'%d-%m-%Y') as 'Production Date', pl.BatchNo, pl.MaterialCode, mm.MaterialDesc, Count(distinct pl.Barcode) " +                //, group_concat(distinct pl.Barcode) as BarcodeList
                    " as 'SecondaryCount', group_concat(distinct pl.CrateBarcode) as Crates from production_lines pl, production_header ph, material_master mm " +
                    " where pl.BatchNo=ph.BatchNo and pl.MaterialCode=mm.MaterialCode and pl.Reject = '1' and ph.PDNDate >= '"+rjtmodel.FromDate+"' and ph.PDNDate <= '"+rjtmodel.ToDate+"' " +
                    " group by mm.MaterialDesc");

            dt = objMas.GetDataTable(Qry);
            return dt;
        }

        [HttpPost]
        [Route("action")]
        [Route("Report/RptRejectionLoadGrid")]
        public ActionResult RptRejectionLoadGrid(RjtReportModel rjtmodel)
        {
            try
            {
                DataTable dt = new DataTable();

                dt = Reject_GetQry(0, rjtmodel);
                if (dt == null)
                {
                    objMas.PrintLog("[A[2,19]", objMas.DBErrBuf);
                    return Content("Report Data Fetch Failed");
                }
                    

                var rjt = (from DataRow row in dt.Rows
                           select new
                           {
                               MaterialCode = row.Field<string>("MaterialCode").ToString(),
                               ProductionDate = row.Field<string>("Production Date").ToString(),
                               MaterialDesc = row.Field<string>("MaterialDesc").ToString(),
                               BatchNo = row.Field<string>("BatchNo").ToString(),
                               Crates = row.Field<string>("Crates").ToString(),
                               SecondaryCount = row.Field<Int64>("SecondaryCount").ToString(),

                           }).ToList();
                return Json(rjt, JsonRequestBehavior.AllowGet);

            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,7]", Ex.Message);
                return null;
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Report/GetBarcodeItems")]
        public ActionResult GetBarcodeItems(RjtReportModel rjtmodel)
        {
            try
            {
                DataTable dt = new DataTable();
                dt = objMas.GetDataTable("select pl.Barcode from production_lines pl, production_header ph where pl.BatchNo='" + rjtmodel.BatchNo + "' " +
                            " AND pl.BatchNo=ph.BatchNo AND pl.Reject=1 group by pl.Barcode");
                if (dt == null)
                {
                    objMas.PrintLog("[A[2,20]", objMas.DBErrBuf);
                    return Json(new { Success = 0, Message = "Data Fetch Failed", Data = "" });
                }
                    
                if (dt.Rows.Count == 0)
                {
                    objMas.PrintLog("[A[2,21]", objMas.DBErrBuf);
                    return Json(new { Success = 0, Message = "No Data Found For this selection", Data = "" });
                }

                var rjt = (from DataRow row in dt.Rows
                           select new
                           {
                               Barcode = row.Field<string>("Barcode").ToString(),

                           }).ToList();
                return Json(rjt, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                return Content("C[2,8]", Ex.Message);
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("Report/RejectionGenReport")]
        public ActionResult RejectionGenReport(RjtReportModel rjtmodel)
        {
            try
            {
                //int ret = 0;
                //DataTable dt = new DataTable();

                //dt = Reject_GetQry(1, rjtmodel);
                //if (dt == null)
                //{
                //    objMas.PrintLog("[A[2,22]", objMas.DBErrBuf);
                //    return Json(new { Success = 0, Message = "Report Data Fetch Failed", Data = "" });
                //}

                //int[] DType = Enumerable.Range(0, dt.Columns.Count).Select(n => MasterLogic.RPTSTYLE_STRING_SHORT).ToArray();

                //ret = objMas.GenerateReport(rjtmodel.DwnldFmt, 1, 0, 1, "Reject", "Rejection Report", DType, null, null, rjtmodel.FromDate, rjtmodel.ToDate, null, null, null,null, dt, null);
                //if (ret == 1)
                //{
                //    objMas.PrintLog("[A[2,23]", objMas.DBErrBuf);
                //    return Json(new { Success = 1, Message = "Success", Data = objMas.DBErrBuf });
                //}





                int ret = 0;
                DataTable dt = new DataTable();

                DataColumn column = new DataColumn("SlNo", typeof(int));
                column.DataType = System.Type.GetType("System.Int32");
                column.AutoIncrement = true;
                column.AutoIncrementSeed = 1;
                column.AutoIncrementStep = 1;


                DataTable dtHeader = new DataTable();
                string[] AddlnData = new string[6];
                int[] AddlnHdrCols = new int[12];
                float[] ColWidth;

                dt = Reject_GetQry(1, rjtmodel);

                DataColumn Col = dt.Columns.Add("SlNo", typeof(int));
                Col.SetOrdinal(0);// to put the column in position 0;

                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["SlNo"] = i + 1;
                if (dt == null)
                {
                    objMas.PrintLog("A[2,5]", objMas.DBErrBuf);
                    return Json(new { Success = 0, Message = "Report Data Fetch Failed", Data = "" });
                }
                ColWidth = new float[7] { 2f, 5f, 5f, 5f, 5f, 5f, 5f};

                PdfPTable pdfTblHdl = new PdfPTable(dt.Columns.Count);
                int[] DType = Enumerable.Range(0, dt.Columns.Count).Select(n => MasterLogic.RPTSTYLE_STRING_SHORT).ToArray();
                DType[0] = MasterLogic.RPTSTYLE_INT;
                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                ret = objMas.GenerateReport(rjtmodel.DwnldFmt, 1, 0, 0, "Reject_", "Rejection Report", DType, null, null, rjtmodel.FromDate, rjtmodel.ToDate, AddlnData, null, null, null, dt, ColWidth);
                if (ret == 1)
                {
                    objMas.PrintLog("A[2,6]", objMas.DBErrBuf);
                    return Json(new { Success = 1, Message = "Success", Data = objMas.DBErrBuf });
                }
                objMas.PrintLog("[A[2,24]", objMas.DBErrBuf);
                return Json(new { Success = 0, Message = "Failed", Data = objMas.DBErrBuf });
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,9]: " , Ex.Message);
                return null;
            }
        }


        public ActionResult CFARejectionReport()
        {
            var model = new CFARjtReportModel();
            DataTable dt = new DataTable();
            model.FromDate = model.ToDate = System.DateTime.Now.ToString("yyyy-MM-dd");

            dt = objApp.LoadDwnldFormats(objMas);
            if (dt == null)
            {
                objMas.PrintLog("[A[2,25]", objMas.DBErrBuf);
                return View(model);
            }

            model.DwnldFmtDetail = (from DataRow a in dt.Rows
                                    select new SelectListItem { Text = a["DwnldDesc"].ToString(), Value = a["DwnldId"].ToString() }).ToList();

            ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.CFA_Rejection_Report, Request.Cookies["UserId"].Value.ToString());
            model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());
            return View(model);
        }

        private DataTable CFAReject_GetQry(int IsDwnld, CFARjtReportModel rjtmodel)
        {
            DataTable dt = new DataTable();
            string Qry = "";
            Qry = ("select date_format(gd.PDNDate,'%d-%m-%Y') as 'Production Date', gd.BatchNo, gd.MaterialCode, mm.MaterialDesc, Count(distinct gd.Barcode) " +                //, group_concat(distinct pl.Barcode) as BarcodeList
                    " as 'SecondaryCount', group_concat(distinct gd.CrateBarcode) as Crates from cfagrn_details gd, material_master mm " +
                    " where gd.MaterialCode=mm.MaterialCode and gd.Reject = '1' and gd.PDNDate >= '" + rjtmodel.FromDate + "' and gd.PDNDate <= '" + rjtmodel.ToDate + "' " +
                    " group by mm.MaterialDesc order by gd.PDNDate");

            dt = objMas.GetDataTable(Qry);
            return dt;
        }

        [HttpPost]
        [Route("action")]
        [Route("Report/CFARptRejectionLoadGrid")]
        public ActionResult CFARptRejectionLoadGrid(CFARjtReportModel rjtmodel)
        {
            try
            {
                DataTable dt = new DataTable();

                dt = CFAReject_GetQry(0, rjtmodel);
                if (dt == null)
                {
                    objMas.PrintLog("[A[2,26]", objMas.DBErrBuf);
                    return Content("Report Data Fetch Failed");
                }


                var rjt = (from DataRow row in dt.Rows
                           select new
                           {
                               MaterialCode = row.Field<string>("MaterialCode").ToString(),
                               ProductionDate = row.Field<string>("Production Date").ToString(),
                               MaterialDesc = row.Field<string>("MaterialDesc").ToString(),
                               BatchNo = row.Field<string>("BatchNo").ToString(),
                               Crates = row.Field<string>("Crates").ToString(),
                               SecondaryCount = row.Field<Int64>("SecondaryCount").ToString(),

                           }).ToList();
                return Json(rjt, JsonRequestBehavior.AllowGet);

            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,10]", Ex.Message);
                return null;
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Report/CFAGetBarcodeItems")]
        public ActionResult CFAGetBarcodeItems(CFARjtReportModel rjtmodel)
        {
            try
            {
                DataTable dt = new DataTable();
                dt = objMas.GetDataTable("select pl.Barcode from cfagrn_details pl where pl.BatchNo='" + rjtmodel.BatchNo + "' " +
                            " AND pl.Reject=1 group by pl.Barcode");
                if (dt == null)
                {
                    objMas.PrintLog("[A[2,27]", objMas.DBErrBuf);
                    return Json(new { Success = 0, Message = "Data Fetch Failed", Data = "" });
                }

                if (dt.Rows.Count == 0)
                {
                    objMas.PrintLog("[A[2,28]", objMas.DBErrBuf);
                    return Json(new { Success = 0, Message = "No Data Found For this selection", Data = "" });
                }

                var rjt = (from DataRow row in dt.Rows
                           select new
                           {
                               Barcode = row.Field<string>("Barcode").ToString(),

                           }).ToList();
                return Json(rjt, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                return Content("C[2,11]", Ex.Message);
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("Report/CFARejectionGenReport")]
        public ActionResult CFARejectionGenReport(CFARjtReportModel rjtmodel)
        {
            try
            {
                //int ret = 0;
                //DataTable dt = new DataTable();

                //dt = CFAReject_GetQry(1, rjtmodel);
                //if (dt == null)
                //{
                //    objMas.PrintLog("[A[2,29]", objMas.DBErrBuf);
                //    return Json(new { Success = 0, Message = "Report Data Fetch Failed", Data = "" });
                //}

                //int[] DType = Enumerable.Range(0, dt.Columns.Count).Select(n => MasterLogic.RPTSTYLE_STRING_SHORT).ToArray();

                //ret = objMas.GenerateReport(rjtmodel.DwnldFmt, 1, 0, 1, "CFAReject_", "CFA Rejection Report", DType, null, null, rjtmodel.FromDate, rjtmodel.ToDate, null, null, null, null, dt, null);
                //if (ret == 1)
                //{
                //    objMas.PrintLog("[A[2,30]", objMas.DBErrBuf);
                //    return Json(new { Success = 1, Message = "Success", Data = objMas.DBErrBuf });
                //}





                int ret = 0;
                DataTable dt = new DataTable();

                DataColumn column = new DataColumn("SlNo", typeof(int));
                column.DataType = System.Type.GetType("System.Int32");
                column.AutoIncrement = true;
                column.AutoIncrementSeed = 1;
                column.AutoIncrementStep = 1;


                DataTable dtHeader = new DataTable();
                string[] AddlnData = new string[6];
                int[] AddlnHdrCols = new int[12];
                float[] ColWidth;

                dt = CFAReject_GetQry(1, rjtmodel);

                DataColumn Col = dt.Columns.Add("SlNo", typeof(int));
                Col.SetOrdinal(0);// to put the column in position 0;

                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["SlNo"] = i + 1;
                if (dt == null)
                {
                    objMas.PrintLog("A[2,5]", objMas.DBErrBuf);
                    return Json(new { Success = 0, Message = "Report Data Fetch Failed", Data = "" });
                }
                ColWidth = new float[7] { 2f, 5f, 5f, 5f, 5f, 5f, 5f };

                PdfPTable pdfTblHdl = new PdfPTable(dt.Columns.Count);
                int[] DType = Enumerable.Range(0, dt.Columns.Count).Select(n => MasterLogic.RPTSTYLE_STRING_SHORT).ToArray();
                DType[0] = MasterLogic.RPTSTYLE_INT;
                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                ret = objMas.GenerateReport(rjtmodel.DwnldFmt, 1, 0, 0, "CFAReject_", "CFA Rejection Report", DType, null, null, rjtmodel.FromDate, rjtmodel.ToDate, AddlnData, null, null, null, dt, ColWidth);
                if (ret == 1)
                {
                    objMas.PrintLog("A[2,6]", objMas.DBErrBuf);
                    return Json(new { Success = 1, Message = "Success", Data = objMas.DBErrBuf });
                }
                objMas.PrintLog("[A[2,31]", objMas.DBErrBuf);
                return Json(new { Success = 0, Message = "Failed", Data = objMas.DBErrBuf });
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,12]: ", Ex.Message);
                return null;
            }
        }




        public ActionResult ProductionStockReport()
        {
            DataTable dt = new DataTable();

            var model = new StockModel();
            dt = objMas.GetDataTable("Select PlantId,PlantName from Plant_master where Status in (1,2) Order by PlantName");
            if (dt == null)
            {
                objMas.PrintLog("A[2,31]", objMas.DBErrBuf);
                return View(model);
            }


            model.PlantDetails = (from DataRow a in dt.Rows
                                  select new SelectListItem { Text = a["PlantName"].ToString(), Value = a["PlantId"].ToString() }).ToList();

            dt = objMas.GetDataTable("Select UnitId,UnitName from Unit_master where Status in (1,2) Order by UnitName");
            if (dt == null)
            {
                objMas.PrintLog("A[2,32]", objMas.DBErrBuf);
                return View(model);
            }


            model.UnitDetails = (from DataRow a in dt.Rows
                                 select new SelectListItem { Text = a["UnitName"].ToString(), Value = a["UnitId"].ToString() }).ToList();

            dt = objMas.GetDataTable("Select SystemId,SystemName from system_master where Status in (1,2) Order by SystemName");
            if (dt == null)
            {
                objMas.PrintLog("A[2,33]", objMas.DBErrBuf);
                return View(model);
            }

            model.SystemDetails = (from DataRow a in dt.Rows
                                   select new SelectListItem { Text = a["SystemName"].ToString(), Value = a["SystemId"].ToString() }).ToList();

            model.FromDate = model.ToDate = System.DateTime.Now.ToString("yyyy-MM-dd");

            dt = objApp.LoadDwnldFormats(objMas);
            if (dt == null)
            {
                objMas.PrintLog("A[2,34]", objMas.DBErrBuf);
                return View(model);
            }

            model.DwnldFmtDetail = (from DataRow a in dt.Rows
                                    select new SelectListItem { Text = a["DwnldDesc"].ToString(), Value = Convert.ToInt16(a["DwnldId"]).ToString() }).ToList();

            ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.STOCK_SMART_RPT, Request.Cookies["UserId"].Value.ToString());
            model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());
            return View(model);
        }

        private DataTable ProductionStock_GetQry(int IsDwnld, StockModel rjtmodel)
        {
            DataTable dt = new DataTable();
            string Qry = "";
            Qry = ("select date_format(PDNDate,'%d-%m-%Y') as 'Date',pm.PlantName as Plant,um.UnitName as Unit, sm.SystemName as 'SystemName'," +
                    " pl.MaterialCode,mm.MaterialDesc as 'Material Name',pl.BatchNo as Batch,Count(Distinct pl.Barcode) as 'Avilable BOX' " +
                    " from production_header ph, production_lines pl, plant_master pm, unit_master um,system_master sm, material_master mm where " +
                    " ph.PDNId=pl.PDNId and Date(ph.PDNDate)>='2023-01-01' and Date(ph.PDNDate)<= '2023-02-14' and ph.PlantCode=pm.PlantId and " +
                    " ph.MaterialCode=mm.MaterialCode and ph.UnitId=um.UnitId and ph.System=sm.SystemId and pl.PickFlag = '0' and pl.Reject = '0' " +
                    " and pl.DeliveryNo = '' and pl.ShipmentNo='' " + (rjtmodel.PlantId != null ? " and ph.PlantCode='" + rjtmodel.PlantId + "'" : "") + 
                     (rjtmodel.UnitId != null ? " and ph.UnitId=" + rjtmodel.UnitId : "") + (rjtmodel.SystemId != null ? " and pl.System=" + rjtmodel.SystemId : "") + " " +
                     " group by pl.BatchNo order by PDNDate");

            dt = objMas.GetDataTable(Qry);
            return dt;
        }

        [HttpPost]
        [Route("action")]
        [Route("Report/ProductionStockLoadGrid")]
        public ActionResult ProductionStockLoadGrid(StockModel rjtmodel)
        {
            try
            {
                DataTable dt = new DataTable();

                dt = ProductionStock_GetQry(0, rjtmodel);
                if (dt == null)
                {
                    objMas.PrintLog("A[2,35]", objMas.DBErrBuf);
                    return Content("Report Data Fetch Failed");
                }


                var emp = (from DataRow row in dt.Rows
                           select new
                           {
                               DispDate = row.Field<string>("Date").ToString(),
                               Plant = row.Field<string>("Plant").ToString(),
                               Unit = row.Field<string>("Unit").ToString(),
                               System = row.Field<string>("SystemName").ToString(),
                               MaterialCode = row.Field<string>("MaterialCode").ToString(),
                               MaterialDesc = row.Field<string>("Material Name").ToString(),
                               Batch = row.Field<string>("Batch").ToString(),
                               StockQty = row.Field<Int64>("Avilable BOX").ToString(),
                           }).ToList();

                return Json(emp, JsonRequestBehavior.AllowGet);

            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,13]", Ex.Message);
                return null;
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Report/ProductionStockGenReport")]
        public ActionResult ProductionStockGenReport(StockModel rjtmodel)
        {
            try
            {
                //int ret = 0;
                //DataTable dt = new DataTable();

                //dt = ProductionStock_GetQry(1, rjtmodel);
                //if (dt == null)
                //{
                //    objMas.PrintLog("[A[2,36]", objMas.DBErrBuf);
                //    return Json(new { Success = 0, Message = "Report Data Fetch Failed", Data = "" });
                //}

                //int[] DType = Enumerable.Range(0, dt.Columns.Count).Select(n => MasterLogic.RPTSTYLE_STRING_SHORT).ToArray();

                //ret = objMas.GenerateReport(rjtmodel.DwnldFmt, 1, 0, 1, "ProductionStock_", "Production Stock Report", DType, null, null, "", "", null, null, null, null, dt, null);
                //if (ret == 1)
                //{
                //    objMas.PrintLog("[A[2,37]", objMas.DBErrBuf);
                //    return Json(new { Success = 1, Message = "Success", Data = objMas.DBErrBuf });
                //}

                int ret = 0;
                DataTable dt = new DataTable();

                DataColumn column = new DataColumn("SlNo", typeof(int));
                column.DataType = System.Type.GetType("System.Int32");
                column.AutoIncrement = true;
                column.AutoIncrementSeed = 1;
                column.AutoIncrementStep = 1;


                DataTable dtHeader = new DataTable();
                string[] AddlnData = new string[6];
                int[] AddlnHdrCols = new int[12];
                float[] ColWidth;

                dt = ProductionStock_GetQry(1, rjtmodel);

                DataColumn Col = dt.Columns.Add("SlNo", typeof(int));
                Col.SetOrdinal(0);// to put the column in position 0;

                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["SlNo"] = i + 1;
                if (dt == null)
                {
                    objMas.PrintLog("A[2,5]", objMas.DBErrBuf);
                    return Json(new { Success = 0, Message = "Report Data Fetch Failed", Data = "" });
                }
                ColWidth = new float[9] { 2f, 5f, 5f, 5f, 5f, 5f, 5f, 5f, 5f };

                PdfPTable pdfTblHdl = new PdfPTable(dt.Columns.Count);
                int[] DType = Enumerable.Range(0, dt.Columns.Count).Select(n => MasterLogic.RPTSTYLE_STRING_SHORT).ToArray();
                DType[0] = MasterLogic.RPTSTYLE_INT;
                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                ret = objMas.GenerateReport(rjtmodel.DwnldFmt, 1, 0, 0, "ProductionStock_", "Production Stock Report", DType, null, null, rjtmodel.FromDate, rjtmodel.ToDate, AddlnData, null, null, null, dt, ColWidth);
                if (ret == 1)
                {
                    objMas.PrintLog("A[2,6]", objMas.DBErrBuf);
                    return Json(new { Success = 1, Message = "Success", Data = objMas.DBErrBuf });
                }
                objMas.PrintLog("[A[2,38]", objMas.DBErrBuf);
                return Json(new { Success = 0, Message = "Failed", Data = objMas.DBErrBuf });
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,14]: ", Ex.Message);
                return null;
            }
        }



        public ActionResult CFAStockReport()
        {
            DataTable dt = new DataTable();

            var model = new StockModel();
            dt = objMas.GetDataTable("Select PlantId,PlantName from Plant_master where Status in (1,2) Order by PlantName");
            if (dt == null)
            {
                objMas.PrintLog("A[2,39]", objMas.DBErrBuf);
                return View(model);
            }


            model.PlantDetails = (from DataRow a in dt.Rows
                                  select new SelectListItem { Text = a["PlantName"].ToString(), Value = a["PlantId"].ToString() }).ToList();

            dt = objMas.GetDataTable("Select UnitId,UnitName from Unit_master where Status in (1,2) Order by UnitName");
            if (dt == null)
            {
                objMas.PrintLog("A[2,2]", objMas.DBErrBuf);
                return View(model);
            }


            model.UnitDetails = (from DataRow a in dt.Rows
                                 select new SelectListItem { Text = a["UnitName"].ToString(), Value = a["UnitId"].ToString() }).ToList();

            dt = objMas.GetDataTable("Select SystemId,SystemName from system_master where Status in (1,2) Order by SystemName");
            if (dt == null)
            {
                objMas.PrintLog("A[2,40]", objMas.DBErrBuf);
                return View(model);
            }

            model.SystemDetails = (from DataRow a in dt.Rows
                                   select new SelectListItem { Text = a["SystemName"].ToString(), Value = a["SystemId"].ToString() }).ToList();

            model.FromDate = model.ToDate = System.DateTime.Now.ToString("yyyy-MM-dd");

            dt = objApp.LoadDwnldFormats(objMas);
            if (dt == null)
            {
                objMas.PrintLog("A[2,41]", objMas.DBErrBuf);
                return View(model);
            }

            model.DwnldFmtDetail = (from DataRow a in dt.Rows
                                    select new SelectListItem { Text = a["DwnldDesc"].ToString(), Value = Convert.ToInt16(a["DwnldId"]).ToString() }).ToList();

            ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.CFA_Stock_Report, Request.Cookies["UserId"].Value.ToString());
            model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());
            return View(model);
        }

        private DataTable CFAStock_GetQry(int IsDwnld, StockModel rjtmodel)
        {
            DataTable dt = new DataTable();
            string Qry = "";
            Qry = ("select date_format(PDNDate,'%d-%m-%Y') as 'Date',pm.PlantName as Plant, gd.MaterialCode, mm.MaterialDesc as 'Material Name',gd.BatchNo as Batch," +
                    " Count(Distinct gd.Barcode) as 'Avilable BOX' from plant_master pm, cfagrn_details gd, material_master mm where gd.MaterialCode=mm.MaterialCode " +
                    " and gd.PickFlag = '0' and gd.Reject = '0' and ToShipmentNo = '' and gd.ReceivingPlant=pm.PlantId and ToDeliveryNo= '' " + (rjtmodel.PlantId != null ? " " +
                    "and gd.ReceivingPlant='" + rjtmodel.PlantId + "'" : "") +  " group by gd.BatchNo order by PDNDate");

            dt = objMas.GetDataTable(Qry);
            return dt;
        }

        [HttpPost]
        [Route("action")]
        [Route("Report/CFAStockLoadGrid")]
        public ActionResult CFAStockLoadGrid(StockModel rjtmodel)
        {
            try
            {
                DataTable dt = new DataTable();

                dt = CFAStock_GetQry(0, rjtmodel);
                if (dt == null)
                {
                    objMas.PrintLog("A[2,42]", objMas.DBErrBuf);
                    return Content("Report Data Fetch Failed");
                }


                var emp = (from DataRow row in dt.Rows
                           select new
                           {
                               DispDate = row.Field<string>("Date").ToString(),
                               Plant = row.Field<string>("Plant").ToString(),
                               MaterialCode = row.Field<string>("MaterialCode").ToString(),
                               MaterialDesc = row.Field<string>("Material Name").ToString(),
                               Batch = row.Field<string>("Batch").ToString(),
                               StockQty = row.Field<Int64>("Avilable BOX").ToString(),
                           }).ToList();

                return Json(emp, JsonRequestBehavior.AllowGet);

            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,15]", Ex.Message);
                return null;
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Report/CFAStockGenReport")]
        public ActionResult CFAStockGenReport(StockModel rjtmodel)
        {
            try
            {
                int ret = 0;
                DataTable dt = new DataTable();

                dt = CFAStock_GetQry(1, rjtmodel);
                if (dt == null)
                {
                    objMas.PrintLog("[A[2,43]", objMas.DBErrBuf);
                    return Json(new { Success = 0, Message = "Report Data Fetch Failed", Data = "" });
                }

                int[] DType = Enumerable.Range(0, dt.Columns.Count).Select(n => MasterLogic.RPTSTYLE_STRING_SHORT).ToArray();

                ret = objMas.GenerateReport(rjtmodel.DwnldFmt, 1, 0, 1, "CFAStock_", "CFA Stock Report", DType, null, null, "", "", null, null, null, null, dt, null);
                if (ret == 1)
                {
                    objMas.PrintLog("[A[2,44]", objMas.DBErrBuf);
                    return Json(new { Success = 1, Message = "Success", Data = objMas.DBErrBuf });
                }
                objMas.PrintLog("[A[2,45]", objMas.DBErrBuf);
                return Json(new { Success = 0, Message = "Failed", Data = objMas.DBErrBuf });
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,16]: ", Ex.Message);
                return null;
            }
        }





        public ActionResult ProductionDispatchReport()
        {
            DataTable dt = new DataTable();

            var model = new PdnDispatchModel();
            dt = objMas.GetDataTable("Select PlantId,PlantName from Plant_master where Status in (1,2) Order by PlantName");
            if (dt == null)
            {
                objMas.PrintLog("A[2,39]", objMas.DBErrBuf);
                return View(model);
            }


            model.PlantDetails = (from DataRow a in dt.Rows
                                  select new SelectListItem { Text = a["PlantName"].ToString(), Value = a["PlantId"].ToString() }).ToList();

            dt = objMas.GetDataTable("Select UnitId,UnitName from Unit_master where Status in (1,2) Order by UnitName");
            if (dt == null)
            {
                objMas.PrintLog("A[2,2]", objMas.DBErrBuf);
                return View(model);
            }


            model.UnitDetails = (from DataRow a in dt.Rows
                                 select new SelectListItem { Text = a["UnitName"].ToString(), Value = a["UnitId"].ToString() }).ToList();

            dt = objMas.GetDataTable("Select SystemId,SystemName from system_master where Status in (1,2) Order by SystemName");
            if (dt == null)
            {
                objMas.PrintLog("A[2,40]", objMas.DBErrBuf);
                return View(model);
            }

            model.SystemDetails = (from DataRow a in dt.Rows
                                   select new SelectListItem { Text = a["SystemName"].ToString(), Value = a["SystemId"].ToString() }).ToList();

            model.FromDate = model.ToDate = System.DateTime.Now.ToString("yyyy-MM-dd");

            dt = objApp.LoadDwnldFormats(objMas);
            if (dt == null)
            {
                objMas.PrintLog("A[2,41]", objMas.DBErrBuf);
                return View(model);
            }

            model.DwnldFmtDetail = (from DataRow a in dt.Rows
                                    select new SelectListItem { Text = a["DwnldDesc"].ToString(), Value = Convert.ToInt16(a["DwnldId"]).ToString() }).ToList();

            ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.Production_Dispatch_Report, Request.Cookies["UserId"].Value.ToString());
            model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());
            return View(model);
        }

        private DataTable ProductionDispatch_GetQry(int IsDwnld, PdnDispatchModel rjtmodel)
        {
            DataTable dt = new DataTable();
            string Qry = "";
            Qry = ("select date_format(pl.PickedOn,'%d-%m-%Y') as 'Dispatch Date',pl.ShipmentNo as 'Shipment Number', pl.DeliveryNo as 'Delivery Number',pm.PlantName as 'From Plant'," +
                    " sdh.RECEIVING_PLANT as 'Receiving Plant',   pl.MaterialCode, mm.MaterialDesc as 'Material Name',pl.BatchNo as Batch,Count(distinct pl.Barcode) as Dispatch_QTY " +
                    " from production_header ph, sapdispatchorderheader sdh, production_lines pl, plant_master pm, unit_master um,system_master sm, " +
                    " material_master mm where ph.PDNId=pl.PDNId and ph.PlantCode=pm.PlantId and sdh.DELIVERY_NO=pl.DeliveryNo and sdh.SHIPMENT_OR_PO=pl.ShipmentNo " +
                    " and Date(pl.PickedOn)>='" + rjtmodel.FromDate + "' and Date(pl.PickedOn)<='" + rjtmodel.ToDate + "' and pl.MaterialCode=mm.MaterialCode " +
                    " and pl.DeliveryNo <> '' and pl.ShipmentNo <> '' and pl.PickFlag = '1'  " +
                    " " + (rjtmodel.PlantId != null ? " and gd.ReceivingPlant='" + rjtmodel.PlantId + "'" : "") + (rjtmodel.UnitId != null ? " and ph.UnitId='" + 
                     rjtmodel.UnitId + "'" : "") + (rjtmodel.SystemId != null ? " and ph.System='" + rjtmodel.SystemId + "'" : "") + " group by pl.DeliveryNo order by pl.PickedOn");

            dt = objMas.GetDataTable(Qry);
            return dt;
        }

        [HttpPost]
        [Route("action")]
        [Route("Report/ProductionDispatchLoadGrid")]
        public ActionResult ProductionDispatchLoadGrid(PdnDispatchModel rjtmodel)
        {
            try
            {
                DataTable dt = new DataTable();

                dt = ProductionDispatch_GetQry(0, rjtmodel);
                if (dt == null)
                {
                    objMas.PrintLog("A[2,42]", objMas.DBErrBuf);
                    return Content("Report Data Fetch Failed");
                }


                var emp = (from DataRow row in dt.Rows
                           select new
                           {
                               DispDate = row.Field<string>("Dispatch Date").ToString(),
                               Shipment = row.Field<string>("Shipment Number").ToString(),
                               Delivery = row.Field<string>("Delivery Number").ToString(),
                               FromPlant = row.Field<string>("From Plant").ToString(),
                               ReceivePlant = row.Field<string>("Receiving Plant").ToString(),
                               MaterialCode = row.Field<string>("MaterialCode").ToString(),
                               MaterialDesc = row.Field<string>("Material Name").ToString(),
                               Batch = row.Field<string>("Batch").ToString(),
                               DispatchQty = row.Field<Int64>("Dispatch_QTY").ToString(),
                           }).ToList();

                return Json(emp, JsonRequestBehavior.AllowGet);

            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,15]", Ex.Message);
                return null;
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Report/ProductionDispatchGenReport")]
        public ActionResult ProductionDispatchGenReport(PdnDispatchModel rjtmodel)
        {
            try
            {
                //int ret = 0;
                //DataTable dt = new DataTable();

                //dt = ProductionDispatch_GetQry(1, rjtmodel);
                //if (dt == null)
                //{
                //    objMas.PrintLog("[A[2,43]", objMas.DBErrBuf);
                //    return Json(new { Success = 0, Message = "Report Data Fetch Failed", Data = "" });
                //}

                //int[] DType = Enumerable.Range(0, dt.Columns.Count).Select(n => MasterLogic.RPTSTYLE_STRING_SHORT).ToArray();

                //ret = objMas.GenerateReport(rjtmodel.DwnldFmt, 1, 0, 1, "ProductionDispatch", "Production Dispatch Report", DType, null, null, rjtmodel.FromDate, rjtmodel.ToDate, null, null, null, null, dt, null);
                //if (ret == 1)
                //{
                //    objMas.PrintLog("[A[2,44]", objMas.DBErrBuf);
                //    return Json(new { Success = 1, Message = "Success", Data = objMas.DBErrBuf });
                //}



                int ret = 0;
                DataTable dt = new DataTable();

                DataColumn column = new DataColumn("SlNo", typeof(int));
                column.DataType = System.Type.GetType("System.Int32");
                column.AutoIncrement = true;
                column.AutoIncrementSeed = 1;
                column.AutoIncrementStep = 1;


                DataTable dtHeader = new DataTable();
                string[] AddlnData = new string[6];
                int[] AddlnHdrCols = new int[12];
                float[] ColWidth;

                dt = ProductionDispatch_GetQry(1, rjtmodel);

                DataColumn Col = dt.Columns.Add("SlNo", typeof(int));
                Col.SetOrdinal(0);// to put the column in position 0;

                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["SlNo"] = i + 1;
                if (dt == null)
                {
                    objMas.PrintLog("A[2,5]", objMas.DBErrBuf);
                    return Json(new { Success = 0, Message = "Report Data Fetch Failed", Data = "" });
                }
                ColWidth = new float[10] { 2f, 5f, 5f, 5f, 5f, 5f, 5f, 5f, 5f, 5f };

                PdfPTable pdfTblHdl = new PdfPTable(dt.Columns.Count);
                int[] DType = Enumerable.Range(0, dt.Columns.Count).Select(n => MasterLogic.RPTSTYLE_STRING_SHORT).ToArray();
                DType[0] = MasterLogic.RPTSTYLE_INT;
                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                ret = objMas.GenerateReport(rjtmodel.DwnldFmt, 1, 0, 0, "ProductionDispatch", "Production Dispatch Report", DType, null, null, rjtmodel.FromDate,
                                    rjtmodel.ToDate, AddlnData, null, null, null, dt, ColWidth);
                if (ret == 1)
                {
                    objMas.PrintLog("A[2,6]", objMas.DBErrBuf);
                    return Json(new { Success = 1, Message = "Success", Data = objMas.DBErrBuf });
                }
                objMas.PrintLog("[A[2,45]", objMas.DBErrBuf);
                return Json(new { Success = 0, Message = "Failed", Data = objMas.DBErrBuf });
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,16]: ", Ex.Message);
                return null;
            }
        }




        public ActionResult CFADispatchReport()
        {
            DataTable dt = new DataTable();

            var model = new CfaDispatchModel();
            dt = objMas.GetDataTable("Select PlantId,PlantName from Plant_master where Status in (1,2) Order by PlantName");
            if (dt == null)
            {
                objMas.PrintLog("A[2,39]", objMas.DBErrBuf);
                return View(model);
            }


            model.PlantDetails = (from DataRow a in dt.Rows
                                  select new SelectListItem { Text = a["PlantName"].ToString(), Value = a["PlantId"].ToString() }).ToList();

            dt = objMas.GetDataTable("Select UnitId,UnitName from Unit_master where Status in (1,2) Order by UnitName");
            if (dt == null)
            {
                objMas.PrintLog("A[2,2]", objMas.DBErrBuf);
                return View(model);
            }


            model.UnitDetails = (from DataRow a in dt.Rows
                                 select new SelectListItem { Text = a["UnitName"].ToString(), Value = a["UnitId"].ToString() }).ToList();

            dt = objMas.GetDataTable("Select SystemId,SystemName from system_master where Status in (1,2) Order by SystemName");
            if (dt == null)
            {
                objMas.PrintLog("A[2,40]", objMas.DBErrBuf);
                return View(model);
            }

            model.SystemDetails = (from DataRow a in dt.Rows
                                   select new SelectListItem { Text = a["SystemName"].ToString(), Value = a["SystemId"].ToString() }).ToList();

            model.FromDate = model.ToDate = System.DateTime.Now.ToString("yyyy-MM-dd");

            dt = objApp.LoadDwnldFormats(objMas);
            if (dt == null)
            {
                objMas.PrintLog("A[2,41]", objMas.DBErrBuf);
                return View(model);
            }

            model.DwnldFmtDetail = (from DataRow a in dt.Rows
                                    select new SelectListItem { Text = a["DwnldDesc"].ToString(), Value = Convert.ToInt16(a["DwnldId"]).ToString() }).ToList();

            ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.CFA_Dispatch_Report, Request.Cookies["UserId"].Value.ToString());
            model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());
            return View(model);
        }

        private DataTable CFADispatch_GetQry(int IsDwnld, CfaDispatchModel rjtmodel)
        {
            DataTable dt = new DataTable();
            string Qry = "";
            Qry = ("select date_format(pl.PickedOn,'%d-%m-%Y') as 'Dispatch Date',pl.ToShipmentNo as 'Shipment Number', pl.ToDeliveryNo as 'Delivery Number',pl.FromPlant " +
                    " as 'From Plant',sdh.RECEIVING_PLANT as 'Receiving Plant', pl.MaterialCode, mm.MaterialDesc as 'Material Name',pl.BatchNo as Batch,Count(distinct pl.Barcode) as Dispatch_QTY " +
                    " from production_header ph, sapdispatchorderheader sdh, cfagrn_details pl, plant_master pm, unit_master um,system_master sm, material_master mm " +
                    " where ph.PlantCode=pm.PlantId and sdh.DELIVERY_NO=pl.ToDeliveryNo and sdh.SHIPMENT_OR_PO=pl.ToShipmentNo and pl.MaterialCode=mm.MaterialCode and " +
                    " pl.ToDeliveryNo <> '' and pl.ToShipmentNo <> '' and pl.PickFlag = '1' " + (rjtmodel.PlantId != null ? " and gd.ReceivingPlant='" + rjtmodel.PlantId + "'" : "") + 
                     (rjtmodel.UnitId != null ? " and ph.UnitId='" + rjtmodel.UnitId + "'" : "") + (rjtmodel.SystemId != null ? " and ph.System='" + rjtmodel.SystemId + "'" : "") + " " +
                     " group by pl.ToDeliveryNo order by pl.PickedOn");

            dt = objMas.GetDataTable(Qry);
            return dt;
        }

        [HttpPost]
        [Route("action")]
        [Route("Report/CFADispatchLoadGrid")]
        public ActionResult CFADispatchLoadGrid(CfaDispatchModel rjtmodel)
        {
            try
            {
                DataTable dt = new DataTable();

                dt = CFADispatch_GetQry(0, rjtmodel);
                if (dt == null)
                {
                    objMas.PrintLog("A[2,42]", objMas.DBErrBuf);
                    return Content("Report Data Fetch Failed");
                }


                var emp = (from DataRow row in dt.Rows
                           select new
                           {
                               DispDate = row.Field<string>("Dispatch Date").ToString(),
                               Shipment = row.Field<string>("Shipment Number").ToString(),
                               Delivery = row.Field<string>("Delivery Number").ToString(),
                               FromPlant = row.Field<string>("From Plant").ToString(),
                               ReceivePlant = row.Field<string>("Receiving Plant").ToString(),
                               MaterialCode = row.Field<string>("MaterialCode").ToString(),
                               MaterialDesc = row.Field<string>("Material Name").ToString(),
                               Batch = row.Field<string>("Batch").ToString(),
                               DispatchQty = row.Field<Int64>("Dispatch_QTY").ToString(),
                           }).ToList();

                return Json(emp, JsonRequestBehavior.AllowGet);

            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,15]", Ex.Message);
                return null;
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Report/CFADispatchGenReport")]
        public ActionResult CFADispatchGenReport(CfaDispatchModel rjtmodel)
        {
            try
            {
                //int ret = 0;
                //DataTable dt = new DataTable();

                //dt = CFADispatch_GetQry(1, rjtmodel);
                //if (dt == null)
                //{
                //    objMas.PrintLog("[A[2,43]", objMas.DBErrBuf);
                //    return Json(new { Success = 0, Message = "Report Data Fetch Failed", Data = "" });
                //}

                //int[] DType = Enumerable.Range(0, dt.Columns.Count).Select(n => MasterLogic.RPTSTYLE_STRING_SHORT).ToArray();

                //ret = objMas.GenerateReport(rjtmodel.DwnldFmt, 1, 0, 1, "CFADispatch", "CFA Dispatch Report", DType, null, null, rjtmodel.FromDate, rjtmodel.ToDate, null, null, null, null, dt, null);
                //if (ret == 1)
                //{
                //    objMas.PrintLog("[A[2,44]", objMas.DBErrBuf);
                //    return Json(new { Success = 1, Message = "Success", Data = objMas.DBErrBuf });
                //}


                int ret = 0;
                DataTable dt = new DataTable();

                DataColumn column = new DataColumn("SlNo", typeof(int));
                column.DataType = System.Type.GetType("System.Int32");
                column.AutoIncrement = true;
                column.AutoIncrementSeed = 1;
                column.AutoIncrementStep = 1;


                DataTable dtHeader = new DataTable();
                string[] AddlnData = new string[6];
                int[] AddlnHdrCols = new int[12];
                float[] ColWidth;

                dt = CFADispatch_GetQry(1, rjtmodel);

                DataColumn Col = dt.Columns.Add("SlNo", typeof(int));
                Col.SetOrdinal(0);// to put the column in position 0;

                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["SlNo"] = i + 1;
                if (dt == null)
                {
                    objMas.PrintLog("A[2,5]", objMas.DBErrBuf);
                    return Json(new { Success = 0, Message = "Report Data Fetch Failed", Data = "" });
                }
                ColWidth = new float[10] { 2f, 5f, 5f, 5f, 5f, 5f, 5f, 5f, 5f, 5f };

                PdfPTable pdfTblHdl = new PdfPTable(dt.Columns.Count);
                int[] DType = Enumerable.Range(0, dt.Columns.Count).Select(n => MasterLogic.RPTSTYLE_STRING_SHORT).ToArray();
                DType[0] = MasterLogic.RPTSTYLE_INT;
                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                ret = objMas.GenerateReport(rjtmodel.DwnldFmt, 1, 0, 0, "CFADispatch", "CFA Dispatch Report", DType, null, null, rjtmodel.FromDate,
                                    rjtmodel.ToDate, AddlnData, null, null, null, dt, ColWidth);
                if (ret == 1)
                {
                    objMas.PrintLog("A[2,6]", objMas.DBErrBuf);
                    return Json(new { Success = 1, Message = "Success", Data = objMas.DBErrBuf });
                }
                objMas.PrintLog("[A[2,45]", objMas.DBErrBuf);
                return Json(new { Success = 0, Message = "Failed", Data = objMas.DBErrBuf });
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[2,16]: ", Ex.Message);
                return null;
            }
        }
    }
}