(function (scope, $) {
    var inteval = null;
    var GameHub = function (hub) {
        hub.server = {};
        hub.client = {};
        $.extend(hub.server, {
            PlayNow: function (roomId, betType) {
                return hub.invoke.apply(hub, $.merge(["PlayNow"], $.makeArray(arguments)));
            },
            UserSpin: function (roomId, betType, linesData) {
                return hub.invoke.apply(hub, $.merge(["UserSpin"], $.makeArray(arguments)));
            },
            pingPong: function () {
                return hub.invoke.apply(hub, $.merge(["PingPong"], $.makeArray(arguments)));
            },
            GetJackPot: function (roomId, betType) {
                return hub.invoke.apply(hub, $.merge(["GetJackPot"], $.makeArray(arguments)));
            },
            LeaveRoom: function () {
                return hub.invoke.apply(hub, $.merge(["LeaveRoom"], $.makeArray(arguments)));
            }
        });

        hub.on('joinGame', function (player) {
            console.log(player);
            App.PlayerJoin(player);

            if (inteval) {
                clearInterval(inteval);
            } else {
                inteval = setInterval(function () {
                    App.gameHub.server.pingPong();
                }, 5000);
            }
        });

        hub.on("ResultSpin", function (result) {
            miniVuabai.RenderResult(result);

        });
        hub.on("message", function (result, time) {
        });
        hub.on('UpdateJackPot', function (result) {
            var s = new miniVuabai.CountUp("vb_sohuloc", miniVuabai.oldJackpotValue, result, 0, 1);
            s.start();
            miniVuabai.oldJackpotValue = result;
        });
    };

    scope.GameHub = GameHub;
})(window, $);