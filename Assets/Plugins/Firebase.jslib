mergeInto(LibraryManager.library, {

    FBStartGameWithUserCode: function(userCode) {
        var userCode = Pointer_stringify(userCode);

        analytics.logEvent("user_code_entered", {
            usercode: userCode
        });
    },

    FBAcceptJob: function(jobId) {
        var jobId = Pointer_stringify(jobId);

        analytics.logEvent("accept_job", {
            job_id: jobId
        });
    },

    FBSwitchJob: function(oldJobId, newJobId) {
        var oldJobId = Pointer_stringify(oldJobId);
        var newJobId = Pointer_stringify(newJobId);

        analytics.logEvent("switch_job", {
            old_job_id: oldJobId,
            new_job_id: newJobId
        });
    },

    FBReceiveFact: function(factId) {
        var factId = Pointer_stringify(factId);

        analytics.logEvent("receive_fact", {
            fact_id: factId
        });
    },

    FBCompleteJob: function(jobId) {
        var jobId = Pointer_stringify(jobId);

        analytics.logEvent("complete_job", {
            job_id: jobId
        });
    },

    FBTaskCompleted: function(jobId, taskId) {
        var jobId = Pointer_stringify(jobId);
        var taskId = Pointer_stringify(taskId);

        analytics.logEvent("task_completed", {
            job_id: jobId,
            task_id: taskId
        });
    },

    FBBeginExperiment: function(jobId, tankType) {
        var jobId = Pointer_stringify(jobId);

        analytics.logEvent("begin_experiment", {
            job_id: jobId,
            tank_type: tankType
        });
    },

    FBEndExperiment: function(jobId, tankType, duration) {
        var jobId = Pointer_stringify(jobId);
        var tankType = Pointer_stringify(tankType);

        analytics.logEvent("end_experiment", {
            job_id: jobId,
            tank_type: tankType,
            duration: duration
        });
    },

    FBBeginDive: function(jobId, siteId) {
        var jobId = Pointer_stringify(jobId);
        var siteId = Pointer_stringify(siteId);

        analytics.logEvent("begin_dive", {
            job_id: jobId,
            site_id: siteId
        });
    },

    FBBeginArgument: function(jobId) {
        var jobId = Pointer_stringify(jobId);

        analytics.logEvent("begin_argument", {
            job_id: jobId
        });
    },

    FBBeginModel: function(jobId) {
        var jobId = Pointer_stringify(jobId);

        analytics.logEvent("begin_model", {
            job_id: jobId
        });
    },

    FBBeginSimulation: function(jobId) {
        var jobId = Pointer_stringify(jobId);

        analytics.logEvent("begin_simulation", {
            job_id: jobId
        });
    },

    FBAskForHelp: function(nodeId) {
        var nodeId = Pointer_stringify(nodeId);

        analytics.logEvent("ask_for_help", {
            node_id: nodeId
        });
    },

    FBTalkWithGuide: function(nodeId) {
        var nodeId = Pointer_stringify(nodeId);

        analytics.logEvent("talk_with_guide", {
            node_id: nodeId
        });
    },

    FBSimulationSyncAchieved: function(jobId) {
        var jobId = Pointer_stringify(jobId);

        analytics.logEvent("simulation_sync_achieved", {
            job_id: jobId
        });
    },

    FBGuideScriptTriggered: function(nodeId) {
        var nodeId = Pointer_stringify(nodeId);

        analytics.logEvent("guide_script_triggered", {
            node_id: nodeId
        });
    },

    FBChangeRoom: function(jobId, roomId) {
        var jobId = Pointer_stringify(jobId);
        var roomId = Pointer_stringify(roomId);

        analytics.logEvent("change_room", {
            job_id: jobId,
            room_id: roomId
        });
    },

    FBChangeStation: function(jobId, stationId) {
        var jobId = Pointer_stringify(jobId);
        var stationId = Pointer_stringify(station_id);

        analytics.LogEvent("change_station", {
            job_id: jobId,
            station_id: stationId
        });
    }

    FBArgueValidResponse: function(jobId, nodeId) {
        var jobId = Pointer_stringify(jobId);
        var nodeId = Pointer_stringify(nodeId);

        analytics.logEvent("argue_valid_response", {
            job_id: jobId,
            node_id: nodeId
        });
    },

    FBArgueInvalidResponse: function(jobId, nodeId) {
        var jobId = Pointer_stringify(jobId)
        var nodeId = Pointer_stringify(nodeId);

        analytics.logEvent("argue_invalid_response", {
            job_id: jobId,
            node_id: nodeId
        });
    }

});
