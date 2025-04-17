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
using PX.Objects.EP.Graphs.EPEventMaint.Extensions;


namespace PX.Objects.EP.Graphs.EPEventMaint.Extensions
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class EPEventMaint_AttendeeExt_BackwardCompatibility
		: PXGraphExtension<EPEventMaint_AttendeeExt, PX.Objects.EP.EPEventMaint>
	{
		public const int ManualAttendeeType = 0;
		public const int ContactAttendeeType = 1;

		public const string CbApi_Type_FieldName = "Attendee$Type";
		public const string CbApi_Key_FieldName = "Attendee$Key";

		public override void Initialize()
		{
			Base1.Attendees.Cache.Fields.Add(CbApi_Key_FieldName);
			Base.FieldSelecting.AddHandler(
				typeof(EPAttendee),
				CbApi_Key_FieldName,
				(s, e) =>
				{
					if (e.Row is EPAttendee attendee)
					{
						e.ReturnValue = attendee.ContactID?.ToString() ?? attendee.AttendeeID?.ToString();
					}
				});

			Base1.Attendees.Cache.Fields.Add(CbApi_Type_FieldName);
			Base.FieldSelecting.AddHandler(
				typeof(EPAttendee),
				CbApi_Type_FieldName,
				(s, e) =>
				{
					if (e.Row is EPAttendee attendee)
					{
						e.ReturnValue = attendee.ContactID != null ? ContactAttendeeType : ManualAttendeeType;
					}
				});
		}
	}
}
