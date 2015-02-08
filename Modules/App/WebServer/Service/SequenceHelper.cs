﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Vixen.Execution;
using Vixen.Execution.Context;
using Vixen.Services;
using Vixen.Sys;
using VixenModules.App.WebServer.Model;

namespace VixenModules.App.WebServer.Service
{
	internal class SequenceHelper
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();
		//private static ISequenceContext _context; 

		public static IEnumerable<Sequence> GetSequences()
		{
			var sequenceNames = SequenceService.Instance.GetAllSequenceFileNames().Select(Path.GetFileName);
			var sequences = sequenceNames.Select(sequenceName => new Sequence
			{
				Name = Path.GetFileNameWithoutExtension(sequenceName),
				FileName = sequenceName
			}).ToList();

			return sequences;
		}

		/// <summary>
		/// Play a sequence of a given file name.
		/// </summary>
		/// <param name="sequence"></param>
		/// <returns></returns>
		public static ContextStatus PlaySequence(Sequence sequence){
			
			var status = new ContextStatus();

			if (sequence == null)
			{
				throw new ArgumentNullException("sequence");
			}

			IEnumerable<IContext> contexts = VixenSystem.Contexts.Where(x => x.Name.Equals(sequence.Name) && (x.IsRunning || x.IsPaused));

			if (contexts.Any(x => x.IsPaused))
			{
				foreach (var context in contexts)
				{
					if (context.IsPaused)
					{
						context.Resume();

					}
				}
				status.Message = string.Format("Resumed {0}", sequence.Name);

			}
			else
			{
				string fileName = HttpUtility.UrlDecode(sequence.FileName);

				ISequence seq = SequenceService.Instance.Load(fileName);
				if (seq != null)
				{
					Logging.Info(string.Format("Web - Prerendering effects for sequence: {0}", sequence.Name));
					Parallel.ForEach(seq.SequenceData.EffectData.Cast<IEffectNode>(), effectNode => effectNode.Effect.PreRender());

					ISequenceContext context = VixenSystem.Contexts.CreateSequenceContext(
						new ContextFeatures(ContextCaching.NoCaching), seq);
					context.ContextEnded += context_ContextEnded;
					context.Play(TimeSpan.Zero, seq.Length);
					status.State = ContextStatus.States.Playing;
					status.Sequence = new Sequence()
					{
						Name = context.Sequence.Name,
						FileName = fileName
					};
					
					status.Message = string.Format("Playing sequence {0} of length {1}", sequence.Name, seq.Length);
				}
				else
				{
					status.Message = string.Format("Sequence {0} not found.", fileName);
				}
				
				
			}
			

			return status;
		}

		private static void context_ContextEnded(object sender, EventArgs e)
		{
			var context = sender as IContext;
			if (context != null)
			{
				context.ContextEnded -= context_ContextEnded;
				VixenSystem.Contexts.ReleaseContext(context);
			}

		}

		/// <summary>
		/// Pause the current playing sequence.
		/// </summary>
		/// <returns>Status</returns>
		public static ContextStatus PauseSequence(Sequence sequence)
		{
			var status = new ContextStatus();
			IEnumerable<IContext> contexts = VixenSystem.Contexts.Where(x => x.Name.Equals(sequence.Name) && (x.IsRunning || x.IsPaused));

			if (!contexts.Any())
			{
				status.Message = "Sequence not found.";
			}
			else
			{
				foreach (var context in contexts)
				{
					if (context.IsRunning)
					{
						context.Pause();
					}
				}
				status.Message = string.Format(@"Sequence {0} paused.", sequence.Name);
			}
			//if (_context != null && _context.IsRunning)
			//{
			//	status.State = ContextStatus.States.Paused;
			//	status.Message = string.Format("{0} paused.", _context.Sequence.Name);
			//	_context.Pause();
			//}
			//else
			//{
			//	status.State = ContextStatus.States.Stopped;
			//	status.Message = "Nothing playing.";
			//}
			return status;
		}

		/// <summary>
		/// Stop the current playing sequnce. Does not effect scheduled sequences.
		/// </summary>
		/// <returns></returns>
		public static ContextStatus StopSequence(Sequence sequence)
		{
			var status = new ContextStatus()
			{
				Sequence = sequence,
				State = ContextStatus.States.Stopped
			};

			IEnumerable<IContext> contexts = VixenSystem.Contexts.Where(x => x.Name.Equals(sequence.Name) && (x.IsRunning || x.IsPaused));

			if (!contexts.Any())
			{
				status.Message = "Sequence not found.";
			}
			else
			{
				foreach (var context in contexts)
				{
					context.Stop();
				}
				status.Message = string.Format(@"Sequence {0} stopped.", sequence.Name);
			}
			//if (_context != null && _context.IsRunning)
			//{
			//	status.Message = string.Format("Stopped {0}", _context.Sequence.Name);
			//	_context.Stop();
			//}
			//else
			//{
			//	status.Message = "Nothing playing.";
			//}
			return status;
		}

		/// <summary>
		/// Retrieve the status of any sequences playing. 
		/// </summary>
		/// <returns>Status</returns>
		public static ContextStatus Status()
		{
			var status = new ContextStatus();
			
			//if (_context != null && _context.IsRunning)
			//{
			//	status.Name = _context.Sequence.Name;
			//	status.State = ContextStatus.States.Playing;
			//	status.Message = string.Format("{0} sequence is playing at position {1}", _context.Sequence.Name,
			//		_context.GetTimeSnapshot());
			//}
			//else
			//{
				//status.State = ContextStatus.States.Stopped;
				status.Message = "Deprecated";
			//}

			return status;
		}
	}
}
