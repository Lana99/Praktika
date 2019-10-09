﻿using DutyOfServiceDepart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using DutyOfServiceDepart.Filters;
using DutyOfServiceDepart.Mail;

namespace DutyOfServiceDepart.Controllers
{

	public class HomeController : Controller
	{		
		DutyContext db = new DutyContext();

		[Authorize]
		[HttpGet]
		public ActionResult Index(DateTime? Start) //возвращает представление
		{
			Calendar calendar;
			DateTime Target1 = Start ?? DateTime.Now.Date; // Start дата начала месяца, в представлении можно перелистывать месяцы

			calendar = GetCalendar(Target1);
			
			SelectList selectLogin = new SelectList(db.Employees, "EmployeId", "Name"); // делаем выборку всех сотрудников в выпадающий список
			ViewBag.Emp = selectLogin;
			return View(calendar);
		}

		private Calendar GetCalendar(DateTime Target)
		{
			/*Метод создаёт экземепляр класса Calendar и записывает в него дату, от которой начинать строить 
			  календарь и дежурных сотрудников в этом месяце */
			using (DutyContext db = new DutyContext())
			{
				Calendar calendar = new Calendar
				{
					CurrentDate = Target
				};

				foreach (DutyList s in db.DutyLists.Include(x => x.Employee).Where(x => x.DateDuty.Year == calendar.CurrentDate.Year && x.DateDuty.Month == calendar.CurrentDate.Month).ToList())
				{
					calendar.Duties.Add(s.DateDuty.Day, s.Employee); // Duties - массив пар значений - число месяца и сотрудник 
				}
				return calendar;
			}
		}
		[MyAuthorize]
		[HttpPost]
		public ActionResult Edit(int selectedEmpId, DateTime DateEdit)
		{
			using (DutyContext db = new DutyContext())
			{
				Employee NewEmployee = db.Employees.Find(selectedEmpId);//находим выбранного на дату дежурства сотрудника 
				List<DutyList> dutyList = db.DutyLists.Where(x => x.DateDuty == DateEdit).ToList(); //находим дежурства с такой датой
				if (dutyList.Count != 0) // если такие записи дежурств есть, меняем дежурного
				{
					foreach (DutyList s in dutyList)
					{
						db.Entry(s).State = EntityState.Modified;
						s.Employee = NewEmployee;
					}
				}
				else // если таких дежурств нет, создаём новую запись
				{
					DutyList NewDutyList = new DutyList() { DateDuty = DateEdit, Employee = NewEmployee, DecrDuty = String.Empty };
					db.DutyLists.Add(NewDutyList);
				}
				db.SaveChanges();				

				IMail sending = new SendingMailRu();
				sending.SendMail(NewEmployee.Email, "Изменения в графике дежурств", "Изучите новый график", DateEdit);
								
				return RedirectToAction("Index");
			}
		}
						

		[MyAuthorize]
		public ViewResult SendAll(DateTime CurDate)
		{
			
			using (DutyContext db = new DutyContext())
			{
		
				IMail sending = new SendingMailRu();
				foreach (Employee e in db.Employees)
				{
					sending.SendMail(e.Email, "График дежурств", "Изучите график дежурств на текущий месяц", CurDate);
				}

				return View();
			}
		}
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				db.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}