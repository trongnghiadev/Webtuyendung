using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Aspose.Cells;
using DoAn2023.Models.DAL;
using DoAn2023.Models.ViewModels.Common;
using DoAn2023.Models.ViewModels.Employer;

namespace DoAn2023.Areas.Admin.Controllers
{
    public class NhaTuyenDungController : BaseController
    {
        NhaTuyenDungDAL dao;

        public NhaTuyenDungController()
        {
            dao = new NhaTuyenDungDAL();
        }

        // GET: Admin/TuyenDung
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
        public async Task<ActionResult> Create(EmployerEdit member)
        {
            if (ModelState.IsValid)
            {
                var result = await dao.Create(member);
                if (result > 0)
                {
                    SetAlert("Tạo nhà tuyển dụng thành công", "success");
                    return RedirectToAction("Index");
                }
                else if (result == 0)
                {
                    ModelState.AddModelError("", "Mã nhà tuyển dụng đã tồn tại");
                }
                else if (result == -2)
                {
                    ModelState.AddModelError("", "Mã nhà tuyển chưa trùng với mã tài khoản đăng ký nhà tuyển dụng");
                }
                else
                {
                    SetAlert("Đã có lỗi xảy ra. Vui lòng thử lại", "error");
                }
            }
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Detail(int id)
        {
            var member = await dao.GetById(id);
            ViewBag.MaNTD = member.MaNTD;
            return View(member);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int maNTD)
        {
            var result = await dao.Delete(maNTD);
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
            var templatePath = Server.MapPath("~/Files/Templates/DSNhaTuyenDung.xlsx");

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
                    sheet.Cells[iStart, cStart++].Value = dsUngVien[i].MaNTD;
                    sheet.Cells[iStart, cStart++].Value = dsUngVien[i].TenNTD;
                    sheet.Cells[iStart, cStart++].Value = dsUngVien[i].SoDienThoai;
                    sheet.Cells[iStart, cStart++].Value = dsUngVien[i].QuyMo;
                    sheet.Cells[iStart, cStart++].Value = dsUngVien[i].DiaChi;
                    sheet.Cells[iStart, cStart++].Value = dsUngVien[i].Website;
                    cStart = 0;
                    sheet.Cells.InsertRow(++iStart);
                }

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = "Danh Sach Nha Tuyen Dung.xlsx",
                    Inline = false,
                };

                Response.Headers.Add("Content-Disposition", cd.ToString());
                var stream = new MemoryStream();
                book.Save(stream, SaveFormat.Xlsx);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Danh Sach Nha Tuyen Dung.xlsx");
            }
        }
    }
}