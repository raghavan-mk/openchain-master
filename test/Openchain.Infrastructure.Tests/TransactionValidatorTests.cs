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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Openchain.Infrastructure.Tests
{
    public class TransactionValidatorTests
    {
        private static readonly ByteString validNamespace = ByteString.Parse("abcdef");
        private static readonly ByteString invalidNamespace = ByteString.Parse("abcdef00");
        private static readonly Dictionary<string, long> defaultAccounts = new Dictionary<string, long>()
        {
            ["/account/1/"] = 90,
            ["/account/2/"] = 110,
        };

        private TestStore store;

        [Fact]
        public async Task PostTransaction_Success()
        {
            TransactionValidator validator = CreateValidator(defaultAccounts);
            ByteString mutation = CreateMutation(validNamespace);

            ByteString result = await validator.PostTransaction(mutation, new SignatureEvidence[0]);

            Assert.Equal(32, result.Value.Count);
            Assert.Equal(1, store.AddedTransactions.Count);
        }

        [Fact]
        public async Task PostTransaction_GenerateValidMutations()
        {
            Mutation generatedMutation = MessageSerializer.DeserializeMutation(CreateMutation(validNamespace));

            TransactionValidator validator = CreateValidator(defaultAccounts, generatedMutation);
            ByteString mutation = CreateMutation(validNamespace);

            ByteString result = await validator.PostTransaction(mutation, new SignatureEvidence[0]);

            Assert.Equal(32, result.Value.Count);
            Assert.Equal(2, store.AddedTransactions.Count);
        }

        [Fact]
        public async Task PostTransaction_InvalidMutation()
        {
            TransactionValidator validator = CreateValidator(new Dictionary<string, long>());
            ByteString mutation = ByteString.Parse("aa");

            TransactionInvalidException exception = await Assert.ThrowsAsync<TransactionInvalidException>(
                () => validator.PostTransaction(mutation, new SignatureEvidence[0]));
            Assert.Equal("InvalidMutation", exception.Reason);
            Assert.Equal(null, store.AddedTransactions);
        }

        [Fact]
        public async Task PostTransaction_GenerateInvalidMutations()
        {
            Mutation generatedMutation = MessageSerializer.DeserializeMutation(CreateMutation(invalidNamespace));

            TransactionValidator validator = CreateValidator(defaultAccounts, generatedMutation);
            ByteString mutation = CreateMutation(validNamespace);

            TransactionInvalidException exception = await Assert.ThrowsAsync<TransactionInvalidException>(
                () => validator.PostTransaction(mutation, new SignatureEvidence[0]));
            Assert.Equal("InvalidNamespace", exception.Reason);
            Assert.Equal(null, store.AddedTransactions);
        }

        [Fact]
        public async Task PostTransaction_MaxKeySize()
        {
            Dictionary<string, long> accounts = new Dictionary<string, long>();

            TransactionValidator validator = CreateValidator(accounts);
            Mutation mutation = new Mutation(
                validNamespace,
                new Record[]
                {
                    new Record(
                        new AccountKey(LedgerPath.Parse("/"), LedgerPath.Parse($"/{new string('a', 512)}/")).Key.ToBinary(),
                        new ByteString(BitConverter.GetBytes(100L).Reverse()),
                        ByteString.Empty)
                },
                ByteString.Empty);

            TransactionInvalidException exception = await Assert.ThrowsAsync<TransactionInvalidException>(
                () => validator.PostTransaction(new ByteString(MessageSerializer.SerializeMutation(mutation)), new SignatureEvidence[0]));
            Assert.Equal("InvalidMutation", exception.Reason);
            Assert.Equal(null, store.AddedTransactions);
        }

        [Fact]
        public async Task PostTransaction_EmptyMutation()
        {
            Dictionary<string, long> accounts = new Dictionary<string, long>();

            TransactionValidator validator = CreateValidator(accounts);
            Mutation mutation = new Mutation(
                validNamespace,
                new Record[0],
                ByteString.Empty);

            TransactionInvalidException exception = await Assert.ThrowsAsync<TransactionInvalidException>(
                () => validator.PostTransaction(new ByteString(MessageSerializer.SerializeMutation(mutation)), new SignatureEvidence[0]));
            Assert.Equal("InvalidMutation", exception.Reason);
            Assert.Equal(null, store.AddedTransactions);
        }

        [Fact]
        public async Task PostTransaction_UnbalancedTransaction()
        {
            Dictionary<string, long> accounts = new Dictionary<string, long>()
            {
                ["/account/1/"] = 100,
                ["/account/2/"] = 110,
            };

            TransactionValidator validator = CreateValidator(accounts);
            ByteString mutation = CreateMutation(validNamespace);

            TransactionInvalidException exception = await Assert.ThrowsAsync<TransactionInvalidException>(
                () => validator.PostTransaction(mutation, new SignatureEvidence[0]));
            Assert.Equal("UnbalancedTransaction", exception.Reason);
            Assert.Equal(null, store.AddedTransactions);
        }

        [Fact]
        public async Task PostTransaction_InvalidNamespace()
        {
            TransactionValidator validator = CreateValidator(defaultAccounts);
            ByteString mutation = CreateMutation(invalidNamespace);

            TransactionInvalidException exception = await Assert.ThrowsAsync<TransactionInvalidException>(
                () => validator.PostTransaction(mutation, new SignatureEvidence[0]));
            Assert.Equal("InvalidNamespace", exception.Reason);
            Assert.Equal(null, store.AddedTransactions);
        }

        [Fact]
        public async Task PostTransaction_ConcurrencyException()
        {
            this.store = new TestStore(defaultAccounts, true);
            TransactionValidator validator = new TransactionValidator(
                this.store,
                new TestValidator(false),
                validNamespace);

            ByteString mutation = CreateMutation(validNamespace);

            TransactionInvalidException exception = await Assert.ThrowsAsync<TransactionInvalidException>(
                () => validator.PostTransaction(mutation, new SignatureEvidence[0]));
            Assert.Equal("OptimisticConcurrency", exception.Reason);
            Assert.Equal(1, store.AddedTransactions.Count);
        }

        [Fact]
        public async Task PostTransaction_ValidationException()
        {
            this.store = new TestStore(defaultAccounts, false);
            TransactionValidator validator = new TransactionValidator(
                this.store,
                new TestValidator(true),
                validNamespace);

            ByteString mutation = CreateMutation(validNamespace);

            TransactionInvalidException exception = await Assert.ThrowsAsync<TransactionInvalidException>(
                () => validator.PostTransaction(mutation, new SignatureEvidence[0]));
            Assert.Equal("Test", exception.Reason);
            Assert.Equal(null, store.AddedTransactions);
        }

        [Fact]
        public async Task Validate_ValidSignature()
        {
            TransactionValidator validator = CreateValidator(defaultAccounts);
            ByteString mutation = CreateMutation(validNamespace);

            SignatureEvidence signature = new SignatureEvidence(
                ByteString.Parse("0213b0006543d4ab6e79f49559fbfb18e9d73596d63f39e2f12ebc2c9d51e2eb06"),
                ByteString.Parse("3045022100e2ecc27c2e0d19329a0c7ad37e20fde00e64be235b2e7e86d285c18ff9c1e5b102200efa46125e057136f5008f4aa15a07e5ae4a0fcb2d00aa37862e246abbee74ab"));

            ByteString result = await validator.PostTransaction(mutation, new[] { signature });

            Assert.Equal(32, result.Value.Count);
            Assert.Equal(1, store.AddedTransactions.Count);
        }

        [Fact]
        public async Task Validate_InvalidSignature()
        {
            TransactionValidator validator = CreateValidator(defaultAccounts);
            ByteString mutation = CreateMutation(validNamespace);

            SignatureEvidence signature = new SignatureEvidence(
                ByteString.Parse("0013b0006543d4ab6e79f49559fbfb18e9d73596d63f39e2f12ebc2c9d51e2eb06"),
                ByteString.Parse("3045022100e2ecc27c2e0d19329a0c7ad37e20fde00e64be235b2e7e86d285c18ff9c1e5b102200efa46125e057136f5008f4aa15a07e5ae4a0fcb2d00aa37862e246abbee74ab"));

            TransactionInvalidException exception = await Assert.ThrowsAsync<TransactionInvalidException>(
                () => validator.PostTransaction(mutation, new[] { signature }));
            Assert.Equal("InvalidSignature", exception.Reason);
            Assert.Equal(null, store.AddedTransactions);
        }

        private ByteString CreateMutation(ByteString @namespace)
        {
            Mutation mutation = new Mutation(
                @namespace,
                new Record[]
                {
                    new Record(
                        new AccountKey(LedgerPath.Parse("/account/1/"), LedgerPath.Parse("/a/")).Key.ToBinary(),
                        new ByteString(BitConverter.GetBytes(100L).Reverse()),
                        ByteString.Empty),
                    new Record(
                        new AccountKey(LedgerPath.Parse("/account/2/"), LedgerPath.Parse("/a/")).Key.ToBinary(),
                        new ByteString(BitConverter.GetBytes(100L).Reverse()),
                        ByteString.Empty),
                },
                ByteString.Empty);

            return new ByteString(MessageSerializer.SerializeMutation(mutation));
        }

        private TransactionValidator CreateValidator(IDictionary<string, long> accounts, params Mutation[] mutations)
        {
            this.store = new TestStore(accounts, false);
            return new TransactionValidator(
                this.store,
                new TestValidator(false, mutations),
                validNamespace);
        }

        private class TestValidator : IMutationValidator
        {
            private readonly bool exception;
            private readonly Mutation[] mutations;

            public TestValidator(bool exception, params Mutation[] mutations)
            {
                this.exception = exception;
                this.mutations = mutations;
            }

            public Task<IList<Mutation>> Validate(ParsedMutation mutation, IReadOnlyList<SignatureEvidence> authentication, IReadOnlyDictionary<AccountKey, AccountStatus> accounts)
            {
                if (exception)
                    throw new TransactionInvalidException("Test");
                else
                    return Task.FromResult<IList<Mutation>>(mutations);
            }
        }

        private class TestStore : IStorageEngine
        {
            private readonly IDictionary<string, long> accounts;
            private readonly bool exception;

            public TestStore(IDictionary<string, long> accounts, bool exception)
            {
                this.accounts = accounts;
                this.exception = exception;
            }

            public IList<ByteString> AddedTransactions { get; private set; }

            public Task Initialize()
            {
                throw new NotImplementedException();
            }

            public Task AddTransactions(IEnumerable<ByteString> transactions)
            {
                this.AddedTransactions = transactions.ToList();

                if (this.exception)
                    throw new ConcurrentMutationException(new Record(ByteString.Empty, ByteString.Empty, ByteString.Empty));
                else
                    return Task.FromResult(0);
            }

            public Task<ByteString> GetLastTransaction()
            {
                throw new NotImplementedException();
            }

            public Task<IReadOnlyList<Record>> GetRecords(IEnumerable<ByteString> keys)
            {
                return Task.FromResult<IReadOnlyList<Record>>(keys.Select(key =>
                {
                    RecordKey recordKey = RecordKey.Parse(key);
                    return new Record(
                        key,
                        new ByteString(BitConverter.GetBytes(this.accounts[recordKey.Path.FullPath]).Reverse()),
                        ByteString.Empty);
                })
                .ToList());
            }

            public Task<IReadOnlyList<ByteString>> GetTransactions(ByteString from)
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            { }
        }
    }
}
