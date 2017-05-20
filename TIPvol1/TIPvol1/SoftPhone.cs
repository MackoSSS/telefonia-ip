using Ozeki.Media.MediaHandlers;
using Ozeki.VoIP;
using Ozeki.VoIP.SDK;
using Ozeki.Common;
using Ozeki.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TIPvol1
{
    class SoftPhone
    {
        private ISoftPhone softPhone;
        private IPhoneLine phoneLine;

        private IPhoneCall call;
        private Microphone microphone;
        private Speaker speaker = Speaker.GetDefaultDevice();
        private MediaConnector connector;
        private PhoneCallAudioSender mediaSender;
        private PhoneCallAudioReceiver mediaReceiver;

        public event EventHandler IncomingCall;
        public event EventHandler<CallStateChangedArgs> CallStateChanged;
        public event EventHandler<RegistrationStateChangedArgs> PhoneLineStateChanged;

        private bool incomingCall;

        public SoftPhone()
        {
            softPhone = SoftPhoneFactory.CreateSoftPhone(5000, 10000);
            microphone = Microphone.GetDefaultDevice();
            speaker = Speaker.GetDefaultDevice();
            connector = new MediaConnector();
            mediaSender = new PhoneCallAudioSender();
            mediaReceiver = new PhoneCallAudioReceiver();

            incomingCall = false;
        }


        public static string GetIPAddress()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }

        //private void InitSoftPhone()
        //{
        //    softPhone = SoftPhoneFactory.CreateSoftPhone(SoftPhoneFactory.GetLocalIP(), 5700, 5750);
        //    //InvokeGUIThread(() => { ListPol.Items.Add("Softphone created!"); });

        //    softPhone.IncomingCall += new EventHandler<VoIPEventArgs<IPhoneCall>>(softphone_IncomingCall);

        //    SIPAccount sa = new SIPAccount(true, "1000", "1000", "1000", "1000", GetIPAddress(), 5060);
        //    //InvokeGUIThread(() => { ListPol.Items.Add("SIP account created! - " + sa.RegisterName); });

        //    phoneLine = softPhone.CreatePhoneLine(sa);
        //    phoneLine.RegistrationStateChanged += phoneLine_PhoneLineStateChanged;
        //    softPhone.RegisterPhoneLine(phoneLine);
        //}

        public void Register(bool registrationRequired, string displayName, string userName, string authenticadtionId, string registerPassword, string domainHost, int domainPort)
        {
            try
            {
                softPhone.IncomingCall += softphone_IncomingCall;

                var account = new SIPAccount(registrationRequired, displayName, userName, authenticadtionId, registerPassword, domainHost, domainPort);
                Console.WriteLine("Creating SIP account {0}", account);

                phoneLine = softPhone.CreatePhoneLine(account);
                Console.WriteLine("Phoneline created");

                phoneLine.RegistrationStateChanged += phoneLine_PhoneLineStateChanged;
                softPhone.RegisterPhoneLine(phoneLine);

                ConnectMedia();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during SIP registration " + ex);
            }
        }

        private void StartDevices()
        {
            if (microphone != null)
            {
                microphone.Start();
            }

            if (speaker != null)
            {
                speaker.Start();
            }
        }

        private void StopDevices()
        {
            if (microphone != null)
            {
                microphone.Stop();
            }

            if (speaker != null)
            {
                speaker.Stop();
            }
        }

        private void ConnectMedia()
        {
            if (microphone != null)
            {
                connector.Connect(microphone, mediaSender);
            }

            if (speaker != null)
            {
                connector.Connect(mediaReceiver, speaker);
            }
        }

        private void DisconnectMedia()
        {
            if (microphone != null)
            {
                connector.Disconnect(microphone, mediaSender);
            }

            if (speaker != null)
            {
                connector.Disconnect(mediaReceiver, speaker);
            }
        }

        private void WireUpCallEvents()
        {
            call.CallStateChanged += (call_CallStateChanged);
        }

        private void WireDownCallEvents()
        {
            call.CallStateChanged -= (call_CallStateChanged);
        }

        private void InvokeGUIThread(Action action)
        {
            action.Invoke();
        }

        private void call_CallStateChanged(object sender, CallStateChangedArgs e)
        {
            if (e.State == CallState.Answered)
            {
                StartDevices();

                mediaReceiver.AttachToCall(call);
                mediaSender.AttachToCall(call);
            }

            if (e.State == CallState.InCall)
            {
                StartDevices();
            }

            if (e.State.IsCallEnded())
            {
                StopDevices();
            }

            DispatchAsync(() =>
            {
                var handler = CallStateChanged;
                if (handler != null)
                    handler(this, e);
            });
        }

        public void StartCall(string numberToDial)
        {
            if (call == null)
            {
                call = softPhone.CreateCallObject(phoneLine, numberToDial);
                WireUpCallEvents();

                call.Start();
            }
        }

        public void AcceptCall()
        {
            if (incomingCall)
            {
                incomingCall = false;
                call.Answer();
            }
        }

        public void HangUp()
        {
            if (call != null)
            {
                call.HangUp();
                call = null;
            }
        }

        public void CallFinished()
        {
            StopDevices();

            mediaReceiver.Detach();
            mediaSender.Detach();

            WireDownCallEvents();

            call = null;
        }

        private void softphone_IncomingCall(object sender, VoIPEventArgs<IPhoneCall> e)
        {
            call = e.Item;
            WireUpCallEvents();
            incomingCall = true;

            DispatchAsync(() =>
            {
                var handler = IncomingCall;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            });
        }

        private void phoneLine_PhoneLineStateChanged(object sender, RegistrationStateChangedArgs e)
        {
            DispatchAsync(() =>
            {
                var handler = PhoneLineStateChanged;
                if (handler != null)
                    handler(this, e);
            });
        }

        //private void phoneLine_PhoneLineInformation(object sender, RegistrationStateChangedArgs e)
        //{

        //    InvokeGUIThread(() =>
        //    {
        //        if (e.State == RegState.RegistrationSucceeded)
        //        {
        //            this.Dispatcher.Invoke(() => { ListPol.Items.Add("Registration succeeded - Online"); });
        //        }
        //        else
        //        {
        //            ListPol.Items.Add("Not registered - Offline: " + e.State.ToString());
        //        }

        //    });
        //}

        private void DispatchAsync(Action action)
        {
            var task = new WaitCallback(o => action.Invoke());
            ThreadPool.QueueUserWorkItem(task);
        }
    }
}
