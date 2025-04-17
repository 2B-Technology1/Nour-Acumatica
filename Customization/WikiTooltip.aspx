<%@ Page Language="C#" AutoEventWireup="true" CodeFile="WikiTooltip.aspx.cs" Inherits="Customization_WikiToolbar" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link href="../App_Themes/GetCSS.aspx?wiki=HelpRoot_QuickReference" type="text/css" rel="stylesheet" />
    <link href="../App_Themes/WikiList.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="wiki">
        <h1 class="wikiH1 separator" id="btnBackText"><%: labelText %></h1>
	
		<div id="wikiField" class="section sH1">
			<div id="wikiField-content" class="collapsable"><%= contextHelpData %></div>
			<div id="showMoreButton" class="show-more-button" style="display: none" onclick="showMoreClick()"><%= showMoreText %><span class="s-h-arrow"></span></div>
		</div>

		<div id="divFormRef" class="emptyspace"></div>

		<% if(!string.IsNullOrEmpty(dacBrowserToEntity.Link)) { %>
			<h1 class="wikiH1 separator">
				DAC Details
			</h1>
			<div class="section sH1">
                <p>
					<a id="dacBrowserToEntityRef" class="formRef" target="_blank" href="<%= dacBrowserToEntity.Link %>"><%= dacBrowserToEntity.Text %></a><% if(!string.IsNullOrEmpty(dacBrowserToField.Link)) { %>.<a id="dacBrowserToFieldRef" class="formRef" target="_blank" href="<%= dacBrowserToField.Link %>"><%= dacBrowserToField.Text %></a> <% } %>
                </p>
			</div>
            <div class="emptyspace"></div>
		<% } %>
        </div>
    </form>

	<script type="text/javascript">
        var screenId = "<%=screenId%>";
        var screenTitle = "<%= screenTitle %>";
		var showMoreText1 = "<%= showMoreText %>";

        function showMoreClick() {
			document.getElementById("showMoreButton").style.display = "none";
			document.getElementById("wikiField-content").classList.remove("collapsable");
		}

        function showMoreLinksClick(el) {
            el.style.display = "none";
            el.nextSibling.style.display = "block";
        }

        function renderSections(fetchedSections) {
            var sections = JSON.parse(fetchedSections || null);
            if (!sections) return;
            var sResultArr = [];
            var startTag = '<p>';
			var endTag = '</p>';
			var closeMore = false;
			var linkImageUrl = "../Frames/GetFile.ashx<%= linkImage%>&width=20";
            sections.forEach(function (sec) {
                sResultArr.push('<h1 class="wikiH1 separator">' + sec.header + '</h1><div class="section sH1">');
                var linkCount = sec.links && sec.links.length;
                if (linkCount) {
                    for (i = 0; i < linkCount; i++) {
                        var link = sec.links[i];
                        if (link.link.indexOf(link.text) > 0) {
                            var s = sResultArr.pop();
                            s = s.substring(0, s.length - endTag.length) + ' (<a id="formRef" class="formRef" target="_blank" href="' + link.link + '">' + link.text + '</a>)' + endTag;
                            sResultArr.push(s);
                        }
						else {
							var linkHTML = '<a id="formRef" class="formRef" target="_blank" href="' + link.link + '">' + link.text;
							if (link.hasVideo) {
								linkHTML += ' <img alt="" src="' + linkImageUrl + '">';
							}
							linkHTML += '</a>';
							sResultArr.push(startTag + linkHTML + endTag);
                        }
						if (i === 2 && linkCount > 4) {
                            sResultArr.push('<div class="show-more-button" onclick="showMoreLinksClick(this)">' + showMoreText1 + '<span class="s-h-arrow"></span></div>');
							sResultArr.push('<div class="show-more-data">');
							closeMore = true;
                        }
                    }
                }
				if (closeMore) sResultArr.push('</div>');
                sResultArr.push('</div>');
            });

			var divFormRef = document.querySelector("div#divFormRef");
			divFormRef.insertAdjacentHTML("afterend", sResultArr.join(''));
		}

		function renderFieldData() {
			var wikiField = document.getElementById("wikiField-content");
			if (wikiField.offsetHeight < wikiField.scrollHeight) {
				var showMoreButton = document.getElementById("showMoreButton");
				showMoreButton.style.display = "";
			}
		}

		function onload() {
			renderFieldData();
            var prefetchedSections = "<%= sections%>";
            if (!prefetchedSections) {
                window.__infotipsBusy = true;
                var xhr = new XMLHttpRequest();
                xhr.open("GET", "../ui/help/" + screenId + "/relatedinfo");
                xhr.onload = function () {
                    if (xhr.status == 200) {
                        renderSections(xhr.responseText);
                    }
                    else {
                        document.querySelector("div#divFormRef").style.display = 'none';
                    }
                    delete window.__infotipsBusy;
                }
                xhr.send();
            } else {
                var el = document.createElement("span");
                el.innerHTML = prefetchedSections;
                renderSections(el.innerHTML);
            }
        }

        window.addEventListener("load", onload);
	</script>
</body>
</html>
