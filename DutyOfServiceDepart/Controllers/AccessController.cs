﻿using DutyOfServiceDepart.Filters;
using DutyOfServiceDepart.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DutyOfServiceDepart.Controllers
{
    public class AccessController : Controller
    {
		private DutyContext db = new DutyContext();
		// GET: Access
		public ActionResult Index(int? page)
        {
			var incs = from s in db.Accesses
					   select s;
			incs = incs.OrderBy(s => s.Login);

			int pageSize = 5;
			int pageNumber = (page ?? 1);
			return View("GetAccess", incs.ToPagedList(pageNumber, pageSize));

        }
		[MyAuthorize]
		[HttpGet]
		public ViewResult Create()
		{
			return View("CreateAccess", db.Employees);
		}
		[MyAuthorize]
		[HttpPost]
		public ActionResult Create(Access access)
		{
			if (ModelState.IsValid)
			{
				db.Accesses.Add(access);
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			return View("CreateAccess");
		}
		[MyAuthorize]
		[HttpGet]
		public ActionResult Delete(int id)
		{
			Access access = db.Accesses.Find(id);
			if (access != null)
			{
				db.Accesses.Remove(access);
				db.SaveChanges();
			}
			return RedirectToAction("Index");
		}
	}
}