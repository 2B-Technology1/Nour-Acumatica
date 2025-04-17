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
using PX.SM;
using System.Collections;
using System.Linq;
using PX.Common;

namespace PX.Objects.Cs.AdvancedAuthenticationRestriction
{
	public class PreferencesSecurityMaintExt : PXGraphExtension<PreferencesSecurityMaint>
	{
		[InjectDependency] private CS.IAdvancedAuthenticationRestrictor AdvancedAuthenticationRestrictor { get; set; }

		public IEnumerable identities()
		{
			return Base.identities()
				.OfType<PreferencesIdentityProvider>()
				.Where(p => AdvancedAuthenticationRestrictor.IsAllowedProviderName(p.ProviderName));
		}

		public virtual void _(Events.RowSelected<PreferencesIdentityProvider> e)
		{
			if(e?.Row==null)
				return;
			if (AdvancedAuthenticationRestrictor.IsDeprecatedProvider(e.Row.ProviderName))
				Base.Identities.Cache.RaiseExceptionHandling<PreferencesIdentityProvider.providerName>(e.Row, e.Row.ProviderName,
					new PXSetPropertyException(
						Messages.DeprecatedIdentityProvider,
						PXErrorLevel.RowWarning, e.Row.ProviderName));
		}
	}

	[PXLocalizable]
	internal class Messages
	{
		public const string DeprecatedIdentityProvider =
			"The {0} identity provider can no longer be configured on this form. Use the OpenID Providers (SM303020) form to configure external identity providers.";
	}
}
