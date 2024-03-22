using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMS.Services.Utility
{
    public class ResetPassword
    {
        public int Token { get; set; }
        public string Password { get; set; }
    }
}
