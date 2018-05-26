﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Resources;
using Vixen.Attributes;
using Vixen.Marks;
using Vixen.Module;
using Vixen.Module.App;
using Vixen.Module.Effect;
using Vixen.Services;
using Vixen.Sys;
using Vixen.Sys.Attribute;
using Vixen.TypeConverters;
using VixenModules.App.Curves;
using VixenModules.App.LipSyncApp;
using VixenModules.EffectEditor.EffectDescriptorAttributes;
using VixenModules.Effect.Effect;
using VixenModules.Effect.Picture;
using ZedGraph;

namespace VixenModules.Effect.LipSync
{

	public class LipSync : EffectModuleInstanceBase
	{
		private LipSyncData _data;
		private EffectIntents _elementData = null;
		static Dictionary<PhonemeType, Bitmap> _phonemeBitmaps = null;
		private LipSyncMapLibrary _library = null;

		private Picture.Picture _thePic = null;
		
		public LipSync()
		{
			_data = new LipSyncData();
			LoadResourceBitmaps();
			_library = ApplicationServices.Get<IAppModuleInstance>(LipSyncMapDescriptor.ModuleID) as LipSyncMapLibrary;
		}

		protected override void TargetNodesChanged()
		{

		}

		protected override void _PreRender(CancellationTokenSource cancellationToken = null)
		{
			_elementData = new EffectIntents();
			RenderNodes();
		}

		// renders the given node to the internal ElementData dictionary. If the given node is
		// not a element, will recursively descend until we render its elements.
		private void RenderNodes()
		{
			EffectIntents result;
			LipSyncMapData mapData = null;
			List<ElementNode> renderNodes = TargetNodes.SelectMany(x => x.GetNodeEnumerator()).ToList();

			//Check to see if we are rendering by marks
			IMarkCollection mc = MarkCollections.FirstOrDefault(x => x.Id == _data.MarkCollectionId);
			var marks = mc?.MarksInclusiveOfTime(StartTime, StartTime + TimeSpan);
		
			if (_data.PhonemeMapping != null) 
			{
				if (!_library.Library.ContainsKey(_data.PhonemeMapping)) 
				{
					_data.PhonemeMapping = _library.DefaultMappingName;
				}

				PhonemeType phoneme = _data.StaticPhoneme; 
				if (_library.Library.TryGetValue(_data.PhonemeMapping, out mapData))
				{
					if (mapData.IsMatrix)
					{
						if (((_thePic == null) || IsDirty) && 
							(File.Exists(mapData.PictureFileName(phoneme))))
						{
							_thePic = new Picture.Picture();
							_thePic.Source = PictureSource.File;
							_thePic.TargetNodes = TargetNodes;
							_thePic.FileName = mapData.PictureFileName(phoneme);
							_thePic.Orientation = Orientation;
							_thePic.ScaleToGrid = ScaleToGrid;
							_thePic.ScalePercent = ScalePercent;
							_thePic.TimeSpan = TimeSpan;

							var intensityCurve = PixelEffectBase.ScaleValueToCurve(IntensityLevel, 100.0, 0.0);
							_thePic.LevelCurve = new Curve(new PointPairList(new[] { 0.0, 100.0 }, new[] { intensityCurve, intensityCurve}));

						}
						if (null != _thePic)
						{
							result = _thePic.Render();
							_elementData.Add(result);
						}
					}
					else
					{
						renderNodes.ForEach(delegate (ElementNode element)
						{
							LipSyncMapItem item = mapData.FindMapItem(element.Name);
							if (item != null)
							{
								if (LipSyncMode == LipSyncMode.MarkCollection && marks!=null)
								{
									foreach (var mark in marks)
									{
										if (mapData.PhonemeState(element.Name, mark.Text.ToUpper(), item))
										{
											var colorVal = mapData.ConfiguredColorAndIntensity(element.Name, mark.Text.ToUpper(), item);
											result = CreateIntentsForPhoneme(element, colorVal.Item1, colorVal.Item2, mark.Duration);
											result.OffsetAllCommandsByTime(mark.StartTime - StartTime);
											_elementData.Add(result);
										}
									}
								}
								else
								{
									if (mapData.PhonemeState(element.Name, phoneme.ToString(), item))
									{
										var colorVal = mapData.ConfiguredColorAndIntensity(element.Name, phoneme.ToString(), item);
										result = CreateIntentsForPhoneme(element, colorVal.Item1, colorVal.Item2, TimeSpan);
										_elementData.Add(result);
									}
									
								}
								
							}
						});
					}

				}
					
			}

		}

		private EffectIntents CreateIntentsForPhoneme(ElementNode element, double intensity, Color color, TimeSpan duration)
		{
			EffectIntents result;
			var level = new SetLevel.SetLevel();
			level.TargetNodes = new[] {element};
			level.Color = color;
			level.IntensityLevel = intensity;
			level.TimeSpan = duration;
			result = level.Render();
			return result;
		}

		protected override EffectIntents _Render()
		{
			return _elementData;
		}

		protected void SetMatrixBrowesables()
		{
			bool scaleIsBrowesable = false;
			LipSyncMapData mapData = null;
			if (_library.Library.TryGetValue(_data.PhonemeMapping, out mapData))
			{
				scaleIsBrowesable = mapData.IsMatrix;
			}

			Dictionary<string, bool> propertyStates = new Dictionary<string, bool>(3)
			{
				{"Orientation", scaleIsBrowesable},
				{"ScaleToGrid", scaleIsBrowesable},
				{"ScalePercent", scaleIsBrowesable && !ScaleToGrid },
				{"IntensityLevel", scaleIsBrowesable }
			};
		
			SetBrowsable(propertyStates);
			TypeDescriptor.Refresh(this);
		}

		protected void SetLipsyncModeBrowsables()
		{
			Dictionary<string, bool> propertyStates = new Dictionary<string, bool>(3)
			{
				{nameof(StaticPhoneme), LipSyncMode == LipSyncMode.Phoneme},
				{nameof(LyricData), LipSyncMode == LipSyncMode.Phoneme},
				{nameof(MarkCollectionId), LipSyncMode == LipSyncMode.MarkCollection}
			};

			SetBrowsable(propertyStates);
			TypeDescriptor.Refresh(this);
		}

		public override IModuleDataModel ModuleData
		{
			get { return _data; }
			set
			{
				_data = value as LipSyncData;
				SetMatrixBrowesables();
				SetLipsyncModeBrowsables();
				IsDirty = true;
			}
		}

		[Value]
		[ProviderCategory(@"Setup",0)]
		[ProviderDisplayName(@"Orientation")]
		[ProviderDescription(@"Orientation")]
		public StringOrientation Orientation
		{
			get { return _data.Orientation; }
			set
			{
				_data.Orientation = value;
				IsDirty = true;
				SetMatrixBrowesables();
				OnPropertyChanged();
			}
		}

		[Value]
		[ProviderCategory("Config", 2)]
		[DisplayName(@"Phoneme/Marks")]
		[Description(@"Use a single Phoneme or Collection of Marks with Phonemes")]
		[PropertyOrder(1)]
		public LipSyncMode LipSyncMode
		{
			get
			{
				return _data.LipSyncMode;

			}
			set
			{
				if (_data.LipSyncMode != value)
				{
					_data.LipSyncMode = value;
					SetLipsyncModeBrowsables();
					IsDirty = true;
					OnPropertyChanged();
				}
			}
		}

		[Value]
		[ProviderCategory("Config",2)]
		[DisplayName(@"Phoneme mapping")]
		[Description(@"The mapping associated.")]
		[PropertyEditor("SelectionEditor")]
		[TypeConverter(typeof(PhonemeMappingConverter))]
		[PropertyOrder(2)]
		public String PhonemeMapping
		{
			get { return _data.PhonemeMapping;  }
			set
			{
				_data.PhonemeMapping = value;
				IsDirty = true;
				SetMatrixBrowesables();
				OnPropertyChanged();
			}
		}

		[Value]
		[ProviderCategory(@"Config", 2)]
		[ProviderDisplayName(@"Mark Collection")]
		[ProviderDescription(@"Mark Collection that has the phonemes to align to.")]
		[TypeConverter(typeof(IMarkCollectionNameConverter))]
		[PropertyEditor("SelectionEditor")]
		[PropertyOrder(3)]
		public string MarkCollectionId
		{
			get
			{
				return MarkCollections.FirstOrDefault(x => x.Id == _data.MarkCollectionId)?.Name;
			}
			set
			{
				var id = MarkCollections.FirstOrDefault(x => x.Name.Equals(value))?.Id ?? Guid.Empty;
				if (!id.Equals(_data.MarkCollectionId))
				{
					_data.MarkCollectionId = id;
					IsDirty = true;
					OnPropertyChanged();
				}
			}
		}

		[Value]
		[ProviderCategory("Config", 2)]
		[DisplayName(@"Lyric")]
		[Description(@"The lyric verbiage this Phoneme is associated with.")]
		[PropertyOrder(4)]
		public String LyricData
		{
			get { return _data.LyricData; }
			set
			{
				_data.LyricData = value;
				IsDirty = true;
				OnPropertyChanged();
			}
		}

		[Value]
		[ProviderCategory("Config", 2)]
		[DisplayName(@"Phoneme")]
		[Description(@"The Phoenme mouth affiliation")]
		[PropertyOrder(5)]
		public PhonemeType StaticPhoneme
		{
			get { return _data.StaticPhoneme; }
			set
			{
				_data.StaticPhoneme = value;
				IsDirty = true;
				OnPropertyChanged();
			}
		}

		

		[Value]
		[ProviderCategory(@"Config", 2)]
		[ProviderDisplayName(@"ScaleToGrid")]
		[ProviderDescription(@"ScaleToGrid")]
		[PropertyOrder(6)]
		public bool ScaleToGrid
		{
			get { return _data.ScaleToGrid; }
			set
			{
				_data.ScaleToGrid = value;
				IsDirty = true;
				SetMatrixBrowesables();
				OnPropertyChanged();
			}
		}

		[Value]
		[ProviderCategory(@"Config", 2)]
		[ProviderDisplayName(@"ScalePercent")]
		[ProviderDescription(@"ScalePercent")]
		[PropertyEditor("SliderEditor")]
		[NumberRange(1, 100, 1)]
		[PropertyOrder(7)]
		public int ScalePercent
		{
			get { return _data.ScalePercent; }
			set
			{
				_data.ScalePercent = value;
				IsDirty = true;
				OnPropertyChanged();
			}
		}

		

		[Value]
		[ProviderCategory(@"Brightness",3)]
		[ProviderDisplayName(@"Brightness")]
		[ProviderDescription(@"Brightness")]
		[PropertyEditor("SliderEditor")]
		[NumberRange(1, 100, 1)]
		[PropertyOrder(1)]
		public int IntensityLevel
		{
			get { return _data.Level; }
			set
			{
				_data.Level = value;
				IsDirty = true;
				OnPropertyChanged();
			}
		}

		private void LoadResourceBitmaps()
		{
			if (_phonemeBitmaps == null)
			{
				Assembly assembly = Assembly.Load("LipSyncApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
				if (assembly != null)
				{
					ResourceManager rm = new ResourceManager("VixenModules.App.LipSyncApp.LipSyncResources", assembly);
					_phonemeBitmaps = new Dictionary<PhonemeType, Bitmap>();
					_phonemeBitmaps.Add(PhonemeType.AI, (Bitmap)rm.GetObject("AI"));
					_phonemeBitmaps.Add(PhonemeType.E, (Bitmap)rm.GetObject("E"));
					_phonemeBitmaps.Add(PhonemeType.ETC, (Bitmap)rm.GetObject("etc"));
					_phonemeBitmaps.Add(PhonemeType.FV, (Bitmap)rm.GetObject("FV"));
					_phonemeBitmaps.Add(PhonemeType.L, (Bitmap)rm.GetObject("L"));
					_phonemeBitmaps.Add(PhonemeType.MBP, (Bitmap)rm.GetObject("MBP"));
					_phonemeBitmaps.Add(PhonemeType.O, (Bitmap)rm.GetObject("O"));
					//_phonemeBitmaps.Add("PREVIEW", (Bitmap)rm.GetObject("Preview"));
					_phonemeBitmaps.Add(PhonemeType.REST, (Bitmap)rm.GetObject("rest"));
					_phonemeBitmaps.Add(PhonemeType.U, (Bitmap)rm.GetObject("U"));
					_phonemeBitmaps.Add(PhonemeType.WQ, (Bitmap)rm.GetObject("WQ"));
				}
			}
		}

		public override bool ForceGenerateVisualRepresentation { get { return true; } }

		public override void GenerateVisualRepresentation(System.Drawing.Graphics g, System.Drawing.Rectangle clipRectangle)
		{
			try
			{
				//if (StaticPhoneme == "")
				//{
				//	StaticPhoneme = "REST";
				//}

				string DisplayValue = string.IsNullOrWhiteSpace(LyricData) ? "-" : LyricData;
				Bitmap displayImage = null;
				Bitmap scaledImage = null;
				if (_phonemeBitmaps.TryGetValue(StaticPhoneme, out displayImage))
				{
					scaledImage = new Bitmap(displayImage, 
											Math.Min(clipRectangle.Height,clipRectangle.Width), 
											clipRectangle.Height);
					g.DrawImage(scaledImage, clipRectangle.X,clipRectangle.Y);
				}
				if ((scaledImage != null) && (scaledImage.Width < clipRectangle.Width))
				{
					clipRectangle.X += scaledImage.Width;
					clipRectangle.Width -= scaledImage.Width;
					Font AdjustedFont = Vixen.Common.Graphics.GetAdjustedFont(g, DisplayValue, clipRectangle, "Vixen.Fonts.DigitalDream.ttf");
					using (var StringBrush = new SolidBrush(Color.Yellow))
					{
						using (var backgroundBrush = new SolidBrush(Color.Green))
						{
							g.FillRectangle(backgroundBrush, clipRectangle);
						}
						g.DrawString(DisplayValue, AdjustedFont, StringBrush, 4 + scaledImage.Width, 4);
					}
				}
				
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}

		public void MakeDirty()
		{
			this.IsDirty = true;
		}
	}
}