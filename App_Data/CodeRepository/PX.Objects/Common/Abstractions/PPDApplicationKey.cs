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

using PX.Common;
using System.Linq;
using System.Reflection;

namespace PX.Objects.Common.Abstractions
{
	public class PPDApplicationKey
	{
		private readonly FieldInfo[] _fields;

		public int? BranchID;
		public int? BAccountID;
		public int? LocationID;
		public string CuryID;
		public decimal? CuryRate;
		public int? AccountID;
		public int? SubID;
		public string TaxZoneID;

		public PPDApplicationKey()
		{
			_fields = GetType().GetFields();
		}

		public override bool Equals(object obj)
		{
			FieldInfo info = _fields.FirstOrDefault(field => !Equals(field.GetValue(this), field.GetValue(obj)));
			return info == null;
		}
		public override int GetHashCode()
		{
			int hashCode = 17;
			_fields.ForEach(field => hashCode = hashCode * 23 + field.GetValue(this).GetHashCode());
			return hashCode;
		}
	}
}
