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

using PX.CloudServices.DAC;
using PX.Common;
using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.AP.InvoiceRecognition.DAC
{
	[PXInternalUseOnly]
	[PXHidden]
	public class RecognizedRecordForProcessing : RecognizedRecord
	{
		[PXUIField(DisplayName = "Selected")]
		[PXBool]
		public virtual bool? Selected { get; set; }
		public abstract class selected : BqlBool.Field<selected> { }

		public new abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }
		public new abstract class owner : BqlInt.Field<owner> { }
		public new abstract class documentLink : BqlGuid.Field<documentLink> { }

		[PXUIField(DisplayName = "Recognition Result", Visible = false)]
		[PXString(IsUnicode = true)]
		public new virtual string RecognitionResult { get; set; }
		public new abstract class recognitionResult : BqlString.Field<recognitionResult> { }

		[PXUIField(DisplayName = "Recognition Feedback", Visible = false)]
		[PXString(IsUnicode = true)]
		public new virtual string RecognitionFeedback { get; set; }
		public new abstract class recognitionFeedback : BqlString.Field<recognitionFeedback> { }
	}
}
