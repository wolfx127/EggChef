using System;
using Overall;
using UnityEngine;

namespace StartToPlay
{
    public class InitialDataMgr : MonoBehaviour
    {
        [SerializeField] private OverallDataSO overallDataSO;

        private void Awake()
        {
            OverallData.CopyFieldsBySO(overallDataSO);
        }
    }
}
