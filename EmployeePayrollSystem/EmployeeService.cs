﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeePayrollSystem
{
    class EmployeeService
    {
        private Func<EmpDbContext> _empDbContext;

        public EmployeeService()
        {
            
        }

        public async Task<ObservableCollection<Employee>> GetEmployeesAsync()
        {
            ObservableCollection<Employee> employees = new ObservableCollection<Employee>();
            foreach (var employee in await getEmpFromDatabaseAsync())
            {
                employees.Add(employee);
            } 
            return employees;
        }

        public static async Task<List<Employee>> getEmpFromDatabaseAsync() {
            using (var ctx = new EmpDbContext()) {                
                return await ctx.Employee.AsNoTracking().ToListAsync(); 
            }
        }

        public async Task<List<EmployeeDetails>> GetPayDetail(Employee selectedEmployee, DateTime from, DateTime to)
        {
            using (var ctx = new EmpDbContext())
            {
                return await ctx.EmployeeDetails.Include(e=>e.Employee).Where(ed => (ed.EmpId == selectedEmployee.EmpId)).Where(ed=>ed.WorkDate>=from && ed.WorkDate<=to).AsNoTracking().ToListAsync();
            }
        }
        

        public async void saveEmployeeDetails(List<EmployeeDetails> changedItems, List<EmployeeDetails> employeeDetailsFromDatabase)
        {
            using (var ctx = new EmpDbContext())
            {
                foreach (var changedItem in changedItems)
                {
                    if (employeeDetailsFromDatabase.Find(e => e.WorkDate.ToShortDateString().Equals(changedItem.WorkDate.ToShortDateString())) == null)
                    {
                        ctx.EmployeeDetails.Add(changedItem);
                        ctx.Entry(changedItem).State = EntityState.Added;
                    }
                    else
                    {
                        ctx.EmployeeDetails.Attach(changedItem);
                        ctx.Entry(changedItem).State = EntityState.Modified;
                    }                        
                    await ctx.SaveChangesAsync();
                }
            }
        }
    }
}