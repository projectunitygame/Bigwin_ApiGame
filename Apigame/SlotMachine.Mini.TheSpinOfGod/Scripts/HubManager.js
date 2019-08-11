(function (scope) {
    var hubmanager = function () { this.ConnectHub(); };
    var connection = null;
    hubmanager.prototype.ConnectHub = function () {

        this.Connection = $.hubConnection(miniVuabai.signalRUrl);
        miniVuabai.GameHub = this.Connection.createHubProxy('HubVuabai');
        $.connection.hub.logging = true;
        connection = this.Connection;
        // trạng thái kết nối
        this.Connection.stateChanged(function (change) {
            if (change.newState === $.signalR.connectionState.connecting) {
                console.log('vuabai connecting...');
            }
            else if (change.newState === $.signalR.connectionState.reconnecting) {
                console.log('vua bai reconnecting...');
            }
            else if (change.newState === $.signalR.connectionState.connected) {
                // kết nối thành công
                miniVuabai.GameHub.server.PlayNow(miniVuabai.roomID, miniVuabai.sao).done(function (result) {
                    if (result < 0) {
                        miniVuabai.roomID = 1;
                        miniVuabai.ShowMessage("Lỗi vào phòng");
                        return;
                    }
                }).fail(function () {
                    miniVuabai.roomID = 1;
                    miniVuabai.ShowMessage("Lỗi vào phòng");
                    return;
                });
            }
            else if (change.newState === $.signalR.connectionState.disconnected) {
                console.log('vuabai disconnected');
            }
        });
        this.Connection.connectionSlow(function () {
            console.log('Connection Slow.');
        });

        this.Connection.error(function (error) {
            // Lỗi kết nối
        });
        this.Connection.reconnected(function () {
            console.log("Connection reconnected");

        });
        //Gọi gameHub
        scope.GameHub(miniVuabai.GameHub);

        this.StartHubs();
        
    };

    hubmanager.prototype.StartHubs = function () {
        try {
            connection.start().done(function () {
                // Đã kết nối
                console.log("Kết nối thành công");
            });
        } catch (e) {
            console.log(e);
        }
    };
    scope.HubManager = hubmanager;
})(window)