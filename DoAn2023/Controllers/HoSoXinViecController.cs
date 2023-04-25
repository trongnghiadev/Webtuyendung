﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DoAn2023.Common;
using DoAn2023.Models.DAL;
using DoAn2023.Models.ViewModels.HoSoXinViec;
using DoAn2023.Models.ViewModels.User;

namespace DoAn2023.Controllers
{
    public class HoSoXinViecController : BaseController
    {
        private readonly HoSoXinViecDAL dal;
        public HoSoXinViecController()
        {
            dal = new HoSoXinViecDAL();
        }

        // GET: HoSoXinViec
        public ActionResult Index()
        {
            if (UserLogin() == null) return RedirectToAction("Index", "Home");
            var list = dal.GetListByIdNguoiDung(UserLogin().Id);
            return View(list);
        }

        public ActionResult HoSo(int ungvien, int id)
        {
            var userLogin = UserLogin();
            var ntd = (UserLogin)Session[CommonConstants.EMPLOYER_SESSION];
            if (userLogin == null && ntd == null) 
            {
                SetAlert("Bạn chưa đăng nhập", "warning");
                return RedirectToAction("Index", "Home");
            }else if(userLogin != null && userLogin.Id != ungvien)
            {
                return RedirectToAction("Index", "HoSoXinViec");
            }
            var model = dal.GetById(id, ungvien);
            if(model == null) return RedirectToAction("Index", "HoSoXinViec");
            return View(model);
        }

        public ActionResult Create()
        {
            if (UserLogin() == null)
            {
                SetAlert("Bạn chưa đăng nhập", "warning");
                return RedirectToAction("Index", "Home");
            }
            return View(new HoSoXinViecCreate()
            {
                TenHoSo = "Hồ sơ xin việc"
            });
        }

        [HttpPost]
        public async Task<ActionResult> Create(HoSoXinViecCreate item)
        {
            if (ModelState.IsValid)
            {
                var result = await dal.Create(item, UserLogin().Id);
                if (result > 0)
                {
                    SetAlert("Tạo hồ sơ xin việc thành công", "success");
                    return RedirectToAction("Index", "Home");
                }else if(result == 0)
                {
                    ModelState.AddModelError("", "Tên hồ sơ đã tồn tại");
                }
                else
                {
                    SetAlert("Đã có lỗi xảy ra. Vui lòng thử lại", "error");
                }
            }
            return View(item);
        }

        public ActionResult Edit(int id)
        {
            if (UserLogin() == null)
            {
                SetAlert("Bạn chưa đăng nhập", "warning");
                return RedirectToAction("Index", "Home");
            }
            var model = dal.GetByIdEdit(id, UserLogin().Id);
            if(model == null)
            {
                SetAlert("Đã có lỗi xảy ra", "warning");
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(HoSoXinViecEdit item)
        {
            if (ModelState.IsValid)
            {
                var result = await dal.Update(item, UserLogin().Id);
                if (result > 0)
                {
                    SetAlert("Cập nhật hồ sơ xin việc thành công", "success");
                    return RedirectToAction("Index", "HoSoXinViec");
                }
                else if (result == 0)
                {
                    ModelState.AddModelError("", "Tên hồ sơ đã tồn tại");
                }
                else
                {
                    SetAlert("Đã có lỗi xảy ra. Vui lòng thử lại", "error");
                }
            }
            return View(item);
        }

        public async Task<ActionResult> Delete(int id)
        {
            if (UserLogin() == null)
            {
                SetAlert("Bạn chưa đăng nhập", "warning");
                return RedirectToAction("Index", "Home");
            }
            var model = dal.GetByIdEdit(id, UserLogin().Id);
            
            if (model == null)
            {
                SetAlert("Đã có lỗi xảy ra, không tìm thấy hồ sơ cần xoá !", "warning");
                return RedirectToAction("Index", "HoSoXinViec");
            }

            bool result = await dal.Delete(id, UserLogin().Id);
            
            if (result == true)
            {
                return RedirectToAction("Index", "HoSoXinViec");
            }

            return RedirectToAction("Index", "HoSoXinViec");
        }
    }
}