using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ozeki.VoIP;
using Ozeki.Common;
using Ozeki.Media;
using Ozeki.Network;
using Ozeki.VoIP.SDK;
using Ozeki.Media.MediaHandlers;
using System.Net;
using System.Net.Sockets;


namespace TIPvol1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public class Contact
    {
        private string Login;
        private string Name;
        private string Surname;
        private string Sex;
        private string Domain;
        public string Contact_Login { get; set; }
        private bool Status = false;

        public Contact(string Login, string Name, string Surname, string Sex, string Domain)
        {
            this.Login = Login;
            Contact_Login = Login;
            this.Name = Name;
            this.Surname = Surname;
            this.Sex = Sex;
            this.Domain = Domain;
        }

        public bool getStatus()
        {
            return this.Status;
        }

        public void setStatus(bool IsAvailable)
        {
            this.Status = IsAvailable;
        }
    }

    public partial class MainWindow : Window
    {
        private bool IsCallIncomming = false;
        private bool IsCallBusy = false;
        //observablelist?
        private List<Contact> List_AllContacts = new List<Contact>();
        private List<Contact> List_FoundedContacts = new List<Contact>();

        private static SoftPhone mySoftPhone;
   
        public MainWindow()
        {
            InitializeComponent();
            Start();
            InitSoftphone();
            ReadRegisterInfos();

            BlockExit();
            

        }

        private void Start()
        {
            stackpanel_Menu.Visibility = System.Windows.Visibility.Hidden;
            border_ShowHideDetails.Visibility = System.Windows.Visibility.Hidden;
            GRID_Profile.Visibility = System.Windows.Visibility.Hidden;
            GRID_Status.Visibility = System.Windows.Visibility.Hidden;
            GRID_CallHistory.Visibility = System.Windows.Visibility.Hidden;
            GRID_About.Visibility = System.Windows.Visibility.Hidden;
            GRID_Contacts.Visibility = System.Windows.Visibility.Hidden;
            GRID_background.Visibility = System.Windows.Visibility.Hidden;
            GRID_Call.Visibility = System.Windows.Visibility.Hidden;       
            GRID_FloatingCall.Visibility = System.Windows.Visibility.Hidden;
            border_SelectedGrid.Visibility = System.Windows.Visibility.Hidden;
            GRID_splashscreen.Visibility = System.Windows.Visibility.Visible;

        }

        #region topdock
        private void btn_minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region menu
        private void border_SelectGrid(double TopMargin, System.Windows.Visibility Visibility)
        {
            border_SelectedGrid.Visibility = System.Windows.Visibility.Visible;
            border_SelectedGrid.Margin = new Thickness(0, TopMargin, 0, 0);
        }

        private void btn_ShowHideDetails_Click(object sender, RoutedEventArgs e)
        {
            if(border_ShowHideDetails.Visibility == System.Windows.Visibility.Visible)
            {
                border_ShowHideDetails.Visibility = System.Windows.Visibility.Hidden;
                GRID_Status.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                border_ShowHideDetails.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void btn_profile_Click(object sender, RoutedEventArgs e)
        {
            border_SelectGrid(120, System.Windows.Visibility.Visible);
            label_welcome.Visibility = System.Windows.Visibility.Hidden;
            btn_CallTo.Visibility = System.Windows.Visibility.Hidden;
            btn_DeleteContact.Visibility = System.Windows.Visibility.Hidden;
            btn_ContactsBack.Visibility = System.Windows.Visibility.Hidden;

            if(GRID_Profile.Visibility == System.Windows.Visibility.Visible)
            {
                if(btn_CallTo.Visibility == System.Windows.Visibility.Visible)
                {
                    /*btn_CallTo.Visibility = System.Windows.Visibility.Hidden;
                    btn_DeleteContact.Visibility = System.Windows.Visibility.Hidden;
                    btn_ContactsBack.Visibility = System.Windows.Visibility.Hidden;*/
                    //TODO void zmiana danych w profile na dane usera
                }
            }
            else 
            {
                GRID_Call.Visibility = System.Windows.Visibility.Hidden;
                GRID_CallHistory.Visibility = System.Windows.Visibility.Hidden;
                GRID_About.Visibility = System.Windows.Visibility.Hidden;
                GRID_AddContacts.Visibility = System.Windows.Visibility.Hidden;
                GRID_Profile.Visibility = System.Windows.Visibility.Visible;

                if(IsCallIncomming == true)
                {
                    GRID_FloatingCall.Visibility = System.Windows.Visibility.Visible;
                    label_Floating_IncommingCallContactName.Content = label_IncommingCallContactName.Content;
                    btn_Floating_Call_Answer.Visibility = System.Windows.Visibility.Visible;
                }
                else if(IsCallBusy == true)
                {
                    GRID_FloatingCall.Visibility = System.Windows.Visibility.Visible;
                    label_Floating_IncommingCallContactName.Content = label_IncommingCallContactName.Content;
                    btn_Floating_Call_Answer.Visibility = System.Windows.Visibility.Hidden;
                }

            }
        }

        private void btn_connectionstatus_Click(object sender, RoutedEventArgs e)
        {
           if(GRID_Status.Visibility == System.Windows.Visibility.Hidden)
           {
               border_ShowHideDetails.Visibility = System.Windows.Visibility.Visible;
               GRID_Status.Visibility = System.Windows.Visibility.Visible;
           }
           else
           {
               btn_HideStatus_Click(sender, e);
           }
        }

        private void btn_HideStatus_Click(object sender, RoutedEventArgs e)
        {
            border_ShowHideDetails.Visibility = System.Windows.Visibility.Visible;
            GRID_Status.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btn_callhistory_Click(object sender, RoutedEventArgs e)
        {
            border_SelectGrid(280, System.Windows.Visibility.Visible);
            label_welcome.Visibility = System.Windows.Visibility.Hidden;
           if(GRID_CallHistory.Visibility == System.Windows.Visibility.Hidden)
           {
               GRID_Call.Visibility = System.Windows.Visibility.Hidden;
               GRID_AddContacts.Visibility = System.Windows.Visibility.Hidden;
               GRID_Profile.Visibility = System.Windows.Visibility.Hidden;
               GRID_About.Visibility = System.Windows.Visibility.Hidden;
               GRID_CallHistory.Visibility = System.Windows.Visibility.Visible;

               if (IsCallIncomming == true)
               {
                   GRID_FloatingCall.Visibility = System.Windows.Visibility.Visible;
                   label_Floating_IncommingCallContactName.Content = label_IncommingCallContactName.Content;
                   btn_Floating_Call_Answer.Visibility = System.Windows.Visibility.Visible;
               }
               else if (IsCallBusy == true)
               {
                   GRID_FloatingCall.Visibility = System.Windows.Visibility.Visible;
                   label_Floating_IncommingCallContactName.Content = label_IncommingCallContactName.Content;
                   btn_Floating_Call_Answer.Visibility = System.Windows.Visibility.Hidden;
               }
           }
        }

        private void btn_about_Click(object sender, RoutedEventArgs e)
        {
            border_SelectGrid(360, System.Windows.Visibility.Visible);
            label_welcome.Visibility = System.Windows.Visibility.Hidden;
            if(GRID_About.Visibility == System.Windows.Visibility.Hidden)
            {
                GRID_Call.Visibility = System.Windows.Visibility.Hidden;
                GRID_AddContacts.Visibility = System.Windows.Visibility.Hidden;
                GRID_Profile.Visibility = System.Windows.Visibility.Hidden;
                GRID_CallHistory.Visibility = System.Windows.Visibility.Hidden;
                GRID_About.Visibility = System.Windows.Visibility.Visible;

                if (IsCallIncomming == true)
                {
                    GRID_FloatingCall.Visibility = System.Windows.Visibility.Visible;
                    label_Floating_IncommingCallContactName.Content = label_IncommingCallContactName.Content;
                    btn_Floating_Call_Answer.Visibility = System.Windows.Visibility.Visible;
                }
                else if (IsCallBusy == true)
                {
                    GRID_FloatingCall.Visibility = System.Windows.Visibility.Visible;
                    label_Floating_IncommingCallContactName.Content = label_IncommingCallContactName.Content;
                    btn_Floating_Call_Answer.Visibility = System.Windows.Visibility.Hidden;
                }
            }
        }

        private void btn_logout_Click(object sender, RoutedEventArgs e)             //TODO
        {
            if (IsCallBusy == true)
            {
                if (MessageBox.Show("Wylogowanie spowoduje przerwanie połączenia, czy chcesz kontynuować?", "Question", MessageBoxButton.YesNo, MessageBoxImage.None) == MessageBoxResult.Yes)
                {
                    btn_Call_HangUp_Click(sender, e);
                    Start();
                }
            }
            else
            {
                Start();
            }
        }
        #endregion

        #region call
        private void btn_Call_Answer_Click(object sender, RoutedEventArgs e)
        {
            //todo połączenie
            border_SelectGrid(40, System.Windows.Visibility.Hidden);
            btn_Call_HangUp.Margin = new Thickness(0, 0, 0, 100);
            btn_Call_MuteMicro.Visibility = System.Windows.Visibility.Visible;
            btn_Call_Hang.Visibility = System.Windows.Visibility.Visible;
            btn_Call_Volume.Visibility = System.Windows.Visibility.Visible;
            btn_Floating_Call_Answer.Visibility = System.Windows.Visibility.Hidden;
            btn_Call_Answer.Visibility = System.Windows.Visibility.Hidden;
            label_CallDuration.Visibility = System.Windows.Visibility.Visible;
        }

        private void btn_Call_HangUp_Click(object sender, RoutedEventArgs e)
        {
            border_SelectGrid(40, System.Windows.Visibility.Hidden);
            btn_Call_HangUp.Margin = new Thickness(200, 0, 0, 100);
            btn_Call_MuteMicro.Visibility = System.Windows.Visibility.Hidden;
            btn_Call_Hang.Visibility = System.Windows.Visibility.Hidden;
            btn_Call_Volume.Visibility = System.Windows.Visibility.Hidden;
            btn_Floating_Call_Answer.Visibility = System.Windows.Visibility.Visible;
            btn_Call_Answer.Visibility = System.Windows.Visibility.Visible;
            label_CallDuration.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btn_Call_Hang_Click(object sender, RoutedEventArgs e)
        {
            //todo zawieś połączenie
        }

        private void btn_Call_MuteMicro_Click(object sender, RoutedEventArgs e)
        {
            //todo MUTE
            btn_Call_MuteMicro.Visibility = System.Windows.Visibility.Hidden;
            btn_Call_UnmuteMicro.Visibility = System.Windows.Visibility.Visible;
        }

        private void btn_Call_UnmuteMicro_Click(object sender, RoutedEventArgs e)
        {
            //todo UNMUTE
            btn_Call_UnmuteMicro.Visibility = System.Windows.Visibility.Hidden;
            btn_Call_MuteMicro.Visibility = System.Windows.Visibility.Visible;
        }

        private void btn_Call_Volume_Click(object sender, RoutedEventArgs e)
        {
            if(slider_Volume.Visibility == System.Windows.Visibility.Hidden)
            {
                slider_Volume.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                slider_Volume.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void slider_Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //to do głośnosć
        }


        private void btn_CallTo_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region contacts
        private void btn_AddContact_Click(object sender, RoutedEventArgs e)
        {
            if (GRID_AddContacts.Visibility == System.Windows.Visibility.Hidden)
            {
                GRID_Profile.Visibility = System.Windows.Visibility.Hidden;
                GRID_CallHistory.Visibility = System.Windows.Visibility.Hidden;
                GRID_About.Visibility = System.Windows.Visibility.Hidden;
                GRID_Call.Visibility = System.Windows.Visibility.Hidden;
                GRID_AddContacts.Visibility = System.Windows.Visibility.Visible;

                if (IsCallIncomming == true)
                {
                    GRID_FloatingCall.Visibility = System.Windows.Visibility.Visible;
                    label_Floating_IncommingCallContactName.Content = label_IncommingCallContactName.Content;
                    btn_Floating_Call_Answer.Visibility = System.Windows.Visibility.Visible;
                }
                else if (IsCallBusy == true)
                {
                    GRID_FloatingCall.Visibility = System.Windows.Visibility.Visible;
                    label_Floating_IncommingCallContactName.Content = label_IncommingCallContactName.Content;
                    btn_Floating_Call_Answer.Visibility = System.Windows.Visibility.Hidden;
                }
            }
        }

        private void btn_DeleteContact_Click(object sender, RoutedEventArgs e)
        {
            //todo delete
        }

        private void btn_ContactsBack_Click(object sender, RoutedEventArgs e)
        {
            GRID_Profile.Visibility = System.Windows.Visibility.Hidden;
            GRID_CallHistory.Visibility = System.Windows.Visibility.Hidden;
            GRID_About.Visibility = System.Windows.Visibility.Hidden;
            GRID_Call.Visibility = System.Windows.Visibility.Hidden;
            GRID_AddContacts.Visibility = System.Windows.Visibility.Visible;

            if (IsCallIncomming == true)
            {
                GRID_FloatingCall.Visibility = System.Windows.Visibility.Visible;
                label_Floating_IncommingCallContactName.Content = label_IncommingCallContactName.Content;
                btn_Floating_Call_Answer.Visibility = System.Windows.Visibility.Visible;
            }
            else if (IsCallBusy == true)
            {
                GRID_FloatingCall.Visibility = System.Windows.Visibility.Visible;
                label_Floating_IncommingCallContactName.Content = label_IncommingCallContactName.Content;
                btn_Floating_Call_Answer.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void listbox_contacts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btn_ContactsBack.Visibility = System.Windows.Visibility.Hidden;

            if(GRID_Profile.Visibility == System.Windows.Visibility.Visible)
            {
                //TODO void zmiana danych na dane wybranego zioma z listy

                if ((IsCallBusy == true) || (IsCallIncomming == true))
                    btn_CallTo.Visibility = System.Windows.Visibility.Hidden;
                else
                    btn_CallTo.Visibility = System.Windows.Visibility.Visible;

                btn_DeleteContact.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                GRID_AddContacts.Visibility = System.Windows.Visibility.Hidden;
                GRID_Call.Visibility = System.Windows.Visibility.Hidden;
                GRID_About.Visibility = System.Windows.Visibility.Hidden;
                GRID_CallHistory.Visibility = System.Windows.Visibility.Hidden;
                GRID_Profile.Visibility = System.Windows.Visibility.Visible;

                if (IsCallIncomming == true)
                {
                    GRID_FloatingCall.Visibility = System.Windows.Visibility.Visible;
                    label_Floating_IncommingCallContactName.Content = label_IncommingCallContactName.Content;
                    btn_Floating_Call_Answer.Visibility = System.Windows.Visibility.Visible;
                }
                else if (IsCallBusy == true)
                {
                    GRID_FloatingCall.Visibility = System.Windows.Visibility.Visible;
                    label_Floating_IncommingCallContactName.Content = label_IncommingCallContactName.Content;
                    btn_Floating_Call_Answer.Visibility = System.Windows.Visibility.Hidden;
                }
            }
        }

        private void listbox_SearchContacts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            GRID_AddContacts.Visibility = System.Windows.Visibility.Hidden;
            GRID_Call.Visibility = System.Windows.Visibility.Hidden;
            GRID_About.Visibility = System.Windows.Visibility.Hidden;
            GRID_CallHistory.Visibility = System.Windows.Visibility.Hidden;
            GRID_Profile.Visibility = System.Windows.Visibility.Visible;
            btn_ContactsBack.Visibility = System.Windows.Visibility.Visible;
        }

        private void textbox_SearchContacts_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                //todo search
            }
        }
        #endregion

        #region login
        private void btn_LogIn_Click(object sender, RoutedEventArgs e)
        {
            //todo zaloguj

            GRID_splashscreen.Visibility = System.Windows.Visibility.Hidden;
            GRID_background.Visibility = System.Windows.Visibility.Visible;
            GRID_Contacts.Visibility = System.Windows.Visibility.Visible;
            label_welcome.Visibility = System.Windows.Visibility.Visible;
            label_welcome.Content = "Witaj " + textbox_login.Text + "!";
            stackpanel_Menu.Visibility = System.Windows.Visibility.Visible;
            border_ShowHideDetails.Visibility = System.Windows.Visibility.Visible;
        }

        private void btn_RegistryShow_Click(object sender, RoutedEventArgs e)
        {
            GRID_NewAccount.Visibility = System.Windows.Visibility.Visible;
            btn_LogIn.Visibility = System.Windows.Visibility.Hidden;
            btn_RegistryShow.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btn_NewAccount_Click(object sender, RoutedEventArgs e)
        {
            //todo utwórz konto
            //btn_LogIn_Click(sender, e);

            GRID_splashscreen.Visibility = System.Windows.Visibility.Hidden;
            GRID_background.Visibility = System.Windows.Visibility.Visible;
            GRID_Contacts.Visibility = System.Windows.Visibility.Visible;
            label_welcome.Visibility = System.Windows.Visibility.Visible;
            label_welcome.Content = "Witaj " + textbox_login.Text + "!";
        }

        private void btn_NewAccountCancel_Click(object sender, RoutedEventArgs e)
        {
            GRID_NewAccount.Visibility = System.Windows.Visibility.Hidden;
            textbox_name.Text = "imię";
            textbox_surname.Text = "nazwisko";
            btn_LogIn.Visibility = System.Windows.Visibility.Visible;
            btn_RegistryShow.Visibility = System.Windows.Visibility.Visible;
        }
        #endregion

        #region softphone
        private static void InitSoftphone()
        {
            mySoftPhone = new SoftPhone();
            mySoftPhone.PhoneLineStateChanged += mySoftPhone_PhoneLineStateChanged;
            mySoftPhone.CallStateChanged += mySoftPhone_CallStateChanged;
            mySoftPhone.IncomingCall += mySoftPhone_IncomingCall;
        }

        private static void mySoftPhone_IncomingCall(object sender, EventArgs e)
        {
            Console.WriteLine("\nIncoming call!");
            Console.WriteLine("Call accepted");
            mySoftPhone.AcceptCall();
        }

        private static void mySoftPhone_CallStateChanged(object sender, CallStateChangedArgs e)
        {
            Console.WriteLine("Call state changed: {0}", e.State);

            if(e.State.IsCallEnded())
            {
                StartToDial();
            }
            if(e.State == CallState.Error)
            {
                Console.WriteLine("Call error occured. {0}", e.Reason);
            }
        }

        private static void mySoftPhone_PhoneLineStateChanged(object sender, RegistrationStateChangedArgs e)
        {
            Console.WriteLine("Phone line state changed: {0}", e.State);

            if(e.State == RegState.Error || e.State == RegState.NotRegistered)
            {
                ReadRegisterInfos();
            }
            else if (e.State == RegState.RegistrationSucceeded)
            {
                Console.WriteLine("Registration succesed - ONLINE");

                StartToDial();
            }
        }

        private static void StartToDial()
        {
            Console.WriteLine("\nTo start a call, type the number and press Enter: ");
            string numberToDial = Console.ReadLine();

            while(string.IsNullOrEmpty(numberToDial))
            {
                numberToDial = Console.ReadLine();
            }
            mySoftPhone.StartCall(numberToDial);
        }

        private static void ReadRegisterInfos()
        {
            bool registrationRequired = true;
            Console.WriteLine("\nPlease set up Your SIP account: \n");

            Console.WriteLine("Please set your authentication ID: ");
            var authenticationId = Read("Authentication ID", true);

            Console.WriteLine("Please set Your username: ");
            var userName = Read("Username", false);

            Console.WriteLine("Please set your name to be displayed: ");
            var displayName = Read("Display name", false);

            Console.WriteLine("Please set Your registration password: ");
            var registerPassword = Read("Password", true);

            Console.WriteLine("Please set the domain name: ");
            var domainHost = Read("Domain name", true);

            Console.WriteLine("\nCreating SIP account and trying to register....\n");
            mySoftPhone.Register(registrationRequired, displayName, userName, authenticationId, registerPassword,
                                 domainHost, 5060); 
        }

        private static string Read(string inputName, bool readWhileEmpty)
        {
            while (true)
            {
                string input = Console.ReadLine();

                if (!readWhileEmpty)
                    return input;

                if (!string.IsNullOrEmpty(input))
                    return input;

                Console.WriteLine(inputName + " cannot be empty.");
                Console.Write(inputName + ": ");
            }
        }

        private static void BlockExit()
        {
            while (true)
            {
                Thread.Sleep(10);
            }
        }
        #endregion

       


    }
}
