# Aqualab
Aqualab is an NSF Funded (DRL #1907384) science practices and life science content learning game produced by Field Day @ University of Wisconsin - Madison, Harvard University and University of Pennslvania.

## Firebase Telementry Events

Firebase automatically adds the following to all events
* event_timestamp
* user_id (We need to manually set this if we have it)
* device.category
* device.mobile_brand_name (i.e Apple)
* device.mobile_model_name (i.e Safari)
* device.operating_system
* device.language
* geo.country
* geo.region (i.e Wisconsin)
* geo.city (i.e Madison)
* ga_session_id

Firebase automatically adds the following meaningful events
* first_visit
* session_start

Init
* session_Begin(session_id, client_time)
* user_code_entered(usercode)

Progression
* accept_job (job_id)
* switch_job (job_id)
* receive_fact (fact_id)
* complete_job (job_id)
* complete_task (job_id, task_id)

Player Actions
* scene_changed (scene_name)
* room_changed (room_name)
* begin_experiment (job_id, (enum)tank_type)
* MORE HERE
* begin_dive (job_id, site_id)
* MORE HERE
* begin_argument (job_id)
* MORE HERE
* begin_model (job_id)
* MORE HERE
* begin_simulation (job_id)
* ask_for_help (node_id) - User clicked the hint button
* talk_with_guide (node_id) - User clicked the button for Kevin

Portable Device Interactions
* open_bestiry
* bestiary_open_species_tab
* bestiary_open_environments_tab
* bestiary_open_models_tab
* bestiary_select_species (species_id)
* bestiary_select_environment (environment_id)
* bestiary_select_model (model_id)
* close_bestiary
* open_status
* status_open_job_tab
* status_open_item_tab
* status_open_tech_tab
* close_status

Game Feedback
* simulation_sync_achieved (job_id)
* script_triggered (node_id)
