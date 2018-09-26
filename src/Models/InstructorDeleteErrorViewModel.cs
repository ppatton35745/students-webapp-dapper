using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Workforce.Models.ViewModels
{
    public class InstructorDeleteErrorViewModel
    {
        public SqlException Error { get; set; }

        public InstructorDeleteErrorViewModel() { }
        public InstructorDeleteErrorViewModel(SqlException e)
        {
            this.Error = e;
        }

    }
}
