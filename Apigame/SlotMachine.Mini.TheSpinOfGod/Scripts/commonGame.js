var commonGame = new function () {
	this.Root = '';
    this.urlRoot = '';
    this.mediaUrl = '';
    this.urlApi = ''
    this.hubName = "";
    this.hubs = "";
	this.miniPokerHub = "";
    this.hiloHub = "";
	this.luckyDiceHub = "";
    
	this.commonLuckyDiceHubs = "";
    this.commonSlotHubs = "";
    this.commonHiLoHubs = "";
	
	if (window.location.href.indexOf("http://saoclub.com") > -1 ||
		window.location.href.indexOf("http://poker.saoclub.com") > -1 )	{
		this.Root = 'http://saoclub.com'
		this.urlRoot = 'http://saoclub.com/public/js/minigame/';
		this.mediaUrl = 'http://saoclub.com/public/js/minigame/';
		this.urlApi = '//saoclub.com/clientminigame/api/luckydice/'
		this.hubName = "miniGameHub";
		this.hubs = "//saoclub.com/clientminigame";
		this.miniPokerHub = "minipoker";
		this.hiloHub = "hilo";
		this.luckyDiceHub = "miniGameHub";
		
		this.commonLuckyDiceHubs = "//txw.saoclub.com";
		this.commonSlotHubs = "//mpw.saoclub.com";
		this.commonHiLoHubs = "//hilow.saoclub.com";
	}else if (window.location.href.indexOf("http://vuachoibai.net") > -1)	{
		this.Root = 'http://vuachoibai.net'
		this.urlRoot = 'http://vuachoibai.net/public/js/minigame/';
		this.mediaUrl = 'http://vuachoibai.net/public/js/minigame/';
		this.urlApi = '//vuachoibai.net/clientminigame/api/luckydice/'
		this.hubName = "miniGameHub";
		this.hubs = "//vuachoibai.net/clientminigame";
		this.miniPokerHub = "minipoker";
		this.hiloHub = "hilo";
		this.luckyDiceHub = "miniGameHub";
		
		this.commonLuckyDiceHubs = "//txw.vuachoibai.net";
		this.commonSlotHubs = "//mpw.vuachoibai.net";
		this.commonHiLoHubs = "//hilow.vuachoibai.net";
	}else if (window.location.href.indexOf("http://vuachoibai.org") > -1)	{
		this.Root = 'http://vuachoibai.org'
		this.urlRoot = 'http://vuachoibai.org/public/js/minigame/';
		this.mediaUrl = 'http://vuachoibai.org/public/js/minigame/';
		this.urlApi = '//vuachoibai.org/clientminigame/api/luckydice/'
		this.hubName = "miniGameHub";
		this.hubs = "//vuachoibai.org/clientminigame";
		this.miniPokerHub = "minipoker";
		this.hiloHub = "hilo";
		this.luckyDiceHub = "miniGameHub";
		
		this.commonLuckyDiceHubs = "//txw.vuachoibai.org";
		this.commonSlotHubs = "//mpw.vuachoibai.org";
		this.commonHiLoHubs = "//hilow.vuachoibai.org";
	}else if (window.location.href.indexOf("alpha.cungchoibai.com") > -1)	{
		this.Root = 'http://alpha.cungchoibai.com'
		this.urlRoot = 'http://alpha.cungchoibai.com/portal/public/js/minigame/';
		this.mediaUrl = 'http://alpha.cungchoibai.com/portal/public/js/minigame/';
		this.urlApi = '//alpha.cungchoibai.com/clientminigame/api/luckydice/'
		this.hubName = "miniGameHub";
		this.hubs = "//alpha.cungchoibai.com/clientminigame";
		this.miniPokerHub = "minipoker";
		this.hiloHub = "hilo";
		this.luckyDiceHub = "miniGameHub";
		
		this.commonLuckyDiceHubs = "//alphamini.cungchoibai.com/txw";
		this.commonSlotHubs = "//alphamini.cungchoibai.com/mpw";
		this.commonHiLoHubs = "//alphamini.cungchoibai.com/hilow";
	}
	
	
    this.isBet = true;//check allow bet
    this.typeBet = 1;

    this.cacheData = null;
    this.rowperPage = 10;
    this.gameSession = 0;
    this.typeHis = 4;
    this.playflag = false;
    this.overOrUnder = 0;
    this.gameConnection = null;
    this.gameHub = null;
    this.commonLuckyConnection = null;
    this.commonSlotConnection = null;
    this.commonHiLoConnection = null;
    this.commonLuckygameHub = null;
    this.commonSlotgameHub = null;
    this.commonHiLogameHub = null;
	this.gameName = {
		luckydice: 1,
		minipoker:2,
		hilo:3,
		minivuabai:4,
		threecards:5
	}

    this.InitGame = function () {
        if (fileLoaded == false) {
            Init(1);
        }
        else {
            initHub();
        }
        this.playflag = true;
    };
    this.disGame = function () {
        $(".minigame-list-icon").hide();
        this.playflag = false;

        if (LuckyDiceGame)
            LuckyDiceGame.HideDiceGUI();
        if (commonSlot)
            commonSlot.hideSlotGUI();
        // if (commonHiLo)
            // commonHiLo.hideHiLoGUI();
		$("#miniVuabai").css("display","none");
		$("#threecards").css("display","none");
        stopHub();
    };

    this.loadFile = function (manifest, callback, async) {
        if (typeof async == 'undefined')
            async = false;
        var preload = new createjs.LoadQueue(async);
        preload.on("complete", function () {
            callback()
        });
        preload.loadManifest(manifest);
		return preload;
    };

    this.showhide = function (type) {
        if (type == 1) {
            // if (typeof commonLuckyDice == 'undefined')
                // return;

            // if ($('#minigamexx').css('display') == 'none') {
                // commonLuckyDice.ShowDiceGUI();
                // window.localStorage.setItem("showhidetx", "1");

                // if (ga && ga !== undefined) {
                    // ga('send', 'event', 'Minigame', 'Tà€â‚¬Ă¢â€Â¬i xì€â‚¬Ă¢â‚¬Â°u', 'Show');
                // }
            // }
            // else {
                // commonLuckyDice.HideDiceGUI();
                // window.localStorage.setItem("showhidetx", "0");
                // if (ga && ga !== undefined) {
                    // ga('send', 'event', 'Minigame', 'Tà€â‚¬Ă¢â€Â¬i xì€â‚¬Ă¢â‚¬Â°u', 'Hide');
                // }
            // }
        } else if (type == 2) {
            if (typeof commonSlot == 'undefined')
                return;

            if (commonSlot.playflag == false) {
                commonSlot.Init();
                setTimeout(function () {
                    if (!$(".slot-game-block").is(":visible")) {
                        commonGame.showhide(2);
                    };
                }, 3000);
                return;
            }
            
            if ($('#slotmachine').css('display') == 'none') {
				if(commonGame.commonSlotConnection.state == 4)
				{
					startHub(commonGame.gameName.minipoker);
				}
                commonSlot.showSlotGUI();
                if (ga && ga !== undefined && type == 2) {
                    ga('send', 'event', 'Minigame', 'MiniPoker', 'Show');
                }
            }
            else {
				if(!commonSlot.isAuto())
				{
					stopHub(commonGame.gameName.minipoker);
				}
                commonSlot.hideSlotGUI();
                if (ga && ga !== undefined) {
                    ga('send', 'event', 'Minigame', 'MiniPoker', 'Hide');
                }
            }
        } else if (type == 3) {
			HiloGame.ShowHideHilo();
            // if (typeof commonHiLo == 'undefined')
                // return;

            // if ($('#HiLo').css('display') == 'none') {
				// startHub(commonGame.gameName.hilo);
                // commonHiLo.showHiLoGUI();
                // if (ga && ga !== undefined && type == 2) {
                    // ga('send', 'event', 'Minigame', 'HiLo', 'Show');
                // }
            // }
            // else {
				// stopHub(commonGame.gameName.hilo);
                // commonHiLo.hideHiLoGUI();
                // if (ga && ga !== undefined) {
                    // ga('send', 'event', 'Minigame', 'HiLo', 'Hide');
                // }
            // }
        }else if (type == 4) {
			if($("#miniVuabai").css("display") == "none")
			{
				if(miniVuabai.GameHub.connection.state == 4)
					startHub(type);
				$("#miniVuabai").show();
			}
			else
			{
				if(!miniVuabai.isAutoMainGame)
					stopHub(type);
				$("#miniVuabai").hide();
			}
        } else if (type == 5) {
            if ($("#threecards").length <= 0) {
                threecards.Init();
                setTimeout(function () {
                    commonGame.showhide(5);
                }, 3000);
                return; 
            }

				if (!$("#threecards").is(":visible")) {
				$("#xxThreeCards").css("opacity", "1")
				if (ga && ga !== undefined) {
					ga('send', 'event', 'miniBaLa', 'miniBaLa', 'Show');
				}
			} else {
				$("#xxThreeCards").css("opacity", "0.2")
				
				if (ga && ga !== undefined) {
					ga('send', 'event', 'miniBaLa', 'miniBaLa', 'Hide');
				}
			}
			if($("#threecards").css("display") == "none")
			{
				startHub(type);
				$("#threecards").show();
			}
			else
			{
				stopHub(type);
				$("#threecards").hide();
			}
        }	else if (type == 6) {
			PyramidGame.ShowHidePyramid();
        } else if(type==7){
		if (typeof minigameThienLong != 'undefined' && typeof minigameThienLong.showHideGame == 'function')
		minigameThienLong.showHideGame();
		}
    };

    this.closePopupParent = function () {
        $('#Popup_Container').remove();
        $('#overlayxx').remove();
        $('.popup').remove();
        $('.overlay').remove();
        this.cacheData = null;
    };
    this.setPopup = function (width, height) {
        $('#popup_nd').css('width', width);
        $('#popup_nd').css('height', height);
        var leftOffset = ($(window).width() - width) / 2;
        var topOffset = ($(window).height() - height) / 2 + $(window).scrollTop();
        $('#Popup_Container').css('left', "-790px");
        $('#Popup_Container').css('z-index', 1300);
        $('#Popup_Container').css("top", "71px");

        $('#Popup_Container').css('position', 'absolute');
        $('#overlayxx').css('height', $(document).height());
        $('#overlayxx').show();
    };

    this.formatTime = function(inputTime) {
        var secNumb = parseInt(inputTime);
        var hours = Math.floor((secNumb) / 3600);
        var minutes = Math.floor((secNumb - hours * 3600) / 60);
        var seconds = secNumb - (minutes * 60);

        if (hours < 10)
            hours = "0" + hours;

        if (minutes < 10)
            minutes = "0" + minutes;

        if (seconds < 10)
            seconds = "0" + seconds;
        return minutes + ':' + seconds;
    };
    this.FormatNumber = function (p_sStringNumber) {
        p_sStringNumber += '';
        x = p_sStringNumber.split(',');
        x1 = x[0];
        x2 = x.length > 1 ? ',' + x[1] : '';
        var rgx = /(\d+)(\d{3})/;
        while (rgx.test(x1))
            x1 = x1.replace(rgx, '$1' + '.' + '$2');

        return x1 + x2;
    };
	
	this.FormatNumberTaiXiu = function (p_sStringNumber) {
        p_sStringNumber += '';
        x = p_sStringNumber.split(',');
        x1 = x[0];

        return this.FormatNumber(parseInt(x1/1000)) + ' K';
    };
	
    this.formDateTimehms = function (date) {
        date = date.replace(/\-/g, '\/').replace(/[T|Z]/g, ' ');
        if (date.indexOf('.') > 0)
            date = date.substring(0, date.indexOf('.'));
        var d = new Date(date);
        var curr_date = d.getDate();
        var curr_month = d.getMonth() + 1;
        var curr_year = d.getFullYear();
        var _hour = d.getHours();
        var _minute = d.getMinutes();
        var _second = d.getSeconds();
        if (curr_date < 10) curr_date = "0" + curr_date;
        if (curr_month < 10) curr_month = "0" + curr_month;
        if (_hour < 10) _hour = "0" + _hour;
        if (_minute < 10) _minute = "0" + _minute;
        return curr_date + "/" + curr_month
            + "/" + curr_year + " " + _hour + ":" + _minute;
    };

    var hubStarted = false;
	var fileLoaded = false;
    function Init(type)//1:star, 2: coin
    {
        commonGame.typeBet = type;
        commonGame.playflag = true;

        var arrJs2 = [
							{ src: commonGame.urlRoot + 'minigameHubs.js' },
							{ src: commonGame.Root + '/miniluckydice/Scripts/MiniGameLuckDice/luckyDiceUpdate.js?v=2.8.7' },
                            { src: commonGame.Root + '/minipoker/Scripts/slotmachine/commonSlot.js?v=1.17' },
                            { src: commonGame.Root + '/minihilo/Scripts/MiniGameHilo/Hilo.js?v=1.7.179' },
							{ src: commonGame.Root + '/miniThienLong/content/js/MiniGameThienLong.js' },
							{ src: commonGame.Root + '/minipyramid/Scripts/game/minigame.pyramid.js?v=1.2.3' },
							{ src: commonGame.Root + '/event/MiniEvents/EventThanBai/js/eventthanbai.js' },
							
							{ src: commonGame.Root + '/event/Contents/VongQuayMayManVCB/js/vongquaymaymanvcb.js?v=1.2.8' },
							
							//{ src: commonGame.Root + '/event/MiniEvents/Qk2016T9/js/Qk2016T9.js?v=1.1.2'},

                            { src: commonGame.urlRoot + 'pager.js' },
                            //{ src: commonGame.urlRoot + 'Scripts/jquery-ui.js' },
                            { src: commonGame.urlRoot + 'flexcroll.js' },
							//{ src: commonGame.Root + '/event/MiniEvents/Qk2016T9/css/style.css', type: createjs.LoadQueue.CSS},
                            { src: commonGame.mediaUrl + 'style-minigame.css?v=1.12', type: createjs.LoadQueue.CSS },
                             { src: commonGame.mediaUrl + 'flexcrollstyles.css', type: createjs.LoadQueue.CSS },
                            //{ src: commonGame.mediaUrl + 'css/jquery.mCustomScrollbar.css', type: createjs.LoadQueue.CSS },
                            //{ src: commonGame.mediaUrl + 'css/jquery-ui.css', type:createjs.LoadQueue.CSS }
							];

        commonGame.loadFile(arrJs2, function () {
            fileLoaded = true;	
			initHub();
        }); 
    };

    function initHub() {
        // commonGame.gameConnection = $.hubConnection(commonGame.hubs);
        // commonGame.gameHub = commonGame.gameConnection.createHubProxy(commonGame.hubName);
        // var miniHub = new MinigameHub(commonGame.gameHub);

        // commonGame.gameConnection.stateChanged(function (change) {
            // if (change.newState === $.signalR.connectionState.connecting) {
                // console.info('minigame connecting');
            // }
            // else if (change.newState === $.signalR.connectionState.reconnecting) {
                // console.info('minigame reconnecting');
            // }
            // else if (change.newState === $.signalR.connectionState.connected) {
                // console.info('minigame connected');
            // }
            // else if (change.newState === $.signalR.connectionState.disconnected) {
                // console.info('minigame disconnected');
                // if (hubStarted)
                    // reconnectHub();
            // }
        // });

        // try {
            // commonGame.gameConnection.start().done(function () {
                // hubStarted = true;
                // bindInterface();
            // }).fail(function () {
                // reconnectHub();
            // });
        // } catch (e) {
            // reconnectHub();
        // }
		
		
		/**************************Game  Lucky Dice Hubs*********************/
		// commonGame.commonLuckyConnection = $.hubConnection(commonGame.commonLuckyDiceHubs);
        // commonGame.commonLuckygameHub = commonGame.commonLuckyConnection.createHubProxy(commonGame.hubName);
        // var miniHubLuckyDice = new MinigameHub(commonGame.commonLuckygameHub);

        // commonGame.commonLuckyConnection.stateChanged(function (change) {
            // if (change.newState === $.signalR.connectionState.connecting) {
                // console.info('minigame luckyDice connecting');
            // }
            // else if (change.newState === $.signalR.connectionState.reconnecting) {
                // console.info('minigame luckyDice reconnecting');
            // }
            // else if (change.newState === $.signalR.connectionState.connected) {            
				// commonLuckyDice.useCoinStar(1)
				// console.info('minigame luckyDice connected');
				// EventTX2016T5.GetEventTime();
				// /*if(App.currentAccount.AccountID > 0)
				// {
					// setTimeout(function(){
						// commonLuckyDice.UpdateEventPoint();
					// }, 10000);		
				// }*/
            // }
            // else if (change.newState === $.signalR.connectionState.disconnected) {
                // console.info('minigame luckyDice disconnected');
                // //if (hubStarted)
                // //reconnectHubLuckyDice();
            // }
        // });

        // try {
            // commonGame.commonLuckyConnection.start({ transport: ['webSockets','longPolling'], jsonp: true  , waitForPageLoad:true}).done(function () {
            // }).fail(function () {
                // reconnectHubLuckyDice();
            // });
        // } catch (e) {
            // reconnectHubLuckyDice();
        // }
        /**************************END Game  Lucky Dice Hubs*********************/
		
		
        /**************************Game  Common Slot Hubs*********************/
		commonGame.commonSlotConnection = $.hubConnection(commonGame.commonSlotHubs);
        commonGame.commonSlotgameHub = commonGame.commonSlotConnection.createHubProxy(commonGame.miniPokerHub);
        var miniHubcommonSlot = new MinigameHub(commonGame.commonSlotgameHub);

        commonGame.commonSlotConnection.stateChanged(function (change) {
            if (change.newState === $.signalR.connectionState.connecting) {
                console.info('minigame minipoker connecting');
            }
            else if (change.newState === $.signalR.connectionState.reconnecting) {
                console.info('minigame minipoker reconnecting');
            }
            else if (change.newState === $.signalR.connectionState.connected) {
				commonSlot.useCoinStar(1)
                console.info('minigame minipoker connected');
            }
            else if (change.newState === $.signalR.connectionState.disconnected) {
                console.info('minigame minipoker disconnected');
                if (hubStarted)
					reconnectHubcommonSlot();
            }
        });

        try {
            commonGame.commonSlotConnection.start({ transport: ['webSockets','longPolling'], jsonp: true  , waitForPageLoad:true}).done(function () {
            }).fail(function () {
                reconnectHubcommonSlot();
            });
        } catch (e) {
            reconnectHubcommonSlot();
        }
        /**************************END Game  Common Slot Hubs*********************/
		
		
		
        /**************************Game Hilo Hubs*********************/
		// commonGame.commonHiLoConnection = $.hubConnection(commonGame.commonHiLoHubs);
        // commonGame.commonHiLogameHub = commonGame.commonHiLoConnection.createHubProxy(commonGame.hiloHub);
        // var miniHubcommonHiLo = new MinigameHub(commonGame.commonHiLogameHub);

        // commonGame.commonHiLoConnection.stateChanged(function (change) {
            // if (change.newState === $.signalR.connectionState.connecting) {
                // console.info('minigame Hilo connecting');
            // }
            // else if (change.newState === $.signalR.connectionState.reconnecting) {
                // console.info('minigame Hilo reconnecting');
            // }
            // else if (change.newState === $.signalR.connectionState.connected) {
                // console.info('minigame Hilo connected');		
				// commonHiLo.useCoinStar(1);		
				// commonGame.commonHiLogameHub.server.GetAccountInfoHiLo();
            // }
            // else if (change.newState === $.signalR.connectionState.disconnected) {
                // console.info('minigame hilo disconnected');
                // //if (hubStarted)
                // //reconnectHubcommonHiLo();
            // }
        // });

        /**************************END Game  Hilo Hubs*********************/
		hubStarted = true;
		bindInterface();
		
    };
	

	function startHub(gameIndex){
		if(gameIndex == commonGame.gameName.minipoker)
		{
			// connect to luckydice server
			try {
				commonGame.commonSlotConnection.start({ transport: ['webSockets','longPolling'], jsonp: true , waitForPageLoad:true }).done(function () {
            }).fail(function () {
                reconnectHubcommonSlot();
            });
			} catch (e) {
				reconnectHubcommonSlot();
			}
		}
		// else if(gameIndex == commonGame.gameName.hilo)
		// {
			// // connect to hilo server
			// try {
				// commonGame.commonHiLoConnection.start({ transport: ['webSockets','longPolling'], jsonp: true  , waitForPageLoad:true}).done(function () {
			// }).fail(function () {
				// reconnectHubcommonHiLo();
			// });
			// } catch (e) {
				// reconnectHubcommonHiLo();
			// }
		// }
		else if(gameIndex == commonGame.gameName.minivuabai)
		{
			//connect to minivuabai server
			miniVuabai.startHub();
		}
		else if(gameIndex == commonGame.gameName.threecards)
		{
			//connect to bala server
			threecards.startHub();
		}
	}				
	
	
    function stopHub(gameIndex) {
        try {
			if(gameIndex == commonGame.gameName.luckydice)
			{
				LuckyDiceGame.StopHub();
			}
			else if(gameIndex == commonGame.gameName.minipoker)
			{
				commonGame.commonSlotConnection.stop();
			}
			// else if(gameIndex == commonGame.gameName.hilo)
			// {
				// commonGame.commonHiLoConnection.stop();
			// }
			else if(gameIndex == commonGame.gameName.minivuabai)
			{
				miniVuabai.stopHub();
			}
			else if(gameIndex == commonGame.gameName.threecards)
			{
				threecards.stopHub();
			}
			else//stop all
			{
				LuckyDiceGame.StopHub();
				commonGame.commonSlotConnection.stop();
				// commonGame.commonHiLoConnection.stop();
				miniVuabai.stopHub();
				threecards.stopHub();
				PyramidGame.stopHub();
			}
            //commonGame.gameConnection.stop();					
            hubStarted = false;
        } catch(e){}
    }
		
	function reconnectHubLuckyDice() {
        if (typeof disconnect != 'underfined') {
            clearInterval(disconnect);
            delete disconnect;
        }
        var disconnect = setInterval(function () {
            if (commonGame.commonLuckyConnection.state == $.signalR.connectionState.disconnected) {
                commonGame.commonLuckyConnection.start({ transport: ['webSockets','longPolling'], jsonp: true , waitForPageLoad:true  }).done(function () {
                    clearInterval(disconnect);
                    delete disconnect;
                });
            }
        }, 5000);
    }
	
	function reconnectHubcommonSlot() {
        if (typeof disconnect != 'underfined') {
            clearInterval(disconnect);
            delete disconnect;
        }
        var disconnect = setInterval(function () {
            if (commonGame.commonSlotConnection.state == $.signalR.connectionState.disconnected) {
                commonGame.commonSlotConnection.start({ transport: ['webSockets','longPolling'], jsonp: true , waitForPageLoad:true  }).done(function () {
                    clearInterval(disconnect);
                    delete disconnect;
                });
            }
        }, 5000);
    }
	
	function reconnectHubcommonHiLo() {
        if (typeof disconnect != 'underfined') {
            clearInterval(disconnect);
            delete disconnect;
        }
        var disconnect = setInterval(function () {
            if (commonGame.commonHiLoConnection.state == $.signalR.connectionState.disconnected) {
                commonGame.commonHiLoConnection.start({ transport: ['webSockets','longPolling'], jsonp: true , waitForPageLoad:true }).done(function () {
                    clearInterval(disconnect);
                    delete disconnect;
                });
            }
        }, 5000);
    }

    function reconnectHub() {
        if (typeof disconnect != 'underfined') {
            clearInterval(disconnect);
            delete disconnect;
        }
        var disconnect = setInterval(function () {
            if (commonGame.gameConnection.state == $.signalR.connectionState.disconnected) {
                commonGame.gameConnection.start().done(function () {
                    clearInterval(disconnect);
                    delete disconnect;
                });
            }
        }, 5000);
    }

    function bindInterface() {
        if (!fileLoaded)
            return;

        // var html1 = '';
        // html1 += '<ul class="minigame-list-icon">';
		// html1 += '<img src="http://vuachoibai.net/portal/public/Events/EventPocker/EVENT.gif" style="position: absolute; left: 12px; top: 31px;">';
		// //html1 += '<img src="http://vuachoibai.net/portal/public/Events/EventPocker/EVENT.gif?v=1.11" style="position: absolute; left: 54px; top: 31px;">';
		// //html1 += '<img src="http://vuachoibai.net/portal/public/Events/EventPocker/EVENT.gif" style="position: absolute; left: 92px; top: 31px;">';
        // html1 += '<li class="dice-game use-star">';
        // html1 += '<div class="icon-game" id="xxid" onclick="commonGame.showhide(1)"></div>';
        // html1 += '<span id="time" style="display:none;">0</span>'
        // html1 += '<span id="prizebet" style="display:none;">+5000</span>';
        // html1 += '</li><li class="slot-game use-coin">';
        // html1 += '<div class="icon-game" id="xxid2" onclick="commonGame.showhide(2)"></div>';
        // html1 += '</li><li class="hilo-game use-coin">';
        // html1 += '<div class="icon-game" id="xxhilo" onclick="commonGame.showhide(3)"></div>';
		// html1 += '</li><li class="kingpocker-game use-coin">';
		// html1 += '<div class="icon-game" id="xxkingpocker" onclick="commonGame.showhide(4)"></div>';
		// html1 += '</li><li class="threecards-game use-coin">';
		// html1 += '<div class="icon-game" id="xxThreeCards" onclick="commonGame.showhide(5)"></div>';
		// html1 += '</li></ul>';        
        // var str = '<div id="ag"></div>';

 
		
        // if ($('.minigame-list-icon').length <= 0)
            // $('#listMinigame').append(html1);
        // if ($('#ag').length <= 0)
            // $('#listMinigame').append(str);
        // $(".minigame-list-icon").show();
        // $("#ag").show();
    
        try {
			// Thai check href poker
			if(window.location.href.indexOf("http://alpha.cungchoibai.com/poker") > -1 ||
			   window.location.href.indexOf("http://poker.saoclub.com") > -1)
			{
				// show mini poker
				  commonSlot.Init();
				   setTimeout(function () {
					      if(!$(".slot-game-block").is(":visible")){
							commonGame.showhide(2);
					   };
				   },3000)
				VongQuayMayManVcb.InitSpin();
					threecards.Init();
					commonLuckyDice.Init();
			}
			else{
				//commonLuckyDice.Init();
				// commonHiLo.Init();
				
				VongQuayMayManVcb.InitSpin();

				var dateInt = new Date().toISOString().substr(0, 10).replace(/\-/g, '');
				var keyFirstLoad = 'LoadMinigame_Sao_' + dateInt + '_' + App.currentAccount.AccountID;
				var firstLoadMinigame = localStorage.getItem(keyFirstLoad);

				if (firstLoadMinigame == null) {
				    commonSlot.Init();
				    miniVuabai.Init();
				    setTimeout(function () {

				        /*if(!$("#HiLo").is(":visible")){
                             commonGame.showhide(3)
                        };*/
				        // if(!$(".dice-game-block").is(":visible")){
				        // commonGame.showhide(1)
				        // };
				        /*if(!$("#miniVuabai").is(":visible")){
                             $("#xxkingpocker").css("opacity","1") 
                             $("#miniVuabai").css("display","block");
                        };*/
				        //ChumTaXi2015.StartGetEventRutLoc()

				        //if(typeof isRunEventMiniPoker != "undefined" && isRunEventMiniPoker > 0){
				        if (!$(".slot-game-block").is(":visible")) {
				            commonGame.showhide(2);
				        };
				        commonGame.showhide(3);
				        //PyramidGame.ShowHidePyramid();
				        //}
				    }, 3000);

				    threecards.Init();
                }
				localStorage.setItem(keyFirstLoad, true);
			}
            
        }
        catch (err) {
        }
    };
};