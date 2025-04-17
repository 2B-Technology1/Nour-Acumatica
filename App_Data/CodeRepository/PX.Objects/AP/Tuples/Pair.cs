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

namespace PX.Objects.AP
{
	public class Pair<FT, ST> : IComparable<Pair<FT, ST>>, IEquatable<Pair<FT, ST>>
		where FT : IComparable<FT>
		where ST : IComparable<ST>
	{
		public Pair(FT aFirst, ST aSecond)
		{
			this.first = aFirst;
			this.second = aSecond;
		}
		public FT first;
		public ST second;

		#region IComparable Pair<FT, ST> Members
		public virtual int CompareTo(Pair<FT, ST> other)
		{
			int res = this.first.CompareTo(other.first);
			if (res == 0) return (this.second.CompareTo(other.second));
			return res;
		}

		#endregion

		#region IEquatable<Pair<FT,ST>> Members
		public override int GetHashCode()
		{
			return 0;
		}

		public virtual bool Equals(Pair<FT, ST> other)
		{
			return (this.CompareTo(other) == 0);
		}

		public override bool Equals(Object other)
		{
			Pair<FT, ST> tmp = other as Pair<FT, ST>;
			if (tmp != null)
				return (this.CompareTo(tmp) == 0);
			return ((Object)this).Equals(other);
		}

		#endregion
	}

}
