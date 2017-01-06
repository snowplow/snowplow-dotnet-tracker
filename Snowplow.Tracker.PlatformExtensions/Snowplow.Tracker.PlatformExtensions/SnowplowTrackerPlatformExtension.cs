/*
 * SnowplowTrackerPlatformExtension.cs
 * 
 * Copyright (c) 2017 Snowplow Analytics Ltd. All rights reserved.
 * This program is licensed to you under the Apache License Version 2.0,
 * and you may not use this file except in compliance with the Apache License
 * Version 2.0. You may obtain a copy of the Apache License Version 2.0 at
 * http://www.apache.org/licenses/LICENSE-2.0.
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the Apache License Version 2.0 is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the Apache License Version 2.0 for the specific
 * language governing permissions and limitations there under.
 * Authors: Joshua Beemster
 * Copyright: Copyright (c) 2017 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

using Snowplow.Tracker.PlatformExtensions.Abstractions;

using System;

namespace Snowplow.Tracker.PlatformExtensions
{
  /// <summary>
  /// Cross platform Snowplow Tracker implemenations
  /// </summary>
  public class SnowplowTrackerPlatformExtension
  {
    static Lazy<ISnowplowTrackerPlatformExtended> Implementation = new Lazy<ISnowplowTrackerPlatformExtended>(() => CreateSnowplowTrackerPlatformExtension(), System.Threading.LazyThreadSafetyMode.PublicationOnly);
    
    /// <summary>
    /// Returns the current implementation
    /// </summary>
    public static ISnowplowTrackerPlatformExtended Current
    {
      get
      {
        var ret = Implementation.Value;
        if (ret == null)
        {
          throw NotImplementedInReferenceAssembly();
        }
        return ret;
      }
    }

    static ISnowplowTrackerPlatformExtended CreateSnowplowTrackerPlatformExtension()
    {
#if PORTABLE
        return null;
#else
        return new SnowplowTrackerPlatformExtendedImplementation();
#endif
        }

        internal static Exception NotImplementedInReferenceAssembly()
    {
      return new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
    }
  }
}
