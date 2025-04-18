﻿using System;

namespace PX.AscxControlsMessages
{
	[PX.Common.PXLocalizable]
	public static class LoginScreen
	{
	    public const string FailedToSendToDevice = "There is no registered device, so the confirmation request could not be sent. Please follow the instructions below to register a device. If you are not able to register a device now, use another authentication method.";
	    public const string PleaseChangePassword = "You are required to change password. Please, enter your new password below.";
		public const string InvalidRecoveryAnswer = "Invalid recovery answer.";
		public const string NewPasswordMustDiffer = "New password must differ from the old one.";
		public const string PasswordNotConfirmed = "The password entered doesn't match confirmation.";
		public const string PasswordBlank = "New password can not be blank.";
		public const string EmailEmpty = "Please, enter your e-mail address.";
		public const string InvalidQueryString = "Invalid query string parameters, e-mail could not be sent.";
		public const string PasswordSent = "The e-mail containing instructions on further actions is sent to your address.";
		public const string InvalidLogin = "Please, enter your valid login.";
		public const string PleaseSelectCompany = "Please select company.";
		public const string IncorrectLoginSymbols = "Invalid credentials. Please try again.";
		public const string TwoFactorAuth = "Two-Factor Authentication";
		public const string TwoFactorMethod = "Use another authentication method";
	    public const string TryAgainMessage =
	        "The confirmation request has been rejected. If you rejected it by mistake, click the Try Again button to try to sign in again.";
	    public const string TryAgain = "Try Again";
	    public const string SignIn = "Sign In";
		public const string TwoFactorSelectMethod = "Select Authentication Method:";
	    public const string MultifactorSendToDevice = "3. Approve push request on your mobile device:";
	    public const string ResendTimer = "You will be able to re-send the code after";
	    public const string SendAgain = "Send again";
	    public const string SendRequestToDevice = "Send request to device";
	    public const string RememberDevice = "Do not request confirmation on this device again";
		public const string MoreSignInOptionsForTenant = "More sign-in options for tenant";
		public const string EnterTenantsName = "Enter a tenant name";
		public const string TenantName = "Tenant name";
		public const string Tenant = "Tenant";
		public const string SelectTenant = "Select a tenant";
		public const string SignInWithLabel = "Select a sign-in option";
		public const string EnterCredentials = "Enter credentials";
		public const string NoSignInMethodsForThisTenant = "There are no sign-in options for this tenant";
		public const string Next = "Next";
		public const string Cancel = "Cancel";
		public const string Username = "Username";
		public const string Password = "Password";
		public const string CredentialsRecovery = "Credentials recovery";

		public const string TrialPopupMessage = "By clicking the \"Agree\" button below I acknowledge and understand that I am accessing an unlicensed tenant and such access and use is not for Production as defined by the Acumatica End-User License Agreement (\"EULA\") or the Acumatica Subscription SaaS Agreement (\"SaaS Agreement\"). Any use of this tenant for Production purposes is not authorized under the EULA or the SaaS Agreement.";
		public const string TrialPopupTitle = "Agree to Proceed";
		public const string TrialPopupAgreeButtonLabel = "Agree";
		public const string TrialPopupDisagreeButtonLabel = "Disagree";
	}

	[PX.Common.PXLocalizable]
    public static class MobileAuthScreen
    {
        public const string AuthenticationFailed = "Authentication failed.";
        public const string AuthenticationSuccessfull = "You have succesfully authenticated.";
    }

	[PX.Common.PXLocalizable]
	public static class SearchBox
	{
		public const string TypeYourQueryHere = "Type your query here";
		public const string SearchSystem = "Search in system.";
	}

	[PX.Common.PXLocalizable]
	public static class PageTitle
	{
		public const string Reminder = "Reminder";
		public const string ttipReminder = "Shows Tasks And Events";
		public const string UserID = "Username";
		public const string ScreenID = "Screen ID";
		public const string TimeZone = "Time Zone";
		public const string TargetFrame = "Target Frame :";
		public const string Title = "Title :";
		public const string Shortcut = "Shortcut :";
		public const string MaxLines = "Max. Lines :";
		public const string MaxPoints = "Max. Points :";
		public const string ChartType = "Chart type";
		public const string Line = "Line";
		public const string Bar = "Bar";
		public const string Pie = "Pie";
		public const string Version = "Build {0}";
		public const string Copyright2 = "Acumatica and Acumatica Logos are trademarks of Acumatica, Inc.";
		public const string InstallationID = "Installation ID:<br /> {0}";
		public const string Customization = "Customization: ";
		public const string Updates = "New version of Acumatica is available.";
		public const string Tools = "Tools";
		public const string ToolsTip = "View Tools";
	}

	[PX.Common.PXLocalizable]
	public static class TasksAndEventsPanel
	{
		public const string Tasks = "Tasks";
		public const string Events = "Events";
        public const string Emails = "Emails";
	}

	[PX.Common.PXLocalizable]
	public static class Messages
	{
		public const string CustomFilesCaption = "Custom Files";
	}
}
