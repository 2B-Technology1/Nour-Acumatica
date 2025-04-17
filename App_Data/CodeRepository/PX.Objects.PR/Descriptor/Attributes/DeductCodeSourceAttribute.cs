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
using System;

namespace PX.Objects.PR
{
	public class DeductCodeSourceAttribute : DeductionSourceListAttribute, IPXFieldUpdatingSubscriber
	{
		private Type _IsWorkersCompensationField;
		private Type _IsCertifiedProjectField;
		private Type _IsUnionField;

		public DeductCodeSourceAttribute(Type isWorkersCompensationField, Type isCertifiedProjectField, Type isUnionField)
		{
			_IsWorkersCompensationField = isWorkersCompensationField;
			_IsCertifiedProjectField = isCertifiedProjectField;
			_IsUnionField = isUnionField;
		}

		public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			base.FieldSelecting(sender, e);

			if (GetBool(sender, _IsWorkersCompensationField, e.Row))
			{
				e.ReturnValue = WorkCode;
			}
			else if (GetBool(sender, _IsCertifiedProjectField, e.Row))
			{
				e.ReturnValue = CertifiedProject;
			}
			else if (GetBool(sender, _IsUnionField, e.Row))
			{
				e.ReturnValue = Union;
			}
			else
			{
				e.ReturnValue = EmployeeSettings;
			}
		}

		public void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.Row == null)
			{
				return;
			}

			sender.SetValueExt(e.Row, _IsWorkersCompensationField.Name, WorkCode.Equals(e.NewValue));
			sender.SetValueExt(e.Row, _IsCertifiedProjectField.Name, CertifiedProject.Equals(e.NewValue));
			sender.SetValueExt(e.Row, _IsUnionField.Name, Union.Equals(e.NewValue));
		}

		private bool GetBool(PXCache sender, Type field, object row)
		{
			return true.Equals(sender.GetValue(row, field.Name));
		}
	}
}
