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
using PX.Objects.Localizations.GB.HMRC.DAC;
using PX.OAuthClient;
using PX.OAuthClient.DAC;
using PX.Data.BQL.Fluent;

namespace PX.Objects.Localizations.GB.HMRC
{
#pragma warning disable PX1016 // A graph extension must include a public static IsActive method with the bool return type. Extensions which are constantly active reduce performance. Suppress the error if you need the graph extension to be constantly active.
	public class ApplicationMaintExt : PXGraphExtension<ApplicationMaint>
#pragma warning restore PX1016 // A graph extension must include a public static IsActive method with the bool return type. Extensions which are constantly active reduce performance. Suppress the error if you need the graph extension to be constantly active.
	{
	    private const string ExternalStorage = "StoredExternally";

		#region Views
		public SelectFrom<BAccountMTDApplication>.
			Where<BAccountMTDApplication.applicationID.IsEqual<OAuthApplication.applicationID.AsOptional>>.View
			CurrentBAccountMTDApplication;
		#endregion

		#region Event Handlers
		public void OAuthApplication_RowSelected(PXCache sender, PXRowSelectedEventArgs e, PXRowSelected BaseInvoke)
        {
            BaseInvoke?.Invoke(sender, e);
            OAuthApplication oAuthApplication = e.Row as OAuthApplication;
            if (oAuthApplication == null) return;
            bool showConnectionSettings = oAuthApplication.Type != MTDCloudApplicationProcessor.Type;
            PXUIFieldAttribute.SetVisible<OAuthApplication.clientID>(sender, oAuthApplication, showConnectionSettings);
            PXUIFieldAttribute.SetVisible<OAuthApplication.clientSecret>(sender, oAuthApplication, showConnectionSettings);
            PXUIFieldAttribute.SetVisible<OAuthApplication.returnUrl>(sender, oAuthApplication, showConnectionSettings);
        }

        public void _(Events.FieldUpdated<OAuthApplication.type> e)
        {
	        if (e.Row == null || e.NewValue == null) return;
	        if ((string) e.NewValue == MTDCloudApplicationProcessor.Type)
	        {
		        e.Cache.SetValue<OAuthApplication.clientID>(e.Row, ExternalStorage);
		        e.Cache.SetValue<OAuthApplication.clientSecret>(e.Row, ExternalStorage);
	        }
        }

		public void _(Events.RowDeleted<OAuthApplication> e)
		{
			if (CurrentBAccountMTDApplication.Select(e.Row.ApplicationID) != null)
			{
				CurrentBAccountMTDApplication.Delete(CurrentBAccountMTDApplication.Select(e.Row.ApplicationID));
			}
		}

        #endregion
    }
}
