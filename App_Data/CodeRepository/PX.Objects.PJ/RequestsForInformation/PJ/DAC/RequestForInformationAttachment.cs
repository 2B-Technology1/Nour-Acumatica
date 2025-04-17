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
using PX.Objects.CR;
using PX.Objects.PJ.Common.Descriptor;
using System;
using CRActivity = PX.Objects.CR.CRActivity;

namespace PX.Objects.PJ.RequestsForInformation.PJ.DAC
{
	[PXCacheName(CacheNames.RequestForInformationAttachment)]
	[Serializable]
	[PXProjection(typeof(Select5<RequestForInformation,
			LeftJoin<CRActivity, On<RequestForInformation.noteID, Equal<CRActivity.refNoteID>>,
			LeftJoin<NoteDoc, On<CRActivity.noteID, Equal<NoteDoc.noteID>,
							  Or<RequestForInformation.noteID, Equal<NoteDoc.noteID>>>>>,
			Where<NoteDoc.fileID, IsNotNull>,
			Aggregate<GroupBy<NoteDoc.fileID>>>), Persistent = false)]
	public class RequestForInformationAttachment : IBqlTable
	{
		public abstract class requestForInformationCd : BqlString.Field<requestForInformationCd> { }
		[PXDBString(IsUnicode = true, IsKey = true, BqlField = typeof(RequestForInformation.requestForInformationCd))]
		public virtual string RequestForInformationCd { get; set; }

		public abstract class fileID : BqlGuid.Field<fileID> { }
		[PXDBGuid(IsKey = true, BqlField = typeof(NoteDoc.fileID))]
		public Guid? FileID { get; set; }

		public abstract class outgoing : BqlBool.Field<outgoing> { }
		[PXDBBool(BqlField = typeof(CRActivity.outgoing))]
		public virtual bool? Outgoing { get; set; }
	}
}
