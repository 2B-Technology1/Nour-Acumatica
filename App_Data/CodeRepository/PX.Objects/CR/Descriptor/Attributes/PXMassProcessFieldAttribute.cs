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
using System.Net.Mail;
using PX.Common;
using PX.Common.Mail;
using PX.Data;
using PX.Objects.Common.Extensions;

namespace PX.Objects.CR.MassProcess
{
	public abstract class PXMassProcessFieldAttribute : PXEventSubscriberAttribute
	{
		private Type _searchCommand;

		public Type SearchCommand
		{
			get { return _searchCommand; }
			set
			{
				if (value != null && !typeof(BqlCommand).IsAssignableFrom(value))
					throw new ArgumentException(string.Format("Type '{0}' must inherite '{1}' type.",
						value.GetLongName(), typeof(BqlCommand).GetLongName()),
						"value");

				if (value != null && !typeof(IBqlSearch).IsAssignableFrom(value))
					throw new ArgumentException(string.Format("Type '{0}' must implement interface '{1}'.",
						value.GetLongName(), typeof(IBqlSearch).GetLongName()),
						"value");

				_searchCommand = value;
			}
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
	public class PXMassUpdatableFieldAttribute : PXMassProcessFieldAttribute
	{
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public class PXMassMergableFieldAttribute : PXEventSubscriberAttribute
	{
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
	public class PXContactInfoFieldAttribute : PXEventSubscriberAttribute
	{
	}
}
