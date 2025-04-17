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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CA;

namespace PX.Objects.CA
{
	public class CABankFeedStatus
	{
		public const string Active = "A";
		public const string Suspended = "S";
		public const string Disconnected = "D";
		public const string SetupRequired = "R";
		public const string MigrationRequired = "M";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(GetStatuses)
			{

			}

			public static (string, string)[] GetStatuses => new[] {
				(Active, Messages.Active), (Suspended, Messages.Suspended),
				(Disconnected, Messages.Disconnected), (SetupRequired, Messages.SetupRequired),
				(MigrationRequired, Messages.MigrationRequired)
			};
		}

		public class active : PX.Data.BQL.BqlString.Constant<active>
		{
			public active() : base(Active) {; }
		}

		public class setupRequired : PX.Data.BQL.BqlString.Constant<setupRequired>
		{
			public setupRequired() : base(SetupRequired) {; }
		}

		public class suspended : PX.Data.BQL.BqlString.Constant<suspended>
		{
			public suspended() : base(Suspended) {; }
		}

		public class disconnected : PX.Data.BQL.BqlString.Constant<disconnected>
		{
			public disconnected() : base(Disconnected) {; }
		}
	}
}
