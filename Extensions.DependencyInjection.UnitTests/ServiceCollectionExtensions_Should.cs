﻿using System;
using Microsoft.Extensions.DependencyInjection;
using TestHelpers.Attributes;
using TheName.DistributedLocking.Abstractions.Factories;
using TheName.DistributedLocking.Extensions.DependencyInjection;
using Xunit;

namespace Extensions.DependencyInjection.UnitTests
{
    public class ServiceCollectionExtensions_Should
    {
        [Fact]
        public void Throw_When_RepositoryFactory_And_RepositoryManagerFactory_AreNotRegistered()
        {
            Assert.Throws<AggregateException>(() => new ServiceCollection()
                .AddDistributedLocking()
                .BuildServiceProvider(new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                }));
        }
        
        [Theory]
        [AutoMoqData]
        public void NotThrow_When_RepositoryFactory_And_RepositoryManagerFactory_AreRegistered(
            IDistributedLockRepositoryFactory repositoryFactory,
            IDistributedLockRepositoryManagerFactory repositoryManagerFactory)
        {
            _ = new ServiceCollection()
                .AddDistributedLocking()
                .AddSingleton(repositoryFactory)
                .AddSingleton(repositoryManagerFactory)
                .BuildServiceProvider(new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                });
        }
    }
}