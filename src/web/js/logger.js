class loggerApi {
    static LoadSources(token, success) {
        $.ajax({
            contentType: 'application/json',
            type: "GET",
            url: "/api/sources",
            beforeSend: function (xhr) { xhr.setRequestHeader('X-Token', token); },
            success: function (data, textStatus, jqXHR) {
                let fields = [];
                fields.push('any');
                data.forEach(function (field) {
                    fields.push(field);
                });
                success(fields);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(jqXHR.statusText);
            }
        });
    };

    static LoadLogLevels(token, success) {
        $.ajax({
            contentType: 'application/json',
            type: "GET",
            url: "/api/loglevels",
            beforeSend: function (xhr) { xhr.setRequestHeader('X-Token', token); },
            success: function (data, textStatus, jqXHR) {
                let fields = [];
                fields.push('any');
                data.forEach(function (field) {
                    fields.push(field);
                });
                success(fields);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(jqXHR.statusText);
            }
        });
    };

    static GetLastRecords(token, seconds, success) {
        $.ajax({
            contentType: 'application/json',
            type: "GET",
            url: "/api/last?seconds=" + seconds,
            beforeSend: function (xhr) { xhr.setRequestHeader('X-Token', token); },
            success: function (data, textStatus, jqXHR) {
                let records = [];
                data.forEach(function (record) {
                    records.push(record);
                });
                success(records);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(jqXHR.statusText);
            }
        });
    };

    static Search(token, tag, text, logLevel, source, success) {
        const info = { "id": null, "tag": tag, "source": source, "text": text, "logLevel": logLevel, "start": null, "end": null };
        $.ajax({
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            type: "POST",
            url: "/api/search",
            data: JSON.stringify(info),
            beforeSend: function (xhr) { xhr.setRequestHeader('X-Token', token); },
            success: function (data, textStatus, jqXHR) {
                let records = [];
                data.forEach(function (record) {
                    records.push(record);
                });
                success(records);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(jqXHR.statusText);
            }
        });
    };

    static Auth(login, password, success) {
        const info = { "username": login, "password": password };
        $.ajax({
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            type: "POST",
            url: "/api/auth",
            data: JSON.stringify(info),
            success: function (token, textStatus, jqXHR) {
                success(token);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(jqXHR.statusText);
            }
        });
    };
}