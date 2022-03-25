using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Data;
using DutyDemo.Models;

namespace DutyDemo.Controllers
{
    public class HomeController : Controller
    {
        DataTable dt;

        public ActionResult ProcessResult()
        {
            return View();
        }
        public ActionResult ImportExcel()
        {
            if (Request.Files["FileUpload"].ContentLength > 0)
            {
                string extension = System.IO.Path.GetExtension(Request.Files["FileUpload"].FileName).ToLower();
                string[] validFileTypes = { ".xls", ".xlsx", ".csv" };

                string path1 = string.Format("{0}/{1}", Server.MapPath("~/Content/Uploads"), Request.Files["FileUpload"].FileName);
                if (!Directory.Exists(path1))
                {
                    Directory.CreateDirectory(Server.MapPath("~/Content/Uploads"));
                }
                if (validFileTypes.Contains(extension))
                {
                    if (System.IO.File.Exists(path1))
                    { System.IO.File.Delete(path1); }
                    Request.Files["FileUpload"].SaveAs(path1);
                    if (extension == ".csv")
                    {
                        dt = ProcessExcel.ConvertCSVtoDataTable(path1);
                        ViewBag.Data = dt;
                    }
                }
                else
                {
                    ViewBag.Error = "Please Upload Files in .xls, .xlsx or .csv format";
                }
            }
            return View();
        } 


        [HttpPost]
        [ActionName ("ProcessResult")]
        public ActionResult ProcessResult1()
        {
            ImportExcel();
            ProcessExcel objProcessExcel = new ProcessExcel();
            objProcessExcel.FindYearsHighRevenueImportDuties(dt);
            objProcessExcel.FindYearsHighRevenueExciseDuties(dt);
            objProcessExcel.FindYearWiseMaxImportDuties(dt);
            objProcessExcel.FindYearWiseMaxExciseDuties(dt);
        
            return View(objProcessExcel);
        }      
    }
}