﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoneyFox.Business.ViewModels;
using MoneyFox.Foundation.DataModels;
using MoneyFox.Foundation.Interfaces;
using MoneyFox.Foundation.Interfaces.Repositories;
using Moq;
using MvvmCross.Platform.Core;
using MvvmCross.Test.Core;

namespace MoneyFox.Shared.Tests.ViewModels
{
    [TestClass]
    public class AccountListViewModelTests : MvxIoCSupportingTest
    {
        private Mock<IAccountRepository> accountRepository;

        [TestInitialize]
        public void Init()
        {
            MvxSingleton.ClearAllSingletons();
            accountRepository = new Mock<IAccountRepository>();
            accountRepository.SetupAllProperties();

            Setup();
        }

        [TestMethod]
        public void DeleteAccountCommand_UserReturnTrue_ExecuteDeletion()
        {
            var deleteCalled = false;

            accountRepository.Setup(x => x.Delete(It.IsAny<AccountViewModel>())).Callback(() => deleteCalled = true);

            var endofMonthManagerSetup = new Mock<IEndOfMonthManager>();
            var dialogServiceSetup = new Mock<IDialogService>();
            dialogServiceSetup.Setup(x => x.ShowConfirmMessage(It.IsAny<string>(), It.IsAny<string>(), null, null))
                .Returns(Task.FromResult(true));

            var settingsManagerMock = new Mock<ISettingsManager>();

            var viewModel = new AccountListViewModel(accountRepository.Object, new Mock<IPaymentManager>().Object,
                dialogServiceSetup.Object, endofMonthManagerSetup.Object, settingsManagerMock.Object);

            viewModel.DeleteAccountCommand.Execute(new AccountViewModel {Id = 3});

            deleteCalled.ShouldBeTrue();
        }

        [TestMethod]
        public void DeleteAccountCommand_UserReturnFalse_SkipDeletion()
        {
            var deleteCalled = false;
            accountRepository.Setup(x => x.Delete(It.IsAny<AccountViewModel>())).Callback(() => deleteCalled = true);
            var endofMonthManagerSetup = new Mock<IEndOfMonthManager>();

            var settingsManagerMock = new Mock<ISettingsManager>();

            var dialogServiceSetup = new Mock<IDialogService>();
            dialogServiceSetup.Setup(x => x.ShowConfirmMessage(It.IsAny<string>(), It.IsAny<string>(), null, null))
                .Returns(Task.FromResult(false));

            var viewModel = new AccountListViewModel(accountRepository.Object, new Mock<IPaymentManager>().Object,
                dialogServiceSetup.Object, endofMonthManagerSetup.Object, settingsManagerMock.Object);

            viewModel.DeleteAccountCommand.Execute(new AccountViewModel {Id = 3});

            deleteCalled.ShouldBeFalse();
        }

        [TestMethod]
        public void DeleteAccountCommand_AccountNull_DoNothing()
        {
            var deleteCalled = false;

            accountRepository.Setup(x => x.Delete(It.IsAny<AccountViewModel>())).Callback(() => deleteCalled = true);
            var endofMonthManagerSetup = new Mock<IEndOfMonthManager>();

            var dialogServiceSetup = new Mock<IDialogService>();
            dialogServiceSetup.Setup(x => x.ShowConfirmMessage(It.IsAny<string>(), It.IsAny<string>(), null, null))
                .Returns(Task.FromResult(true));

            var settingsManagerMock = new Mock<ISettingsManager>();

            var viewModel = new AccountListViewModel(accountRepository.Object, new Mock<IPaymentManager>().Object,
                dialogServiceSetup.Object, endofMonthManagerSetup.Object, settingsManagerMock.Object);

            viewModel.DeleteAccountCommand.Execute(null);

            deleteCalled.ShouldBeFalse();
        }

        [TestMethod]
        public void IsAllAccountsEmpty_AccountsEmpty_True()
        {
            var settingsManagerMock = new Mock<ISettingsManager>();
            var endofMonthManagerSetup = new Mock<IEndOfMonthManager>();
            accountRepository.Setup(x => x.GetList(null)).Returns(new List<AccountViewModel>());
            var vm = new AccountListViewModel(accountRepository.Object, new Mock<IPaymentManager>().Object, null,
                endofMonthManagerSetup.Object, settingsManagerMock.Object);
            vm.LoadedCommand.Execute();
            vm.IsAllAccountsEmpty.ShouldBeTrue();
        }

        [TestMethod]
        public void IsAllAccountsEmpty_OneAccount_False()
        {
            var settingsManagerMock = new Mock<ISettingsManager>();
            accountRepository.Setup(x => x.GetList(null)).Returns(new List<AccountViewModel> {
                new AccountViewModel()
            });
            var endofMonthManagerSetup = new Mock<IEndOfMonthManager>();
            var vm = new AccountListViewModel(accountRepository.Object, new Mock<IPaymentManager>().Object, null, endofMonthManagerSetup.Object, settingsManagerMock.Object);
            vm.LoadedCommand.Execute();
            vm.IsAllAccountsEmpty.ShouldBeFalse();
        }

        [TestMethod]
        public void IsAllAccountsEmpty_TwoAccount_False()
        {
            var settingsManagerMock = new Mock<ISettingsManager>();
            var endofMonthManagerSetup = new Mock<IEndOfMonthManager>();
            accountRepository.Setup(x => x.GetList(null)).Returns(new List<AccountViewModel> {
                new AccountViewModel(),
                new AccountViewModel()
            });

            var vm = new AccountListViewModel(accountRepository.Object, new Mock<IPaymentManager>().Object, null, endofMonthManagerSetup.Object, settingsManagerMock.Object);
            vm.LoadedCommand.Execute();
            vm.IsAllAccountsEmpty.ShouldBeFalse();
        }

        [TestMethod]
        public void AllAccounts_AccountsAvailable_MatchesRepository()
        {
            var settingsManagerMock = new Mock<ISettingsManager>();
            var endofMonthManagerSetup = new Mock<IEndOfMonthManager>();
            accountRepository.Setup(x => x.GetList(It.IsAny<Expression<Func<AccountViewModel, bool>>>())).Returns(new List<AccountViewModel>());
            accountRepository.Setup(x => x.GetList(null)).Returns(new List<AccountViewModel> {
                new AccountViewModel {Id = 22},
                new AccountViewModel{Id = 33},
            });
            var vm = new AccountListViewModel(accountRepository.Object, new Mock<IPaymentManager>().Object, null, endofMonthManagerSetup.Object, settingsManagerMock.Object);

            vm.LoadedCommand.Execute();
            vm.AllAccounts.Count.ShouldBe(2);
            vm.AllAccounts[0].Id.ShouldBe(22);
            vm.AllAccounts[1].Id.ShouldBe(33);
        }

        [TestMethod]
        public void AllAccounts_NoAccountsAvailable_MatchesRepository()
        {
            var settingsManagerMock = new Mock<ISettingsManager>();
            var endofMonthManagerSetup = new Mock<IEndOfMonthManager>();
            accountRepository.Setup(x => x.GetList(null)).Returns(new List<AccountViewModel>());
            accountRepository.Setup(x => x.GetList(It.IsAny<Expression<Func<AccountViewModel, bool>>>())).Returns(new List<AccountViewModel>());
            var vm = new AccountListViewModel(accountRepository.Object, new Mock<IPaymentManager>().Object, null, endofMonthManagerSetup.Object, settingsManagerMock.Object);
            vm.LoadedCommand.Execute();
            vm.AllAccounts.Any().ShouldBeFalse();
        }
    }
}