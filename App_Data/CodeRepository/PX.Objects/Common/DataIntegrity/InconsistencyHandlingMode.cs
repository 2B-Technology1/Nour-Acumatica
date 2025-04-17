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

namespace PX.Objects.Common.DataIntegrity
{
	public class InconsistencyHandlingMode : ILabelProvider
	{
		/// <summary>
		/// The release integrity validator will do nothing.
		/// </summary>
		public const string None = "N";
		/// <summary>
		/// The release integrity validator will log errors into the <see cref="DataIntegrityLog"/> table.
		/// </summary>
		public const string Log = "L";
		/// <summary>
		/// The release integrity validator will prevent errors and not log them into the database.
		/// </summary>
		public const string Prevent = "P";

		public IEnumerable<ValueLabelPair> ValueLabelPairs => new ValueLabelList
		{
			// This option is not currently shown on the UI and will be
			// turned on for particular clients on a case-by-case basis, 
			// only if someone complains about performance.
			// -
			// { None, nameof(None) },
			{ Log, Messages.LogErrors },
			{ Prevent, Messages.PreventRelease },
		};
	}
}
