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
using PX.Data.BQL;
using PX.Payroll.Data;
using System;

namespace PX.Objects.PR
{
	public abstract class DeductionSourceListAttribute : PXStringListAttribute, IPXRowSelectedSubscriber
	{
		public DeductionSourceListAttribute() : base(
			new string[] { EmployeeSettings, CertifiedProject, Union, WorkCode },
			new string[] { Messages.EmployeeSettingsSource, Messages.CertifiedProjectSource, Messages.UnionSource, Messages.WorkCodeSource })
		{ }

		public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (PRCountryAttribute.GetPayrollCountry() != LocationConstants.USCountryCode)
			{
				SetList(sender, e.Row, FieldName,
					new string[] { EmployeeSettings, Union, WorkCode },
					new string[] { Messages.EmployeeSettingsSource, Messages.UnionSource, Messages.WorkCodeSource });
			}
		}

		public const string EmployeeSettings = "SET";
		public const string CertifiedProject = "PRO";
		public const string Union = "UNI";
		public const string WorkCode = "WCC";

		public class employeeSetting : BqlString.Constant<employeeSetting>
		{
			public employeeSetting() : base(EmployeeSettings) { }
		}

		public class certifiedProject : BqlString.Constant<certifiedProject>
		{
			public certifiedProject() : base(CertifiedProject) { }
		}

		public class union : BqlString.Constant<union>
		{
			public union() : base(Union) { }
		}

		public class workCode : BqlString.Constant<workCode>
		{
			public workCode() : base(WorkCode) { }
		}

		public static string GetSource(PRDeductCode deductCode)
		{
			if (deductCode.IsWorkersCompensation == true)
			{
				return WorkCode;
			}
			if (deductCode.IsCertifiedProject == true)
			{
				return CertifiedProject;
			}
			if (deductCode.IsUnion == true)
			{
				return Union;
			}
			return EmployeeSettings;
		}
	}
}
