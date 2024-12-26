using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace NagaSapService
{
    public partial class Service1 : ServiceBase
    {
        MasterLogic objMas = new MasterLogic();
        Webservice webservice = new Webservice();
        public Timer Service_Time;
        Int64 Intervel = Convert.ToInt32(ConfigurationManager.AppSettings["Intervel"].ToString());

        public Service1()
        {
            InitializeComponent();
            log4net.Config.XmlConfigurator.Configure();
            Service_Time = new Timer(Intervel);
            Service_Time.Enabled = true;
            Service_Time.Interval = Intervel;
            Service_Time.AutoReset = true;
            Service_Time.Start();
            Service_Time.Elapsed += new ElapsedEventHandler(Service_Time_Tick);

        }

        private void Service_Time_Tick(object Sender, ElapsedEventArgs e)
        {
            try
            {
                objMas.PrintLog(" Version", ""  + Globals.APPLN_VERSION);
                Service_Time.Enabled = false;
                Service_Time.Stop();
                objMas.PrintLog(" Timer Stopped", "");
                DeleteDebugFiles();
                SAPProcess();
                Service_Time.Enabled = true;
                Service_Time.Start();
                objMas.PrintLog(" Timer Started", "");
            }
            catch (Exception Ex)
            {
                objMas.PrintLog(" Timer Thick Error : C[1,1]", Ex.Message.ToString());
            }
        }

        public void DeleteDebugFiles()
        {
            try
            {

                int ret, Days;
                string flPath = "";
                DateTime CurrDate;

                // Delete log files
                Days = Convert.ToInt16(ConfigurationManager.AppSettings["DebugDays"]);
                ret = Days == 0 ? -10 : Days * -1;
                CurrDate = DateTime.Now.AddDays(ret);
                flPath = AppDomain.CurrentDomain.BaseDirectory + "\\Log\\";
                string[] sLogs = Directory.GetFiles(flPath);
                foreach (string sfile in sLogs)
                {
                    if (sfile.StartsWith(flPath + "Debug") == false)
                        continue;
                    FileInfo fi = new FileInfo(sfile);
                    if (fi.CreationTime <= CurrDate)
                        fi.Delete();
                }
            }
            catch (Exception Ex)
            {
                objMas.PrintLog(" Delete Debug file Error - C[1,17] : ", Ex.Message.ToString());
            }
        }

        protected override void OnStart(string[] args)
        {
            objMas.PrintLog(" Timer Started", "OnStart");
            Service_Time.Enabled = true;
        }
        protected override void OnStop()
        {
            objMas.PrintLog(" Timer Stopped", "OnStop");
            Service_Time.Enabled = false;
        }
        public void SAPProcess()
        {
            try
            {
                SendProductionToSAP(); // Production To SAP
                GetOrderFromSAP(); // Getting SAP Order To Barcode 
                PostSTODispatchToSAP(); // STO Delivery To SAP
                PostCFAGRNToSAP(); //CFA GRN TO SAP
                PostNoBarcodeStockToSAP(); // Stock No Barcode To SAP
                PostCFADispatchToSAP(); //CFA Sales Delivery To SAP
                PostPlantSalesToSAP(); //Plant Sales Deleivery To SAP
                PostPlantSalesReturnToSAP(); //Plant Sales Return To SAP
            }
            catch (Exception Ex)
            {
                objMas.PrintLog("Catche Error : C[1,1] ", Ex.Message.ToString());
            }
        }
        private void SendProductionToSAP()
        {
            try
            {
                Int64 ret = 0;

                List<ProductionDetails> proddetails = new List<ProductionDetails>();

                DataTable dt = objMas.GetDataTable("select BatchNo,a.MaterialCode,ChefCnt,QCCnt,DamageCnt,RndCnt,ExcessCnt," +
                    "Date_format(PdnDate,'%d.%m.%Y') as ManufacturingDate,PdnId,ShiftId,(a.PDNCnt * b.Denominator) as PrdCnt " +
                    "from production_header a,material_master b where a.SapFlag=0 " +
                    "and a.MaterialCode = b.MaterialCode and ServerSendFlag=1");

                if (dt.Rows.Count == 0)
                    return;

                foreach (DataRow dr in dt.Rows)
                    proddetails.Add(new ProductionDetails
                    {
                        batch = dr["BatchNo"].ToString(),
                        materialcode = dr["MaterialCode"].ToString(),
                        chef_sample_qty = Convert.ToInt16(dr["ChefCnt"].ToString()),
                        order_number = dr["PdnId"].ToString(),
                        shift = dr["ShiftId"].ToString(),
                        damaged_quantity = Convert.ToInt16(dr["DamageCnt"].ToString()),
                        manufacturing_date = dr["ManufacturingDate"].ToString(),
                        Packed_quantity=Convert.ToInt16(dr["PrdCnt"]),
                        RD_sample_qty=Convert.ToInt16(dr["RndCnt"]),
                        qc_rejection=Convert.ToInt16(dr["QCCnt"])
                    });
                   
                string rsp = webservice.WebApiPostRequest(Globals.MTHD_PRODUCTION, JsonConvert.SerializeObject(proddetails)).Result;
                if (rsp != "") 
                {
                    var rspdata = JsonConvert.DeserializeObject<List<ProductionDetails>>(rsp);
                    if (rspdata.Count == 0)
                        return;
                    string PdnId = string.Join(",", rspdata.Select(x => x.order_number).ToArray());

                    ret = objMas.ExecQry("Update production_header set SapFlag=1 where PdnId in ("+ PdnId +")");
                    if (ret == -1)
                    {
                        objMas.PrintLog("Error : ", objMas.DBErrBuf);
                        return;
                    }
                }


            }
            catch (Exception Ex)
            {
                objMas.PrintLog("Catche Error : C[1,2] ", Ex.Message.ToString());
            }
        }
        private void GetOrderFromSAP()
        {
            try
            {
                Int64 ret = 0;
                int k = 0;
                string data = "";



                data = webservice.WebApiGetRequest(Globals.MTHD_DELIVERYORDER).Result;
                if (data == null)
                    return;

                objMas.PrintLog("Get Order Response", data);
                string[] sQry = new string[1];

                var rsp = JsonConvert.DeserializeObject<List<DispatchOrderHeader>>(data);
                if (rsp == null)
                    return;

                for (int i = 0; i < rsp.Count; i++)
                {
                    ret = objMas.GetTableCount("sapdispatchorderheader where DELIVERY_NO = '" + rsp[i].DELIVERY_NO + "'");
                    if (ret >= 1)
                        continue;

                    k = 0;

                    Array.Resize(ref sQry, rsp[i].LINE_ITEM.Count + 1);
                    Array.Clear(sQry, 0, rsp[i].LINE_ITEM.Count + 1);

                    sQry[k] = "Insert into sapdispatchorderheader(DELIVERY_NO,TYPES_OF_TRANSACTION,SHIPMENT_OR_PO,DELIVERY_DATE," +
                        "SENDING_PLANT,SENDING_LOCATION,RECEIVING_PLANT,RECEIVING_LOCATION,CreatedOn,TRUCK_NUMBER,OUTLET_NAME,INDENT_NUMBER)Values('" +
                        rsp[i].DELIVERY_NO + "','" + rsp[i].TYPES_OF_TRANSACTION + "','" +  rsp[i].SHIPMENT_OR_PO + "','" +  rsp[i].DELIVERY_DATE + "','" +
                        rsp[i].SENDING_PLANT + "','" + rsp[i].SENDING_LOCATION + "','" + rsp[i].RECEIVING_PLANT + "','" +  rsp[i].RECEIVING_LOCATION
                        + "',now(),'" + rsp[i].TRUCK_NUMBER + "','" + rsp[i].OUTLET_NAME + "','" + rsp[i].INDENT_NUMBER + "')";

                    for (k = 1; k <= rsp[i].LINE_ITEM.Count; k++)
                        sQry[k] = "Insert into sapdispatchorderlines(DELIVERY_NO,LINE,MATERIAL_CODE,DELIVERY_QTY_IN_BOX)values('" +
                            rsp[i].DELIVERY_NO + "','" + rsp[i].LINE_ITEM[k-1].LINE +"','" + rsp[i].LINE_ITEM[k-1].MATERIAL_CODE
                            + "','" + rsp[i].LINE_ITEM[k-1].DELIVERY_QTY_IN_BOX + "')";

                    ret = objMas.ExecMultipleQry(k, sQry);
                    if (ret == -1)
                    {
                        return;
                    }
                    objMas.PrintLog("Sap DispatchOrderDetails :", "Number of Rows Inserted :"+ ret);
                }

            }            
            catch (Exception Ex)
            {
                objMas.PrintLog("Catche Error : C[1,3] ", Ex.Message.ToString());
            }
        }
        private void PostSTODispatchToSAP()
        {
            try
            {
                Int64 ret = 0;

                List<DispatchSTOHeaderDetails> stodispatchdetails;
                List<DispatchSTOLineDetails> stolinedetails;
                List<DispatchSTOBatchDetails> stobatchdetails;

                DataTable dt = objMas.GetDataTable("select TYPES_OF_TRANSACTION,SHIPMENT_OR_PO,DELIVERY_NO,DELIVERY_DATE from sapdispatchorderheader " +
                    "where PickFlag=1 and SapDispatchFlag=0 and TYPES_OF_TRANSACTION='STO'");

                if (dt.Rows.Count == 0)
                    return;

                stodispatchdetails = new List<DispatchSTOHeaderDetails>();

                for (int i = 0;i < dt.Rows.Count;i++)
                {
                    DataTable dtlineItems = objMas.GetDataTable("select Distinct MaterialCode,b.LINE from production_lines a,sapdispatchorderlines b where a.ShipmentNo = '" +
                                                dt.Rows[i]["SHIPMENT_OR_PO"].ToString() + "' and a.DeliveryNo = '" + dt.Rows[i]["DELIVERY_NO"].ToString() 
                                                +"' and a.MaterialCode=b.MATERIAL_CODE and b.DELIVERY_NO = '" + dt.Rows[i]["DELIVERY_NO"].ToString() + "'");
                    
                    if (dtlineItems.Rows.Count == 0)
                        continue;

                    stolinedetails = new List<DispatchSTOLineDetails>();
                    for (int j = 0; j < dtlineItems.Rows.Count; j++)
                    {
                        DataTable dtbatches = objMas.GetDataTable("select MaterialCode,BatchNo,Barcode,CrateBarcode from production_lines where ShipmentNo='"+
                                        dt.Rows[i]["SHIPMENT_OR_PO"] +"' and DeliveryNo = '"+ dt.Rows[i]["DELIVERY_NO"].ToString() 
                                        +"' and MaterialCode='"+ dtlineItems.Rows[j]["MaterialCode"] +"' Order by TranDate");

                        if (dtbatches.Rows.Count == 0)
                            continue;

                        stobatchdetails = new List<DispatchSTOBatchDetails>();
                        for (int k = 0; k < dtbatches.Rows.Count; k++)
                        {
                            
                            stobatchdetails.Add(new DispatchSTOBatchDetails
                            {
                                SNo = (k + 1) +"",
                                Secondary_batch = dtbatches.Rows[k]["Barcode"].ToString(),
                                Primary_batch = dtbatches.Rows[k]["BatchNo"].ToString(),
                                CRATE_NUMBER = dtbatches.Rows[k]["CrateBarcode"].ToString(),
                                Batch_Qty_in_box = "1"
                            });

                        }
                        stolinedetails.Add(new DispatchSTOLineDetails { LINE_ITEM = dtlineItems.Rows[j]["LINE"].ToString(), Material_Code = dtlineItems.Rows[j]["MaterialCode"].ToString(), batches = stobatchdetails });
                    }
                    stodispatchdetails.Add(new DispatchSTOHeaderDetails
                    {
                        Type_of_transaction = dt.Rows[i]["TYPES_OF_TRANSACTION"].ToString(),
                        Shipment_or_po = dt.Rows[i]["SHIPMENT_OR_PO"].ToString(),
                        Delivery_No = dt.Rows[i]["DELIVERY_NO"].ToString(),
                        Delivery_Date = dt.Rows[i]["DELIVERY_DATE"].ToString(),
                        LINE = stolinedetails
                    });
                }

                if (stodispatchdetails.Count == 0)
                {
                    objMas.PrintLog("Message :", "No Pending Orders To Post");
                    return;
                }
                objMas.PrintLog("STO Request :", JsonConvert.SerializeObject(stodispatchdetails));
                string rsp = webservice.WebApiPostRequest(Globals.MTHD_DELIVER, JsonConvert.SerializeObject(stodispatchdetails)).Result;
                if (rsp != "")
                {
                    objMas.PrintLog("STO Response : ", rsp);
                    var rspdata = JsonConvert.DeserializeObject<List<DispatchSTOHeaderDetails>>(rsp);
                    if (rspdata.Count == 0)
                        return;

                    string DeliveryNo = string.Join(",", rspdata.Select(x => x.Delivery_No).ToArray());

                    ret = objMas.ExecQry("Update sapdispatchorderheader set SapDispatchFlag=1 where DELIVERY_NO in ("+ DeliveryNo +")");
                    if (ret == -1)
                    {
                        objMas.PrintLog("Error : ", objMas.DBErrBuf);
                        return;
                    }
                }

            }
            catch (Exception Ex)
            {
                objMas.PrintLog("Catche Error : ", Ex.Message.ToString());
            }
        }
        private void PostCFAGRNToSAP()
        {
            try
            {
                Int64 ret = 0;

                List<GRNHeaderDetails> grndetails;
                List<DispatchSTOLineDetails> grnlinedetails;
                List<DispatchSTOBatchDetails> grnbatchdetails;

                DataTable dt = objMas.GetDataTable("select Distinct FromDeliveryNo,ReceivingPlant,Date_format(GrnDate,'%d.%m.%Y') as ReceivingDate from cfagrn_details where SAPGRNFlag = 0 and MissingFlag=0");

                if (dt.Rows.Count == 0)
                    return;

                grndetails  = new List<GRNHeaderDetails>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataTable dtlineItems = objMas.GetDataTable("select Distinct MaterialCode from cfagrn_details where FromDeliveryNo = '" + 
                                            dt.Rows[i]["FromDeliveryNo"].ToString() +"' and MissingFlag=0");

                    if (dtlineItems.Rows.Count == 0)
                        continue;

                    grnlinedetails = new List<DispatchSTOLineDetails>();
                    for (int j = 0; j < dtlineItems.Rows.Count; j++)
                    {
                        DataTable dtbatches = objMas.GetDataTable("select MaterialCode,BatchNo,Barcode,CrateBarcode from cfagrn_details where FromDeliveryNo = '"+ 
                            dt.Rows[i]["FromDeliveryNo"].ToString()  +"' and MaterialCode='"+ dtlineItems.Rows[j]["MaterialCode"] +"' and MissingFlag=0");

                        if (dtbatches.Rows.Count == 0)
                            continue;

                        grnbatchdetails = new List<DispatchSTOBatchDetails>();

                        for (int k = 0; k < dtbatches.Rows.Count; k++)
                            grnbatchdetails.Add(new DispatchSTOBatchDetails
                            {
                                SNo = (k + 1) +"",
                                Secondary_batch = dtbatches.Rows[k]["Barcode"].ToString(),
                                Primary_batch = dtbatches.Rows[k]["BatchNo"].ToString(),
                                CRATE_NUMBER = dtbatches.Rows[k]["CrateBarcode"].ToString(),
                                Batch_Qty_in_box = "1"
                            });

                        grnlinedetails.Add(new DispatchSTOLineDetails { LINE_ITEM = (j + 1) + "", Material_Code = dtlineItems.Rows[j]["MaterialCode"].ToString(), batches = grnbatchdetails });
                    }
                    grndetails.Add(new GRNHeaderDetails
                    {
                        Delivery_No = dt.Rows[i]["FromDeliveryNo"].ToString(),
                        Receiving_plant = dt.Rows[i]["ReceivingPlant"].ToString(),
                        Receiving_Date = dt.Rows[i]["ReceivingDate"].ToString(),
                        LINE = grnlinedetails
                    });
                }

                if (grndetails.Count == 0)
                {
                    objMas.PrintLog("Message :", "No Pending Orders To Post");
                    return;
                }

                string rsp = webservice.WebApiPostRequest(Globals.MTHD_RECEIVING, JsonConvert.SerializeObject(grndetails)).Result;
                if (rsp != "")
                {
                    objMas.PrintLog("CFAGRN Response : ", rsp);
                    var rspdata = JsonConvert.DeserializeObject<List<GRNHeaderDetails>>(rsp);
                    if (rspdata.Count == 0)
                        return;

                    string DeliveryNo = string.Join(",", rspdata.Select(x => x.Delivery_No).ToArray());

                    ret = objMas.ExecQry("Update cfagrn_details set SAPGRNFlag=1 where FromDeliveryNo in ("+ DeliveryNo +")");
                    if (ret == -1)
                    {
                        objMas.PrintLog("Error : ", objMas.DBErrBuf);
                        return;
                    }
                }

            }
            catch (Exception Ex)
            {
                objMas.PrintLog("Catche Error : ", Ex.Message.ToString());
            }
        }
        private void PostNoBarcodeStockToSAP()
        {
            try
            {
                Int64 ret = 0;
                string Barcodes = "";
                List<GRNHeaderDetails> grndetails;
                List<DispatchSTOLineDetails> grnlinedetails;
                List<DispatchSTOBatchDetails> grnbatchdetails;

                DataTable dt = objMas.GetDataTable("select Distinct PlantId,Date_format(CreatedOn,'%d.%m.%Y') as ReceivingDate from stock_nobarcode where SAPGRNFlag = 0");

                if (dt.Rows.Count == 0)
                    return;

                grndetails  = new List<GRNHeaderDetails>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataTable dtlineItems = objMas.GetDataTable("select Distinct MaterialCode from stock_nobarcode where PlantId = '" +
                                            dt.Rows[i]["PlantId"].ToString() +"' and SAPGRNFlag = 0");

                    if (dtlineItems.Rows.Count == 0)
                        continue;

                    grnlinedetails = new List<DispatchSTOLineDetails>();
                    for (int j = 0; j < dtlineItems.Rows.Count; j++)
                    {
                        DataTable dtbatches = objMas.GetDataTable("select MaterialCode,BatchNo,Barcode,PdnCnt from stock_nobarcode where PlantId = '"+
                            dt.Rows[i]["PlantId"].ToString()  +"' and MaterialCode='"+ dtlineItems.Rows[j]["MaterialCode"] +"' and SAPGRNFlag = 0");

                        if (dtbatches.Rows.Count == 0)
                            continue;

                        grnbatchdetails = new List<DispatchSTOBatchDetails>();

                        for (int k = 0; k < dtbatches.Rows.Count; k++)
                        {
                            grnbatchdetails.Add(new DispatchSTOBatchDetails
                            {
                                SNo = (k + 1) +"",
                                Secondary_batch = dtbatches.Rows[k]["Barcode"].ToString(),
                                Primary_batch = dtbatches.Rows[k]["BatchNo"].ToString(),
                                Batch_Qty_in_box = dtbatches.Rows[k]["PdnCnt"].ToString(),
                            });
                            Barcodes += "'"+ dtbatches.Rows[k]["Barcode"].ToString() + "',";
                        }

                        grnlinedetails.Add(new DispatchSTOLineDetails { LINE_ITEM = (j + 1) + "", Material_Code = dtlineItems.Rows[j]["MaterialCode"].ToString(), batches = grnbatchdetails });
                    }
                    grndetails.Add(new GRNHeaderDetails
                    {
                        Delivery_No = "",
                        Receiving_plant = dt.Rows[i]["PlantId"].ToString(),
                        Receiving_Date = dt.Rows[i]["ReceivingDate"].ToString(),
                        LINE = grnlinedetails
                    });
                }

                if (grndetails.Count == 0)
                {
                    objMas.PrintLog("Message :", "No Pending Orders To Post");
                    return;
                }

                string rsp = webservice.WebApiPostRequest(Globals.MTHD_RECEIVING, JsonConvert.SerializeObject(grndetails)).Result;
                if (rsp != "")
                {
                    objMas.PrintLog("CFAGRN Response : ", rsp);
                    var rspdata = JsonConvert.DeserializeObject<List<GRNHeaderDetails>>(rsp);
                    if (rspdata.Count == 0)
                        return;

                    if (Barcodes.Length >= 1)
                        Barcodes = Barcodes.Substring(0, Barcodes.Length-1);
                    ret = objMas.ExecQry("Update stock_nobarcode set SAPGRNFlag=1 where Barcode in ("+ Barcodes +") and SAPGRNFlag=0");
                    if (ret == -1)
                    {
                        objMas.PrintLog("Error : ", objMas.DBErrBuf);
                        return;
                    }
                }

            }
            catch (Exception Ex)
            {
                objMas.PrintLog("Catche Error : ", Ex.Message.ToString());
            }
        }
        private void PostCFADispatchToSAP()
        {
            try
            {
                Int64 ret = 0;

                List<DispatchSTOHeaderDetails> stodispatchdetails;
                List<DispatchSTOLineDetails> stolinedetails;
                List<DispatchSTOBatchDetails> stobatchdetails;

                DataTable dt = objMas.GetDataTable("select TYPES_OF_TRANSACTION,SHIPMENT_OR_PO,DELIVERY_NO,DELIVERY_DATE from sapdispatchorderheader " +
                    "where PickFlag=1 and SapDispatchFlag=0 and TYPES_OF_TRANSACTION='SALES'");

                if (dt.Rows.Count == 0)
                    return;

                stodispatchdetails = new List<DispatchSTOHeaderDetails>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataTable dtlineItems = objMas.GetDataTable("select Distinct MaterialCode,Line from (select Distinct MaterialCode," +
                        "b.LINE from cfagrn_details a,sapdispatchorderlines b where a.ToShipmentNo = '" +
                        dt.Rows[i]["SHIPMENT_OR_PO"].ToString() + "' and a.ToDeliveryNo = '" + 
                        dt.Rows[i]["DELIVERY_NO"].ToString() + "' and a.MaterialCode=b.MATERIAL_CODE and b.DELIVERY_NO = '" + 
                        dt.Rows[i]["DELIVERY_NO"].ToString() + "' UNION ALL select Distinct MaterialCode,b.LINE from " +
                        "stocknobarcode_dispatchdetails a,sapdispatchorderlines b where a.ShipmentNo = '"+ 
                        dt.Rows[i]["SHIPMENT_OR_PO"].ToString()+"' and a.DeliveryNo = '" + dt.Rows[i]["DELIVERY_NO"].ToString() 
                        + "' and a.MaterialCode=b.MATERIAL_CODE and b.DELIVERY_NO = '" + dt.Rows[i]["DELIVERY_NO"].ToString() + "')a");

                    if (dtlineItems.Rows.Count == 0)
                        continue;

                    stolinedetails = new List<DispatchSTOLineDetails>();
                    for (int j = 0; j < dtlineItems.Rows.Count; j++)
                    {
                        DataTable dtbatches = objMas.GetDataTable("select MaterialCode,BatchNo,Barcode,CASE NewCrateBarcode WHEN '' " +
                            "THEN CrateBarcode ELSE NewCrateBarcode END as CrateBarcode from cfagrn_details where ToShipmentNo='"+
                                        dt.Rows[i]["SHIPMENT_OR_PO"] +"' and ToDeliveryNo = '"+ dt.Rows[i]["DELIVERY_NO"].ToString()
                                        +"' and MaterialCode='"+ dtlineItems.Rows[j]["MaterialCode"] +"' UNION ALL " +
                                        "select MaterialCode,BatchNo,Barcode,CrateBarcode from stocknobarcode_dispatchdetails where " +
                                        "ShipmentNo='" + dt.Rows[i]["SHIPMENT_OR_PO"] + "' and DeliveryNo = '" +
                                        dt.Rows[i]["DELIVERY_NO"].ToString() + "' and MaterialCode='" + dtlineItems.Rows[j]["MaterialCode"] + "'");

                        if (dtbatches.Rows.Count == 0)
                            continue;

                        stobatchdetails = new List<DispatchSTOBatchDetails>();
                        for (int k = 0; k < dtbatches.Rows.Count; k++)
                        {

                            stobatchdetails.Add(new DispatchSTOBatchDetails
                            {
                                SNo = (k + 1) +"",
                                Secondary_batch = dtbatches.Rows[k]["Barcode"].ToString(),
                                Primary_batch = dtbatches.Rows[k]["BatchNo"].ToString(),
                                CRATE_NUMBER = dtbatches.Rows[k]["CrateBarcode"].ToString(),
                                Batch_Qty_in_box = "1"
                            });
                        }
                        stolinedetails.Add(new DispatchSTOLineDetails { LINE_ITEM = dtlineItems.Rows[j]["LINE"].ToString(), Material_Code = dtlineItems.Rows[j]["MaterialCode"].ToString(), batches = stobatchdetails });
                    }
                    stodispatchdetails.Add(new DispatchSTOHeaderDetails
                    {
                        Type_of_transaction = dt.Rows[i]["TYPES_OF_TRANSACTION"].ToString(),
                        Shipment_or_po = dt.Rows[i]["SHIPMENT_OR_PO"].ToString(),
                        Delivery_No = dt.Rows[i]["DELIVERY_NO"].ToString(),
                        Delivery_Date = dt.Rows[i]["DELIVERY_DATE"].ToString(),
                        LINE = stolinedetails
                    });
                }

                if (stodispatchdetails.Count == 0)
                {
                    objMas.PrintLog("Message :", "No Pending Orders To Post");
                    return;
                }
                objMas.PrintLog("CFA DISPATCH SALES Request :", JsonConvert.SerializeObject(stodispatchdetails));
                string rsp = webservice.WebApiPostRequest(Globals.MTHD_DELIVER, JsonConvert.SerializeObject(stodispatchdetails)).Result;
                if (rsp != "")
                {
                    objMas.PrintLog("CFA DISPATCH SALES  Response : ", rsp);
                    var rspdata = JsonConvert.DeserializeObject<List<DispatchSTOHeaderDetails>>(rsp);
                    if (rspdata.Count == 0)
                        return;

                    string DeliveryNo = string.Join(",", rspdata.Select(x => x.Delivery_No).ToArray());

                    ret = objMas.ExecQry("Update sapdispatchorderheader set SapDispatchFlag=1 where DELIVERY_NO in ("+ DeliveryNo +")");
                    if (ret == -1)
                    {
                        objMas.PrintLog("Error : ", objMas.DBErrBuf);
                        return;
                    }
                }

            }
            catch (Exception Ex)
            {
                objMas.PrintLog("Catche Error : ", Ex.Message.ToString());
            }
        }
        private void PostPlantSalesToSAP()
        {
            try
            {
                Int64 ret = 0;

                List<DispatchSTOHeaderDetails> stodispatchdetails;
                List<DispatchSTOLineDetails> stolinedetails;
                List<DispatchSTOBatchDetails> stobatchdetails;

                DataTable dt = objMas.GetDataTable("select TYPES_OF_TRANSACTION,SHIPMENT_OR_PO,DELIVERY_NO,DELIVERY_DATE from sapdispatchorderheader " +
                    "where PickFlag=1 and SapDispatchFlag=0 and TYPES_OF_TRANSACTION='SALES'");

                if (dt.Rows.Count == 0)
                    return;

                stodispatchdetails = new List<DispatchSTOHeaderDetails>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataTable dtlineItems = objMas.GetDataTable("select Distinct MaterialCode,b.LINE from production_lines a,sapdispatchorderlines b where a.ShipmentNo = '" +
                                                dt.Rows[i]["SHIPMENT_OR_PO"].ToString() + "' and a.DeliveryNo = '" + dt.Rows[i]["DELIVERY_NO"].ToString()
                                                + "' and a.MaterialCode=b.MATERIAL_CODE and b.DELIVERY_NO = '" + dt.Rows[i]["DELIVERY_NO"].ToString() + "'");

                    if (dtlineItems.Rows.Count == 0)
                        continue;

                    stolinedetails = new List<DispatchSTOLineDetails>();
                    for (int j = 0; j < dtlineItems.Rows.Count; j++)
                    {
                        DataTable dtbatches = objMas.GetDataTable("select MaterialCode,BatchNo,Barcode,NewCrateBarcode from production_lines where ShipmentNo='"+
                                        dt.Rows[i]["SHIPMENT_OR_PO"] +"' and DeliveryNo = '"+ dt.Rows[i]["DELIVERY_NO"].ToString()
                                        +"' and MaterialCode='"+ dtlineItems.Rows[j]["MaterialCode"] +"'");

                        if (dtbatches.Rows.Count == 0)
                            continue;

                        stobatchdetails = new List<DispatchSTOBatchDetails>();
                        for (int k = 0; k < dtbatches.Rows.Count; k++)
                        {

                            stobatchdetails.Add(new DispatchSTOBatchDetails
                            {
                                SNo = (k + 1) +"",
                                Secondary_batch = dtbatches.Rows[k]["Barcode"].ToString(),
                                Primary_batch = dtbatches.Rows[k]["BatchNo"].ToString(),
                                CRATE_NUMBER = dtbatches.Rows[k]["NewCrateBarcode"].ToString(),
                                Batch_Qty_in_box = "1"
                            });
                        }
                        stolinedetails.Add(new DispatchSTOLineDetails { LINE_ITEM = dtlineItems.Rows[j]["LINE"].ToString(), Material_Code = dtlineItems.Rows[j]["MaterialCode"].ToString(), batches = stobatchdetails });
                    }
                    stodispatchdetails.Add(new DispatchSTOHeaderDetails
                    {
                        Type_of_transaction = dt.Rows[i]["TYPES_OF_TRANSACTION"].ToString(),
                        Shipment_or_po = dt.Rows[i]["SHIPMENT_OR_PO"].ToString(),
                        Delivery_No = dt.Rows[i]["DELIVERY_NO"].ToString(),
                        Delivery_Date = dt.Rows[i]["DELIVERY_DATE"].ToString(),
                        LINE = stolinedetails
                    });
                }

                if (stodispatchdetails.Count == 0)
                {
                    objMas.PrintLog("Message :", "No Pending Orders To Post");
                    return;
                }
                objMas.PrintLog("Plant DISPATCH SALES Request :", JsonConvert.SerializeObject(stodispatchdetails));
                string rsp = webservice.WebApiPostRequest(Globals.MTHD_DELIVER, JsonConvert.SerializeObject(stodispatchdetails)).Result;
                if (rsp != "")
                {
                    objMas.PrintLog("Plant DISPATCH SALES  Response : ", rsp);
                    var rspdata = JsonConvert.DeserializeObject<List<DispatchSTOHeaderDetails>>(rsp);
                    if (rspdata.Count == 0)
                        return;

                    string DeliveryNo = string.Join(",", rspdata.Select(x => x.Delivery_No).ToArray());

                    ret = objMas.ExecQry("Update sapdispatchorderheader set SapDispatchFlag=1 where DELIVERY_NO in ("+ DeliveryNo +")");
                    if (ret == -1)
                    {
                        objMas.PrintLog("Error : ", objMas.DBErrBuf);
                        return;
                    }
                }

            }
            catch (Exception Ex)
            {
                objMas.PrintLog("Catche Error : ", Ex.Message.ToString());
            }
        }
        private void PostPlantSalesReturnToSAP()
        {
            try
            {
                Int64 ret = 0;

                List<SalesReturnDetails> salesreturndetails;
                List<SalesReturnLineDetails> salesreturnlinedetails;
                List<SalesReturnBatchDetails> salesreturnbatchdetails;
                string sQry = "", TranNo = "";

                DataTable dt = objMas.GetDataTable("select a.TranNo,OutletCode,OutletName,ReceivingPlant,StorageLoc,TruckNo,Date_Format(a.CreatedOn,'%d.%m.%Y') as " +
                    "ReceivingDate,b.PlantTypeFlag from SalesreturnHeader a, Plant_master b where a.ReceivingPlant = b.PlantId and a.SapFlag = 0");

                if (dt.Rows.Count == 0)
                    return;

                salesreturndetails = new List<SalesReturnDetails>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sQry = "select Distinct b.MaterialCode from salesreturnlines a," + (dt.Rows[i]["PlantTypeFlag"].ToString() == "0" ? "production_lines" :
                        "cfagrn_details") + " b where a.TranNo='" + dt.Rows[i]["TranNo"].ToString() + "' and a.Barcode = b.Barcode";

                    DataTable dtlineItems = objMas.GetDataTable(sQry);

                    if (dtlineItems.Rows.Count == 0)
                        continue;

                    salesreturnlinedetails = new List<SalesReturnLineDetails>();
                    for (int j = 0; j < dtlineItems.Rows.Count; j++)
                    {
                        DataTable dtbatches = objMas.GetDataTable("select a.Barcode,a.CrateBarcode,b.BatchNo,Case  a.AcceptHold WHEN 1 THEN 'Accept' ELSE 'Hold' END " +
                            "as Status from salesreturnlines a, " + (dt.Rows[i]["PlantTypeFlag"].ToString() == "0" ? "production_lines" : "cfagrn_details")
                            + " b where a.TranNo = '" + dt.Rows[i]["TranNo"].ToString() + "' and a.Barcode = b.Barcode and b.MaterialCode='" +
                            dtlineItems.Rows[j]["MaterialCode"].ToString() + "'");

                        if (dtbatches.Rows.Count == 0)
                            continue;

                        salesreturnbatchdetails = new List<SalesReturnBatchDetails>();
                        for (int k = 0; k < dtbatches.Rows.Count; k++)
                        {

                            salesreturnbatchdetails.Add(new SalesReturnBatchDetails
                            {
                                SNo = (k + 1) + "",
                                Secondary_batch = dtbatches.Rows[k]["Barcode"].ToString(),
                                Primary_batch = dtbatches.Rows[k]["BatchNo"].ToString(),
                                Crate_Number = dtbatches.Rows[k]["CrateBarcode"].ToString(),
                                Status = dtbatches.Rows[k]["Status"].ToString(),
                                Batch_Qty_in_box = "1"
                            });
                        }
                        salesreturnlinedetails.Add(new SalesReturnLineDetails { LINE = (j + 1).ToString(), MATERIAL_CODE = dtlineItems.Rows[j]["MaterialCode"].ToString(), batches = salesreturnbatchdetails });
                    }
                    salesreturndetails.Add(new SalesReturnDetails
                    {
                        TYPES_OF_TRANSACTION = "SALES_RETURN",
                        OUTLET_CODE = dt.Rows[i]["OutletCode"].ToString(),
                        OUTLET_NAME = dt.Rows[i]["OutletName"].ToString(),
                        RECEIVING_SLOC = dt.Rows[i]["StorageLoc"].ToString(),
                        RECEIVING_PLANT = dt.Rows[i]["ReceivingPlant"].ToString(),
                        RECEIVING_DATE = dt.Rows[i]["ReceivingDate"].ToString(),
                        TRUCK_NUMBER = dt.Rows[i]["TruckNo"].ToString(),
                        LINE_ITEM = salesreturnlinedetails
                    });
                    TranNo += "'" + dt.Rows[i]["TranNo"].ToString() + "',";
                }

                if (TranNo != "")
                    TranNo = TranNo.Substring(0, TranNo.Length - 1);

                if (salesreturndetails.Count == 0)
                {
                    objMas.PrintLog("Message :", "No Pending Returns To Post");
                    return;
                }
                objMas.PrintLog("Return Request :", JsonConvert.SerializeObject(salesreturndetails));
                string rsp = webservice.WebApiPostRequest(Globals.MTHD_DELIVER, JsonConvert.SerializeObject(salesreturndetails)).Result;
                if (rsp != "")
                {
                    objMas.PrintLog("Plant Return  Response : ", rsp);
                    var rspdata = JsonConvert.DeserializeObject<List<SalesReturnDetails>>(rsp);
                    if (rspdata.Count == 0)
                        return;

                    ret = objMas.ExecQry("Update SalesreturnHeader set SapFlag=1 where TranNo in (" + TranNo + ")");
                    if (ret == -1)
                    {
                        objMas.PrintLog("Error : ", objMas.DBErrBuf);
                        return;
                    }
                }

            }
            catch (Exception Ex)
            {
                objMas.PrintLog("Catche Error : ", Ex.Message.ToString());
            }
        }

        public class ProductionDetails 
        {
            public string order_number { get; set; }
            public string materialcode { get; set; }
            public int Packed_quantity { get; set; }
            public int damaged_quantity { get; set; }
            public int qc_rejection { get; set; }
            public int chef_sample_qty { get; set; }
            public int RD_sample_qty { get; set; }
            public string batch { get; set; }
            public string manufacturing_date { get; set; }
            public string shift { get; set; }

        }

        public class DispatchSTOHeaderDetails 
        {
            public string Type_of_transaction { get; set; }
            public string Shipment_or_po { get; set; }
            public string Delivery_No { get; set; }
            public string Delivery_Date { get; set; }
            public List<DispatchSTOLineDetails> LINE { get; set; }
        }

        public class GRNHeaderDetails
        {
            public string Delivery_No { get; set; }
            public string Receiving_Date { get; set; }
            public string Receiving_plant { get; set; }
            public List<DispatchSTOLineDetails> LINE { get; set; }
        }

        public class DispatchSTOLineDetails
        {
            public string LINE_ITEM { get; set; }
            public string Material_Code { get; set; }
            public List<DispatchSTOBatchDetails> batches { get; set; }
        }

        public class DispatchSTOBatchDetails
        {
            public string SNo { get; set; }
            public string Secondary_batch { get; set; }
            public string Primary_batch { get; set; }
            public string Batch_Qty_in_box { get; set; }
            public string CRATE_NUMBER { get; set; }
        }

        public class SalesReturnDetails 
        {
            public string OUTLET_CODE { get; set; }
            public string OUTLET_NAME { get; set; }
            public string TYPES_OF_TRANSACTION { get; set; }
            public string RECEIVING_PLANT { get; set; }
            public string RECEIVING_SLOC { get; set; }
            public string RECEIVING_DATE { get; set; }
            public string TRUCK_NUMBER { get; set; }
            public List<SalesReturnLineDetails> LINE_ITEM { get; set; }
        }

        public class SalesReturnLineDetails 
        {
            public string LINE {  get; set; }

            public string MATERIAL_CODE { get; set; }

            public List<SalesReturnBatchDetails> batches { get; set; }
        }

        public class SalesReturnBatchDetails
        {
            public string SNo { get; set; }
            public string Secondary_batch { get; set; }
            public string Primary_batch { get; set; }
            public string Batch_Qty_in_box { get; set; }
            public string Crate_Number { get; set; }
            public string Status { get; set; }
        }

    }
}
