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
using System.IO;
using System.Web;
using PX.Data;
using PX.Data.EP;
using PX.Export;
using PX.Export.Imc;

namespace PX.Objects.EP
{
	public sealed class PXCalendarSyncHandler : PXFileExportHandler
	{
		public const string DATA_SESSION_KEY = "CalendarSyncExportKeys";
		public const string FILE_EXTENSION = "Ics";
		private const string _CALENDAR_SETTINGS_KEY = "id";
		private const string _COMPANY_KEY = "cid";

		protected override string ContentType
		{
			get
			{
				return "Content-Type: text/calendar; charset=UTF-8";
			}
		}

		protected override string DataSessionKey
		{
			get
			{
				return DATA_SESSION_KEY;
			}
		}

		protected override bool NullableData
		{
			get
			{
				return true;
			}
		}

		protected override void Write(Stream stream, ProcessBag bag)
		{
			var calendar = bag.Data as vCalendarIcs;
			if (calendar == null)
			{
				var calendarSettingsId = bag.Parameters[_CALENDAR_SETTINGS_KEY];
				var companyId = bag.Parameters[_COMPANY_KEY];
				if (!string.IsNullOrEmpty(calendarSettingsId))
				{
					var syncGraph = PXGraph.CreateInstance<EPCalendarSync>();
					try
					{
						using (new PXLoginScope(string.IsNullOrEmpty(companyId) ? "admin" : ("admin@" + companyId), PXAccess.GetAdministratorRoles()))
						{
							var events = syncGraph.GetCalendarEvents(new Guid(calendarSettingsId));
							calendar = (vCalendarIcs)syncGraph.VCalendarFactory.CreateVCalendar(events);
						}

					}
					catch (FormatException) { }
				}
			}
			if (calendar == null) calendar = new vCalendarIcs();

			using (var sw = new StreamWriter(stream))
			{
				calendar.Write(sw);
			}
		}

		public static string GetSyncUrl(HttpContext context,  string userId)
		{
			String externalLink;

			externalLink = context.Request.GetWebsiteUrl().TrimEnd('/');
			externalLink += PX.Common.PXUrl.ToAbsoluteUrl(String.Format("~/calendarSync.ics?{0}={1}&{2}={3}", _CALENDAR_SETTINGS_KEY, userId, _COMPANY_KEY, PXAccess.GetCompanyName()));

			return externalLink;
		}
	}
}
