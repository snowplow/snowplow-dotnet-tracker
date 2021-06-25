/*
 * Copyright (c) 2021 Snowplow Analytics Ltd. All rights reserved.
 * This program is licensed to you under the Apache License Version 2.0,
 * and you may not use this file except in compliance with the Apache License
 * Version 2.0. You may obtain a copy of the Apache License Version 2.0 at
 * http://www.apache.org/licenses/LICENSE-2.0.
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the Apache License Version 2.0 is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the Apache License Version 2.0 for the specific
 * language governing permissions and limitations there under.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Snowplow.Tracker.Models;

namespace Snowplow.Tracker.Endpoints
{
    public class SendResult
    {
        public List<string> SuccessIds { get; set; } = new List<string>();
        public List<string> FailureIds { get; set; } = new List<string>();
    }

    public class RequestResult
    {
        public bool IsOversize { get; set; } = false;
        public Task<int> StatusCodeTask { get; set; } = null;
        public List<string> ItemIds { get; set; }
    }

    public interface IEndpoint
    {
        SendResult Send(List<Tuple<string, Payload>> p);
    }
}
