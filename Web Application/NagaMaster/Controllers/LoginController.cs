using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NagaMaster.Models;
using System.Web.Routing;

namespace NagaMaster.Controllers
    
{
    public class LoginController : Controller
    {
        string userId = "";
        // GET: Login
        MasterLogic objMas = new MasterLogic();
        public ActionResult LoginPage()
        {
            
                //Response.Cookies.Clear();
                //Session["EmpId"] = null;
            return View();
        }

        public List<Menus> LoadMenus(string userid)
        {

            var mnulist = new List<Menus>();
            string Qry = "Select d.MenuType,c.MenuId,c.View as IsVisible from employee_master a, userroleheader b,userrolelines c,webuser_menu d,systemuser_master e" +
                " where c.MenuId=d.MenuId and b.Id = c.RoleId and a.EmpType=b.Id and a.EmpId = e.EmpId and e.UserId='"+userid+"'";

            DataTable dt = objMas.GetDataTable(Qry);
            if (dt == null)
                return null;
            else
            {
                foreach (DataRow dr in dt.Rows)
                    mnulist.Add(new Menus
                    {
                        MenuType = Convert.ToInt16(dr["MenuType"]),
                        MenuId = Convert.ToInt16(dr["MenuId"]),
                        IsVisible = Convert.ToInt16(dr["IsVisible"])
                    });
            }
            return mnulist;
        }

        public Int64 GetMenuEdit(int MenuId, string userid)
        {
            Int64 ret = objMas.GetTableCount("employee_master a, userroleheader b,userrolelines c,webuser_menu d, systemuser_master e where c.MenuId=d.MenuId and b.Id = c.RoleId and " +
                "a.EmpType=b.Id and a.EmpId=e.EmpId and c.MenuId='" + MenuId + "' and c.Edit= 1 and e.UserId='" + userid + "'");

            return ret;
        }




        [HttpPost]
        public JsonResult LoginPage(LoginModel Data)
        {
            try
            {
                var Msg = new ResponseHelper { Success = 0, Message = "" };
                Int64 Ret = 0;
                DataTable dt = new DataTable();
                Ret = objMas.GetTableCount("systemuser_master where UserId='" + Data.UserId + "' and UserPWD='" + Data.UserPWD + "' ");
                if(Ret == -1)
                {
                    Msg = new ResponseHelper { Success = 0, Message = objMas.DBErrBuf };
                    objMas.PrintLog("A[0,1]", objMas.DBErrBuf);
                    return Json(Msg, JsonRequestBehavior.AllowGet);

                }
                else if (Ret >= 1)
                {
                    dt = objMas.GetDataTable("select a.UserId, a.PlantId, a.UserPWD, b.PlantId, b.PlantName, c.EmpId, c.EmpDesc, c.EmpType, d.OptEdit, " +
                            " d.OptView,d.MenuId, e.id, e.RoleName from systemuser_master a, plant_master b, employee_master c, webuser_menu d, userroleheader e " +
                            " where a.UserId = '" + Data.UserId + "' and a.PlantId=b.PlantId and a.EmpId=c.EmpId and c.EmpType=e.id");

                    if (dt == null)
                    {
                        Msg = new ResponseHelper { Success = 0, Message = "Please Enter Valid User Name and Password" };
                        objMas.PrintLog("A[0,2]", objMas.DBErrBuf);
                        return Json(Msg, JsonRequestBehavior.AllowGet);
                    }
                    HttpCookie hc8 = new HttpCookie("UserId", dt.Rows[0]["UserId"].ToString());
                    Response.Cookies.Add(hc8);
                    HttpCookie hc4 = new HttpCookie("PlantId", dt.Rows[0]["PlantId"].ToString());
                    Response.Cookies.Add(hc4);
                    HttpCookie hc5 = new HttpCookie("PlantName", dt.Rows[0]["PlantName"].ToString());
                    Response.Cookies.Add(hc5);
                    HttpCookie hc6 = new HttpCookie("EmpDesc", dt.Rows[0]["EmpDesc"].ToString());
                    Response.Cookies.Add(hc6);
                    HttpCookie hc7 = new HttpCookie("RoleName", dt.Rows[0]["RoleName"].ToString());
                    Response.Cookies.Add(hc7);
                    HttpCookie hc9 = new HttpCookie("EmpId", dt.Rows[0]["EmpId"].ToString());
                    Response.Cookies.Add(hc9);

                    if (Request.Cookies["UserId"] != null)
                    {
                        userId = Request.Cookies["UserId"].Value;
                        // Use the value of the "UserId" cookie here
                    }
                    Msg = new ResponseHelper { Success = 1, Message = "Success" };
                    objMas.PrintLog("A[0,3]", "Login Success");
                    return Json(Msg, JsonRequestBehavior.AllowGet);
                   
                }
                else
                {
                    Msg = new ResponseHelper { Success = 0, Message = "User Id and Password Invalid" };
                    objMas.PrintLog("A[0,4]", objMas.DBErrBuf);
                    return Json(Msg, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception Ex)
            {
                objMas.PrintLog("C[0,1]", objMas.DBErrBuf);
                return Json(new ResponseHelper { Success = -1, Message = Ex.Message.ToString(), Data = null });
            }

        }

    }

   
}


