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

namespace Openchain.Infrastructure
{
    /// <summary>
    /// Represents the key of an account record.
    /// </summary>
    public class AccountKey : IEquatable<AccountKey>
    {
        public AccountKey(LedgerPath account, LedgerPath asset)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            this.Account = account;
            this.Asset = asset;
            this.Key = new RecordKey(RecordType.Account, account, asset.FullPath);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="AccountKey"/> class from an account and asset.
        /// </summary>
        /// <param name="account">The account path.</param>
        /// <param name="asset">The asset path.</param>
        /// <returns>An instance of the <see cref="AccountKey"/> class representing the account and asset provided.</returns>
        public static AccountKey Parse(string account, string asset)
        {
            return new AccountKey(
                LedgerPath.Parse(account),
                LedgerPath.Parse(asset));
        }

        /// <summary>
        /// Gets the <see cref="LedgerPath"/> of the account that this instance represents.
        /// </summary>
        public LedgerPath Account { get; }

        /// <summary>
        /// Gets the <see cref="LedgerPath"/> of the asset that this instance represents.
        /// </summary>
        public LedgerPath Asset { get; }

        /// <summary>
        /// Gets the <see cref="RecordKey"/> equivalent to this instance.
        /// </summary>
        public RecordKey Key { get; }

        public bool Equals(AccountKey other)
        {
            if (other == null)
                return false;
            else
                return StringComparer.Ordinal.Equals(Key.ToString(), other.Key.ToString());
        }

        public override bool Equals(object obj)
        {
            if (obj is AccountKey)
                return this.Equals((AccountKey)obj);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(ToString());
        }
    }
}
