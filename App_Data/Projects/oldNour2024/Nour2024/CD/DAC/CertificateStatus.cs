using PX.Data;
using PX.Data.BQL;

namespace Maintenance.CD
{
    
    public class CertificateStatus 
    {

            
            public const string NewC = "NEW";
            public const string Printed = "PRINTED";
            public const string Sended = "SENDED";
            public const string Received = "RECEIVED";
            public const string Released = "RELEASED";
            public const string Transfered = "TRANSFERED";
            public const string CustomerReceived = "CUSTOMERRECEIVED";
            public const string ClosedOut = "ClosedOut";
            public const string PendingClosedOut = "PendingClosedOut";

            public class newC :BqlString.Constant<newC>
            {
                public newC() : base(NewC) { ;}
            }
            public class printed:BqlString.Constant<printed>
            {
                public printed() : base(Printed) { ;}
            }
            public class sended :BqlString.Constant<sended>
            {
                public sended() : base(Sended) { ;}
            }
            public class received :BqlString.Constant<received>
            {
                public received() : base(Received) { ;}
            }
            public class released :BqlString.Constant<released>
            {
                public released() : base(Released) { ;}
            }
            public class transfered : BqlString.Constant<transfered>
            {
                public transfered() : base(Transfered) { ;}
            }
            public class customerReceived : BqlString.Constant<customerReceived>
            {
                public customerReceived() : base(CustomerReceived) { ;}
            }
            public class closedOut :BqlString.Constant<closedOut>
            {
                public closedOut() : base(ClosedOut) { ;}
            }
            public class pendingClosedOut :BqlString.Constant<pendingClosedOut>
            {
                public pendingClosedOut() : base(PendingClosedOut) { ;}
            }
        }

    public class CertificateType
    {


        public const string NewC = "NEW";
        public const string ReNew = "RE-NEW";
       // public const string Compensation = "COMPENSATION";
        public const string Extension = "EXTENSION";
        //public const string CloseOut = "CLOSE-OUT";
        

        public class newC :BqlString. Constant<newC>
        {
            public newC() : base(NewC) { ;}
        }
        public class reNew : BqlString.Constant<reNew>
        {
            public reNew() : base(ReNew) { ;}
        }
        /**
        public class compensation : Constant<string>
        {
            public compensation() : base(Compensation) { ;}
        }
        **/
        public class extension : BqlString.Constant<extension>
        {
            public extension() : base(Extension) { ;}
        }
        /**
        public class closeOut : Constant<string>
        {
            public closeOut() : base(CloseOut) { ;}
        }
        **/

    }

    public class InsuranceKinds
    {


        public const string WithCoverage = "With-Coverage";
        public const string WithoutCoverage = "Without-Coverage";


        public class withCoverage : BqlString.Constant<withCoverage>
        {
            public withCoverage() : base(WithCoverage) { ;}
        }
        public class withoutCoverage : BqlString. Constant<withoutCoverage>
        {
            public withoutCoverage() : base(WithoutCoverage) { ;}
        }
     
    }

    public class InvoicePaymentTypes
    {


        public const string Cash = "Cash";
        public const string Installment = "Installment";


        public class cash : BqlString.Constant<cash>
        {
            public cash() : base(Cash) { ;}
        }
        public class installment : BqlString.Constant<installment>
        {
            public installment() : base(Installment) { ;}
        }

    }
}
