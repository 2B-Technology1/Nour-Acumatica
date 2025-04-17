/* ---------------------------------------------------------------------*
*                             Acumatica Inc.                            *

*              Copyright (c) 2005-2023 All rights reserved.             *

*                                                                       *

*                                                                       *

* This file and its contents are protected by United States and         *

* International copyright laws.  Unauthorized reproduction and/or       *

* distribution of all or any portion of the code contained herein       *

* is strictly prohibited and will result in severe civil and criminal   *

* penalties.  Any violations of this copyright will be prosecuted       *

* to the fullest extent possible under law.                             *

*                                                                       *

* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *

* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *

* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ACUMATICA PRODUCT.       *

*                                                                       *

* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *

* --------------------------------------------------------------------- */

using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Production order statuses
    /// </summary>
    public class ProductionOrderStatus
    {
        /// <summary>
        /// Planned = P (New order default)
        /// </summary>
        public const string Planned = "P";
        /// <summary>
        /// Released = R
        /// </summary>
        public const string Released = "R";
        /// <summary>
        /// InProcess = I
        /// </summary>
        public const string InProcess = "I";
        /// <summary>
        /// Hold = H
        /// Status is not set in DB when order is placed on hold. Used for inquiry displays
        /// </summary>
        public const string Hold = "H";
        /// <summary>
        /// Cancel = X
        /// </summary>
        public const string Cancel = "X";
        /// <summary>
        /// Completed = M
        /// </summary>
        public const string Completed = "M";
		/// <summary>
		/// Locked = L
		/// </summary>
		public const string Locked = "L";
		/// <summary>
		/// Closed = C
		/// </summary>
		public const string Closed = "C";
		/// <summary>
		/// Description/labels for identifiers
		/// </summary>/// 

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new string[] {
						Planned,
						Released,
						InProcess,
						Hold,
						Cancel,
						Completed,
						Closed,
						Locked},
                    new string[] {
                        Messages.Planned, 
                        Messages.Released, 
                        Messages.InProcess,
						Messages.Hold,
                        Messages.Canceled,
                        Messages.Completed,
                        Messages.Closed,
						Messages.Locked}) {}
        }

        #region METHODS

        /// <summary>
        /// Returns the production status description/label from the status single letter ID
        /// </summary>
        /// <param name="statusID">single letter status ID</param>
        /// <returns>Production description/label</returns>
        public static string GetStatusDescription(string statusID)
        {
            switch (statusID)
            {
                case Planned:
					return Messages.GetLocal(Messages.Planned);
                case Released:
					return Messages.GetLocal(Messages.Release);
                case InProcess:
					return Messages.GetLocal(Messages.InProcess);
				case Hold:
					return Messages.GetLocal(Messages.Hold);
				case Cancel:
					return Messages.GetLocal(Messages.Canceled);
				case Completed:
                    return Messages.GetLocal(Messages.Completed);
				case Closed:
					return Messages.GetLocal(Messages.Closed);
				case Locked:
					return Messages.GetLocal(Messages.Locked);
			}

            return Messages.GetLocal(Messages.Unknown);
        }
        #endregion
        
        public class planned : PX.Data.BQL.BqlString.Constant<planned>
        {
            public planned() : base(Planned) { ;}
        }
        public class released : PX.Data.BQL.BqlString.Constant<released>
        {
            public released() : base(Released) { ;}
        }
        public class inProcess : PX.Data.BQL.BqlString.Constant<inProcess>
        {
            public inProcess() : base(InProcess) { ;}
        }
        public class hold : PX.Data.BQL.BqlString.Constant<hold>
        {
            public hold() : base(Hold) { ;}
        }
        public class cancel : PX.Data.BQL.BqlString.Constant<cancel>
        {
            public cancel() : base(Cancel) { ;}
        }
        public class completed : PX.Data.BQL.BqlString.Constant<completed>
        {
            public completed() : base(Completed) { ;}
        }
        public class closed : PX.Data.BQL.BqlString.Constant<closed>
        {
            public closed() : base(Closed) { ;}
        }
		public class locked : PX.Data.BQL.BqlString.Constant<locked>
		{
			public locked() : base(Locked) {; }
		}
	}
}
