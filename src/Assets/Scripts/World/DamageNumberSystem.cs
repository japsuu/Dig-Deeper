﻿using DamageNumbersPro;
using Singletons;
using UnityEngine;

namespace World
{
    public class DamageNumberSystem : SingletonBehaviour<DamageNumberSystem>
    {
        [SerializeField]
        private DamageNumber _damageNumberPrefab;
        
        [SerializeField]
        private DamageNumber _damageNumberPlayerPrefab;
        
        
        public void SpawnDamageNumber(Vector3 position, int damage, bool playerDamage = false)
        {
            DamageNumber prefab = playerDamage ? _damageNumberPlayerPrefab : _damageNumberPrefab;
            prefab.Spawn(position, damage);
        }
    }
}