using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NagaMaster.Models.StockImport;
using System.Data;
using OfficeOpenXml;
using System.IO;
using System.Web.Script.Serialization;

namespace NagaMaster.Controllers
{
    public class StockImportController : Controller
    {
        // GET: StockImport
        MasterLogic objMas = new MasterLogic();
        LoginController objMas1 = new LoginController();
        //public ActionResult Index()
        //{
        //    return View();
        //}

        public ActionResult NoBarcodeData()
        {
            ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.Import_WithoutBarcode, Request.Cookies["UserId"].Value.ToString());
            var model = new NoBarcodeModel();
            model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());
            return View(model);
        }
        [HttpGet]
        [Route("action")]
        [Route("StockImport/GetWithoutBarcodeData")]
        public ActionResult GetWithoutBarcodeData()
        {
            try
            {
                DataTable dt = new DataTable();
                dt = objMas.GetDataTable("select a.MaterialCode,a.MaterialDesc,a.BatchNo,a.Barcode,a.CrateBarcode,a.PDNDate,a.PDNCnt,a.PlantId,b.PlantName " +
                                        " from stock_nobarcode a, plant_master b where a.PlantId=b.PlantId");
                if(dt == null)
                {
                    objMas.PrintLog("A[4,1]", objMas.DBErrBuf);
                    return null;
                }
                List<NoBarcodeModel> nbGrid = new List<NoBarcodeModel>();
                nbGrid = (from DataRow row in dt.Rows
                          select new NoBarcodeModel
                          {
                              PlantId = row["PlantName"].ToString(),
                              MaterialCode = row["MaterialCode"].ToString(),
                              MaterialDesc = row["MaterialDesc"].ToString(),
                              BatchNo = row["BatchNo"].ToString(),
                              Barcode = row["Barcode"].ToString(),
                              CrateBarcode = row["CrateBarcode"].ToString(),
                              PDNDate = row["PDNDate"].ToString(),
                              PDNCnt = Convert.ToDecimal(row["PDNCnt"]),
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
                objMas.PrintLog("C[4,1]", Ex.Message);
                return null;
            }
            
        }

        [HttpPost]
        [Route("action")]
        [Route("/StockImport/GetNoBarcodeDataImport")]
        public ActionResult GetNoBarcodeDataImport(HttpPostedFileBase excelfile)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                string filePath = Server.MapPath("~/ExcelFile/" + excelfile.FileName);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
                excelfile.SaveAs(filePath);
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    int worksheetCount = package.Workbook.Worksheets.Count;
                    int index = 0;

                    ExcelWorksheet worksheet = package.Workbook.Worksheets[index];
                    ExcelRangeBase range = worksheet.Cells[worksheet.Dimension.Start.Row, worksheet.Dimension.Start.Column, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column];

                    int rowCount = range.Rows;
                    int colCount = range.Columns;
                    List<NoBarcodeModel> listItem = new List<NoBarcodeModel>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        NoBarcodeModel entProduct = new NoBarcodeModel();

                        entProduct.PlantId = worksheet.Cells[row, 1].Value.ToString();
                        entProduct.MaterialCode = worksheet.Cells[row, 2].Value.ToString();
                        entProduct.MaterialDesc = worksheet.Cells[row, 3].Value.ToString();
                        entProduct.BatchNo = worksheet.Cells[row, 4].Value.ToString();
                        entProduct.Barcode = worksheet.Cells[row, 5].Value.ToString();
                        entProduct.PDNDate = worksheet.Cells[row, 6].Value.ToString();
                        entProduct.PDNCnt = Convert.ToDecimal(worksheet.Cells[row, 7].Value.ToString());
                        entProduct.CrateBarcode = worksheet.Cells[row, 8].Value.ToString();
                        listItem.ToList();
                        listItem.Add(entProduct);
                    }
                    if (listItem == null)
                    {
                        objMas.PrintLog("GridNull:", objMas.DBErrBuf);
                        return null;
                    }
                    else if (listItem.Count == 0)
                        objMas.PrintLog("Grid Count == 0:", objMas.DBErrBuf);
                    return Json(listItem, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[4,2]", Ex.Message);
                return Content(Ex.Message.ToString());
            }
        }



        [HttpPost]
        [Route("[action]")]
        [Route("StockImport/ImportNoBarcodeDataAdd")]
        public ActionResult ImportNoBarcodeDataAdd(IEnumerable<NoBarcodeModel> collection)
        {
            try
            {
                string userid = Request.Cookies["UserId"].Value.ToString();
                Int64 Ret = 0;
                var ItemList = new List<NoBarcodeModel>();
                foreach (var data in collection)
                {
                    if (data.CrateBarcode == "" || data.CrateBarcode == null)
                        continue;
                    Ret = objMas.ExecQry("insert into stock_nobarcode (PlantId,MaterialCode,MaterialDesc,Barcode,CrateBarcode,BatchNo,PDNDate,PDNCnt,BalanceQty,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn)" +
                                " values ('" + data.PlantId + "','" + data.MaterialCode+ "','" + data.MaterialDesc + "','" + data.Barcode + "','" + data.CrateBarcode + "','" + data.BatchNo+ "','" + data.PDNDate + "','" + 
                                 data.PDNCnt + "','"+ data.PDNCnt + "','" + userid + "',now(),'" + userid + "',now())");
                    if (Ret != 1)
                    {
                        objMas.PrintLog("A[4,3]", objMas.DBErrBuf);
                        return Json(new ResponseHelper { Success = 0, Message = objMas.DBErrBuf, Data = null });
                    }
                        
                }
                if (Ret == -1)
                {
                    objMas.PrintLog("A[4,4]", objMas.DBErrBuf);
                    return Json(new ResponseHelper { Success = 0, Message = "DB Issue Contact System Admin", Data = null });
                }
                    
                else if (Ret == 0)
                {
                    objMas.PrintLog("A[4,5]", objMas.DBErrBuf);
                    return Json(new ResponseHelper { Success = 0, Message = "Failed", Data = null });
                }
                    
                else
                {
                    objMas.PrintLog("A[4,6]", "Added Successfully");
                    return Json(new ResponseHelper { Success = 1, Message = "Items Added", Data = null });
                }
                    
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[4,3]", Ex.Message);
                return Content(Ex.Message.ToString());
            }

        }


        public ActionResult BarcodeWithData()
        {
            var model = new BarcodeWithDataModel();
            var Plants = objMas.GetDataTable("select * from plant_master");
            model.PlantDetail = (from DataRow a in Plants.Rows
                                  select new SelectListItem { Text = a["PlantId"].ToString(), Value = a["PlantId"].ToString() }).ToList();
            


            ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.Import_WithBarcode, Request.Cookies["UserId"].Value.ToString());
            
            model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());
            return View(model);
        }

        [HttpGet]
        [Route("action")]
        [Route("StockImport/GetBarcodeWithData")]
        public ActionResult GetBarcodeWithData(BarcodeWithDataModel Plant)
        {
            try
            {
                string Qry = "";
                DataTable dt = new DataTable();
                    
                Qry=("SELECT stockcfagrn_details.GRNNO ,date_format(stockcfagrn_details.GRNDate,'%Y-%m-%d %h:%m:%s') as GRNDate,stockcfagrn_details.FromShipmentNo," +
                    " stockcfagrn_details.FromDeliveryNo,stockcfagrn_details.FromPlant, stockcfagrn_details.ReceivingPlant, date_format(stockcfagrn_details.PDNDate," +
                    " '%Y-%m-%d %h:%m:%s') as PDNDate, stockcfagrn_details.MaterialCode,stockcfagrn_details.Barcode,stockcfagrn_details.CrateBarcode," +
                    " stockcfagrn_details.NewCrateBarcode, stockcfagrn_details.RackNo, date_format(stockcfagrn_details.PutawayDate,'%Y-%m-%d %h:%m:%s') as PutawayDate," +
                    " stockcfagrn_details.PutAwayFlag,stockcfagrn_details.Weight,stockcfagrn_details.BatchNo FROM stockcfagrn_details  LEFT JOIN plant_master as" +
                    " PlantTable1 ON PlantTable1.PlantId = stockcfagrn_details.FromPlant  LEFT JOIN plant_master as PlantTable2 ON PlantTable2.PlantId = " +
                    " stockcfagrn_details.ReceivingPlant");


                //if (Plant.PlantId != null)
                //    Qry += " Where stockcfagrn_details.ReceivingPlant='" + Plant.PlantId + "'";
                dt = objMas.GetDataTable(Qry);
                
                if (dt == null)
                {
                    objMas.PrintLog("A[4,7]", objMas.DBErrBuf);
                    return null;
                }
                List<BarcodeWithDataModel> nbGrid = new List<BarcodeWithDataModel>();
                nbGrid = (from DataRow row in dt.Rows
                          select new BarcodeWithDataModel
                          {
                              GRNNO = row["GRNNO"].ToString(),
                              GRNDate = row["GRNDate"].ToString(),
                              FromShipmentNo = row["FromShipmentNo"].ToString(),
                              FromDeliveryNo = row["FromDeliveryNo"].ToString(),
                              FromPlant = row["FromPlant"].ToString(),
                              ReceivingPlant = row["ReceivingPlant"].ToString(),
                              PDNDate = row["PDNDate"].ToString(),
                              MaterialCode = row["MaterialCode"].ToString(),
                              Barcode = row["Barcode"].ToString(),
                              CrateBarcode = row["CrateBarcode"].ToString(),
                              NewCrateBarcode = row["NewCrateBarcode"].ToString(),
                              RackNo = row["RackNo"].ToString(),
                              Weight = Convert.ToDecimal(row["Weight"]),
                              BatchNo = row["BatchNo"].ToString(),
                              
                          }).ToList();
                if(nbGrid == null)
                {
                    objMas.PrintLog("GridNull:", objMas.DBErrBuf);
                    return null;
                }
                else if(nbGrid.Count == 0)
                    objMas.PrintLog("Grid Count == 0:", objMas.DBErrBuf);
                return Json(nbGrid, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[4,4]", Ex.Message);
                return null;
            }

        }


        [HttpPost]
        [Route("action")]
        [Route("StockImport/GetBarcodeWithDatawithPlant")]
        public ActionResult GetBarcodeWithDatawithPlant(BarcodeWithDataModel Plant)
        {
            try
            {
                string Qry = "";
                DataTable dt = new DataTable();

                Qry = ("SELECT stockcfagrn_details.GRNNO ,date_format(stockcfagrn_details.GRNDate,'%Y-%m-%d %h:%m:%s') as GRNDate,stockcfagrn_details.FromShipmentNo," +
                    " stockcfagrn_details.FromDeliveryNo,stockcfagrn_details.FromPlant, stockcfagrn_details.ReceivingPlant, date_format(stockcfagrn_details.PDNDate," +
                    " '%Y-%m-%d %h:%m:%s') as PDNDate, stockcfagrn_details.MaterialCode,stockcfagrn_details.Barcode,stockcfagrn_details.CrateBarcode," +
                    " stockcfagrn_details.NewCrateBarcode, stockcfagrn_details.RackNo, date_format(stockcfagrn_details.PutawayDate,'%Y-%m-%d %h:%m:%s') as PutawayDate," +
                    " stockcfagrn_details.PutAwayFlag,stockcfagrn_details.Weight,stockcfagrn_details.BatchNo FROM stockcfagrn_details  LEFT JOIN plant_master as" +
                    " PlantTable1 ON PlantTable1.PlantId = stockcfagrn_details.FromPlant  LEFT JOIN plant_master as PlantTable2 ON PlantTable2.PlantId = " +
                    " stockcfagrn_details.ReceivingPlant");

                if (Plant.PlantId != null)
                    Qry += " Where stockcfagrn_details.ReceivingPlant='" + Plant.PlantId + "'";
                dt = objMas.GetDataTable(Qry);
                if (dt == null)
                {
                    objMas.PrintLog("A[4,15]", objMas.DBErrBuf);
                    return null;
                }
                List<BarcodeWithDataModel> nbGrid = new List<BarcodeWithDataModel>();
                nbGrid = (from DataRow row in dt.Rows
                          select new BarcodeWithDataModel
                          {
                              GRNNO = row["GRNNO"].ToString(),
                              GRNDate = row["GRNDate"].ToString(),
                              FromShipmentNo = row["FromShipmentNo"].ToString(),
                              FromDeliveryNo = row["FromDeliveryNo"].ToString(),
                              FromPlant = row["FromPlant"].ToString(),
                              ReceivingPlant = row["ReceivingPlant"].ToString(),
                              PDNDate = row["PDNDate"].ToString(),
                              MaterialCode = row["MaterialCode"].ToString(),
                              Barcode = row["Barcode"].ToString(),
                              CrateBarcode = row["CrateBarcode"].ToString(),
                              NewCrateBarcode = row["NewCrateBarcode"].ToString(),
                              RackNo = row["RackNo"].ToString(),
                              Weight = Convert.ToDecimal(row["Weight"]),
                              BatchNo = row["BatchNo"].ToString(),

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
            catch (Exception Ex)
            {
                objMas.PrintLog("C[4,5]", Ex.Message);
                return null;
            }

        }

        [HttpPost]
        [Route("action")]
        [Route("StockImport/BarcodeDataGenExcel")]
        public ActionResult BarcodeDataGenExcel(BarcodeWithDataModel data)
        {
            try
            {
                string Qry = "";
                // Create a DataTable and fill it with data
                DataTable dataTable = new DataTable();
                Qry = ("select GRNNO,date_format(GRNDate,'%Y-%m-%d %h:%m:%s') as GRNDate,FromShipmentNo,FromDeliveryNo,FromPlant,ReceivingPlant," +
                                " date_format(PDNDate,'%Y-%m-%d %h:%m:%s') as PDNDate,MaterialCode,Barcode,CrateBarcode,NewCrateBarcode,RackNo,Weight,BatchNo from stockcfagrn_details");

                if (data.PlantId != null)
                    Qry += " Where ReceivingPlant='" + data.PlantId + "'";
                dataTable = objMas.GetDataTable(Qry);
                if (dataTable == null)
                {
                    objMas.PrintLog("A[4,8]", objMas.DBErrBuf);
                    return null;
                }
                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                // Create a new Excel package
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("MyData");
                    var worksheet = excel.Workbook.Worksheets["MyData"];
                    worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);


                    // Save the Excel file to a MemoryStream
                    using (var memoryStream = new MemoryStream())
                    {
                        excel.SaveAs(memoryStream);
                        memoryStream.Position = 0;
                        using (FileStream file = new FileStream(Server.MapPath("~/Reports/MyExcelFile.xlsx"), FileMode.Create, System.IO.FileAccess.Write))
                            memoryStream.WriteTo(file);
                        return Json(new { Success = 1, Message = "Success", Data = "" });
                    }
                }
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[4,6]: ", Ex.Message);
                return null;
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("StockImport/GetBarcodeWithDataImport")]
        public ActionResult GetBarcodeWithDataImport(HttpPostedFileBase excelfile)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                string filePath = Server.MapPath("~/ExcelFile/" + excelfile.FileName);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
                excelfile.SaveAs(filePath);
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    int worksheetCount = package.Workbook.Worksheets.Count;
                    int index = 0;

                    ExcelWorksheet worksheet = package.Workbook.Worksheets[index];
                    ExcelRangeBase range = worksheet.Cells[worksheet.Dimension.Start.Row, worksheet.Dimension.Start.Column, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column];

                    int rowCount = range.Rows;
                    int colCount = range.Columns;
                    List<BarcodeWithDataModel> listItem = new List<BarcodeWithDataModel>();

                    for (int row = 2; row <= rowCount; row++)
                    {
                        BarcodeWithDataModel entProduct = new BarcodeWithDataModel();

                        entProduct.GRNNO = worksheet.Cells[row, 1].Value.ToString();
                        entProduct.GRNDate = worksheet.Cells[row, 2].Value.ToString();
                        entProduct.FromShipmentNo = worksheet.Cells[row, 3].Value.ToString();
                        entProduct.FromDeliveryNo = worksheet.Cells[row, 4].Value.ToString();
                        entProduct.FromPlant = worksheet.Cells[row, 5].Value.ToString();
                        entProduct.ReceivingPlant = worksheet.Cells[row, 6].Value.ToString();
                        entProduct.PDNDate = worksheet.Cells[row, 7].Value.ToString();
                        entProduct.MaterialCode = worksheet.Cells[row, 8].Value.ToString();
                        entProduct.Barcode = worksheet.Cells[row, 9].Value.ToString();
                        entProduct.CrateBarcode = worksheet.Cells[row, 10].Value.ToString();
                        entProduct.NewCrateBarcode = worksheet.Cells[row, 11].Value.ToString();
                        entProduct.RackNo = worksheet.Cells[row, 12].Value.ToString();
                        entProduct.Weight = Convert.ToDecimal(worksheet.Cells[row, 13].Value.ToString());
                        entProduct.BatchNo = worksheet.Cells[row, 14].Value.ToString();

                        listItem.ToList();
                        listItem.Add(entProduct);

                    }
                    if (listItem == null)
                    {
                        objMas.PrintLog("GridNull:", objMas.DBErrBuf);
                        return null;
                    }
                    else if (listItem.Count == 0)
                        objMas.PrintLog("Grid Count == 0:", objMas.DBErrBuf);
                    return Json(listItem, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[4,7]", Ex.Message);
                return Content(Ex.Message.ToString());
            }


        }

        [HttpPost]
        [Route("[action]")]
        [Route("/StockImport/ImportBarcodeWithDataAdd")]
        public ActionResult ImportBarcodeWithDataAdd(IEnumerable<BarcodeWithDataModel> collection)
        {
            try
            {
                string userid = Request.Cookies["UserId"].Value.ToString();
                Int64 Ret = 0;
                var ItemList = new List<BarcodeWithDataModel>();
                foreach (var data in collection)
                {
                    if (data.MaterialCode == "" || data.MaterialCode == null)
                        continue;
                    Ret = objMas.ExecQry("insert into cfagrn_details(GRNNO,GRNDate,FromShipmentNo,FromDeliveryNo,FromPlant,ReceivingPlant,PDNDate," +
                                    " MaterialCode,Barcode,CrateBarcode,NewCrateBarcode,RackNo,PutawayDate,PutAwayFlag,Weight,BatchNo,CreatedBy,CreatedOn) values(" +
                                    " '" + data.GRNNO + "','" + data.GRNDate + "','" + data.FromShipmentNo + "','" + data.FromDeliveryNo + "','" + data.FromPlant + "','" + data.ReceivingPlant + "'," +
                                    " '" + data.PDNDate + "','" + data.MaterialCode + "','" + data.Barcode + "','" + data.CrateBarcode + "',''," +
                                    " '" + data.RackNo + "',now(),'1','" + data.Weight + "','" + data.BatchNo + "','"+userid+"',now())");

                    if (Ret != 1)
                    {
                        objMas.PrintLog("A[4,10]", objMas.DBErrBuf);
                        return Json(new ResponseHelper { Success = 0, Message = objMas.DBErrBuf, Data = null });
                    }
                    //Ret = objMas.ExecQry("delete from stockcfagrn_details where Barcode = '" + data.Barcode + "'");
                    //if (Ret != 1)
                    //{
                    //    objMas.PrintLog("A[4,11]", objMas.DBErrBuf);
                    //    return Json(new ResponseHelper { Success = 0, Message = objMas.DBErrBuf, Data = null });
                    //}

                }
                if (Ret == -1)
                {
                    objMas.PrintLog("A[4,12]", objMas.DBErrBuf);
                    return Json(new ResponseHelper { Success = 0, Message = "DB Issue Contact System Admin", Data = null });
                }
                    
                else if (Ret == 0)
                {
                    objMas.PrintLog("A[4,13]", objMas.DBErrBuf);
                    return Json(new ResponseHelper { Success = 0, Message = "Failed", Data = null });
                }
                    
                else
                {
                    objMas.PrintLog("A[4,14]", "Added Successfully");
                    return Json(new ResponseHelper { Success = 1, Message = "Items Added", Data = null });
                }
                    
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[4,8]", Ex.Message);
                return Content(Ex.Message.ToString());
            }

        }
    }
}