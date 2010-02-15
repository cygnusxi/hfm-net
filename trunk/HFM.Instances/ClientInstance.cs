/*
 * HFM.NET - Client Instance Class
 * Copyright (C) 2006 David Rawling
 * Copyright (C) 2009-2010 Ryan Harlamert (harlam357)
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
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301, USA.
 */

using System;
using System.Globalization;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;

using harlam357.Security;
using harlam357.Security.Encryption;

using HFM.Framework;
using HFM.Helpers;
using HFM.Instrumentation;

namespace HFM.Instances
{
   public class ClientInstance
   {
      #region Constants
      // Xml Serialization Constants
      private const string xmlNodeInstance = "Instance";
      private const string xmlAttrName = "Name";
      private const string xmlNodeFAHLog = "FAHLogFile";
      private const string xmlNodeUnitInfo = "UnitInfoFile";
      private const string xmlNodeQueue = "QueueFile";
      private const string xmlNodeClientMHz = "ClientMHz";
      private const string xmlNodeClientVM = "ClientVM";
      private const string xmlNodeClientOffset = "ClientOffset";
      private const string xmlPropType = "HostType";
      private const string xmlPropPath = "Path";
      private const string xmlPropServ = "Server";
      private const string xmlPropUser = "Username";
      private const string xmlPropPass = "Password";
      private const string xmlPropPassiveFtpMode = "PassiveFtpMode";

      // Log Filename Constants
      public const string LocalFAHLog = "FAHlog.txt";
      public const string LocalUnitInfo = "unitinfo.txt";
      public const string LocalQueue = "queue.dat";
      
      // Default ID Constants
      public const string DefaultUserID = "";
      public const int DefaultMachineID = 0;

      // Encryption Key and Initialization Vector
      private readonly Data IV = new Data("zX!1=D,^7K@u33+d");
      private readonly Data SymmetricKey = new Data("cNx/7+,?%ubm*?j8");

      /// <summary>
      /// UnitInfo Log File Maximum Download Size.
      /// </summary>
      private const int UnitInfoMax = 1048576; // 1 Megabyte
      #endregion

      #region Events
      /// <summary>
      /// Raised when Instance Host Type is Changed
      /// </summary>
      public event EventHandler InstanceHostTypeChanged;
      /// <summary>
      /// Raised when Instance Host Type is Changed
      /// </summary>
      protected void OnInstanceHostTypeChanged(EventArgs e)
      {
         if (InstanceHostTypeChanged != null)
         {
            InstanceHostTypeChanged(this, e);
         }
      }
      #endregion
      
      #region Private Event Handlers
      /// <summary>
      /// Handles the InstanceHostTypeChanged Event
      /// </summary>
      private void ClientInstance_InstanceHostTypeChanged(object sender, EventArgs e)
      {
         InitUserSpecifiedMembers();
      }
      #endregion
      
      /// <summary>
      /// PreferenceSet Interface
      /// </summary>
      private readonly IPreferenceSet _Prefs;
      
      /// <summary>
      /// Protein Collection Interface
      /// </summary>
      private readonly IProteinCollection _proteinCollection;
      
      /// <summary>
      /// Protein Collection Interface
      /// </summary>
      private readonly IProteinBenchmarkContainer _benchmarkContainer;
      
      private readonly IDataAggregator _dataAggregator;
      /// <summary>
      /// Data Aggregator Interface
      /// </summary>
      [CLSCompliant(false)]
      public IDataAggregator DataAggregator
      {
         get { return _dataAggregator; }
      }
      
      #region Constructor
      /// <summary>
      /// Primary Constructor
      /// </summary>
      public ClientInstance(IPreferenceSet Prefs, IProteinCollection proteinCollection, IProteinBenchmarkContainer benchmarkContainer, InstanceType type)
      {
         // When Instance Host Type Changes, Clear the User Specified Values
         InstanceHostTypeChanged += ClientInstance_InstanceHostTypeChanged;

         _Prefs = Prefs;
         _proteinCollection = proteinCollection;
         _benchmarkContainer = benchmarkContainer;
         _dataAggregator = InstanceProvider.GetInstance<IDataAggregator>();
         
         // Set the Host Type
         _InstanceHostType = type;
         // Init Client Level Members
         Init();
         // Init User Specified Client Level Members
         InitUserSpecifiedMembers();
         // Create a fresh UnitInfo
         _CurrentUnitInfo = new UnitInfoLogic(_Prefs, _proteinCollection, new UnitInfo(), false, InstanceName, Path, DateTime.Now);
      }
      #endregion

      #region Client Level Members
      private ClientStatus _Status;
      /// <summary>
      /// Status of this client
      /// </summary>
      public ClientStatus Status
      {
         get { return _Status; }
         set
         {
            if (_Status != value)
            {
               _Status = value;
               //OnStatusChanged(EventArgs.Empty);
            }
         }
      }

      /// <summary>
      /// Flag denoting if Progress, Production, and Time based values are OK to Display
      /// </summary>
      public bool ProductionValuesOk
      {
         get
         {
            if (Status.Equals(ClientStatus.Running) ||
                Status.Equals(ClientStatus.RunningAsync) ||
                Status.Equals(ClientStatus.RunningNoFrameTimes))
            {
               return true;
            }

            return false;
         }
      }

      private string _Arguments;
      /// <summary>
      /// Client Startup Arguments
      /// </summary>
      public string Arguments
      {
         get { return _Arguments; }
         set { _Arguments = value; }
      }

      /// <summary>
      /// Client Path and Arguments (If Arguments Exist)
      /// </summary>
      public string ClientPathAndArguments
      {
         get
         {
            if (Arguments.Length == 0)
            {
               return Path;
            }

            return String.Format(CultureInfo.InvariantCulture, "{0} ({1})", Path, Arguments);
         }
      }

      private string _UserID;
      /// <summary>
      /// User ID associated with this client
      /// </summary>
      public string UserID
      {
         get { return _UserID; }
         set { _UserID = value; }
      }

      /// <summary>
      /// True if User ID is Unknown
      /// </summary>
      public bool UserIDUnknown
      {
         get { return UserID.Length == 0; }
      }

      private int _MachineID;
      /// <summary>
      /// Machine ID associated with this client
      /// </summary>
      public int MachineID
      {
         get { return _MachineID; }
         set { _MachineID = value; }
      }

      /// <summary>
      /// Combined User ID and Machine ID String
      /// </summary>
      public string UserAndMachineID
      {
         get { return String.Format(CultureInfo.InvariantCulture, "{0} ({1})", UserID, MachineID); }
      }

      private string _FoldingID;
      /// <summary>
      /// The Folding ID (Username) attached to this client
      /// </summary>
      public string FoldingID
      {
         get { return _FoldingID; }
         set { _FoldingID = value; }
      }

      private Int32 _Team;
      /// <summary>
      /// The Team number attached to this client
      /// </summary>
      public Int32 Team
      {
         get { return _Team; }
         set { _Team = value; }
      }

      /// <summary>
      /// Combined Folding ID and Team String
      /// </summary>
      public string FoldingIDAndTeam
      {
         get { return String.Format(CultureInfo.InvariantCulture, "{0} ({1})", FoldingID, Team); }
      }

      private Int32 _NumberOfCompletedUnitsSinceLastStart;
      /// <summary>
      /// Number of completed units since the last client start
      /// </summary>
      public Int32 NumberOfCompletedUnitsSinceLastStart
      {
         get { return _NumberOfCompletedUnitsSinceLastStart; }
         set { _NumberOfCompletedUnitsSinceLastStart = value; }
      }

      private Int32 _NumberOfFailedUnitsSinceLastStart;
      /// <summary>
      /// Number of failed units since the last client start
      /// </summary>
      public Int32 NumberOfFailedUnitsSinceLastStart
      {
         get { return _NumberOfFailedUnitsSinceLastStart; }
         set { _NumberOfFailedUnitsSinceLastStart = value; }
      }

      private Int32 _TotalUnits;
      /// <summary>
      /// Total Units Completed for lifetime of the client (read from log file)
      /// </summary>
      public Int32 TotalUnits
      {
         get { return _TotalUnits; }
         set { _TotalUnits = value; }
      }

      private UnitInfoLogic _CurrentUnitInfo;
      /// <summary>
      /// Class member containing info specific to the current work unit
      /// </summary>
      public UnitInfoLogic CurrentUnitInfoConcrete
      {
         get { return _CurrentUnitInfo; }
         protected set
         {
            UpdateTimeOfLastProgress(value);
            _CurrentUnitInfo = value;
         }
      }
      
      /// <summary>
      /// Class member containing info specific to the current work unit
      /// </summary>
      public IUnitInfoLogic CurrentUnitInfo
      {
         get { return _CurrentUnitInfo; }
      }

      /// <summary>
      /// Init Client Level Members
      /// </summary>
      private void Init()
      {
         Arguments = String.Empty;
         UserID = DefaultUserID;
         MachineID = DefaultMachineID;
         FoldingID = Constants.FoldingIDDefault;
         Team = Constants.TeamDefault;
         NumberOfCompletedUnitsSinceLastStart = 0;
         NumberOfFailedUnitsSinceLastStart = 0;
         TotalUnits = 0;
      }

      /// <summary>
      /// Return LogLine List for Specified Queue Index
      /// </summary>
      /// <param name="QueueIndex">Index in Queue</param>
      public IList<ILogLine> GetLogLinesForQueueIndex(int QueueIndex)
      {
         if (_dataAggregator.UnitLogLines != null && 
             _dataAggregator.UnitLogLines[QueueIndex] != null)
         {
            return _dataAggregator.UnitLogLines[QueueIndex];
         }

         return null;
      }
      #endregion

      #region User Specified Client Level Members
      /// <summary>
      /// Client host type (Path, FTP, or HTTP)
      /// </summary>
      private InstanceType _InstanceHostType;
      /// <summary>
      /// Client host type (Path, FTP, or HTTP)
      /// </summary>
      public InstanceType InstanceHostType
      {
         get { return _InstanceHostType; }
         set
         {
            if (_InstanceHostType != value)
            {
               _InstanceHostType = value;
               OnInstanceHostTypeChanged(EventArgs.Empty);
            }
         }
      }
      
      private string _InstanceName;
      /// <summary>
      /// The name assigned to this client instance
      /// </summary>
      public string InstanceName
      {
         get { return _InstanceName; }
         set { _InstanceName = value; }
      }

      #region Cached Log File Name Properties
      /// <summary>
      /// Cached FAHlog Filename for this instance
      /// </summary>
      public string CachedFAHLogName
      {
         get { return String.Format("{0}-{1}", InstanceName, LocalFAHLog); }
      }

      /// <summary>
      /// Cached UnitInfo Filename for this instance
      /// </summary>
      public string CachedUnitInfoName
      {
         get { return String.Format("{0}-{1}", InstanceName, LocalUnitInfo); }
      }

      /// <summary>
      /// Cached Queue Filename for this instance
      /// </summary>
      public string CachedQueueName
      {
         get { return String.Format("{0}-{1}", InstanceName, LocalQueue); }
      }
      #endregion

      private Int32 _ClientProcessorMegahertz;
      /// <summary>
      /// The number of processor megahertz for this client instance
      /// </summary>
      public Int32 ClientProcessorMegahertz
      {
         get { return _ClientProcessorMegahertz; }
         set { _ClientProcessorMegahertz = value; }
      }

      private string _RemoteFAHLogFilename;
      /// <summary>
      /// Remote client log file name
      /// </summary>
      public string RemoteFAHLogFilename
      {
         get { return _RemoteFAHLogFilename; }
         set
         {
            if (String.IsNullOrEmpty(value))
            {
               _RemoteFAHLogFilename = LocalFAHLog;
            }
            else
            {
               _RemoteFAHLogFilename = value;
            }

         }
      }

      private string _RemoteUnitInfoFilename;
      /// <summary>
      /// Remote client unit info log file name
      /// </summary>
      public string RemoteUnitInfoFilename
      {
         get { return _RemoteUnitInfoFilename; }
         set
         {
            if (String.IsNullOrEmpty(value))
            {
               _RemoteUnitInfoFilename = LocalUnitInfo;
            }
            else
            {
               _RemoteUnitInfoFilename = value;
            }
         }
      }

      private string _RemoteQueueFilename;
      /// <summary>
      /// Remote client queue.dat file name
      /// </summary>
      public string RemoteQueueFilename
      {
         get { return _RemoteQueueFilename; }
         set
         {
            if (String.IsNullOrEmpty(value))
            {
               _RemoteQueueFilename = LocalQueue;
            }
            else
            {
               _RemoteQueueFilename = value;
            }
         }
      }

      private string _Path;
      /// <summary>
      /// Location of log files for this instance
      /// </summary>
      public string Path
      {
         get { return _Path; }
         set { _Path = value; }
      }

      private string _Server;
      /// <summary>
      /// FTP Server name or IP Address
      /// </summary>
      public string Server
      {
         get { return _Server; }
         set { _Server = value; }
      }

      private string _Username;
      /// <summary>
      /// Username on remote server
      /// </summary>
      public string Username
      {
         get { return _Username; }
         set { _Username = value; }
      }

      private string _Password;
      /// <summary>
      /// Password on remote server
      /// </summary>
      public string Password
      {
         get { return _Password; }
         set { _Password = value; }
      }

      private FtpType _FtpMode;
      /// <summary>
      /// Specifies the FTP Communication Mode for this client
      /// </summary>
      public FtpType FtpMode
      {
         get { return _FtpMode; }
         set { _FtpMode = value; }
      }

      private bool _ClientIsOnVirtualMachine;
      /// <summary>
      /// Specifies that this client is on a VM that reports local time as UTC
      /// </summary>
      public bool ClientIsOnVirtualMachine
      {
         get { return _ClientIsOnVirtualMachine; }
         set { _ClientIsOnVirtualMachine = value; }
      }

      private Int32 _ClientTimeOffset;
      /// <summary>
      /// Specifies the number of minutes (+/-) this client's clock differentiates
      /// </summary>
      public Int32 ClientTimeOffset
      {
         get { return _ClientTimeOffset; }
         set { _ClientTimeOffset = value; }
      }

      /// <summary>
      /// Init User Specified Client Level Members that Define this Instance
      /// </summary>
      private void InitUserSpecifiedMembers()
      {
         InstanceName = String.Empty;
         ClientProcessorMegahertz = 1;
         RemoteFAHLogFilename = LocalFAHLog;
         RemoteUnitInfoFilename = LocalUnitInfo;
         RemoteQueueFilename = LocalQueue;

         Path = String.Empty;
         Server = String.Empty;
         Username = String.Empty;
         Password = String.Empty;

         ClientIsOnVirtualMachine = false;
         ClientTimeOffset = 0;
      }
      #endregion

      #region Unit Progress Client Level Members
      private DateTime _TimeOfLastUnitStart = DateTime.MinValue;
      /// <summary>
      /// Local Time when this Client last detected Frame Progress
      /// </summary>
      internal DateTime TimeOfLastUnitStart
      {
         get { return _TimeOfLastUnitStart; }
         set { _TimeOfLastUnitStart = value; }
      }

      private DateTime _TimeOfLastFrameProgress = DateTime.MinValue;
      /// <summary>
      /// Local Time when this Client last detected Frame Progress
      /// </summary>
      internal DateTime TimeOfLastFrameProgress
      {
         get { return _TimeOfLastFrameProgress; }
         set { _TimeOfLastFrameProgress = value; }
      } 
      #endregion

      #region CurrentUnitInfo Pass-Through Properties
      /// <summary>
      /// Frame progress of the unit
      /// </summary>
      public Int32 FramesComplete
      {
         get
         {
            if (ProductionValuesOk)
            {
               return CurrentUnitInfo.FramesComplete;
            }
            
            return 0;
         }
      }

      /// <summary>
      /// Current progress (percentage) of the unit
      /// </summary>
      public Int32 PercentComplete
      {
         get
         {
            if (ProductionValuesOk)
            {
               return CurrentUnitInfo.PercentComplete;
            }

            return 0;
         }
      }

      /// <summary>
      /// Time per frame (TPF) of the unit
      /// </summary>
      public TimeSpan TimePerFrame
      {
         get
         {
            if (ProductionValuesOk)
            {
               return CurrentUnitInfo.TimePerFrame;
            }

            return TimeSpan.Zero;
         }
      }

      /// <summary>
      /// Units per day (UPD) rating for this instance
      /// </summary>
      public Double UPD
      {
         get
         {
            if (ProductionValuesOk)
            {
               return CurrentUnitInfo.UPD;
            }

            return 0;
         }
      }

      /// <summary>
      /// Points per day (PPD) rating for this instance
      /// </summary>
      public Double PPD
      {
         get
         {
            if (ProductionValuesOk)
            {
               return CurrentUnitInfo.PPD;
            }

            return 0;
         }
      }

      /// <summary>
      /// Esimated time of arrival (ETA) for this protein
      /// </summary>
      public TimeSpan ETA
      {
         get
         {
            if (ProductionValuesOk)
            {
               return CurrentUnitInfo.ETA;
            }

            return TimeSpan.Zero;
         }
      }

      ///// <summary>
      ///// Esimated Finishing Time for this unit
      ///// </summary>
      //public TimeSpan EFT
      //{
      //   get
      //   {
      //      if (ProductionValuesOk)
      //      {
      //         return CurrentUnitInfo.EFT;
      //      }

      //      return TimeSpan.Zero;
      //   }
      //}

      internal double Credit
      {
         get
         {
            // Issue 125
            if (ProductionValuesOk && _Prefs.GetPreference<bool>(Preference.CalculateBonus))
            {
               return CurrentUnitInfo.GetBonusCredit();
            }

            return CurrentUnitInfo.Credit;
         }
      }
      #endregion

      #region Retrieval Properties
      private bool _HandleStatusOnRetrieve = true;
      /// <summary>
      /// Local flag set when retrieval acts on the status returned from parse
      /// </summary>
      public bool HandleStatusOnRetrieve
      {
         get { return _HandleStatusOnRetrieve; }
         set { _HandleStatusOnRetrieve = value; }
      }

      private volatile bool _RetrievalInProgress = false;
      /// <summary>
      /// Local flag set when log retrieval is in progress
      /// </summary>
      public bool RetrievalInProgress
      {
         get { return _RetrievalInProgress; }
         protected set 
         { 
            _RetrievalInProgress = value;
         }
      }

      private DateTime _LastRetrievalTime = DateTime.MinValue;
      /// <summary>
      /// When the log files were last successfully retrieved
      /// </summary>
      public DateTime LastRetrievalTime
      {
         get { return _LastRetrievalTime; }
         protected set
         {
            _LastRetrievalTime = value;
         }
      }
      #endregion

      #region Retrieval Methods
      /// <summary>
      /// Retrieve Instance Log Files based on Instance Type
      /// </summary>
      public void Retrieve()
      {
         // Don't allow this to fire more than once at a time
         if (RetrievalInProgress) return;

         try
         {
            RetrievalInProgress = true;

            switch (InstanceHostType)
            {
               case InstanceType.PathInstance:
                  RetrievePathInstance();
                  break;
               case InstanceType.HTTPInstance:
                  RetrieveHTTPInstance();
                  break;
               case InstanceType.FTPInstance:
                  RetrieveFTPInstance();
                  break;
               default:
                  throw new NotImplementedException(String.Format(CultureInfo.CurrentCulture,
                     "Instance Type '{0}' is not implemented", InstanceHostType));
            }

            // Re-Init Client Level Members Before Processing
            Init();
            // Process the retrieved logs
            ClientStatus returnedStatus = ProcessExisting();

            /*** Setting this flag false aids in Unit Test since the results of *
             *   determining status are relative to the current time of day. ***/
            if (HandleStatusOnRetrieve)
            {
               // Handle the status retured from the log parse
               HandleReturnedStatus(returnedStatus);
            }
            else
            {
               Status = returnedStatus;
            }
         }
         catch (Exception ex)
         {
            Status = ClientStatus.Offline;
            HfmTrace.WriteToHfmConsole(InstanceName, ex);
         }
         finally
         {
            RetrievalInProgress = false;
         }

         HfmTrace.WriteToHfmConsole(TraceLevel.Info, String.Format("{0} ({1}) Client Status: {2}", HfmTrace.FunctionName, InstanceName, Status));
      }

      /// <summary>
      /// Retrieve the log and unit info files from the configured Local path
      /// </summary>
      private void RetrievePathInstance()
      {
         DateTime Start = HfmTrace.ExecStart;

         try
         {
            FileInfo fiLog = new FileInfo(System.IO.Path.Combine(Path, RemoteFAHLogFilename));
            string FAHLog_txt = System.IO.Path.Combine(_Prefs.CacheDirectory, CachedFAHLogName);
            FileInfo fiCachedLog = new FileInfo(FAHLog_txt);

            HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, InstanceName, "FAHlog copy (start)");
            
            if (fiLog.Exists)
            {
               if (fiCachedLog.Exists == false || fiLog.Length != fiCachedLog.Length)
               {
                  fiLog.CopyTo(FAHLog_txt, true);
                  HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, InstanceName, "FAHlog copy (success)");
               }
               else
               {
                  HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, InstanceName, "FAHlog copy (file has not changed)");
               }
            }
            else
            {
               throw new FileNotFoundException(String.Format(CultureInfo.CurrentCulture, 
                  "The path {0} is inaccessible.", fiLog.FullName));
            }

            // Retrieve unitinfo.txt (or equivalent)
            FileInfo fiUI = new FileInfo(System.IO.Path.Combine(Path, RemoteUnitInfoFilename));
            string UnitInfo_txt = System.IO.Path.Combine(_Prefs.CacheDirectory, CachedUnitInfoName);

            HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, InstanceName, "unitinfo copy (start)");
            
            if (fiUI.Exists)
            {
               // If file size is too large, do not copy it and delete the current cached copy - Issue 2
               if (fiUI.Length < UnitInfoMax)
               {
                  fiUI.CopyTo(UnitInfo_txt, true);
                  HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, InstanceName, "unitinfo copy (success)");
               }
               else
               {
                  if (File.Exists(UnitInfo_txt))
                  {
                     File.Delete(UnitInfo_txt);
                  }
                  HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, String.Format(CultureInfo.CurrentCulture, 
                     "unitinfo copy (file is too big: {0} bytes).", fiUI.Length));
               }
            }
            /*** Remove Requirement for UnitInfo to be Present ***/
            else
            {
               if (File.Exists(UnitInfo_txt))
               {
                  File.Delete(UnitInfo_txt);
               }
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, String.Format(CultureInfo.CurrentCulture, 
                  "The path {0} is inaccessible.", fiUI.FullName));
            }

            // Retrieve queue.dat (or equivalent)
            FileInfo fiQueue = new FileInfo(System.IO.Path.Combine(Path, RemoteQueueFilename));
            string Queue_dat = System.IO.Path.Combine(_Prefs.CacheDirectory, CachedQueueName);

            HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, InstanceName, "queue copy (start)");
            
            if (fiQueue.Exists)
            {
               fiQueue.CopyTo(Queue_dat, true);
               HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, InstanceName, "queue copy (success)");
            }
            /*** Remove Requirement for Queue to be Present ***/
            else
            {
               if (File.Exists(Queue_dat))
               {
                  File.Delete(Queue_dat);
               }
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, String.Format(CultureInfo.CurrentCulture, 
                  "The path {0} is inaccessible.", fiQueue.FullName));
            }

            LastRetrievalTime = DateTime.Now;
         }
         finally
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Info, InstanceName, Start);
         }
      }

      /// <summary>
      /// Retrieve the log and unit info files from the configured HTTP location
      /// </summary>
      private void RetrieveHTTPInstance()
      {
         DateTime Start = HfmTrace.ExecStart;

         NetworkOps net = new NetworkOps();

         try
         {
            string HttpPath = String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", Path, "/", RemoteFAHLogFilename);
            string LocalFile = System.IO.Path.Combine(_Prefs.CacheDirectory, CachedFAHLogName);
            net.HttpDownloadHelper(HttpPath, LocalFile, Username, Password);

            try
            {
               HttpPath = String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", Path, "/", RemoteUnitInfoFilename);
               LocalFile = System.IO.Path.Combine(_Prefs.CacheDirectory, CachedUnitInfoName);

               long length = net.GetHttpDownloadLength(HttpPath, Username, Password);
               if (length < UnitInfoMax)
               {
                  net.HttpDownloadHelper(HttpPath, LocalFile, Username, Password);
               }
               else
               {
                  if (File.Exists(LocalFile))
                  {
                     File.Delete(LocalFile);
                  }

                  HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, String.Format(CultureInfo.CurrentCulture,
                     "unitinfo download (file is too big: {0} bytes).", length));
               }
            }
            /*** Remove Requirement for UnitInfo to be Present ***/
            catch (WebException ex)
            {
               if (File.Exists(LocalFile))
               {
                  File.Delete(LocalFile);
               }
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, String.Format(CultureInfo.CurrentCulture, 
                  "unitinfo download failed: {0}", ex.Message));
            }

            try
            {
               HttpPath = String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", Path, "/", RemoteQueueFilename);
               LocalFile = System.IO.Path.Combine(_Prefs.CacheDirectory, CachedQueueName);
               net.HttpDownloadHelper(HttpPath, LocalFile, Username, Password);
            }
            /*** Remove Requirement for Queue to be Present ***/
            catch (WebException ex)
            {
               if (File.Exists(LocalFile))
               {
                  File.Delete(LocalFile);
               }
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, String.Format(CultureInfo.CurrentCulture,
                  "queue download failed: {0}", ex.Message));
            }

            LastRetrievalTime = DateTime.Now;
         }
         finally
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Info, InstanceName, Start);
         }
      }

      /// <summary>
      /// Retrieve the log and unit info files from the configured FTP location
      /// </summary>
      private void RetrieveFTPInstance()
      {
         DateTime Start = HfmTrace.ExecStart;

         NetworkOps net = new NetworkOps();

         try
         {
            string LocalFilePath = System.IO.Path.Combine(_Prefs.CacheDirectory, CachedFAHLogName);
            net.FtpDownloadHelper(Server, Path, RemoteFAHLogFilename, LocalFilePath, Username, Password, FtpMode);

            try
            {
               LocalFilePath = System.IO.Path.Combine(_Prefs.CacheDirectory, CachedUnitInfoName);

               long length = net.GetFtpDownloadLength(Server, Path, RemoteUnitInfoFilename, Username, Password, FtpMode);
               if (length < UnitInfoMax)
               {
                  net.FtpDownloadHelper(Server, Path, RemoteUnitInfoFilename, LocalFilePath, Username, Password, FtpMode);
               }
               else
               {
                  if (File.Exists(LocalFilePath))
                  {
                     File.Delete(LocalFilePath);
                  }

                  HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, String.Format(CultureInfo.CurrentCulture,
                     "unitinfo download (file is too big: {0} bytes).", length));
               }
            }
            /*** Remove Requirement for UnitInfo to be Present ***/
            catch (WebException ex)
            {
               if (File.Exists(LocalFilePath))
               {
                  File.Delete(LocalFilePath);
               }
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, String.Format(CultureInfo.CurrentCulture, 
                  "unitinfo download failed: {0}.", ex.Message));
            }

            try
            {
               LocalFilePath = System.IO.Path.Combine(_Prefs.CacheDirectory, CachedQueueName);
               net.FtpDownloadHelper(Server, Path, RemoteQueueFilename, LocalFilePath, Username, Password, FtpMode);
            }
            /*** Remove Requirement for Queue to be Present ***/
            catch (WebException ex)
            {
               if (File.Exists(LocalFilePath))
               {
                  File.Delete(LocalFilePath);
               }
               HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, String.Format(CultureInfo.CurrentCulture,
                  "queue download failed: {0}", ex.Message));
            }

            LastRetrievalTime = DateTime.Now;
         }
         finally
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Info, InstanceName, Start);
         }
      }
      #endregion

      #region Queue and Log Processing Functions
      /// <summary>
      /// Process the cached log files that exist on this machine
      /// </summary>
      public ClientStatus ProcessExisting()
      {
         // Exec Start
         DateTime Start = HfmTrace.ExecStart;

         #region Setup UnitInfo Aggregator
         _dataAggregator.InstanceName = InstanceName;
         _dataAggregator.QueueFilePath = System.IO.Path.Combine(_Prefs.CacheDirectory, CachedQueueName);
         _dataAggregator.FahLogFilePath = System.IO.Path.Combine(_Prefs.CacheDirectory, CachedFAHLogName);
         _dataAggregator.UnitInfoLogFilePath = System.IO.Path.Combine(_Prefs.CacheDirectory, CachedUnitInfoName); 
         #endregion
         
         #region Run the Aggregator and Set ClientInstance Level Results
         IList<IUnitInfo> units = _dataAggregator.AggregateData();
         // Issue 126 - Use the Folding ID, Team, User ID, and Machine ID from the FAHlog data.
         // Use the Current Queue Entry as a backup data source.
         PopulateRunLevelData(_dataAggregator.CurrentClientRun);
         if (_dataAggregator.Queue != null)
         {
            PopulateRunLevelData(_dataAggregator.Queue.CurrentQueueEntry);
         }
         #endregion
         
         UnitInfoLogic[] parsedUnits = new UnitInfoLogic[units.Count];
         for (int i = 0; i < units.Count; i++)
         {
            if (units[i] != null)
            {
               parsedUnits[i] = new UnitInfoLogic(_Prefs, _proteinCollection, units[i], ClientIsOnVirtualMachine, 
                                                  InstanceName, Path, LastRetrievalTime);   
            }
         }

         // *** THIS HAS TO BE DONE BEFORE UPDATING THE CurrentUnitInfo ***
         // Update Benchmarks from parsedUnits array 
         UpdateBenchmarkData(parsedUnits, _dataAggregator.CurrentUnitIndex);

         // Update the CurrentUnitInfo if we have a Status
         ClientStatus CurrentWorkUnitStatus = _dataAggregator.CurrentWorkUnitStatus;
         if (CurrentWorkUnitStatus.Equals(ClientStatus.Unknown) == false)
         {
            CurrentUnitInfoConcrete = parsedUnits[_dataAggregator.CurrentUnitIndex];
         }

         HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, InstanceName, Start);

         // Return the Status
         return CurrentWorkUnitStatus;
      }

      private void PopulateRunLevelData(IClientRun run)
      {
         Arguments = run.Arguments;
      
         FoldingID = run.FoldingID;
         Team = run.Team;
         
         UserID = run.UserID;
         MachineID = run.MachineID;

         NumberOfCompletedUnitsSinceLastStart = run.NumberOfCompletedUnits;
         NumberOfFailedUnitsSinceLastStart = run.NumberOfFailedUnits;
         TotalUnits = run.NumberOfTotalUnitsCompleted;
      }

      private void PopulateRunLevelData(IQueueEntry queueEntry)
      {
         if (FoldingID == Constants.FoldingIDDefault)
         {
            FoldingID = queueEntry.FoldingID;
         }
         if (Team == Constants.TeamDefault)
         {
            Team = (int)queueEntry.TeamNumber;
         }
         if (UserID == DefaultUserID)
         {
            UserID = queueEntry.UserID;
         }
         if (MachineID == DefaultMachineID)
         {
            MachineID = (int)queueEntry.MachineID;
         }
      }

      /// <summary>
      /// Update Project Benchmarks
      /// </summary>
      /// <param name="parsedUnits">Parsed UnitInfo Array</param>
      /// <param name="BenchmarkUpdateIndex">Index of Current UnitInfo</param>
      private void UpdateBenchmarkData(UnitInfoLogic[] parsedUnits, int BenchmarkUpdateIndex)
      {
         bool FoundCurrent = false;

         int index = BenchmarkUpdateIndex;
         // Set index for the oldest unit in the array
         if (index == parsedUnits.Length - 1)
         {
            index = 0;
         }
         else
         {
            index++;
         }

         while (index != -1)
         {
            if (FoundCurrent == false && IsUnitInfoCurrentUnitInfo(parsedUnits[index]))
            {
               FoundCurrent = true;
            }

            if (FoundCurrent || index == BenchmarkUpdateIndex)
            {
               int previousFrameID = 0;
               // check this against the CurrentUnitInfo
               if (IsUnitInfoCurrentUnitInfo(parsedUnits[index]))
               {
                  // current frame has already been recorded, increment to the next frame
                  previousFrameID = CurrentUnitInfo.LastUnitFrameID + 1;
               }

               // Update benchmarks
               _benchmarkContainer.UpdateBenchmarkData(parsedUnits[index], previousFrameID, parsedUnits[index].LastUnitFrameID);
            }

            if (index == BenchmarkUpdateIndex)
            {
               index = -1;
            }
            else if (index == parsedUnits.Length - 1)
            {
               index = 0;
            }
            else
            {
               index++;
            }
         }
      }

      /// <summary>
      /// Update Time of Last Frame Progress based on Current and Parsed UnitInfo
      /// </summary>
      private void UpdateTimeOfLastProgress(IUnitInfoLogic parsedUnitInfo)
      {
         // Same UnitInfo (based on Project (R/C/G)
         if (IsUnitInfoCurrentUnitInfo(parsedUnitInfo))
         {
            // If the Unit Start Time Stamp is no longer the same as the CurrentUnitInfo
            if (parsedUnitInfo.UnitStartTimeStamp.Equals(TimeSpan.Zero) == false &&
                CurrentUnitInfo.UnitStartTimeStamp.Equals(TimeSpan.Zero) == false &&
                parsedUnitInfo.UnitStartTimeStamp.Equals(CurrentUnitInfo.UnitStartTimeStamp) == false)
            {
               TimeOfLastUnitStart = DateTime.Now;
            }
         
            // If the Last Unit Frame ID is greater than the CurrentUnitInfo Last Unit Frame ID
            if (parsedUnitInfo.LastUnitFrameID > CurrentUnitInfo.LastUnitFrameID)
            {
               // Update the Time Of Last Frame Progress
               TimeOfLastFrameProgress = DateTime.Now;
            }
         }
         else // Different UnitInfo - Update the Time Of Last 
              // Unit Start and Clear Frame Progress Value
         {
            TimeOfLastUnitStart = DateTime.Now;
            TimeOfLastFrameProgress = DateTime.MinValue;
         }
      }

      /// <summary>
      /// Does the given UnitInfo.ProjectRunCloneGen match the CurrentUnitInfo.ProjectRunCloneGen?
      /// </summary>
      private bool IsUnitInfoCurrentUnitInfo(IUnitInfoLogic parsedUnitInfo)
      {
         Debug.Assert(CurrentUnitInfo != null);
      
         // if the parsed Project is known
         if (parsedUnitInfo != null && parsedUnitInfo.ProjectIsUnknown == false)
         {
            // Matches the Current Project and Download Time
            if (ProjectsMatch(parsedUnitInfo, CurrentUnitInfo) &&
                parsedUnitInfo.DownloadTime.Equals(CurrentUnitInfo.DownloadTime))
            {
               return true;
            }
         }

         return false;
      }

      private static bool ProjectsMatch(IProjectInfo project1, IProjectInfo project2)
      {
         if (project1 == null || project2 == null) return false;

         return (project1.ProjectID == project2.ProjectID &&
                 project1.ProjectRun == project2.ProjectRun &&
                 project1.ProjectClone == project2.ProjectClone &&
                 project1.ProjectGen == project2.ProjectGen);
      }
      #endregion

      #region Status Handling and Determination
      
      /// <summary>
      /// Handles the Client Status Returned by Log Parsing and then determines what values to feed the DetermineStatus routine.
      /// </summary>
      /// <param name="returnedStatus">Client Status</param>
      private void HandleReturnedStatus(ClientStatus returnedStatus)
      {
         StatusData statusData = new StatusData();
         statusData.InstanceName = InstanceName;
         statusData.TypeOfClient = CurrentUnitInfo.TypeOfClient;
         statusData.LastRetrievalTime = LastRetrievalTime;
         statusData.IgnoreUtcOffset = ClientIsOnVirtualMachine;
         statusData.UtcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
         statusData.ClientTimeOffset = ClientTimeOffset;
         statusData.TimeOfLastUnitStart = TimeOfLastUnitStart;
         statusData.TimeOfLastFrameProgress = TimeOfLastFrameProgress;
         statusData.CurrentStatus = Status;
         statusData.ReturnedStatus = returnedStatus;
         statusData.FrameTime = CurrentUnitInfo.RawTimePerSection;
         statusData.AverageFrameTime = InstanceProvider.GetInstance<IProteinBenchmarkContainer>().GetBenchmarkAverageFrameTime(CurrentUnitInfo);
         statusData.TimeOfLastFrame = CurrentUnitInfo.TimeOfLastFrame;
         statusData.UnitStartTimeStamp = CurrentUnitInfo.UnitStartTimeStamp;
         statusData.AllowRunningAsync = _Prefs.GetPreference<bool>(Preference.AllowRunningAsync);
      
         Status = HandleReturnedStatus(statusData, _Prefs);
      }

      /// <summary>
      /// Handles the Client Status Returned by Log Parsing and then determines what values to feed the DetermineStatus routine.
      /// </summary>
      /// <param name="statusData">Client Status Data</param>
      /// <param name="Prefs">PreferenceSet Interface</param>
      public static ClientStatus HandleReturnedStatus(StatusData statusData, IPreferenceSet Prefs)
      {
         // If the returned status is EuePause and current status is not
         if (statusData.ReturnedStatus.Equals(ClientStatus.EuePause) && statusData.CurrentStatus.Equals(ClientStatus.EuePause) == false)
         {
            if (Prefs.GetPreference<bool>(Preference.EmailReportingEnabled) && 
                Prefs.GetPreference<bool>(Preference.ReportEuePause))
            {
               SendEuePauseEmail(statusData.InstanceName, Prefs);
            }
         }

         switch (statusData.ReturnedStatus)
         {
            case ClientStatus.Running:      // at this point, we should not see Running Status
            case ClientStatus.RunningAsync: // at this point, we should not see RunningAsync Status
            case ClientStatus.RunningNoFrameTimes:
               break;
            case ClientStatus.Unknown:
               HfmTrace.WriteToHfmConsole(TraceLevel.Error,
                                          String.Format("Unable to Determine Status for Client '{0}'", statusData.InstanceName), true);
               // Update Client Status - don't call Determine Status
               return statusData.ReturnedStatus;
            case ClientStatus.Offline:
            case ClientStatus.Stopped:
            case ClientStatus.EuePause:
            case ClientStatus.Hung:
            case ClientStatus.Paused:
            case ClientStatus.SendingWorkPacket:
            case ClientStatus.GettingWorkPacket:
               // Update Client Status - don't call Determine Status
               return statusData.ReturnedStatus;
         }

         // if we have a frame time, use it
         if (statusData.FrameTime > 0)
         {
            ClientStatus Status = DetermineStatus(statusData);
            if (Status.Equals(ClientStatus.Hung) && statusData.AllowRunningAsync) // Issue 124
            {
               return DetermineAsyncStatus(statusData);
            }
            
            return Status;
         }

         // no frame time based on the current PPD calculation selection ('LastFrame', 'LastThreeFrames', etc)
         // this section attempts to give DetermineStats values to detect Hung clients before they have a valid
         // frame time - Issue 10
         else
         {
            // if we have no time stamp
            if (statusData.TimeOfLastFrame == TimeSpan.Zero)
            {
               // use the unit start time
               statusData.TimeOfLastFrame = statusData.UnitStartTimeStamp;
            }

            statusData.FrameTime = GetBaseFrameTime(statusData.AverageFrameTime, statusData.TypeOfClient);
            if (DetermineStatus(statusData).Equals(ClientStatus.Hung))
            {
               // Issue 124
               if (statusData.AllowRunningAsync)
               {
                  if (DetermineAsyncStatus(statusData).Equals(ClientStatus.Hung))
                  {
                     return ClientStatus.Hung;
                  }
                  else
                  {
                     return statusData.ReturnedStatus;
                  }
               }
               
               return ClientStatus.Hung;
            }
            else
            {
               return statusData.ReturnedStatus;
            }
         }
      }
      
      private static int GetBaseFrameTime(TimeSpan averageFrameTime, ClientType TypeOfClient)
      {
         // no frame time based on the current PPD calculation selection ('LastFrame', 'LastThreeFrames', etc)
         // this section attempts to give DetermineStats values to detect Hung clients before they have a valid
         // frame time - Issue 10

         // get the average frame time for this client and project id
         if (averageFrameTime > TimeSpan.Zero)
         {
            return Convert.ToInt32(averageFrameTime.TotalSeconds);
         }

         // no benchmarked average frame time, use some arbitrary (and large) values for the frame time
         // we want to give the client plenty of time to show progress but don't want it to sit idle for days
         else
         {
            // CPU: use 1 hour (3600 seconds) as a base frame time
            int BaseFrameTime = 3600;
            if (TypeOfClient.Equals(ClientType.GPU))
            {
               // GPU: use 10 minutes (600 seconds) as a base frame time
               BaseFrameTime = 600;
            }

            return BaseFrameTime;
         }
      }

      /// <summary>
      /// Send EuePause Status Email
      /// </summary>
      private static void SendEuePauseEmail(string InstanceName, IPreferenceSet Prefs)
      {
         string messageBody = String.Format("HFM.NET detected that Client '{0}' has entered a 24 hour EUE Pause state.", InstanceName);
         try
         {
            NetworkOps.SendEmail(Prefs.GetPreference<bool>(Preference.EmailReportingServerSecure), 
                                 Prefs.GetPreference<string>(Preference.EmailReportingFromAddress), 
                                 Prefs.GetPreference<string>(Preference.EmailReportingToAddress),
                                 "HFM.NET - Client EUE Pause Error", messageBody, 
                                 Prefs.GetPreference<string>(Preference.EmailReportingServerAddress),
                                 Prefs.GetPreference<int>(Preference.EmailReportingServerPort),
                                 Prefs.GetPreference<string>(Preference.EmailReportingServerUsername), 
                                 Prefs.GetPreference<string>(Preference.EmailReportingServerPassword));
         }
         catch (Exception ex)
         {
            HfmTrace.WriteToHfmConsole(ex);
         }
      }

      /// <summary>
      /// Determine Client Status
      /// </summary>
      /// <param name="statusData">Client Status Data</param>
      private static ClientStatus DetermineStatus(StatusData statusData)
      {
         #region Get Terminal Time
         // Terminal Time - defined as last retrieval time minus twice (7 times for GPU) the current Raw Time per Section.
         // if a new frame has not completed in twice the amount of time it should take to complete we should deem this client Hung.
         DateTime terminalDateTime;

         if (statusData.TypeOfClient.Equals(ClientType.GPU))
         {
            terminalDateTime = statusData.LastRetrievalTime.Subtract(TimeSpan.FromSeconds(statusData.FrameTime * 7));
         }
         else
         {
            terminalDateTime = statusData.LastRetrievalTime.Subtract(TimeSpan.FromSeconds(statusData.FrameTime * 2));
         }
         #endregion

         #region Get Last Retrieval Time Date
         DateTime currentFrameDateTime;

         if (statusData.IgnoreUtcOffset)
         {
            // get only the date from the last retrieval time (in universal), we'll add the current time below
            currentFrameDateTime = new DateTime(statusData.LastRetrievalTime.Date.Ticks, DateTimeKind.Utc);
         }
         else
         {
            // get only the date from the last retrieval time, we'll add the current time below
            currentFrameDateTime = statusData.LastRetrievalTime.Date;
         }
         #endregion

         #region Apply Frame Time Offset and Set Current Frame Time Date
         TimeSpan offset = TimeSpan.FromMinutes(statusData.ClientTimeOffset);
         TimeSpan adjustedFrameTime = statusData.TimeOfLastFrame;
         if (statusData.IgnoreUtcOffset == false)
         {
            adjustedFrameTime = adjustedFrameTime.Add(statusData.UtcOffset);
         }
         adjustedFrameTime = adjustedFrameTime.Subtract(offset);

         // client time has already rolled over to the next day. the offset correction has 
         // caused the adjusted frame time span to be negetive.  take the that negetive span
         // and add it to a full 24 hours to correct.
         if (adjustedFrameTime < TimeSpan.Zero)
         {
            adjustedFrameTime = TimeSpan.FromDays(1).Add(adjustedFrameTime);
         }

         // the offset correction has caused the frame time span to be greater than 24 hours.
         // subtract the extra day from the adjusted frame time span.
         else if (adjustedFrameTime > TimeSpan.FromDays(1))
         {
            adjustedFrameTime = adjustedFrameTime.Subtract(TimeSpan.FromDays(1));
         }

         // add adjusted Time of Last Frame (TimeSpan) to the DateTime with the correct date
         currentFrameDateTime = currentFrameDateTime.Add(adjustedFrameTime);
         #endregion

         #region Check For Frame from Prior Day (Midnight Rollover on Local Machine)
         bool priorDayAdjust = false;

         // if the current (and adjusted) frame time hours is greater than the last retrieval time hours, 
         // and the time difference is greater than an hour, then frame is from the day prior.
         // this should only happen after midnight time on the machine running HFM when the monitored client has 
         // not completed a frame since the local machine time rolled over to the next day, otherwise the time
         // stamps between HFM and the client are too far off, a positive offset should be set to correct.
         if (currentFrameDateTime.TimeOfDay.Hours > statusData.LastRetrievalTime.TimeOfDay.Hours &&
             currentFrameDateTime.TimeOfDay.Subtract(statusData.LastRetrievalTime.TimeOfDay).Hours > 0)
         {
            priorDayAdjust = true;

            // subtract 1 day from today's date
            currentFrameDateTime = currentFrameDateTime.Subtract(TimeSpan.FromDays(1));
         }
         #endregion

         #region Write Verbose Trace
         if (TraceLevelSwitch.Switch.TraceVerbose)
         {
            List<string> messages = new List<string>(10);

            messages.Add(String.Format("{0} ({1})", HfmTrace.FunctionName, statusData.InstanceName));
            messages.Add(String.Format(" - Retrieval Time (Date) ------- : {0}", statusData.LastRetrievalTime));
            messages.Add(String.Format(" - Time Of Last Frame (TimeSpan) : {0}", statusData.TimeOfLastFrame));
            messages.Add(String.Format(" - Offset (Minutes) ------------ : {0}", statusData.ClientTimeOffset));
            messages.Add(String.Format(" - Time Of Last Frame (Adjusted) : {0}", adjustedFrameTime));
            messages.Add(String.Format(" - Prior Day Adjustment -------- : {0}", priorDayAdjust));
            messages.Add(String.Format(" - Time Of Last Frame (Date) --- : {0}", currentFrameDateTime));
            messages.Add(String.Format(" - Terminal Time (Date) -------- : {0}", terminalDateTime));

            HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, messages);
         }
         #endregion

         if (currentFrameDateTime > terminalDateTime)
         {
            return ClientStatus.Running;
         }
         else // current frame is less than terminal time
         {
            return ClientStatus.Hung;
         }
      }

      /// <summary>
      /// Determine Client Status
      /// </summary>
      /// <param name="statusData">Client Status Data</param>
      private static ClientStatus DetermineAsyncStatus(StatusData statusData)
      {
         #region Get Terminal Time
         // Terminal Time - defined as last retrieval time minus twice (7 times for GPU) the current Raw Time per Section.
         // if a new frame has not completed in twice the amount of time it should take to complete we should deem this client Hung.
         DateTime terminalDateTime;

         if (statusData.TypeOfClient.Equals(ClientType.GPU))
         {
            terminalDateTime = statusData.LastRetrievalTime.Subtract(TimeSpan.FromSeconds(statusData.FrameTime * 7));
         }
         else
         {
            terminalDateTime = statusData.LastRetrievalTime.Subtract(TimeSpan.FromSeconds(statusData.FrameTime * 2));
         }
         #endregion

         #region Determine Unit Progress Value to Use
         Debug.Assert(statusData.TimeOfLastUnitStart.Equals(DateTime.MinValue) == false);

         DateTime LastProgress = statusData.TimeOfLastUnitStart;
         if (statusData.TimeOfLastFrameProgress > statusData.TimeOfLastUnitStart)
         {
            LastProgress = statusData.TimeOfLastFrameProgress;
         } 
         #endregion
         
         #region Write Verbose Trace
         if (TraceLevelSwitch.Switch.TraceVerbose)
         {
            List<string> messages = new List<string>(4);

            messages.Add(String.Format("{0} ({1})", HfmTrace.FunctionName, statusData.InstanceName));
            messages.Add(String.Format(" - Retrieval Time (Date) ------- : {0}", statusData.LastRetrievalTime));
            messages.Add(String.Format(" - Time Of Last Unit Start ----- : {0}", statusData.TimeOfLastUnitStart));
            messages.Add(String.Format(" - Time Of Last Frame Progress - : {0}", statusData.TimeOfLastFrameProgress));
            messages.Add(String.Format(" - Terminal Time (Date) -------- : {0}", terminalDateTime));

            HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, messages);
         }
         #endregion

         if (LastProgress > terminalDateTime)
         {
            return ClientStatus.RunningAsync;
         }
         else // time of last progress is less than terminal time
         {
            return ClientStatus.Hung;
         }
      }

      #region Status Color Helper Functions
      /// <summary>
      /// Gets Status Color Pen Object
      /// </summary>
      /// <param name="status">Client Status</param>
      /// <returns>Status Color (Pen)</returns>
      public static Pen GetStatusPen(ClientStatus status)
      {
         return new Pen(GetStatusColor(status));
      }

      /// <summary>
      /// Gets Status Color Brush Object
      /// </summary>
      /// <param name="status">Client Status</param>
      /// <returns>Status Color (Brush)</returns>
      public static SolidBrush GetStatusBrush(ClientStatus status)
      {
         return new SolidBrush(GetStatusColor(status));
      }

      /// <summary>
      /// Gets Status Html Color String
      /// </summary>
      /// <param name="status">Client Status</param>
      /// <returns>Status Html Color (String)</returns>
      public static string GetStatusHtmlColor(ClientStatus status)
      {
         return ColorTranslator.ToHtml(GetStatusColor(status));
      }

      /// <summary>
      /// Gets Status Html Font Color String
      /// </summary>
      /// <param name="status">Client Status</param>
      /// <returns>Status Html Font Color (String)</returns>
      public static string GetStatusHtmlFontColor(ClientStatus status)
      {
         switch (status)
         {
            case ClientStatus.Running:
               return ColorTranslator.ToHtml(Color.White);
            case ClientStatus.RunningAsync:
               return ColorTranslator.ToHtml(Color.White);
            case ClientStatus.RunningNoFrameTimes:
               return ColorTranslator.ToHtml(Color.Black);
            case ClientStatus.Stopped:
            case ClientStatus.EuePause:
            case ClientStatus.Hung:
               return ColorTranslator.ToHtml(Color.White);
            case ClientStatus.Paused:
               return ColorTranslator.ToHtml(Color.Black);
            case ClientStatus.SendingWorkPacket:
            case ClientStatus.GettingWorkPacket:
               return ColorTranslator.ToHtml(Color.White);
            case ClientStatus.Offline:
               return ColorTranslator.ToHtml(Color.Black);
            default:
               return ColorTranslator.ToHtml(Color.Black);
         }
      }

      /// <summary>
      /// Gets Status Color Object
      /// </summary>
      /// <param name="status">Client Status</param>
      /// <returns>Status Color (Color)</returns>
      public static Color GetStatusColor(ClientStatus status)
      {
         switch (status)
         {
            case ClientStatus.Running:
               return Color.Green; // Issue 45
            case ClientStatus.RunningAsync:
               return Color.Blue;
            case ClientStatus.RunningNoFrameTimes:
               return Color.Yellow;
            case ClientStatus.Stopped:
            case ClientStatus.EuePause:
            case ClientStatus.Hung:
               return Color.DarkRed;
            case ClientStatus.Paused:
               return Color.Orange;
            case ClientStatus.SendingWorkPacket:
            case ClientStatus.GettingWorkPacket:
               return Color.Purple;
            case ClientStatus.Offline:
               return Color.Gray;
            default:
               return Color.Gray;
         }
      }
      #endregion
      
      #endregion

      #region XML Serialization
      /// <summary>
      /// Serialize this Client Instance to Xml
      /// </summary>
      public System.Xml.XmlDocument ToXml()
      {
         DateTime Start = HfmTrace.ExecStart;

         try
         {
            System.Xml.XmlDocument xmlData = new System.Xml.XmlDocument();

            System.Xml.XmlElement xmlRoot = xmlData.CreateElement(xmlNodeInstance);
            xmlRoot.SetAttribute(xmlAttrName, InstanceName);
            xmlData.AppendChild(xmlRoot);
            
            xmlData.ChildNodes[0].AppendChild(XMLOps.createXmlNode(xmlData, xmlNodeFAHLog, RemoteFAHLogFilename));
            xmlData.ChildNodes[0].AppendChild(XMLOps.createXmlNode(xmlData, xmlNodeUnitInfo, RemoteUnitInfoFilename));
            xmlData.ChildNodes[0].AppendChild(XMLOps.createXmlNode(xmlData, xmlNodeQueue, RemoteQueueFilename));
            xmlData.ChildNodes[0].AppendChild(XMLOps.createXmlNode(xmlData, xmlNodeClientMHz, ClientProcessorMegahertz.ToString()));
            xmlData.ChildNodes[0].AppendChild(XMLOps.createXmlNode(xmlData, xmlNodeClientVM, ClientIsOnVirtualMachine.ToString()));
            xmlData.ChildNodes[0].AppendChild(XMLOps.createXmlNode(xmlData, xmlNodeClientOffset, ClientTimeOffset.ToString()));
            xmlData.ChildNodes[0].AppendChild(XMLOps.createXmlNode(xmlData, xmlPropType, InstanceHostType.ToString()));
            xmlData.ChildNodes[0].AppendChild(XMLOps.createXmlNode(xmlData, xmlPropPath, Path));
            xmlData.ChildNodes[0].AppendChild(XMLOps.createXmlNode(xmlData, xmlPropServ, Server));
            xmlData.ChildNodes[0].AppendChild(XMLOps.createXmlNode(xmlData, xmlPropUser, Username));

            Symmetric SymetricProvider = new Symmetric(Symmetric.Provider.Rijndael, false);
            
            string encryptedPassword = String.Empty;
            if (Password.Length > 0)
            {
               try
               {
                  SymetricProvider.IntializationVector = IV;
                  encryptedPassword = SymetricProvider.Encrypt(new Data(Password), SymmetricKey).ToBase64();
               }
               catch (CryptographicException)
               {
                  HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, "Failed to encrypt Server Password... saving clear value.");
                  encryptedPassword = Password;
               }
            }
            xmlData.ChildNodes[0].AppendChild(XMLOps.createXmlNode(xmlData, xmlPropPass, encryptedPassword));
            xmlData.ChildNodes[0].AppendChild(XMLOps.createXmlNode(xmlData, xmlPropPassiveFtpMode, FtpMode.ToString()));
            
            return xmlData;
         }
         catch (Exception ex)
         {
            HfmTrace.WriteToHfmConsole(ex);
         }
         finally
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Info, InstanceName, Start);
         }
         
         return null;
      }

      /// <summary>
      /// Deserialize Xml data into this Client Instance
      /// </summary>
      /// <param name="xmlData">XmlNode containing the client instance data</param>
      public void FromXml(System.Xml.XmlNode xmlData)
      {
         DateTime Start = HfmTrace.ExecStart;
         
         InstanceName = xmlData.Attributes[xmlAttrName].ChildNodes[0].Value;
         try
         {
            RemoteFAHLogFilename = xmlData.SelectSingleNode(xmlNodeFAHLog).InnerText;
         }
         catch (NullReferenceException)
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, String.Format("{0} {1}.", HfmTrace.FunctionName, "Cannot load Remote FAHlog Filename."));
            RemoteFAHLogFilename = LocalFAHLog;
         }
         
         try
         {
            RemoteUnitInfoFilename = xmlData.SelectSingleNode(xmlNodeUnitInfo).InnerText;
         }
         catch (NullReferenceException)
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, String.Format("{0} {1}.", HfmTrace.FunctionName, "Cannot load Remote Unitinfo Filename."));
            RemoteUnitInfoFilename = LocalUnitInfo;
         }
         
         try
         {
            RemoteQueueFilename = xmlData.SelectSingleNode(xmlNodeQueue).InnerText;
         }
         catch (NullReferenceException)
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, String.Format("{0} {1}.", HfmTrace.FunctionName, "Cannot load Remote Queue Filename."));
            RemoteQueueFilename = LocalQueue;
         }
         
         try
         {
            ClientProcessorMegahertz = int.Parse(xmlData.SelectSingleNode(xmlNodeClientMHz).InnerText);
            if (ClientProcessorMegahertz < 1)
            {
               ClientProcessorMegahertz = 1;
            }
         }
         catch (NullReferenceException)
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, String.Format("{0} {1}.", HfmTrace.FunctionName, "Cannot load Client MHz, defaulting to 1 MHz."));
            ClientProcessorMegahertz = 1;
         }
         catch (FormatException)
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, String.Format("{0} {1}.", HfmTrace.FunctionName, "Could not parse Client MHz, defaulting to 1 MHz."));
            ClientProcessorMegahertz = 1;
         }

         try
         {
            ClientIsOnVirtualMachine = Convert.ToBoolean(xmlData.SelectSingleNode(xmlNodeClientVM).InnerText);
         }
         catch (NullReferenceException)
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, String.Format("{0} {1}.", HfmTrace.FunctionName, "Cannot load Client VM Flag, defaulting to false."));
            ClientIsOnVirtualMachine = false;
         }
         catch (InvalidCastException)
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, String.Format("{0} {1}.", HfmTrace.FunctionName, "Could not parse Client VM Flag, defaulting to false."));
            ClientIsOnVirtualMachine = false;
         }

         try
         {
            ClientTimeOffset = int.Parse(xmlData.SelectSingleNode(xmlNodeClientOffset).InnerText);
         }
         catch (NullReferenceException)
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, String.Format("{0} {1}.", HfmTrace.FunctionName, "Cannot load Client Time Offset, defaulting to 0."));
            ClientTimeOffset = 0;
         }
         catch (FormatException)
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, String.Format("{0} {1}.", HfmTrace.FunctionName, "Could not parse Client Time Offset, defaulting to 0."));
            ClientTimeOffset = 0;
         }

         try
         {
            Path = xmlData.SelectSingleNode(xmlPropPath).InnerText;
         }
         catch (NullReferenceException)
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, String.Format("{0} {1}.", HfmTrace.FunctionName, "Cannot load Client Path."));
         }

         try
         {
            Server = xmlData.SelectSingleNode(xmlPropServ).InnerText;
         }
         catch (NullReferenceException)
         {
            Server = String.Empty;
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, String.Format("{0} {1}.", HfmTrace.FunctionName, "Cannot load Client Server."));
         }
         
         try
         {
            Username = xmlData.SelectSingleNode(xmlPropUser).InnerText;
         }
         catch (NullReferenceException)
         {
            Username = String.Empty;
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, String.Format("{0} {1}.", HfmTrace.FunctionName, "Cannot load Server Username."));
         }
         
         Symmetric SymetricProvider = new Symmetric(Symmetric.Provider.Rijndael, false);
         
         try
         {
            Password = String.Empty;
            if (xmlData.SelectSingleNode(xmlPropPass).InnerText.Length > 0)
            {
               try
               {
                  SymetricProvider.IntializationVector = IV;
                  Password = SymetricProvider.Decrypt(new Data(Utils.FromBase64(xmlData.SelectSingleNode(xmlPropPass).InnerText)), SymmetricKey).ToString();
               }
               catch (FormatException)
               {
                  HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, "Server Password is not Base64 encoded... loading clear value.");
                  Password = xmlData.SelectSingleNode(xmlPropPass).InnerText;
               }
               catch (CryptographicException)
               {
                  HfmTrace.WriteToHfmConsole(TraceLevel.Warning, InstanceName, "Cannot decrypt Server Password... loading clear value.");
                  Password = xmlData.SelectSingleNode(xmlPropPass).InnerText;
               }
            }
         }
         catch (NullReferenceException)
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, String.Format("{0} {1}.", HfmTrace.FunctionName, "Cannot load Server Password."));
         }

         try
         {
            string FtpModeText = xmlData.SelectSingleNode(xmlPropPassiveFtpMode).InnerText;
            FtpMode = (FtpType)Enum.Parse(typeof(FtpType), FtpModeText, false);
         }
         catch (NullReferenceException)
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, String.Format("{0} {1}.", HfmTrace.FunctionName, "Cannot load Ftp Mode Flag, defaulting to Passive."));
            FtpMode = FtpType.Passive;
         }
         catch (InvalidCastException)
         {
            HfmTrace.WriteToHfmConsole(TraceLevel.Warning, String.Format("{0} {1}.", HfmTrace.FunctionName, "Could not parse Ftp Mode Flag, defaulting to Passive."));
            FtpMode = FtpType.Passive;
         }

         HfmTrace.WriteToHfmConsole(TraceLevel.Verbose, InstanceName, Start);
      }

      /// <summary>
      /// Restore the given UnitInfo into this Client Instance
      /// </summary>
      /// <param name="unitInfo">UnitInfo Object to Restore</param>
      public void RestoreUnitInfo(UnitInfo unitInfo)
      {
         CurrentUnitInfoConcrete = new UnitInfoLogic(_Prefs, _proteinCollection, unitInfo, ClientIsOnVirtualMachine);
      }
      #endregion

      #region Other Helper Functions
      public bool IsUsernameOk()
      {
         // if these are the default assigned values, don't check otherwise and just return true
         if (FoldingID == Constants.FoldingIDDefault && Team == Constants.TeamDefault)
         {
            return true;
         }

         if ((FoldingID != _Prefs.GetPreference<string>(Preference.StanfordID) || 
              Team != _Prefs.GetPreference<int>(Preference.TeamID)) &&
             (Status.Equals(ClientStatus.Unknown) == false && Status.Equals(ClientStatus.Offline) == false))
         {
            return false;
         }

         return true;
      }
      
      public bool Owns(IOwnedByClientInstance value)
      {
         if (value.OwningInstanceName.Equals(InstanceName) &&
             value.OwningInstancePath.Equals(Path))
         {
            return true;
         }
         
         return false;
      }
      #endregion
   }
}