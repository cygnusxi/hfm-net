﻿/*
 * HFM.NET - History Entry Class
 * Copyright (C) 2009-2011 Ryan Harlamert (harlam357)
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
using System.Diagnostics;

namespace HFM.Framework.DataTypes
{
   public class HistoryEntry : IEquatable<HistoryEntry>
   {
      public HistoryEntry()
      {
         ProductionView = HistoryProductionView.BonusDownloadTime;
      }

      public long ID { get; set; }
      public int ProjectID { get; set; }
      public int ProjectRun { get; set; }
      public int ProjectClone { get; set; }
      public int ProjectGen { get; set; }
      public string InstanceName { get; set; }
      public string InstancePath { get; set; }
      public string Username { get; set; }
      public int Team { get; set; }
      public float CoreVersion { get; set; }
      public int FramesCompleted { get; set; }
      public TimeSpan FrameTime { get; set; }
      public WorkUnitResult Result { get; set; }
      public DateTime DownloadDateTime { get; set; }
      public DateTime CompletionDateTime { get; set; }

      private IProtein _protein;
      
      public string WorkUnitName { get { return _protein == null ? String.Empty : _protein.WorkUnitName; } }
      public double KFactor { get { return _protein == null ? 0 : _protein.KFactor; } }
      public string Core { get { return _protein == null ? String.Empty : _protein.Core; } }
      public int Frames { get { return _protein == null ? 0 : _protein.Frames; } }
      public int Atoms { get { return _protein == null ? 0 : _protein.NumAtoms; } }

      public ClientType ClientType { get; private set; }
      
      public HistoryProductionView ProductionView { get; set; }
      
      public double PPD
      {
         get
         {
            if (_protein == null) return 0;
         
            switch (ProductionView)
            {
               case HistoryProductionView.Standard:
                  return _protein.GetPPD(FrameTime);
               case HistoryProductionView.BonusFrameTime:
                  return _protein.GetPPD(FrameTime, TimeSpan.FromSeconds(FrameTime.TotalSeconds * Frames));
               case HistoryProductionView.BonusDownloadTime:
                  return _protein.GetPPD(FrameTime, CompletionDateTime.Subtract(DownloadDateTime));
               default:
                  // ReSharper disable HeuristicUnreachableCode
                  Debug.Assert(false);
                  return 0;
                  // ReSharper restore HeuristicUnreachableCode
            }
         }
      }

      public double Credit
      {
         get
         {
            if (_protein == null) return 0;

            switch (ProductionView)
            {
               case HistoryProductionView.Standard:
                  return _protein.Credit;
               case HistoryProductionView.BonusFrameTime:
                  return _protein.GetBonusCredit(TimeSpan.FromSeconds(FrameTime.TotalSeconds * Frames));
               case HistoryProductionView.BonusDownloadTime:
                  return _protein.GetBonusCredit(CompletionDateTime.Subtract(DownloadDateTime));
               default:
                  // ReSharper disable HeuristicUnreachableCode
                  Debug.Assert(false);
                  return 0;
                  // ReSharper restore HeuristicUnreachableCode
            }
         }
      }
      
      public HistoryEntry SetProtein(IProtein protein)
      {
         _protein = protein;
         if (protein != null)
         {
            ClientType = Protein.GetClientTypeFromCore(protein.Core);
         }

         return this;
      }

      #region IEquatable<HistoryEntry> Members

      public bool Equals(HistoryEntry other)
      {
         return (ProjectID == other.ProjectID &&
                 ProjectRun == other.ProjectRun &&
                 ProjectClone == other.ProjectClone &&
                 ProjectGen == other.ProjectGen &&
                 DownloadDateTime == other.DownloadDateTime);
      }

      #endregion
   }
}