using System;
using UnityEngine;
using Weapons.Mines;

namespace Weapons.Controllers
{
    public class DrillMineController : MonoBehaviour
    {
        public static event Action<int> MineCountChanged;
        
        [SerializeField]
        private ExplodingMine _minePrefab;
        
        [SerializeField]
        private Transform _mineLayPosition;

        [SerializeField]
        [Tooltip("How often mines can be laid.")]
        private float _mineLayInterval = 3f;

        [SerializeField]
        [Tooltip("How many mines can be carried at once.")]
        private int _maxMines = 8;
        
        private int _currentMineCount;
        private float _timeSinceLastMineLay;


        private void Start()
        {
            _currentMineCount = _maxMines;
            MineCountChanged?.Invoke(_currentMineCount);
        }


        private void Update()
        {
            _timeSinceLastMineLay += Time.deltaTime;
        }


        public void RefillMines()
        {
            _currentMineCount = _maxMines;
            MineCountChanged?.Invoke(_currentMineCount);
        }
        
        
        public void TryLayMine()
        {
            if (_currentMineCount <= 0)
                return;
            
            if (_timeSinceLastMineLay < _mineLayInterval)
                return;
            
            Instantiate(_minePrefab, _mineLayPosition.position, Quaternion.identity);
            
            _currentMineCount--;
            _timeSinceLastMineLay = 0f;
            MineCountChanged?.Invoke(_currentMineCount);
        }
    }
}