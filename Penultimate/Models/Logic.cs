using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Penultimate.Models
{
    public class Employee
    {
        public int EmpNo { get; set; }
        public string EmpName { get; set; }
    }

    public class Employees : List<Employee>
    {
        public Employees()
        {
            Add(new Employee() { EmpNo = 101, EmpName = "Emp-1" });
            Add(new Employee() { EmpNo = 102, EmpName = "Emp-2" });
        }
    }
}
