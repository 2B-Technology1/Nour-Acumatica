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
	public class FfdRecord : AufRecord
	{
		public FfdRecord(int ffdID) : base(AufRecordType.Ffd)
		{
			FfdID = ffdID;
		}

		public override string ToString()
		{
			object[] lineData =
			{
				AufConstants.UnusedField, // Form Name
				AufConstants.UnusedField, // Field Name
				AufConstants.UnusedField, // Item Number
				AufConstants.UnusedField, // Field Value
				AufConstants.UnusedField, // State
				FfdID
			};

			return FormatLine(lineData);
		}

		public virtual int FfdID { get; set; }
	}
}
