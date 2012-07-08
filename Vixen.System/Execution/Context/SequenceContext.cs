﻿using System;
using Vixen.Sys;

namespace Vixen.Execution.Context {
	abstract class SequenceContext : ContextBase, ISequenceContext {
		private ISequenceExecutor _sequenceExecutor;
		private ISequence _sequence;

		public event EventHandler<SequenceStartedEventArgs> SequenceStarted;
		public event EventHandler<SequenceEventArgs> SequenceEnded;
		public event EventHandler<ExecutorMessageEventArgs> Message;
		public event EventHandler<ExecutorMessageEventArgs> Error;

		public ISequence Sequence {
			get { return _sequence; }
			set {
				_sequence = value;
				_AssignSequenceToExecutor();
			}
		}

		public void Play(TimeSpan startTime, TimeSpan endTime) {
			if(_sequenceExecutor == null) throw new InvalidOperationException("Attempt to start a sequence without an executor.");
			_sequenceExecutor.Play(startTime, endTime);
		}

		public override string Name {
			get { return (_sequenceExecutor != null) ? _sequenceExecutor.Name : null; }
		}

		public override bool IsRunning {
			get { return _IsSequenceExecutorRunning(); }
		}

		public override bool IsPaused {
			get { return _IsSequenceExecutorPaused(); }
		}

		override public IExecutor Executor {
			set {
				if(_sequenceExecutor != null) {
					_DisposeSequenceExecutor();
				}

				if(!(value is ISequenceExecutor)) {
					throw new InvalidOperationException("Attempt to use a non-sequence executor with a sequence context.");
				}

				_sequenceExecutor = (ISequenceExecutor)value;

				if(_sequenceExecutor != null) {
					_AssignSequenceToExecutor();
					_AssignEventHandlers();
				}
			}
		}

		protected override void _OnStart() {
			_sequenceExecutor.Start();
		}

		protected override void _OnPause() {
			_sequenceExecutor.Pause();
		}

		protected override void _OnResume() {
			_sequenceExecutor.Resume();
		}

		protected override void _OnStop() {
			_sequenceExecutor.Stop();
		}

		protected override Module.Timing.ITiming _SequenceTiming {
			get { return _sequenceExecutor != null ? _sequenceExecutor.TimingSource : null; }
		}

		private void _AssignSequenceToExecutor() {
			if(_sequenceExecutor != null) {
				_sequenceExecutor.Sequence = Sequence;
			}
		}

		private void _AssignEventHandlers() {
			_sequenceExecutor.SequenceStarted += _SequenceExecutorSequenceStarted;
			_sequenceExecutor.SequenceEnded += _SequenceExecutorSequenceEnded;
			_sequenceExecutor.Message += _SequenceExecutorMessage;
			_sequenceExecutor.Error += _SequenceExecutorError;
		}

		private void _RemoveEventHandlers() {
			_sequenceExecutor.SequenceStarted -= _SequenceExecutorSequenceStarted;
			_sequenceExecutor.SequenceEnded -= _SequenceExecutorSequenceEnded;
			_sequenceExecutor.Message -= _SequenceExecutorMessage;
			_sequenceExecutor.Error -= _SequenceExecutorError;
		}

		private void _DisposeSequenceExecutor() {
			if(_sequenceExecutor != null) {
				_RemoveEventHandlers();
				_sequenceExecutor.Dispose();
				_sequenceExecutor = null;
			}
		}

		private bool _IsSequenceExecutorRunning() {
			return _sequenceExecutor != null && _sequenceExecutor.IsRunning;
		}

		private bool _IsSequenceExecutorPaused() {
			return _sequenceExecutor != null && _sequenceExecutor.IsPaused;
		}

		#region Events
		private void _SequenceExecutorSequenceStarted(object sender, SequenceStartedEventArgs e) {
			OnContextStarted(EventArgs.Empty);
			OnSequenceStarted(e);
		}

		protected virtual void OnSequenceStarted(SequenceStartedEventArgs e) {
			if(SequenceStarted != null) {
				SequenceStarted(this, e);
			}
		}

		private void _SequenceExecutorSequenceEnded(object sender, SequenceEventArgs e) {
			OnSequenceEnded(e);
			OnContextEnded(EventArgs.Empty);
		}

		protected virtual void OnSequenceEnded(SequenceEventArgs e) {
			if(SequenceEnded != null) {
				SequenceEnded(this, e);
			}
		}

		private void _SequenceExecutorMessage(object sender, ExecutorMessageEventArgs e) {
			OnMessage(e);
		}

		protected virtual void OnMessage(ExecutorMessageEventArgs e) {
			if(Message != null) {
				Message(this, e);
			}
		}

		private void _SequenceExecutorError(object sender, ExecutorMessageEventArgs e) {
			OnError(e);
		}

		protected virtual void OnError(ExecutorMessageEventArgs e) {
			if(Error != null) {
				Error(this, e);
			}
		}
		#endregion

		~SequenceContext() {
			Dispose(false);
		}

		override protected void Dispose(bool disposing) {
			_DisposeSequenceExecutor();
			GC.SuppressFinalize(this);
		}
	}
}