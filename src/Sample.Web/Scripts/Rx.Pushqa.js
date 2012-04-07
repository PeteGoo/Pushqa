﻿/// <reference path="rx.min.js" />
/// <reference path="jquery-1.6.2.js" />
(function() {
    /// <param name="$" type="jQuery" />
    "use strict";

    if (typeof($.signalR) !== "function") {
        throw "SignalR: SignalR is not loaded. Please ensure jquery.signalR.js is referenced before Rx.Pushqa.";
    }
    
    if (typeof(Rx) !== "object") {
        throw "Rx: RxJs is not loaded. Please ensure rx.min.js is referenced before Rx.Pushqa.";
    }

    $.signalR.prototype.subject = new Rx.Subject();

    $.signalR.prototype.asObservable = function () {
        this.received(function (data) {
            if (data.Type == 'Completed') {
                this.subject.onCompleted();
                this.stop();
            }
            else if(data.Type == 'Error') {
                this.subject.onError(data.ErrorMessage);
            } else {
                this.subject.onNext(data.Message);
            }
        });
        return this.subject.asObservable();
    };
})();