﻿/*
 * HFM.NET - Work Unit History Presenter
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
using System.Windows.Forms;

using Castle.Core.Logging;

using harlam357.Windows.Forms;

using HFM.Core;
using HFM.Core.DataTypes;
using HFM.Forms.Models;

namespace HFM.Forms
{
   public interface IHistoryPresenterFactory
   {
      HistoryPresenter Create();

      void Release(HistoryPresenter presenter);
   }

   public class HistoryPresenter
   {
      private readonly IPreferenceSet _prefs;
      private readonly IQueryParametersCollection _queryCollection;
      private readonly IHistoryView _view;
      private readonly IViewFactory _viewFactory;
      private readonly IMessageBoxView _messageBoxView;
      private readonly HistoryPresenterModel _model;

      private ILogger _logger;

      public ILogger Logger
      {
         [CoverageExclude]
         get { return _logger ?? (_logger = NullLogger.Instance); }
         [CoverageExclude]
         set { _logger = value; }
      }

      public event EventHandler PresenterClosed;
      
      public HistoryPresenter(IPreferenceSet prefs, 
                              IQueryParametersCollection queryCollection, 
                              IHistoryView view, 
                              IViewFactory viewFactory, 
                              IMessageBoxView messageBoxView, 
                              HistoryPresenterModel model)
      {
         _prefs = prefs;
         _queryCollection = queryCollection;
         _view = view;
         _viewFactory = viewFactory;
         _messageBoxView = messageBoxView;
         _model = model;
      }
      
      public void Initialize()
      {
         _view.AttachPresenter(this);
         _model.Load(_prefs, _queryCollection);
         _view.DataBindModel(_model);
      }

      public void Show()
      {
         _view.Show();
         if (_view.WindowState == FormWindowState.Minimized)
         {
            _view.WindowState = FormWindowState.Normal;            
         }
         else
         {
            _view.BringToFront();
         }
      }

      public void Close()
      {
         var handler = PresenterClosed;
         if (handler != null)
         {
            handler(this, EventArgs.Empty);
         }
      }
      
      public void OnViewClosing()
      {
         // Save location and size data
         // RestoreBounds remembers normal position if minimized or maximized
         if (_view.WindowState == FormWindowState.Normal)
         {
            _model.FormLocation = _view.Location;
            _model.FormSize = _view.Size;
         }
         else
         {
            _model.FormLocation = _view.RestoreBounds.Location;
            _model.FormSize = _view.RestoreBounds.Size;
         }

         _model.FormColumns = _view.GetColumnSettings();
         _model.Update(_prefs, _queryCollection);
      }

      public void FirstPageClicked()
      {
         _model.CurrentPage = 1;
      }

      public void PreviousPageClicked()
      {
         _model.CurrentPage -= 1;
      }

      public void NextPageClicked()
      {
         _model.CurrentPage += 1;
      }

      public void LastPageClicked()
      {
         _model.CurrentPage = _model.TotalPages;
      }
      
      public void NewQueryClick()
      {
         var queryView = _viewFactory.GetQueryDialog();
         var query = new QueryParameters { Name = "* New Query *" };
         query.Fields.Add(new QueryField());
         queryView.Query = query;
         
         bool showDialog = true;
         while (showDialog)
         {
            if (queryView.ShowDialog(_view) == DialogResult.OK)
            {
               try
               {
                  _model.AddQuery(queryView.Query);
                  showDialog = false;
               }
               catch (ArgumentException ex)
               {
                  _messageBoxView.ShowError(_view, ex.Message, Core.Application.NameAndVersion);
               }
            }
            else
            {
               showDialog = false;
            }
         }
         _viewFactory.Release(queryView);
      }
      
      public void EditQueryClick()
      {
         var queryView = _viewFactory.GetQueryDialog();
         queryView.Query = _model.SelectedQuery.DeepClone();

         bool showDialog = true;
         while (showDialog)
         {
            if (queryView.ShowDialog(_view) == DialogResult.OK)
            {
               try
               {
                  _model.ReplaceQuery(queryView.Query);
                  showDialog = false;
               }
               catch (ArgumentException ex)
               {
                  _messageBoxView.ShowError(_view, ex.Message, Core.Application.NameAndVersion);
               }
            }
            else
            {
               showDialog = false;
            }
         }
         _viewFactory.Release(queryView);
      }

      public void DeleteQueryClick()
      {
         var result = _messageBoxView.AskYesNoQuestion(_view, "Are you sure?", Core.Application.NameAndVersion);
         if (result == DialogResult.Yes)
         {
            try
            {
               _model.RemoveQuery(_model.SelectedQuery);
            }
            catch (ArgumentException ex)
            {
               _messageBoxView.ShowError(_view, ex.Message, Core.Application.NameAndVersion);
            }
         }
      }
      
      public void DeleteWorkUnitClick()
      {
         var entry = _model.SelectedHistoryEntry;
         if (entry == null)
         {
            _messageBoxView.ShowInformation(_view, "No work unit selected.", Core.Application.NameAndVersion);
         }
         else
         {
            var result = _messageBoxView.AskYesNoQuestion(_view, "Are you sure?  This operation cannot be undone.", Core.Application.NameAndVersion);
            if (result == DialogResult.Yes)
            {
               _model.DeleteHistoryEntry(entry);
            }
         }
      }
      
      public void RefreshProjectDataClick(ProteinUpdateType type)
      {
         var result = _messageBoxView.AskYesNoQuestion(_view, "Are you sure?  This operation cannot be undone.", Core.Application.NameAndVersion);
         if (result == DialogResult.No)
         {
            return;
         }

         var processor = ServiceLocator.Resolve<ProteinDataUpdater>();
         processor.UpdateType = type;
         if (type == ProteinUpdateType.Project)
         {
            processor.UpdateArg = _model.SelectedHistoryEntry.ProjectID;   
         }
         else if (type == ProteinUpdateType.Id)
         {
            processor.UpdateArg = _model.SelectedHistoryEntry.ID;
         }
         // Execute Asynchronous Operation
         var view = _viewFactory.GetProgressDialog();
         view.ProcessRunner = processor;
         view.Icon = Properties.Resources.hfm_48_48;
         view.Text = "Updating Project Data";
         view.OwnerWindow = _view;
         view.Process();

         var runner = view.ProcessRunner;
         if (runner.Exception != null)
         {
            _logger.Error(runner.Exception.Message, runner.Exception);
            _messageBoxView.ShowError(_view, runner.Exception.Message, Core.Application.NameAndVersion);
         }
         else
         {
            _model.ResetBindings(true);
         }
         _viewFactory.Release(view);
      }
   }
}
