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

using System;

using PX.Data;

namespace PX.Objects.GL
{
	public class BatchNbrAttribute : PXSelectorAttribute
	{
		public virtual Type IsMigratedRecordField
		{
			get;
			set;
		}

		public BatchNbrAttribute(Type searchType)
			: base(searchType)
		{ }

		private bool IsMigratedRecord(PXCache cache, object data)
		{
			string fieldName = cache.GetField(IsMigratedRecordField);
			return (cache.GetValue(data, fieldName) as bool?) == true;
		}

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (IsMigratedRecord(sender, e.Row))
			{
				e.Cancel = true;
			}
			else
			{
				base.FieldVerifying(sender, e);
			}
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);

			if (IsMigratedRecord(sender, e.Row))
			{
				e.ReturnValue = Common.Messages.MigrationModeBatchNbr;
			}
		}
	}

	public class BatchNbrExtAttribute : BatchNbrAttribute
	{
		public BatchNbrExtAttribute(Type searchType)
			: base(searchType)
		{ }
		protected override bool IsReadDeletedSupported => false;
	}
}