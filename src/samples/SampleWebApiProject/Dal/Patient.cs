using System.Collections;
using System.Collections.Generic;

namespace SampleWebApiProject.Dal
{
    public class Patient
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

        public virtual ICollection<Visist> Visists
        {
            get;
            set;
        }

        public Patient()
        {

        }
    }
}
