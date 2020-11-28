using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleWebApiProject.Models
{
    public class PatientDetail
    {
        public int Id
        {
            get;
            set;
        }

        public string FirstName
        {
            get;
            set;
        }

        public string LastName
        {
            get;
            set;
        }

        public string SocialSecurityNumber
        {
            get;
            set;
        }

        public string OtherId
        {
            get;
            set;
        }

        public bool Alert
        {
            get;
            set;
        }

        public string Notes
        {
            get;
            set;
        }
        public PatientDetail()
        {

        }
    }
}
