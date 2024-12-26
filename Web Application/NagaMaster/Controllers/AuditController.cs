using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NagaMaster.Models.Audit;
using OfficeOpenXml;

namespace NagaMaster.Controllers
{
    public class AuditController : Controller
    {
        MasterLogic objMas = new MasterLogic();
        AppDet objApp = new AppDet();
        LoginController objMas1 = new LoginController();
        const string PAGEDESC = "AuditController";


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
        // GET: Audit
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult StockAudit()
        {
            try
            {
                var model = new AuditModel();
                DataTable dt = new DataTable();

                ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.Stock_Audit, Request.Cookies["UserId"].Value.ToString());
                model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());

                dt = objMas.GetDataTable("Select PlantId,PlantName from Plant_master where Status in (1,2) Order by PlantName");
                if (dt == null)
                {
                    objMas.PrintLog("A[2,39]", objMas.DBErrBuf);
                    return View(model);
                }


                model.PlantDetails = (from DataRow a in dt.Rows
                                      select new SelectListItem { Text = a["PlantName"].ToString(), Value = a["PlantId"].ToString() }).ToList();

                return View(model);
            }
            catch(Exception Ex)
            {
                return Json(new ResponseHelper { Success = 0, Message = "C[1,1]: " + Ex.Message, Data = null });
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Audit/StockAudit_Get")]
        public ActionResult StockAudit_Get(AuditModel data)
        {
            try
            {
                DataTable dt = new DataTable();

                dt = objMas.GetDataTable("Call GetStockAudit('" + data.PlantId + "')");
                if (dt == null)
                    return Json(new { Success = 0, Message = "Data Fetch Failed", Data = "" });
                List<AuditModel_Grid> nbGrid = new List<AuditModel_Grid>();
                nbGrid = (from DataRow row in dt.Rows
                          select new AuditModel_Grid
                          {
                              Plant = row["Plant"].ToString(),
                              CrateBarcode = row["CrateBarcode"].ToString(),
                              Barcode = row["Barcode"].ToString(),
                              BatchNo = row["BatchNo"].ToString(),
                              MaterialCode = row["MaterialCode"].ToString(),
                              MaterialDesc = row["MaterialDesc"].ToString(),
                              sysPlant = row["sysPlant"].ToString(),
                              sysCrate = row["sysCrate"].ToString(),
                              sysBarcode = row["sysBarcode"].ToString(),
                              sysBatch = row["sysBatch"].ToString(),
                              sysMCode = row["sysMCode"].ToString(),
                              sysMDesc = row["sysMDesc"].ToString(),
                              Flags = Convert.ToInt32(row["Flags"]),

                          }).ToList();
                if (nbGrid == null)
                {
                    objMas.PrintLog("GridNull:", objMas.DBErrBuf);
                    return null;
                }
                else if (nbGrid.Count == 0)
                    objMas.PrintLog("Grid Count == 0:", objMas.DBErrBuf);
                return Json(nbGrid, JsonRequestBehavior.AllowGet);
            }
            catch(Exception Ex)
            {
                return Json(new ResponseHelper { Success = 0, Message = "C[1,1]: " + Ex.Message, Data = null });
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("Master/StockAudit_Dwnld")]
        public ActionResult StockAudit_Dwnld(string Plant)
        {
            try
            {
                DataTable dt = new DataTable();

                string RptName = "";

                dt = objMas.GetDataTable("Call GetStockAudit('" + Plant + "')");

                if (dt == null)
                    return Json(new { Success = 0, Message = "Report Generate Failed", Data = "" });
                else if (dt.Rows.Count == 0)
                    return Json(new { Success = 0, Message = "No Data Found For this Selection", Data = "" });
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                using (ExcelPackage excel = new ExcelPackage())
                {
                    dt.Columns.Add("#", typeof(int));
                    dt.Columns.Add("Status", typeof(string));

                    // Populate the serial numbers (assuming you want 1-based numbering)
                    for (int i = 0; i < dt.Rows.Count; i++)
                        dt.Rows[i]["#"] = i + 1;
                    dt.Columns["#"].SetOrdinal(0);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        int FlgVal = Convert.ToInt32(dt.Rows[i]["Flags"]);

                        dt.Rows[i]["Status"] = FlgVal == 0 ? "No Issues" : FlgVal == 1 ? "Crate Mismatch" : FlgVal == 3 ? "Physical Missing" : FlgVal == 2 ? "System Missing" : "";
                    }
                    dt.Columns["Status"].SetOrdinal(14);
                    dt.Columns.Remove("Flags");



                    excel.Workbook.Worksheets.Add("MyAudit");
                    var worksheet = excel.Workbook.Worksheets["MyAudit"];
                    worksheet.Cells["A1"].LoadFromDataTable(dt, true);

                    using (var range = worksheet.Cells[2, 1, dt.Rows.Count + 1, 1]) // Assuming data starts from row 2
                    {
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    }
                    worksheet.Column(1).Width = 5;
                    worksheet.Column(2).Width = 25;
                    worksheet.Column(3).Width = 15;
                    worksheet.Column(4).Width = 20;
                    worksheet.Column(5).Width = 20;
                    worksheet.Column(6).Width = 20;
                    worksheet.Column(7).Width = 30;
                    worksheet.Column(8).Width = 20;
                    worksheet.Column(9).Width = 20;
                    worksheet.Column(10).Width = 20;
                    worksheet.Column(11).Width = 20;
                    worksheet.Column(12).Width = 20;
                    worksheet.Column(13).Width = 30;
                    worksheet.Column(14).Width = 20;
                    using (var range = worksheet.Cells[worksheet.Dimension.Address])
                    {
                        range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    }
                    using (var range = worksheet.Cells[1, 1, 1, dt.Columns.Count]) // Assuming the header row is the first row
                    {
                        range.Style.Font.Bold = true;
                    }

                    // Save the Excel file to a MemoryStream
                    using (var memoryStream = new MemoryStream())
                    {
                        excel.SaveAs(memoryStream);
                        memoryStream.Position = 0;
                        RptName = "Audit_";
                        RptName += RptName = System.DateTime.UtcNow.ToString("yyMMddHHmmss") + ".xlsx";

                        using (FileStream file = new FileStream(Server.MapPath("~/Reports/" + RptName),
                            FileMode.Create, System.IO.FileAccess.Write)) memoryStream.WriteTo(file);
                     
                        return Json(new { Success = 1, Message = objMas.DBErrBuf, Data = RptName });
                    }
                }
            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = 0, Message = "C[1,1]: " + Ex.Message, Data = null });
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("Master/StockAudit_Adjust")]
        public ActionResult StockAudit_Adjust(string Plant)
        {
            try
            {
                int Ret = 0;

                if(Plant == "" || Plant == null)
                    return Json(new { Success = 0, Message = "Select Valid Plant", Data = "" });


                Ret = objMas.ExecQry("Call StockAdjust('" + Plant + "')");

                if(Ret == -1) {
                    objMas.PrintLog("StockAudit_Adjust", objMas.DBErrBuf);
                    return Json(new { Success = 0, Message = "DB Error Contact System Admin", Data = "" });
                }
                else
                    return Json(new { Success = 1, Message = "Success", Data = "" });
            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = 0, Message = "C[1,1]: " + Ex.Message, Data = null });
            }
        }
    }
}