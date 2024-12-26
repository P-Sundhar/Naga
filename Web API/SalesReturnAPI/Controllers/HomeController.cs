using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SalesReturnAPI.Controllers
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
                else {
                    var plant = objMas.GetDataTable("select a.PlantId,b.PlantName,b.ShortName,b.PlantType,b.PlantAlias,b.PlantTypeFlag from deviceuser_master a, plant_master b " +
                        "where a.DeviceId = '" + lgnmodel.Imei + "' and a.UserId='" + lgnmodel.UserId + "' and a.UserPWD='" + lgnmodel.Password
                        + "' and a.Status in (1,2) and a.PlantId=b.PlantId");

                    if (plant == null)
                    {
                        response = new ResponseHelper { status = 0, message = "Failed To Fetch Plant Details" };
                        return Json(response, JsonRequestBehavior.AllowGet);
                    }

                    response = new PlantRespose { status = 1, message = "Success", data = objMas.ConvertTo<PlantMaster>(plant) };
                }
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

        [HttpGet]
        public JsonResult GetOutlet()
        {
            var response = new OutletResponse { status = 0, message = "" };
            try
            {
                DataTable dt = objMas.GetDataTable("select OutletCode,OutletName,Status from OutletMaster where Status=1 Order by OutletName ");
                if (dt == null)
                {
                    response = new OutletResponse { status = 0, message = objMas.DBErrBuf };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }

                var data = objMas.ConvertTo<OutletModel>(dt);
                response = new OutletResponse { status = 1, message = "", data = data };
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("GetOutlet", Ex.Message.ToString());
                response = new OutletResponse { status = 0, message = Ex.Message.ToString() + "  Inner Exception : " + Ex.InnerException.ToString() };
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ValidateCrate(SalesReturn brcdedtls)
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
                ret = objMas.GetTableCount("crate_master where CrateCode='" + brcdedtls.CrateBarcode + "'");
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
        public JsonResult ValidateBarcode(SalesReturn brcdedtls)
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

                ret = objMas.GetTableCount("crate_master where CrateCode='" + brcdedtls.CrateBarcode + "'");
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
                ret = objMas.GetTableCount("" + tblname + " where Barcode='" + brcdedtls.SecondaryBarcode + "' and " +
                    (tblname == "cfagrn_details" ? "ToDeliveryNo != ''" : "DeliveryNo != ''") + "");
                if (ret == -1)
                {
                    response = new ResponseHelper { status = 0, message = objMas.DBErrBuf };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                else if (ret == 0)
                {
                    if (tblname == "cfagrn_details")
                    {
                        ret = objMas.GetTableCount("production_lines where Barcode='" + brcdedtls.SecondaryBarcode + "' and DeliveryNo != ''");
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
                ret = objMas.GetTableCount("temp_table_salesreturn where Barcode='" + brcdedtls.SecondaryBarcode + "'");
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
                ret = objMas.GetTableCount("salesreturnlines where Barcode='" + brcdedtls.SecondaryBarcode + "'");
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

                ret = objMas.GetTableCount("production_header a,production_lines b where a.PDNId = b.PDNId and " +
                    "b.Barcode='" + brcdedtls.SecondaryBarcode + "'  and Date(a.ExpDate) < Curdate()");
                if (ret == -1)
                {
                    response = new ResponseHelper { status = 0, message = "Failed To Fetch Barcode Expiry" };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }

                ret = objMas.ExecQry("insert into temp_table_salesreturn(PlantId,OutletCode,OutletName,StorageLoc,TruckNo," +
                    "CrateBarcode,Barcode,IsExpired,CreatedOn,AcceptHold)values('" + brcdedtls.PlantId + "','" +
                    brcdedtls.OutletCode + "','" + brcdedtls.OutletName + "','" + brcdedtls.RackBarcode + "','" + brcdedtls.TruckNo
                    + "','" + brcdedtls.CrateBarcode + "','" + brcdedtls.SecondaryBarcode + "'," + ret + ",now()," + brcdedtls.AcceptorHold + ")");

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
        public JsonResult GetSalesReturnDetails(SalesReturn brcdedtls)
        {
            var response = new ResponseHelper { status = 0, message = "" };
            try
            {
                var dt = objMas.GetDataTable("Select PlantId,OutletCode,OutletName,StorageLoc as RackBarcode,TruckNo," +
                         "CrateBarcode,Barcode as SecondaryBarcode,CreatedOn from temp_table_salesreturn where PlantId='" +
                         brcdedtls.PlantId + "'");

                if (dt == null)
                {
                    response = new ResponseHelper { status = 0, message = objMas.DBErrBuf };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                var data = objMas.ConvertTo<SalesReturn>(dt);
                response = new SalesReturnResponse { status = 1, message = "", data = data };
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("GetSalesReturnDetails", Ex.Message.ToString());
                response = new ResponseHelper { status = 0, message = Ex.Message.ToString() + "  Inner Exception : " + Ex.InnerException.ToString() };
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveSalesReturn(SalesReturn brcdedtls)
        {
            var response = new ResponseHelper { status = 0, message = "" };
            try
            {
                Int64 ret = 0;
                string buf = "", tblname = "";
                string[] sQry = new string[7];

                ret = objMas.GetTableCount("temp_table_salesreturn where PlantId='" + brcdedtls.PlantId + "'");
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
                ret = objMas.GetTableCount("Plant_Master where PlantId='" + brcdedtls.PlantId + "' and PlantTypeFlag=0");
                if (ret <= -1)
                {
                    response = new ResponseHelper { status = 0, message = objMas.DBErrBuf };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                tblname = ret >= 1 ? "production_lines" : "cfagrn_details";

                buf = System.DateTime.Now.ToString("ddMMyyyyHHmmss");

                if (ret >= 1) // plant
                {
                    sQry[0] = "Update production_lines a,temp_table_salesreturn b set a.Barcode= Concat(b.Barcode,'_','" + buf
                                        + "') where a.Barcode=b.Barcode and b.OutletCode = '" + brcdedtls.OutletCode + "' and b.PlantId = '" + brcdedtls.PlantId + "'";

                    sQry[1] = "Insert into production_lines(PDNId,System,TranDate,MaterialCode,Barcode,CrateBarcode,NewCrateBarcode,RackNo,PutawayDate," +
                              "TranNo,Weight,MonthYear,BatchNo,BoxNo,SaleDocNo,PickFlag,PickedOn,VehicleNo,PutAwayFlag,DeliveryNo,ShipmentNo,IsStock,Reject," +
                              "Hold,IsReserved,ReservedShipment,Status,ServerSendFlag,PutawayBy,DispatchBy,Adjusted,AuditOn,RejectedBy,RejectedOn) select a.PDNId," +
                              "a.System,a.TranDate,a.MaterialCode,Replace(a.Barcode,'_" + buf + "',''),a.CrateBarcode,a.NewCrateBarcode,a.RackNo,a.PutawayDate," +
                              "a.TranNo,a.Weight,a.MonthYear,a.BatchNo,a.BoxNo,a.SaleDocNo,a.PickFlag,a.PickedOn,a.VehicleNo,a.PutAwayFlag,a.DeliveryNo," +
                              "a.ShipmentNo,a.IsStock,a.Reject,a.Hold,a.IsReserved,a.ReservedShipment,a.Status,a.ServerSendFlag,a.PutawayBy,a.DispatchBy," +
                              "a.Adjusted,a.AuditOn,a.RejectedBy,a.RejectedOn from production_lines a, temp_table_salesreturn b where a.Barcode=Concat(b.Barcode,'_','" +
                              buf + "') and b.OutletCode = '" + brcdedtls.OutletCode + "' and b.PlantId = '" + brcdedtls.PlantId + "'";

                    sQry[2] = "Update production_lines a,temp_table_salesreturn b set a.pickflag=0,a.IsStock=1,a.PickedOn='0001-01-01 00:00:00',a.VehicleNo='',a.DeliveryNo='',a.ShipmentNo=''," +
                              "DispatchBy='' where a.Barcode=b.Barcode and b.IsExpired=0 and b.AcceptHold = 1 and b.OutletCode = '" +
                              brcdedtls.OutletCode + "' and b.PlantId = '" + brcdedtls.PlantId + "'";

                    sQry[3] = "Update production_lines a,temp_table_salesreturn b set a.Hold=1 where a.Barcode=b.Barcode and (b.IsExpired=1 or AcceptHold = 2) and " +
                             "b.OutletCode = '" + brcdedtls.OutletCode + "' and b.PlantId = '" + brcdedtls.PlantId + "'";
                }

                else // cfa plant
                {
                    

                    sQry[0] = "Update cfagrn_details a,temp_table_salesreturn b set a.Barcode= Concat(b.Barcode,'_','" + buf
                                    + "') where a.Barcode=b.Barcode and b.OutletCode = '" + brcdedtls.OutletCode + "' and b.PlantId = '" + brcdedtls.PlantId + "'";

                    sQry[1] = "Insert into cfagrn_details(GRNNO,GRNDate,TransferGRNNO,TransferGRNDate,FromShipmentNo,FromDeliveryNo,CfaGrnShipmentNo,CfagrnDeliveryNo," +
                              "FromPlant,ReceivingPlant,StockTransferPlant,PDNDate,MaterialCode,Barcode,CrateBarcode,NewCrateBarcode,RackNo,PutawayDate,PutAwayFlag," +
                              "Weight,BatchNo,PickFlag,CfaPickFlag,SAPGRNFlag,PickedOn,ToShipmentNo,ToDeliveryNo,TransferDeliveryNo,TransferShipmentNo,Reject,Hold," +
                              "IsReserved,ReservedShipment,Status,ServerSendFlag,MissingFlag,DispatchBy,StockTransferBy,StockTransferOn,StockReceivedBy,StockReceivedOn," +
                              "RejectedBy,RejectedOn,Adjusted,AuditOn,CreatedBy,CreatedOn) select a.GRNNO,a.GRNDate,a.TransferGRNNO,a.TransferGRNDate,a.FromShipmentNo," +
                              "a.FromDeliveryNo,a.CfaGrnShipmentNo,a.CfagrnDeliveryNo,a.FromPlant,a.ReceivingPlant,a.StockTransferPlant,a.PDNDate,a.MaterialCode," +
                              "Replace(a.Barcode,'_" + buf + "',''),a.CrateBarcode,a.NewCrateBarcode,a.RackNo,a.PutawayDate,a.PutAwayFlag,a.Weight,a.BatchNo,a.PickFlag," +
                              "a.CfaPickFlag,a.SAPGRNFlag,a.PickedOn,a.ToShipmentNo,a.ToDeliveryNo,a.TransferDeliveryNo,a.TransferShipmentNo,a.Reject,a.Hold,a.IsReserved," +
                              "a.ReservedShipment,a.Status,a.ServerSendFlag,a.MissingFlag,a.DispatchBy,a.StockTransferBy,a.StockTransferOn,a.StockReceivedBy,a.StockReceivedOn,a.RejectedBy," +
                              "a.RejectedOn,a.Adjusted,a.AuditOn,a.CreatedBy,a.CreatedOn from cfagrn_details a, temp_table_salesreturn b where " +
                               "a.Barcode=Concat(b.Barcode,'_','" + buf + "') and b.OutletCode = '" + brcdedtls.OutletCode + "' and b.PlantId = '" + brcdedtls.PlantId + "'";

                    sQry[2] = "Update cfagrn_details a,temp_table_salesreturn b set a.PickedOn='',a.CfaPickflag=0,a.ToDeliveryNo='',a.ToShipmentNo=''," +
                              "DispatchBy='' where a.Barcode=b.Barcode and  b.IsExpired=0 and b.AcceptHold = 1 and b.OutletCode = '" + 
                              brcdedtls.OutletCode + "' and b.PlantId = '" + brcdedtls.PlantId + "'";

                    sQry[3] = "Update cfagrn_details a,temp_table_salesreturn b set a.Hold=1 where a.Barcode=b.Barcode and (b.IsExpired=1 or AcceptHold = 2)  " +
                              "and b.OutletCode = '" + brcdedtls.OutletCode + "' and b.PlantId = '" + brcdedtls.PlantId + "'";
                }

                sQry[4] = "insert into salesreturnheader(TranNo,OutletCode,OutletName,ReceivingPlant,StorageLoc,TruckNo,CreatedOn,Status)select '" +
                          buf + "',OutletCode,OutletName,PlantId,StorageLoc,TruckNo,now(),1 from temp_table_salesreturn where " +
                          "PlantId='" + brcdedtls.PlantId + "' and OutletCode = '" + brcdedtls.OutletCode + "' limit 1";

                sQry[5] = "insert into salesreturnlines(TranNo,Barcode,CrateBarcode,CreatedOn,IsExpired,AcceptHold)select '" +
                          buf + "',Barcode,CrateBarcode,CreatedOn,IsExpired,AcceptHold from temp_table_salesreturn where " +
                          "PlantId='" + brcdedtls.PlantId + "' and OutletCode = '" + brcdedtls.OutletCode + "'";

                sQry[6] = "Delete from temp_table_salesreturn where PlantId='" + brcdedtls.PlantId + "' and OutletCode = '" + brcdedtls.OutletCode + "'";

                ret = objMas.ExecMultipleQry(7, sQry);
                if (ret == -1)
                    response = new ResponseHelper { status = 0, message = objMas.DBErrBuf };
                else
                    response = new ResponseHelper { status = 1, message = "Success" };
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("SaveSalesReturn", Ex.Message.ToString());
                response = new ResponseHelper { status = 0, message = Ex.Message.ToString() + "  Inner Exception : " + Ex.InnerException.ToString() };
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}