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

using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.GraphQL
{
	/// <summary>
	/// An entity in the external system that has a unique ID.
	/// </summary>
	public interface INode
	{
		/// <summary>
		/// THe unique id in the external system.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Equality comparer for INode objects.
		/// </summary>
		public class NodeEqualityComparer : IEqualityComparer<INode>
		{
			/// <inheritdoc />
			public bool Equals(INode x, INode y)
			{
				if (ReferenceEquals(x, y))
				{
					return true;
				}

				if (ReferenceEquals(x, null))
				{
					return false;
				}

				if (ReferenceEquals(y, null))
				{
					return false;
				}

				if (x.GetType() != y.GetType())
				{
					return false;
				}

				return x.Id == y.Id;
			}

			public int GetHashCode(INode obj)
			{
				return (obj.Id != null ? obj.Id.GetHashCode() : 0);
			}
		}
	}
}

