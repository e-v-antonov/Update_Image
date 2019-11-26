﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Penultimate.Models;
using Penultimate.Services;

namespace Penultimate.Controllers
{
    public class EmployeeController : Controller
    {
        EmployeeService serv;

        public EmployeeController(EmployeeService serv)
        {
            this.serv = serv;
        }

        [Authorize(Policy = "readonlypolicy")]
        public IActionResult Index()
        {
            var emps = serv.Get();
            return View(emps);
        }

        [Authorize(Policy = "writepolicy")]
        public IActionResult Create()
        {
            return View(new Employee());
        }

        [HttpPost]
        public IActionResult Create(Employee emp)
        {
            var emps = serv.Create(emp);
            return View("Index", emps);
        }
    }
}