# Change Log - Version 0.9.1 - [Revision 595](https://code.google.com/p/hfm-net/source/detail?r=595) #

  * Release Date: July 19, 2012

  * Change: Requires the .NET Framework 4.0 Client Profile on Windows or Mono 2.8+ (2.10 recommended) on Linux.
  * Change: HFM is now offered in a 32-bit only package.  This build has "x86" in the file name.  The only difference in this build vs. the existing distribution, now with "Any CPU" in the file name, is that the "x86" build will run as a 32-bit process on a 64-bit OS.  The builds are mutually exclusive and cannot be installed side-by-side with windows installer.  If you have a 32-bit OS then either the "Any CPU" or "x86" build will offer the same experience.

  * Fix: Memory consumption issues.
  * Fix: [Issue 234](https://code.google.com/p/hfm-net/issues/detail?id=234) - User experiencing exception on startup due to Log Viewer Splitter location.
  * Fix: [Issue 236](https://code.google.com/p/hfm-net/issues/detail?id=236) - Add a user prompt when existing HFM instance is detected but cannot be signaled.  Allow the user to start HFM anyway.
  * Fix: [Issue 255](https://code.google.com/p/hfm-net/issues/detail?id=255) - Show command line argument usage when arguments fail parsing.
  * Fix: [Issue 273](https://code.google.com/p/hfm-net/issues/detail?id=273) - Problem writing to Work Unit History Database running under Czech Culture (or possibly any other culture which uses a comma as the decimal separator).
  * Fix: [Issue 276](https://code.google.com/p/hfm-net/issues/detail?id=276) - Exception thrown setting the update timer length for user stats data.
  * Fix: [Issue 277](https://code.google.com/p/hfm-net/issues/detail?id=277) - Exception thrown accessing UnitInfo class data.
  * Fix: [Issue 279](https://code.google.com/p/hfm-net/issues/detail?id=279) - v0.9.0.548 web generation will not run on Ubuntu 12.04 + Mono 2.10.  Remove use of msxsl and C# script.  Use native XSLT v1.0 functions to format number and date/time.
  * Fix: [Issue 280](https://code.google.com/p/hfm-net/issues/detail?id=280) - Exception thrown calculating client totals.  Thanks Hayes!
  * Fix: Legacy Client Hung email notification.  Thanks Rick!

  * Enhancement: Add HFM.NET build instructions to source control.
  * Enhancement: Add Mono version detection (contributed by Shelnutt).

  * Change: Increase default connection timeout for v7 clients to 5 seconds.
  * Change: v7 log buffer size.  Now 20 million bytes (~19 MB).
  * Change: Receipt time of v7 client messages is now written to the log in Debug mode.


# HFM v7 Client API Change Log - Version 0.9.1 - [Revision 595](https://code.google.com/p/hfm-net/source/detail?r=595) #

  * Release Date: July 19, 2012

  * Change: Increase default connection timeout to 5 seconds.
  * Change: Decrease the default receive loop time from 100ms to 1ms.
  * Change: Add GetBufferChunks() method to Connection class.  Returns the buffer value in an enumerable collection of up to 8000 element char arrays.


# Change Log - Version 0.9.0 - [Revision 548](https://code.google.com/p/hfm-net/source/detail?r=548) #

  * Release Date: March 21, 2012

  * Note: User preferences will be reset by this version.  This is a one time issue and your preferences will persist with future upgrades.

  * Enhancement: [Issue 44](https://code.google.com/p/hfm-net/issues/detail?id=44) - Add a progress bar to the progress column.
  * Enhancement: Add initial Folding@Home v7 support (v7.1.48 or newer supported).  This support includes MONITORING ONLY for this release.  Later releases will incrementally add support for manipulating v7 client slots.
  * Enhancement: Add HFM.Client Tool and HFM.Log Tool applications.  These applications are data diagnostic tools for the HFM Client and Log APIs respectively.
  * Enhancement: Download Projects From Stanford will now display a results dialog detailing what projects were added or changed and what project properties were changed.
  * Enhancement: [Issue 215](https://code.google.com/p/hfm-net/issues/detail?id=215) & 229 - Allow FTP Ports to be specified for Web Generation and Legacy FTP Clients.
  * Enhancement: [Issue 263](https://code.google.com/p/hfm-net/issues/detail?id=263) - Add an option to calculate Bonus PPD based on Download Time, Frame Time, or No Bonus (None).  Hotkey is Alt+O.  The option is also in the Preferences - Options Tab.
  * Enhancement: Points Calculator - allows you to calculate project production numbers based on a given frame time.
  * Enhancement: Add Follow Log File option for log file viewer.  See the View menu.

  * Change: Remove MHz setting and MHZ and PPD/MHz columns from the main grid.
  * Change: The default "hfm" file format has been superceeded by the "hfmx" file format.  You can still open your old "hfm" files by selecting it as the file type in the open dialog.  However, please save your legacy configuration to the new format and use the new file format moving forward.  The auto-load feature will only work with "hfmx" files.
  * Change: The EOC Status Update is on it's own timer and will engage somewhere between 15-30 minutes after the update hour to allow for the update to process and to stagger all the HFM clients updating their data.
  * Change: Logging - the Messages (F7) window should now be much easier to read and understand if you have the need or want to see what HFM is doing behind the scenes.  There are now only two supported logging levels: Info and Debug.  Users should leave the level at Info unless they're trying to diagnose a problem.
  * Change: [Issue 264](https://code.google.com/p/hfm-net/issues/detail?id=264) - Web Generation - It generally looks the same but the XSLT transforms as well as the XML data format have had a major overhaul.  Customized XSLT transforms based on previous HFM versions will NOT WORK with this new version.  Use copies of the packaged XSLT files as a basis for your customizations.  Consumers of the XML data provided by HFM will also need to update their code accordingly.  The XML is much more robust and strongly typed.  There is one set of tags `<GridData>` that is intented specifically for display purposes and should not be parsed for strongly typed data.
  * Change: "External Client Data" feature has been removed for the time being.  I'll bring it back if there is demand.  Configurations of this type will be ignored when reading legacy "hfm" configuration files.
  * Change: EVERYTHING ELSE!!!  It looks the same on the surface but replumbing for v7 was quite an effort.

  * Fix: [Issue 112](https://code.google.com/p/hfm-net/issues/detail?id=112) - Issue with a large number of clients and drawing the benchmarks graphs.  You now have the option to split the clients into multiple graphs.  See the 'Graph Config' tab in the Benchmarks dialog.
  * Fix: Command Line Switches Verified Working:
    * "HFM.exe /r" will reset (delete) the user.config (settings) file for the version being run.
    * "HFM.exe /f `<filename>`" will load the specified file upon startup and override any auto-load configuration file specified in the Preferences.


# HFM v7 Client API Change Log - Version 0.9.0 - [Revision 548](https://code.google.com/p/hfm-net/source/detail?r=548) #

  * Release Date: March 21, 2012

  * Microsoft.NET API for connecting to and communicating with a Folding@Home v7 client.  This is a free and open source API available under the same GPLv2 license as HFM itself.  Developers can use this API to develop their own applications that access the Folding@Home v7 client.  Included in the package are all the necessary DLLs in addition to full API documentation in a Microsoft Compiled HTML Help (chm) file.


# Change Log - Version 0.6.2 - [Revision 336](https://code.google.com/p/hfm-net/source/detail?r=336) - Beta #

  * Release Date: March 8, 2011

  * Enhancement: [Issue 245](https://code.google.com/p/hfm-net/issues/detail?id=245) - Capture Client Core Communications error as a failed Work Unit.
  * Enhancement: HFM.Queue is now a public API.  Developers can reference HFM.Queue.dll to add queue.dat reading support to their own open source applications.
  * Enhancement: HFM now includes a queue.dat reading tool found in the .\Tools folder in the HFM install or zip file.  This executable (HFM.Queue.exe) is a clone of the qd tool.  The HFM.Queue API is based on knowledge of the queue.dat structure gleened from the qd source code (http://linuxminded.xs4all.nl/?target=software-qd-tools.plc).

Usage:

>HFM.Queue.exe -u<br>
HFM.Queue.Tool version 0.6.2.336 (qd clone)<br>
Copyright (C) 2002-2005 Richard P. Howell IV.<br>
Copyright (C) 2005-2008 Sebastiaan Couwenberg<br>
Copyright (C) 2009-2010 Ryan Harlamert<br>
<blockquote>-u      Print this usage message (and exit)<br>
-C      Only show current index<br>
-P      Print extra Project, Run, Clone, Generation line like in FAHlog.txt<br>
-a      Show all, also displays the passkey if it's set<br>
-q file Explicitly specify queue data file<br>
-v      Just print version information and stop<br></blockquote>

<ul><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=253'>Issue 253</a> - GPU3 clients reporting 99 Frames Completed to the WU History Database.<br>
</li><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=254'>Issue 254</a> - Issue writing strings with an apostrophe to the WU History Database.<br>
</li><li>Fix: Halt problems writing to the Work Unit History Database from resulting in a client being shown as 'Offline'.<br>
</li><li>Fix: Update core version parsing to comply with latest OPENMMGPU core v2.19.<br>
</li><li>Fix: (Mono) Problems with File and Path browser dialogs not showing the selected file/path in the target text box upon return from the dialog.  Thanks Shel!</li></ul>

<ul><li>Change: Add Client Type support for the GRO-A5 and UNLISTED core.<br>
</li><li>Change: (Mono) Check for a valid connection to the WU History Database on startup.  If the connection fails mark the database as having a bad connection and disable the menu item in the main UI.  Unfortunately not all Linux distributions have a compatible SQLite library installed by default.<br>
</li><li>Change: <a href='https://code.google.com/p/hfm-net/issues/detail?id=236'>Issue 236</a> - Made changes to try and thwart startup issues with the message "Single Instance Helper Failed to Start."<br>
</li><li>Change: Log lines that fail data parsing will now be shown as orange color.  This most frequently happens when starting a client and its last checkpoint was in the middle of a frame.  For a reported frame to be recognized by HFM it must be within 10% of the reported frame percentage.</li></ul>

For example:<br>
<br>
<pre><code>ex. [00:19:40] Completed 82499 out of 250000 steps  (33%) - Would Validate<br>
    [00:19:40] Completed 82750 out of 250000 steps  (33%) - Would Validate<br>
</code></pre>

10% frame step tolerance. In the example the completed must be within 250 steps.<br>
<br>
<br>
<h1>Change Log - Version 0.6.1 - <a href='https://code.google.com/p/hfm-net/source/detail?r=251'>Revision 251</a> - Beta</h1>

<ul><li>Release Date: November 4, 2010</li></ul>

<ul><li><a href='https://code.google.com/p/hfm-net/issues/detail?id=239'>Issue 239</a> - Clients Show Offline after starting up with Auto-Run.</li></ul>

<h1>Change Log - Version 0.6.0 - <a href='https://code.google.com/p/hfm-net/source/detail?r=249'>Revision 249</a> - Beta</h1>

<ul><li>Release Date: October 28, 2010</li></ul>

<ul><li>Enhancement: Added the WU History Database (Tools -> Work Unit History Viewer) to replace the CompletedUnits.csv file.  The WU History Database can import your previous CompletedUnits.csv file information through File -> Import CompletedUnits.csv (supports en-US culture only) - users with other culture settings on Windows may or may not be able to import.  The WU History Database is built on a SQLite backend and is compatible with Windows 32-bit and 64-bit environments as well as Linux (Mono) through the same installation package.  Linux (Mono) users will need to have a compatible native Linux SQLite engine installed on their machines to run HFM (SQLite v3.6.23.1 or better).  Currently includes basic query definition and sorting options as well as the ability to specify the PPD Calculation type.  Future plans include enhanced query functionality, multi-level sort, and data graphing.<br>
</li><li>Enhancement: The "Import FahMon Configuration File" option has been removed.  The functionality is now available as a Plugin.  Users need to download and place the Plugin in the proper folder to enable this functionality, then use the standard Open menu item and select "FahMon Configuration Files" from the File Type/Filter combo box of the Open File Dialog.  See the readme.txt file inside the "HFM FahMon Plugin" zip file for install instructions.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=79'>Issue 79</a> - Allow presistence of HFM local client data to file and provide an option to deploy that file during Web Generation.  Then allow HFM to merge that data into it's primary UI.  See the new Clients -> Merge Client Data option and Copy Client Data to Target option in the Preferences -> Web Generation section.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=101'>Issue 101</a> - Show Project Download Progress when executing a manual psummary download.<br>
</li><li>Enhancement: Issues 157 & 223 - Allow EOC Stats to show User or Team Stats.  Can activate the choice through a right-click context menu.  Right-click any of the status labels.  Also added an option to 'Force Refresh EOC Stats'.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=203'>Issue 203</a> - Move the Calculate Bonus Credit and PPD option to a Menu/Hotkey option that will allow users to change the style quickly (Alt+O).  Also added an option for cycling through the PPD Calculation styles (Alt+P).<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=222'>Issue 222</a> - Send email notifications on Hung status.</li></ul>

<ul><li>Change: Clients Menu now reacts accordingly and shows/hides the 'View Cached Log File' and 'View Client Files' menu options like the right-click context menu does.<br>
</li><li>Change: The Data Grid column selector context menu can no longer have less than 1 visible column.</li></ul>

<ul><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=162'>Issue 162</a> - Add further validation to psummary download to counter act issues with possibly missing k factor values.<br>
</li><li>Fix: Issues 177, 190, 220 - Superseded by the WU History Database.<br>
</li><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=219'>Issue 219</a> - Error dialog received when selecting to cancel an update download.<br>
</li><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=230'>Issue 230</a> - Refresh background timer does not start after adding first client.<br>
</li><li>Fix: In cases where the EOC Xml Stats retrieval failed it halted the Refresh background timer from starting again.  I've had reports in the past that the timer loop was not restarting... this may indeed be what those users were experiencing.<br>
</li><li>Fix: Bug with FahMon import plugin when attempting to import FTP client types.</li></ul>

<h1>Change Log - Version: 0.5.1 - <a href='https://code.google.com/p/hfm-net/source/detail?r=198'>Revision 198</a> - Beta</h1>

<ul><li>Release Date: June 27, 2010</li></ul>

<ul><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=76'>Issue 76</a> - ETA as a Date or as a TimeSpan: Added Preferences option to display the unformatted value as a Date rather than the default TimeSpan.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=79'>Issue 79</a> - Added options for HTML and/or XML web gen upload.  You can only deselect HTML if XML is specified.  If XML is deselected and HTML is not already selected, HTML will be default to selected.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=155'>Issue 155</a> - Add capability to specify the maximum size of the FAHlog.txt files being uploaded via FTP.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=193'>Issue 193</a> - Add Client Version to Client Type Column and Combine Core and Core Version Columns: Added Toggle option (View Menu / F11) to turn this data on/off.  Also replaced the Core Version column with Core ID (hex core code).<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=206'>Issue 206</a> - Display Timezone on Generated Web Pages (after time).<br>
</li><li>Enhancement:  <a href='https://code.google.com/p/hfm-net/issues/detail?id=76'>Issue 76</a> /193 - Update XML and XSLT files used for Web Generation to comply with primary grid updates (non-breaking changes - custom XSLT should continue to work fine).<br>
</li><li>Enhancement: Recognize OPENMMGPU core WUs as GPU Client Type.<br>
</li><li>Enhancement: Add Verbose Debug code showing PPD calculation values.<br>
</li><li>Enhancement: Add 'Auto Size Grid Columns' item to the View Menu.  Column Auto Size now leaves the column slightly wider than before - to give each column's text a little more "buffer" room.<br>
</li><li>Enhancement: HFM.Plugins.dll - This will be the project that will specify the third-party consumable interfaces allowing input/output plugins to be created.</li></ul>

<ul><li>Change: Folding Instance Setup and Preferences dialog backends have been mostly rewritten.<br>
</li><li>Change: Increased the relative width of the standard "Overview" web page to 32% - allow room for the new UTC offset value without word wrap.<br>
</li><li>Change: If an Add or Edit Folding Instance operation fails due to a duplicate name, show the dialog again with the same settings that had already been input... gives the user a chance to correct the duplicate without having to enter ALL the data again.</li></ul>

<ul><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=210'>Issue 210</a> - Remove smb:// as a valid URI protocol for an Http Instance.<br>
</li><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=211'>Issue 211</a> - Built on date with wrong locale on About Dialog.  Forcing to Invariant (en-US) culture until application is localized.<br>
</li><li>Fix: Turn off LogViewer Word Wrap.  If Word Wrap happens it screws up the line coloring.<br>
</li><li>Fix: Update custom grid painting code to fix some drawing abnormalities under Mono - looks much better now.</li></ul>

<h1>Change Log - Version: 0.5.0 - <a href='https://code.google.com/p/hfm-net/source/detail?r=180'>Revision 180</a> - Beta</h1>

<ul><li>Release Date: May 10, 2010</li></ul>

<ul><li>Change: HFM now REQUIRES the .NET Framework v3.5 on Windows.  Mono v2.4 (2.6 recommended) on Linux.</li></ul>

<ul><li>Change: XML Output for 'Completed', 'Failed', and now 'TotalCompleted' work units per client and for all clients are now specified as such.<br>
<ul><li>XML Templates and XLS Transforms delivered with HFM have been updated to reflect this change.  If you have created your own custom XSLT files to Transform this XML data into HTML output, I apologize for the change but this now makes similar data in the XML/XSLT files use the same tag names (and make much more sense).  You'll need to update your custom XSL Transforms.</li></ul></li></ul>

<ul><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=58'>Issue 58</a> - Add New Version Notification.  Including option to check for updates on startup and a Help Menu option to forcibly check for updates.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=59'>Issue 59</a> - Add Toggle Switch (F10) to change between 'ClientTotal' or 'CurrentClientRun' Completed Units Count.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=85'>Issue 85</a> - Add an unhandled exception reporting dialog.  Now when HFM encounters an unexpected exception the user will have easy access to exception information and a means to easily open up and post the information to the HFM Google Group.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=165'>Issue 165</a> - Move Web Generation to its own thread (when run "After Full Refresh").<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=166'>Issue 166</a> - Make the Web Summary Page respect the current Sort Column and Direction.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=167'>Issue 167</a> - Rework the About Dialog.  I like much better. :-)  Include the build date in addition to the version number.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=180'>Issue 180</a> - Restore the already running instance to the screen when starting another HFM process.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=189'>Issue 189</a> - When a client is 'RunningAsync' or 'RunningNoFrameTimes' then use the reported frame time to calculate EFT (Estimated Finishing Time) for Bonus Credit and PPD Calculations.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=192'>Issue 192</a> - Detect and Handle "+ Running on battery power" Pause Messages.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=196'>Issue 196</a> - Capture BAD_WORK_UNIT Result as Failed Work Unit.<br>
</li><li>Enhancement: Add option to maintain the selected client during a Full Refresh operation (see option in the Preferences -> Options menu).<br>
</li><li>Enhancement: Add Help Menu Items for HFM.log File and HFM.NET Data Files.</li></ul>

<ul><li>Change: Use TimePerFrame, which takes into account the benchmark frame time, when calculating EftByFrameTime.  Really a fix for an error made implementing <a href='https://code.google.com/p/hfm-net/issues/detail?id=189'>Issue 189</a>.<br>
</li><li>Change: Allow clients to report Percent Complete during Pause status.<br>
</li><li>Change: Move Show/Hide Messages Window Menu Item from Tools Menu to View Menu.<br>
</li><li>Change: Move External Application options from "Options" Tab to "Startup & External" Tab in Preferences.</li></ul>

<ul><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=176'>Issue 176</a> - HFM Crashes on Startup when it encounters a corrupt user.config file.<br>
</li><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=189'>Issue 189</a> - Unhandled Exception when saving Preferences with no EOC ID or Team Number in Web Settings.  Made each User and Team ID Web Settings Field require a value.<br>
</li><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=191'>Issue 191</a> - New ProtoMol Projects don't report frame progress on the precent boundry - thus they go cyclically out of 'Running' (Green) status.<br>
</li><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=199'>Issue 199</a> - Unhandled Exception calculating frame duration.  The UnitInfo.UnitFrames property is now a generic dictionary<br>
</li><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=201'>Issue 201</a> - Web Generation Fails when a Client with no CurrentWorkUnitLogLines is encountered.  Alter MarkupGeneratorTests to validate.  Thanks AtlasFolder!<br>
</li><li>Fix: Allow spaces in Unix style path names (Regex).<br>
</li><li>Fix: Unhandled Exception when Right-Click on Column Header when no Row is selected in the Grid.<br>
</li><li>Fix: Issue with Clients going gray (Offline) and remaining that way.  The only other fix was to delete the UnitInfoCache.dat file after shutdown of HFM.<br>
</li><li>Fix: Clean Instance Names when reading from Xml according to the same rules used for importing from a FahMon config file - for those who edit their .hfm files manually.</li></ul>

<ul><li>Add: Test Logs - ProtoMol - verify <a href='https://code.google.com/p/hfm-net/issues/detail?id=191'>Issue 191</a>.<br>
</li><li>Add: Test Logs - Stanadard - verify <a href='https://code.google.com/p/hfm-net/issues/detail?id=192'>Issue 192</a>.</li></ul>

<h1>Change Log - Version: 0.4.10 - <a href='https://code.google.com/p/hfm-net/source/detail?r=156'>Revision 156</a> - Beta</h1>

<ul><li>Release Date: March 18, 2010</li></ul>

<ul><li>Fix: Remove call to Stanford cgi-bin Project Pages.</li></ul>

<h1>Change Log - Version: 0.4.9 - <a href='https://code.google.com/p/hfm-net/source/detail?r=154'>Revision 154</a> - Beta</h1>

<ul><li>Release Date: March 11, 2010</li></ul>

<ul><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=87'>Issue 87</a> - Add FTP Mode (Passive, Active) option for FTP Instance Type.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=108'>Issue 108</a> - Add option to specify SSL connection and SMTP server port for Email Reporting.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=123'>Issue 123</a> - Add settings for specifying Custom XSLT files for Web Generation.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=123'>Issue 123</a> - The XML available to both XSLT Summary variations now includes all the data available in Overview.xml.<br>
<ul><li>Removed Summary.xml from the package.  No longer used.  Summary pages get generated from the concatenation of Overview.xml and SummaryFrag.xml.<br>
</li><li>Zip install users: you may safely delete this file after upgrading to 0.4.9.<br>
</li><li>MSI install users: the file will be automatically removed during upgrade installation.<br>
</li></ul></li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=135'>Issue 135</a> - Add option to control where HFM will be minimized - System Tray, Task Bar, or Both.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=144'>Issue 144</a> - Add Test Connection Button on Client Instance Dialog.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=145'>Issue 145</a> - Add Test Connection Button for Web Generation Target Folder/URL.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=160'>Issue 160</a> - Added Total Working Clients and Total PPD to Summary HTML Pages.<br>
</li><li>Enhancement: Time values (Download, Due, Finished) now take into consideration the user configured "Client Time Offset" value.<br>
<ul><li>This will allow users to adjust their Download Time to fix the calculated bonus multiplier which is contingent on the Download Time being correct.<br>
</li></ul></li><li>Enhancement: Added Links to Standard Overview and Summary from Mobile Versions.<br>
</li><li>Enhancement: Add HFM.NET Google Group link to Help Menu.</li></ul>

<ul><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=126'>Issue 126</a> - Use the Folding ID, Team, User ID, and Machine ID from the FAHlog data.  Use the Current Queue Entry as a backup data source only.<br>
</li><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=134'>Issue 134</a> - Duplicate Units reported in CompletedUnits.csv file.<br>
<ul><li>A unit must now become history (i.e. another unit must be downloaded and begin processing) before this data is written to the file.<br>
</li></ul></li><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=148'>Issue 148</a> - Require FTP Style URL to end in a "/" character.<br>
</li><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=149'>Issue 149</a> - Follow the selected client when sorting the grid.  (Windows only - Mono DataGridView implemenation does not work the same)<br>
</li><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=151'>Issue 151</a> - Installer now obeys the user's directory selection.<br>
</li><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=153'>Issue 153</a> - Previously completed project recognized instead.<br>
<ul><li>Capture all the Project strings encountered from beginning of WU till the log begins writing frames, not just the first.<br>
</li></ul></li><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=154'>Issue 154</a> - Benchmarks Form: set focus to Projects List Box on Load.<br>
</li><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=156'>Issue 156</a> - Bonus (KFactor) calculations are not correct for Clients/Projects with no benchmark history.<br>
</li><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=171'>Issue 171</a> - Clear the Queue data if the next read is unsuccessful.<br>
</li><li>Fix: Local Folder Web Generation was not respecting the "Copy FAHlog.txt File to Target" Preference.<br>
</li><li>Fix: When Client Names contain a dot "." the generated HTML Instance Page name is truncated after the first dot.<br>
</li><li>Fix: Bug with unitinfo.txt Due Time parsing (bad Substring Length).<br>
</li><li>Fix: Primary Grid: halt reselecting an already selected client when right-click selecting.</li></ul>

<ul><li>Change: Migrated all binary serialization to use Google Protocol Buffers format via protobuf-net.<br>
<ul><li>BenchmarkCache.dat file is copied to LegacyBenchmarkCache.dat file before conversion.<br>
</li></ul></li><li>Change: Moved Navigation Links on Summary and Instance HTML Pages to Left Justified.</li></ul>

<ul><li>Add: Third-Party Libraries Castle.Windsor and protobuf-net.  License files included but not yet included in About Dialog Credits.</li></ul>

<ul><li>Update: ProjectInfo.tab file</li></ul>

<h1>Change Log - Version: 0.4.8 - <a href='https://code.google.com/p/hfm-net/source/detail?r=121'>Revision 121</a> - Beta</h1>

<ul><li>Release Date: January 17, 2010</li></ul>

<ul><li>Fixed: <a href='https://code.google.com/p/hfm-net/issues/detail?id=142'>Issue 142</a> - Some Projects (looks like p10xxx - GPU2 and ProtoMol) write the Queue ID and MachineID values in Big Endian byte order - corrected code to compensate.<br>
</li><li>Fixed: <a href='https://code.google.com/p/hfm-net/issues/detail?id=136'>Issue 136</a> - Problem with SMP Clients reporting Hung when the Project string in the FAHlog file is crippled.</li></ul>

<ul><li>Change: Make sure the queue.dat file exists before read attempt - would like to avoid the exception overhead.</li></ul>

<ul><li>Enhancement:  <a href='https://code.google.com/p/hfm-net/issues/detail?id=119'>Issue 119</a> - Enable Scrolling on QueueControl.</li></ul>

<ul><li>Add: Framework, Log, and Queue Projects.</li></ul>

<h1>Change Log - Version: 0.4.7 - <a href='https://code.google.com/p/hfm-net/source/detail?r=104'>Revision 104</a> - Beta</h1>

<ul><li>Release Date: December 23, 2009</li></ul>

<ul><li>Fix: Detect ProtoMol work as 'Standard' Client Type.<br>
</li><li>Fix: psummary download code to handle new format (and empty column values).<br>
</li><li>Fix: Pass-Through Property in UnitInfo for Protein.NumAtoms was incorrectly pointing to Protein.Credit.<br>
<ul><li>This was causing the Protein Credit to be written to the CompletedUnits.csv file in lieu of the Number of Atoms.</li></ul></li></ul>

<ul><li>Change: Update NetworkOps Unit Tests using Mock Object Injection (using Rhino Mocks).<br>
</li><li>Change: Add StringOpsTests (100% Coverage)<br>
</li><li>Change: Clean-up UserStatsDataContainer & Tests<br>
</li><li>Change: Update BenchmarkClient Class Implementation and Add Unit Tests.</li></ul>

<ul><li>Add: GPU Instance Test 4 (Status - GettingWorkPacket).<br>
</li><li>Add: Validation Code for each psummary variant (Standard, B, & C).</li></ul>

<ul><li>Update: ProjectInfo.tab file.</li></ul>

<h1>Change Log - Version: 0.4.6 - <a href='https://code.google.com/p/hfm-net/source/detail?r=99'>Revision 99</a> - Beta</h1>

<ul><li>Release Date: December 7, 2009</li></ul>

<ul><li>Fix: Problem Determining Async Status when there is no frame time based on the user selection.<br>
</li><li>Add: Some Unit Tests for the Status Determination code (so this little bug won't slip by again).</li></ul>

<h1>Change Log - Version: 0.4.5 - <a href='https://code.google.com/p/hfm-net/source/detail?r=97'>Revision 97</a> - Beta</h1>

<ul><li>Release Date: December 6, 2009</li></ul>

<ul><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=124'>Issue 124</a> - Add option to enable/disable 'RunningAsync' detection.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=125'>Issue 125</a> - Add option to enable/disable Bonus Credit and PPD Calculation.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=129'>Issue 129</a> - Enable Bonus Calculations for Minimum and Average Frame Time Benchmarks.<br>
</li><li>Enhancement: Rework the Preferences Dialog and Underlying PreferenceSet Class now using DataBinding and new ValidatingTextBox Control found in harlam357.Windows.Forms.dll.<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=127'>Issue 127</a> & <a href='https://code.google.com/p/hfm-net/issues/detail?id=128'>Issue 128</a> - Add detection of CPU Client 'EuePause' Message and WorkUnitResult.CoreOutDataed (CORE_OUTDATED).</li></ul>

<ul><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=130'>Issue 130</a> - Project Summary Download - was using culture sensetive parsing to parse double values from the HTML, specify InvariantCulture for parsing and persistence.<br>
</li><li>Fix: Validate the read MHz value of Client Instances is 1 or greater.<br>
</li><li>Fix: Clear Project Information in the Benchmarks Dialog when the associated Protein is unavailable (not cached or available on the psummary).</li></ul>

<ul><li>Change: Add sanity check to Protein.GetBonusMultiplier() - make sure the given Unit Time is a positive TimeSpan, not just a non-Zero value.</li></ul>

<ul><li>File Addition: Adding a ProjectInfo Folder to the source tree - this folder will be used to source an externally available (and much more complete) ProjectInfo.tab file.<br>
<ul><li><a href='http://hfm-net.googlecode.com/svn/trunk/ProjectInfo/ProjectInfo.tab'>http://hfm-net.googlecode.com/svn/trunk/ProjectInfo/ProjectInfo.tab</a></li></ul></li></ul>

<h1>Change Log - Version: 0.4.2 - <a href='https://code.google.com/p/hfm-net/source/detail?r=92'>Revision 92</a> - Beta</h1>

<ul><li>Release Date: December 3, 2009</li></ul>

<ul><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=94'>Issue 94</a>  - Add logic to Determine 'RunningAsync' Status (Blue).<br>
</li><li>Enhancement: <a href='https://code.google.com/p/hfm-net/issues/detail?id=113'>Issue 113</a> - Calculate Bonus Credit and PPD for Projects that specify a positive KFactor.<br>
</li><li>Enhancement: Rework the Project Summary (psummary) download code and enable capture of the KFactor.<br>
</li><li>Enhancement: Add 'SendingWorkPacket' Status (Purple).<br>
</li><li>Enhancement: Added PPD, PPW, and WU Counts to HTML Instance Pages.<br>
</li><li>Enhancement: Add Links to the Mobile Versions of the Web Overview and Summary Pages.</li></ul>

<ul><li>Fix: Unhandled Exception generated by the ZedGraph drawing code when a Project is unknown.<br>
</li><li>Fix: <a href='https://code.google.com/p/hfm-net/issues/detail?id=114'>Issue 114</a> - Registry operations to create the Auto Run Key if it does not exist.<br>
</li><li>Fix: Auto-Run and Run Minimized options.  These options were not being saved.<br>
</li><li>Fix: Only save Messages Window Size and Location if WindowState is Normal.</li></ul>

<ul><li>Change: Parse Project Information from FAHlog.txt only if it hasn't already been populated by the Queue data.<br>
</li><li>Change: Parse the unitinfo.txt file after Parsing the FAHlog.txt file.<br>
</li><li>Change: Reordered the Project data on HTML Instance Pages to resemble the same data and order as seen in the Benchmarks UI.<br>
</li><li>Change: Don't compute Deadline (Preferred & Final) if the Project is unknown.</li></ul>

<h1>Change Log - Version: 0.4.0 - <a href='https://code.google.com/p/hfm-net/source/detail?r=86'>Revision 86</a> - Beta</h1>

<ul><li>Release Date: October 31, 2009</li></ul>

<ul><li>Enhancement: HFM.NET is now also delivered in an MSI Installer Package.  This is now the preferred method for installing HFM.NET on Windows.<br>
</li><li>Enhancement: Queue Viewer - for viewing information contained within the F@H client queue.dat file.<br>
</li><li>Enhancement: Benchmarks Upgrades<br>
<ul><li>Minimum & Avgerage Frame Time Graphs (bar colors user configurable).<br>
</li><li>Ability to remove all benchmarks associated with a specific Client ("X" next to client names).<br>
</li><li>Ability to remove all benchmarks associated with a specific Project (right-click on Projects).<br>
</li><li>Ability to refresh the Minimum Frame Time based on current cache of 300 Frame Durations (right-click on Projects).<br>
</li><li>Save Benchmark Graphs to file (png, jpeg, gif, tiff). - Note: emf file type crashes under Mono<br>
</li></ul></li><li>Enhancement: HTML Output Instance Pages - include the FAHlog.txt section for the active work unit.<br>
</li><li>Enhancement: HTML Output Instance Pages - include a link to the full FAHlog.txt file.<br>
<ul><li>Note: The option enable or disable the copy/upload of the FAHlog.txt files is available in the Preferences.<br>
</li></ul></li><li>Enhancement: Benchmarks and Messages Forms now save their Size and Position from session to session.<br>
</li><li>Enhancement: Option to run the application minimized to the system tray.</li></ul>


<ul><li>Fix: Bug causing partial cpu client frames to be detected as a full frame.<br>
</li><li>Fix: HTML Output pages are now correctly marked as utf-8 and have their DOCTYPE set to HTML 4.01 TRANSITIONAL.<br>
</li><li>Fix: (Mostly) Compatibility issues with GUI layout under 120DPI fonts (Windows only).  There are still some small alignment issues with the queue viewer show/hide functionality.<br>
</li><li>Fix: Grid Tooltip paints when entering a cell the first time, not continually. This fixes high cpu usage on Windows 7 (and probably Vista) due to continual redraw.</li></ul>


<ul><li>Change: Text benchmark output only shows production lines when the client is running the benchmark project.<br>
</li><li>Change: Reduced the number of simultaneous log retrieval threads from 64 to 20 (I believe I was starving the thread pool with too many threads).<br>
</li><li>Change: Effective Rate is no longer based on a sliding value (current Date and Time).  It is now based on the last log retrieval time for each client, yielding a fixed value between data refresh cycles.<br>
</li><li>Change: VERY IMPORTANT!!!!<br>
<ul><li>Due to UAC rules for installed applications under Vista & Windows 7 this version will move your BenchmarkCache.dat, CompletedUnits.csv, & CompletedUnits.0_2_2.csv files to your user's HFM.NET Application Data Folder. <code>*</code></li></ul></li></ul>

<blockquote><code>*</code> HFM.NET Application Data Folder (example locations)</blockquote>

<blockquote>Windows XP: C:\Documents and Settings\<code>&lt;username&gt;</code>\Application Data\HFM</blockquote>

<blockquote>Vista / Windows 7: C:\Users\<code>&lt;username&gt;</code>\AppData\Roaming\HFM</blockquote>

<h1>Change Log - Version: 0.3.0 - <a href='https://code.google.com/p/hfm-net/source/detail?r=53'>Revision 53</a> - Beta</h1>

<ul><li>Release Date: September 5, 2009</li></ul>

<ul><li>Enhancement: Log Parsing... Log Parsing... Log Parsing... a new approach, more being identified, just a huge improvement in reliability and future enhancements.<br>
</li><li>Enhancement: Add support for 'GettingWorkPacket' Status (Purple Status Color).<br>
</li><li>Enhancement: Email Reporting - Configure a reporting Email Address and SMTP Server and option to report clients detected in a 24hr. EUE Pause state (not yet tested on live data).<br>
</li><li>Enhancement: Encrypt sensetive data before storing in XML.  This includes Host Server Passwords (encrypted the next time the configuration file is saved) and the following preferences fields: Proxy Server Password, SMTP Server Password, & HTML Output Folder or FTP URL (these fields will be encrypted on the first run of HFM v0.3.0).<br>
</li><li>Enhancement: Support for Log Text Coloring.  Current Color Key: Blue="Key" Line / Green=Frame Progress Line / Red=Core and Client Shutdown / Grey=Not Identified<br>
</li><li>Enhancement: Add Auto-Run on Windows Startup option (tied to user account via reg key in HKCU).<br>
</li><li>Enhancement: Update code to write Core Name and Core Version, Average Frame Time based on all frames, and upgrade support from previous file layout for CompletedUnits.csv.<br>
</li><li>Enhancement: Indentify a Core Shutdown Result of "INTERRUPTED" and add these to the failed WU count.</li></ul>


<ul><li>Fix: Altered code to use DateTimeStyle values specifically for Mono Framework.  Mono does not appear to like the NoCurrentDateDefault style, which results in the DateTime.ParseExact method returning a DateTime equivalent to DateTime.MinValue.  This yields a TimeOfDay equal to TimeSpan.Zero, which results in no frame durations being calculated.  The bottom line... I BELIEVE THIS FIXES THE LOG PARSING ISSUES ON MONO!!!<br>
</li><li>Fix: Use local volatile list in Messages Form to hold current Debug messages in lieu of accessing the current message lines from the TextBox.  This appears to have solved the problem with messages that go "missing" from the Messages Form (most evident when running under the Mono Framework).<br>
</li><li>Fix: Unhandled exception when double-clicking a client with the Ctrl key held down - Thanks ixor!<br>
</li><li>Fix: Progress Column sorting.  The value 100% was not being sorted properly (sorting was based on text value, now based on float value) - Thanks ixor!<br>
</li><li>Fix: Benchmarks Form to use TableLayoutPanel.  Now scales properly when using Large Fonts (120 DPI) - Thanks ixor!<br>
</li><li>Fix: Project ID has to be known before any work unit data is added to the benchmarks data.  Any data added by previous versions with a Project ID of 0 will be deleted upon entering the Benchmarks Form.<br>
</li><li>Fix: Fixed a long standing bug that output an incorrect Expected Completion time on Instance web pages.<br>
</li><li>Fix: Correct spelling of "occurred" in process start error messages (ProcessStartError Resource String).<br>
</li><li>Fix: File name validation (as used by the Host Configuration Dialog).  FAHlog.txt and unitinfo.txt file names could be entered with grossly unacceptable values.</li></ul>


<ul><li>Change: Remove requirement for unitinfo.txt file to be present for log parsing to run.<br>
</li><li>Change: Update Main Form and HTML Summary to draw "Unknown" when Download Time or Deadline is DateTime.MinValue.<br>
</li><li>Change: SysTray Icon ToolTip to read "x Working Clients" and "x Non-Working Clients" to use the same nomenclature as the HTML Output.<br>
</li><li>Change (for anyone building from source): ScriptedRelease config is the only config that runs Pre or Post Build events.  It is to be used for CruiseControl.NET CI Server Builds.  The standard Debug and Release configs are available to use without any third-party dependencies (MSBuild Community Tasks, NUnit, NCover, FxCop, etc).  Set AssemblyVersion.cs Revision number to 0, only official releases should contain the Revision number.  This file will be updated only with Major.Minor.Build from now on.</li></ul>

<h1>Change Log - Version: 0.2.2 - Build 31 - Beta</h1>

<ul><li>Release Date: July 26, 2009</li></ul>

<ul><li>Fix: Issue with Save As... function not using the file name supplied by the user in the Save As... Dialog.  Was using current config file name instead... which in the case of a new configuration would result in an unhandled exception - Thanks augie!!!</li></ul>

<ul><li>Change: Messages Window can now be closed using F7 key.<br>
</li><li>Change: Now a single instance application (attempting to start HFM.exe again won't yield an exception, just a dialog telling you the app is already running).<br>
</li><li>Change: User specific UI settings like log file splitter location, column indexes, and column sort are now being saved at the time they are changed... not just on application shutdown.</li></ul>

<ul><li>Enhancement: HTML Summary now respects the 'Allways List Offline Clients Last' option.<br>
</li><li>Enhancement: Add options to turn on/off the search for Duplicate User/Machine ID and Project (R/C/G) (HTML Summary reflects these changes as well).</li></ul>

<h1>Change Log - Version: 0.2.2 - Build 30 - Beta</h1>

<ul><li>Release Date: July 23, 2009</li></ul>

<ul><li>Fix: FTP Client Type Download on Linux (Mono).<br>
</li><li>Fix: Frame calculations 'AllFrames' and 'EffectiveRate' were not persisting from session to session, would default back to 'LastThreeFrames'.<br>
</li><li>Fix: Allow special keystrokes (Ctrl+C, Ctrl+V, etc) in Numeric only textboxes.<br>
</li><li>Fix: Formatting error with links to individual clients on the standard web summary.html page (client links would not work when used with Firefox on Linux... Windows based browsers were fine)<br>
</li><li>Fix: Show NotifyIcon (sys tray icon) after Form has been shown (user reported unhandled exception when double-clicking on Icon before main Form is shown).<br>
</li><li>Fix: Current UnitInfo states now being saved correctly when leaving the current configuration (not just on shutdown).</li></ul>

<ul><li>Change: Status Color Green is Color.Green (lighter) in lieu of Color.DarkGreen.<br>
</li><li>Change: Halting lookup of unknown protein description when doing Web Generation.  Eliminates "HFM.Helpers.ProteinData.DescriptionFromURL threw exception..." messages that appear unnecessarily.<br>
</li><li>Change: Host Configuration - Validate 'Local Path' after returning from the Folder Browse dialog.<br>
</li><li>Change: Added check for EOC XML Stats 'Status' to limit the number of XML data retrievals when the stats do not change (like what usually happens at 12 noon update).<br>
</li><li>Change: Time based column alternate formatting to use leading 0 (i.e. 00min 00sec).<br>
</li><li>Change: Don't show Save Dialog when user selects a "Save" operation and no Filename and no Clients are defined.<br>
</li><li>Change: Added error dialog when Import FahMon Configuration yields no added clients.<br>
</li><li>Change: Added error dialog when Configuration Load yields no added clients.</li></ul>

<ul><li>Enhancement: Add support for finding duplicate Project (R/C/G) and User + Machine ID combinations (HTML Summary reflects these changes as well).</li></ul>

<h1>Change Log - Version: 0.2.2 - Build 28 - Beta</h1>

<ul><li>Release Date: July 4, 2009</li></ul>

<ul><li>Enhancement: Add User Stats Data Display from EOC XML along with option to enable or disable the display.<br>
</li><li>Enhancement: HTML Summary - User Name cells as Orange background when User Name does not match the User Name configured in the Preferences.<br>
</li><li>Enhancement: Added five new style sheets for more Website color options.<br>
</li><li>Enhancement: Conversation time of HTML Upload via FTP is now being logged (Info Level).</li></ul>

<ul><li>Change: Each HTML page now has a link to the HFM.NET Google Code page.  Using alternating colors on Overview page.<br>
</li><li>Change: Small tweak to the LogParser.cs when CheckForProjectID() writes the warning message "Failed to parse the Project (R/C/G) values from '{0}'" generated by SetProjectID().  There was no indication of which client log failed the parse and what value generated the warning.<br>
</li><li>Change: Protein Information on the Benchmarks Form now in text boxes (just aesthetics).  Fix the spelling of "Preferred" on the Benchmarks Form.<br>
</li><li>Change: All Forms are now being set with DoubleBuffered = true in an attempt to further alleviate UI lag issues.</li></ul>

<ul><li>Fix: Grid columns not appearing in default order on some users systems.  Manually create the columns and bind.  Allows column headers to be labeled in a more visually friendly manner (i.e. they can have spaces now).</li></ul>

<h1>Change Log - Version: 0.2.1 - Build 23 - Beta</h1>

<ul><li>Release Date: June 15, 2009</li></ul>

<ul><li>Enhancement: Major upgrade to the HTML Summary page.  Now outputs a table with the same columns as the main HFM UI including the proper status color.<br>
</li><li>Enhancement: Add Mobile Overview (mobile.html) and Mobile Summary (mobilesummary.html) Web Generation.<br>
</li><li>Enhancement: Allow website to be uploaded via FTP when a FTP style URL is input as the Target Folder (i.e. <a href='ftp://username:password@ftp.com/FolderName/'>ftp://username:password@ftp.com/FolderName/</a>).<br>
</li><li>Enhancement: Added ToolTip for the Status column that shows the text representation of the current Status.<br>
</li><li>Enhancement: Added check of Username and Team vs. the configured Username and Team (will show Orange with ToolTip if incorrect).<br>
</li><li>Enhancement: Added Preferences setting to control how many decimals are shown in PPD calculations (0 to 5 decimal places).</li></ul>

<ul><li>Change: Add the user input processor MHz as a column.<br>
</li><li>Change: Remove the Team column and Combine it with the Username column (will require you to reset your column widths).<br>
</li><li>Change: Reference the correct casing of the CSS files, and add code to make sure previous versions of HFM will still find the right CSS selection... however, to "fix" the entry in the Preferences the user will need to accept the Preferences Form after upgrading.  This change for the casing is necessary to support running on Mono in a Linux environment.</li></ul>

<ul><li>Fix: Preferences Dialog - add code to allow only digits to be input in either Minutes field, the EOC ID, Team Number, or Proxy Port field.<br>
</li><li>Fix: Bug that caused the main grid Sort Order to be lost after minimizing the main UI window.<br>
</li><li>Fix: Web Generation code now wrapped in try/catch, if the web generation would fail the timers would not be reset and thus HFM would stop refreshing the client data.</li></ul>

<ul><li>Suggested Setting (Linux): Try "xdg-open" for logfile viewer and file explorer.</li></ul>

<h1>Change Log - Version: 0.2.0 - Build 20 - Beta</h1>

<ul><li>Release Date: June 3, 2009</li></ul>

<ul><li>Fix: Only accumulate Completed and Failed counts on most recent unit parse.<br>
</li><li>Fix: Final fix for resuming from a long pause.  I think I've got it licked now.<br>
</li><li>Fix: Parsing of frame percentage from older clients like v5.02.<br>
</li><li>Fix: Parsing Frame 0 - which doesn't happen with GPUs and rarely on SMP.  However, Standard clients often report Frame 0.<br>
</li><li>Fix: Unhandled exception that occurs when viewing the benchmarks window with no clients loaded (actually no client selected).</li></ul>

<ul><li>Change: Setup frmPreferences to only show the WebBrowser control when not running under Mono Framework.<br>
</li><li>Change: Patched the main Project Resources.resx file for case sensitivity.</li></ul>

<h1>Change Log - Version: 0.2.0 - Build 18 - Beta</h1>

<ul><li>Release Date: May 30, 2009</li></ul>

<ul><li>Fix: Fixed the problem with completed units being written to the CompletedUnits.csv file with no frame time or PPD.<br>
</li><li>Fix: Fixed problem with determining client 'Status' when a client resumes from a paused state (specifically a very long pause).<br>
</li><li>Fix: Pull the last Username and Team ID string from the FAHlog.txt file, not the first.<br>
</li><li>Fix: Fixed 'AllFrames' calculation.<br>
</li><li>Fix: Wrap the FAHLogStats.NET Link process start on the About Dialog in a try/catch.</li></ul>

<ul><li>Change: Use Path.Combine and remove hard coded "\\" platform specific Path Delimiter characters.<br>
</li><li>Change: Change log file name defaults to their accurate, case-sensitive names "FAHlog.txt" and "unitinfo.txt".<br>
</li><li>Change: Minor UI tweaks to the Benchmarks Form Layout.<br>
</li><li>Change: Replace Forms based timers with System.Timers.Timer objects (no need to Invoke calls anymore).<br>
</li><li>Change: Corrected "\\" to "//" in the commented logo areas of the Overview.xml and Summary.xml files.<br>
</li><li>Change: Changed the HTML Output "Page rendered" strings to read "Page rendered by HFM.NET on...", also reformatted the xslt files.</li></ul>

<ul><li>Enhancement: Add "Auto Save Configuration" option.<br>
</li><li>Enhancement: Add support for detecting clients as Hung before they have generated a valid frame time.</li></ul>

<h1>Change Log - Version: 0.2.0 - Build 17 - Beta</h1>

<ul><li>Release Date: May 24, 2009</li></ul>

<ul><li>Enhancement: MAJOR REWORK!!!  Added support for Benchmarking Work Units.<br>
</li><li>Enhancement: Added ability to save the currently in progress UnitInfo objects on application exit.  This allows the state of the last known projects in progress to be restored the next time the application starts.<br>
</li><li>Enhancement: Added output of completed units to CompletedUnits.csv file.  Allows external analysis of completed work.  Preliminary at this point, still needs a little work I think.<br>
</li><li>Enhancement: The benchmarking changes yield the functionality to show frame times (PPD, etc) as 'AllFrames' and also 'EffectiveRate' in addition to 'LastFrame' and 'LastThreeFrames'.<br>
</li><li>Enhancement: Added option to Import FahMon style ClientsTab.txt files as a HFM Configuration (used file from v2.3.99.1 to test).</li></ul>

<ul><li>Fix: Fixed HUGE BUG in the frmMessages.UpdateMessages() routine which resulted in full UI deadlock (use BeginInvoke in lieu of Invoke).<br>
</li><li>Fix: Unit DownloadTime now handled correctly for clients marked as 'Client is on Virtual Machine'.<br>
</li><li>Fix: Fixed small bug where a DirectoryPathSeparator character was being added to the end of a PathInstance path every time the TextBox was left (added in Rev 15 - no build produced).</li></ul>

<ul><li>Change: Add StringOps.cs Class to facilitate validation and parsing of Client Instance data strings.<br>
</li><li>Change: Update ClientInstance Http and Ftp download routines (these are now verified working).<br>
</li><li>Change: Add "Wrapper" classes (that specify DoubleBuffered = true) for the most commonly used controls in an effort to further reduce UI lag.<br>
</li><li>Change: Host Instance Add/Edit Dialog got a much needed work over. Now using StringOps for input validation.<br>
</li><li>Change: Allow the user to continue with Client Input Parameters that fail validation if they choose to do so (not recommended).</li></ul>

<h1>Change Log - Version: 0.1.1 - Build 14 - Beta</h1>

<ul><li>Release Date: May 13, 2009</li></ul>

<ul><li>Fix: Corrected on error in the log parsing that was causing SMP A1 Core (and likely Standard Client Cores) to fail parsing.  Resulting in the clients status staying yellow.</li></ul>

<h1>Change Log - Version: 0.1.1 - Build 13 - Beta</h1>

<ul><li>Release Date: May 13, 2009</li></ul>

<ul><li>Fix: Set DoubleBuffering on the DataGridView to fix extremely slow painting on XP while GPU2 is active.  If this doesn't fix the problem of slow UI response, then nothing will.</li></ul>

<ul><li>Change: Switch back to using GDI for text rendering. It just looks better.</li></ul>

<h1>Change Log - Version: 0.1.1 - Build 12 - Beta</h1>

<ul><li>Release Date: May 12, 2009</li></ul>

<ul><li>Fix: Change text rendering engine to GDI+.  This fixes the extreme lag issue experienced on XP.  Note: There is still some lag when GPU2 is active, but HFM is now totally usable while also running GPU2 on XP based on my testing.</li></ul>

<ul><li>Change: Increase maximum UnitInfo.txt size to 1 Megabyte.<br>
</li><li>Change: Fix host path regex to accept fully qualified server names such as my.server.com (patch from smartcat99s).</li></ul>

<ul><li>Enhancement: Add capture of Username and Team to the main grid.<br>
</li><li>Enhancement: Allow the log file window to be sized.</li></ul>

<h1>Change Log - Version: 0.1.1 - Build 11 - Beta</h1>

<ul><li>Release Date: May 5, 2009</li></ul>

<ul><li>Fix: "No Prompt on Exit" after configuration has changed when exiting via the "X" on the main window.<br>
</li><li>Fix: Large 'Progress' values being parsed from UnitInfo.txt file.<br>
</li><li>Fix: Halt downloading of overly large UnitInfo.txt files (Local Clients Only).<br>
</li><li>Fix: Client status shows as 'Hung' (Red) when a client's log file time stamp is ahead by 1 hour.  Setting an 'Offset' value of 60 minutes will now correct this.</li></ul>

<ul><li>Change: Set default Stanford Project download to psummary.html, not psummaryC.html.<br>
</li><li>Change: Increase Client 'Offset' minimum and maximum values to +/- 720 minutes (12 hours).<br>
</li><li>Change: Messages Window Font to "Courier New" (fixed width) and made text read only.<br>
</li><li>Change: Format debug messages with message type identifier (" ", "X", "!", "-", "+").<br>
</li><li>Change: A lot of code realignment and changing of Debug Message Levels.</li></ul>

<ul><li>Enhancement: Add Debug Level for Logging/Messages as a user defined setting (Preferences -> Defaults).  Use this setting to effect the level of detail shown in the Log File and Messages Window.<br>
</li><li>Enhancement: Added support for missing FAH Cores currently not in production (GBGROMACS, QMD, GROST, DGROMACSB) and ATI & NVIDIA Development Cores.<br>
</li><li>Enhancement: Auto download new Project data when Project cannot be identified using cached Project data.<br>
</li><li>Enhancement: Only download FAHLog.txt when it has changed vs. the currently cached copy (Local Clients Only).  Can speed up retrieval cycle, especially when set to very low intervals.<br>
</li><li>Enhancement: Output 'Status' Determination data when using 'Verbose' Debug Level.  If you have clients continually showing as 'Hung' (Red) this output will help me debug the problem with the time stamps coming from the local machine vs. the hung clients.