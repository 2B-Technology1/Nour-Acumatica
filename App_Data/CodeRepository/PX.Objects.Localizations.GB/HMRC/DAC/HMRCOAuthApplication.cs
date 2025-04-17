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
using PX.Data.ReferentialIntegrity.Attributes;
using PX.OAuthClient.DAC;

namespace PX.Objects.Localizations.GB.HMRC.DAC
{
	[PXHidden]
	public class HMRCOAuthApplication : OAuthApplication
	{
		public class PK : PrimaryKeyOf<HMRCOAuthApplication>.By<applicationID>
		{
			public static HMRCOAuthApplication Find(PXGraph graph, int? applicationID) => FindBy(graph, applicationID);
		}

		#region OAuthApplication
		public new abstract class applicationID : PX.Data.BQL.BqlInt.Field<applicationID> { }
		public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		public new abstract class applicationName : PX.Data.BQL.BqlString.Field<applicationName> { }
		public new abstract class clientID : PX.Data.BQL.BqlString.Field<clientID> { }
		public new abstract class clientSecret : PX.Data.BQL.BqlString.Field<clientSecret> { }
		public class HMRCApplicationType : PX.Data.BQL.BqlString.Constant<HMRCApplicationType>
		{
			public HMRCApplicationType()
				: base(MTDCloudApplicationProcessor.Type)
			{

			}
		}
		#endregion
	}
}
