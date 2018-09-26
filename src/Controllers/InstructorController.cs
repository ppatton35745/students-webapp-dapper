using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Workforce.Models;
using Workforce.Models.ViewModels;
using System.Data.SqlClient;

namespace Workforce.Controllers
{
    public class InstructorController : Controller
    {
        private readonly IConfiguration _config;

        public InstructorController(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        public async Task<IActionResult> Index()
        {

            string sql = @"
            select
                s.Id,
                s.FirstName,
                s.LastName,
                s.SlackHandle,
                s.Specialty,
                c.Id,
                c.Name
            from Instructor s
            join Cohort c on s.CohortId = c.Id
        ";

            using (IDbConnection conn = Connection)
            {
                Dictionary<int, Instructor> instructors = new Dictionary<int, Instructor>();

                var InstructorQuerySet = await conn.QueryAsync<Instructor, Cohort, Instructor>(
                        sql,
                        (instructor, cohort) => {
                            if (!instructors.ContainsKey(instructor.Id))
                            {
                                instructors[instructor.Id] = instructor;
                            }
                            instructors[instructor.Id].Cohort = cohort;
                            return instructor;
                        }
                    );
                return View(instructors.Values);

            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string sql = $@"
            select
                s.Id,
                s.FirstName,
                s.LastName,
                s.SlackHandle,
                s.Specialty
            from Instructor s
            WHERE s.Id = {id}";

            using (IDbConnection conn = Connection)
            {

                Instructor Instructor = (await conn.QueryAsync<Instructor>(sql)).ToList().Single();

                if (Instructor == null)
                {
                    return NotFound();
                }

                return View(Instructor);
            }
        }

        private async Task<SelectList> CohortList(int? selected)
        {
            using (IDbConnection conn = Connection)
            {
                // Get all cohort data
                List<Cohort> cohorts = (await conn.QueryAsync<Cohort>("SELECT Id, Name FROM Cohort")).ToList();

                // Add a prompting cohort for dropdown
                cohorts.Insert(0, new Cohort() { Id = 0, Name = "Select cohort..." });

                // Generate SelectList from cohorts
                var selectList = new SelectList(cohorts, "Id", "Name", selected);
                return selectList;
            }
        }

        public IActionResult Create()
        {
                InstructorCreateViewModel icm = new InstructorCreateViewModel(_config);
                return View(icm);
        }

        // POST: Employee/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Instructor Instructor)
        {

            if (ModelState.IsValid)
            {
                string sql = $@"
                    INSERT INTO Instructor
                        ( FirstName, LastName, SlackHandle, Specialty, CohortId )
                        VALUES
                        (  '{Instructor.FirstName}'
                            , '{Instructor.LastName}'
                            , '{Instructor.SlackHandle}'
                            , '{Instructor.Specialty}'
                            , {Instructor.CohortId}
                        )
                    ";

                using (IDbConnection conn = Connection)
                {
                    int rowsAffected = await conn.ExecuteAsync(sql);

                    if (rowsAffected > 0)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            // ModelState was invalid, or saving the Instructor data failed. Show the form again.
            using (IDbConnection conn = Connection)
            {
                IEnumerable<Cohort> cohorts = (await conn.QueryAsync<Cohort>("SELECT Id, Name FROM Cohort")).ToList();
                // ViewData["CohortId"] = new SelectList (cohorts, "Id", "Name", Instructor.CohortId);
                ViewData["CohortId"] = await CohortList(Instructor.CohortId);
                return View(Instructor);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string sql = $@"
                SELECT
                    s.Id,
                    s.FirstName,
                    s.LastName,
                    s.SlackHandle,
                    s.Specialty,
                    s.CohortId,
                    c.Id,
                    c.Name
                FROM Instructor s
                JOIN Cohort c on s.CohortId = c.Id
                WHERE s.Id = {id}";

            using (IDbConnection conn = Connection)
            {
                InstructorEditViewModel model = new InstructorEditViewModel(_config);

                model.Instructor = (await conn.QueryAsync<Instructor, Cohort, Instructor>(
                    sql,
                    (instructor, cohort) => {
                        instructor.Cohort = cohort;
                        return instructor;
                    }
                )).Single();

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InstructorEditViewModel model)
        {
            if (id != model.Instructor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                string sql = $@"
                UPDATE Instructor
                SET FirstName = '{model.Instructor.FirstName}',
                    LastName = '{model.Instructor.LastName}',
                    SlackHandle = '{model.Instructor.SlackHandle}',
                    Specialty = '{model.Instructor.Specialty}',
                    CohortId = {model.Instructor.CohortId}
                WHERE Id = {id}";

                using (IDbConnection conn = Connection)
                {
                    int rowsAffected = await conn.ExecuteAsync(sql);
                    if (rowsAffected > 0)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    throw new Exception("No rows affected");
                }
            }
            else
            {
                return new StatusCodeResult(StatusCodes.Status406NotAcceptable);
            }
        }

        public async Task<IActionResult> DeleteConfirm(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string sql = $@"
                select
                    s.Id,
                    s.FirstName,
                    s.LastName,
                    s.SlackHandle
                from Instructor s
                WHERE s.Id = {id}";

            using (IDbConnection conn = Connection)
            {

                Instructor instructor = (await conn.QueryAsync<Instructor>(sql)).ToList().Single();

                if (instructor == null)
                {
                    return NotFound();
                }

                return View(instructor);
            }
        }

        public IActionResult DeleteError(SqlException e)
        {
            InstructorDeleteErrorViewModel myError = new InstructorDeleteErrorViewModel(e);
            Console.WriteLine("Phil exception was passed is your error message:");
            Console.WriteLine(e.GetType());
            Console.WriteLine(e.Message);
            Console.WriteLine("Phil End of your error message");
            return View(myError);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            string sql = $@"DELETE FROM Instructor WHERE Id = {id}";

            using (IDbConnection conn = Connection)
            {
     
                try
                {
                    int rowsAffected = await conn.ExecuteAsync(sql);

                    if (rowsAffected > 0)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    throw new Exception("No rows affected");
                }

                catch (SqlException e)
                {
                    Console.WriteLine("Phil this is your error message:");
                    Console.WriteLine(e.GetType());
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Phil End of your error message");
                    ControllerContext.RouteData.Values.Add("e", e);
                    return RedirectToAction(nameof(DeleteError));
                }
        
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
