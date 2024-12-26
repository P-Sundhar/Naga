using NagaMaster.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using NagaMaster.Models.User;
//using Excel = Microsoft.Office.Interop.Excel;
using static NagaMaster.Models.ItemModel;
using System.IO;
using OfficeOpenXml;

namespace NagaMaster.Controllers
{
    
    public class HomeController : Controller
    {
        MasterLogic objMas = new MasterLogic();
        LoginController objMas1 = new LoginController();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ItemMaster()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.Item_Master, Request.Cookies["UserId"].Value.ToString());
            var model = new ItemModel();
            model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());
            return View(model);
        }
       
        [HttpGet]
        [Route("[action]")]
        [Route("Home/GetItems")]
        public JsonResult GetItems()
            {
            try { 
           
                DataTable dt = new DataTable();
                dt = objMas.GetDataTable("Select * from material_master");
                if (dt == null)
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
                List<ItemModel> emp = new List<ItemModel>();
                emp = (from DataRow row in dt.Rows

                       select new ItemModel
                       {
                           Id = Convert.ToInt32(row["Id"].ToString()),
                           MaterialCode = row["MaterialCode"].ToString(),
                           MaterialDesc = row["MaterialDesc"].ToString(),
                           BUOMName = row["BUOMName"].ToString(),
                           ItemShortName1 = row["ItemShortName1"].ToString(),
                           ItemShortName2 = row["ItemShortName2"].ToString(),
                           Denominator = Convert.ToInt32(row["Denominator"].ToString()),
                           AUOMName = row["AUOMName"].ToString(),
                           Numerator = Convert.ToInt32(row["Numerator"].ToString()),
                           BoxQuantityInCrate = Convert.ToInt32(row["BoxQuantityInCrate"].ToString()),
                           ShelflifeMin = Convert.ToInt32(row["ShelflifeMin"].ToString()),
                           ShelflifeMax = Convert.ToInt32(row["ShelflifeMax"].ToString()),
                           PeriodIndicatorSLED = row["PeriodIndicatorSLED"].ToString(),
                           ProdGroupCode = row["ProdGroupCode"].ToString(),
                           ProdGroupDesc = row["ProdGroupDesc"].ToString(),
                           SizeInInchCode = row["SizeInInchCode"].ToString(),
                           SizeInInchDesc = row["SizeInInchDesc"].ToString(),
                           GrossWgt = Convert.ToDecimal(row["GrossWgt"].ToString()),
                           NetWgt = Convert.ToDecimal(row["NetWgt"].ToString()),
                           MinTolerance = Convert.ToDecimal(row["MinTolerance"].ToString()),
                           MaxTolerance = Convert.ToDecimal(row["MaxTolerance"].ToString()),
                           Status = Convert.ToInt32(row["Status"].ToString()),
                       }).ToList();
                return Json(emp, JsonRequestBehavior.AllowGet);
            }
            catch(Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }
        public JsonResult AddItems(ItemModel collection)
        {
            Int64 Itm = 0;
            try
            {
                Itm = objMas.GetTableCount("material_master where MaterialCode='" + collection.MaterialCode + "'");
                if(Itm >= 1)
                    return Json(new ResponseHelper { Success = 0, Message = "MaterialCode Already Exists", Data = null });

                Itm = objMas.GetTableCount("material_master where MaterialDesc='" + collection.MaterialDesc + "' ");
                if (Itm >= 1)
                    return Json(new ResponseHelper { Success = 0, Message = "MaterialDesc Already Exists", Data = null });

                Itm = objMas.ExecQry("insert into material_master(MaterialCode, MaterialDesc, BUOMName, ItemShortName1, ItemShortName2, Denominator, " +
                    "AUOMName, Numerator, BoxQuantityInCrate, ShelflifeMin, ShelflifeMax, PeriodIndicatorSLED, ProdGroupCode, " +
                    "ProdGroupDesc, SizeInInchCode, SizeInInchDesc, GrossWgt, NetWgt, MinTolerance, MaxTolerance, Status, CreatedBy, CreatedOn, " +
                    "ModifiedBy, ModifiedOn) values('"+collection.MaterialCode+"','"+collection.MaterialDesc+"','"+collection.BUOMName+"'," +
                    "'"+collection.ItemShortName1+"','"+ collection.ItemShortName2+ "','"+collection.Denominator+"','"+collection.AUOMName+"','"+collection.Numerator+"'," +
                    "'"+collection.BoxQuantityInCrate+"','"+collection.ShelflifeMin+"','"+collection.ShelflifeMax+"','"+collection.PeriodIndicatorSLED+"'," +
                    "'"+collection.ProdGroupCode+"','"+collection.ProdGroupDesc+"','"+collection.SizeInInchCode+"','"+collection.SizeInInchDesc+"','"+collection.GrossWgt+"'," +
                    "'"+collection.NetWgt+"','"+collection.MinTolerance+"','"+collection.MaxTolerance+ "','" + collection.Status + "'," +
                    "'"+ Request.Cookies["EmpId"].Value.ToString() + "',now(),'" + Request.Cookies["EmpId"].Value.ToString() + "',now())");

                if(Itm >=1)
                    return Json(new ResponseHelper { Success = 1, Message = "Item Added", Data = collection });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Save Failed !" + objMas.DBErrBuf, Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }
        [HttpPost]
        [Route("[action]")]
        [Route("Home/EditItems")]
        public JsonResult EditItems(ItemModel collection)
        {
            Int64 Itm = 0;
            try
            {
                Itm = objMas.ExecQry("update material_master set MaterialCode = '" + collection.MaterialCode + "', MaterialDesc = '" + collection.MaterialDesc + "'," +
                    "BUOMName = '" + collection.BUOMName + "', ItemShortName1 = '" + collection.ItemShortName1 + "', ItemShortName2 = '" + collection.ItemShortName2 + "'," +
                    "Denominator = '" + collection.Denominator + "', AUOMName = '" + collection.AUOMName + "', Numerator = '" + collection.Numerator + "'," +
                    "BoxQuantityInCrate = '" + collection.BoxQuantityInCrate + "', ShelflifeMin = '" + collection.ShelflifeMin + "', ShelflifeMax = '" + collection.ShelflifeMax + "'," +
                    "PeriodIndicatorSLED = '" + collection.PeriodIndicatorSLED + "', ProdGroupCode = '" + collection.ProdGroupCode + "', ProdGroupDesc = '" + collection.ProdGroupDesc + "'," +
                    "SizeInInchCode = '" + collection.SizeInInchCode + "', SizeInInchDesc = '" + collection.SizeInInchDesc + "', GrossWgt = '" + collection.GrossWgt + "'," +
                    "NetWgt = '" + collection.NetWgt + "', MinTolerance = '" + collection.MinTolerance + "', MaxTolerance = '" + collection.MaxTolerance + "'," +
                    "Status = '" + collection.Status + "', ModifiedBy = '" + Request.Cookies["EmpId"].Value.ToString() + "', ModifiedOn = now() where Id = '" + collection.Id+"'");
                if (Itm >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "Item Updated", Data = collection });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Update Failed !" + objMas.DBErrBuf, Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }
        [HttpPost]
        [Route("[action]")]
        [Route("/Home/DeleteItems")]
        public JsonResult DeleteItems(Int32 Id)
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.GetTableCount("material_master where Id = '" + Id + "' and Status = 0");
                if(Ret >= 1)
                    return Json(new ResponseHelper { Success = 0, Message = "Already Deleted!" + objMas.DBErrBuf, Data = null });

                Ret = objMas.ExecQry("update material_master set Status = 0 where Id = '" + Id + "'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "Item Deleted" });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Failed To Delete " + objMas.DBErrBuf, Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }

        [HttpPost]
        [Route("[action]")]
        [Route("/Home/GetItemImport")]
        public ActionResult GetItemImport(HttpPostedFileBase excelfile1)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                string filePath = Server.MapPath("~/ExcelFile/" + excelfile1.FileName);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
                excelfile1.SaveAs(filePath);
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    int worksheetCount = package.Workbook.Worksheets.Count;
                    int index = 0;

                    ExcelWorksheet worksheet = package.Workbook.Worksheets[index];
                    ExcelRangeBase range = worksheet.Cells[worksheet.Dimension.Start.Row, worksheet.Dimension.Start.Column, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column];

                    int rowCount = range.Rows;
                    int colCount = range.Columns;
                    List<importModel> listItem = new List<importModel>();
                    for (int row = 2; row <= rowCount; row++)
                    {
                        importModel entProduct = new importModel();
                        entProduct.MaterialCode = worksheet.Cells[row, 1].Value.ToString();
                        entProduct.MaterialCode = worksheet.Cells[row, 1].Text;
                        entProduct.MaterialDesc = worksheet.Cells[row, 2].Text;
                        entProduct.BUOMName = worksheet.Cells[row, 3].Text;
                        entProduct.ItemShortName1 = worksheet.Cells[row, 4].Text;
                        entProduct.ItemShortName2 = worksheet.Cells[row, 5].Text;
                        entProduct.Denominator = Convert.ToInt32(worksheet.Cells[row, 6].Text);
                        entProduct.AUOMName = worksheet.Cells[row, 7].Text;
                        entProduct.Numerator = Convert.ToInt32(worksheet.Cells[row, 8].Text);
                        entProduct.BoxQuantityInCrate = Convert.ToInt32(worksheet.Cells[row, 9].Text);
                        entProduct.ShelflifeMin = Convert.ToInt32(worksheet.Cells[row, 10].Text);
                        entProduct.ShelflifeMax = Convert.ToInt32(worksheet.Cells[row, 11].Text);
                        entProduct.PeriodIndicatorSLED = worksheet.Cells[row, 12].Text;
                        entProduct.ProdGroupCode = worksheet.Cells[row, 13].Text;
                        entProduct.ProdGroupDesc = worksheet.Cells[row, 14].Text;
                        entProduct.SizeInInchCode = worksheet.Cells[row, 15].Text;
                        entProduct.SizeInInchDesc = worksheet.Cells[row, 16].Text;
                        entProduct.GrossWgt = Convert.ToDecimal(worksheet.Cells[row, 17].Text);
                        entProduct.NetWgt = Convert.ToDecimal(worksheet.Cells[row, 18].Text);
                        entProduct.MinTolerance = Convert.ToDecimal(worksheet.Cells[row, 19].Text);
                        entProduct.MaxTolerance = Convert.ToDecimal(worksheet.Cells[row, 20].Text);
                        listItem.ToList();
                        listItem.Add(entProduct);

                    }

                    return Json(listItem, JsonRequestBehavior.AllowGet);     //new ResponseHelper { Success = 1, Message = "File Uploaded Successfully", Data = entProduct },


                }
            }
            catch (Exception Ex)
            {
                return Content(Ex.Message.ToString());
            }


        }


        [HttpPost]
        [Route("[action]")]
        [Route("Home/ImportAddItem")]
        public ActionResult ImportAddItem(IEnumerable<importModel> collection)
        {
            try
            {
                Int64 Ret = 0;
                var ItemList = new List<importModel>();
                foreach (var data in collection)
                {
                    if (data.MaterialCode == "" || data.MaterialCode == null)
                        continue;
                    Ret = objMas.ExecQry("insert into material_master(MaterialCode, MaterialDesc, BUOMName, ItemShortName1, ItemShortName2, Denominator, " +
                        "AUOMName, Numerator, BoxQuantityInCrate, ShelflifeMin, ShelflifeMax, PeriodIndicatorSLED, ProdGroupCode, " +
                        "ProdGroupDesc, SizeInInchCode, SizeInInchDesc, GrossWgt, NetWgt, MinTolerance, MaxTolerance, CreatedBy, CreatedOn, " +
                        "ModifiedBy, ModifiedOn) values('" + data.MaterialCode + "','" + data.MaterialDesc + "','" + data.BUOMName + "'," +
                        "'" + data.ItemShortName1 + "','" + data.ItemShortName2 + "','" + data.Denominator + "','" + data.AUOMName + "','" + data.Numerator + "'," +
                        "'" + data.BoxQuantityInCrate + "','" + data.ShelflifeMin + "','" + data.ShelflifeMax + "','" + data.PeriodIndicatorSLED + "'," +
                        "'" + data.ProdGroupCode + "','" + data.ProdGroupDesc + "','" + data.SizeInInchCode + "','" + data.SizeInInchDesc + "','" + data.GrossWgt + "'," +
                        "'" + data.NetWgt + "','" + data.MinTolerance + "','" + data.MaxTolerance + "','" + Request.Cookies["EmpId"].Value.ToString() + "',now()," +
                        "'" + Request.Cookies["EmpId"].Value.ToString() + "',now())");
                    if (Ret != 1)
                        return Json(new ResponseHelper { Success = 0, Message = objMas.DBErrBuf, Data = null });
                }
                if (Ret == -1)
                    return Json(new ResponseHelper { Success = 0, Message = "DB Issue Contact System Admin", Data = null });
                else if (Ret == 0)
                    return Json(new ResponseHelper { Success = 0, Message = "Failed", Data = null });
                else
                    return Json(new ResponseHelper { Success = 1, Message = "Items Added", Data = null });
            }
            catch(Exception Ex)
            {
                return Content(Ex.Message.ToString());
            }
            
        }
        
        public ActionResult SettingsMaster()
        {
            try
            {
                Int64 Ret = 0;
                DataTable dt = new DataTable();
                Ret = objMas.GetTableCount("systemuser_master where PlantId ='" + Request.Cookies["PlantId"].Value.ToString() + "' and '" + Request.Cookies["PlantId"].Value.ToString() + "'='9300'");
                if (Ret >= 1)
                {
                    dt = objMas.GetDataTable("select * from plant_master");       //select a.PlantId, b.PlantName from unit_master a, plant_master b where a.PlantId=b.PlantId
                    if (dt == null)
                    {
                        return View(new SettingsModel());
                    }

                    ViewBag.items = (from DataRow row in dt.Rows
                                     select new SettingsModel
                                     {
                                         PlantId = row["PlantId"].ToString(),
                                         PlantName = row["PlantName"].ToString(),
                                     }).ToList();
                }
                else
                {
                    dt = objMas.GetDataTable("Select PlantName from plant_master where PlantId = '" + Request.Cookies["PlantId"].Value.ToString() + "' ");/*Status in (1,2)*/
                    if (dt == null)
                    {
                        return View(new SettingsModel());
                    }
                    ViewBag.items = (from DataRow row in dt.Rows
                                     select new SettingsModel
                                     {
                                         PlantId = Request.Cookies["PlantId"].Value.ToString(),
                                         PlantName = row["PlantName"].ToString(),
                                     }).ToList();
                }
                dt = objMas.GetDataTable("Select UnitId,UnitName from unit_master where Status in (1,2)");
                if (dt == null)
                {
                    return View(new SettingsModel());
                }
                ViewBag.data = (from DataRow row in dt.Rows
                                select new
                                {
                                    UnitId = row["UnitId"].ToString(),
                                    UnitName = row["UnitName"].ToString(),
                                }).ToList();

                ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.COM_Settings, Request.Cookies["UserId"].Value.ToString());

                var model = new SettingsModel();
                model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());
                return View(model);
            }
            catch(Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }

        [HttpGet]
        [Route("action")]
        [Route("Home/GetSettings")]

        public ActionResult GetSettings()
        {
            Int64 Ret = 0;
            try
            {
                DataTable dt = new DataTable();
                Ret = objMas.GetTableCount("systemuser_master where '" + Request.Cookies["PlantId"].Value.ToString() + "' ='9300'");
                if (Ret >= 1)
                {
                    dt = objMas.GetDataTable("Select a.Id,a.ComPort,a.BaudRate,a.DataBits,a.StopBits,a.System,a.PlantId,a.Status,b.UnitId, c.PlantId, c.PlantName " +
                        "from settings a,unit_master b, plant_master c where a.UnitId=b.UnitId and a.PlantId=c.PlantId");

                    if (dt == null)
                    {
                        return Content(objMas.DBErrBuf);
                    }
                    List<SettingsModel> emp = new List<SettingsModel>();
                    emp = (from DataRow row in dt.Rows

                           select new SettingsModel
                           {
                               Id = Convert.ToInt32(row["Id"].ToString()),
                               ComPort = row["ComPort"].ToString(),
                               UnitId = row["UnitId"].ToString(),
                               BaudRate = Convert.ToInt32(row["BaudRate"].ToString()),
                               DataBits = Convert.ToInt32(row["DataBits"].ToString()),
                               StopBits = Convert.ToInt32(row["StopBits"].ToString()),
                               System = row["System"].ToString(),
                               PlantId = row["PlantId"].ToString(),
                               PlantName = row["PlantName"].ToString(),
                               Status = Convert.ToInt32(row["Status"].ToString()),
                           }).ToList();
                    return Json(emp, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    dt = objMas.GetDataTable("Select a.Id,a.ComPort,a.BaudRate,a.DataBits,a.StopBits,a.System,a.PlantId,a.Status,b.UnitId, c.PlantId, c.PlantName, d.OptEdit, d.OptView " +
                        "from settings a,unit_master b, plant_master c, webuser_menu d where a.UnitId=b.UnitId and a.PlantId=c.PlantId " +
                        "and '" + Request.Cookies["PlantId"].Value.ToString() + "'= c.PlantId and '"+Request.Cookies["EdDashBoard"].Value.ToString() +"' != 0");

                    if (dt == null)
                    {
                        return Content(objMas.DBErrBuf);
                    }
                    List<SettingsModel> emp = new List<SettingsModel>();
                    emp = (from DataRow row in dt.Rows

                           select new SettingsModel
                           {
                               Id = Convert.ToInt32(row["Id"].ToString()),
                               ComPort = row["ComPort"].ToString(),
                               UnitId = row["UnitId"].ToString(),
                               BaudRate = Convert.ToInt32(row["BaudRate"].ToString()),
                               DataBits = Convert.ToInt32(row["DataBits"].ToString()),
                               StopBits = Convert.ToInt32(row["StopBits"].ToString()),
                               System = row["System"].ToString(),
                               PlantId = row["PlantId"].ToString(),
                               PlantName = row["PlantName"].ToString(),
                               Status = Convert.ToInt32(row["Status"].ToString()),
                           }).ToList();
                    return Json(emp, JsonRequestBehavior.AllowGet);
                }
                
            }
            catch (Exception Ex)
            {
                return Content(Ex.Message.ToString());
            }


        }

        [HttpPost]
        [Route("action")]
        [Route("Home/AddSettings")]
        public JsonResult AddSettings(SettingsModel collection)
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.GetTableCount("settings where ComPort = '" + collection.ComPort + "'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 0, Message = "ComPort Already Exists", Data = null });

                Ret = objMas.ExecQry("insert into settings (ComPort, BaudRate, DataBits, StopBits, System, UnitId, PlantId,Status) values ('"+collection.ComPort+"'," +
                    "'"+collection.BaudRate + "','"+collection.DataBits+"','"+collection.StopBits+"','"+collection.System+"'," +
                    "'"+collection.UnitId+"','"+collection.PlantId+"','"+collection.Status+"')"); 
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "Settings Added",Data = null });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Save Failed !" + objMas.DBErrBuf, Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }

        [HttpPost]
        [Route("[action]")]
        [Route("Home/EditSettings")]
        public JsonResult EditSettings(SettingsModel collection)
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.ExecQry("update settings set BaudRate = '" + collection.BaudRate + "', DataBits = '" + collection.DataBits + "'," +
                    "StopBits = '" + collection.StopBits + "', System = '" + collection.System + "', UnitId = '" + collection.UnitId + "'," +
                    "Status = '"+collection.Status+ "', ComPort = '" + collection.ComPort + "', PlantId = '"+collection.PlantId+"' where Id = '" + collection.Id+"'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "Settings Updated", Data = null });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Update Failed !" + objMas.DBErrBuf, Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }

        [HttpPost]
        [Route("[action]")]
        [Route("Home/DeleteSettings")]
        public JsonResult DeleteSettings(Int32 Id)
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.GetTableCount("settings where Id = '" + Id + "' and Status = 0");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 0, Message = "Already Deleted!" + objMas.DBErrBuf, Data = null });

                Ret = objMas.ExecQry("update settings set Status = 0 where Id = '" + Id + "'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "Settings Deleted" });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Failed To Delete " + objMas.DBErrBuf, Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }

        public ActionResult UnitMaster()
        {
            try
            {
                Int64 Ret = 0;
                DataTable dt = new DataTable();
                var model = new UnitModel();
                Ret = objMas.GetTableCount("systemuser_master where PlantId ='" + Request.Cookies["PlantId"].Value.ToString() + "' and '" + Request.Cookies["PlantId"].Value.ToString() + "'='9300'");
                if (Ret >= 1)
                {
                    dt = objMas.GetDataTable("select * from plant_master");        //select a.PlantId, b.PlantName from unit_master a, plant_master b where a.PlantId=b.PlantId
                    if (dt == null)
                    {
                        return View(model);
                    }

                    model.PlantDetails = (from DataRow row in dt.Rows
                                        select new SelectListItem
                                        {
                                            Value = row["PlantId"].ToString(),
                                            Text = row["PlantName"].ToString(),
                                        }).ToList();

                    //ViewBag.IsEdit = objMas.GetMenuEdit(Globals.Unit_Master);

                    //model.menu = objMas.LoadMenus();
                    //return View(model);
                }
                else
                {
                    dt = objMas.GetDataTable("Select PlantName from plant_master where PlantId = '" + Request.Cookies["PlantId"].Value.ToString() + "' and Status in (1,2)");/*Status in (1,2)*/
                    if (dt == null)
                    {
                        return View(new UnitModel());
                    }
                    ViewBag.items = (from DataRow row in dt.Rows
                                     select new UnitModel
                                     {
                                         PlantId = Request.Cookies["PlantId"].Value.ToString(),
                                         PlantName = row["PlantName"].ToString(),
                                     }).ToList();
                    
                }
                dt = objMas.GetDataTable("Select SystemId,SystemName from system_master where Status in (1,2)");
                if (dt == null)
                    return View(new UnitModel());

                model.System = (from DataRow row in dt.Rows
                                select new SelectListItem
                                {
                                    Value = row["SystemId"].ToString(),
                                    Text = row["SystemName"].ToString(),

                                }).ToList();
                ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.Unit_Master, Request.Cookies["UserId"].Value.ToString());

                model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());

                return View(model);
            }
            catch(Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
            
        }

        [HttpGet]
        [Route("action")]
        [Route("Home/GetUnits")]

        public ActionResult GetUnits()
        {
            
            Int64 Ret = 0;
            try
            {
                DataTable dt = new DataTable();
                Ret = objMas.GetTableCount("systemuser_master where '" + Request.Cookies["PlantId"].Value.ToString() + "' ='9300'");
                if(Ret>=1)
                {
                    dt = objMas.GetDataTable("Select a.Id,a.UnitId, a.UnitName,a.Systems,a.Status,b.PlantId, b.PlantName from unit_master a, plant_master b " +
                        "where a.PlantId=b.PlantId");
                    if (dt == null)
                    {
                        return Content(objMas.DBErrBuf);
                    }
                    List<UnitModel> emp = new List<UnitModel>();
                    emp = (from DataRow row in dt.Rows

                           select new UnitModel
                           {
                               Id = Convert.ToInt32(row["Id"].ToString()),
                               PlantId = row["PlantId"].ToString(),
                               PlantName = row["PlantName"].ToString(),
                               UnitId = row["UnitId"].ToString(),
                               UnitName = row["UnitName"].ToString(),
                               Systems = row["Systems"].ToString(),
                               Status = Convert.ToInt32(row["Status"].ToString()),
                           }).ToList();
                    return Json(emp, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    dt = objMas.GetDataTable("Select a.Id,a.UnitId, a.UnitName,a.Systems,a.Status,b.PlantId, b.PlantName from unit_master a, plant_master b where a.PlantId=b.PlantId and '" + Request.Cookies["PlantId"].Value.ToString() + "'=b.PlantId");

                    if (dt == null)
                    {
                        return Content(objMas.DBErrBuf);
                    }
                    List<UnitModel> emp = new List<UnitModel>();
                    emp = (from DataRow row in dt.Rows

                           select new UnitModel
                           {
                               Id = Convert.ToInt32(row["Id"].ToString()),
                               PlantId = row["PlantId"].ToString(),
                               PlantName = row["PlantName"].ToString(),
                               UnitId = row["UnitId"].ToString(),
                               UnitName = row["UnitName"].ToString(),
                               Systems = row["Systems"].ToString(),
                               Status = Convert.ToInt32(row["Status"].ToString()),
                           }).ToList();
                    return Json(emp, JsonRequestBehavior.AllowGet);
                }
                
            }
            catch (Exception Ex)
            {
                return Content(Ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("Home/AddUnits")]
        public JsonResult AddUnits(UnitModel collection)
        {
            Int64 Ret = 0;
            //UnitModel collection = new UnitModel();
            try
            {
                Ret = objMas.GetTableCount("unit_master where UnitId = '" + collection.UnitId + "'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 0, Message = "UnitId Already Exists", Data = null });

                Ret = objMas.ExecQry("insert into unit_master (UnitId, UnitName, PlantId, WHouseType, Systems,Status,ServerSendFlag," +
                    "CreatedBy,CreatedOn,ModifiedBy,ModifiedOn) values ('" + collection.UnitId + "'," +
                    "'" + collection.UnitName + "','" + collection.PlantId + "','" + collection.WHouseType + "','" + collection.Systems + "'," +
                    "'" + collection.Status + "','" + collection.ServerSendFlag + "','" + Request.Cookies["EmpId"].Value.ToString() + "',now()," +
                    "'" + Request.Cookies["EmpId"].Value.ToString() + "',now())");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "Unit Added", Data = null });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Save Failed !" + objMas.DBErrBuf, Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }


        [HttpPost]
        [Route("[action]")]
        [Route("Home/EditUnits")]
        public JsonResult EditUnits(UnitModel collection)
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.ExecQry("update unit_master set UnitName = '" + collection.UnitName + "', PlantId = '" + collection.PlantId + "'," +
                    "Systems = '" + collection.Systems + "', Status = '" + collection.Status + "', ModifiedBy = '" + Request.Cookies["EmpId"].Value.ToString() + "'," +
                    " ModifiedOn = now() where UnitId = '" + collection.UnitId + "'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "Unit Updated", Data = null });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Update Failed !" + objMas.DBErrBuf, Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }

        [HttpPost]
        [Route("[action]")]
        [Route("/Home/DeleteUnits")]
        public JsonResult DeleteUnits(Int32 Id)
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.GetTableCount("unit_master where Id = '" + Id + "' and Status = 0");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 0, Message = "Already Deleted!" + objMas.DBErrBuf, Data = null });

                Ret = objMas.ExecQry("update unit_master set Status = 0 where Id = '" + Id + "'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "Unit Deleted" });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Failed To Delete " + objMas.DBErrBuf, Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }

        public ActionResult EmployeeMaster()
        {
            try
            {
                DataTable dt = new DataTable();
                var model = new EmployeeModel();
                dt = objMas.GetDataTable("Select id, RoleName from userroleheader where Status in (1,2)");
                if (dt == null)
                {
                    return View(new EmployeeModel());
                }
                model.RoleDetail = (from DataRow row in dt.Rows
                                 select new SelectListItem
                                 {
                                     Value = row["id"].ToString(),
                                     Text = row["RoleName"].ToString(),
                                 }).ToList();

                ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.Employee_Master, Request.Cookies["UserId"].Value.ToString());
                //model = new ItemModel();
                model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());

                return View(model);
            }
            catch(Exception Ex)
            {
                return Content(Ex.Message.ToString());
            }
            
        }
        [HttpGet]
        [Route("action")]
        [Route("/Home/GetEmployee")]
        public ActionResult GetEmployee()
        {
            try
            {
                DataTable dt = new DataTable();
                dt = objMas.GetDataTable("Select a.EmpId, a.EmpDesc, a.EmpType, a.Status, b.id, b.RoleName from employee_master a, userroleheader b where a.EmpType=b.id");

                if (dt == null)
                {
                    return Content(objMas.DBErrBuf);
                }
                List<EmployeeModel> emp = new List<EmployeeModel>();
                emp = (from DataRow row in dt.Rows

                       select new EmployeeModel
                       {
                           EmpId = Convert.ToDecimal(row["EmpId"].ToString()),
                           EmpDesc = row["EmpDesc"].ToString(),
                           EmpType = Convert.ToInt32(row["EmpType"].ToString()),
                           Status = Convert.ToInt32(row["Status"].ToString()),
                           id = Convert.ToInt32(row["id"].ToString()),
                           RoleName = row["RoleName"].ToString(),
                       }).ToList();

                return Json(emp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                return Content(Ex.Message.ToString());
            }
        }
        [HttpPost]
        [Route("action")]
        [Route("Home/AddEmployee")]
        public JsonResult AddEmployee(EmployeeModel collection)
        {
            Int64 Ret = 0;
            try
            {
                
                //Ret = objMas.GetTableCount("employee_master where EmpDesc = '" + collection.EmpDesc + "' ");
                //if(Ret >=1)
                //    return Json(new ResponseHelper { Success = 0, Message = "Employee Name Already Exists", Data = null });


                Ret = objMas.InsertMaster("select Concat(Date_format(Curdate(),'%y%m%d'),Lpad(Count(1) + 1, 4, '0')) from employee_master " +
                    "where Date(CreatedOn)=CurDate()", new string[] {"insert into employee_master (EmpId,EmpDesc,EmpType,Status,CreatedBy,CreatedOn,ModifiedBy," +
                    "ModifiedOn) values ('"}, new string[] { "','"+ collection.EmpDesc + "', '" + collection.EmpType + "','" + collection.Status
                    + "', '" + Request.Cookies["EmpId"].Value.ToString() + "',now(),'" + Request.Cookies["EmpId"].Value.ToString() + "',now())" }, 1);
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "Employee Added", Data = null });
                else if(Ret == -1)
                    return Json(new ResponseHelper { Success = 0, Message = "Save Failed Contact System Admin!", Data = null });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Save Failed !" + objMas.DBErrBuf, Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("/Home/EditEmployee")]
        public JsonResult EditEmployee(EmployeeModel collection)
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.ExecQry("update employee_master set EmpDesc = '" + collection.EmpDesc + "', EmpType = '" + collection.EmpType +
                    "',Status = '" + collection.Status + "',ModifiedBy = '" + Request.Cookies["EmpId"].Value.ToString() + "', ModifiedOn = now() where EmpId = '" + collection.EmpId+"'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "Employee Updated", Data = null });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Update Failed !" + objMas.DBErrBuf, Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("Home/DeleteEmployee")]
        public JsonResult DeleteEmployee(Int64 EmpId)
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.GetTableCount("employee_master where EmpId = '" + EmpId + "' and Status = 0");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 0, Message = "Already Deleted!" + objMas.DBErrBuf, Data = null });
                Ret = objMas.ExecQry("update employee_master set Status = 0 where EmpId = '" + EmpId + "'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "Employee Deleted" });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Failed To Delete " + objMas.DBErrBuf, Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }

        public ActionResult SysUserMaster()
        {
            try
            {
                Int64 Ret = 0;
                DataTable dt = new DataTable();
                Ret = objMas.GetTableCount("systemuser_master where PlantId ='" + Request.Cookies["PlantId"].Value.ToString() + "' and '" + Request.Cookies["PlantId"].Value.ToString() + "'='9300'");
                if (Ret >= 1)
                {
                    dt = objMas.GetDataTable("select * from plant_master");//select a.PlantId, b.PlantName from systemuser_master a, plant_master b where a.PlantId=b.PlantId
                    if (dt == null)
                    {
                        return View(new UserModel());
                    }

                    ViewBag.items = (from DataRow row in dt.Rows
                                     select new UserModel
                                     {
                                         PlantId = row["PlantId"].ToString(),
                                         PlantName = row["PlantName"].ToString(),
                                     }).ToList();
                }
                else
                {
                    dt = objMas.GetDataTable("Select PlantName from plant_master where PlantId = '" + Request.Cookies["PlantId"].Value.ToString() + "' and Status in (1,2)");/*Status in (1,2)*/
                    if (dt == null)
                    {
                        return View(new UserModel());
                    }
                    ViewBag.items = (from DataRow row in dt.Rows
                                     select new UserModel
                                     {
                                         PlantId = Request.Cookies["PlantId"].Value.ToString(),
                                         PlantName = row["PlantName"].ToString(),
                                     }).ToList();
                }
                var model = new UserModel();
                dt = objMas.GetDataTable("select UnitId, UnitName from unit_master where Status in (1,2)");
                if (dt == null)
                    return View(new UserModel());
                model.UnitDetail = (from DataRow row in dt.Rows
                                    select new UserModel.UnitModel
                                    {
                                        UnitId = row["UnitId"].ToString(),
                                        UnitName = row["UnitName"].ToString(),
                                    }).ToList();

                dt = objMas.GetDataTable("select EmpId, EmpDesc from employee_master where Status in (1,2)");
                if (dt == null)
                    return View(new UserModel());
                model.EmployeeDetail = (from DataRow row in dt.Rows
                                        select new UserModel.EmployeeModel
                                        {
                                            EmpId = row["EmpId"].ToString(),
                                            EmpDesc = row["EmpDesc"].ToString(),
                                        }).ToList();

                dt = objMas.GetDataTable("Select SystemId,SystemName from system_master where Status in (1,2)");
                if (dt == null)
                    return View(new UserModel());

                model.SystemDetail = (from DataRow row in dt.Rows
                                      select new SelectListItem
                                      {
                                          Value = row["SystemId"].ToString(),
                                          Text = row["SystemName"].ToString(),

                                      }).ToList();

                ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.System_User_Master, Request.Cookies["UserId"].Value.ToString());


                model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());
                return View(model);
            }
            catch(Exception Ex)
            {
                return Content(Ex.Message.ToString());
            }
            
        }
        [HttpGet]
        [Route("action")]
        [Route("/Home/GetUser")]
        public ActionResult GetUser()
        {
            Int64 Ret = 0;
            try
            {
                DataTable dt = new DataTable();

                Ret = objMas.GetTableCount("systemuser_master where '" + Request.Cookies["PlantId"].Value.ToString() + "' ='9300'");
                if (Ret >= 1)
                {
                    dt = objMas.GetDataTable("select a.UserId,a.UserPWD,a.Systems, a.Status,b.PlantId, b.PlantName, c.UnitId, c.UnitName, d.EmpId,d.EmpDesc " +
                    "from systemuser_master a,plant_master b, unit_master c, employee_master d where" +
                    " a.PlantId=b.PlantId and a.UnitId=c.UnitId and a.EmpId=d.EmpId");
                    List<UserModel> emp = new List<UserModel>();
                    emp = (from DataRow row in dt.Rows
                           select new UserModel
                           {
                               PlantId = row["PlantId"].ToString(),
                               PlantName = row["PlantName"].ToString(),
                               UnitId = row["UnitId"].ToString(),
                               UnitName = row["UnitName"].ToString(),
                               EmpId = Convert.ToDecimal(row["EmpId"].ToString()),
                               EmpDesc = row["EmpDesc"].ToString(),
                               UserId = row["UserId"].ToString(),
                               UserPWD = row["UserPWD"].ToString(),
                               Status = Convert.ToInt32(row["Status"].ToString()),
                               Systems = row["Systems"].ToString(),
                           }).ToList();
                    return Json(emp, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //dt = objMas.GetDataTable("select * from systemuser_master");
                    dt = objMas.GetDataTable("select a.UserId,a.UserPWD,a.Systems, a.Status, b.PlantId, b.PlantName,c.UnitId, c.UnitName, d.EmpId,d.EmpDesc from systemuser_master a,plant_master b, unit_master c, employee_master d where" +
                        " a.PlantId= b.PlantId and '" + Request.Cookies["PlantId"].Value.ToString() + "'= b.PlantId and a.UnitId=c.UnitId and a.EmpId=d.EmpId");
                    if (dt == null)
                        return Content(objMas.DBErrBuf);
                    List<UserModel> emp = new List<UserModel>();
                    emp = (from DataRow row in dt.Rows
                           select new UserModel
                           {
                               PlantId = row["PlantId"].ToString(),
                               PlantName = row["PlantName"].ToString(),
                               UnitId = row["UnitId"].ToString(),
                               UnitName = row["UnitName"].ToString(),
                               EmpId = Convert.ToDecimal(row["EmpId"].ToString()),
                               EmpDesc = row["EmpDesc"].ToString(),
                               UserId = row["UserId"].ToString(),
                               UserPWD = row["UserPWD"].ToString(),
                               Status = Convert.ToInt32(row["Status"].ToString()),
                               Systems = row["Systems"].ToString(),
                           }).ToList();
                    return Json(emp, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("/Home/AddUser")]
        public JsonResult AddUser(UserModel collection)
        {
            Int64 Ret = 0;
            try
            {

                Ret = objMas.GetTableCount("systemuser_master where UserId = '" + collection.UserId + "'");
                if (Ret >= 1)
                {
                    Ret = objMas.ExecQry("update systemuser_master set PlantId = '" + collection.PlantId + "', UnitId = '" + collection.UnitId +
                    "',EmpId = '" + collection.EmpId + "',UserPWD = '" + collection.UserPWD + "',Systems = '" + collection.Systems + "'," +
                    " Status = '" + collection.Status + "',ModifiedBy = '" + Request.Cookies["EmpId"].Value.ToString() + "', ModifiedOn = now() where UserId = '" + collection.UserId + "'");
                    if (Ret >= 1)
                        return Json(new ResponseHelper { Success = 1, Message = "User Updated", Data = null });
                    else
                        return Json(new ResponseHelper { Success = 0, Message = "Update Failed !" + objMas.DBErrBuf, Data = null });
                }

                Ret = objMas.ExecQry("insert into systemuser_master (UserId,PlantId,UnitId,Systems,UserPWD,EmpId,Status,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn)values" +
                    "('" + collection.UserId + "','" + collection.PlantId + "','" + collection.UnitId + "','" + collection.Systems + "','" + collection.UserPWD + "', '" + collection.EmpId +
                    "','" + collection.Status + "','" + Request.Cookies["EmpId"].Value.ToString() + "',now(),'" + Request.Cookies["EmpId"].Value.ToString() + "',now())");
                

                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "User Added", Data = null });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Save Failed!" + objMas.DBErrBuf, Data = null });
            }
            
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }



        }
        [HttpPost]
        [Route("action")]
        [Route("/Home/EditUser")]
        public JsonResult EditUser(UserModel collection)
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.ExecQry("update systemuser_master set PlantId = '" + collection.PlantId + "', UnitId = '" + collection.UnitId +
                    "',EmpId = '"+collection.EmpId+ "',UserPWD = '"+collection.UserPWD+"',Systems = '"+collection.Systems+"',Status = '" + collection.Status + "'," +
                    "ModifiedBy = '" + Request.Cookies["EmpId"].Value.ToString() + "', ModifiedOn = now() where UserId = '" + collection.UserId + "'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "User Updated", Data = null });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Update Failed !" + objMas.DBErrBuf, Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("/Home/DeleteUser")]
        public JsonResult DeleteUser(Int64 UserId)
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.GetTableCount("systemuser_master where UserId = '" + UserId + "' and Status = 0");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 0, Message = "Already Deleted!" + objMas.DBErrBuf, Data = null });
                Ret = objMas.ExecQry("update systemuser_master set Status = 0 where UserId = '" + UserId + "'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "User Deleted" });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Failed To Delete " + objMas.DBErrBuf, Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }

        public ActionResult CrateMaster()
        {
            ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.CrateMaster, Request.Cookies["UserId"].Value.ToString());

            var model = new CrateModel();
            model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());
            return View(model);
        }
        [HttpGet]
        [Route("action")]
        [Route("/Home/GetCrate")]
        public ActionResult GetCrate()
        {
            try
            {
                DataTable dt = new DataTable();
                dt = objMas.GetDataTable("select * from crate_master");
                if (dt == null)
                    return Content(objMas.DBErrBuf);

                List<CrateModel> emp = new List<CrateModel>();
                emp = (from DataRow row in dt.Rows
                       select new CrateModel
                       {
                           Id = Convert.ToInt32(row["Id"].ToString()),
                           CrateCode = row["CrateCode"].ToString(),
                           CurrentStatus = row["CurrentStatus"].ToString(),
                           Remarks = row["Remarks"].ToString(),
                           Status = Convert.ToInt32(row["Status"].ToString()),

                       }).ToList();
                return Json(emp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                return Content(Ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("/Home/AddCrate")]
        public JsonResult AddCrate(CrateModel collection)
        {
            Int64 Ret = 0;
            try
            {

                Ret = objMas.GetTableCount("crate_master where CrateCode = '" + collection.CrateCode + "'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 0, Message = "Crate Code Already Exists" });
                if(collection.CrateCode == null)
                    return Json(new ResponseHelper { Success = 0, Message = "Input Valid CrateCode", Data = null });
                if (collection.CurrentStatus == null)
                    collection.CurrentStatus = "";
                if (collection.Remarks == null)
                    collection.Remarks = "";

                Ret = objMas.ExecQry("insert into crate_master (CrateCode,CurrentStatus,Remarks,Status,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn) values ('" + 
                        collection.CrateCode+"','"+collection.CurrentStatus+"','"+collection.Remarks+"','"+collection.Status+
                        "','" + Request.Cookies["EmpId"].Value.ToString() + "',now(),'" + Request.Cookies["EmpId"].Value.ToString() + "',now())");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "Crate Added", Data = null });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Save Failed!" + objMas.DBErrBuf, Data = null });
            }

            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }



        }
        [HttpPost]
        [Route("action")]
        [Route("/Home/EditCrate")]
        public JsonResult EditCrate(CrateModel collection)
        {
            Int64 Ret = 0;
            try
            {
                if (collection.CurrentStatus == null)
                    collection.CurrentStatus = "";
                if (collection.Remarks == null)
                    collection.Remarks = "";

                Ret = objMas.ExecQry("update crate_master set CrateCode = '" + collection.CrateCode + "',CurrentStatus = '" + collection.CurrentStatus + "',Remarks = '" + collection.Remarks +
                    "',Status = '" + collection.Status + "',ModifiedBy = '" + Request.Cookies["EmpId"].Value.ToString() + "', ModifiedOn = now() where Id = '" + collection.Id + "'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "Crate Updated", Data = null });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Update Failed !" + objMas.DBErrBuf, Data = null });
            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }


        [HttpPost]
        [Route("action")]
        [Route("/Home/DeleteCrate")]
        public JsonResult DeleteCrate(Int64 Id)
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.GetTableCount("crate_master where Id = '" + Id + "' and Status = 0");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 0, Message = "Already Deleted!" + objMas.DBErrBuf, Data = null });
                Ret = objMas.ExecQry("update crate_master set Status = 0 where Id = '" + Id + "'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "Crate Deleted" });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Failed To Delete " + objMas.DBErrBuf, Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }

        public ActionResult ShiftDetails()
        {
            return View();
        }

        
        public ActionResult PlantMaster()
        {
            try
            {
                var model = new PlantsModel();
                var dt = objMas.GetDataTable("Select CompanyID,CompanyName from company_master where Status in (1,2)");
                if (dt == null)
                    return View(new PlantsModel());

                model.CompanyDetail = (from DataRow row in dt.Rows
                                       select new Company
                                       {
                                           CompanyID = row["CompanyID"].ToString(),
                                           CompanyName = row["CompanyName"].ToString(),

                                       }).ToList();

                ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.PlantMaster, Request.Cookies["UserId"].Value.ToString());


                model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());
                return View(model);
            }
            catch(Exception Ex)
            {
                return Content(Ex.Message.ToString());
            }
        }

        [HttpGet]
        [Route("action")]
        [Route("/Home/GetPlant")]
        public ActionResult GetPlant()
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.GetTableCount("systemuser_master where '" + Request.Cookies["PlantId"].Value.ToString() + "' ='9300'");
                DataTable dt = new DataTable();
                if (Ret >= 1)
                //Ret = objMas.GetTableCount("select a.PlantId, b.PlantName from systemuser_master a, plant_master b where a.PlantId and '" + Request.Cookies["PlantId"].Value.ToString() + "' ='0'");
                {
                    dt = objMas.GetDataTable("select * from plant_master");
                    List<PlantsModel> emp = new List<PlantsModel>();
                    emp = (from DataRow row in dt.Rows
                           select new PlantsModel
                           {
                               Id = Convert.ToInt32(row["Id"].ToString()),
                               PlantId = row["PlantId"].ToString(),
                               PlantName = row["PlantName"].ToString(),
                               CompanyID = row["CompanyID"].ToString(),
                               ShortName = row["ShortName"].ToString(),
                               PlantType = row["PlantType"].ToString(),
                               Status = Convert.ToInt32(row["Status"].ToString()),

                           }).ToList();
                    return Json(emp, JsonRequestBehavior.AllowGet);
                }
                else 
                { 
                    //dt = objMas.GetDataTable("select * from plant_master");
                    dt = objMas.GetDataTable("select Id, PlantId, PlantName, CompanyId, ShortName, PlantType,Status from plant_master where PlantId='" + Request.Cookies["PlantId"].Value.ToString() + "' ");
                    if (dt == null)
                        return Content(objMas.DBErrBuf);
                    List<PlantsModel> emp = new List<PlantsModel>();
                    emp = (from DataRow row in dt.Rows
                           select new PlantsModel
                           {
                               Id = Convert.ToInt32(row["Id"].ToString()),
                               PlantId = row["PlantId"].ToString(),
                               PlantName = row["PlantName"].ToString(),
                               CompanyID = row["CompanyID"].ToString(),
                               ShortName = row["ShortName"].ToString(),
                               PlantType = row["PlantType"].ToString(),
                               Status = Convert.ToInt32(row["Status"].ToString()),

                           }).ToList();
                    return Json(emp, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception Ex)
            {
                return Content(Ex.Message.ToString());
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("/Home/AddPlant")]
        public JsonResult AddPlant(PlantsModel collection)
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.GetTableCount("plant_master where PlantId = '" + collection.PlantId + "'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 0, Message = "PlantId Already Exists!", Data = null });
                Ret = objMas.ExecQry("insert into plant_master (PlantId,PlantName,CompanyID,ShortName,PlantType,PlantAlias,Dashboard,Status,ServerSendFlag," +
                    "CreatedBy,CreatedOn,ModifiedBy,ModifiedOn) values ('" + collection.PlantId + "','" + collection.PlantName + "','" + collection.CompanyID + "'," +
                    "'" + collection.ShortName + "','" + collection.PlantType + "','I',0,'" + collection.Status + "'," +
                    "0,'" + Request.Cookies["EmpId"].Value.ToString() + "',now(),'" + Request.Cookies["EmpId"].Value.ToString() + "',now())");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "Plant Added", Data = null });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Save Failed!" + objMas.DBErrBuf, Data = null });
            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("/Home/EditPlant")]
        public JsonResult EditPlant(PlantsModel collection)
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.ExecQry("update plant_master set PlantId = '" + collection.PlantId + "', PlantName = '" + collection.PlantName + "',CompanyID = '" + collection.CompanyID + "'," +
                    " ShortName = '" + collection.ShortName + "', PlantType = '" + collection.PlantType + "', Status = '" + collection.Status + "'," +
                    "ModifiedBy = '" + Request.Cookies["EmpId"].Value.ToString() + "', ModifiedOn = now() where Id = '" + collection.Id + "'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "Plant Updated", Data = null });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Update Failed!", Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("/Home/DeletePlant")]
        public JsonResult DeletePlant(Int64 Id)
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.GetTableCount("plant_master where Id = '" + Id + "' and Status = 0");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 0, Message = "Already Deleted!" + objMas.DBErrBuf, Data = null });
                Ret = objMas.ExecQry("update plant_master set Status = 0 where Id = '" + Id + "'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "Plant Deleted" });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Failed To Delete " + objMas.DBErrBuf, Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }

        public ActionResult RackMaster()
        {
            try
            {
                Int64 Ret = 0;
                DataTable dt = new DataTable();
                Ret = objMas.GetTableCount("systemuser_master where PlantId ='" + Request.Cookies["PlantId"].Value.ToString() + "' and '" + Request.Cookies["PlantId"].Value.ToString() + "'='9300'");
                if (Ret >= 1)
                {
                    dt = objMas.GetDataTable("select a.PlantId, b.PlantName, b.PlantType from systemuser_master a, plant_master b where a.PlantId=b.PlantId");
                    if (dt == null)
                    {
                        return View(new RackModel());
                    }

                    ViewBag.items = (from DataRow row in dt.Rows
                                     select new RackModel
                                     {
                                         PlantId = row["PlantId"].ToString(),
                                         PlantName = row["PlantName"].ToString(),
                                         PlantType = row["PlantType"].ToString(),
                                     }).ToList();
                }
                else
                {
                    dt = objMas.GetDataTable("Select PlantName, PlantType from plant_master where PlantId = '" + Request.Cookies["PlantId"].Value.ToString() + "' and Status in (1,2)");/*Status in (1,2)*/
                    if (dt == null)
                    {
                        return View(new RackModel());
                    }
                    ViewBag.items = (from DataRow row in dt.Rows
                                     select new RackModel
                                     {
                                         PlantId = Request.Cookies["PlantId"].Value.ToString(),
                                         PlantName = row["PlantName"].ToString(),
                                         PlantType = row["PlantType"].ToString(),
                                     }).ToList();


                }
                var model = new Plant();

                ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.RackMaster, Request.Cookies["UserId"].Value.ToString());
                model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());
                return View(model);
            }
            catch (Exception Ex)
            {
                return Content(Ex.Message.ToString());
            }

        }


        [HttpGet]
        [Route("action")]
        [Route("/Home/GetRack")]
        public ActionResult GetRack()
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.GetTableCount("systemuser_master where '" + Request.Cookies["PlantId"].Value.ToString() + "' ='9300'");
                DataTable dt = new DataTable();
                if (Ret >= 1)
                {
                    dt = objMas.GetDataTable("select a.Id, a.RackNumber, a.StorageLocation,a.Minimum, a.Maximum, a.Status, a.PlantId, " +
                    "b.PlantName, b.PlantType from rack_master a, plant_master b where a.PlantId=b.PlantId");
                    List<RackModel> emp = new List<RackModel>();
                    emp = (from DataRow row in dt.Rows
                           select new RackModel
                           {
                               Id = Convert.ToInt32(row["Id"].ToString()),
                               RackNumber = row["RackNumber"].ToString(),
                               PlantId = row["PlantId"].ToString(),
                               PlantName = row["PlantName"].ToString(),
                               PlantType = row["PlantType"].ToString(),
                               StorageLocation = row["StorageLocation"].ToString(),
                               Minimum = Convert.ToInt32(row["Minimum"].ToString()),
                               Maximum = Convert.ToInt32(row["Maximum"].ToString()),
                               Status = Convert.ToInt32(row["Status"].ToString()),

                           }).ToList();
                    return Json(emp, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    dt = objMas.GetDataTable("select a.Id, a.RackNumber, a.StorageLocation,a.Minimum, a.Maximum, a.Status, a.PlantId, " +
                    "b.PlantName, b.PlantType from rack_master a, plant_master b where a.PlantId=b.PlantId and '" + Request.Cookies["PlantId"].Value.ToString() + "' = b.PlantId");
                    if (dt == null)
                        return Content(objMas.DBErrBuf);
                    List<RackModel> emp = new List<RackModel>();
                    emp = (from DataRow row in dt.Rows
                           select new RackModel
                           {
                               Id = Convert.ToInt32(row["Id"].ToString()),
                               RackNumber = row["RackNumber"].ToString(),
                               PlantId = row["PlantId"].ToString(),
                               PlantName = row["PlantName"].ToString(),
                               PlantType = row["PlantType"].ToString(),
                               StorageLocation = row["StorageLocation"].ToString(),
                               Minimum = Convert.ToInt32(row["Minimum"].ToString()),
                               Maximum = Convert.ToInt32(row["Maximum"].ToString()),
                               Status = Convert.ToInt32(row["Status"].ToString()),

                           }).ToList();
                    return Json(emp, JsonRequestBehavior.AllowGet);
                }
                    
            }
            catch (Exception Ex)
            {
                return Content(Ex.Message.ToString());
            }
        }
        [HttpPost]
        [Route("action")]
        [Route("/Home/AddRack")]
        public JsonResult AddRack(RackModel collection)
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.GetTableCount("rack_master where RackNumber = '" + collection.RackNumber + "'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 0, Message = "RackNumber Already Exists!", Data = null });
                Ret = objMas.ExecQry("insert into rack_master (RackNumber, PlantId, StorageLocation, Minimum, Maximum,Status,CreatedBy,CreatedOn, ModifiedBy, ModifiedOn) values ('"+collection.RackNumber+"'," +
                    "'"+collection.PlantId+"','"+collection.StorageLocation+"','"+collection.Minimum+"','"+collection.Maximum+"','"+collection.Status+ "'," +
                    "'" + Request.Cookies["EmpId"].Value.ToString() + "',now(),'" + Request.Cookies["EmpId"].Value.ToString() + "',now())");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "Rack Added", Data = null });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Save Failed!" + objMas.DBErrBuf, Data = null });
            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }
        [HttpPost]
        [Route("action")]
        [Route("/Home/EditRack")]
        public JsonResult EditRack(RackModel collection)
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.ExecQry("update rack_master set PlantId = '" + collection.PlantId + "', StorageLocation = '" + collection.StorageLocation + "',Minimum = '" + collection.Minimum + "'," +
                    " Maximum = '" + collection.Maximum + "', Status = '" + collection.Status + "', ModifiedBy = '" + Request.Cookies["EmpId"].Value.ToString() + "', ModifiedOn = now() where RackNumber = '" + collection.RackNumber + "'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "Rack Updated", Data = null });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Update Failed!", Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }
        [HttpPost]
        [Route("action")]
        [Route("/Home/DeleteRack")]
        public JsonResult DeleteRack(Int64 Id)
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.GetTableCount("rack_master where Id = '" + Id + "' and Status = 0");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 0, Message = "Already Deleted!" + objMas.DBErrBuf, Data = null });
                Ret = objMas.ExecQry("update rack_master set Status = 0 where Id = '" + Id + "'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "Rack Deleted" });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Failed To Delete " + objMas.DBErrBuf, Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }

        public ActionResult WorkCetreMaster()
        {
            return View();
        }
        [HttpGet]
        [Route("action")]
        [Route("/Home/GetWorkCentre")]
        public ActionResult GetWorkCentre()
        {
            try
            {
                DataTable dt = new DataTable();
                dt = objMas.GetDataTable("select * from workcentre_master");
                if (dt == null)
                    return Content(objMas.DBErrBuf);
                List<WorkCentreModel> emp = new List<WorkCentreModel>();
                emp = (from DataRow Row in dt.Rows
                       select new WorkCentreModel
                       {
                           PlantDesc = Row["PlantDesc"].ToString(),
                           MachineName = Row["MachineName"].ToString(),
                           Status = Convert.ToInt32(Row["Status"].ToString()),
                       }).ToList();
                return Json(emp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                return Content(Ex.Message.ToString());
            }
            
        }

        //public ActionResult UserRoleTesting()
        //{
        //    return View();
        //}

        public ActionResult UserRole()
        {
            try
            {
                var model = new UserRoleModel();
                var dt = objMas.GetDataTable("Select * from userroleheader");
                if (dt == null)
                    return View(new WebuserModel());

                var data = (from DataRow row in dt.Rows
                                       select new WebuserModel
                                       {
                                           id = Convert.ToInt32(row["id"].ToString()),
                                           DisplayName = row["DisplayName"].ToString(),
                                           RoleName = row["RoleName"].ToString(),
                                           Status = Convert.ToInt32(row["Status"].ToString()),
                                       }).ToList();

                ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.UserRole, Request.Cookies["UserId"].Value.ToString());
                model.User = data;
                model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());
                return View(model);

            }
            catch(Exception Ex)
            {
                return Content(Ex.Message.ToString());
            }
        }
        public ActionResult GetWebUserRoleDetails(Int32 RoleId)
        {
            try
            {
                DataTable dt = new DataTable();
                if (RoleId == 0)
                {
                    dt = objMas.GetDataTable("select a.MenuId, a.MenuDesc, a.OptEdit, a.OptView, a.Status from webuser_menu a");        //, userrolelines b " +      "where a.MenuId=b.MenuId

                    if (dt == null)
                        return Content(objMas.DBErrBuf);

                    List<UserRoleDetails> emp = new List<UserRoleDetails>();
                    emp = (from DataRow Row in dt.Rows
                           select new UserRoleDetails
                           {
                               //id = 0, //Convert.ToInt32(Row["id"].ToString()),
                               MenuId = Convert.ToInt32(Row["MenuId"].ToString()),
                               //RoleId = Convert.ToInt32(Row["RoleId"].ToString()),
                               MenuDesc = Row["MenuDesc"].ToString(),                               //, b.RoleId, b.View, b.Edit
                               Edit = false, 
                               View = false, 
                               OptEdit = Convert.ToInt32(Row["OptEdit"].ToString()),
                               OptView = Convert.ToInt32(Row["OptView"].ToString()),
                           }).ToList();
                    return Json(emp, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    dt = objMas.GetDataTable("select a.id, a.RoleId, a.MenuId, a.View, a.Edit, b.MenuDesc, b.OptEdit, b.OptView from userrolelines a, " +
                        "webuser_menu b,userroleheader c where a.MenuId=b.MenuId and a.RoleId=c.id and a.RoleId='" + RoleId + "'");
                    if (dt == null)
                        return Content(objMas.DBErrBuf);

                    List<UserRoleDetails> emp = new List<UserRoleDetails>();
                    emp = (from DataRow Row in dt.Rows
                           select new UserRoleDetails
                           {
                               id = Convert.ToInt32(Row["id"].ToString()),
                               MenuId = Convert.ToInt32(Row["MenuId"].ToString()),
                               RoleId = Convert.ToInt32(Row["RoleId"].ToString()),
                               MenuDesc = Row["MenuDesc"].ToString(),
                               Edit = Convert.ToBoolean(Row["Edit"]),
                               View = Convert.ToBoolean(Row["View"]),
                               OptEdit = Convert.ToInt32(Row["OptEdit"].ToString()),
                               OptView = Convert.ToInt32(Row["OptView"].ToString()),
                           }).ToList();
                    return Json(emp, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception Ex)
            {
                return Content(Ex.Message.ToString());
            }
        }      //Success For Getting
        public ActionResult RoleSaveUpdate(List<WebuserModel> collection)
        {

            try
            {
                //Int64 roleID = objMas.InsertMaster("Select @roleID := MAX(id) From userroleheader;");
                Int64 Ret = 0;
                int RoleId = 0;
                string buf = "";
                RoleId = collection[0].RoleId;
                DataTable dt = new DataTable();

                if (collection[0].RoleId == 0)
                {

                    Ret = objMas.GetTableCount("userroleheader where RoleName = '" + collection[0].RoleName + "'");
                    if (Ret >= 1)
                        return Json(new ResponseHelper { Success = 0, Message = "RoleName Already Exists!", Data = null });

                    Ret = objMas.ExecQry("insert into userroleheader (id,RoleName,DisplayName,Status) Values('" + collection[0].id + "','" + collection[0].RoleName + "'," +
                        "'" + collection[0].DisplayName + "','" + collection[0].Status + "')");
                    if (Ret >= 1)
                    {
                        Ret = objMas.ExecScalar("select MAX(id) from userroleheader", ref buf);
                        if (Ret >= 1)
                        {
                            foreach (var item in collection)
                            {
                                Ret = objMas.ExecQry("insert into userrolelines (RoleId,MenuId,View,Edit) Values('" + buf + "','" + item.MenuId + "'," +
                                "" + item.View + "," + item.Edit + ")");
                            }
                            if (Ret >= 1)
                            {
                                return Json(new ResponseHelper { Success = 1, Message = "Role Added", Data = null });
                            }
                            else
                                return Json(new ResponseHelper { Success = 0, Message = "Save Failed!" + objMas.DBErrBuf, Data = null });
                        }

                        return Json(new ResponseHelper { Success = 0, Message = "Save Failed!" + objMas.DBErrBuf, Data = null });
                    }
                    return Json(new ResponseHelper { Success = 0, Message = "Save Failed!" + objMas.DBErrBuf, Data = null });
                }
                else
                {
                    Ret = objMas.ExecQry("update userroleheader set RoleName = '" + collection[0].RoleName + "',DisplayName = '" + collection[0].DisplayName + "'," +
                        "Status = '" + collection[0].Status + "' where id = '" + collection[0].RoleId + "'");

                    for (int row = 0; row < collection.Count; row++)
                    {
                        Ret = objMas.ExecQry("update userrolelines set View=" + collection[row].View + ", Edit=" + collection[row].Edit + " " +
                            "where RoleId= '" + collection[row].RoleId + "' and MenuId='" + collection[row].MenuId + "'");

                    }
                    if(Ret >= 1)
                        return Json(new ResponseHelper { Success = 1, Message = "Role Updated", Data = null });
                    else
                        return Json(new ResponseHelper { Success = 0, Message = "Update Failed!" + objMas.DBErrBuf, Data = null });

                }
            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }


        }

        [HttpPost]
        [Route("action")]
        [Route("/Home/DeleteUserRole")]
        public JsonResult DeleteUserRole(Int64 Id)
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.GetTableCount("userroleheader where id = '" + Id + "' and Status = 0");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 0, Message = "Already Deleted!" + objMas.DBErrBuf, Data = null });
                Ret = objMas.ExecQry("update userroleheader set Status = 0 where id = '" + Id + "'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "User Role Deleted" });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Failed To Delete " + objMas.DBErrBuf, Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }



        public ActionResult DeviceUserMaster()
        {
            try
            {
                Int64 Ret = 0;
                DataTable dt = new DataTable();
                var model = new DeviceUserModel();
                Ret = objMas.GetTableCount("deviceuser_master where PlantId ='" + Request.Cookies["PlantId"].Value.ToString() + "' " +
                     " and '" + Request.Cookies["PlantId"].Value.ToString() + "'='9300'");
                if (Ret >= 1)
                {
                    dt = objMas.GetDataTable("select * from plant_master");
                    if (dt == null)
                    {
                        return View(new DeviceUserModel());
                    }

                    model.PlantDetail = (from DataRow a in dt.Rows
                                         select new SelectListItem { Text = a["PlantName"].ToString(), Value = a["PlantId"].ToString() }).ToList();
                }
                else
                {
                    dt = objMas.GetDataTable("Select PlantId,PlantName from plant_master where PlantId = '" + Request.Cookies["PlantId"].Value.ToString() + "' and Status in (1,2)");/*Status in (1,2)*/
                    if (dt == null)
                    {
                        return View(new DeviceUserModel());
                    }

                    model.PlantDetail = (from DataRow a in dt.Rows
                                         select new SelectListItem { Text = a["PlantName"].ToString(), Value = a["PlantId"].ToString() }).ToList();
                }
                

                dt = objMas.GetDataTable("select EmpId, EmpDesc from employee_master where Status in (1,2)");
                if (dt == null)
                    return View(new DeviceUserModel());
                model.EmployeeDetail = (from DataRow a in dt.Rows
                                     select new SelectListItem { Text = a["EmpDesc"].ToString(), Value = a["EmpId"].ToString() }).ToList();


                ViewBag.IsEdit = objMas1.GetMenuEdit(Globals.DeviceUserMaster, Request.Cookies["UserId"].Value.ToString());


                model.menu = objMas1.LoadMenus(Request.Cookies["UserId"].Value.ToString());
                return View(model);
            }
            catch (Exception Ex)
            {
                return Content(Ex.Message.ToString());
            }

        }
        [HttpGet]
        [Route("action")]
        [Route("/Home/GetDeviceUser")]
        public ActionResult GetDeviceUser()
        {
            Int64 Ret = 0;
            try
            {
                DataTable dt = new DataTable();

                Ret = objMas.GetTableCount("deviceuser_master where '" + Request.Cookies["PlantId"].Value.ToString() + "' ='9300'");
                if (Ret >= 1)
                {
                    dt = objMas.GetDataTable("select a.UserId,a.DeviceId,a.UserPWD, a.Status,b.PlantId, b.PlantName, d.EmpId,d.EmpDesc " +
                    "from deviceuser_master a,plant_master b, employee_master d where" +
                    " a.PlantId=b.PlantId and a.EmpId=d.EmpId");
                    List<DeviceUserModel> emp = new List<DeviceUserModel>();
                    emp = (from DataRow row in dt.Rows
                           select new DeviceUserModel
                           {
                               PlantId = row["PlantId"].ToString(),
                               PlantName = row["PlantName"].ToString(),
                               EmpId = row["EmpId"].ToString(),
                               EmpDesc = row["EmpDesc"].ToString(),
                               UserId = row["UserId"].ToString(),
                               DeviceId = row["DeviceId"].ToString(),
                               UserPWD = row["UserPWD"].ToString(),
                               Status = row["Status"].ToString(),
                           }).ToList();
                    return Json(emp, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //dt = objMas.GetDataTable("select * from systemuser_master");
                    dt = objMas.GetDataTable("select a.UserId,a.DeviceId,a.UserPWD,a.Status, b.PlantId, b.PlantName,d.EmpId,d.EmpDesc from deviceuser_master a,plant_master b, employee_master d where" +
                        " a.PlantId= b.PlantId and '" + Request.Cookies["PlantId"].Value.ToString() + "'= b.PlantId and a.EmpId=d.EmpId");
                    if (dt == null)
                        return Content(objMas.DBErrBuf);
                    List<DeviceUserModel> emp = new List<DeviceUserModel>();
                    emp = (from DataRow row in dt.Rows
                           select new DeviceUserModel
                           {
                               PlantId = row["PlantId"].ToString(),
                               PlantName = row["PlantName"].ToString(),
                               EmpId = row["EmpId"].ToString(),
                               EmpDesc = row["EmpDesc"].ToString(),
                               UserId = row["UserId"].ToString(),
                               DeviceId = row["DeviceId"].ToString(),
                               UserPWD = row["UserPWD"].ToString(),
                               Status = row["Status"].ToString(),
                           }).ToList();
                    return Json(emp, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("/Home/AddDeviceUser")]
        public JsonResult AddDeviceUser(DeviceUserModel collection)
        {
            Int64 Ret = 0;
            try
            {

                Ret = objMas.GetTableCount("deviceuser_master where UserId = '" + collection.UserId + "'");
                if (Ret >= 1)
                        return Json(new ResponseHelper { Success = 0, Message = "User Id Already Exists", Data = null });

                Ret = objMas.ExecQry("insert into deviceuser_master (UserId,PlantId,DeviceId,UserPWD,EmpId,Status,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn)values" +
                    "('" + collection.UserId + "','" + collection.PlantId + "','" + collection.DeviceId + "','" + collection.UserPWD + "', '" + collection.EmpId +
                    "','" + collection.Status + "','" + Request.Cookies["EmpId"].Value.ToString() + "',now(),'" + Request.Cookies["EmpId"].Value.ToString() + "',now())");

                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "User Added", Data = null });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Save Failed!" + objMas.DBErrBuf, Data = null });
            }

            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }



        }
        [HttpPost]
        [Route("action")]
        [Route("/Home/EditDeviceUser")]
        public JsonResult EditDeviceUser(UserModel collection)
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.ExecQry("update deviceuser_master set PlantId = '" + collection.PlantId + "', EmpId = '" + collection.EmpId + "'," +
                    "UserPWD = '" + collection.UserPWD + "',Status = '" + collection.Status + "'," +
                    "ModifiedBy = '" + Request.Cookies["EmpId"].Value.ToString() + "', ModifiedOn = now() where UserId = '" + collection.UserId + "'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "User Updated", Data = null });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Update Failed !" + objMas.DBErrBuf, Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }

        [HttpPost]
        [Route("action")]
        [Route("/Home/DeleteDeviceUser")]
        public JsonResult DeleteDeviceUser(Int64 UserId)
        {
            Int64 Ret = 0;
            try
            {
                Ret = objMas.GetTableCount("deviceuser_master where UserId = '" + UserId + "' and Status = 0");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 0, Message = "Already Deleted!" + objMas.DBErrBuf, Data = null });
                Ret = objMas.ExecQry("update deviceuser_master set Status = 0 where UserId = '" + UserId + "'");
                if (Ret >= 1)
                    return Json(new ResponseHelper { Success = 1, Message = "User Deleted" });
                else
                    return Json(new ResponseHelper { Success = 0, Message = "Failed To Delete " + objMas.DBErrBuf, Data = null });

            }
            catch (Exception Ex)
            {
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }
        }
    }
}