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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Objects.PR.AUF
{
	public class EciRecord : AufRecord
	{
		public EciRecord(string employeeID) : base(AufRecordType.Eci)
		{
			_EmployeeID = employeeID;
		}

		public override string ToString()
		{
			List<object> lineData = new List<object>()
			{
				FormatSsn(SocialSecurityNumber, _EmployeeID, false),
				BirthDate,
				FirstName,
				MiddleName,
				LastName,
				NameSuffix
			};

			for (int i = 0; i < 12; i++)
			{
				lineData.Add(CoverageIndicator[i] == true ? AufConstants.SelectedBox : AufConstants.NotSelectedBox);
			}

			return FormatLine(lineData.ToArray());
		}

		public virtual string SocialSecurityNumber { get; set; }
		public virtual DateTime? BirthDate { get; set; }
		public virtual string FirstName { get; set; }
		public virtual string MiddleName { get; set; }
		public virtual string LastName { get; set; }
		public virtual string NameSuffix { get; set; }
		public virtual bool?[] CoverageIndicator { get; set; } = new bool?[12];

		private string _EmployeeID;
	}
}
