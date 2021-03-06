﻿using Windows.ApplicationModel.Background;
using Cheesebaron.MvxPlugins.Settings.WindowsCommon;
using MoneyFox.Business.Manager;
using MoneyFox.DataAccess;
using MoneyFox.DataAccess.Repositories;
using MvvmCross.Plugins.File.WindowsCommon;
using MvvmCross.Plugins.Sqlite.WindowsUWP;

namespace MoneyFox.Windows.Tasks
{
    public sealed class RecurringPaymentTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            try
            {
                var dbManager = new DatabaseManager(new WindowsSqliteConnectionFactory(),
                    new MvxWindowsCommonFileStore());

                var paymentRepository = new PaymentRepository(dbManager);

                var paymentManager = new PaymentManager(paymentRepository,
                    new AccountRepository(dbManager),
                    new RecurringPaymentRepository(dbManager),
                    null);

                new RecurringPaymentManager(paymentManager, paymentRepository,
                    new SettingsManager(new WindowsCommonSettings())).CheckRecurringPayments();
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}