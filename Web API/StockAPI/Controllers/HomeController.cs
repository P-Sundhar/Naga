using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StockAPI.Controllers
{
    public class HomeController : Controller
    {
        MasterLogic objMas = new MasterLogic();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public JsonResult LoginValidate(LoginModel lgnmodel)
        {
            var response = new ResponseHelper { status = 0, message = "" };
            try
            {
                Int64 Ret = objMas.GetTableCount("deviceuser_master where DeviceId = '" + lgnmodel.Imei + "' and UserId='" + lgnmodel.UserId 
                    + "' and UserPWD='" + lgnmodel.Password + "' and Status in (1,2)");
                if (Ret == -1)
                    response = new ResponseHelper { status = 0, message = objMas.DBErrBuf };
                else if (Ret == 0)
                    response = new ResponseHelper { status = 0, message = "Invalid Credentails" };
                else
                    response = new PlantRespose { status = 1, message = "Success" };
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("GetPlant", Ex.Message.ToString());
                response = new PlantRespose { status = 0, message = Ex.Message.ToString() + "  Inner Exception : " + Ex.InnerException.ToString() };
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetPlant()
        {
            var response = new ResponseHelper { status = 0, message = "" };
            try
            {
                DataTable dt = objMas.GetDataTable("select PlantId,PlantName,ShortName,PlantType,PlantAlias,PlantTypeFlag,Status from Plant_Master ");
                if (dt == null)
                {
                    response = new ResponseHelper { status = 0, message = objMas.DBErrBuf };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }

                var data = objMas.ConvertTo<PlantMaster>(dt);
                response = new PlantRespose { status = 1, message = "", data = data };
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("GetPlant", Ex.Message.ToString());
                response = new PlantRespose { status = 0, message = Ex.Message.ToString() + "  Inner Exception : " + Ex.InnerException.ToString() };
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ValidateCrate(BarcodeValidate brcdedtls)
        {
            var response = new ResponseHelper { status = 0, message = "" };
            try
            {
                Int64 ret = 0;

                ret = objMas.GetTableCount("Plant_Master where PlantId='" + brcdedtls.PlantId + "' and PlantTypeFlag=0");
                if (ret <= -1)
                {
                    response = new ResponseHelper { status = 0, message = objMas.DBErrBuf };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                ret = objMas.GetTableCount("crate_master where CrateCode='" + brcdedtls.CrateBarcode +"'");
                if (ret <= -1)
                   response = new ResponseHelper { status = 0, message = objMas.DBErrBuf };
                else if (ret == 0)
                    response = new ResponseHelper { status = 0, message = "Invalid Crate Barcode" };
                else
                    response = new ResponseHelper { status = 1, message = "Success" };

            }
            catch (Exception Ex)
            {
                objMas.PrintLog("ValidateCrate", Ex.Message.ToString());
                response = new ResponseHelper { status = 0, message = Ex.Message.ToString() + "  Inner Exception : " + Ex.InnerException.ToString() };
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ValidateBarcode(BarcodeValidate brcdedtls)
        {
            var response = new ResponseHelper { status = 0, message = "" };
            try
            {
                Int64 ret = 0;
                string tblname = "";

                ret = objMas.GetTableCount("Plant_Master where PlantId='" + brcdedtls.PlantId + "' and PlantTypeFlag=0");
                if (ret <= -1)
                {
                    response = new ResponseHelper { status = 0, message = objMas.DBErrBuf };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                tblname = ret >= 1 ? "production_lines" : "cfagrn_details";

                ret = objMas.GetTableCount("crate_master where CrateCode='" + brcdedtls.CrateBarcode +"'");
                if (ret <= -1)
                {
                    response = new ResponseHelper { status = 0, message = objMas.DBErrBuf };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                else if (ret == 0)
                {
                    response = new ResponseHelper { status = 0, message = "Invalid Crate Barcode" };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                ret = objMas.GetTableCount("" + tblname +" where Barcode='" + brcdedtls.SecondaryBarcode + "'");
                if (ret == -1)
                {
                    response = new ResponseHelper { status = 0, message = objMas.DBErrBuf };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                else if (ret == 0)
                {
                    if (tblname == "cfagrn_details")
                    {
                        ret = objMas.GetTableCount("production_lines where Barcode='" + brcdedtls.SecondaryBarcode + "'");
                        if (ret == -1)
                        {
                            response = new ResponseHelper { status = 0, message = objMas.DBErrBuf };
                            return Json(response, JsonRequestBehavior.AllowGet);
                        }
                        else if (ret == 0)
                        {
                            response = new ResponseHelper { status = 0, message = "Invalid Secondary Barcode" };
                            return Json(response, JsonRequestBehavior.AllowGet);
                        }
                        tblname = "production_lines";
                    }
                    else
                    {
                        response = new ResponseHelper { status = 0, message = "Invalid Secondary Barcode" };
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }
                }
                ret = objMas.GetTableCount("temp_tble_stockaudit where Barcode='" + brcdedtls.SecondaryBarcode
                        + "'");
                if (ret == -1)
                {
                    response = new ResponseHelper { status = 0, message = objMas.DBErrBuf };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                else if (ret >= 1)
                {
                    response = new ResponseHelper { status = 0, message = "Barcode Already Scanned" };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                ret = objMas.GetTableCount("stockaudit where Barcode='" + brcdedtls.SecondaryBarcode
                        + "'");
                if (ret == -1)
                {
                    response = new ResponseHelper { status = 0, message = objMas.DBErrBuf };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                else if (ret >= 1)
                {
                    response = new ResponseHelper { status = 0, message = "Barcode Already Scanned" };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }

                ret = objMas.ExecQry("insert into temp_tble_stockaudit(Plant,RackNo,CrateBarcode,Barcode,BatchNo,MaterialCode," +
                    "MaterialDesc,ScannedOn,ScannedBy)select '" + brcdedtls.PlantId + "','" + brcdedtls.RackBarcode
                    + "','" + brcdedtls.CrateBarcode + "','" + brcdedtls.SecondaryBarcode + "',BatchNo,a.MaterialCode," +
                    "b.MaterialDesc,now(),'" + brcdedtls.ScannedBy + "' from "+ tblname +" a,material_master b where " +
                    "a.MaterialCode=b.MaterialCode and a.Barcode='" + brcdedtls.SecondaryBarcode +"'");
                if (ret == -1)
                    response = new ResponseHelper { status = 0, message = objMas.DBErrBuf };
                else
                    response = new ResponseHelper { status = 1, message = "Success" };
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("ValidateBarcode", Ex.Message.ToString());
                response = new ResponseHelper { status = 0, message = Ex.Message.ToString() + "  Inner Exception : " + Ex.InnerException.ToString() };
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

       [HttpPost]
        public JsonResult GetStockDetails(BarcodeValidate brcdedtls)
        {
            var response = new ResponseHelper { status = 0, message = "" };
            try
            {
                var dt = objMas.GetDataTable("Select Plant,RackNo,CrateBarcode,Barcode,BatchNo,MaterialCode,MaterialDesc," +
                    "ScannedOn from temp_tble_stockaudit where Plant='" + brcdedtls.PlantId + "'");

                if (dt == null)
                {
                    response = new ResponseHelper { status = 0, message = objMas.DBErrBuf };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                var data = objMas.ConvertTo<StockAudit>(dt);
                response = new StockAuditResponse { status = 1, message = "", data = data };
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("GetStockDetails", Ex.Message.ToString());
                response = new ResponseHelper { status = 0, message = Ex.Message.ToString() + "  Inner Exception : " + Ex.InnerException.ToString() };
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveStockAudit(BarcodeValidate brcdedtls)
        {
            var response = new ResponseHelper { status = 0, message = "" };
            try
            {
                Int64 ret = 0;
                string buf = "";
                string[] sQry = new string[2];

                ret = objMas.GetTableCount("temp_tble_stockaudit where Plant='"+ brcdedtls.PlantId + "'");
                if (ret == -1)
                {
                    response = new ResponseHelper { status = 0, message = objMas.DBErrBuf };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                else if (ret == 0)
                {
                    response = new ResponseHelper { status = 0, message = "No Data To Save" };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }

                buf = brcdedtls.PlantId + System.DateTime.Now.ToString("HHmmss");

                sQry[0] = "insert into stockaudit(StockAuditId,Plant,RackNo,CrateBarcode,Barcode,BatchNo,MaterialCode,MaterialDesc,ScannedOn,ScannedBy)select '" +
                          buf + "',Plant,RackNo,CrateBarcode,Barcode,BatchNo,MaterialCode,MaterialDesc,ScannedOn,ScannedBy from temp_tble_stockaudit where " +
                          "Plant='" + brcdedtls.PlantId + "'";

                sQry[1] = "Delete from temp_tble_stockaudit where Plant='" + brcdedtls.PlantId + "'";

                ret = objMas.ExecMultipleQry(2, sQry);
                if (ret == -1)
                    response = new ResponseHelper { status = 0, message = objMas.DBErrBuf };
                else
                    response = new ResponseHelper { status = 1, message = "Success" };
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("SaveStockAudit", Ex.Message.ToString());
                response = new ResponseHelper { status = 0, message = Ex.Message.ToString() + "  Inner Exception : " + Ex.InnerException.ToString() };
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult GetPhysicalAdjust(string AuditDate)
        {
            var response = new ResponseHelper { status = 0, message = "" };
            try
            {
                var dt = objMas.GetDataTable("call PhysicalAdjustment('" + AuditDate + "')");

                if (dt == null)
                {
                    response = new ResponseHelper { status = 0, message = objMas.DBErrBuf };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                var data = objMas.ConvertTo<PhysicalAdjust>(dt);
                response = new PhysicalAdjustResponse { status = 1, message = "", data = data };
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("GetStockDetails", Ex.Message.ToString());
                response = new ResponseHelper { status = 0, message = Ex.Message.ToString() + "  Inner Exception : " + Ex.InnerException.ToString() };
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}