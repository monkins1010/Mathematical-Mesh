﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Threading;
using Goedel.Trojan;

namespace Goedel.MeshSampleClient {

    public partial class _Data_EmailDialog :Goedel.Trojan.Data  {
		public Dialog_EmailDialog Dialog;

		public override Page Page {
		    get { return Dialog; }
			}

		protected SampleApplication _Data;
		public SampleApplication Data {
			get {return _Data;}
			}


		public void Refresh () {
			if (Dialog != null) {
				Dialog.Refresh ();
				}
			}

//		protected Wizard_SampleApplication  _Wizard;	
//		public Wizard_SampleApplication  Wizard {
//				get {return _Wizard;}
//				}
//		public Data_SampleApplication  Data {
//				get {return Wizard.Data;}
//				}

		// Input backing variables
		string _Input_MailAccount;
		public string Input_MailAccount {
            get { return _Input_MailAccount; }
            set { _Input_MailAccount = value;  Refresh (); }
            }
		string _Input_MailServer;
		public string Input_MailServer {
            get { return _Input_MailServer; }
            set { _Input_MailServer = value;  Refresh (); }
            }

		// Output backing variables



		/// <summary>
		/// Here goes the action to be overriden
		/// </summary>

		public virtual bool AcceptEmail () {
			return true;
			}

		/// <summary>
		/// Here goes the action to be overriden
		/// </summary>

		public virtual bool CancelEmail2 () {
			return true;
			}


		}

    public partial class EmailDialog : _Data_EmailDialog {

		
		public EmailDialog (SampleApplication  Data) {
			_Data = Data;

			// NB call to the initializer before we creaate the dialog so the
			// dialog can display the initialized data.
			Initialize ();
			this.Dialog = new Dialog_EmailDialog (this);
			}
		}


    /// <summary>
	/// This is the code behind for the XAML generated class.
    /// </summary>
    public partial class Dialog_EmailDialog : Page {

		public EmailDialog  Data;


		public Dialog_EmailDialog (EmailDialog Data) {
			InitializeComponent();
			this.Data = Data;
			Refresh ();

			}

		public void Refresh () {
			Input_MailAccount.Text  = Data.Input_MailAccount;
			Input_MailServer.Text  = Data.Input_MailServer;
			}

        private void Action_AcceptEmail(object sender, RoutedEventArgs e) {
			var Result = Data.AcceptEmail ();
			if (Result) {
				Data.Data.Navigate (Data.Data.Data_SelectApplication);
				}
            }
        private void Action_CancelEmail2(object sender, RoutedEventArgs e) {
			var Result = Data.CancelEmail2 ();
			if (Result) {
				Data.Data.Navigate (Data.Data.Data_SelectApplication);
				}
            }


		private void Changed_Input_MailAccount (object sender, TextChangedEventArgs e) {
			Data.Input_MailAccount = Input_MailAccount.Text;
			}
		private void Changed_Input_MailServer (object sender, TextChangedEventArgs e) {
			Data.Input_MailServer = Input_MailServer.Text;
			}

		}
	}
