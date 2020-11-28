using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleWebApiProject.Models
{
    public class UpdatePatientName
    {
        [Required]
        [MaxLength(150)]
        [MinLength(1)]
        public string FirstName
        {
            get;
            set;
        }

        [Required]
        [MaxLength(150)]
        [MinLength(1)]
        public string LastName
        {
            get;
            set;
        }

        public UpdatePatientName()
        {

        }
    }
}
