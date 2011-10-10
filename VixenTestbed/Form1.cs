﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Vixen.Sys;
using Vixen.Module.App;
using Vixen.Module.Editor;
using Vixen.Module.Effect;
using Vixen.Module.EffectEditor;
using Vixen.Module.Media;
using Vixen.Module.Output;
using Vixen.Module.Property;
using Vixen.Module.Timing;

namespace VixenTestbed {
	public partial class Form1 : Form, IApplication {
		private Guid _id = new Guid("{5E315C9B-1759-466c-A4E5-462EE750C708}");

		public Form1() {
			InitializeComponent();
			AppCommands = new AppCommand(this);
		}

		private void Form1_Load(object sender, EventArgs e) {
			Logging.ItemLogged += _ItemLogged;
			Vixen.Sys.VixenSystem.Start(this);

			moduleListApp.SetModuleType<IAppModuleInstance>();
			moduleListEditor.SetModuleType<IEditorModuleInstance>();
			moduleListEffect.SetModuleType<IEffectModuleInstance>();
			moduleListEffectEditor.SetModuleType<IEffectEditorModuleInstance>();
			moduleListMedia.SetModuleType<IMediaModuleInstance>();
			moduleListOutput.SetModuleType<IOutputModuleInstance>();
			moduleListProperty.SetModuleType<IPropertyModuleInstance>();
			moduleListTiming.SetModuleType<ITimingModuleInstance>();
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
			Logging.ItemLogged -= _ItemLogged;
			Vixen.Sys.VixenSystem.Stop();
		}

		public Guid ApplicationId {
			get { return _id; }
		}

		public IEditorUserInterface ActiveEditor {
			get { throw new NotImplementedException(); }
		}

		public IEditorUserInterface[] AllEditors {
			get { throw new NotImplementedException(); }
		}

		public AppCommand AppCommands { get; private set; }

		private void _ItemLogged(object sender, LogEventArgs e) {
			MessageBox.Show(e.Text, e.LogName);
		}

		private void buttonControllers_Click(object sender, EventArgs e) {
			using(ControllersForm controllersForm = new ControllersForm()) {
				controllersForm.ShowDialog();
			}
		}

		private void buttonChannels_Click(object sender, EventArgs e) {
			using(ChannelsForm channelsForm = new ChannelsForm()) {
				channelsForm.ShowDialog();
			}
		}

		private void buttonPatching_Click(object sender, EventArgs e) {
			using(PatchingForm patchingForm = new PatchingForm()) {
				patchingForm.ShowDialog();
			}
		}
	}
}
