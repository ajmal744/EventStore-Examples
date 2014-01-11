﻿using System;
using EventStore.ClientAPI;
using Soloco.EventStore.Test.MeasurementProjections.Events;
using Soloco.EventStore.Test.MeasurementProjections.Infrastructure;

namespace Soloco.EventStore.Test.MeasurementProjections.Queries
{
    public class MeasurementReadCounterQuery
    {
        private readonly IColorConsole _console;
        private readonly IEventStoreConnection _connection;
        private readonly IProjectionContext _projectionContext;

        public MeasurementReadCounterQuery(IEventStoreConnection connection, IProjectionContext projectionContext, IColorConsole console)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (console == null) throw new ArgumentNullException("console");

            _connection = connection;
            _projectionContext = projectionContext;
            _console = console;
        }

        public MeasurementReadCounter GetValue()
        {
            return _projectionContext.GetState<MeasurementReadCounter>("MeasurementReadCounter");
        }

        public void SubscribeValueChange(Action<MeasurementReadCounter> valueChanged)
        {
            _connection.SubscribeToStream(
                "$projections-MeasurementReadCounter-result", 
                false, 
                (subscription, resolvedEvent) => ValueChanged(subscription, resolvedEvent, valueChanged), 
                Dropped, 
                EventStoreCredentials.Default);
        }

        private void Dropped(EventStoreSubscription subscription, SubscriptionDropReason subscriptionDropReason, Exception exception)
        {
            _console.Error("Subscription {0} dropped: {1} (Currently no recovery implemented){2}{3}", subscription.StreamId, subscriptionDropReason, Environment.NewLine, exception);
        }

        private void ValueChanged(EventStoreSubscription eventStoreSubscription, ResolvedEvent resolvedEvent, Action<MeasurementReadCounter> valueChanged)
        {
            var value = resolvedEvent.ParseJson<MeasurementReadCounter>();
            valueChanged(value);
        }
    }
}