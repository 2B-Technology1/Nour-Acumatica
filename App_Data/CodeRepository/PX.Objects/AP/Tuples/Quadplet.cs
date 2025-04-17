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
	public class Quadplet<T1, T2, T3, T4> : IComparable<Quadplet<T1, T2, T3, T4>>, IEquatable<Quadplet<T1, T2, T3, T4>>
		where T1 : IComparable<T1>
		where T2 : IComparable<T2>
		where T3 : IComparable<T3>
		where T4 : IComparable<T4>
	{
		public Quadplet(T1 aArg1, T2 aArg2, T3 aArg3, T4 aArg4)
		{
			this.first = aArg1;
			this.second = aArg2;
			this.third = aArg3;
			this.fourth = aArg4;
		}
		public T1 first;
		public T2 second;
		public T3 third;
		public T4 fourth;

		#region IComparable<Quadplet<T1,T2,T3,T4>> Members
		public virtual int CompareTo(Quadplet<T1, T2, T3, T4> other)
		{
			int res = this.first.CompareTo(other.first);
			if (res == 0)
				res = this.second.CompareTo(other.second);
			if (res == 0)
				res = this.third.CompareTo(other.third);
			if (res == 0)
				return this.fourth.CompareTo(other.fourth);
			return res;
		}

		#endregion

		#region IEquatable<Quadplet<T1,T2,T3>> Members
		public override int GetHashCode()
		{
			return 0;
		}

		public virtual bool Equals(Quadplet<T1, T2, T3, T4> other)
		{
			return (this.CompareTo(other) == 0);
		}

		public override bool Equals(Object other)
		{
			Quadplet<T1, T2, T3, T4> tmp = other as Quadplet<T1, T2, T3, T4>;
			if (tmp != null)
				return (this.CompareTo(tmp) == 0);
			return ((Object)this).Equals(other);
		}

		#endregion
	}
}
