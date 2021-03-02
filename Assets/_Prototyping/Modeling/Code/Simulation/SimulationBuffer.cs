using System;
using System.Collections.Generic;
using Aqua;
using BeauUtil;

namespace ProtoAqua.Modeling
{
    /// <summary>
    /// Buffer for all simulation data.
    /// </summary>
    public class SimulationBuffer
    {
        private readonly SimulationProfile m_HistoricalProfile = new SimulationProfile();
        private readonly SimulationProfile m_PlayerProfile = new SimulationProfile();

        private ModelingScenarioData m_Scenario;
        private SimulationResult[] m_HistoricalResultBuffer;
        private SimulationResult[] m_PlayerResultBuffer;

        private bool m_HistoricalSimDirty;
        private bool m_PlayerSimDirty;

        private readonly HashSet<PlayerFactParams> m_PlayerFacts = new HashSet<PlayerFactParams>();
        private readonly RingBuffer<ActorCount> m_PlayerActors = new RingBuffer<ActorCount>(Simulator.MaxTrackedCritters);

        public SimulatorFlags Flags;

        #region Scenario

        /// <summary>
        /// Returns the current scenario.
        /// </summary>
        public ModelingScenarioData Scenario() { return m_Scenario; }

        /// <summary>
        /// Sets the current scenario.
        /// </summary>
        public bool SetScenario(ModelingScenarioData inScenarioData)
        {
            if (m_Scenario == inScenarioData)
                return false;

            m_Scenario = inScenarioData;
            Array.Resize(ref m_HistoricalResultBuffer, (int) m_Scenario.TickCount() + 1);
            Array.Resize(ref m_PlayerResultBuffer, 1 + (int) m_Scenario.TickCount() + m_Scenario.PredictionTicks());

            m_HistoricalProfile.Clear();
            m_PlayerProfile.Clear();

            m_PlayerFacts.Clear();
            m_PlayerActors.Clear();

            foreach(var actor in inScenarioData.Actors())
            {
                m_PlayerActors.PushBack(new ActorCount(actor.Id, actor.Population));
            }

            m_PlayerActors.SortByKey<StringHash32, uint, ActorCount>();

            m_PlayerSimDirty = true;
            m_HistoricalSimDirty = true;
            return true;
        }

        #endregion // Scenario

        #region Player

        /// <summary>
        /// Returns the player critter count.
        /// </summary>
        public uint GetPlayerCritters(StringHash32 inId)
        {
            uint pop;
            m_PlayerActors.TryBinarySearch(inId, out pop);
            return pop;
        }

        /// <summary>
        /// Sets the player critter count.
        /// </summary>
        public bool SetPlayerCritters(StringHash32 inId, uint inPopulation)
        {
            int idx = m_PlayerActors.BinarySearch<StringHash32, uint, ActorCount>(inId);
            if (idx < 0)
            {
                m_PlayerActors.PushBack(new ActorCount(inId, inPopulation));
                m_PlayerActors.SortByKey<StringHash32, uint, ActorCount>();
                m_PlayerSimDirty = true;
                return true;
            }

            ref ActorCount existing = ref m_PlayerActors[idx];
            if (existing.Population != inPopulation)
            {
                existing.Population = inPopulation;
                m_PlayerSimDirty = true;
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Adds a fact to the player sim.
        /// </summary>
        public bool AddFact(PlayerFactParams inFact)
        {
            if (m_PlayerFacts.Add(inFact))
            {
                m_PlayerSimDirty = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a fact from the player sim.
        /// </summary>
        public bool RemoveFact(PlayerFactParams inFact)
        {
            if (m_PlayerFacts.Remove(inFact))
            {
                m_PlayerSimDirty = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns if the player sim contains the given fact.
        /// </summary>
        public bool ContainsFact(PlayerFactParams inFact)
        {
            if (Services.Assets.Bestiary.IsAutoFact(inFact.FactId))
                return true;

            return m_PlayerFacts.Contains(inFact);
        }

        /// <summary>
        /// Returns all facts added to the player sim.
        /// </summary>
        public IEnumerable<PlayerFactParams> PlayerFacts()
        {
            return m_PlayerFacts;
        }

        #endregion // Player

        #region Results

        /// <summary>
        /// Returns historical data results.
        /// </summary>
        public SimulationResult[] HistoricalData()
        {
            RefreshHistorical();
            return m_HistoricalResultBuffer;
        }

        /// <summary>
        /// Returns player model results.
        /// </summary>
        public SimulationResult[] PlayerData()
        {
            RefreshModel();
            return m_PlayerResultBuffer;
        }

        /// <summary>
        /// Refreshes historical data.
        /// </summary>
        public bool RefreshHistorical()
        {
            if (!m_HistoricalSimDirty)
                return false;

            m_HistoricalProfile.Clear();
            m_HistoricalProfile.Construct(m_Scenario.Environment(), m_Scenario.Critters(), m_Scenario.Facts());
            foreach(var critter in m_Scenario.Actors())
                m_HistoricalProfile.InitialState.SetCritters(critter.Id, critter.Population);

            Simulator.GenerateToBuffer(m_HistoricalProfile, m_HistoricalResultBuffer, m_Scenario.TickScale(), Flags);
            m_HistoricalSimDirty = false;
            return true;
        }
    
        /// <summary>
        /// Refreshes player data.
        /// </summary>
        public bool RefreshModel()
        {
            if (!m_PlayerSimDirty)
                return false;

            m_PlayerProfile.Construct(m_Scenario.Environment(), m_Scenario.Critters(), m_PlayerFacts);
            foreach(var critter in m_PlayerActors)
                m_PlayerProfile.InitialState.SetCritters(critter.Id, critter.Population);

            Simulator.GenerateToBuffer(m_PlayerProfile, m_PlayerResultBuffer, m_Scenario.TickScale(), Flags);
            m_PlayerSimDirty = false;
            return true;
        }
    
        #endregion // Results
    }
}