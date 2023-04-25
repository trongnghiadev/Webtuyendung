using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Aspose.Cells;
using DoAn2023.Models.DAL;
using DoAn2023.Models.ViewModels.Common;
using DoAn2023.Models.ViewModels.UngVien;

namespace DoAn2023.Areas.Admin.Controllers
{
    public class UngVienController : BaseController
    {
        UngVienDAL dao;

        public UngVienController()
        {
            dao = new UngVienDAL();
        }

        // GET: Admin/UngVien
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetPaging(string keyWord, int pageIndex = 1, int pageSize = 5)
        {
            var request = new GetListPaging()
            {
                keyWord = keyWord,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            var data = await dao.GetList(request);
            int totalRecord = data.TotalRecord;
            int toalPage = (int)Math.Ceiling((double)totalRecord / pageSize);
            return Json(new { data = data.Items, pageCurrent = pageIndex, toalPage = toalPage, totalRecord = totalRecord }
                , JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(UngVienVm ungvien)
        {
            if (ModelState.IsValid)
            {
                var result = await dao.Create(ungvien);
                if (result > 0)
                {
                    SetAlert("Tạo ứng viên thành công", "success");
                    return RedirectToAction("Index");
                }
                else if (result == 0)
                {
                    ModelState.AddModelError("", "Mã ứng viên đã tồn tại");
                }
                else if (result == -1)
                {
                    SetAlert("Đã có lỗi xảy ra. Vui lòng thử lại", "error");
                }
            }
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Detail(int id)
        {
            var ungvien = await dao.GetById(id);
            ViewBag.MaUngVien = ungvien.MaUngVien;
            return View(ungvien);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int maUngVien)
        {
            var result = await dao.Delete(maUngVien);
            if (result)
            {
                SetAlert("Xóa thành công", "success");
            }
            else
            {
                SetAlert("Có lỗi xảy ra. Vui lòng thử lại!", "error");
            }
            return Json(result);
        }

        [HttpGet]
        public async Task<ActionResult> ExportExcel()
        {
            var templatePath = Server.MapPath("~/Files/Templates/DSUngVien.xlsx");

            if (!System.IO.File.Exists(templatePath))
            {
                return new HttpNotFoundResult();
            }
            
            var dsUngVien = await dao.GetAll();

            using (var file = new FileStream(templatePath
                , FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var book = new Workbook(file);

                Worksheet sheet = book.Worksheets.FirstOrDefault();

                if (sheet == null)
                    return null;
                
                int iStart = 8;
                int cStart = 0;

                for (int i = 0; i < dsUngVien.Count; i++)
                {
                    sheet.Cells[iStart, cStart++].Value = i;
                    sheet.Cells[iStart, cStart++].Value = dsUngVien[i].MaUngVien;
                    sheet.Cells[iStart, cStart++].Value = dsUngVien[i].TenUngVien;
                    sheet.Cells[iStart, cStart++].Value = dsUngVien[i].GioiTinh;
                    sheet.Cells[iStart, cStart++].Value = dsUngVien[i].NgaySinh;
                    sheet.Cells[iStart, cStart++].Value = dsUngVien[i].SoDienThoai;
                    sheet.Cells[iStart, cStart++].Value = dsUngVien[i].DiaChi;
                    cStart = 0;
                    sheet.Cells.InsertRow(++iStart);
                }

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = "Danh Sach Ung Vien.xlsx",
                    Inline = false,
                };

                Response.Headers.Add("Content-Disposition", cd.ToString());
                var stream = new MemoryStream();
                book.Save(stream, SaveFormat.Xlsx);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Danh Sach Ung Vien.xlsx");
            }
        }

    }
}