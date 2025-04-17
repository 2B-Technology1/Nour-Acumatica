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

namespace PX.Objects.GL
{
	public class WorkBookStatus
	{
        /// <summary>
        /// The active status of the workbook.
        /// This status means that the workbook is visible in selector on the Workbooks (GL307000) form and new voucher batches can be added to the workbook.
        /// </summary>
        public const short Active = (short)0;
        /// <summary>
        /// The inactive (disabled) status of the workbook.
        /// This status means that the workbook is visible in selector on the Workbooks (GL307000) form and new voucher batches cannot be added to the workbook.
        /// </summary>
        public const short Inactive = (short)1;
        /// <summary>
        /// The hidden status of the workbook.
        /// This status means that the workbook is not visible in selector on the Workbooks (GL307000) form and new voucher batches cannot be added to the workbook.
        /// </summary>
        public const short Hidden = (short)2;

		public const string ActiveName = "Active";
		public const string InactiveName = "Inactive";
		public const string HiddenName = "Hidden";

		public class active : PX.Data.BQL.BqlShort.Constant<active>
		{
			public active() : base(Active) { }
		}

		public class inactive : PX.Data.BQL.BqlShort.Constant<inactive>
		{
			public inactive() : base(Inactive) { }
		}

		public class hidden : PX.Data.BQL.BqlShort.Constant<hidden>
		{
			public hidden() : base(Hidden) { }
		}
	}
}
