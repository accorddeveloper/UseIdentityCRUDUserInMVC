using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication4.Infrastructure;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            return View(UserManager.Users);
        }

        //创建显示
        public ActionResult Create()
        {
            return View();
        }

        //创建接收
        [HttpPost]
        public async Task<ActionResult> Create(CreateModel model)
        {
            if(ModelState.IsValid)
            {
                var user = new AppUser{UserName = model.Name, Email = model.Email};
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                if(result.Succeeded)
                {
                    return RedirectToAction("Index");
                }else{
                    AddErrorsFromResult(result);
                }
            }
            return View(model);
        }

        //编辑显示
        public async Task<ActionResult> Edit(string id)
        {
            AppUser user = await UserManager.FindByIdAsync(id);
            
            if(User != null)
            {
                CreateModel createModel = new CreateModel();
                createModel.Id = user.Id;
                createModel.Email = user.Email;
                createModel.Name = user.UserName;
                createModel.Password = user.PasswordHash;
                return View(createModel);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        //接收编辑
        [HttpPost]
        public async Task<ActionResult> Edit(CreateModel createModel)
        {
            
            if(ModelState.IsValid)
            {
                AppUser user = await UserManager.FindByIdAsync(createModel.Id);
                if (user != null)
                {
                    //关于邮箱
                    user.Email = createModel.Email;
                    IdentityResult validEmail = await UserManager.UserValidator.ValidateAsync(user);
                    if (!validEmail.Succeeded)
                    {
                        AddErrorsFromResult(validEmail);
                    }

                    user.UserName = createModel.Name;

                    //关于密码
                    IdentityResult validPass = null;
                    if (createModel.Password != string.Empty)
                    {
                        validPass = await UserManager.PasswordValidator.ValidateAsync(createModel.Password);
                        if (validPass.Succeeded)
                        {
                            user.PasswordHash = UserManager.PasswordHasher.HashPassword(createModel.Password);
                        }
                        else
                        {
                            AddErrorsFromResult(validPass);
                        }
                    }

                    user.Email = createModel.Email;

                    //验证结果
                    if ((validEmail.Succeeded && validPass == null) || (validEmail.Succeeded
    && createModel.Password != string.Empty && validPass.Succeeded))
                    {
                        IdentityResult result = await UserManager.UpdateAsync(user);
                        if (result.Succeeded)
                        {
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            AddErrorsFromResult(result);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "无此用户");
                    }
                }
                return View(createModel);
            }
            else
            {
                return View(createModel);
            }
            

        }

        //删除
        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            AppUser user = await UserManager.FindByIdAsync(id);
            if(user != null)
            {
                IdentityResult result = await UserManager.DeleteAsync(user);
                if(result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return View("Error", result.Errors);
                }
            }
            else
            {
                return View("Error", new string[] { "没有此用户" });
            }
        }

        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach(var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private AppUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
            }
        }
	}
}