# Aqualab
Aqualab is an NSF Funded (DRL #1907384) science practices and life science content learning game produced by Field Day @ University of Wisconsin - Madison, Harvard University and University of Pennslvania.

## Firebase Telemetry Events

Progression
* Accept_Job (user_id, session_id, client_time, job_id)
* Switch_Job (old_job_id, new_job_id)
* Receive_Fact (user_id, session_id, client_time, fact_id)
* Complete_Job (user_id, session_id, client_time, job_id)
* Complete_Task (task_id)

Player Actions
* Begin_Experiment (user_id, session_id, client_time, job_id, (enum)tank_type)
* End_Experiment (job_id, tank_type, duration)
* Begin_Dive (user_id, session_id, client_time, job_id, site_id)
* Begin_Argument (user_id, session_id, client_time, job_id)
* Begin_Model (user_id, session_id, client_time, job_id)
* Begin_Simulation (user_id, session_id, client_time, job_id)
* Ask_For_Help (user_id, session_id, client_time, node_id)
* Talk_With_Guide (user_id, session_id, client_time, node_id)
* Change_Room (job_id, room_id)
* Change_Station (job_id, station_id)
* Argue_Valid_Claim (job_id, node_id)
* Argue_Invalid_Claim (job_id, node_id)

Game Feedback
* Simulation_Sync_Achieved (user_id, session_id, client_time, job_id)
* Guide_Script_Triggered (user_id, session_id, client_time, node_id)