﻿using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio;
using Twilio.TwiML;
using Twilio.TwiML.Mvc;

namespace Queue_Demo.Controllers
{
    public class CallController : TwilioController
    {
        public ActionResult QueueCall()
        {
            var response = new TwilioResponse();

            response.Enqueue("Demo Queue", new {
                action= Url.Action("LeaveQueue"),       //url to call when the call is dequeued
                waitUrl = Url.Action("WaitInQueue")    //url to call while the call waits
            });

            return TwiML(response);
        }

        public ActionResult WaitInQueue(string CurrentQueueSize, string QueuePosition)
        {
            var response = new TwilioResponse();

            var context = GlobalHost.ConnectionManager.GetHubContext<Hubs.QueueHub>();
            context.Clients.All.reportQueueSize(CurrentQueueSize);

            response.Say(string.Format("You are number {0} in the queue.  Please hold.", QueuePosition));
            response.Play("http://demo.brooklynhacker.com/music/ramones.mp3");

            return TwiML(response);
        }

        public ActionResult LeaveQueue(string QueueSid)
        {
            var client = new TwilioRestClient(Queue_Demo.Settings.AccountSid, Queue_Demo.Settings.AuthToken);

            var queue  = client.GetQueue(QueueSid);

            if (queue.RestException == null)
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<Hubs.QueueHub>();
                context.Clients.All.reportQueueSize(queue.CurrentSize);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// Action Method that returns the TwiML needed to connect an 'Agent' to the first call in the Queue
        /// </summary>
        /// <remarks>This method also includes the 'url' parameter in the generated TwiML.  This allows you to provide a URL that can return TwiML that will be executed to the dequeued caller as a Whisper</remarks>
        /// <returns></returns>
        public ActionResult Dial()
        {
            var response = new TwilioResponse();
            response.DialQueue("Demo Queue", new { url = Url.Action("Connect") }, new { });

            return TwiML(response);
        }

        public ActionResult Connect()
        {
            var response = new TwilioResponse();
            response.Say("Connecting you to an agent");

            return TwiML(response);
        }
    }
}
