using System;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Diagnostics;

namespace dependentPropertyTest
{
	public partial class InnerView : ContentView
	{
		public InnerView ()
		{
			InitializeComponent ();
			BindingContext = this;
			ButtonText = "Start speaking";
		}

		private string noteText;

		public string NoteText {
			get {
				return noteText;
			}
			set {
				if (value == null)
					value = noteText;

				noteText = value;
				OnPropertyChanged ();
			}
		}

		private bool recording;

		public bool Recording {
			get {
				return recording;
			}
			set {
				recording = value;
				OnPropertyChanged ();
			}	
		}

		private bool isBusy;

		public bool IsBusy {
			get {
				return isBusy;
			}
			set {
				isBusy = value;
				OnPropertyChanged ();
			}	
		}


		private string buttonText = string.Empty;

		public string ButtonText {
			get {
				return buttonText;
			}
			set {
				if (value == null)
					value = buttonText;

				buttonText = value;
				OnPropertyChanged ();
			}
		}

		private async  void OnSend (object sender, EventArgs e)
		{
			await RecordSpeech ();
		}

		private async Task RecordSpeech ()
		{
			
			if (IsBusy)
				return;

			IsBusy = true;

			try {
				if (!Recording) {
					DependencyService.Get<IAudioRecorderService> ().StartRecording ();

					Recording = !Recording;
					ButtonText = "Stop speaking";
					NoteText = string.Empty;
				} else {
					DependencyService.Get<IAudioRecorderService> ().StopRecording ();

					Recording = !Recording;

					//UserDialogs.Instance.ShowLoading("Converting Speech to Text");
					var speechToText = await BingSpeechApi.SpeechToText ();
					NoteText = speechToText.Lexical;
					//UserDialogs.Instance.HideLoading ();
					ButtonText = "Start speaking";
				}
			} catch (Exception ex) {
				Debug.WriteLine (ex.Message + " \n Exception: " + ex.StackTrace);
			} finally {
				IsBusy = false;
			}

		}
	}
}

