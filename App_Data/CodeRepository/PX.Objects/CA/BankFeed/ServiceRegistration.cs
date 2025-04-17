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
using Autofac;
using PX.Data;

namespace PX.Objects.CA.BankFeed
{
	internal class ServiceRegistration : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<MXBankFeedManager>()
				.Named<BankFeedManager>($"{nameof(BankFeedManager)}_{CABankFeedType.MX}")
				.SingleInstance();
			builder.RegisterType<PlaidBankFeedManager>()
				.Named<BankFeedManager>($"{nameof(BankFeedManager)}_{CABankFeedType.Plaid}")
				.SingleInstance();
			builder.RegisterType<PlaidBankFeedManager>()
				.Named<BankFeedManager>($"{nameof(BankFeedManager)}_{CABankFeedType.TestPlaid}")
				.SingleInstance();
			builder.RegisterType<BankFeedUserDataProvider>().SingleInstance().AsSelf();
			builder.Register<Func<string, BankFeedManager>>(c =>
			{
				var context = c.Resolve<IComponentContext>();
				return (string feedType) =>
				{
					var bankFeedManager = context.ResolveNamed<BankFeedManager>($"{nameof(BankFeedManager)}_{feedType}");
					if (bankFeedManager == null)
					{
						throw new PXArgumentException(nameof(feedType));
					}
					return bankFeedManager;

				};
			}).SingleInstance();
		}
	}
}
