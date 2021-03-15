using System;
using Aqua;
using Aqua.Portable;
using BeauRoutine.Extensions;
using BeauUtil;
using UnityEngine;
using UnityEngine.UI;

namespace ProtoAqua.Modeling
{
    public class ChartUI : MonoBehaviour
    {
        #region Inspector

        [SerializeField, Required] private GraphAxis m_Axis = null;
        [SerializeField, Required] private GraphDisplay m_Historical = null;
        [SerializeField, Required] private GraphDisplay m_Player = null;
        [SerializeField, Required] private GraphDisplay m_Predict = null;
        [SerializeField, Required] private GraphPlot m_Targets = null;
        
        #endregion // Inspector

        [NonSerialized] private bool m_PredictMode;

        private void Awake()
        {
            m_Predict.gameObject.SetActive(false);
            m_Targets.gameObject.SetActive(false);
        }

        public void ShowPrediction()
        {
            m_Predict.gameObject.SetActive(true);
            m_Targets.gameObject.SetActive(true);
            m_PredictMode = true;
        }

        public void Refresh(SimulationBuffer inBuffer, SimulationBuffer.UpdateFlags inUpdate)
        {
            if (inUpdate == 0)
                return;

            if ((inUpdate & SimulationBuffer.UpdateFlags.Historical) != 0)
            {
                m_Historical.LoadCritters(inBuffer.HistoricalData(), inBuffer.Scenario());
                m_Targets.LoadTargets(inBuffer.Scenario());
            }

            if ((inUpdate & SimulationBuffer.UpdateFlags.Model) != 0)
            {
                m_Player.LoadCritters(inBuffer.PlayerData(), inBuffer.Scenario());
                m_Predict.LoadCritters(inBuffer.PredictData(), inBuffer.Scenario());
            }

            uint totalTicks = inBuffer.Scenario().TotalTicks();

            GraphingUtils.AxisRangePair axisPair;
            if (m_PredictMode)
            {
                axisPair = CalculateGraphRect(m_Historical.Range, m_Player.Range, m_Predict.Range, m_Targets.Range, totalTicks, 8);
            }
            else
            {
                axisPair = CalculateGraphRect(m_Historical.Range, m_Player.Range, default(Rect), default(Rect), totalTicks, 8);
            }

            Rect fullRect = axisPair.ToRect();
            m_Historical.RenderLines(fullRect);
            m_Player.RenderLines(fullRect);
            m_Predict.RenderLines(fullRect);
            m_Targets.RenderPoints(fullRect);
            m_Axis.Load(axisPair);
        }

        static private GraphingUtils.AxisRangePair CalculateGraphRect(in Rect inA, in Rect inB, in Rect inC, in Rect inD, uint inTickCountX, uint inTickCountY)
        {
            Rect rect = inA;
            Geom.Encapsulate(ref rect, inB);
            Geom.Encapsulate(ref rect, inC);
            Geom.Encapsulate(ref rect, inD);

            GraphingUtils.AxisRangePair pair;
            pair.X = new GraphingUtils.AxisRangeInfo() { Min = 0, Max = inTickCountX, TickCount = inTickCountX + 1, TickInterval = 1 };
            pair.Y = GraphingUtils.CalculateAxis(rect.yMin, rect.yMax, inTickCountY);
            pair.Y.SetMinAtOrigin();
            return pair;
        }
    }
}