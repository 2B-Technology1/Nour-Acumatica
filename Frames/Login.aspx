﻿<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Login.master" ClientIDMode="Static" AutoEventWireup="true" 
	CodeFile="Login.aspx.cs" Inherits="Frames_Login" EnableEventValidation="false" ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/Login.master" %>
<asp:Content ID="Content2" ContentPlaceHolderID="phLogo" runat="Server">
	<asp:DropDownList runat="server" ID="cmbLang" CssClass="login_lang" AutoPostBack="true" OnSelectedIndexChanged="cmbLang_SelectedIndexChanged"/>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="phUser" Runat="Server">
	<asp:Label runat="server" Visible="False" ID="lblUnderMaintenance" CssClass="login_error"></asp:Label>
	<asp:Label runat="server" Visible="False" ID="lblUnderMaintenanceReason" CssClass="login_error"></asp:Label>

	<input runat="server" id="txtSingleCompany" type="hidden" />

	<div runat="server" id="tenantContainer">
		<asp:Label runat="server" ID="lblTenant" Text="Tenant" CssClass="tenant_caption" />
		<asp:DropDownList runat="server" ID="cmbCompany" CssClass="login_company border-box" AutoPostBack="true" OnSelectedIndexChanged="cmbCompany_SelectedIndexChanged" />
		<input runat="server" id="txtDummyCpny" type="hidden" />
		
		<asp:TextBox runat="server" ID="txtSecureTenantName" CssClass="login_tenant border-box" placeholder="Enter Your Company Name" Visible="false" />			
		<div runat="server" id="secureTenantNameDisplayContainer" class="input_with_button_wrapper" style="display: none">
			<asp:TextBox runat="server" ID="txtSecureTenantNameDisplay" CssClass="login_tenant border-box" />
			<asp:LinkButton runat="server" ID="btnSecureTenantCancelInline" ToolTip="Cancel" CssClass="cross_button"
				OnClick="btnSecureTenantCancel_Click" Text="<img src='../Icons/cross.png' alt='logo' />" />
		</div>
		<input runat="server" id="txtSecureTenantFlowState" type="hidden" />

		<asp:Button runat="server" ID="btnSecureTenantSubmit" Text="Next" OnClick="btnSecureTenantSubmit_Click" CssClass="login_button" Visible="false" />	
		<asp:Button runat="server" ID="btnSecureTenantCancel" Text="Cancel" OnClick="btnSecureTenantCancel_Click" CssClass="logincancel_button" Visible="false" />
	</div>

	<div runat="server" id="loginPasswordContainer">
		<asp:Label runat="server" ID="lblSignIn" Text="Sign in" CssClass="signin_caption" Visible="true" />
		<asp:TextBox runat="server" ID="txtUser" CssClass="login_user border-box" autocomplete="username" placeholder="Username" />
		<asp:TextBox runat="server" ID="txtPass" Width="100%" CssClass="login_pass border-box" 
			TextMode="Password" placeholder="Password" autocomplete="current-password" />
	</div>
				
	<asp:TextBox runat="server" ID="txtDummyPass" CssClass="login_pass dummy border-box" ReadOnly="true" Visible="false" />
	<input runat="server" id="txtVeryDummyPass" type="hidden" />

	<asp:TextBox runat="server" ID="txtNewPassword" TextMode="Password" CssClass="login_pass border-box" 
		placeholder="New Password" Visible="False" />
	<asp:TextBox runat="server" ID="txtConfirmPassword" TextMode="Password" CssClass="login_pass border-box" 
		placeholder="Confirm Password" Visible="False" />

	<div runat="server" id="divEula" Visible="false" style="margin-bottom: 10px;">
		<asp:CheckBox runat="server" ID="chkEula" OnClick="onchkEulaChanged(this.checked);" Style="float: left" />
		<span style="display: block;">
			<asp:Label ID="lbEula" runat="server" AssociatedControlID="chkEula" Text="Check here to indicate that you have read and agree to the terms of the " CssClass="labelB" />
			<asp:HyperLink ID="hlEula" runat="server" CssClass="login_link" NavigateUrl="~/EULA/saas.pdf" Text="Acumatica User Agreement" Target="_blank" />
		</span>
	</div>
	<div id="multiFactorTip" class="multi-factor-tip" style="display: none;">
		<div class="auth-caption">
			<i id="multiFactorIcon" class="ac ac-smartphone"></i>
			<asp:Label runat="server" ID="lbl2FactorCap" CssClass="labelB"></asp:Label>
		</div>
		<span id="lb2Factor" class="labelB auth-info"></span>
		<asp:TextBox runat="server" ID="oneTimePasswordText" CssClass="login_user border-box pass-text" style="display: none;"/>
		<input id="mfLoginButton" name="mfLoginButton" value="<%=PX.Data.PXLocalizer.Localize(PX.AscxControlsMessages.LoginScreen.SignIn)%>"  
			   type="button" style="float: left; margin-top: 10px; margin-bottom: 10px; display: none" class="login_button" />
		<div style="display: none; margin: 10px 0px; clear: both" id="rememberContainer">
			<asp:CheckBox runat="server" ID="rememberDevice" Checked="True"/>
		</div>
		<span id ="resendTimer" class="labelB auth-info" style="display: none; clear: both"></span>
		<input id="resendButton" name="resendButton" value="<%=PX.Data.PXLocalizer.Localize(PX.AscxControlsMessages.LoginScreen.SendAgain)%>" onclick="resend();" 
			   type="button" style="float: left; margin-top: 10px; margin-bottom: 10px; display: none; clear: both" class="login_button" />
		<div id="noDeviceSend" style="display: none; clear: both">
			<span id="noDeviceLabel" class="labelB auth-info"><%=PX.Data.PXLocalizer.Localize(PX.AscxControlsMessages.LoginScreen.MultifactorSendToDevice)%></span>
			<input id="noDeviceSendButton" name="noDeviceSendButton" value="<%=PX.Data.PXLocalizer.Localize(PX.AscxControlsMessages.LoginScreen.SendRequestToDevice)%>" onclick="startProviderSend('MobilePush', false, 'smartphone', true);" 
				   type="button" style="float: left; width: 219px; margin-top: 10px; margin-bottom: 10px;" class="login_button" />
		</div>
		<asp:HiddenField runat="server" ID="MultiFactorPipelineNotStarted" />
		<asp:HiddenField runat="server" ID="MultiFactorWarninigWasShown" />
	</div>
	<div id="retryAfterDeny" class="multi-factor-method" style="display: none">
		<input value="<%=PX.Data.PXLocalizer.Localize(PX.AscxControlsMessages.LoginScreen.TryAgain)%>" onclick="window.location.reload(); return false;" class="login_button" style="text-align: center; float: left;" />
	</div>

	<asp:TextBox runat="server" ID="txtRecoveryQuestion" CssClass="login_user border-box"  
		placeholder="Recovery Question" Visible="False" />
	<asp:TextBox runat="server" ID="txtRecoveryAnswer" CssClass="login_user border-box"
			placeholder="Your Answer" Visible="False" />

	<div id="openOtherMultiFactor" class="multi-factor-method" style="display: none; clear:both;">
		<a class="login_link multy-factor" href="javascript:void 0" onclick="showMultiFactorMenu(); return false;">
			<%=PX.Data.PXLocalizer.Localize(PX.AscxControlsMessages.LoginScreen.TwoFactorMethod)%>
		</a>
	</div>
	<div id="multiFactorMenu" style="display: none" class="list-group auth">
		<div class="auth-caption">
			<asp:Label runat="server" ID="lbl2FactorMethod" CssClass="labelB"></asp:Label>
		</div>
		<% foreach (var provider in this.MultifactorProviders)
			{%>
				<div id="<%=provider.Key+"buttonId"%>" class="multiFactorMenuItem list-group-item" onclick="startProviderSend('<%=provider.Key%>', 
    <%=provider.Value.ShowTextBox.ToString().ToLower()%>, '<%=this.GetLoginMethodIcon(provider.Key) %>', false );">
					<i class="<%="ac ac-fw ac-" + this.GetLoginMethodIcon(provider.Key) %>"></i>
					<span><%=PX.Data.PXLocalizer.Localize(provider.Value.ButtonToolTip) %></span>
				</div>
		<%} %>
	</div>

	<div runat="server" id="loginButtonsContainer">
		<asp:Button runat="server" ID="btnLogin" Text="Sign In" OnClick="btnLogin_Click" CssClass="login_button" OnClientClick="return wrapClick(this, 'isReal', realFlagContainer, doLogin, multiFactorNotStarted || secureTenantCheck)" />
		<asp:Button runat="server" ID="btnCancel" Text="Cancel" OnClick="btnCancel_Click" CssClass="logincancel_button" Visible="false" />
		
		<input runat="server" id="txtDummyInstallationID" type="hidden" />
		<asp:HyperLink ID="lnkForgotPswd" runat="server" CssClass="login_link" NavigateUrl="~/PasswordRemind.aspx" 
			Text="Forgot Your Credentials?" />
	</div>
	<script type="text/javascript">

        var connection = new signalR.HubConnectionBuilder()
            .withUrl(normalizeSignalRUrl("signalr/hubs/MultifactorHub")
                , {
                    transport: signalR.HttpTransportType.WebSockets
                    ,skipNegotiation: true
                }
            )
            .configureLogging(signalR.LogLevel.Debug)
            .build();
        var resend = function() {};
        var timeoutId = 0;

        var multiFactorNotStarted = $("[id$=<%=this.MultiFactorPipelineNotStarted.ID%>]").val() === 'true';
        var secureTenantCheck = $("[id$=<%=this.txtSecureTenantFlowState.ID%>]").val() === 'LoginPassTenantSelection';

        var trialMessage = '<%=HttpUtility.JavaScriptStringEncode(PX.Data.PXLocalizer.Localize(PX.AscxControlsMessages.LoginScreen.TrialPopupMessage))%>';
        var trialTitle = '<%=HttpUtility.JavaScriptStringEncode(PX.Data.PXLocalizer.Localize(PX.AscxControlsMessages.LoginScreen.TrialPopupTitle))%>';
        var agreeBtnLabel = '<%=HttpUtility.JavaScriptStringEncode(PX.Data.PXLocalizer.Localize(PX.AscxControlsMessages.LoginScreen.TrialPopupAgreeButtonLabel))%>';
        var disagreeBtnLabel = '<%=HttpUtility.JavaScriptStringEncode(PX.Data.PXLocalizer.Localize(PX.AscxControlsMessages.LoginScreen.TrialPopupDisagreeButtonLabel))%>';

        function normalizeSignalRUrl(url) {
            if (url.lastIndexOf("https://", 0) === 0 || url.lastIndexOf("http://", 0) === 0) {
                return url;
            }
            if (typeof window === "undefined" || !window || !window.document) {
                throw new Error("Cannot resolve '" + url + "'.");
            }
            var aTag = window.document.createElement("a");
            var base = aTag.baseURI;
            var baseIndex = base.indexOf("Frames");
            if (baseIndex > 0) base = base.substring(0, baseIndex);
            url = base + url;
            return url;
        }

		function onchkEulaChanged(checked)
		{
            btnLoginDisable(!checked);
		}

        var realFlagContainer = {
            isReal: false,
            isRealFederation: false,
            isRealOAuth: false,
            isRealOpenId: false
        }

        function wrapClick(e, isRealFlag, isRealFlagContainer, loginAction, needCheck) {
            if (isRealFlagContainer[isRealFlag] === true) {
                isRealFlagContainer[isRealFlag] = false;
                return loginAction(e);
            }

            var opt = {
                title: trialTitle,
                body: trialMessage,
                buttons: {
                    elements: [
                        {
                            text: agreeBtnLabel, click: function () {
                                isRealFlagContainer[isRealFlag] = true;
                                e.click();
                            }
                        },
                        { text: disagreeBtnLabel, }
                    ]
                }
            }


            if (needCheck) {
                var selectedOne = $('#txtSingleCompany');
                if (selectedOne) {
                    var istrial = selectedOne.attr('istrial');

                    if (istrial != null && istrial === 'true') {
                        adialog.dialog(opt);
                        return false;
                    }
                }

                var selectedItem = $('#cmbCompany').find(":selected");
                if (selectedItem) {
                    var istrial = selectedItem.attr('istrial');

                    if (istrial != null && istrial === 'true') {
                        adialog.dialog(opt);
                        return false;
                    }
                }
            } 

            isRealFlagContainer[isRealFlag] = true;
            return wrapClick(e, isRealFlag, isRealFlagContainer, loginAction, needCheck);
        }

        function doLogin(e) {
            var multiFactorNotStarted = $("[id$=<%=this.MultiFactorPipelineNotStarted.ID%>]").val() === 'true';

            var login = $("[id$=<%=this.txtUser.ID%>]").val();
            if (!login) return false;

            var isOutlookPlugin = localStorage.getItem('doRedirect');
            if (isOutlookPlugin) {
                var lang = $('[id$=<%=this.cmbLang.ID%>]').val();
                if (lang) {
                    localStorage.setItem('acumaticaLocale', lang);
                }
            }

			window.delayedCallback = true;
			clearInterval(timeoutId);
			if (e == null)
				e = window.event;
			if (e && (e.ctrlKey || e.shiftKey) && e.preventDefault != null)
				e.preventDefault();
			
			if (multiFactorNotStarted) {
				disableLoginFields();
				startTwoFactorPipeline();
				return false;
			}
			else {
				delete window.delayedCallback;
			}
		}

        function startTwoFactorPipeline() {
            connection.on("setOneTimePassword",
                function(oneTimePassword, correlationCode) {
                    $('[id$=<%=this.oneTimePasswordText.ID%>]').val(oneTimePassword);
                    $("[id$=<%=this.MultiFactorPipelineNotStarted.ID%>]").val('false');
                    btnLoginDisable(false);
                    var btnLogin = document.getElementById('btnLogin');
                    realFlagContainer.isReal = true;
                    btnLogin.click();
                });

            connection.on("denyAcceptTwoFactor",
                function(correlationCode) {
                    $('[id$=<%=this.oneTimePasswordText.ID%>]').val("");
                    $("#lblMsg").text("<%=PX.Data.PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.LoginScreen.TryAgainMessage)%>");
                    $("#openOtherMultiFactor").hide();
                    $(".multi-factor-tip").hide();
                    $("[id$=<%=this.txtUser.ID%>]").hide();
                    $('#retryAfterDeny').show();
                });
            startHub(0);
        }

		function btnLoginDisable(value) {
			var btnLogin = document.getElementById('btnLogin');
			if (!btnLogin) return;
			btnLogin.disabled = !!value;
		}

		function disableLoginFields(disableCompany) {
			$("[id$=<%=this.txtUser.ID%>]").attr('readonly', 'readonly');
			$("[id$=<%=this.txtPass.ID%>]").attr('readonly', 'readonly');
			if ((disableCompany || "") != "none") $("[id$=<%=this.cmbCompany.ID%>] option:not([selected])").attr('disabled', 'disabled');
			btnLoginDisable(true);
			$("#lblMsg").text("");
			document.body.style.cursor = 'wait';
		}

		function enableLoginFields(withUser) {
            if (withUser) $("[id$=<%=this.txtUser.ID%>]").attr('readonly', null);
			$("[id$=<%=this.txtPass.ID%>]").attr('readonly', null);
			$("[id$=<%=this.cmbCompany.ID%>] option:not([selected])").attr('disabled', null);
			btnLoginDisable(false);
			document.body.style.cursor = '';
		}

		function hideLoginFields() {
			$("[id$=<%=this.txtPass.ID%>]").hide();
			$("[id$=<%=this.lblTenant.ID%>]").hide();
			$("[id$=<%=this.cmbCompany.ID%>]").hide();
			$("[id$=<%=this.txtSecureTenantNameDisplay.ID%>]").hide();
			$("[id$=<%=this.btnLogin.ID%>]").hide();
			$("#login_ext").hide();
		}

        function startHub(retryCounter) {
            console.log("Start hub connection");
            $('#mfLoginButton').click(function () {
                realFlagContainer.isReal = true;
                btnLoginDisable(false);
                document.getElementById('btnLogin').click();
            });
            if(connection.connection.connectionState === 2)
                connection.start()
                    .then(r=>
                    {
                        window.console.log("Hub connection started; transport = "+connection.transport);
                        startProviderSend("MobilePush", false, "smartphone", false);
                    })
                    .catch(e => {
                        window.console.log(e);
                        if (retryCounter < 5) {
                            setTimeout(function() {
                                startHub(retryCounter+1);
                            }, 2000);
                        } else {
                            $("[id$=<%=this.MultiFactorPipelineNotStarted.ID%>]").val('false');
                            var btnLogin = document.getElementById('btnLogin');
                            realFlagContainer.isReal = true;
                            enableLoginFields(true);
                            btnLogin.click();
                        }
                    });
        }

        function showMultiFactorMenu()
        {
            $("#multiFactorMenu").show();
            $('[id$=<%=this.oneTimePasswordText.ID%>]').hide();
            $("#openOtherMultiFactor").hide();
            $('#multiFactorTip').hide();
            $('#noDeviceSend').hide();
            $("#mfLoginButton").hide();
            stopTimer();
        }

        function stopTimer() {
            if(timeoutId!=0)
                clearInterval(timeoutId);
            $("#resendTimer").hide();
            $("#resendTimer").html("");
            $("#resendButton").hide();
        }

        function hideMultiFactorMenu()
        {
            $("#multiFactorMenu").hide();
            $("#openOtherMultiFactor").show();
            $('#multiFactorTip').show();
            document.getElementById("lnkForgotPswd").classList.add("multy-factor");
            document.getElementById("lblSignIn").innerHTML = "<%=PX.Data.PXLocalizer.Localize(PX.AscxControlsMessages.LoginScreen.Username)%>";

        }

        function SetVisibilityForMultiFactorMenuItems(providers)
        {
            $(".multiFactorMenuItem").hide();
            for (i = 0; i < providers.length; i++)
            {
                $("#" + providers[i] + "buttonId").show();
            }
        };

        function startProviderSend(providerType, showTextbox, iconName, noDeviceSend)
        {
            var login = $("[id$=<%=this.txtUser.ID%>]").val();
            var pass = $("[id$=<%=this.txtPass.ID%>]").val();
			
            startTwoFactorPipeLine(providerType, login, pass, 0, showTextbox, iconName, noDeviceSend);
        }

       

        function startTwoFactorPipeLine(providerType, login, pass, retryCount, showTextbox, iconName, noDeviceSend) {
            $("#lblMsg").text("");
            if (connection.state === "Disconnected") {
                connection.start().then(r => {
                    window.console.log("Hub connection started; transport = "+connection.transport);
                    startTwoFactorPipeLine(providerType, login, pass, retryCount, showTextbox, iconName, noDeviceSend);
                }).catch (e=> {
                    window.console.log(e);
                    document.getElementById('btnLogin').click();
                });
            }
            //var hub = $.connection.multifactorHub;
            console.log("Start two factor pipeline, provider: " + providerType);
            var lang=$('[id$=<%=this.cmbLang.ID%>]').val();
            connection.invoke("startTwoFactorPipeline", login, pass, providerType, lang).then(result =>
            {
                console.log("Success");
                stopTimer();
                if (result.isMultiFactor>0) {
                    $("#lb2Factor").html(result.text.replace(new RegExp("\r\n", "g"), "<br />"));
                    $('#multiFactorTip').show();
                    $('#multiFactorIcon').attr('class', 'ac ac-' + iconName);
                    document.body.style.cursor = '';
                    hideLoginFields();
                    if (showTextbox&&!result.isError) {
                        $('[id$=<%=this.oneTimePasswordText.ID%>]').show();
                        $("#mfLoginButton").show();
                        $("[id$=<%=this.MultiFactorPipelineNotStarted.ID%>]").val('false');
                    } else {
                        $('[id$=<%=this.oneTimePasswordText.ID%>]').hide();
                        $("#mfLoginButton").hide();
                    }
                    SetVisibilityForMultiFactorMenuItems(result.providers);
                    hideMultiFactorMenu();
                    if (result.resendTimer > 0&&!(result.hasNoDevice&&noDeviceSend))
                        startResendTimer(result.resendTimer, providerType, showTextbox, iconName, false);
                    if (result.isMultiFactor == 1&&!result.hasNoDevice) {
                        $("#rememberContainer").show();
                    } else {
                        $("#rememberContainer").hide();
                    }
                    if (result.hasNoDevice) {
                        $("#noDeviceSend").show();
                        if (noDeviceSend) {
                            $("#lblMsg").text("<%=PX.Data.PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.LoginScreen.FailedToSendToDevice)%>");
                        }
                    } else {
                        $("#noDeviceSend").hide();
                    }
                    delete window.delayedCallback;
                } else {
                    $("[id$=<%=this.MultiFactorPipelineNotStarted.ID%>]").val('false');
                    realFlagContainer.isReal = true;
                    btnLoginDisable(false);
                    document.getElementById('btnLogin').click();
                    btnLoginDisable(true);
                }
                if (result.isError)
                {
                    $("#lb2Factor").addClass("error");
                    $("#rememberContainer").hide();
                    stopTimer();
                }
                else
                {
                    $("#lb2Factor").removeClass("error");
                }
            }).catch(error => {
                if (retryCount < 5) {
                    console.log('Invocation of startTwoFactorPipeline failed. Error: ' + error + '; Retry');
                    startTwoFactorPipeLine(providerType, login, pass, retryCount + 1, showTextbox, iconName, noDeviceSend);
                } else {
                    console.log('Invocation of startTwoFactorPipeline failed. Error: ' + error);
                    $("#lblMsg").text(error);
                    enableLoginFields(true);
                }
            });
        }

        function startResendTimer(time, providerType, showTextbox, iconName, noDeviceSend) {
            resend = function() {
                $("#resendButton").hide();
                startProviderSend(providerType, showTextbox, iconName, noDeviceSend);
            };
            var setTimerSpan = function(countDown) {
                var minutes = Math.floor(countDown / 60);
                var seconds = "0"+countDown % 60;
                $("#resendTimer")
                    .html("<%=PX.Data.PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.LoginScreen.ResendTimer)%> "+minutes+":"+seconds.substr(seconds.length-2));
            };
            if (time === 0)
                $("#resendButton").show();
            else {
                $("#resendTimer").show();
                setTimerSpan(time);
                timeoutId = setInterval(function() {
					
                    if (time <= 0) {
                        $("#resendButton").show();
                        $("#resendTimer").hide();
                        $("#resendTimer").html("");
                        clearInterval(timeoutId);
                        timeoutId = 0;
                    }
                    time--;
                    setTimerSpan(time);
                }, 1000);
            }
        }

        function ShowNoDeviceSendButton() {
            $("#noDeviceSend").show();
        };
    </script>
</asp:Content>

<asp:Content ID="extLogins" ContentPlaceHolderID="phExt" Runat="Server">
	<asp:LinkButton runat="server" ID="btnLoginFederation" class="extlogin_wide_button" OnClick="btnLoginFederation_Click" OnClientClick="return wrapClick(this, 'isRealFederation', realFlagContainer, function() {return true}, true)" Visible="false" 
		Text="<img src='../Icons/loginFederation_new.png' alt='logo' /><span>Active Directory</span>" />
	<!-- Oidc providers will be added dinamically here -->
	<asp:LinkButton runat="server" ID="btnMoreOpenIdProviders" class="extlogin_wide_button" OnClick="btnMoreOpenIdProviders_Click" Visible="false" 
		Text="<img src='../Icons/moreOidcProviders.png' alt='logo' /><span>More sign-in options for tenant</span>"/>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="phInfo" runat="Server">
	<div runat="server" id="login_info" style="display:none;">
		<div id="logOutReasone" runat="server" style="display:none;">
			<div runat="server" id="logOutReasoneMsg" class="login_error">Last update was unsuccessful.</div>
		</div>
		<div id="dbmsMisconfigured" runat="server" style="display:none;">
			<div runat="server" id="dbmsProblems" class="login_error">There are problems on database server side:</div>
			<div runat="server" id="dbmsMisconfiguredLabel" class="label">Contact server administrator.</div>
		</div>
		<div id="updateError" runat="server" style="display:none;">
			<div runat="server" id="updateErrorMsg" class="login_error">Last update was unsuccessful.</div>
			<div runat="server" id="updateErrorLabel" class="label">Contact server administrator.</div>
		</div>
		<div id="customizationError" runat="server" style="display:none;">
			<div runat="server" id="customizationErrorMsg" class="login_error">Warning: customization failed to apply automatically after the upgrade.</div>
			<div runat="server" id="customizationErrorLabel" class="label">
				Some functionality may be unavailable.<br /> Contact server administrator.<br />
				Click <a href="#" onclick="document.getElementById('custErrorDetails').style.display='';">
				here</a> to view details about this error.
			</div>
			<div style="display:none; width: 100%; height: 200px; margin-top: 10px;" id="custErrorDetails">
				<pre runat="server" id="custErrorContent"></pre>
			</div>
		</div>
		<div id="passwordRecoveryError" runat="server" style="display:none">
			<div class="login_error" id="passwordRecoveryErrorMsg" runat="server" />
		</div>
	</div>
</asp:Content>

<asp:Content ID="Content5" ContentPlaceHolderID="phLinks" runat="Server">
	
</asp:Content>

<asp:Content ID="Content6" ContentPlaceHolderID="phStart" runat="Server">
	<script type='text/javascript'>
        window.onload = function ()
        {
            try
            {
                if (window != window.top && window.top.location.origin == window.location.origin)
                {
                    if (window.top.location.pathname.split("/")[1] == window.location.pathname.split("/")[1]) {
                        window.top.location.href = window.top.location.href;
                    }
                }
            }
            catch (ex) { }
            var cmbCompanyEl = document.getElementById("cmbCompany");
            if (cmbCompanyEl) cmbCompanyEl.addEventListener("change", function (el) {
                disableLoginFields("none");
            });
            document.getElementById("login_data").style.paddingBottom = (document.getElementById("login_copyright").clientHeight + 40) + "px";
            var editor = document.form1['txtUser'];
            if (editor == null || editor.readOnly) editor = document.form1['txtNewPassword'];
            if (editor && !editor.readOnly) editor.focus();	    }
    </script>
</asp:Content>

