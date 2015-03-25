/*
 * HFM.NET - User Preferences Form
 * Copyright (C) 2006-2007 David Rawling
 * Copyright (C) 2009-2015 Ryan Harlamert (harlam357)
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; version 2
 * of the License. See the included file GPLv2.TXT.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

using Castle.Core.Logging;

using harlam357.Windows.Forms;

using HFM.Core;
using HFM.Forms.Models;
using HFM.Forms.Controls;

namespace HFM.Forms
{
   public interface IPreferencesView
   {
      DialogResult ShowDialog(IWin32Window owner);
   }

   public partial class PreferencesDialog : FormWrapper, IPreferencesView
   {
      /// <summary>
      /// Tab Name Enumeration (maintain in same order as tab pages)
      /// </summary>
      private enum TabName
      {
         ScheduledTasks,
         StartupAndExternal,
         Options,
         Reporting,
         WebSettings,
         WebVisualStyles
      }
   
      #region Fields

      private const string XsltExt = "xslt";
      private const string XsltFilter = "XML Transform (*.xslt;*.xsl)|*.xslt;*.xsl";
      private const string HfmExt = "hfmx";
      private const string HfmFilter = "HFM Configuration Files|*.hfmx";
      private const string ExeExt = "exe";
      private const string ExeFilter = "Program Files|*.exe";
      
      private readonly IPreferenceSet _prefs;
      private readonly IAutoRun _autoRun;
      
      private readonly List<IValidatingControl>[] _validatingControls;
      private readonly PropertyDescriptorCollection[] _propertyCollection;
      private readonly object[] _models;
      
      private readonly WebBrowser _cssSampleBrowser;

      private ILogger _logger;

      public ILogger Logger
      {
         [CoverageExclude]
         get { return _logger ?? (_logger = NullLogger.Instance); }
         [CoverageExclude]
         set { _logger = value; }
      }

      /// <summary>
      /// Network Operations Interface
      /// </summary>
      private NetworkOps _net;

      private readonly ScheduledTasksModel _scheduledTasksModel;
      private readonly StartupAndExternalModel _startupAndExternalModel;
      private readonly OptionsModel _optionsModel;
      private readonly ReportingModel _reportingModel;
      private readonly WebSettingsModel _webSettingsModel;
      private readonly WebVisualStylesModel _webVisualStylesModel;
      
      #endregion

      #region Constructor And Binding/Load Methods

      public PreferencesDialog(IPreferenceSet prefs, IAutoRun autoRun)
      {
         if (prefs == null) throw new ArgumentNullException("prefs");
         if (autoRun == null) throw new ArgumentNullException("autoRun");

         _prefs = prefs;
         _autoRun = autoRun;
      
         InitializeComponent();

         udDecimalPlaces.Minimum = 0;
         udDecimalPlaces.Maximum = Constants.MaxDecimalPlaces;

         _validatingControls = new List<IValidatingControl>[tabControl1.TabCount];
         _propertyCollection = new PropertyDescriptorCollection[tabControl1.TabCount];
         _models = new object[tabControl1.TabCount];
         if (!Core.Application.IsRunningOnMono)
         {
            _cssSampleBrowser = new WebBrowser();

            pnl1CSSSample.Controls.Add(_cssSampleBrowser);

            _cssSampleBrowser.Dock = DockStyle.Fill;
            _cssSampleBrowser.Location = new Point(0, 0);
            _cssSampleBrowser.MinimumSize = new Size(20, 20);
            _cssSampleBrowser.Name = "_cssSampleBrowser";
            _cssSampleBrowser.Size = new Size(354, 208);
            _cssSampleBrowser.TabIndex = 0;
            _cssSampleBrowser.TabStop = false;
         }

         txtCollectMinutes.ErrorToolTipText = String.Format("Minutes must be a value from {0} to {1}.", Constants.MinMinutes, Constants.MaxMinutes);
         txtWebGenMinutes.ErrorToolTipText = String.Format("Minutes must be a value from {0} to {1}.", Constants.MinMinutes, Constants.MaxMinutes);

         _scheduledTasksModel = new ScheduledTasksModel(prefs);
         _startupAndExternalModel = new StartupAndExternalModel(prefs);
         _optionsModel = new OptionsModel(prefs);
         _reportingModel = new ReportingModel(prefs);
         _webSettingsModel = new WebSettingsModel(prefs);
         _webVisualStylesModel = new WebVisualStylesModel(prefs);
      }

      private void PreferencesDialogLoad(object sender, EventArgs e)
      {
         LoadScheduledTasksTab();
         LoadStartupTab();
         LoadOptionsTab();
         LoadReportingTab();
         LoadWebSettingsTab();
         LoadVisualStylesTab();
      
         // Cycle through Tabs to create all controls and Bind data
         for (int i = 0; i < tabControl1.TabPages.Count; i++)
         {
            tabControl1.SelectTab(i);
            if (tabControl1.SelectedIndex == (int)TabName.WebVisualStyles)
            {
               ShowCssPreview();
            }
            _validatingControls[i] = FindValidatingControls(tabControl1.SelectedTab.Controls);
         }
         tabControl1.SelectTab(0);

         _scheduledTasksModel.PropertyChanged += ScheduledTasksPropertyChanged;
         _startupAndExternalModel.PropertyChanged += StartupAndExternalPropertyChanged;
         //_optionsModel.PropertyChanged += OptionsPropertyChanged;
         _reportingModel.PropertyChanged += ReportingPropertyChanged;
         _webSettingsModel.PropertyChanged += WebSettingsChanged;
         _webVisualStylesModel.PropertyChanged += WebVisualStylesPropertyChanged;
      }
      
      private static List<IValidatingControl> FindValidatingControls(Control.ControlCollection controls)
      {
         var validatingControls = new List<IValidatingControl>();

         foreach (Control control in controls)
         {
            var validatingControl = control as IValidatingControl;
            if (validatingControl != null)
            {
               validatingControls.Add(validatingControl);
            }

            validatingControls.AddRange(FindValidatingControls(control.Controls));
         }

         return validatingControls;
      }

      private void ScheduledTasksPropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         SetPropertyErrorState((int)TabName.ScheduledTasks, e.PropertyName, true);
         if (Core.Application.IsRunningOnMono && this.Enabled)
         {
            HandleScheduledTasksPropertyEnabledForMono(e.PropertyName);
            HandleScheduledTasksPropertyChangedForMono(e.PropertyName);
         }
      }

      private void HandleScheduledTasksPropertyEnabledForMono(string propertyName)
      {
         switch (propertyName)
         {
            case "SyncOnSchedule":
               txtCollectMinutes.Enabled = _scheduledTasksModel.SyncOnSchedule;
               break;
            case "GenerateWeb":
               radioSchedule.Enabled = _scheduledTasksModel.GenerateWeb;
               lbl2MinutesToGen.Enabled = _scheduledTasksModel.GenerateWeb;
               radioFullRefresh.Enabled = _scheduledTasksModel.GenerateWeb;
               WebSiteTargetPathTextBox.Enabled = _scheduledTasksModel.GenerateWeb;
               chkHtml.Enabled = _scheduledTasksModel.GenerateWeb;
               chkXml.Enabled = _scheduledTasksModel.GenerateWeb;
               chkFAHlog.Enabled = _scheduledTasksModel.GenerateWeb;
               TestConnectionButton.Enabled = _scheduledTasksModel.GenerateWeb;
               WebGenTypePanel.Enabled = _scheduledTasksModel.GenerateWeb;
               break;
            case "GenerateIntervalEnabled":
               txtWebGenMinutes.Enabled = _scheduledTasksModel.GenerateIntervalEnabled;
               break;
            case "FtpModeEnabled":
               WebSiteServerTextBox.Enabled = _scheduledTasksModel.FtpModeEnabled;
               WebSiteServerLabel.Enabled = _scheduledTasksModel.FtpModeEnabled;
               WebSitePortTextBox.Enabled = _scheduledTasksModel.FtpModeEnabled;
               WebSitePortLabel.Enabled = _scheduledTasksModel.FtpModeEnabled;
               WebSiteUsernameTextBox.Enabled = _scheduledTasksModel.FtpModeEnabled;
               WebSiteUsernameLabel.Enabled = _scheduledTasksModel.FtpModeEnabled;
               WebSitePasswordTextBox.Enabled = _scheduledTasksModel.FtpModeEnabled;
               WebSitePasswordLabel.Enabled = _scheduledTasksModel.FtpModeEnabled;
               FtpModePanel.Enabled = _scheduledTasksModel.FtpModeEnabled;
               break;
            case "BrowseLocalPathEnabled":
               btnBrowseWebFolder.Enabled = _scheduledTasksModel.BrowseLocalPathEnabled;
               break;
            case "LimitLogSizeEnabled":
               chkLimitSize.Enabled = _scheduledTasksModel.LimitLogSizeEnabled;
               break;
            case "LimitLogSizeLengthEnabled":
               udLimitSize.Enabled = _scheduledTasksModel.LimitLogSizeLengthEnabled;
               break;
         }
      }

      private void HandleScheduledTasksPropertyChangedForMono(string propertyName)
      {
         switch (propertyName)
         {
            case "WebRoot":
               WebSiteTargetPathTextBox.Text = _scheduledTasksModel.WebRoot;
               break;
         }
      }
      
      private void StartupAndExternalPropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         if (Core.Application.IsRunningOnMono && this.Enabled)
         {
            HandleStartupAndExternalPropertyEnabledForMono(e.PropertyName);
            HandleStartupAndExternalPropertyChangedForMono(e.PropertyName);
         }
      }

      private void HandleStartupAndExternalPropertyEnabledForMono(string propertyName)
      {
         switch (propertyName)
         {
            case "UseDefaultConfigFile":
               txtDefaultConfigFile.Enabled = _startupAndExternalModel.UseDefaultConfigFile;
               btnBrowseConfigFile.Enabled = _startupAndExternalModel.UseDefaultConfigFile;
               break;
         }
      }

      private void HandleStartupAndExternalPropertyChangedForMono(string propertyName)
      {
         switch (propertyName)
         {
            case "DefaultConfigFile":
               txtDefaultConfigFile.Text = _startupAndExternalModel.DefaultConfigFile;
               break;
            case "LogFileViewer":
               txtLogFileViewer.Text = _startupAndExternalModel.LogFileViewer;
               break;
            case "FileExplorer":
               txtFileExplorer.Text = _startupAndExternalModel.FileExplorer;
               break;
         }
      }

      private void ReportingPropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         SetPropertyErrorState((int)TabName.Reporting, e.PropertyName, true);
         if (Core.Application.IsRunningOnMono && this.Enabled) HandleReportingPropertyEnabledForMono(e.PropertyName);
      }

      private void HandleReportingPropertyEnabledForMono(string propertyName)
      {
         switch (propertyName)
         {
            case "ReportingEnabled":
               chkEmailSecure.Enabled = _reportingModel.ReportingEnabled;
               btnTestEmail.Enabled = _reportingModel.ReportingEnabled;
               txtToEmailAddress.Enabled = _reportingModel.ReportingEnabled;
               txtFromEmailAddress.Enabled = _reportingModel.ReportingEnabled;
               txtSmtpServer.Enabled = _reportingModel.ReportingEnabled;
               txtSmtpServerPort.Enabled = _reportingModel.ReportingEnabled;
               txtSmtpUsername.Enabled = _reportingModel.ReportingEnabled;
               txtSmtpPassword.Enabled = _reportingModel.ReportingEnabled;
               grpReportSelections.Enabled = _reportingModel.ReportingEnabled;
               break;
         }
      }

      private void WebSettingsChanged(object sender, PropertyChangedEventArgs e)
      {
         SetPropertyErrorState((int)TabName.WebSettings, e.PropertyName, true);
         if (Core.Application.IsRunningOnMono && this.Enabled) HandleWebSettingsPropertyEnabledForMono(e.PropertyName);
      }

      private void HandleWebSettingsPropertyEnabledForMono(string propertyName)
      {
         switch (propertyName)
         {
            case "UseProxy":
               txtProxyServer.Enabled = _webSettingsModel.UseProxy;
               txtProxyPort.Enabled = _webSettingsModel.UseProxy;
               chkUseProxyAuth.Enabled = _webSettingsModel.UseProxy;
               break;
            case "ProxyAuthEnabled":
               txtProxyUser.Enabled = _webSettingsModel.ProxyAuthEnabled;
               txtProxyPass.Enabled = _webSettingsModel.ProxyAuthEnabled;
               break;
         }
      }

      private void WebVisualStylesPropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         if (Core.Application.IsRunningOnMono && this.Enabled) HandleWebVisualStylesPropertyChangedForMono(e.PropertyName);
      }

      private void HandleWebVisualStylesPropertyChangedForMono(string propertyName)
      {
         switch (propertyName)
         {
            case "WebOverview":
               txtOverview.Text = _webVisualStylesModel.WebOverview;
               break;
            case "WebMobileOverview":
               txtMobileOverview.Text = _webVisualStylesModel.WebMobileOverview;
               break;
            case "WebSummary":
               txtSummary.Text = _webVisualStylesModel.WebSummary;
               break;
            case "WebMobileSummary":
               txtMobileSummary.Text = _webVisualStylesModel.WebMobileSummary;
               break;
            case "WebSlot":
               txtInstance.Text = _webVisualStylesModel.WebSlot;
               break;
         }
      }

      private void SetPropertyErrorState()
      {
         for (int index = 0; index < _propertyCollection.Length; index++)
         {
            foreach (PropertyDescriptor property in _propertyCollection[index])
            {
               SetPropertyErrorState(index, property.DisplayName, false);
            }
         }
      }

      private void SetPropertyErrorState(int index, string boundProperty, bool showToolTip)
      {
         var errorProperty = _propertyCollection[index].Find(boundProperty + "Error", false);
         if (errorProperty != null)
         {
            SetPropertyErrorState(index, boundProperty, errorProperty, showToolTip);
         }
      }

      private void SetPropertyErrorState(int index, string boundProperty, PropertyDescriptor errorProperty, bool showToolTip)
      {
         IEnumerable<IValidatingControl> validatingControls = FindBoundControls(index, boundProperty);
         var errorState = (bool)errorProperty.GetValue(_models[index]);
         foreach (var control in validatingControls)
         {
            control.ErrorState = errorState;
            if (showToolTip) control.ShowToolTip();
         }
      }

      private IEnumerable<IValidatingControl> FindBoundControls(int index, string propertyName)
      {
         //foreach (var control in _validatingControls[index])
         //{
         //   Debug.WriteLine(control.DataBindings["Text"].BindingMemberInfo.BindingField);
         //}

         return _validatingControls[index].FindAll(x => x.DataBindings["Text"].BindingMemberInfo.BindingField == propertyName).AsReadOnly();
      }

      private void LoadScheduledTasksTab()
      {
         _propertyCollection[(int)TabName.ScheduledTasks] = TypeDescriptor.GetProperties(_scheduledTasksModel);
         _models[(int)TabName.ScheduledTasks] = _scheduledTasksModel;
      
         #region Refresh Data
         // Always Add Bindings for CheckBoxes that control input TextBoxes after
         // the data has been bound to the TextBox
         
         // Add the CheckBox.Checked => TextBox.Enabled Binding
         txtCollectMinutes.BindEnabled(_scheduledTasksModel, "SyncOnSchedule");
         // Bind the value to the TextBox
         txtCollectMinutes.BindText(_scheduledTasksModel, "SyncTimeMinutes");
         // Finally, add the CheckBox.Checked Binding
         chkScheduled.BindChecked(_scheduledTasksModel, "SyncOnSchedule");

         chkSynchronous.BindChecked(_scheduledTasksModel, "SyncOnLoad");

         chkAllowRunningAsync.BindChecked(_scheduledTasksModel, "AllowRunningAsync");
         #endregion

         #region Web Generation
         // Always Add Bindings for CheckBoxes that control input TextBoxes after
         // the data has been bound to the TextBox

         radioSchedule.BindEnabled(_scheduledTasksModel, "GenerateWeb");
         lbl2MinutesToGen.BindEnabled(_scheduledTasksModel, "GenerateWeb");
         // Bind the value to the TextBox
         txtWebGenMinutes.BindText(_scheduledTasksModel, "GenerateInterval");
         txtWebGenMinutes.DataBindings.Add("Enabled", _scheduledTasksModel, "GenerateIntervalEnabled", false, DataSourceUpdateMode.OnValidation);
         // Finally, add the RadioButton.Checked Binding
         radioFullRefresh.BindChecked(_scheduledTasksModel, "WebGenAfterRefresh");
         radioFullRefresh.BindEnabled(_scheduledTasksModel, "GenerateWeb");

         WebGenTypePanel.DataSource = _scheduledTasksModel;
         WebGenTypePanel.ValueMember = "WebGenType";
         WebGenTypePanel.BindEnabled(_scheduledTasksModel, "GenerateWeb");

         WebSiteTargetPathTextBox.BindText(_scheduledTasksModel, "WebRoot");
         WebSiteTargetPathTextBox.BindEnabled(_scheduledTasksModel, "GenerateWeb");
         WebSiteTargetPathLabel.BindEnabled(_scheduledTasksModel, "GenerateWeb");

         WebSiteServerTextBox.BindText(_scheduledTasksModel, "WebGenServer");
         WebSiteServerTextBox.BindEnabled(_scheduledTasksModel, "FtpModeEnabled");
         WebSiteServerLabel.BindEnabled(_scheduledTasksModel, "FtpModeEnabled");

         WebSitePortTextBox.BindText(_scheduledTasksModel, "WebGenPort");
         WebSitePortTextBox.BindEnabled(_scheduledTasksModel, "FtpModeEnabled");
         WebSitePortLabel.BindEnabled(_scheduledTasksModel, "FtpModeEnabled");

         WebSiteUsernameTextBox.BindText(_scheduledTasksModel, "WebGenUsername");
         WebSiteUsernameTextBox.BindEnabled(_scheduledTasksModel, "FtpModeEnabled");
         WebSiteUsernameTextBox.DataBindings.Add("ErrorToolTipText", _scheduledTasksModel, "CredentialsErrorMessage", false, DataSourceUpdateMode.OnPropertyChanged);
         WebSiteUsernameLabel.BindEnabled(_scheduledTasksModel, "FtpModeEnabled");

         WebSitePasswordTextBox.BindText(_scheduledTasksModel, "WebGenPassword");
         WebSitePasswordTextBox.BindEnabled(_scheduledTasksModel, "FtpModeEnabled");
         WebSitePasswordTextBox.DataBindings.Add("ErrorToolTipText", _scheduledTasksModel, "CredentialsErrorMessage", false, DataSourceUpdateMode.OnPropertyChanged);
         WebSitePasswordLabel.BindEnabled(_scheduledTasksModel, "FtpModeEnabled");

         chkHtml.BindChecked(_scheduledTasksModel, "CopyHtml");
         chkHtml.BindEnabled(_scheduledTasksModel, "GenerateWeb");
         chkXml.BindChecked(_scheduledTasksModel, "CopyXml");
         chkXml.BindEnabled(_scheduledTasksModel, "GenerateWeb");
         chkFAHlog.BindChecked(_scheduledTasksModel, "CopyFAHlog");
         chkFAHlog.BindEnabled(_scheduledTasksModel, "GenerateWeb");
         FtpModePanel.DataSource = _scheduledTasksModel;
         FtpModePanel.ValueMember = "FtpMode";
         FtpModePanel.BindEnabled(_scheduledTasksModel, "FtpModeEnabled");
         chkLimitSize.BindChecked(_scheduledTasksModel, "LimitLogSize");
         chkLimitSize.BindEnabled(_scheduledTasksModel, "LimitLogSizeEnabled");
         udLimitSize.DataBindings.Add("Value", _scheduledTasksModel, "LimitLogSizeLength", false, DataSourceUpdateMode.OnPropertyChanged);
         udLimitSize.BindEnabled(_scheduledTasksModel, "LimitLogSizeLengthEnabled");
         
         // Finally, add the CheckBox.Checked Binding
         TestConnectionButton.BindEnabled(_scheduledTasksModel, "GenerateWeb");
         btnBrowseWebFolder.BindEnabled(_scheduledTasksModel, "BrowseLocalPathEnabled");
         chkWebSiteGenerator.BindChecked(_scheduledTasksModel, "GenerateWeb");
         #endregion
      }
      
      private void LoadStartupTab()
      {
         _propertyCollection[(int)TabName.StartupAndExternal] = TypeDescriptor.GetProperties(_startupAndExternalModel);
         _models[(int)TabName.StartupAndExternal] = _startupAndExternalModel;
      
         #region Startup
         /*** Auto-Run Is Not DataBound ***/
         if (!Core.Application.IsRunningOnMono)
         {
            chkAutoRun.Checked = _autoRun.IsEnabled();
         }
         else
         {
            // No AutoRun under Mono
            chkAutoRun.Enabled = false;
         }
         
         chkRunMinimized.BindChecked(_startupAndExternalModel, "RunMinimized");
         chkCheckForUpdate.BindChecked(_startupAndExternalModel, "StartupCheckForUpdate");
         #endregion

         #region Configuration File
         txtDefaultConfigFile.BindEnabled(_startupAndExternalModel, "UseDefaultConfigFile");
         btnBrowseConfigFile.BindEnabled(_startupAndExternalModel, "UseDefaultConfigFile");
         txtDefaultConfigFile.BindText(_startupAndExternalModel, "DefaultConfigFile");
         
         chkDefaultConfig.BindChecked(_startupAndExternalModel, "UseDefaultConfigFile");
         #endregion

         #region External Programs
         txtLogFileViewer.BindText(_startupAndExternalModel, "LogFileViewer");
         txtFileExplorer.BindText(_startupAndExternalModel, "FileExplorer");
         #endregion
      }

      private void LoadOptionsTab()
      {
         _propertyCollection[(int)TabName.Options] = TypeDescriptor.GetProperties(_optionsModel);
         _models[(int)TabName.Options] = _optionsModel;
      
         #region Interactive Options
         chkOffline.BindChecked(_optionsModel, "OfflineLast");
         chkColorLog.BindChecked(_optionsModel, "ColorLogFile");
         chkAutoSave.BindChecked(_optionsModel, "AutoSaveConfig");
         DuplicateProjectCheckBox.BindChecked(_optionsModel, "DuplicateProjectCheck");
         DuplicateUserCheckBox.BindChecked(_optionsModel, "DuplicateUserIdCheck");
         ShowUserStatsCheckBox.BindChecked(_optionsModel, "ShowXmlStats");

         PpdCalculationComboBox.DataSource = OptionsModel.PpdCalculationList;
         PpdCalculationComboBox.DisplayMember = "DisplayMember";
         PpdCalculationComboBox.ValueMember = "ValueMember";
         PpdCalculationComboBox.DataBindings.Add("SelectedValue", _optionsModel, "PpdCalculation", false, DataSourceUpdateMode.OnPropertyChanged);
         BonusCalculationComboBox.DataSource = OptionsModel.BonusCalculationList;
         BonusCalculationComboBox.DisplayMember = "DisplayMember";
         BonusCalculationComboBox.ValueMember = "ValueMember";
         BonusCalculationComboBox.DataBindings.Add("SelectedValue", _optionsModel, "CalculateBonus", false, DataSourceUpdateMode.OnPropertyChanged);
         udDecimalPlaces.DataBindings.Add("Value", _optionsModel, "DecimalPlaces", false, DataSourceUpdateMode.OnPropertyChanged);
         chkEtaAsDate.BindChecked(_optionsModel, "EtaDate");
         #endregion

         #region Debug Message Level
         cboMessageLevel.DataSource = OptionsModel.DebugList;
         cboMessageLevel.DisplayMember = "DisplayMember";
         cboMessageLevel.ValueMember = "ValueMember";
         cboMessageLevel.DataBindings.Add("SelectedValue", _optionsModel, "MessageLevel", false, DataSourceUpdateMode.OnPropertyChanged);
         #endregion

         #region Form Docking Style
         cboShowStyle.DataSource = OptionsModel.DockingStyleList;
         cboShowStyle.DisplayMember = "DisplayMember";
         cboShowStyle.ValueMember = "ValueMember";
         cboShowStyle.DataBindings.Add("SelectedValue", _optionsModel, "FormShowStyle", false, DataSourceUpdateMode.OnPropertyChanged);
         #endregion
      }
      
      private void LoadReportingTab()
      {
         _propertyCollection[(int)TabName.Reporting] = TypeDescriptor.GetProperties(_reportingModel);
         _models[(int)TabName.Reporting] = _reportingModel;
      
         #region Email Settings
         chkEmailSecure.BindChecked(_reportingModel, "ServerSecure");
         chkEmailSecure.BindEnabled(_reportingModel, "ReportingEnabled");

         btnTestEmail.BindEnabled(_reportingModel, "ReportingEnabled");
         
         txtToEmailAddress.BindText(_reportingModel, "ToAddress");
         txtToEmailAddress.BindEnabled(_reportingModel, "ReportingEnabled");
         
         txtFromEmailAddress.BindText(_reportingModel, "FromAddress");
         txtFromEmailAddress.BindEnabled(_reportingModel, "ReportingEnabled");
         
         txtSmtpServer.BindText(_reportingModel, "ServerAddress");
         txtSmtpServer.DataBindings.Add("ErrorToolTipText", _reportingModel, "ServerPortPairErrorMessage", false, DataSourceUpdateMode.OnPropertyChanged);
         txtSmtpServer.BindEnabled(_reportingModel, "ReportingEnabled");
         
         txtSmtpServerPort.BindText(_reportingModel, "ServerPort");
         txtSmtpServerPort.DataBindings.Add("ErrorToolTipText", _reportingModel, "ServerPortPairErrorMessage", false, DataSourceUpdateMode.OnPropertyChanged);
         txtSmtpServerPort.BindEnabled(_reportingModel, "ReportingEnabled");
         
         txtSmtpUsername.BindText(_reportingModel, "ServerUsername");
         txtSmtpUsername.DataBindings.Add("ErrorToolTipText", _reportingModel, "UsernamePasswordPairErrorMessage", false, DataSourceUpdateMode.OnPropertyChanged);
         txtSmtpUsername.BindEnabled(_reportingModel, "ReportingEnabled");
         
         txtSmtpPassword.BindText(_reportingModel, "ServerPassword");
         txtSmtpPassword.DataBindings.Add("ErrorToolTipText", _reportingModel, "UsernamePasswordPairErrorMessage", false, DataSourceUpdateMode.OnPropertyChanged);
         txtSmtpPassword.BindEnabled(_reportingModel, "ReportingEnabled");

         chkEnableEmail.BindChecked(_reportingModel, "ReportingEnabled");
         #endregion
         
         #region Report Selections
         grpReportSelections.BindEnabled(_reportingModel, "ReportingEnabled");
         chkClientEuePause.BindChecked(_reportingModel, "ReportEuePause");
         chkClientHung.BindChecked(_reportingModel, "ReportHung");
         #endregion
      }

      private void LoadWebSettingsTab()
      {
         _propertyCollection[(int)TabName.WebSettings] = TypeDescriptor.GetProperties(_webSettingsModel);
         _models[(int)TabName.WebSettings] = _webSettingsModel;
      
         #region Web Statistics
         txtEOCUserID.BindText(_webSettingsModel, "EocUserId");
         txtStanfordUserID.BindText(_webSettingsModel, "StanfordId");
         txtStanfordTeamID.BindText(_webSettingsModel, "TeamId");
         #endregion
         
         #region Project Download URL
         txtProjectDownloadUrl.BindText(_webSettingsModel, "ProjectDownloadUrl");
         #endregion

         #region Web Proxy Settings
         // Always Add Bindings for CheckBoxes that control input TextBoxes after
         // the data has been bound to the TextBox
         txtProxyServer.BindText(_webSettingsModel, "ProxyServer");
         txtProxyServer.DataBindings.Add("ErrorToolTipText", _webSettingsModel, "ServerPortPairErrorMessage", false, DataSourceUpdateMode.OnPropertyChanged);
         txtProxyServer.BindEnabled(_webSettingsModel, "UseProxy");

         txtProxyPort.BindText(_webSettingsModel, "ProxyPort");
         txtProxyPort.DataBindings.Add("ErrorToolTipText", _webSettingsModel, "ServerPortPairErrorMessage", false, DataSourceUpdateMode.OnPropertyChanged);
         txtProxyPort.BindEnabled(_webSettingsModel, "UseProxy");

         // Finally, add the CheckBox.Checked Binding
         chkUseProxy.BindChecked(_webSettingsModel, "UseProxy");
         chkUseProxyAuth.BindEnabled(_webSettingsModel, "UseProxy");

         // Always Add Bindings for CheckBoxes that control input TextBoxes after
         // the data has been bound to the TextBox
         txtProxyUser.BindText(_webSettingsModel, "ProxyUser");
         txtProxyUser.DataBindings.Add("ErrorToolTipText", _webSettingsModel, "UsernamePasswordPairErrorMessage", false, DataSourceUpdateMode.OnPropertyChanged);
         txtProxyUser.BindEnabled(_webSettingsModel, "ProxyAuthEnabled");

         txtProxyPass.BindText(_webSettingsModel, "ProxyPass");
         txtProxyPass.DataBindings.Add("ErrorToolTipText", _webSettingsModel, "UsernamePasswordPairErrorMessage", false, DataSourceUpdateMode.OnPropertyChanged);
         txtProxyPass.BindEnabled(_webSettingsModel, "ProxyAuthEnabled");

         // Finally, add the CheckBox.Checked Binding
         chkUseProxyAuth.BindChecked(_webSettingsModel, "UseProxyAuth");
         #endregion
      }
      
      private void LoadVisualStylesTab()
      {
         _propertyCollection[(int)TabName.WebVisualStyles] = TypeDescriptor.GetProperties(_webVisualStylesModel);
         _models[(int)TabName.WebVisualStyles] = _webVisualStylesModel;
      
         if (Core.Application.IsRunningOnMono)
         {
            StyleList.Sorted = false;
         }
         StyleList.DataSource = _webVisualStylesModel.CssFileList;
         StyleList.DisplayMember = "DisplayMember";
         StyleList.ValueMember = "ValueMember";
         StyleList.DataBindings.Add("SelectedValue", _webVisualStylesModel, "CssFile", false, DataSourceUpdateMode.OnPropertyChanged);
         
         txtOverview.DataBindings.Add("Text", _webVisualStylesModel, "WebOverview", false, DataSourceUpdateMode.OnPropertyChanged);
         txtMobileOverview.DataBindings.Add("Text", _webVisualStylesModel, "WebMobileOverview", false, DataSourceUpdateMode.OnPropertyChanged);
         txtSummary.DataBindings.Add("Text", _webVisualStylesModel, "WebSummary", false, DataSourceUpdateMode.OnPropertyChanged);
         txtMobileSummary.DataBindings.Add("Text", _webVisualStylesModel, "WebMobileSummary", false, DataSourceUpdateMode.OnPropertyChanged);
         txtInstance.DataBindings.Add("Text", _webVisualStylesModel, "WebSlot", false, DataSourceUpdateMode.OnPropertyChanged);
      }

      private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
      {
         toolTipPrefs.RemoveAll();
         
         if (tabControl1.SelectedIndex == (int)TabName.WebVisualStyles)
         {
            ShowCssPreview();
         }
      }

      #endregion

      #region Scheduled Tasks Tab

      private void btnBrowseWebFolder_Click(object sender, EventArgs e)
      {
         if (_scheduledTasksModel.WebRoot.Length != 0)
         {
            locateWebFolder.SelectedPath = _scheduledTasksModel.WebRoot;
         }
         if (locateWebFolder.ShowDialog() == DialogResult.OK)
         {
            _scheduledTasksModel.WebRoot = locateWebFolder.SelectedPath;
         }
      }

      #endregion
      
      #region Reporting Tab

      private void txtFromEmailAddress_MouseHover(object sender, EventArgs e)
      {
         if (txtFromEmailAddress.BackColor.Equals(Color.Yellow)) return;

         toolTipPrefs.RemoveAll();
         toolTipPrefs.Show(String.Format("Depending on your SMTP server, this 'From Address' field may or may not be of consequence.{0}If you are required to enter credentials to send Email through the SMTP server, the server will{0}likely use the Email Address tied to those credentials as the sender or 'From Address'.{0}Regardless of this limitation, a valid Email Address must still be specified here.", Environment.NewLine), 
            txtFromEmailAddress.Parent, txtFromEmailAddress.Location.X + 5, txtFromEmailAddress.Location.Y - 55, 10000);
      }

      private void btnTestEmail_Click(object sender, EventArgs e)
      {
         if (_reportingModel.Error)
         {
            MessageBox.Show(this, "Please correct error conditions before sending a Test Email.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
         }
         else
         {
            try
            {
               NetworkOps.SendEmail(chkEmailSecure.Checked, txtFromEmailAddress.Text, txtToEmailAddress.Text, "HFM.NET - Test Email",
                  "HFM.NET - Test Email", txtSmtpServer.Text, int.Parse(txtSmtpServerPort.Text), txtSmtpUsername.Text, txtSmtpPassword.Text);
               MessageBox.Show(this, "Test Email sent successfully.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
               _logger.WarnFormat(ex, "{0}", ex.Message);
               MessageBox.Show(this, String.Format("Test Email failed to send.  Please check your Email settings.{0}{0}Error: {1}", Environment.NewLine, ex.Message), 
                  Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
         }
      }

      private void grpReportSelections_EnabledChanged(object sender, EventArgs e)
      {
         foreach (Control ctrl in grpReportSelections.Controls)
         {
            if (ctrl is CheckBox)
            {
               ctrl.Enabled = grpReportSelections.Enabled;
            }
         }
      }

      #endregion

      #region Web Tab

      private void linkEOC_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
      {
         try
         {
            Process.Start(String.Concat(Constants.EOCUserBaseUrl, txtEOCUserID.Text));
         }
         catch (Exception ex)
         {
            _logger.ErrorFormat(ex, "{0}", ex.Message);
            MessageBox.Show(String.Format(CultureInfo.CurrentCulture, Properties.Resources.ProcessStartError, "EOC User Stats page"));
         }
      }

      private void linkStanford_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
      {
         try
         {
            Process.Start(String.Concat(Constants.StanfordBaseUrl, txtStanfordUserID.Text));
         }
         catch (Exception ex)
         {
            _logger.ErrorFormat(ex, "{0}", ex.Message);
            MessageBox.Show(String.Format(CultureInfo.CurrentCulture, Properties.Resources.ProcessStartError, "Stanford User Stats page"));
         }
      }

      private void linkTeam_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
      {
         try
         {
            Process.Start(String.Concat(Constants.EOCTeamBaseUrl, txtStanfordTeamID.Text));
         }
         catch (Exception ex)
         {
            _logger.ErrorFormat(ex, "{0}", ex.Message);
            MessageBox.Show(String.Format(CultureInfo.CurrentCulture, Properties.Resources.ProcessStartError, "EOC Team Stats page"));
         }
      }

      #endregion

      #region Visual Style Tab

      private void StyleList_SelectedIndexChanged(object sender, EventArgs e)
      {
         ShowCssPreview();
      }

      private void ShowCssPreview()
      {
         if (Core.Application.IsRunningOnMono) return;
         
         string sStylesheet = Path.Combine(Path.Combine(_prefs.ApplicationPath, Constants.CssFolderName), _webVisualStylesModel.CssFile);
         var sb = new StringBuilder();

         sb.Append("<HTML><HEAD><TITLE>Test CSS File</TITLE>");
         sb.Append("<LINK REL=\"Stylesheet\" TYPE=\"text/css\" href=\"file://" + sStylesheet + "\" />");
         sb.Append("</HEAD><BODY>");

         sb.Append("<table class=\"Instance\">");
         sb.Append("<tr>");
         sb.Append("<td class=\"Heading\">Heading</td>");
         sb.Append("<td class=\"Blank\" width=\"100%\"></td>");
         sb.Append("</tr>");
         sb.Append("<tr>");
         sb.Append("<td class=\"LeftCol\">Left Col</td>");
         sb.Append("<td class=\"RightCol\">Right Column</td>");
         sb.Append("</tr>");
         sb.Append("<tr>");
         sb.Append("<td class=\"AltLeftCol\">Left Col</td>");
         sb.Append("<td class=\"AltRightCol\">Right Column</td>");
         sb.Append("</tr>");
         sb.Append("<tr>");
         sb.Append("<td class=\"LeftCol\">Left Col</td>");
         sb.Append("<td class=\"RightCol\">Right Column</td>");
         sb.Append("</tr>");
         sb.Append("<tr>");
         sb.Append("<td class=\"AltLeftCol\">Left Col</td>");
         sb.Append("<td class=\"AltRightCol\">Right Column</td>");
         sb.Append("</tr>");
         sb.Append("<tr>");
         sb.Append(String.Format("<td class=\"Plain\" colspan=\"2\" align=\"center\">Last updated {0} at {1}</td>", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString()));
         sb.Append("</tr>");
         sb.Append("</table>");
         sb.Append("</BODY></HTML>");

         _cssSampleBrowser.DocumentText = sb.ToString();
      }

      #endregion

      #region Button Click Event Handlers

      private delegate void FtpCheckConnectionDelegate(string server, int port, string path, string username, string password, FtpType ftpMode);

      private void TestConnectionButtonClick(object sender, EventArgs e)
      {
         if (_net == null)
         {
            _net = new NetworkOps(_prefs);
         }

         try
         {
            SetWaitCursor();
            if (!_scheduledTasksModel.FtpModeEnabled)
            {
               Action<string> del = CheckFileConnection;
               del.BeginInvoke(WebSiteTargetPathTextBox.Text, CheckFileConnectionCallback, del);
            }
            else
            {
               string path = _scheduledTasksModel.WebRoot;
               string server = _scheduledTasksModel.WebGenServer;
               int port = _scheduledTasksModel.WebGenPort;
               string username = _scheduledTasksModel.WebGenUsername;
               string password = _scheduledTasksModel.WebGenPassword;

               FtpCheckConnectionDelegate del = _net.FtpCheckConnection;
               del.BeginInvoke(server, port, path, username, password, _scheduledTasksModel.FtpMode, FtpCheckConnectionCallback, del);
            }
         }
         catch (Exception ex)
         {
            _logger.ErrorFormat(ex, "{0}", ex.Message);
            ShowConnectionFailedMessage(ex.Message);
         }
      }
      
      public void CheckFileConnection(string directory)
      {
         if (Directory.Exists(directory) == false)
         {
            throw new IOException(String.Format(CultureInfo.CurrentCulture,
               "Folder Path '{0}' does not exist.", directory));
         }
      }

      private void CheckFileConnectionCallback(IAsyncResult result)
      {
         try
         {
            var del = (Action<string>)result.AsyncState;
            del.EndInvoke(result);
            ShowConnectionSucceededMessage();
         }
         catch (Exception ex)
         {
            _logger.ErrorFormat(ex, "{0}", ex.Message);
            ShowConnectionFailedMessage(ex.Message);
         }
         finally
         {
            SetDefaultCursor();
         }
      }

      private void FtpCheckConnectionCallback(IAsyncResult result)
      {
         try
         {
            var del = (FtpCheckConnectionDelegate)result.AsyncState;
            del.EndInvoke(result);
            ShowConnectionSucceededMessage();
         }
         catch (Exception ex)
         {
            _logger.ErrorFormat(ex, "{0}", ex.Message);
            ShowConnectionFailedMessage(ex.Message);
         }
         finally
         {
            SetDefaultCursor();
         }
      }

      private void ShowConnectionSucceededMessage()
      {
         if (InvokeRequired)
         {
            Invoke(new MethodInvoker(ShowConnectionSucceededMessage));
            return;
         }

         MessageBox.Show(this, "Test Connection Succeeded", Core.Application.NameAndVersion,
            MessageBoxButtons.OK, MessageBoxIcon.Information);
      }

      private void ShowConnectionFailedMessage(string message)
      {
         if (InvokeRequired)
         {
            Invoke(new Action<string>(ShowConnectionFailedMessage), message);
            return;
         }

         MessageBox.Show(this, String.Format(CultureInfo.CurrentCulture, "Test Connection Failed{0}{0}{1}",
            Environment.NewLine, message), Core.Application.NameAndVersion, MessageBoxButtons.OK,
               MessageBoxIcon.Error);
      }
      
      private void SetDefaultCursor()
      {
         if (InvokeRequired)
         {
            Invoke(new MethodInvoker(SetDefaultCursor));
            return;
         }
         
         Cursor = Cursors.Default;
      }
      
      private void SetWaitCursor()
      {
         if (InvokeRequired)
         {
            Invoke(new MethodInvoker(SetWaitCursor));
            return;
         }

         Cursor = Cursors.WaitCursor;
      }

      private void btnOK_Click(object sender, EventArgs e)
      {
         if (CheckForErrorConditions() == false)
         {
            SetAutoRun();
            _scheduledTasksModel.Update(_prefs);
            _startupAndExternalModel.Update(_prefs);
            _optionsModel.Update(_prefs);
            _reportingModel.Update(_prefs);
            _webSettingsModel.Update(_prefs);
            _webVisualStylesModel.Update(_prefs);
            _prefs.Save();

            DialogResult = DialogResult.OK;
            Close();
         }
      }
      
      private bool CheckForErrorConditions()
      {
         SetPropertyErrorState();
         if (_scheduledTasksModel.Error)
         {
            tabControl1.SelectedTab = tabSchdTasks;
            return true;
         }
         if (_reportingModel.Error)
         {
            tabControl1.SelectedTab = tabReporting;
            return true;
         }
         if (_webSettingsModel.Error)
         {
            tabControl1.SelectedTab = tabWeb;
            return true;
         }
         
         return false;
      }

      private void SetAutoRun()
      {
         if (Core.Application.IsRunningOnMono) return;
         
         try
         {
            _autoRun.SetFilePath(chkAutoRun.Checked ? System.Windows.Forms.Application.ExecutablePath : null);
         }
         catch (InvalidOperationException ex)
         {
            _logger.ErrorFormat(ex, "{0}", ex.Message);
            MessageBox.Show(this, "Failed to save HFM.NET Auto Run Registry Value.  Please see the Messages Windows for detailed error information.",
               Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }

      private void btnCancel_Click(object sender, EventArgs e)
      {
         _prefs.Discard();
      }

      #region Folder Browsing

      private void btnBrowseConfigFile_Click(object sender, EventArgs e)
      {
         string path = DoFolderBrowse(_startupAndExternalModel.DefaultConfigFile, HfmExt, HfmFilter);
         if (String.IsNullOrEmpty(path) == false)
         {
            _startupAndExternalModel.DefaultConfigFile = path;
         }
      }

      private void btnBrowseLogViewer_Click(object sender, EventArgs e)
      {
         string path = DoFolderBrowse(_startupAndExternalModel.LogFileViewer, ExeExt, ExeFilter);
         if (String.IsNullOrEmpty(path) == false)
         {
            _startupAndExternalModel.LogFileViewer = path;
         }
      }

      private void btnBrowseFileExplorer_Click(object sender, EventArgs e)
      {
         string path = DoFolderBrowse(_startupAndExternalModel.FileExplorer, ExeExt, ExeFilter);
         if (String.IsNullOrEmpty(path) == false)
         {
            _startupAndExternalModel.FileExplorer = path;
         }
      }

      private string DoFolderBrowse(string path, string extension, string filter)
      {
         if (String.IsNullOrEmpty(path) == false)
         {
            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
               openConfigDialog.InitialDirectory = fileInfo.DirectoryName;
               openConfigDialog.FileName = fileInfo.Name;
            }
            else
            {
               var dirInfo = new DirectoryInfo(path);
               if (dirInfo.Exists)
               {
                  openConfigDialog.InitialDirectory = dirInfo.FullName;
                  openConfigDialog.FileName = String.Empty;
               }
               else
               {
                  openConfigDialog.InitialDirectory = String.Empty;
                  openConfigDialog.FileName = String.Empty;
               }
            }
         }
         else
         {
            openConfigDialog.InitialDirectory = String.Empty;
            openConfigDialog.FileName = String.Empty;
         }

         openConfigDialog.DefaultExt = extension;
         openConfigDialog.Filter = filter;
         if (openConfigDialog.ShowDialog() == DialogResult.OK)
         {
            return openConfigDialog.FileName;
         }

         return null;   
      }

      private void btnOverviewBrowse_Click(object sender, EventArgs e)
      {
         string path = DoXsltBrowse(_webVisualStylesModel.WebOverview, XsltExt, XsltFilter);
         if (String.IsNullOrEmpty(path) == false)
         {
            _webVisualStylesModel.WebOverview = path;
         }
      }

      private void btnMobileOverviewBrowse_Click(object sender, EventArgs e)
      {
         string path = DoXsltBrowse(_webVisualStylesModel.WebMobileOverview, XsltExt, XsltFilter);
         if (String.IsNullOrEmpty(path) == false)
         {
            _webVisualStylesModel.WebMobileOverview = path;
         }
      }

      private void btnSummaryBrowse_Click(object sender, EventArgs e)
      {
         string path = DoXsltBrowse(_webVisualStylesModel.WebSummary, XsltExt, XsltFilter);
         if (String.IsNullOrEmpty(path) == false)
         {
            _webVisualStylesModel.WebSummary = path;
         }
      }

      private void btnMobileSummaryBrowse_Click(object sender, EventArgs e)
      {
         string path = DoXsltBrowse(_webVisualStylesModel.WebMobileSummary, XsltExt, XsltFilter);
         if (String.IsNullOrEmpty(path) == false)
         {
            _webVisualStylesModel.WebMobileSummary = path;
         }
      }

      private void btnInstanceBrowse_Click(object sender, EventArgs e)
      {
         string path = DoXsltBrowse(_webVisualStylesModel.WebSlot, XsltExt, XsltFilter);
         if (String.IsNullOrEmpty(path) == false)
         {
            _webVisualStylesModel.WebSlot = path;
         }
      }

      private string DoXsltBrowse(string path, string extension, string filter)
      {
         if (String.IsNullOrEmpty(path) == false)
         {
            var fileInfo = new FileInfo(path);
            string xsltPath = Path.Combine(_prefs.ApplicationPath, Constants.XsltFolderName);
            
            if (fileInfo.Exists)
            {
               openConfigDialog.InitialDirectory = fileInfo.DirectoryName;
               openConfigDialog.FileName = fileInfo.Name;
            }
            else if (File.Exists(Path.Combine(xsltPath, path)))
            {
               openConfigDialog.InitialDirectory = xsltPath;
               openConfigDialog.FileName = path;
            }
            else
            {
               var dirInfo = new DirectoryInfo(path);
               if (dirInfo.Exists)
               {
                  openConfigDialog.InitialDirectory = dirInfo.FullName;
                  openConfigDialog.FileName = String.Empty;
               }
            }
         }
         else
         {
            openConfigDialog.InitialDirectory = String.Empty;
            openConfigDialog.FileName = String.Empty;
         }

         openConfigDialog.DefaultExt = extension;
         openConfigDialog.Filter = filter;
         if (openConfigDialog.ShowDialog() == DialogResult.OK)
         {
            // Check to see if the path for the file returned is the \HFM\XSL path
            if (Path.Combine(_prefs.ApplicationPath, Constants.XsltFolderName).Equals(Path.GetDirectoryName(openConfigDialog.FileName)))
            {
               // If so, return the file name only
               return Path.GetFileName(openConfigDialog.FileName);
            }

            return openConfigDialog.FileName;
         }
         
         return null;
      }

      #endregion
      
      #endregion

      #region TextBox KeyPress Event Handler (to enforce digits only)

      private void TextBoxDigitsOnlyKeyPress(object sender, KeyPressEventArgs e)
      {
         Debug.WriteLine(String.Format("Keystroke: {0}", (int)e.KeyChar));
      
         // only allow digits special keystrokes - Issue 65
         if (char.IsDigit(e.KeyChar) == false &&
               e.KeyChar != 8 &&       // backspace 
               e.KeyChar != 26 &&      // Ctrl+Z
               e.KeyChar != 24 &&      // Ctrl+X
               e.KeyChar != 3 &&       // Ctrl+C
               e.KeyChar != 22 &&      // Ctrl+V
               e.KeyChar != 25)        // Ctrl+Y
         {
            e.Handled = true;
         }
      }

      #endregion
   }
}
