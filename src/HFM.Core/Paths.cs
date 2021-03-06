﻿/*
 * HFM.NET - Paths Class
 * Copyright (C) 2009-2012 Ryan Harlamert (harlam357)
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
using System.Linq;

namespace HFM.Core
{
   // Tested 100% through BenchmarkClientTests

   public static class Paths
   {
      /// <summary>
      /// Are two paths equal?
      /// </summary>
      public static bool Equal(string path1, string path2)
      {
         IEnumerable<string> path1Variations = GetPathVariations(path1);
         IEnumerable<string> path2Variations = GetPathVariations(path2);

         foreach (var variation1 in path1Variations)
         {
            foreach (var variation2 in path2Variations)
            {
               if (String.Equals(variation1, variation2, StringComparison))
               {
                  return true;
               }
            }
         }

         return false;
      }

      private static IEnumerable<string> GetPathVariations(string path)
      {
         var pathVariations = new List<string>(2);
         if (path.EndsWith("\\", StringComparison.OrdinalIgnoreCase ) || 
             path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
         {
            pathVariations.Add(path);
         }
         else
         {
            pathVariations.Add(String.Concat(path, "\\"));
            pathVariations.Add(String.Concat(path, "/"));
         }
         return pathVariations;
      }

      /// <summary>
      /// String Comparison for Paths (case sensetive on Mono / case insensetive on .NET)
      /// </summary>
      public static StringComparison StringComparison
      {
         get { return Application.IsRunningOnMono ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase; }
      }

      /// <summary>
      /// Add trailing path slash (Windows or Unix).
      /// </summary>
      public static string AddTrailingSlash(string path)
      {
         if (path == null) return String.Empty;

         const char backslash = '\\';
         const char forwardslash = '/';

         char separatorChar = backslash;
         if (path.TakeWhile(c => !c.Equals(backslash)).Any(c => c.Equals(forwardslash)))
         {
            separatorChar = forwardslash;
         }

         // if the path is of sufficient length but does not
         // end with the dectected directory separator character
         // then append the detected separator character
         if (path.Length > 2 && (!path.EndsWith(separatorChar.ToString())))
         {
            path = String.Concat(path, separatorChar);
         }

         return path;
      }

      /// <summary>
      /// Add Unix style trailing path slash.
      /// </summary>
      public static string AddUnixTrailingSlash(string path)
      {
         if (path == null) return String.Empty;

         if (!path.EndsWith("/"))
         {
            path = String.Concat(path, "/");
         }

         return path;
      }
   }
}
