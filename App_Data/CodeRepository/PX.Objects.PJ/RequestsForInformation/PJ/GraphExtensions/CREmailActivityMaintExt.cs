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
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.RequestsForInformation.Descriptor;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using System;
using System.Linq;
using static PX.Objects.CR.CREmailActivityMaint;

namespace PX.Objects.PJ.RequestsForInformation.PJ.GraphExtensions
{
	public class CREmailActivityMaintExt : PXGraphExtension<CREmailActivityMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
		}

		[PXOverride]
		public virtual void SendEmail(SendEmailParams sendEmailParams, Action<CRSMEmail> handler, Action<SendEmailParams, Action<CRSMEmail>> baseHandler)
		{
			if (IsRequestForInformationEmail(sendEmailParams))
			{
				var currentRequestForInformationCD = RequestForInformationContext.CurrentRequestForInformationCD;

				if (!string.IsNullOrWhiteSpace(currentRequestForInformationCD))
				{
					RequestForInformation currentRequestForInformation
						= SelectFrom<RequestForInformation>
						.Where<RequestForInformation.requestForInformationCd.IsEqual<P.AsString>>
						.View.Select(Base, currentRequestForInformationCD);

					if (currentRequestForInformation != null)
					{
						baseHandler(sendEmailParams, email => {
							handler?.Invoke(email);

							email.RefNoteIDType = currentRequestForInformation.GetType().FullName;
							email.RefNoteID = currentRequestForInformation.NoteID;
						});

						return;
					}
				}
			}

			baseHandler(sendEmailParams, handler);
		}

		static bool IsRequestForInformationEmail(SendEmailParams sendEmailParams)
			=> sendEmailParams != null
			&& sendEmailParams.Attachments != null
			&& sendEmailParams.Attachments.Any(attachment
				=> !string.IsNullOrWhiteSpace(attachment?.FullName)
				&& attachment.FullName.StartsWith(ScreenIds.RequestForInformationReport, StringComparison.OrdinalIgnoreCase));
	}
}
