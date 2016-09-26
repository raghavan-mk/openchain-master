﻿// Copyright 2015 Coinprism, Inc.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace Openchain
{
    /// <summary>
    /// Represents an error caused by the attempt of modifying a record using the wrong base version.
    /// </summary>
    public class ConcurrentMutationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentMutationException"/> class.
        /// </summary>
        /// <param name="failedMutation">The failed record mutation.</param>
        public ConcurrentMutationException(Record failedMutation)
            : base($"Version '{failedMutation.Version}' of key '{failedMutation.Key}' no longer exists.")
        {
            this.FailedMutation = failedMutation;
        }

        /// <summary>
        /// Gets the failed record mutation.
        /// </summary>
        public Record FailedMutation { get; }
    }
}
