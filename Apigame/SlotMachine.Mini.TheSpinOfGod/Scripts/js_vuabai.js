var stageVuabai;
var queueVuabai;
var slotVB1, slotVB2, slotVB3;
var itemVB1, itemVB2, itemVB3, itemVB4, itemVB5, itemVB6, itemVB7, itemVB8, itemVB9;
var containerSLot1, containerSlot2, containerSlot3;
var tilenhan = ['Jackpot', 85, 40, 20, 8, 0.7, 3, 0.5, 1.2];
var miniVuabai = new function () {
    if (window.location.href.indexOf("http://localhost:50300/") > -1) {
        this.rootUrl = 'http://localhost:50300/';
        this.cssUrl = 'http://localhost:50300/';
        this.MediaUrl = 'http://localhost:50300/';
        this.urlApi = 'http://localhost:50300/api/VuaBaiApi/';
        this.signalRUrl = 'http://localhost:50300/';
        this.imageUrl = 'http://localhost:50300/images/';
        this.StarServiceUrl = "http://alpha.vuachoibai.com/starservices/";
    } else if (window.location.href.indexOf("http://alpha.vuachoibai.com") > -1) {
        this.rootUrl = '/minivuabai/';
        this.cssUrl = '/minivuabai/css/';
        this.MediaUrl = '/minivuabai/';
        this.urlApi = '/minivuabai/api/VuaBaiApi/';
        this.signalRUrl = '/minivuabai/';
        this.imageUrl = '/minivuabai/Images/';
        this.GameHub = "";
    } else if (window.location.href.indexOf("http://beta.vuachoibai.com") > -1) {
        this.rootUrl = 'http://beta.vuachoibai.com/minivuabai/';
        this.cssUrl = 'http://beta.vuachoibai.com/minivuabai/css/';
        this.MediaUrl = 'http://beta.vuachoibai.com/minivuabai/';
        this.urlApi = 'http://betamini.vuachoibai.com/minivuabai/api/VuaBaiApi/';
        this.signalRUrl = 'http://betamini.vuachoibai.com/minivuabai/';
        this.imageUrl = 'http://beta.vuachoibai.com/minivuabai/images/';
        this.StarServiceUrl = "http://beta.vuachoibai.com/starservices/";
    } else if (window.location.href.indexOf("http://vuachoibai.com") > -1) {
        this.rootUrl = 'http://vuachoibai.com/minivuabai/';
        this.cssUrl = 'http://vuachoibai.com/minivuabai/css/';
        this.MediaUrl = 'http://vuachoibai.com/minivuabai/';
        this.urlApi = 'http://vuabaiw.vuachoibai.com/api/VuaBaiApi/';
        this.signalRUrl = 'http://vuabaiw.vuachoibai.com/';
        this.imageUrl = 'http://vuachoibai.com/minivuabai/images/';
        this.StarServiceUrl = "http://vuachoibai.com/starservices/"
    } else if (window.location.href.indexOf("http://vuachoibai.net") > -1) {
        this.rootUrl = 'http://vuachoibai.net/minivuabai/';
        this.cssUrl = 'http://vuachoibai.net/minivuabai/css/';
        this.MediaUrl = 'http://vuachoibai.net/minivuabai/';
        this.urlApi = 'http://vuabaiw.vuachoibai.net/api/VuaBaiApi/';
        this.signalRUrl = 'http://vuabaiw.vuachoibai.net/';
        this.imageUrl = 'http://vuachoibai.net/minivuabai/images/';
        this.StarServiceUrl = "http://vuachoibai.net/starservices/"
    } else if (window.location.href.indexOf("http://vuachoibai.org") > -1) {
        this.rootUrl = 'http://vuachoibai.org/minivuabai/';
        this.cssUrl = 'http://vuachoibai.org/minivuabai/css/';
        this.MediaUrl = 'http://vuachoibai.org/minivuabai/';
        this.urlApi = 'http://vuabaiw.vuachoibai.org/api/VuaBaiApi/';
        this.signalRUrl = 'http://vuabaiw.vuachoibai.org/';
        this.imageUrl = 'http://vuachoibai.org/minivuabai/images/';
        this.StarServiceUrl = "http://vuachoibai.org/starservices/"
    }
    this.GameHub = "";

    this.version = '1.0';
    this.totalPage = 0;
    this.recordPage = 9;
    this.recordPageHistory = 10;
    this.oldJackpotValue = 0;
    this.sao = 1;
    this.timeIntervalJackpot = 0;
    this.positionMaingame = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1];
    this.numberPacketMainGame = 20;
    this.justCancelAutoMainGame = false;
    this.isAutoMainGame = false;
    this.roomID = 1;
    this.countQuay = 0;

    this.Inited = false;
    this.InitInterface = false;
    this.Init = function () {
        if (miniVuabai.Inited) {
            $("#xxkingpocker").css("opacity", "1");
            //$("#miniVuabai").css("display", "block");
            return;
        }
        miniVuabai.Inited = true;
        this.LoadResource(function () {
            new window.HubManager();
            miniVuabai.bindInterface();
        });
    };
    this.LoadResource = function (callback) {
        var verLoad = '?v=1.1.11';
        var resource = [
            { id: 'HubManager', src: miniVuabai.rootUrl + 'Scripts/HubManager.js' + verLoad },
            { id: 'GameHub', src: miniVuabai.rootUrl + 'Scripts/GameHub.js' + verLoad },
            //{ id: 'pager', src: miniVuabai.rootUrl + 'Scripts/pager.js' },

            { id: 'minislotcss', src: miniVuabai.rootUrl + 'css/vuabai.css' + verLoad }
        ];
        commonGame.loadFile(resource, function () {
            if (callback) {
                callback();
            }
        });
        // queueVuabai = new createjs.LoadQueue(false);
        // queueVuabai.addEventListener("complete", function () {
        // if (callback) {
        // callback();
        // }
        // });
        // queueVuabai.loadManifest(resource);
    };

    this.bindInterface = function (rootEle) {
        if (miniVuabai.InitInterface) {
            return;
        }
        miniVuabai.InitInterface = true;
        if ($("#contentMiniSlot").length > 0) {
            $("#contentMiniSlot").remove();
        }
        //urlAPI
        var html = '<div style="z-index: 999;display:block;" class="vb_main" id="miniVuabai">';
        var vb_thongtin = '<ul class="vb_thongtin">';
        vb_thongtin += '<li class="vb_icon_cup"><a href="javascript:miniVuabai.vinhdanh();"></a></li>';
        vb_thongtin += '<li class="vb_icon_info"><a href="javascript:miniVuabai.history();"></a></li>';
        vb_thongtin += '<li class="vb_icon_cauhoi"><a href="javascript:;" onclick="miniVuabai.popHuongdan();"></a></li>';
        vb_thongtin += '</ul>';
        html += vb_thongtin;
        var vb_luachon = '<ul class="vb_luachon">';
        vb_luachon += '<li class="vb_bansao"><a id="vb_tiensao" href="javascript:;" onclick="miniVuabai.chonsao(1);"></a></li>';
        vb_luachon += '<li class="vb_banxu"><a id="vb_tienxu" href="javascript:;" onclick="miniVuabai.chonsao(2);"></a></li>';
        vb_luachon += '</ul>';
        html += vb_luachon;
        html += '<a class="vb_btn_close" href="javascript:commonGame.showhide(4);"></a>';
        var hu_sao = '<div class="vb_quythuong">';
        hu_sao += '<span>Quỹ thưởng</span>';
        hu_sao += '<p id="vb_sohuloc">0</p>';
        hu_sao += '</div>';
        html += hu_sao;
        html += '<div class="vb_may-quay">';
        //line 2 bên
        html += '<div style="float: left;height: 163px;width: 21px;position:absolute;">' +
            '<div style="float: left; height: 163px;width: 234px;position: absolute;z-index: 0;" id="vb_divanh"></div>' +
            '<div style="width: 21px;height: 14.2px;z-index: 1000;position: absolute;top:9px;" id="vb_line1"></div>' +
            '<div style="width: 21px;height: 14.2px;z-index: 1000;position: absolute;top: 23.2px;" id="vb_line6"></div>' +
            '<div style="width: 21px;height: 14.2px;z-index: 1000;position: absolute;top: 38.4px;" id="vb_line12"></div>' +
            '<div style="width: 21px;height: 14.2px;z-index: 1000;position: absolute;top: 53.6px;" id="vb_line14"></div>' +
            '<div style="width: 21px;height: 14.2px;z-index: 1000;position: absolute;top: 67.8px;" id="vb_line2"></div>' +
            '<div style="width: 21px;height: 14.2px;z-index: 1000;position: absolute;top: 82.0px;" id="vb_line13"></div>' +
            '<div style="width: 21px;height: 14.2px;z-index: 1000;position: absolute;top: 97.2px;" id="vb_line9"></div>' +
            '<div style="width: 21px;height: 14.2px;z-index: 1000;position: absolute;top: 112.4px;" id="vb_line11"></div>' +
            '<div style="width: 21px;height: 14.2px;z-index: 1000;position: absolute;top: 125.6px;" id="vb_line3"></div>' +
            '<div style="width: 21px;height: 14.2px;z-index: 1000;position: absolute;top: 140.8px;" id="vb_line17"></div>' +
            '</div>';
        html += '<canvas id="vb_easelCanvasVuabai" width="233" height="155" style="margin:4px 0  0 0;"></canvas>';
        html += '<div style="height: 163px;width: 21px;position: absolute;left: 211px;top: 0px;">' +
            '<div style="width: 21px;height: 14.2px;z-index: 1000;position: absolute;top:9px;" id="vb_line4"></div>' +
            '<div style="width: 21px;height: 14.2px;z-index: 1000;position: absolute;top: 23.2px;" id="vb_line8"></div>' +
            '<div style="width: 21px;height: 14.2px;z-index: 1000;position: absolute;top: 38.4px;" id="vb_line16"></div>' +
            '<div style="width: 21px;height: 14.2px;z-index: 1000;position: absolute;top: 53.6px;" id="vb_line15"></div>' +
            '<div style="width: 21px;height: 14.2px;z-index: 1000;position: absolute;top: 67.8px;" id="vb_line20"></div>' +
            '<div style="width: 21px;height: 14.2px;z-index: 1000;position: absolute;top: 82.0px;" id="vb_line10"></div>' +
            '<div style="width: 21px;height: 14.2px;z-index: 1000;position: absolute;top: 97.2px;" id="vb_line18"></div>' +
            '<div style="width: 21px;height: 14.2px;z-index: 1000;position: absolute;top: 112.4px;" id="vb_line7"></div>' +
            '<div style="width: 21px;height: 14.2px;z-index: 1000;position: absolute;top: 125.6px;" id="vb_line5"></div>' +
            '<div style="width: 21px;height: 14.2px;z-index: 1000;position: absolute;top: 140.8px;" id="vb_line19"></div>' +
            '</div>';
        html += '</div>';
        var vb_room = '<div class="vb_coin">';
        vb_room += '<ul>';
        vb_room += '<li id="vb_tien1" onclick="miniVuabai.chontien(1);"></li>';
        vb_room += '<li id="vb_tien2" onclick="miniVuabai.chontien(2);"></li>';
        vb_room += '<li id="vb_tien4" onclick="miniVuabai.chontien(4);"></li>';
        vb_room += '<li id="vb_tien3" onclick="miniVuabai.chontien(3);"></li>';
        vb_room += '</ul>';
        vb_room += '</div>';
        html += vb_room;
        var vb_datcuoc = '<div id="vb_datcuoc" class="vb_tieude" style="display: none;">';
        vb_datcuoc += '<div>';
        vb_datcuoc += '<h1>DÒNG ĐẶT CƯỢC</h1>';
        vb_datcuoc += '<a href="javascript:miniVuabai.onClickRemoveSelectionpacket();">Bỏ chọn</a>';
        vb_datcuoc += '</div>';
        vb_datcuoc += '<div>';
        vb_datcuoc += '<ul>';
        vb_datcuoc += '<li id="vb_so1" class="vb_selectline" onclick="miniVuabai.onClickInPacket(1);"></li>';
        vb_datcuoc += '<li id="vb_so2" class="vb_selectline" onclick="miniVuabai.onClickInPacket(2);"></li>';
        vb_datcuoc += '<li id="vb_so3" class="vb_selectline" onclick="miniVuabai.onClickInPacket(3);"></li>';
        vb_datcuoc += '<li id="vb_so4" class="vb_selectline" onclick="miniVuabai.onClickInPacket(4);"></li>';
        vb_datcuoc += '<li id="vb_so5" class="vb_selectline" onclick="miniVuabai.onClickInPacket(5);"></li>';
        vb_datcuoc += '<li id="vb_so6" class="vb_selectline" onclick="miniVuabai.onClickInPacket(6);"></li>';
        vb_datcuoc += '<li id="vb_so7" class="vb_selectline" onclick="miniVuabai.onClickInPacket(7);"></li>';
        vb_datcuoc += '<li id="vb_so8" class="vb_selectline" onclick="miniVuabai.onClickInPacket(8);"></li>';
        vb_datcuoc += '<li id="vb_so9" class="vb_selectline" onclick="miniVuabai.onClickInPacket(9);"></li>';
        vb_datcuoc += '<li id="vb_so10" class="vb_selectline" onclick="miniVuabai.onClickInPacket(10);"></li>';
        vb_datcuoc += '<li id="vb_so11" class="vb_selectline" onclick="miniVuabai.onClickInPacket(11);"></li>';
        vb_datcuoc += '<li id="vb_so12" class="vb_selectline" onclick="miniVuabai.onClickInPacket(12);"></li>';
        vb_datcuoc += '<li id="vb_so13" class="vb_selectline" onclick="miniVuabai.onClickInPacket(13);"></li>';
        vb_datcuoc += '<li id="vb_so14" class="vb_selectline" onclick="miniVuabai.onClickInPacket(14);"></li>';
        vb_datcuoc += '<li id="vb_so15" class="vb_selectline" onclick="miniVuabai.onClickInPacket(15);"></li>';
        vb_datcuoc += '<li id="vb_so16" class="vb_selectline" onclick="miniVuabai.onClickInPacket(16);"></li>';
        vb_datcuoc += '<li id="vb_so17" class="vb_selectline" onclick="miniVuabai.onClickInPacket(17);"></li>';
        vb_datcuoc += '<li id="vb_so18" class="vb_selectline" onclick="miniVuabai.onClickInPacket(18);"></li>';
        vb_datcuoc += '<li id="vb_so19" class="vb_selectline" onclick="miniVuabai.onClickInPacket(19);"></li>';
        vb_datcuoc += '<li id="vb_so20" class="vb_selectline" onclick="miniVuabai.onClickInPacket(20);"></li>';
        vb_datcuoc += '</ul>';
        vb_datcuoc += '</div>';
        vb_datcuoc += '<div>';
        vb_datcuoc += '<li onclick="miniVuabai.onClickInPacketAll();">Tất cả</li>';
        vb_datcuoc += '<li onclick="miniVuabai.onClickInPacketChan();">Chẵn</li>';
        vb_datcuoc += '<li onclick="miniVuabai.onClickInPacketLe();">Lẻ</li>';
        vb_datcuoc += '<li onclick="miniVuabai.confirmChondong();"></li>';
        vb_datcuoc += '</div>';
        vb_datcuoc += '</div>';
        html += vb_datcuoc;
        var vb_btn = '<h3 id="vb_button_chondong" class="chon-line" onclick="miniVuabai.chondong();" >' + miniVuabai.numberPacketMainGame + ' DÒNG</h3>';
        vb_btn += '<div class="vb_btn-quay">';
        vb_btn += '<ul>';
        vb_btn += '<li id="button_quay" onclick="miniVuabai.run(false);"></li>';
        vb_btn += '<li onclick="miniVuabai.run(true);" id="button_tuquay"></li>';
        vb_btn += '<li style="display: none;" id="button_stoptuquay" onclick="miniVuabai.stopAutoMainGame();"></li>';
        vb_btn += '</ul>';
        vb_btn += '</div>';
        html += vb_btn;

        if (typeof rootEle == 'undefined' || rootEle == '')
            rootEle = 'BODY';
        $("#miniVuabai1").append(html);


        $("#miniVuabai").css("left", "0px");
        $("#miniVuabai").css("top", "72px");
        $("#miniVuabai").css("cursor", "pointer");
        $("#miniVuabai").css("position", "absolute");

        $("#miniVuabai").draggable({ containment: "window", cancel: 'object' });

        //Xu li canvas
        var canvasvuabai = null;
        canvasvuabai = document.getElementById("vb_easelCanvasVuabai");
        stageVuabai = new createjs.Stage(canvasvuabai);
        stageVuabai.enableMouseOver(10);
        createjs.Ticker.setFPS(30);
        createjs.Ticker.addEventListener("tick", miniVuabai.tick);

        miniVuabai.LoadImages();

    };

    this.tick = function (event) {
        stageVuabai.update();
    };

    this.LoadImages = function () {
        var settingSlotVuabai =
        [
            { src: miniVuabai.imageUrl + "jocker.png", id: "quan(1)" },
            { src: miniVuabai.imageUrl + "gg1.png", id: "quan(2)" },
            { src: miniVuabai.imageUrl + "gg3.png", id: "quan(3)" },
            { src: miniVuabai.imageUrl + "soQ.png", id: "quan(4)" },
            { src: miniVuabai.imageUrl + "soJ.png", id: "quan(5)" },
            { src: miniVuabai.imageUrl + "gg2.png", id: "quan(6)" }
        ];
        queueVuabai = new createjs.LoadQueue(true, "", true);
        queueVuabai.addEventListener("complete", function () {
            miniVuabai.render();
        });
        queueVuabai.loadManifest(settingSlotVuabai);
    };
    this.numberY = 50;
    this.vb_resultOld = [];
    this.render = function (event) {
        slotVB1 = new createjs.Container();
        slotVB1.set({ x: 33, y: 5 });

        slotVB2 = new createjs.Container();
        slotVB2.set({ x: 95, y: 5 });

        slotVB3 = new createjs.Container();
        slotVB3.set({ x: 160, y: 5 });

        for (var k = 0; k < 9; k++) {
            miniVuabai.vb_resultOld.push(Math.floor(Math.random() * 6) + 1);
        }

        for (var l = 0; l < 3; l++) {
            var itemImg1 = new createjs.Bitmap(queueVuabai.getResult("quan(" + miniVuabai.vb_resultOld[l * 3] + ")"));
            itemImg1.y = l * miniVuabai.numberY;
            slotVB1.addChild(itemImg1);

            var itemImg2 = new createjs.Bitmap(queueVuabai.getResult("quan(" + miniVuabai.vb_resultOld[1 + l * 3] + ")"));
            itemImg2.y = l * miniVuabai.numberY;
            slotVB2.addChild(itemImg2);

            var itemImg3 = new createjs.Bitmap(queueVuabai.getResult("quan(" + miniVuabai.vb_resultOld[2 + l * 3] + ")"));
            itemImg3.y = l * miniVuabai.numberY;
            slotVB3.addChild(itemImg3);
        }

        stageVuabai.addChild(slotVB1, slotVB2, slotVB3);
        stageVuabai.update();

        for (var i = 1; i < 21; i++) {
            hoverline02(i);
        }
    };

    this.startHub = function () {
        try {
            miniVuabai.GameHub.connection.start({ transport: Config.transports, waitForPageLoad: false }).done(function () {
                // Đã kết nối
                console.log("vuabai connected");
                miniVuabai.chontien(1);
            });
        } catch (e) {
        }
    }

    this.stopHub = function () {
        try {
            miniVuabai.GameHub.connection.stop();
        } catch (e) {
        }
    }

    //Ham cap nhat Jackpot, quy thuong
    this.CountUp = function (target, startVal, endVal, decimals, duration, options) {
        this.options = options || {
            useEasing: true, // toggle easing
            useGrouping: true, // 1,000,000 vs 1000000
            separator: '.', // character to use as a separator
            decimal: '.' // character to use as a decimal
        };
        var lastTime = 0;
        var vendors = ['webkit', 'moz', 'ms'];
        for (var x = 0; x < vendors.length && !window.requestAnimationFrame; ++x) {
            window.requestAnimationFrame = window[vendors[x] + 'RequestAnimationFrame'];
            window.cancelAnimationFrame =
              window[vendors[x] + 'CancelAnimationFrame'] || window[vendors[x] + 'CancelRequestAnimationFrame'];
        }
        if (!window.requestAnimationFrame) {
            window.requestAnimationFrame = function (callback, element) {
                var currTime = new Date().getTime();
                var timeToCall = Math.max(0, 16 - (currTime - lastTime));
                var id = window.setTimeout(function () { callback(currTime + timeToCall); },
                    timeToCall);
                lastTime = currTime + timeToCall;
                return id;
            };
        }
        if (!window.cancelAnimationFrame) {
            window.cancelAnimationFrame = function (id) {
                clearTimeout(id);
            };
        }

        var self = this;

        this.d = (typeof target === 'string') ? document.getElementById(target) : target;
        this.startVal = Number(startVal);
        this.endVal = Number(endVal);
        this.countDown = (this.startVal > this.endVal) ? true : false;
        this.startTime = null;
        this.timestamp = null;
        this.remaining = null;
        this.frameVal = this.startVal;
        this.rAF = null;
        this.decimals = Math.max(0, decimals || 0);
        this.dec = Math.pow(10, this.decimals);
        this.duration = duration * 1000 || 2000;

        // Robert Penner's easeOutExpo
        this.easeOutExpo = function (t, b, c, d) {
            return c * (-Math.pow(2, -10 * t / d) + 1) * 1024 / 1023 + b;
        };
        this.count = function (timestamp) {

            if (self.startTime === null) self.startTime = timestamp;

            self.timestamp = timestamp;

            var progress = timestamp - self.startTime;
            self.remaining = self.duration - progress;

            // to ease or not to ease
            if (self.options.useEasing) {
                if (self.countDown) {
                    var i = self.easeOutExpo(progress, 0, self.startVal - self.endVal, self.duration);
                    self.frameVal = self.startVal - i;
                } else {
                    self.frameVal = self.easeOutExpo(progress, self.startVal, self.endVal - self.startVal, self.duration);
                }
            } else {
                if (self.countDown) {
                    var i = (self.startVal - self.endVal) * (progress / self.duration);
                    self.frameVal = self.startVal - i;
                } else {
                    self.frameVal = self.startVal + (self.endVal - self.startVal) * (progress / self.duration);
                }
            }

            // decimal
            self.frameVal = Math.round(self.frameVal * self.dec) / self.dec;

            // don't go past endVal since progress can exceed duration in the last frame
            if (self.countDown) {
                self.frameVal = (self.frameVal < self.endVal) ? self.endVal : self.frameVal;
            } else {
                self.frameVal = (self.frameVal > self.endVal) ? self.endVal : self.frameVal;
            }

            // format and print value
            self.d.innerHTML = self.formatNumber(self.frameVal.toFixed(self.decimals));

            // whether to continue
            if (progress < self.duration) {
                self.rAF = requestAnimationFrame(self.count);
            } else {
                if (self.callback != null) self.callback();
            }
        };
        this.start = function (callback) {
            self.callback = callback;
            // make sure values are valid
            if (!isNaN(self.endVal) && !isNaN(self.startVal)) {
                self.rAF = requestAnimationFrame(self.count);
            } else {
                console.log('countUp error: startVal or endVal is not a number');
                self.d.innerHTML = '--';
            }
            return false;
        };
        this.stop = function () {
            cancelAnimationFrame(self.rAF);
        };
        this.reset = function () {
            self.startTime = null;
            cancelAnimationFrame(self.rAF);
            self.d.innerHTML = self.formatNumber(self.startVal.toFixed(self.decimals));
        };
        this.resume = function () {
            self.startTime = null;
            self.duration = self.remaining;
            self.startVal = self.frameVal;
            requestAnimationFrame(self.count);
        }
        this.formatNumber = function (nStr) {
            nStr += '';
            var x, x1, x2, rgx;
            x = nStr.split('.');
            x1 = x[0];
            x2 = x.length > 1 ? self.options.decimal + x[1] : '';
            rgx = /(\d+)(\d{3})/;
            if (self.options.useGrouping) {
                while (rgx.test(x1)) {
                    x1 = x1.replace(rgx, '$1' + self.options.separator + '$2');
                }
            }
            return x1 + x2;
        };

        // format startVal on initialization
        self.d.innerHTML = self.formatNumber(self.startVal.toFixed(self.decimals));
    };

    this.run = function (isAuto) {

        if (miniVuabai.roomID != 0) {
            if (miniVuabai.numberPacketMainGame > 0) {
                $("#vb_button_chondong").removeAttr("onclick");
                $("#button_tuquay").removeAttr("onclick");
                $("#button_quay").removeAttr("onclick");
                $("#vb_tien1").removeAttr("onclick");
                $("#vb_tien2").removeAttr("onclick");
                $("#vb_tien3").removeAttr("onclick");
                $("#vb_tien4").removeAttr("onclick");
                $("#vb_tiensao").removeAttr("onclick");
                $("#vb_tienxu").removeAttr("onclick");
                if (isAuto) {
                    miniVuabai.isAutoMainGame = true;
                    $("#button_tuquay").css('display', 'none');
                    $("#button_stoptuquay").css('display', 'block');
                    $("#button_quay").css('opacity', '0.3');
                } else {
                    $("#button_tuquay").css('display', 'block');
                    $("#button_stoptuquay").css('display', 'none');
                    $("#button_quay").css('opacity', '1');
                    miniVuabai.isAutoMainGame = false;
                }
                miniVuabai.spins();
            } else {
                //Thong bao chua chon line
                miniVuabai.showError(-3000);
            }
        } else {
            //Thong bao chua chon phong choi
            miniVuabai.showError(-3001);
        }

    };

    this.spins = function () {

        var lineData = "";
        for (var i = 0; i < miniVuabai.positionMaingame.length; i++) {
            if (miniVuabai.positionMaingame[i] == 1) {
                lineData += i + 1;
                lineData += ",";
            }
        }
        lineData = lineData.substring(0, lineData.length - 1);
        if (lineData.length > 0) {

            miniVuabai.GameHub.server.UserSpin(miniVuabai.roomID, miniVuabai.sao, lineData).done(function (result) {
                if (result < 0) {
                    if (result === -999) {
                        miniVuabai.ShowMessage("Bạn vui lòng đăng nhập", true);
                    } else if (result === -3000) {
                        miniVuabai.ShowMessage("Bạn chưa chọn dòng cược", true);
                    } else if (result == -3001) {
                        miniVuabai.ShowMessage("Bạn chưa chọn phòng chơi", true);
                    } else if (result == -232) {
                        miniVuabai.ShowMessage("Đặt cược không hợp lệ", true);
                    } else if (result == -51) {
                        miniVuabai.ShowMessage("Số dư của bạn không đủ", true);
                    } else if (result == -48) {
                        miniVuabai.ShowMessage("Game đã bị khóa", true);
                        Util.showMessage("Game đã bị khóa, vui lòng mở khóa để tiếp tục giao dịch");
                    } else {
                        miniVuabai.ShowMessage("Lượt quay thất bại. Xin vui lòng thử lại");
                    }
                }
            }).fail(function () {
                miniVuabai.ShowMessage("Lượt quay thất bại. Xin vui lòng thử lại");
            });
        } else {

        }
    };
    this.ShowMessage = function (message, isStop) {
        var htmlerror = '<div style="float: left; height: 163px;width: 234px;position: absolute;' +
            'z-index: 0;background-color: rgba(13, 12, 12, 0.49);"> <span style="font: bold 18px Arial, ' +
            'Helvetica,sans-serif;color: #feef00;width: 231px;top: 73px;text-align: center;' +
            'font-size:0.8em;position:absolute;z-index:4;">' + message + '</span></div>';
        $("#vb_divanh").html(htmlerror);
        if (isStop) {
            miniVuabai.isAutoMainGame = false;
        }
        miniVuabai.defaultButton(1000);
        miniVuabai.stopAutoMainGame();
    };
    this.RenderResult = function (result) {
        if (result._ResponseStatus >= 0) {
            var arraySlotData = result._SlotsData.split(',');

            slotVB1.removeAllChildren();
            slotVB2.removeAllChildren();
            slotVB3.removeAllChildren();
            for (var m = 0; m < 3; m++) {
                var itemImg11 = new createjs.Bitmap(queueVuabai.getResult("quan(" + arraySlotData[m * 3] + ")"));
                itemImg11.y = m * miniVuabai.numberY;
                slotVB1.addChild(itemImg11);

                var itemImg21 = new createjs.Bitmap(queueVuabai.getResult("quan(" + arraySlotData[1 + m * 3] + ")"));
                itemImg21.y = m * miniVuabai.numberY;
                slotVB2.addChild(itemImg21);

                var itemImg31 = new createjs.Bitmap(queueVuabai.getResult("quan(" + arraySlotData[2 + m * 3] + ")"));
                itemImg31.y = m * miniVuabai.numberY;
                slotVB3.addChild(itemImg31);
            }
            for (var i = 1; i <= 3; i++) {
                for (var j = 3; j < 47; j++) {
                    var random_number = Math.floor(Math.random() * 6) + 1;
                    var item = new createjs.Bitmap(queueVuabai.getResult("quan(" + random_number + ")"));
                    item.x = 0;
                    item.y = j * miniVuabai.numberY;
                    if (i == 1) {
                        slotVB1.addChild(item);
                    } else if (i == 2) {
                        slotVB2.addChild(item);
                    } else {
                        slotVB3.addChild(item);
                    }
                }
            }
            for (var l = 0; l < 3; l++) {
                var itemImg1 = new createjs.Bitmap(queueVuabai.getResult("quan(" + miniVuabai.vb_resultOld[l * 3] + ")"));
                itemImg1.y = (47 + l) * miniVuabai.numberY;
                slotVB1.addChild(itemImg1);

                var itemImg2 = new createjs.Bitmap(queueVuabai.getResult("quan(" + miniVuabai.vb_resultOld[1 + l * 3] + ")"));
                itemImg2.y = (47 + l) * miniVuabai.numberY;
                slotVB2.addChild(itemImg2);

                var itemImg3 = new createjs.Bitmap(queueVuabai.getResult("quan(" + miniVuabai.vb_resultOld[2 + l * 3] + ")"));
                itemImg3.y = (47 + l) * miniVuabai.numberY;
                slotVB3.addChild(itemImg3);
            }

            slotVB1.y = slotVB2.y = slotVB3.y = 5 - miniVuabai.numberY * 47;
            miniVuabai.vb_resultOld = new Array();
            for (var k = 0; k < 9; k++) {
                miniVuabai.vb_resultOld.push(arraySlotData[k]);
            }
            stageVuabai.update();
            createjs.Tween.get(slotVB1, { loop: false, override: true }).to({ y: 5, alpha: 1 }, 3000, createjs.Ease.cubicInOut).call(function () {
                for (var n = 49; n > 2; n--) {
                    slotVB1.removeChildAt(n);
                }
            });
            createjs.Tween.get(slotVB2, { loop: false, override: true }).wait(250).to({ y: 5, alpha: 1 }, 3000, createjs.Ease.cubicInOut).call(function () {
                for (var n = 49; n > 2; n--) {
                    slotVB2.removeChildAt(n);
                }
            });
            createjs.Tween.get(slotVB3, { loop: false, override: true }).wait(500).to({ y: 5, alpha: 1 }, 3000, createjs.Ease.cubicInOut).call(function () {
                for (var n = 49; n > 2; n--) {
                    slotVB3.removeChildAt(n);
                }
            });
            miniVuabai.countQuay++;
            var listPrizeData = [];
            var listLine = [];
            if (result._PrizesData != '') {
                listPrizeData = result._PrizesData.split(";");
            }
            if (listPrizeData.length > 0) {
                for (var i = 0; i < listPrizeData.length; i++) {
                    listLine.push(listPrizeData[i].split(",")[0]);
                }
            }

            setTimeout(function () {
                var checkFreeze = result.IsAutoFreeze;
                setTimeout(function () {
                    if (checkFreeze > 0) {
                        $.ajax({
                            type: "GET",
                            url: miniVuabai.StarServiceUrl + "api/AccountSecurity/GetSmsPlusMobileInfo",
                            crossDomain: true,
                            xhrFields: {
                                withCredentials: true
                            },
                            success: function (data) {
                                if (data.responseCode > 0) {
                                    //Đã ĐK SMS plus
                                    Util.showMessage('<span style="font-weight:bold;font-size: 15px;">Chúc mừng bạn đã thắng ' + Util.parseMoney(result._TotalPrizeValue) + ' Sao, phiên ' + result._SpinID + ' trong game Mini Vua Bài.' +
                                        ' Để bảo mật, hệ thống đã tự đóng băng giúp bạn. Để sử dụng hãy tiến hành mở băng tài khoản ' +
                                        '<a style=" color: yellow; text-decoration: underline; " href="javascript:;" onclick="AccountInfo.showPopupAccInfo(3);">tại đây</a></span>', "", true);
                                    $("#popupwrap h2").html("CHÚC MỪNG")
                                } else {
                                    //Chưa ĐK SMS plus
                                    Util.showMessage('<span style="font-weight:bold;font-size: 15px;">Chúc mừng bạn đã thắng ' + Util.parseMoney(result._TotalPrizeValue) + ' Sao, phiên ' + result._SpinID + ' trong game Mini Vua Bài.' +
                                        ' Để bảo mật, hệ thống đã tự đóng băng giúp bạn. Để sử dụng vui lòng đăng ký SMS Plus và mở băng tài khoản ' +
                                        '<a style=" color: yellow; text-decoration: underline; " href="javascript:;" onclick="AccountInfo.showPopupAccInfo(3);">tại đây</a></span>', "", true);
                                    $("#popupwrap h2").html("CHÚC MỪNG");
                                }
                            }
                        });
                    }
                }, 11500);

                //setTimeout(function () {
                //    if (eventVuaBai2016T8) {
                //        eventVuaBai2016T8.EffectEvent(Math.floor(Math.random() * 6) + 1);
                //    }
                //}, 1000);

                var number = 0;
                if (result.IsX2 == 1) {
                    number = result.TotalSo10;
                }

                if (result._IsJackpot) { //no quy
                    miniVuabai.showresult(number, 1, result._TotalPrizeValue, listLine);

                } else if (result._TotalPrizeValue >= 7 * result._TotalBetValue) {
                    miniVuabai.showresult(number, 2, result._TotalPrizeValue, listLine); //Thang lon
                } else if (result._TotalPrizeValue > 0) { //Thang bt
                    miniVuabai.showresult(number, 3, result._TotalPrizeValue, listLine);
                } else {
                    miniVuabai.showresult(number, 0);
                }
                setTimeout(function () {
                    miniVuabai.updateBalance(result._Balance, miniVuabai.sao);
                }, 1500);
            }, 3500);

        } else {
            if (miniVuabai.isAutoMainGame) {
                miniVuabai.stopAutoMainGame();
            }
            miniVuabai.showError(result._ResponseStatus);
            miniVuabai.showresult(0);
        }

    }
    this.showError = function (error) {
        var htmlerror_check = '';
        switch (error) {
            case -3000:
                htmlerror_check = 'Bạn chưa chọn dòng cược';
                break;
            case -3001:
                htmlerror_check = 'Bạn chưa chọn phòng chơi';
                break;
            case -232:
                htmlerror_check = 'Đặt cược không hợp lệ';
                break;
            case -999:
                htmlerror_check = 'Bạn vui lòng đăng nhập';
                break;
            default:
                htmlerror_check = 'Hệ thống bận. Vui lòng thử lại sau';
                break;
        }
        var htmlerror = '<div style="float: left; height: 163px;width: 234px;position: absolute;' +
                    'z-index: 0;background-color: rgba(13, 12, 12, 0.49);"> <span style="font: bold 18px Arial, ' +
                    'Helvetica,sans-serif;color: #feef00;width: 231px;top: 73px;text-align: center;' +
                    'font-size:0.8em;position:absolute;z-index:4;">' + htmlerror_check + '</span></div>';
        $("#vb_divanh").html(htmlerror);
    };
    this.updateBalance = function (totalValue, type) {
        if (type == 1) {
            $('.sao-number p').html(miniVuabai.FormatNumber(totalValue));
        } else {
            $('.xu-number p').html(miniVuabai.FormatNumber(totalValue));
        }
    };
    this.stopAutoMainGame = function () {
        $("#button_stoptuquay").css('display', 'none');
        $("#button_tuquay").css('display', 'block');
        $("#button_quay").css('opacity', '1');
        miniVuabai.isAutoMainGame = false;
    };

    //Tra thuong va xu li auto
    this.showresult = function (number, type, tienthang, listLine) {
        var time = 0;
        if (number > 0) {
            time = 2000;
        }
        switch (type) {
            case 0://Thua
                miniVuabai.defaultButton(1500 + time);
                if (number > 0) {
                    setTimeout(function () {
                        eventVuaBai2016T8.EffectEvent(number);
                    }, 1500);
                }
                break;
            case 1://No quy
                miniVuabai.sangline(listLine, tienthang, type);
                miniVuabai.defaultButton(10000 + time);
                if (number > 0) {
                    setTimeout(function () {
                        eventVuaBai2016T8.EffectEvent(number);
                    }, 10000);
                }
                break;
            case 2://Thang lon
                miniVuabai.sangline(listLine, tienthang, type);
                miniVuabai.defaultButton(5000 + time);
                if (number > 0) {
                    setTimeout(function () {
                        eventVuaBai2016T8.EffectEvent(number);
                    }, 5000);
                }
                break;
            case 3://Thang bt
                miniVuabai.sangline(listLine, tienthang, type);
                miniVuabai.defaultButton(1500 + time);
                if (number > 0) {
                    setTimeout(function () {
                        eventVuaBai2016T8.EffectEvent(number);
                    }, 1500);
                }
                break;
            default:
                miniVuabai.defaultButton(2000 + time);
                if (number > 0) {
                    setTimeout(function () {
                        eventVuaBai2016T8.EffectEvent(number);
                    }, 2000);
                }
                break;
        }

    };

    this.FormatNumber = function (p_sStringNumber) {
        p_sStringNumber += '';
        var x = p_sStringNumber.split(',');
        var x1 = x[0];
        var x2 = x.length > 1 ? ',' + x[1] : '';
        var rgx = /(\d+)(\d{3})/;
        while (rgx.test(x1))
            x1 = x1.replace(rgx, '$1' + '.' + '$2');

        return x1 + x2;
    };

    this.timeIntervalSangline = 0;
    this.countSangline = 0;

    //sang line
    this.sangline = function (listLine, tienthang, type) {
        var htmlanhline = '';
        for (var i = 0; i < listLine.length; i++) {
            htmlanhline += '<img src="' + miniVuabai.imageUrl + 'so' + listLine[i] + '.png"/>';
        }
        if (type == 1) {
            htmlanhline += '<img src="' + miniVuabai.imageUrl + 'noquy.png" style="left: 39px;top: 12px;width: 155px;z-index: 3;" id="vb_anh_sangline">';
            htmlanhline += '<span id="vb_tienthang" class="bounceIn" style="font: bold 18px Arial, Helvetica, ' +
                'sans-serif;color: #F1E961;width: 234px;top: 110px;text-align: center;font-size: ' +
                '22px;position:absolute;z-index:4;">0</span>';
        } else if (type == 2) {
            htmlanhline += '<span id="vb_tienthang" class="bounceIn" style="font: bold 18px Arial, Helvetica, ' +
                'sans-serif;color: #F1E961;width: 234px;top: 109px;text-align: center;font-size: 20px;' +
                'position:absolute;z-index:4;">0</span>';
            htmlanhline += '<img src="' + miniVuabai.imageUrl + 'winner.png" style=" left: 7px;top: 3px;width: 223px;z-index: 3;" id="vb_anh_sangline">';
        } else if (type == 3) {
            htmlanhline += '<span id="vb_tienthang" class="bounceIn" style="font: bold 18px Arial, Helvetica, ' +
                'sans-serif;color: #F1E961;width: 234px;top: 65px;text-align: center;font-size: 22px;' +
                'position:absolute;z-index:4;">' + miniVuabai.FormatNumber(tienthang.toString()) + '</span>';
            htmlanhline += '<img src="' + miniVuabai.imageUrl + 'win.png" style=" left: 35px;top: 47px;width: 168px;z-index: 3;" id="vb_anh_sangline">';
        }
        $('#vb_divanh').html(htmlanhline);
        miniVuabai.countSangline = 0;
        miniVuabai.callTimerSangline(type);

        if (type == 1 || type == 2) {
            var s = new miniVuabai.CountUp("vb_tienthang", 0, tienthang, 0, 1);
            s.start();
        }
        for (var j = 1; j < 21; j++) {
            nonHoverline02(j, htmlanhline);
        }
    };

    this.callTimerSangline = function (type) {
        // Call function with 1000 milliseconds gap
        clearInterval(miniVuabai.timeIntervalSangline);
        miniVuabai.timeIntervalSangline = setInterval(function () { miniVuabai.doianh(type); }, 200);
    };

    this.doianh = function (type) {
        if (type == 1) {
            if (miniVuabai.countSangline % 2 == 0) {
                $("#vb_anh_sangline").attr("src", miniVuabai.imageUrl + "noquy.png");
            } else {
                $("#vb_anh_sangline").attr("src", miniVuabai.imageUrl + "noquy1.png");
            }
        }
        if (type == 2) {
            if (miniVuabai.countSangline % 2 == 0) {
                $("#vb_anh_sangline").attr("src", miniVuabai.imageUrl + "winner.png");
            } else {
                $("#vb_anh_sangline").attr("src", miniVuabai.imageUrl + "winner1.png");
            }
        }
        if (type == 3) {
            if (miniVuabai.countSangline % 2 == 0) {
                $("#vb_anh_sangline").attr("src", miniVuabai.imageUrl + "win.png");
            } else {
                $("#vb_anh_sangline").attr("src", miniVuabai.imageUrl + "win1.png");
            }
        }
        miniVuabai.countSangline++;
    };

    this.defaultButton = function (time) {
        setTimeout(function () {
            clearInterval(miniVuabai.timeIntervalSangline);
            $("#vb_divanh").html('');
            for (var i = 1; i < 21; i++) {
                hoverline02(i);
            }
            if (!miniVuabai.isAutoMainGame) {
                $("#vb_button_chondong").attr("onclick", "miniVuabai.chondong();");
                $("#button_tuquay").attr("onclick", "miniVuabai.run(true)");
                $("#button_quay").attr("onclick", "miniVuabai.run(false)");
                $("#vb_tien1").attr("onclick", "miniVuabai.chontien(1);");
                $("#vb_tien2").attr("onclick", "miniVuabai.chontien(2);");
                $("#vb_tien3").attr("onclick", "miniVuabai.chontien(3);");
                $("#vb_tien4").attr("onclick", "miniVuabai.chontien(4);");
                $("#vb_tiensao").attr("onclick", "miniVuabai.chonsao(1);");
                $("#vb_tienxu").attr("onclick", "miniVuabai.chonsao(2);");
            } else {
                miniVuabai.spins();
            }
        }, time);

    };

    this.draw = function () {
        var canvas = document.getElementById('canvas');
        if (canvas.getContext) {
            var ctx = canvas.getContext('2d');

            ctx.fillRect(25, 25, 100, 100);
            ctx.clearRect(45, 45, 60, 60);
            ctx.strokeRect(50, 50, 50, 50);
        }
    };

    function nonHoverline02(stt, htmlanhline) {
        $("div#vb_line" + stt).hover(function () {
            $("#vb_divanh").html(htmlanhline);
        }, function () {
            $("#vb_divanh").html(htmlanhline);
        });
    }

    function hoverline02(stt) {
        $("div#vb_line" + stt).hover(function () {
            $("#vb_divanh").html('<img src="' + miniVuabai.imageUrl + 'so' + stt + '.png"/>');
        }, function () {
            $("#vb_divanh").html('');
        });
    }

    this.chondong = function () {
        $("#vb_datcuoc").css('display', 'block');
        for (var j = 1; j < 21; j++) {
            nonHoverline02(j, '');
        }
    };

    this.confirmChondong = function () {
        $("#vb_datcuoc").css({ "display": "none" });
        $("#vb_button_chondong").html(miniVuabai.numberPacketMainGame + " DÒNG");
        for (var i = 1; i < 21; i++) {
            hoverline02(i);
        }
    };

    //Onclick chon tui tray loc
    this.onClickInPacket = function (stringNumber) {
        var arrayParse = JSON.parse("[" + stringNumber + "]");
        for (var i = 0; i < arrayParse.length; i++) {
            if (miniVuabai.positionMaingame[arrayParse[i] - 1] == 1) {
                miniVuabai.positionMaingame[arrayParse[i] - 1] = 0;
                $("#vb_so" + arrayParse[i]).removeClass("vb_selectline");
                miniVuabai.numberPacketMainGame--;
            } else {
                miniVuabai.positionMaingame[arrayParse[i] - 1] = 1;
                $("#vb_so" + arrayParse[i]).addClass("vb_selectline");
                miniVuabai.numberPacketMainGame++;
            }
        }
    };

    this.chontien = function (roomId) {
        miniVuabai.GameHub.server.PlayNow(roomId, miniVuabai.sao).done(function (result) {
            if (result < 0) {
                miniVuabai.ShowMessage("Lỗi chọn phòng");
                return;
            } else {
                miniVuabai.roomID = roomId;
                for (var i = 1; i < 5; i++) {
                    $("#vb_tien" + i).removeClass("vb_valueRoom_active");
                }
                $("#vb_tien" + roomId).addClass("vb_valueRoom_active");
            }
        }).fail(function () {
            miniVuabai.ShowMessage("Lỗi chọn phòng");
            return;
        });
    };

    this.popHuongdan = function () {
        var width = 780;
        var height = 489;
        miniVuabai.closePopupParent();
        var htmlContent = '<div  class="vb_huongdan vb_bang_vinhdanh_da">';
        htmlContent += '<a class="vb_btn_close" href="javascript:;" onclick="miniVuabai.closePopupParent();"></a>';
        htmlContent += '<div id="vb_popup_guide" class="vb_scroll">';
        htmlContent += '<p>1. Bảng trả thưởng</p>';
        htmlContent += '<table width="90%">';
        htmlContent += '<thead>' +
                            '<tr>' +
                                '<td>Hình ảnh</td>' +
                                '<td>Tỷ lệ trả thưởng</td>' +
                            '</tr>' +
                        '</thead>';
        htmlContent += '<tbody>' +
                '<tr>' +
                    '<td>' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat5.png">' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat5.png">' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat5.png">' +
                    '</td>' +
                    '<td>Nổ quỹ</td>' +
                '</tr>' +
                '<tr>' +
                    '<td>' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat1.png">' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat1.png">' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat1.png">' +
                    '</td>' +
                    '<td>x85</td>' +
                '</tr>' +
                '<tr>' +
                    '<td>' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat6.png">' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat6.png">' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat6.png">' +
                    '</td>' +
                    '<td>x40</td>' +

                '<tr>' +
                    '<td>' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat3.png">' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat3.png">' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat3.png">' +
                    '</td>' +
                    '<td>x20</td>' +
                '</tr>' +
                '<tr>' +
                    '<td>' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat2.png">' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat2.png">' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat2.png">' +
                    '</td>' +
                    '<td>x8</td>' +
                '</tr>' +
                '<tr>' +
                    '<td>' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat2.png">' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat2.png">' +
                    '</td>' +
                    '<td>x0.7</td>' +
                '</tr>' +
                '<tr>' +
                    '<td>' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat4.png">' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat4.png">' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat4.png">' +
                    '</td>' +
                    '<td>x3</td>' +
                '</tr>' +
                '<tr>' +
                    '<td>' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat4.png">' +
                        '<img src="' + miniVuabai.imageUrl + 'vb_chat4.png">' +
                    '</td>' +
                    '<td>x0.5</td>' +
                '</tr>' +
            '</tbody>';
        htmlContent += '</table>';
        htmlContent += '<p>' +
            '* Joker có khả năng thay thế bất cứ biểu tượng nào khác.<br>' +

            'Ví dụ: (vị trị xuất hiện của các biểu tượng có thể thay đổi): <br>' +
            '<img src="' + miniVuabai.imageUrl + 'vb_chat5.png">' +
            '<img src="' + miniVuabai.imageUrl + 'vb_chat1.png">' +
            '<img src="' + miniVuabai.imageUrl + 'vb_chat1.png">' +
            '=' +
            '<img src="' + miniVuabai.imageUrl + 'vb_chat1.png">' +
            '<img src="' + miniVuabai.imageUrl + 'vb_chat5.png">' +
            '<img src="' + miniVuabai.imageUrl + 'vb_chat1.png">' +
            '=' +
            '<img src="' + miniVuabai.imageUrl + 'vb_chat1.png">' +
            '<img src="' + miniVuabai.imageUrl + 'vb_chat1.png">' +
            '<img src="' + miniVuabai.imageUrl + 'vb_chat1.png">' +
            '= x85' +

            '<br>' +

            '* Lưu ý: trường hợp xuất hiện tổ hợp biểu tượng sau (vị trí xuất hiện có thể thay đổi)<br>' +
            '<img src="' + miniVuabai.imageUrl + 'vb_chat4.png">' +
            '<img src="' + miniVuabai.imageUrl + 'vb_chat5.png">' +
            '<img src="' + miniVuabai.imageUrl + 'vb_chat2.png">' +
            '=' +
            '<img src="' + miniVuabai.imageUrl + 'vb_chat4.png">' +
            '<img src="' + miniVuabai.imageUrl + 'vb_chat4.png">' +
            '+' +
            '<img src="' + miniVuabai.imageUrl + 'vb_chat2.png">' +
            '<img src="' + miniVuabai.imageUrl + 'vb_chat2.png">' +
            '= x0.5 + x0.7 = x1.2<br><br>' +


            '<span>2. Các bước chơi và thể lệ trả thưởng</span><br>' +

            'Vua bài là là thể loại game quay ngẫu nhiên các biểu tượng để giành được giải thưởng. Người chơi cần phải tuân <br> theo các bước sau để chơi:<br>' +

        '<span><b>Bước 1</b></span>: Người chơi chọn phòng để chơi: Sao hoặc Xu<br>' +
        '<span><b>Bước 2</b></span>: Người chơi lựa chọn mức cược 100; 1.000; 5.000; 10.000 Sao đối với phòng SAO hoặc 1.000; 10.000; 50.000; 100.000 Xu đối với phòng XU.<br>' +
        '<span><b>Bước 3</b></span>: Người chơi chọn số lượng dòng cược. Dòng cược là các tổ hợp các biểu tượng nằm theo vị trí đã được<br> thiết lập trước. Có tất cả 20 dòng cược. Người chơi có thể chọn từng dòng, tất cả các dòng mang số lẻ, tất cả các dòng mang số chắn, toàn bộ 20 dòng cược hoặc bỏ toàn bộ. <br>' +
        'Tổng tiền cược sẽ được tính như sau: Tổng tiền cược = Mức cược * số dòng<br>' +
        '<span><b>Bước 4</b></span>: Chọn nút Quay để thực hiện việc quay ngẫu nhiên. Khách hàng có thể chọn hình thức Tự quay để quay <br>liên tục<br><br>' +



        '* Hệ thống sẽ trả thưởng lý thuyết: lên đến 98% số Sao đặt<br>' +
        '* Cách chơi Vua bài độc lập hoàn toàn với kết quả chơi game hiện tại!<br>' +
        '* Tại mỗi thời điểm, mỗi mức cược sẽ chỉ có duy nhất một Quỹ Sao.<br>' +

        'Giá trị Quỹ Sao liên tục tăng cao, đưa ra mức giải thưởng vô cùng hấp dẫn!<br>' +
    '</p>';
        htmlContent += '</div>';
        htmlContent += '</div>';

        var html =
                        '<div id="mini_popup_Container" style="width:' + width + 'px;">' +
                            '<div  id="mini_popup">' +
                            '<div>' + htmlContent +
                            '</div></div></div>';

        $('BODY').append(html);
        miniVuabai.setPopup(width, height);
        $('#vb_popup_guide').mCustomScrollbar({ autoHideScrollbar: true, advanced: { updateOnContentResize: true } });
    };

    this.chonsao = function (type) {
        if (miniVuabai.sao != type) {
            miniVuabai.GameHub.server.PlayNow(1, type).done(function (result) {
                if (result < 0) {
                    miniVuabai.ShowMessage("Lỗi chọn phòng");
                    return;
                } else {
                    miniVuabai.roomID = 1;
                    miniVuabai.sao = type;
                    if (type == 1) $('#miniVuabai').removeClass('vb_main_da');
                    else $('#miniVuabai').addClass('vb_main_da');
                    for (var i = 2; i < 5; i++) {
                        $("#vb_tien" + i).removeClass("vb_valueRoom_active");
                    }
                    $("#vb_tien" + 1).addClass("vb_valueRoom_active");
                }
            }).fail(function () {
                miniVuabai.ShowMessage("Lỗi chọn phòng");
                return;
            });
        }
    };

    //Onclick button tui chan 
    this.onClickInPacketChan = function () {
        for (var i = 0; i < miniVuabai.positionMaingame.length ; i++) {
            if (i % 2 != 0) {
                miniVuabai.positionMaingame[i] = 1;
                $("#vb_so" + (i + 1)).addClass('vb_selectline');
            } else {
                miniVuabai.positionMaingame[i] = 0;
                $("#vb_so" + (i + 1)).removeClass('vb_selectline');
            }
        }
        miniVuabai.numberPacketMainGame = 10;
    };

    //Onclick button tui le
    this.onClickInPacketLe = function () {
        for (var i = 0; i < miniVuabai.positionMaingame.length ; i++) {
            if (i % 2 == 0) {
                miniVuabai.positionMaingame[i] = 1;
                $("#vb_so" + (i + 1)).addClass('vb_selectline');
            } else {
                miniVuabai.positionMaingame[i] = 0;
                $("#vb_so" + (i + 1)).removeClass('vb_selectline');
            }
        }
        miniVuabai.numberPacketMainGame = 10;
    };

    //Onclick button All
    this.onClickInPacketAll = function () {
        for (var i = 0; i < miniVuabai.positionMaingame.length ; i++) {
            miniVuabai.positionMaingame[i] = 1;
            $("#vb_so" + (i + 1)).addClass('vb_selectline');
        }
        miniVuabai.numberPacketMainGame = 20;
    };

    //Bo lua chon tui loc da chon
    this.onClickRemoveSelectionpacket = function () {
        for (var i = 0; i < miniVuabai.positionMaingame.length; i++) {
            if (miniVuabai.positionMaingame[i] == 1) {
                $("#vb_so" + (i + 1)).removeClass('vb_selectline');
                miniVuabai.positionMaingame[i] = 0;
            }
        }
        miniVuabai.numberPacketMainGame = 0;
    };

    this.showHide = function () {
        $('#miniVuabai').toggle()
    };

    this.cacheObjHistoryAccount = null;

    this.vinhdanh = function () {
        var width = 570;
        var height = 489;
        miniVuabai.closePopupParent();
        var htmlContent = '<div id="vb_vinhdanh_all" class="vb_bang_vinhdanh vb_bang_vinhdanh_da">';
        htmlContent += '<span class="vb_tieude_bangthanhtich"><img src="' + miniVuabai.imageUrl + 'vb_ver2_tieude_bangthanhtich.png"></span>';
        htmlContent += '<ul class="vb_luachon">';
        htmlContent += '<li class="vb_bansao"><a href="javascript:miniVuabai.vinhdanhItem(1,1,10);"></a></li>';
        htmlContent += '<li class="vb_banxu"><a href="javascript:miniVuabai.vinhdanhItem(1,2,10);"></a></li>';
        htmlContent += '</ul>';
        htmlContent += '<a class="vb_btn_close" href="javascript:;" onclick="miniVuabai.closePopupParent();"></a>';
        htmlContent += '<div class="vb_bao_lichsu">'
                        + '<table class="vb_tieude_bang1" id="vb_vinhdanh_table">'
                        + '</table>'
        + '</div>';
        htmlContent += '<center>'
			                + '<ul class="vb_phan_trang" id="vb_vinhdanh_phantrang">'
			                + '</ul>'
		                + '</center>';
        htmlContent += '</div>';
        var html =
            '<div id="mini_popup_Container" style="width:' + width + 'px;">' +
                '<div  id="mini_popup">' +
                '<div>' + htmlContent +
                '</div></div></div>';

        $('BODY').append(html);
        miniVuabai.setPopup(width, height);
        miniVuabai.vinhdanhItem(1, 1, 10);
    };

    this.history = function () {
        var width = 780;
        var height = 489;
        miniVuabai.closePopupParent();
        var htmlContent = '';
        htmlContent += '<div id="vb_lichsu_all" class="vb_bang_vinhdanh">';
        htmlContent += '<span class="vb_tieude_bangthanhtich vb_tieude_banglichsu"><img src="' + miniVuabai.imageUrl + 'vb_ver2_tieude_lichsu.png"></span>';
        htmlContent += '<ul class="vb_luachon">'
                        + '<li class="vb_bansao"><a href="javascript:miniVuabai.hisAccountItem(1,1,10);"></a></li>'
                        + '<li class="vb_banxu"><a href="javascript:miniVuabai.hisAccountItem(1,2,10);"></a></li>'
                     + '</ul>';
        htmlContent += '<a class="vb_btn_close" href="javascript:;" onclick="miniVuabai.closePopupParent();"></a>';
        htmlContent += '<div class="vb_bao_lichsu">';
        htmlContent += '<table class="vb_tieude_bang1" id="vb_lichsu_table">'
                    + '</table>';
        htmlContent += '</div>';
        htmlContent += '<center>'
                        + '<ul class="vb_phan_trang" id="vb_lichsu_pt">'
                        + '</ul>'
                    + '</center>';
        htmlContent += '</div>';

        var html =
            '<div id="mini_popup_Container" style="width:' + width + 'px;">' +
                '<div  id="mini_popup">' +
                '<div>' + htmlContent +
                '</div></div></div>';

        $('BODY').append(html);
        miniVuabai.setPopup(width, height);
        miniVuabai.hisAccountItem(1, 1, 10);

    };

    this.historyType = 1;
    this.vinhdanhType = 1;

    this.vinhdanhItem = function (curr, type, recordPerpage) {
        var typeCount = '';
        if (type == 1) {
            miniVuabai.vinhdanhType = 1;
            $('#vb_vinhdanh_all').removeClass('vb_bang_vinhdanh_da');
            typeCount = '<td>Sao thắng</td>';
        } else {
            miniVuabai.vinhdanhType = 2;
            $('#vb_vinhdanh_all').addClass('vb_bang_vinhdanh_da');
            typeCount = '<td>Xu thắng</td>';
        }

        $.ajax({
            type: "GET",
            url: miniVuabai.urlApi + "GetNotification/?betType=" + type +
                "&currPage=" + curr + "&recordPerPage=" + recordPerpage,
            crossDomain: true,
            cache: false,
            xhrFields: {
                withCredentials: true
            },
            success: function (data) {
                var html = '';
                html += '<thead>'
                    + '<tr>'
                    + '<td>Phiên</td>'
                    + '<td>Thời gian</td>'
                    + '<td>Mức cược</td>'
                    + '<td>Tài khoản</td>'
                    + typeCount
                    + '<td>Loại thắng</td>'
                    + '</tr>'
                    + '</thead>'
                    + '<tbody>';
                var row = '';
                if (data.HistoryInfo.length > 0) {
                    $.each(data.HistoryInfo, function (i, item) {
                        row += '<tr>';
                        row += '<td>' + item.SpinID + '</td>';
                        row += '<td>' + miniVuabai.formDateTimehmsny(item.CreatedTime) + '</td>';
                        row += '<td>' + miniVuabai.FormatNumber(item.TotalBetValue) + '</td>';
                        row += '<td>' + item.Username + '</td>';
                        row += '<td>' + miniVuabai.FormatNumber(item.TotalPrizeValue.toString()) + '</td>';
                        row += '<td>Nổ quỹ</td>';
                        row += '</tr>';
                    });
                }
                html += row;
                html += '</tbody>';
                $('#vb_vinhdanh_table').html(html);

                var stringPt = '';
                var pageCount = 0;
                var startPage = 0;
                var totalPage = data.TotalCount;
                if (totalPage < 10) totalPage = curr;
                if (totalPage == curr) {
                    startPage = curr - 1;
                    if (curr == 1) startPage = 1;
                } else if (totalPage - curr < 3) {
                    if (curr - 3 > 1) startPage = curr - 3;
                    else startPage = 1;
                } else {
                    if (curr - 2 > 1) startPage = curr - 2;
                    else startPage = 1;
                }
                for (var i = startPage; i <= totalPage; i++) {
                    if (pageCount == 5) break;
                    if (i == curr) stringPt += '<li class="vb_active"><a href="javascript:;" onclick="">' + i + '</a></li>';
                    else stringPt += '<li><a href="javascript:;" onclick="miniVuabai.vinhdanhItem(' + i + ', ' + type + ', 10)">' + i + '</a></li>';
                    pageCount++;
                }
                $('#vb_vinhdanh_phantrang').html(stringPt);
            }
        });
    };

    this.hisAccountItem = function (curr, type, recordPerpage) {
        miniVuabai.historyType = 2;
        $('#vb_lichsu_all').addClass('vb_bang_vinhdanh_da');
        var typeCount = '<td>Xu đặt</td>'
                  + '<td>Xu thắng</td>';
        if (type == 1) {
            miniVuabai.historyType = 1;
            $('#vb_lichsu_all').removeClass('vb_bang_vinhdanh_da');
            typeCount = '<td>Sao đặt</td>'
                      + '<td>Sao thắng</td>';
        }

        $.ajax({
            type: "GET",
            url: miniVuabai.urlApi + "GetHistory/?topCount=200&betType=" + type +
                "&currPage=" + curr + "&recordPerPage=" + recordPerpage,
            crossDomain: true,
            cache: false,
            xhrFields: {
                withCredentials: true
            },
            success: function (data) {
                miniVuabai.cacheObjHistoryAccount = data.HistoryInfo;
                var html = '';
                html += '<thead>'
                            + '<tr>'
                                + '<td>Phiên</td>'
                                + '<td>Thời gian</td>'
                                + '<td>Số dòng cược</td>'
                                + '<td>Số line trúng</td>'
                                + typeCount
                                + '<td>Chi tiết</td>'
                            + '</tr>'
                      + '</thead>';
                html += '<tbody>';

                if (data.HistoryInfo.length > 0) {
                    $.each(data.HistoryInfo, function (i, item) {
                        html += '<tr>';
                        html += '<td>' + item.SpinID + '</td>';
                        html += '<td>' + miniVuabai.formDateTimehmsny(item.CreatedTime) + '</td>';
                        html += '<td>' + item.TotalLines + '</td>';
                        var listPrizeData = [];
                        if (item.PrizesData != '') {
                            listPrizeData = item.PrizesData.split(";");
                        }
                        var lineThang = listPrizeData.length;
                        html += '<td>' + lineThang + '</td>';
                        html += '<td>' + miniVuabai.FormatNumber(item.TotalBetValue.toString()) + '</td>';
                        html += '<td>' + miniVuabai.FormatNumber(item.TotalPrizeValue.toString()) + '</td>';
                        html += '<td><a style="color: #43e7ff;" href="javascript:;" onclick="miniVuabai.hisAccountDetails(' + item.SpinID + ', ' + type + ')">Chi tiết</a></td>';
                        html += '</tr>';
                    });
                }
                html += '</tbody>';


                var stringPt = '';
                var pageCount = 0;
                var startPage = 0;
                var totalPage = data.TotalCount;
                if (totalPage < 10) totalPage = curr;
                if (totalPage == curr) {
                    startPage = curr - 1;
                    if (curr == 1) startPage = 1;
                } else if (totalPage - curr < 3) {
                    if (curr - 3 > 1) startPage = curr - 3;
                    else startPage = 1;
                } else {
                    if (curr - 2 > 1) startPage = curr - 2;
                    else startPage = 1;
                }
                for (var i = startPage; i <= totalPage; i++) {
                    if (pageCount == 5) break;
                    if (i == curr) stringPt += '<li class="vb_active"><a href="javascript:;" onclick="">' + i + '</a></li>';
                    else stringPt += '<li><a href="javascript:;" onclick="miniVuabai.hisAccountItem(' + i + ', ' + type + ', 10)">' + i + '</a></li>';
                    pageCount++;
                }
                $('#vb_lichsu_pt').html(stringPt);
            }
        });
    };

    this.hisAccountDetails = function (spinID, type) {
        var width = 570;
        var height = 489;
        var typeString = 'Xu';
        if (type == 1) typeString = 'Sao';
        miniVuabai.closePopupParent();
        var htmlContent = '';
        htmlContent += '<div id="vb_chitiet_lichsu" class="vb_bang_vinhdanh">';
        htmlContent += '<span class="vb_tieude_bangthanhtich vb_tieude_banglichsu"><img src="' + miniVuabai.imageUrl + 'vb_ver2_tieude_lichsu.png"></span>';
        htmlContent += '<div class="back" onclick="javascript:miniVuabai.history();" ></div>';
        htmlContent += '<a class="vb_btn_close" href="javascript:;" onclick="miniVuabai.closePopupParent();"></a>';
        htmlContent += '<div class="vb_bao_lichsu">';

        htmlContent += '<table class="vb_tieude_bang1">'
                    + ' <thead>'
                    + '<tr>'
                    + '<td width="25%">' + typeString + ' đặt</td>'
                    + '<td width="25%">Dòng cược</td>'
                    + '<td width="25%">Tỉ lệ nhân</td>'
                    + '<td width="25%">' + typeString + ' thắng</td>'
                    + '</tr>'
                    + '</thead>'
                    + '</table>';

        htmlContent += '<div id="vb_bao_lichsu_table" class="ls_chitiet" style="height: 322px;">';
        htmlContent += '<table style="margin-top: 0;" class="vb_tieude_bang1" id="vb_chitiet_history">'
                     + '</table>';
        htmlContent += '</div>';
        htmlContent += '</div>';
        htmlContent += '</div>';

        var html =
            '<div id="mini_popup_Container" style="width:' + width + 'px;">' +
                '<div  id="mini_popup">' +
                '<div>' + htmlContent +
                '</div></div></div>';

        $('BODY').append(html);
        miniVuabai.setPopup(width, height);
        miniVuabai.hisAccountDetailContent(spinID, type);
    };

    this.hisAccountDetailContent = function (spinID, type) {
        var obj = miniVuabai.getObjects(miniVuabai.cacheObjHistoryAccount, "spinID", spinID);
        var html = '';

        var listPrizeData = [];
        var listLine = [];
        var listTile = [];
        var listMonney = [];
        var saodat = obj[0].BetValue;
        if (obj[0].PrizesData != '') {
            listPrizeData = obj[0].PrizesData.split(";");
        }
        var listLineData = [];
        listLineData = obj[0].LinesData.split(",");
        if (listLineData.length > 0) {
            for (var j = 0; j < listLineData.length; j++) {
                var check = false;
                if (listPrizeData.length > 0) {
                    for (var i = 0; i < listPrizeData.length; i++) {
                        if (listPrizeData[i].split(",")[0] == listLineData[j]) {
                            listLine.push(listPrizeData[i].split(",")[0]);
                            listTile.push(miniVuabai.tinhtile(listPrizeData[i].split(",")[1]));
                            listMonney.push(listPrizeData[i].split(",")[2]);
                            check = true;
                            break;
                        }
                    }
                    if (check == false) {
                        listLine.push(listLineData[j]);
                        listTile.push('Trượt');
                        listMonney.push(0);
                    }
                }
            }
        }


        html += '<tbody>';
        if (listLine.length > 0) {
            $.each(listLine, function (i, item) {
                html += '<tr>';
                html += '<td width="25%">' + miniVuabai.FormatNumber(saodat.toString()) + '</td>';
                html += '<td width="25%">' + listLine[i] + '</td>';
                html += '<td width="25%">' + listTile[i] + '</td>';
                html += '<td width="25%">' + miniVuabai.FormatNumber(listMonney[i].toString()) + '</td>';
                html += '</tr>';
            });
        }
        html += '</tbody>';

        $('#vb_chitiet_history').html(html);
        $('#vb_bao_lichsu_table').mCustomScrollbar({ autoHideScrollbar: true, advanced: { updateOnContentResize: true } });
    };

    this.tinhtile = function (id) {
        return tilenhan[id - 1];
    };

    this.setPopup = function (width, height) {
        $('#mini_popup').css('width', width);
        $('#mini_popup').css('height', height);
        var leftOffset = ($(window).width() - width) / 2;
        var topOffset = ($(window).height() - height) / 2 + $(window).scrollTop();
        $('#mini_popup_Container').css('left', leftOffset + "px");
        $('#mini_popup_Container').css('z-index', 1300);
        $('#mini_popup_Container').css("top", "71px");
        $('#mini_popup_Container').css('position', 'absolute');
        $('#mini_popup_Container').draggable();
    };

    this.closePopupParent = function () {
        $('#mini_popup_Container').remove();
        $('#mini_overlay').remove();
    };

    this.formDateTimehmsny = function (date) {
        date = date.replace(/\-/g, '\/').replace(/[T|Z]/g, ' ');
        if (date.indexOf('.') > 0)
            date = date.substring(0, date.indexOf('.'));
        var d = new Date(date);
        var curr_date = d.getDate();
        var curr_month = d.getMonth();
        curr_month = curr_month + 1;
        var curr_year = d.getFullYear();
        var _hour = d.getHours();
        var _minute = d.getMinutes();
        var _second = d.getSeconds();
        if (curr_date < 10) curr_date = "0" + curr_date;
        if (curr_month < 10) curr_month = "0" + (curr_month);
        if (_hour < 10) _hour = "0" + _hour;
        if (_minute < 10) _minute = "0" + _minute;
        return curr_date + "/" + curr_month + "/" + curr_year //+ " <br> "
             + _hour + ":" + _minute;
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

    this.getObjects = function (obj, key, val) {
        var objects = [];
        for (var i in obj) {

            var spinID = obj[i].SpinID;
            if (spinID == val) {
                objects.push(obj[i]);
            }
        }
        return objects;
    };
};
$(document).ready(function () {
    miniVuabai.Init();
});