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

using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace PX.Commerce.Shopify
{
    internal static class ShopifyAppConnectedApplication
    {
        //Here until platform allows storing in DB
        internal const string ShopifyAppClientId = "970cbcb2-0169-e1b6-e8ff-8f171b2e691f";
        private static readonly string ShopifyAppClientSecret = "7bTuyRGipXif0V2MPpBCgwcKnSK4J9MX1nHrS3ZXvKQ=";

        private static readonly List<string> ShopifyAppRedirectUris = new List<string>
        {
			"https://commerceconnect.acumatica.com/auth/callback",
			"https://commerceconnect.acumatica.com/spc/auth/callback",
			"https://shopifyconnect.acumatica.com/auth/callback",
			"https://getpostman.com/oauth2/callback",
			"https://oauth.pstmn.io/v1/callback"
		};

        internal static Client ShopifyAppClientForRegistration()
        {
            var shopifyAppClient = new Client
			{
                ClientName = "Shopify Public App",
                Enabled = true,
                AllowedGrantTypes = GrantTypes.Code,
                RedirectUris = ShopifyAppRedirectUris,
                ClientSecrets = new List<Secret> { new Secret(ShopifyAppClientSecret.Sha256()) },
                AccessTokenType = AccessTokenType.Reference,
                RequireConsent = false,
                AllowedScopes = new List<string>
                {
					"api"
                }
            };
            return shopifyAppClient;
        }
    }
}
