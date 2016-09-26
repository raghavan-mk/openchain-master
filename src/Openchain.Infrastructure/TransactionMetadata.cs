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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Openchain.Infrastructure
{
    /// <summary>
    /// Represents the metadata object that can be attached to a transaction, and contains signatures for that transaction.
    /// </summary>
    public class TransactionMetadata
    {
        public TransactionMetadata(IEnumerable<SignatureEvidence> signatures)
        {
            this.Signatures = new ReadOnlyCollection<SignatureEvidence>(signatures.ToList());
        }

        /// <summary>
        /// Gets the list of signatures.
        /// </summary>
        public IReadOnlyList<SignatureEvidence> Signatures { get; }
    }
}
