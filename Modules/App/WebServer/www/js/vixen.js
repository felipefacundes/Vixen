﻿var apiUrl = "/api";
var playerUrl = apiUrl + "/play";
var elementUrl = apiUrl + "/element";
var timeoutTimer;
var storeTimeKey = "timeout";
var storeIntesityKey = "intensity";

function ViewModel() {

	var self = this;
	self.elements = ko.observableArray();
	self.sequences = ko.observableArray();
	self.selectedSequence = ko.observable();
	self.status = ko.observable();
	self.timeout = ko.observable(30);
	self.elementIntensity = ko.observable(100);
	self.delayedElementIntensity = ko.pureComputed(self.elementIntensity).extend({ rateLimit: { timeout: 500, method: "notifyWhenChangesStop" } });
	self.elementResults = ko.observableArray();
	self.elements = ko.observable();
	self.selectedElement = ko.observable();
	self.selectedColor = ko.observable("#FFFFFF");
	self.elementTree = ko.observableArray();
	self.selectedSequence = ko.observable();
	self.search = ko.observable(true);
	self.searchToken = ko.observable();
	self.delayedSearchToken = ko.pureComputed(self.searchToken).extend({ rateLimit: { timeout: 300, method: "notifyWhenChangesStop" } });
	self.searchTokenHold = "";
	self.searchResultsOverflow = ko.observable(false);
	self.nowPlayingList = ko.observableArray([]).extend({ rateLimit: 25 });
	
	
	self.contextStatus = function(state) {
		var icon;
		switch (state) {
		case 1:
			icon = "glyphicon-play";
			break;
		case 2:
			icon = "glyphicon-pause";
			break;
		case 0:
		default:
			icon = "glyphicon-stop";
		}
		return icon;
	};

	self.isPlaying = function (state) {
		return state == 1;
	};

	self.isPaused = function (state) {
		return state == 2;
	};

	self.clearSearch = function() {
		self.searchToken("");
	}

	self.navigateChild = function (elem) {
		self.showLoading();
		self.elementTree.push(self.elements());
		if (self.search()) {
			self.search(false);
		}

		self.elements(elem.Children);
		self.hideLoading();
		return false;
	}

	self.navigateParent = function (elem) {
		
		self.showLoading();
		self.elements(self.elementTree().pop());
		if (self.elementTree().length == 0) {
			self.search(true);
		}
		self.hideLoading();
		
		return false;
	}

	// Status functions
	self.getStatus = function() {
		$.get(playerUrl + '/status')
			.done(function (status) {
				self.status(status.Message);
			});
	}

	self.clearStatus = function(seconds) {
		if (timeoutTimer) {
			clearTimeout(timeoutTimer);
		}
		timeoutTimer = setTimeout(function () {
			self.status("");
		}, seconds * 1000);

	}

	self.updateStatus = function (seconds) {
		if (timeoutTimer) {
			clearTimeout(timeoutTimer);
		}
		timeoutTimer = setTimeout(function () {
			self.getStatus();
		}, seconds * 1000);

	}

	//Element retrieval 
	self.searchElements = function (value) {
		
		if (value && value.length > 0) {
			self.showLoading();
			$.ajax({
				url: elementUrl + '/searchElements',
				dataType: "json",
				data: {
					q: value
				}
			})
				.then(function (response) {
					if (response.length < 50) {
						self.elements(response);
						self.searchResultsOverflow(false);
					} else {
						self.searchResultsOverflow(true);
					}
					
					self.hideLoading();
				});
		} else {
			self.showLoading();
			self.getElements();
			self.searchResultsOverflow(false);
			self.hideLoading();
		}
	}

	self.getElements = function () {
		$.get(elementUrl + '/getElements')
			.done(function (data) {
				self.elements(data);
			}).error(function (jqXHR, status, error) {
				self.status(error);
				self.hideLoading();
			});
	}

	self.turnOnElement = function(data, event, timed) {
		var a = $(event.target).closest("form").serializeArray();;

		//model data
		var parms = {
			id : data.Id,
			duration : timed?self.timeout():0,
			intensity:self.elementIntensity()
		};
		
		//supplement with form data for the Color
		$.each(a, function () {
			if (parms[this.name] !== undefined) {
				if (!parms[this.name].push) {
					parms[this.name] = [parms[this.name]];
				}
				parms[this.name].push(this.value || '');
			} else {
				parms[this.name] = this.value || '';
			}
		});

		$.ajax({url: elementUrl + '/on',
			type: 'POST',
			datatype: "JSON",
			data:parms
		})
			.done(function (status) {
				self.status(status.Message);
				self.updateStatus(self.timeout());
			}).error(function (jqXHR, status, error) {
				self.status(error);
				self.hideLoading();
			});
		return false;
	}

	self.turnOffElement = function (data, event) {
		
		var parms = {
			id: data.Id
		};
		
		$.post(elementUrl + '/off', parms, null, 'JSON')
			.done(function (status) {
				self.status(status.Message);
			}).error(function (jqXHR, status, error) {
				self.status(error);
				self.hideLoading();
			});
		return false;
	}

	//Sequence functions

	self.stopSequence = function(data) {
		$.post(playerUrl + '/stopSequence', data.Sequence ? ko.mapping.toJS(data.Sequence) : data.selectedSequence(), null, 'JSON')
			.done(function (status) {
				self.status(status.Message);
				self.updateStatus(5);
			}).error(function (jqXHR, status, error) {
				self.status(error);
				self.hideLoading();
			});
	}

	self.pauseSequence = function (data) {
		$.post(playerUrl + '/pauseSequence', data.Sequence ? ko.mapping.toJS(data.Sequence) : data.selectedSequence(), null, 'JSON')
			.done(function (status) {
				self.status(status.Message);
				self.updateStatus(5);
			}).error(function (jqXHR, status, error) {
				self.status(error);
				self.hideLoading();
			});
	}

	self.playSequence = function(data) {
		self.showLoading();
		$.ajax({
			url: playerUrl + '/playSequence',
			type: 'POST',
			dataType:"json",
			data: data.Sequence ? ko.mapping.toJS(data.Sequence) : data.selectedSequence()
			})
			.done(function (status) {
				self.status(status.Message);
				self.updateStatus(5);
				self.hideLoading();
		}).error(function(jqXHR, status, error) {
				self.status(error);
				self.hideLoading();
		});
	}

	self.getSequences = function() {
		$.get(playerUrl + '/getSequences')
			.done(function (data) {
				self.sequences(data);
			}).error(function(jqXHR, status, error) {
				self.status(error);
				self.hideLoading();
			});
	}

	//subscriptions

	self.delayedSearchToken.subscribe(function (val) {
		self.searchElements(val);
	});

	self.timeout.subscribe(function(val) {
		amplify.store(storeTimeKey, val);
	});

	self.delayedElementIntensity.subscribe(function(val) {
		amplify.store(storeIntesityKey, val);
	});

	//self.nowPlayingList.subscribe(function (val) {
	//	alert("now Playing");
	//});
	

	self.showLoading = function() {
		setTimeout(function () {
			$('#loading-indicator').show();
		}, 1);
	}

	self.hideLoading = function() {
		setTimeout(function () {
			$('#loading-indicator').hide();
		}, 300);
	}

	self.retrieveStoredSettings = function () {
		var intensity = amplify.store(storeIntesityKey);
		if (intensity) {
			self.elementIntensity(intensity);
		}
		var time = amplify.store(storeTimeKey);
		if (time) {
			self.timeout(time);
		}
		
	}

	self.initSysytemStatusHub = function () {

		//register for updates
		$.connection.ContextStates.client.updatePlayingContextStates = self.updatePlayingStates;

		// Start the connection
		$.connection.hub.start();
	}

	self.updatePlayingStates = function (states) {
		//The knockout mapping plugin was not working correctly in IE only so this is a
		//direct mapping to only add or remove what changed to prevent recreating 
		//the whole DOM for the now playing on each refresh. Need to revist why IE is the only one
		//that did not work with the plugin. This is probably faster anyway.

		//Remove old and update existing.
		for (var index2 = 0; index2 < self.nowPlayingList().length; ++index2) {
			var remove = true;
			for (var index = 0; index < states.length; ++index) {
				if (states[index].Sequence.Name === self.nowPlayingList()[index2].Sequence.Name()) {
					self.nowPlayingList()[index2].Position(states[index].Position);
					self.nowPlayingList()[index2].State(states[index].State);
					remove = false;
					break;
				}
			}
			if (remove) {
				self.nowPlayingList.remove(self.nowPlayingList()[index2]);
			}
		}

		//Add new
		for (var index = 0; index < states.length; ++index) {
			var add = true;
			for (var index2 = 0; index2 < self.nowPlayingList().length; ++index2) {
				if (states[index].Sequence.Name === self.nowPlayingList()[index2].Sequence.Name()) {
					add = false;
					break;
				}
			}
			if (add) {
				var state = {
					Position: ko.observable(states[index].Position),
					State: ko.observable(states[index].State),
					Sequence: {
						FileName: ko.observable(states[index].Sequence.FileName),
						Name: ko.observable(states[index].Sequence.Name)
					}
				}

				self.nowPlayingList.push(state);
			}
		}
	}

	self.init = function ()
	{
		self.showLoading();
		self.getStatus();
		self.getElements();
		self.retrieveStoredSettings();
		self.getSequences();
		self.initSysytemStatusHub();
		self.hideLoading();
	}

};

ko.bindingHandlers.duration = {
	update: function (element, valueAccessor, allBindingsAccessor) {
		var value = ko.unwrap(valueAccessor());

		var duration = moment.duration(value);
		var format = allBindingsAccessor().format || 'MM/DD/YYYY';
		var trim = allBindingsAccessor().trim || true;
		var formatted = duration.format(format, { trim:trim });

		element.innerText = formatted;
	}
};

ko.bindingHandlers.jqmSlider = {
	// Initialize slider
	//https://github.com/seiyria/bootstrap-slider#options
	init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
		var valueUnwrapped = ko.utils.unwrapObservable(valueAccessor());
		$(element).slider({
			min: allBindings.get("min")||0,
			max: allBindings.get("max") || 100,
			value: allBindings.get("value") || valueUnwrapped
		}).on("change", function() {
			valueAccessor()( $(element).slider('getValue'));
		});
	},
	update: function(element, valueAccessor) {
		var value = ko.unwrap(valueAccessor());
		$(element).slider('setValue', value);
	}
};

//https://bgrins.github.io/spectrum/
ko.bindingHandlers.spectrumColorPicker = {
	init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
		var defaultColor = allBindings.get('default') || "#FFFFFF";
		var colors = bindingContext.$data.Colors;
		if (colors.length>0) {
			$(element).spectrum({
				color: colors[0],
				change: function (color) {
					$(element).val(color.toHexString());
				},
				theme: "sp-bigger",
				hideAfterPaletteSelect: true,
				//flat:true,
				showPaletteOnly: true,
				palette: colors
			}).val(colors[0]);
		} else {
			$(element).spectrum({
				color: defaultColor,
				change: function (color) {
					$(element).val(color.toHexString());
				},
				theme: "sp-big",
				showInput: true,
				preferredFormat: "hex",
				chooseText: "Ok",
				localStorageKey: "spectrum.color",
				hideAfterPaletteSelect: true,
				showPaletteOnly: true,
				togglePaletteOnly: true,
				togglePaletteMoreText: 'more',
				togglePaletteLessText: 'less',
				maxSelectionSize: 4,
				palette: [
					['red', 'green', 'blue', 'white']
				]
			}).val(defaultColor);
		}
		
	}
};

var model = new ViewModel();
ko.applyBindings(model);

model.init();

var b = document.documentElement;
b.setAttribute('data-useragent', navigator.userAgent);
b.setAttribute('data-platform', navigator.platform);
b.className += ((!!('ontouchstart' in window) || !!('onmsgesturechange' in window)) ? ' touch' : '');

$(document).on('click', '.navbar-collapse.in', function (e) {
	if ($(e.target).is('a')) {
		$(this).collapse('hide');
	}
});


//jQuery for page scrolling feature - requires jQuery Easing plugin
$(function () {
	$('a.page-scroll').bind('click', function (event) {
		var $anchor = $(this);
		var top = $($anchor.attr('href')).offset().top;
		var offset = top == 0 ? top : top - $(".navbar-header").height() - 20;
		$('html, body').stop().animate({
			scrollTop: offset
	}, 1500, 'easeInOutExpo');
		event.preventDefault();
	});
});

