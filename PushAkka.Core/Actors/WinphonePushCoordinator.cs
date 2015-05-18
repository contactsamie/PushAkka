using System;
using System.Collections.Generic;
using System.Diagnostics;
using Akka.Actor;
using Akka.Monitoring;
using Akka.Routing;
using PushAkka.Core.Messages;

namespace PushAkka.Core.Actors
{
    public class WinphonePushCoordinator : BaseReceiveActor
    {
        private readonly IActorRef _whoWaitToReply;
        private IActorRef _winPhonePushRouter;

        public WinphonePushCoordinator(IActorRef whoWaitToReply)
        {
            _whoWaitToReply = whoWaitToReply;
            Receive<BaseWindowsPhonePushMessage>(push =>
            {
                _winPhonePushRouter.Tell(push);
                Context.IncrementCounter("windows_phone_push_notification");
            });

            Receive<NotificationResult>(result =>
            {
                Context.IncrementCounter(result.IsSuccess ? "success_push_notification" : "failed_push_notification");
                Info("Message \"{0}\" sent: {1}", result.Id, result.IsSuccess);
            });
        }

        protected override void Unhandled(object message)
        {
            Info("Unhandled: " + message);
            Context.IncrementUnhandledMessage();
            base.Unhandled(message);
        }

        protected override void PreStart()
        {
            _winPhonePushRouter = Context
                .ActorOf(Props.Create<WindowsPhonePushActor>(_whoWaitToReply)
                .WithRouter(FromConfig.Instance.WithFallback(new NoRouter())),
                "WP_PushActor");
            base.PreStart();
        }
    }
}