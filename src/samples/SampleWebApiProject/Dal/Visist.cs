using System;

namespace SampleWebApiProject.Dal
{
    public class Visist
    {
        public int Id
        {
            get;
            set;
        }

        public DateTime Date
        {
            get;
            set;
        }

        public string Note
        {
            get;
            set;
        }

        public double Price
        {
            get;
            set;
        }

        public int GumsState //GumsState
        {
            get;
            set;
        }

        public int GingivalBleedingState //GingivalBleedingState
        {
            get;
            set;
        }

        public int DentalPlaqueState //DentalPlaqueState
        {
            get;
            set;
        }

        public int TartarState //TartarState
        {
            get;
            set;
        }

        public int GingivalCamsState //GingivalCamsState
        {
            get;
            set;
        }

        public int GingivalCamsTherapy //GingivalCamsTherapy
        {
            get;
            set;
        }

        public int Braces //HasSymptom
        {
            get;
            set;
        }

        public int FurkationResoprtion //TODO refaktor//HasSymptom
        {
            get;
            set;
        }

        public int FixSupport //HasSymptom
        {
            get;
            set;
        }

        public int Pigment //HasSymptom
        {
            get;
            set;
        }

        public int PatientId
        {
            get;
            set;
        }

        public virtual Patient Patient
        {
            get;
            set;
        }


        public Visist()
        {

        }
    }
}
