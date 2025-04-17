using CommonServiceLocator;
using Microsoft.Exchange.WebServices.Data;
using PX.Api.Web.Infotips.Interfaces;
using PX.Api.Web.Infotips.Models;
using PX.Api.Web.Infotips.Services;
using PX.Data;
using PX.SM;
using PX.Web.UI.WebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.Json;

public partial class Customization_WikiToolbar : Page
{
	internal string labelText;
	internal string showMoreText;
	internal string screenId;
	internal string screenTitle;
	internal (string Link, string Text) dacBrowserToField;
	internal (string Link, string Text) dacBrowserToEntity;
	internal string contextHelpData;
	internal string sections;
	internal string linkImage;

	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);

		var themeVariables = PXThemeLoader.GetThemeVariables(ServiceLocator.Current.GetInstance<ICurrentUserInformationProvider>().GetBranchCD());
		Header.Controls.AddAt(0, new LiteralControl($"<style >:root{{ {themeVariables} }}</style>"));

		var baseUrl = ResolveUrl(AppRelativeVirtualPath);
		Header.Controls.AddAt(0, new LiteralControl($"<base href=\"{baseUrl}\">"));
	}

	protected void Page_Load(object sender, EventArgs e)
    {
		screenId = Request.QueryString["screenId"];
		labelText = Request.QueryString["labelText"];
		screenTitle = Request.QueryString["screenTitle"];

		var screenRelatedInformation = ServiceLocator.Current.GetInstance<IHelpProvider>().GetScreenRelatedInformation(screenId);
		sections = HttpUtility.HtmlEncode(JsonSerializer.Serialize(screenRelatedInformation, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));

		linkImage = "?file=UserGuide/Images/icon_video_NAV.png";

		showMoreText = PXLocalizer.Localize(WikiTooltipMessages.More, typeof(WikiTooltipMessages).FullName);

		if (!DesignMode && !PXGraph.ProxyIsActive && ServiceLocator.IsLocationProviderSet)
		{
			GetData();
		}
	}

	#region Helpers
	private void GetData()
	{
		var fieldName = Request.QueryString["fieldName"];
		var viewName = Request.QueryString["viewName"];
		var clientId = Request.QueryString["clientId"];
		var path = Request.QueryString["path"];

		var wikiTooltipProvider = ServiceLocator.Current.GetInstance<IWikiTooltipProvider>();

		WikiTooltipResult fieldWiki;
		if (!string.IsNullOrEmpty(viewName)) fieldWiki = wikiTooltipProvider.GetTooltipInternal(path, fieldName, viewName);
		else if (!string.IsNullOrEmpty(clientId)) fieldWiki = wikiTooltipProvider.GetTooltipInternal(path, clientId);
		else return;

		var regex = new Regex("<wk-more>(?<first>.*)</wk-more>(?<second>.*)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		Match match = regex.Match(fieldWiki.Body);
		if (match.Success)
		{
			string first = match.Groups["first"]?.Value;
			string second = match.Groups["second"]?.Value;

			contextHelpData = first + second;
		}
		else
		{
			contextHelpData = fieldWiki.Body;
		}

		dacBrowserToField = (fieldWiki.LinkToField, fieldWiki.LinkToField?.Split("/".ToCharArray()).Last());
		dacBrowserToEntity = (fieldWiki.LinkToEntity, fieldWiki.LinkToEntity?.Split("/".ToCharArray()).Last());
	}
	#endregion

}
